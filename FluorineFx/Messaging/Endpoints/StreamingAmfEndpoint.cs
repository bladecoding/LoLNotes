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
using System.Web;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using FluorineFx;
using FluorineFx.IO;
using FluorineFx.Context;
using FluorineFx.Collections;
using FluorineFx.Configuration;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Endpoints.Filter;
using FluorineFx.Messaging.Services.Remoting;
using FluorineFx.Util;
using log4net;

namespace FluorineFx.Messaging.Endpoints
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class StreamingAmfEndpoint : AMFEndpoint
    {
        static byte[] Heartbeat = new byte[] { 0x00};

        private static byte[] CRLF_BYTES = {(byte)13, (byte)10};
        /// <summary>
        /// Parameter name for 'command' passed in a request for a new streaming connection.
        /// </summary>
        internal const string CommandParameterName = "command";
        /// <summary>
        /// Token at the end of the HTTP request line that indicates that it's
        /// a stream connection that we should hold open to push data back to the client over
        /// as opposed to a regular request-response message.
        /// </summary>
        internal const string OpenCommand = "open";
        /// <summary>
        /// This is the token at the end of the HTTP request line that indicates that it's
        /// a stream connection that we should close.
        /// </summary>
        internal const string CloseCommand = "close";
        /// <summary>
        /// Parameter name for the stream id; passed with commands for an existing streaming connection.
        /// </summary>
        private const string StreamIdParameterName = "streamId";
        /// <summary>
        /// Parameter name for version passed in a request for a new streaming connection.
        /// </summary>
        private const string VERSION_PARAM_NAME = "version";
        /// <summary>
        /// Constant for HTTP/1.0.
        /// </summary>
        private const string HTTP_1_0 = "HTTP/1.0";

        private static readonly ILog log = LogManager.GetLogger(typeof(StreamingAmfEndpoint));

        AtomicInteger _streamingClientsCount;
        Hashtable _currentStreamingRequests;
        volatile bool _canStream = true;

        public StreamingAmfEndpoint(MessageBroker messageBroker, ChannelDefinition channelDefinition)
            : base(messageBroker, channelDefinition)
		{
            _streamingClientsCount = new AtomicInteger();
		}

        public int MaxStreamingClients
        {
            get { return this.ChannelDefinition.Properties.MaxStreamingClients; }
        }

        public override void Start()
        {
            _currentStreamingRequests = new Hashtable();
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            // Shutdown currently open streaming connections.
            lock (_currentStreamingRequests.SyncRoot)
            {
                foreach (DictionaryEntry entry in _currentStreamingRequests)
                {
                    EndpointPushNotifier notifier = entry.Value as EndpointPushNotifier;
                    notifier.Close();
                }
                _currentStreamingRequests = null;
            }
        }

        public override void Service()
        {
            string command = HttpContext.Current.Request.Params["command"];
            if (command != null)
                ServiceStreamingRequest();
            else
                base.Service();
        }

        private void ServiceStreamingRequest()
        {
            string command = HttpContext.Current.Request.Params[CommandParameterName];
            // Only HTTP 1.1 is supported, disallow HTTP 1.0.
            if (HTTP_1_0.Equals(HttpContext.Current.Request.ServerVariables["SERVER_PROTOCOL"]))
            {
                string msg = string.Format("Endpoint with id {0} cannot service the streaming request made with HTTP 1.0. Only HTTP 1.1 is supported.", this.Id);
                if (log.IsWarnEnabled)
                    log.Warn(msg);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }

            if (!OpenCommand.Equals(command) && !CloseCommand.Equals(command))
            {
                string msg = string.Format("Unknown streaming request {0} in endpoint {1}", command, this.Id);
                log.Error(msg);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException) 
                { }
                return;
            }

            if (!FluorineConfiguration.Instance.FluorineSettings.Runtime.AsyncHandler)
            {
                string msg = "The requested operation is not supported in synchronous mode";
                log.Error(msg);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }

            string flexClientId = HttpContext.Current.Request.Params[MessageBase.FlexClientIdHeader];
            if (flexClientId == null)
            {
                string msg = string.Format("Cannot service streaming request FlexClient id is missing in endpoint {0}", this.Id);
                log.Error(msg);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException) 
                { }
                return;
            }
            if (FluorineContext.Current.Client == null)
            {
                if (!CloseCommand.Equals(command))
                {
                    //HttpSession session = HttpSession.GetHttpSession();
                    Client client = this.GetMessageBroker().ClientRegistry.GetClient(flexClientId) as Client;
                    //FluorineContext.Current.SetSession(session);
                    FluorineContext.Current.SetClient(client);

                    if (client == null || !client.IsValid)
                    {
                        string msg = string.Format("Cannot service streaming request Client {0} is missing in endpoint {0}", flexClientId, this.Id);
                        log.Error(msg);
                        try
                        {
                            HandleBadRequest(msg, HttpContext.Current.Response);
                        }
                        catch (HttpException)
                        { }
                        return;
                    }
                    //lease time?
                }
            }
            if (FluorineContext.Current.Session == null)
            {
                if (HttpContext.Current.Items.Contains(Session.FxASPNET_SessionId))
                {
                    HttpCookie cookie = HttpContext.Current.Items[Session.FxASPNET_SessionId] as HttpCookie;
                    ISession session = this.GetMessageBroker().SessionManager.GetHttpSession(HttpContext.Current, cookie.Value);
                    FluorineContext.Current.SetSession(session);
                }
            }
            if (FluorineContext.Current.Client != null)
            {
                FluorineContext.Current.Client.Renew();
                if (OpenCommand.Equals(command))
                    HandleFlexClientStreamingOpenRequest(HttpContext.Current.Request, HttpContext.Current.Response, FluorineContext.Current.Client);
            }
            if (CloseCommand.Equals(command))
                HandleFlexClientStreamingCloseRequest(HttpContext.Current.Request, HttpContext.Current.Response, FluorineContext.Current.Client);
        }

        private void HandleFlexClientStreamingOpenRequest(HttpRequest request, HttpResponse response, IClient flexClient)
        {
            Session session = FluorineContext.Current.Session as Session;
            if (session == null)
            {
                string msg = string.Format("Cannot grant streaming connection when ASP.NET session state is disabled", this.Id);
                if (log.IsWarnEnabled)
                    log.Warn(msg);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }
            if (!_canStream || !session.CanStream)
            {
                string msg = string.Format("Cannot grant streaming connection, limit has been reached", this.Id);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }

            bool canStream = false;
            lock (this.SyncRoot)
            {
                _streamingClientsCount.Increment();
                if (_streamingClientsCount.Value == this.MaxStreamingClients)
                {
                    canStream = true; // This thread got the last spot.
                    _canStream = false;
                    
                }
                else if (_streamingClientsCount.Value > this.MaxStreamingClients)
                {
                    canStream = false; // This thread lost the last spot.
                    _streamingClientsCount.Decrement();// We're not going to grant the streaming right to the client.
                }
                else
                {
                    // Allow this thread to stream.
                    canStream = true;
                }
            }
            if (!canStream)
            {
                string msg = string.Format("Cannot service streaming request, max-streaming-clients reached in endpoint {0}", this.Id);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }

            UserAgent userAgent = this.ChannelDefinition.Properties.UserAgentSettings[request.Browser.Browser];
            if (userAgent != null)
            {
                lock (session.SyncRoot)
                {
                    session.MaxConnectionsPerSession = userAgent.MaxStreamingConnectionsPerSession;
                }
            }

            lock (session.SyncRoot)
            {
                session.StreamingConnectionsCount++;
                if (session.StreamingConnectionsCount == session.MaxConnectionsPerSession)
                {
                    canStream = true; // This thread got the last spot in the session.
                    session.CanStream = false;
                }
                else if (session.StreamingConnectionsCount > session.MaxConnectionsPerSession)
                {
                    canStream = false;
                    session.StreamingConnectionsCount--;
                    _streamingClientsCount.Decrement();
                }
                else
                {
                    canStream = true;
                }
            }
            if (!canStream)
            {
                string msg = string.Format("Cannot grant streaming connection, limit has been reached", this.Id);
                try
                {
                    HandleBadRequest(msg, HttpContext.Current.Response);
                }
                catch (HttpException)
                { }
                return;
            }

            EndpointPushNotifier notifier = null;
            try
            {
                response.ContentType = ContentType.AMF;
                response.AppendHeader("Cache-Control", "no-cache");
                response.AppendHeader("Pragma", "no-cache");
                response.AppendHeader("Connection", "close");
                //response.AppendHeader("Transfer-Encoding", "chunked");
                response.Flush();

                // Setup for specific user agents.
                byte[] kickStartBytesToStream = null;
                int kickStartBytes = userAgent != null ? userAgent.KickstartBytes : 0;
                if (kickStartBytes > 0)
                {
                    // The minimum number of actual bytes that need to be sent to kickstart, taking into account transfer-encoding overhead.
                    try
                    {
                        int chunkLengthHeaderSize = System.Text.Encoding.ASCII.GetBytes(System.Convert.ToString(kickStartBytes, 0x10)).Length; //System.Text.ASCIIEncoding.ASCII.GetBytes(kickStartBytes.ToString("X")).Length;
                        int chunkOverhead = chunkLengthHeaderSize + 4; // 4 for the 2 wrapping CRLF tokens.
                        int minimumKickstartBytes = kickStartBytes - chunkOverhead;
                        kickStartBytesToStream = new byte[(minimumKickstartBytes > 0) ? minimumKickstartBytes : kickStartBytes];
                    }
                    catch
                    {
                        kickStartBytesToStream = new byte[kickStartBytes];
                    }
                }
                if (kickStartBytesToStream != null)
                {
                    StreamChunk(kickStartBytesToStream, response);
                }
                try
                {
                    notifier = new EndpointPushNotifier(this, flexClient);
                    lock (_currentStreamingRequests.SyncRoot)
                    {
                        _currentStreamingRequests.Add(notifier.Id, notifier);
                    }
                    // Push down an acknowledgement for the 'connect' request containing the unique id for this specific stream.
                    AcknowledgeMessage connectAck = new AcknowledgeMessage();
                    connectAck.body = notifier.Id;
                    connectAck.correlationId = OpenCommand;
                    StreamMessage(connectAck, response);
                }
                catch (MessageException)
                {
                }

                if (log.IsDebugEnabled)
                {
                    string msg = string.Format("Start streaming for endpoint with id {0} and client with id {1}", this.Id, flexClient.Id);
                    log.Debug(msg);
                }

                int serverToClientHeartbeatMillis = this.ChannelDefinition.Properties.ServerToClientHeartbeatMillis >= 0 ? this.ChannelDefinition.Properties.ServerToClientHeartbeatMillis : 0;
                serverToClientHeartbeatMillis = 100;
                while (!notifier.IsClosed)
                {
                    IList messages = notifier.GetPendingMessages();
                    StreamMessages(messages, response);
                    lock (notifier.SyncRoot)
                    {
                        Monitor.Wait(notifier.SyncRoot, serverToClientHeartbeatMillis);

                        messages = notifier.GetPendingMessages();
                        // If there are no messages to send to the client, send a 0
                        // byte as a heartbeat to make sure the client is still valid.
                        if ((messages == null || messages.Count == 0) && serverToClientHeartbeatMillis > 0)
                        {
                            try
                            {
                                StreamChunk(Heartbeat, response);
                                response.Flush();
                            }
                            catch (HttpException)
                            {
                                break;
                            }
                            catch (IOException)
                            {
                                break;
                            }
                        }
                        else
                        {
                            StreamMessages(messages, response);
                        }
                    }
                }
                // Terminate the response.
                StreamChunk(null, response);
                if (log.IsDebugEnabled)
                {
                    string msg = string.Format("Releasing streaming connection for endpoint with id {0} and and client with id {1}", this.Id, flexClient.Id);
                    log.Debug(msg);
                }
            }
            catch (IOException ex)//HttpException?
            {
                if (log.IsWarnEnabled)
                    log.Warn("Streaming thread for endpoint with id " + this.Id + " is closing connection due to an IO error.", ex);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Streaming thread for endpoint with id " + this.Id + " is closing connection due to an error.", ex);
            }
            finally
            {
                if (notifier != null && _currentStreamingRequests != null)
                {
                    if (_currentStreamingRequests != null)
                    {
                        lock (_currentStreamingRequests.SyncRoot)
                        {
                            _currentStreamingRequests.Remove(notifier.Id);
                        }
                    }
                    notifier.Close();
                }
                _streamingClientsCount.Decrement();
                lock (session.SyncRoot)
                {
                    session.StreamingConnectionsCount--;
                    session.CanStream = session.StreamingConnectionsCount < session.MaxConnectionsPerSession;
                }
            }
        }

        private void HandleFlexClientStreamingCloseRequest(HttpRequest request, HttpResponse response, IClient flexClient)
        {
            if (flexClient != null)
            {
                if (log.IsDebugEnabled)
                {
                    string msg = string.Format("Close streaming for endpoint with id {0} and client with id {1}", this.Id, flexClient.Id);
                    log.Debug(msg);
                }
                string streamId = HttpContext.Current.Request.Params[StreamIdParameterName];
                IEndpointPushHandler notifier = flexClient.GetEndpointPushHandler(this.Id);
                if (notifier != null && notifier.Id == streamId)
                    notifier.Close();
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    string flexClientId = HttpContext.Current.Request.Params[MessageBase.FlexClientIdHeader];
                    string msg = string.Format("Skip close streaming request for endpoint with id {0} and client with id {1}", this.Id, flexClientId);
                    log.Debug(msg);
                }
            }
        }

        private void HandleBadRequest(string message, HttpResponse response)
        {
            response.StatusCode = 400;
            response.ContentType = "text/plain";
            response.AppendHeader("Content-Length", message.Length.ToString());
            response.Write(message);
            response.Flush();
        }

        protected void StreamChunk(byte[] bytes, HttpResponse response)
        {
            if (bytes != null && bytes.Length > 0)
            {
                byte[] chunkLength = System.Text.Encoding.ASCII.GetBytes(System.Convert.ToString(bytes.Length, 0x10));
                response.BinaryWrite(chunkLength);
                response.BinaryWrite(CRLF_BYTES);
                response.BinaryWrite(bytes);
                response.Flush();
            }
            else
            {
                //response.OutputStream.WriteByte(48);
                //response.BinaryWrite(CRLF_BYTES);
                response.Flush();
            }
        }

        protected void StreamMessages(IList messages, HttpResponse response)
        {
            if (messages == null || messages.Count == 0)
                return;
            foreach (IMessage message in messages)
            {
                StreamMessage(message, response);
            }
        }

        protected void StreamMessage(IMessage message, HttpResponse response)
        {
            MemoryStream ms = new MemoryStream();
            AMFWriter writer = new AMFWriter(ms);
            writer.UseLegacyCollection = this.IsLegacyCollection;
            writer.UseLegacyThrowable = this.IsLegacyThrowable;
            writer.WriteAMF3Data(message);
            ms.Close();
            byte[] messageBytes = ms.ToArray();
            StreamChunk(messageBytes, response);
        }

        public override IMessage ServiceMessage(IMessage message)
        {
            if (message is CommandMessage)
            {
                CommandMessage commandMessage = message as CommandMessage;
                switch (commandMessage.operation)
                {
                    case CommandMessage.PollOperation:
                        {
                            if (FluorineContext.Current.Client != null)
                                FluorineContext.Current.Client.Renew();
                        }
                        break;
                    case CommandMessage.SubscribeOperation:
                        {
                            /*
                            if (FluorineContext.Current.Client == null)
                                FluorineContext.Current.SetCurrentClient(this.GetMessageBroker().ClientRegistry.GetClient(message));
                            RemotingConnection remotingConnection = null;
                            foreach (IConnection connection in FluorineContext.Current.Client.Connections)
                            {
                                if (connection is RemotingConnection)
                                {
                                    remotingConnection = connection as RemotingConnection;
                                    break;
                                }
                            }
                            if (remotingConnection == null)
                            {
                                remotingConnection = new RemotingConnection(this, null, FluorineContext.Current.Client.Id, null);
                                FluorineContext.Current.Client.Renew(this.ClientLeaseTime);
                                remotingConnection.Initialize(FluorineContext.Current.Client);
                            }
                            FluorineWebContext webContext = FluorineContext.Current as FluorineWebContext;
                            webContext.SetConnection(remotingConnection);
                            */
                        }
                        break;
                }
            }
            return base.ServiceMessage(message);
        }

        public override void Push(IMessage message, MessageClient messageClient)
        {
            IEndpointPushHandler handler = messageClient.Client.GetEndpointPushHandler(this.Id);
            if (handler != null)
            {
                IMessage messageClone = message.Copy() as IMessage;
                messageClone.SetHeader(MessageBase.DestinationClientIdHeader, messageClient.ClientId);
                messageClone.clientId = messageClient.ClientId;
                handler.PushMessage(messageClone);
            }
            /*
            IMessage messageClone = message.Clone() as IMessage;
            messageClone.SetHeader(MessageBase.DestinationClientIdHeader, messageClient.ClientId);
            messageClone.clientId = messageClient.ClientId;
            messageClient.AddMessage(messageClone);
            */
        }

        public override int ClientLeaseTime
        {
            get
            {
                int timeout = this.GetMessageBroker().FlexClientSettings.TimeoutMinutes;
                timeout = Math.Max(timeout, 1);//start with 1 minute timeout at least
                int idleTimeoutMinutes = this.ChannelDefinition.Properties.IdleTimeoutMinutes;
                timeout = Math.Max(timeout, idleTimeoutMinutes);
                return timeout;
            }
        }

    }
}
