/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Permissions;
using log4net;
using FluorineFx.Context;
using FluorineFx.Util;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Api.SO;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.IO;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Scheduling;
using FluorineFx.Threading;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    class RtmpHandler : BaseRtmpHandler
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmpHandler));


        IEndpoint _endpoint;

        public RtmpHandler(IEndpoint endpoint)
            : base()
        {
            _endpoint = endpoint;
        }

        internal IEndpoint Endpoint
        {
            get { return _endpoint; }
        }

        public override void ConnectionOpened(RtmpConnection connection)
        {
            base.ConnectionOpened(connection);
            if (connection.Context.Mode == RtmpMode.Server)
            {
                connection.StartWaitForHandshake();
            }
        }

        public override void MessageSent(RtmpConnection connection, object message)
        {
            base.MessageSent(connection, message);
            RtmpPacket sent = message as RtmpPacket;
            int channelId = sent.Header.ChannelId;
            IClientStream stream = null;
            if( connection is IStreamCapableConnection )
                stream = (connection as IStreamCapableConnection).GetStreamByChannelId(channelId);
            // XXX we'd better use new event model for notification
            if (stream != null && (stream is PlaylistSubscriberStream))
            {
                (stream as PlaylistSubscriberStream).Written(sent.Message);
                
            }
        }

/*
FlexInvoke flexInvoke = new FlexInvoke();
flexInvoke.Cmd = "onstatus";
flexInvoke.DataType = DataType.TypeUnknown;
StatusASO statusASO = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_CLOSED, connection.ObjectEncoding);
flexInvoke.Parameters = new object[]{ statusASO };
RtmpChannel channel = connection.GetChannel(3);
channel.Write(flexInvoke);
*/


        protected override void OnChunkSize(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ChunkSize chunkSize) 
        {
            if (connection is IStreamCapableConnection)
            {
                IStreamCapableConnection streamCapableConnection = connection as IStreamCapableConnection;
                {
                    foreach (IClientStream stream in streamCapableConnection.GetStreams())
                    {
                        if (stream is IClientBroadcastStream)
                        {
                            IClientBroadcastStream bs = stream as IClientBroadcastStream;
                            IBroadcastScope scope = bs.Scope.GetBasicScope(Constants.BroadcastScopeType, bs.PublishedName) as IBroadcastScope;
                            if (scope == null)
                                continue;

                            OOBControlMessage setChunkSize = new OOBControlMessage();
                            setChunkSize.Target = "ClientBroadcastStream";
                            setChunkSize.ServiceName = "chunkSize";
                            setChunkSize.ServiceParameterMap.Add("chunkSize", chunkSize.Size);
                            scope.SendOOBControlMessage((IConsumer)null, setChunkSize);
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Sending chunksize " + chunkSize + " to " + bs.Provider);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPing(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, Ping ping)
        {
            switch (ping.PingType)
            {
                case Ping.ClientBuffer:
                    IClientStream stream = null;
                    // Get the stream id
                    int streamId = ping.Value2;
                    // Get requested buffer size in milliseconds
                    int buffer = ping.Value3;
                    if (streamId != 0)
                    {
                        // The client wants to set the buffer time
                        stream = connection.GetStreamById(streamId);
                        if (stream != null)
                        {
                            stream.SetClientBufferDuration(buffer);
                            if (log.IsDebugEnabled)
                                log.Debug(string.Format("Client sent a buffer size: {0} ms for stream id: {1}", buffer, streamId ));
                        }
                    }
                    // Catch-all to make sure buffer size is set
                    if (stream == null)
                    {
                        // Remember buffer time until stream is created
                        connection.RememberStreamBufferDuration(streamId, buffer);
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("Remembering client buffer size: {0} on stream id: {1} ", buffer, streamId));
                    }
                    break;
                case Ping.PongServer:
                    // This is the response to an IConnection.Ping request
                    connection.PingReceived(ping);
                    break;
                default:
                    log.Warn("Unhandled ping: " + ping);
                    break;
            }
        }

        protected override void OnServerBW(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ServerBW serverBW)
        {
        }

        protected override void OnClientBW(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ClientBW clientBW)
        {
            
        }

        protected override void OnInvoke(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, Notify invoke)
		{
			IServiceCall serviceCall = invoke.ServiceCall;

			// If it's a callback for server remote call then pass it over to callbacks handler
			// and return
			if(serviceCall.ServiceMethodName.Equals("_result") || serviceCall.ServiceMethodName.Equals("_error"))
			{
                HandlePendingCallResult(connection, invoke);
				return;
			}

			bool disconnectOnReturn = false;
			string action = null;
            if (serviceCall.ServiceName == null)
            {
                action = serviceCall.ServiceMethodName;
                switch (action)
                {
                    case ACTION_CONNECT:
                        {
                            if (!connection.IsConnected)
                            {
                                IDictionary parameters = invoke.ConnectionParameters;
                                string host = null;
                                if( parameters.Contains("tcUrl") )
                                    host = GetHostname(parameters["tcUrl"] as string);
                                if (host != null && host.IndexOf(":") != -1)
                                {
                                    // Remove default port from connection string
                                    host = host.Substring(0, host.IndexOf(":"));
                                }
                                string app = parameters["app"] as string;
                                string path = parameters["app"] as string;
                                // App name as path, but without query string if there is one
                                if (path != null && path.IndexOf("?") != -1)
                                {
                                    int idx = path.IndexOf("?");
                                    parameters["queryString"] = path.Substring(idx);
                                    path = path.Substring(0, idx);
                                }
                                parameters["path"] = path;

                                connection.Setup(host, path, parameters);
                                try
                                {
                                    //IGlobalScope global = this.Endpoint.LookupGlobal(host, path);
                                    IGlobalScope global = this.Endpoint.GetMessageBroker().GlobalScope;
                                    if (global == null)
                                    {
                                        serviceCall.Status = Call.STATUS_SERVICE_NOT_FOUND;
                                        if (serviceCall is IPendingServiceCall)
                                        {
                                            StatusASO status = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_INVALID_APPLICATION, connection.ObjectEncoding);
                                            status.description = "No global scope on this server.";
                                            (serviceCall as IPendingServiceCall).Result = status;
                                        }
                                        log.Info(string.Format("No application scope found for {0} on host {1}. Misspelled or missing application folder?", path, host));
                                        disconnectOnReturn = true;
                                    }
                                    else
                                    {
                                        IScopeContext context = global.Context;
                                        IScope scope = null;
                                        try
                                        {
                                            scope = context.ResolveScope(global, path);
                                        }
                                        catch (ScopeNotFoundException /*exception*/)
                                        {
                                            if (log.IsErrorEnabled)
                                                log.Error(__Res.GetString(__Res.Scope_NotFound, path));

                                            serviceCall.Status = Call.STATUS_SERVICE_NOT_FOUND;
                                            if (serviceCall is IPendingServiceCall)
                                            {
                                                StatusASO status = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_REJECTED, connection.ObjectEncoding);
                                                status.description = "No scope \"" + path + "\" on this server.";
                                                (serviceCall as IPendingServiceCall).Result = status;
                                            }
                                            disconnectOnReturn = true;
                                        }
                                        catch (ScopeShuttingDownException)
                                        {
                                            serviceCall.Status = Call.STATUS_APP_SHUTTING_DOWN;
                                            if (serviceCall is IPendingServiceCall)
                                            {
                                                StatusASO status = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_APPSHUTDOWN, connection.ObjectEncoding);
                                                status.description = "Application at \"" + path + "\" is currently shutting down.";
                                                (serviceCall as IPendingServiceCall).Result = status;
                                            }
                                            log.Info(string.Format("Application at {0} currently shutting down on {1}", path, host));
                                            disconnectOnReturn = true;
                                        }
                                        if (scope != null)
                                        {
                                            if (log.IsInfoEnabled)
                                                log.Info(__Res.GetString(__Res.Scope_Connect, scope.Name));
                                            bool okayToConnect;
                                            try
                                            {
                                                //The only way to differentiate NetConnection.connect() and Consumer.subscribe() seems to be the app name
                                                if (app == string.Empty)
                                                {
                                                    connection.SetIsFlexClient(true);
                                                    okayToConnect = connection.Connect(scope, serviceCall.Arguments);
                                                    if (okayToConnect)
                                                    {
                                                        if (serviceCall.Arguments != null && serviceCall.Arguments.Length >= 3)
                                                        {
                                                            string credentials = serviceCall.Arguments[2] as string;
                                                            if (credentials != null && credentials != string.Empty)
                                                            {
                                                                MessageBroker messageBroker = this.Endpoint.GetMessageBroker();
                                                                AuthenticationService authenticationService = messageBroker.GetService(AuthenticationService.ServiceId) as AuthenticationService;
                                                                authenticationService.Authenticate(credentials);
                                                            }
                                                        }
                                                        //FDS 2.0.1 fds.swc
                                                        if (serviceCall.Arguments != null && serviceCall.Arguments.Length == 1)
                                                        {
                                                            string credentials = serviceCall.Arguments[0] as string;
                                                            if (credentials != null && credentials != string.Empty)
                                                            {
                                                                MessageBroker messageBroker = this.Endpoint.GetMessageBroker();
                                                                AuthenticationService authenticationService = messageBroker.GetService(AuthenticationService.ServiceId) as AuthenticationService;
                                                                authenticationService.Authenticate(credentials);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    connection.SetIsFlexClient(false);
                                                    okayToConnect = connection.Connect(scope, serviceCall.Arguments);
                                                }
                                                if (okayToConnect)
                                                {
                                                    if (log.IsDebugEnabled)
                                                        log.Debug("Connected RtmpClient: " + connection.Client.Id);
                                                    serviceCall.Status = Call.STATUS_SUCCESS_RESULT;
                                                    if (serviceCall is IPendingServiceCall)
                                                    {
                                                        StatusASO statusASO = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_SUCCESS, connection.ObjectEncoding);
                                                        statusASO.Add("id", connection.Client.Id);
                                                        (serviceCall as IPendingServiceCall).Result = statusASO;
                                                    }
                                                    // Measure initial roundtrip time after connecting
                                                    connection.GetChannel((byte)2).Write(new Ping(Ping.StreamBegin, 0, -1));
                                                    connection.StartRoundTripMeasurement();
                                                }
                                                else
                                                {
                                                    if (log.IsDebugEnabled)
                                                        log.Debug("Connect failed");
                                                    serviceCall.Status = Call.STATUS_ACCESS_DENIED;
                                                    if (serviceCall is IPendingServiceCall)
                                                        (serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_REJECTED, connection.ObjectEncoding);
                                                    disconnectOnReturn = true;
                                                }
                                            }
                                            catch (ClientRejectedException rejected)
                                            {
                                                if (log.IsDebugEnabled)
                                                    log.Debug("Connect rejected");
                                                serviceCall.Status = Call.STATUS_ACCESS_DENIED;
                                                if (serviceCall is IPendingServiceCall)
                                                {
                                                    StatusASO statusASO = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_REJECTED, connection.ObjectEncoding);
                                                    statusASO.Application = rejected.Reason;
                                                    (serviceCall as IPendingServiceCall).Result = statusASO;
                                                }
                                                disconnectOnReturn = true;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (log.IsErrorEnabled)
                                        log.Error("Error connecting", ex);

                                    serviceCall.Status = Call.STATUS_GENERAL_EXCEPTION;
                                    if (serviceCall is IPendingServiceCall)
                                        (serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_FAILED, connection.ObjectEncoding);
                                    disconnectOnReturn = true;
                                }
                            }
                            else
                            {
                                // Service calls, must be connected.
                                InvokeCall(connection, serviceCall);
                            }
                        }
                        break;
                    case ACTION_DISCONNECT:
                        connection.Close();
                        break;
                    case ACTION_CREATE_STREAM:
                    case ACTION_DELETE_STREAM:
                    case ACTION_RELEASE_STREAM:
                    case ACTION_PUBLISH:
                    case ACTION_PLAY:
                    case ACTION_SEEK:
                    case ACTION_PAUSE:
                    case ACTION_CLOSE_STREAM:
                    case ACTION_RECEIVE_VIDEO:
                    case ACTION_RECEIVE_AUDIO:
                        {
                            IStreamService streamService = ScopeUtils.GetScopeService(connection.Scope, typeof(IStreamService)) as IStreamService;
                            StatusASO status = null;
                            try
                            {
                                if (!InvokeCall(connection, serviceCall, streamService))
                                {
                                    status = StatusASO.GetStatusObject(StatusASO.NS_INVALID_ARGUMENT, connection.ObjectEncoding);
                                    status.description = "Failed to " + action + " (stream ID: " + header.StreamId + ")";
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error while invoking " + action + " on stream service.", ex);
                                status = StatusASO.GetStatusObject(StatusASO.NS_FAILED, connection.ObjectEncoding);
                                status.description = "Error while invoking " + action + " (stream ID: " + header.StreamId + ")";
                                status.details = ex.Message;
                            }
                            if (status != null)
                                channel.SendStatus(status);
                        }
                        break;
                    default:
                        if (connection.IsConnected)
                            InvokeCall(connection, serviceCall);
                        else
                        {
                            // Warn user attemps to call service without being connected
                            if (log.IsWarnEnabled)
                                log.Warn("Not connected, closing connection");
                            connection.Close();
                        }
                        break;
                }
            }
            /*
			if(invoke is FlexInvoke) 
			{
				FlexInvoke reply = new FlexInvoke();
				reply.InvokeId = invoke.InvokeId;
				reply.SetResponseSuccess();
				//TODO
				if( serviceCall is IPendingServiceCall )
				{
					IPendingServiceCall pendingCall = (IPendingServiceCall)serviceCall;
					reply.Response = pendingCall.Result;
				}
				channel.Write(reply);
			}
			else if(invoke is Invoke) 
            */
            if (invoke is Invoke) 
			{
				if((header.StreamId != 0)
					&& (serviceCall.Status == Call.STATUS_SUCCESS_VOID || serviceCall.Status == Call.STATUS_SUCCESS_NULL)) 
				{
				    if (log.IsDebugEnabled)
					    log.Debug("Method does not have return value, do not reply");
					return;
				}

				// The client expects a result for the method call.
				Invoke reply = new Invoke();
				reply.ServiceCall = serviceCall;
				reply.InvokeId = invoke.InvokeId;
				//sending reply
				channel.Write(reply);
			}
			if (disconnectOnReturn)
			{
				connection.Close();
			}
            if (action == ACTION_CONNECT)
            {
                connection.Context.ObjectEncoding = connection.ObjectEncoding;
            }
		}

        protected override void OnSharedObject(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, SharedObjectMessage message) 
		{
			ISharedObject so = null;
			string name = message.Name;
			IScope scope = connection.Scope;
            bool persistent = message.IsPersistent;
			if(scope == null) 
			{
                // The scope already has been deleted.
                SendSOCreationFailed(connection, name, persistent);
				return;
			}
            ISharedObjectService sharedObjectService = ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService)) as ISharedObjectService;
			if (!sharedObjectService.HasSharedObject(scope, name))
			{
                ISharedObjectSecurityService securityService = ScopeUtils.GetScopeService(scope, typeof(ISharedObjectSecurityService)) as ISharedObjectSecurityService;
			    if (securityService != null) 
                {
				    // Check handlers to see if creation is allowed
                    IEnumerator enumerator = securityService.GetSharedObjectSecurity();
                    while(enumerator.MoveNext())
                    {
                        ISharedObjectSecurity handler = enumerator.Current as ISharedObjectSecurity;
                        if (!handler.IsCreationAllowed(scope, name, persistent))
                        {
                            SendSOCreationFailed(connection, name, persistent);
						    return;
					    }
				    }
			    }

                if (!sharedObjectService.CreateSharedObject(scope, name, persistent)) 
				{
                    SendSOCreationFailed(connection, name, persistent);
					return;
				}
			}
			so = sharedObjectService.GetSharedObject(scope, name);
            if (so.IsPersistentObject != persistent)
            {
                log.Debug(string.Format("Shared object '{0}' persistence mismatch", name));
                SendSOPersistenceMismatch(connection, name, persistent);
                return;
            }
			so.DispatchEvent(message);
		}

        private static void SendSOCreationFailed(RtmpConnection connection, string name, bool persistent)
        {
            SharedObjectMessage msg;
            if (connection.ObjectEncoding == ObjectEncoding.AMF0)
                msg = new SharedObjectMessage(name, 0, persistent);
            else
                msg = new FlexSharedObjectMessage(name, 0, persistent);
            msg.AddEvent(new SharedObjectEvent(SharedObjectEventType.CLIENT_STATUS, StatusASO.SO_CREATION_FAILED, "error"));
            connection.GetChannel((byte)3).Write(msg);
        }

        private static void SendSOPersistenceMismatch(RtmpConnection connection, string name, bool persistent)
        {
            SharedObjectMessage msg;
            if (connection.ObjectEncoding == ObjectEncoding.AMF0)
                msg = new SharedObjectMessage(name, 0, persistent);
            else
                msg = new FlexSharedObjectMessage(name, 0, persistent);
            msg.AddEvent(new SharedObjectEvent(SharedObjectEventType.CLIENT_STATUS, StatusASO.SO_PERSISTENCE_MISMATCH, "error"));
            connection.GetChannel((byte)3).Write(msg);
        }

        protected override void OnFlexInvoke(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, FlexInvoke invoke)
		{
            IMessage message = null;
            if (invoke.ServiceCall.Arguments != null && invoke.ServiceCall.Arguments.Length > 0)
                message = invoke.ServiceCall.Arguments[0] as IMessage;
			if( message != null )
			{
                MessageBroker messageBroker = this.Endpoint.GetMessageBroker();
				if( message.clientId == null )
				{
					message.clientId = Guid.NewGuid().ToString("D");
					/*
					if( !(message is CommandMessage) )
					{
						//producer may send messages without subscribing
						CommandMessage commandMessageSubscribe = new CommandMessage(CommandMessage.SubscribeOperation);
						commandMessageSubscribe.messageId = Guid.NewGuid().ToString("D");
						commandMessageSubscribe.headers = message.headers.Clone() as Hashtable;
						commandMessageSubscribe.messageRefType = message.GetType().FullName;//"flex.messaging.messages.AsyncMessage"
						commandMessageSubscribe.destination = message.destination;

						IMessage subscribeResponse = messageBroker.RouteMessage(commandMessageSubscribe, _endpoint, connection);
						message.clientId = subscribeResponse.clientId;
					}
					}
					*/
				}
                IMessage response = messageBroker.RouteMessage(message, this.Endpoint);
                invoke.ServiceCall.Status = response is ErrorMessage ? Call.STATUS_INVOCATION_EXCEPTION : Call.STATUS_SUCCESS_RESULT;
                if (invoke.ServiceCall is IPendingServiceCall)
                    (invoke.ServiceCall as IPendingServiceCall).Result = response;

				FlexInvoke reply = new FlexInvoke();
				reply.InvokeId = invoke.InvokeId;
                reply.ServiceCall = invoke.ServiceCall;
                /*
                if( response is ErrorMessage )
                    reply.SetResponseFailure();
                else
				    reply.SetResponseSuccess();
				reply.Response = response;
                */
				channel.Write(reply);
			}
			else
			{
				// If it's a callback for server remote call then pass it over to callbacks handler and return
				OnInvoke(connection, channel, header, invoke);
			}
		}

        public static void InvokeCall(RtmpConnection connection, IServiceCall serviceCall) 
		{
			IScope scope = connection.Scope;
			if(scope.HasHandler) 
			{
				IScopeHandler handler = scope.Handler;
				if(!handler.ServiceCall(connection, serviceCall)) 
				{
					// What do do here? Return an error?
					return;
				}
			}
			IScopeContext context = scope.Context;
			context.ServiceInvoker.Invoke(serviceCall, scope);
		}

        /// <summary>
        /// Remoting call invocation handler.
        /// </summary>
        /// <param name="connection">RTMP connection.</param>
        /// <param name="serviceCall">Service call.</param>
        /// <param name="service">Server-side service object.</param>
        /// <returns><code>true</code> if the call was performed, otherwise <code>false</code>.</returns>
	    private static bool InvokeCall(RtmpConnection connection, IServiceCall serviceCall, object service) 
        {
		    IScope scope = connection.Scope;
		    IScopeContext context = scope.Context;
		    if (log.IsDebugEnabled) 
            {
			    log.Debug("Scope: " + scope);
			    log.Debug("Service: " + service);
			    log.Debug("Context: " + context);
		    }
            return context.ServiceInvoker.Invoke(serviceCall, service);
	    }
	}
}
