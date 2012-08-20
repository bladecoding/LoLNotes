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
using System.Web;
using System.Threading;
using System.IO;
using FluorineFx;
using FluorineFx.Context;
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
	class AMFEndpoint : EndpointBase
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(AMFEndpoint));
        protected FilterChain _filterChain;
        AtomicInteger _waitingPollRequests;

        public AMFEndpoint(MessageBroker messageBroker, ChannelDefinition channelDefinition)
            : base(messageBroker, channelDefinition)
		{
            _waitingPollRequests = new AtomicInteger();
		}

		public override void Start()
		{
			DeserializationFilter deserializationFilter = new DeserializationFilter();
			deserializationFilter.UseLegacyCollection = this.IsLegacyCollection;
			ServiceMapFilter serviceMapFilter = new ServiceMapFilter(this);
			WsdlFilter wsdlFilter = new WsdlFilter();
            ContextFilter contextFilter = new ContextFilter(this);
            AuthenticationFilter authenticationFilter = new AuthenticationFilter(this);
            DescribeServiceFilter describeServiceFilter = new DescribeServiceFilter();
			//CacheFilter cacheFilter = new CacheFilter();
			ProcessFilter processFilter = new ProcessFilter(this);
			MessageFilter messageFilter = new MessageFilter(this);
			DebugFilter debugFilter = new DebugFilter();
			SerializationFilter serializationFilter = new SerializationFilter();
			serializationFilter.UseLegacyCollection = this.IsLegacyCollection;
            serializationFilter.UseLegacyThrowable = this.IsLegacyThrowable;
			
			deserializationFilter.Next = serviceMapFilter;
			serviceMapFilter.Next = wsdlFilter;
            wsdlFilter.Next = contextFilter;
            contextFilter.Next = authenticationFilter;
            authenticationFilter.Next = describeServiceFilter;
            describeServiceFilter.Next = processFilter;
            //describeServiceFilter.Next = cacheFilter;
			//cacheFilter.Next = processFilter;
			processFilter.Next = debugFilter;
			debugFilter.Next = messageFilter;
			messageFilter.Next = serializationFilter;

			_filterChain = new FilterChain(deserializationFilter);
		}

		public override void Stop()
		{
			_filterChain = null;
			base.Stop();
		}

		public override void Service()
		{
            //AMFContext amfContext = new AMFContext(HttpContext.Current.Request.InputStream, HttpContext.Current.Response.OutputStream);
            AMFContext amfContext = null;
            if (FluorineConfiguration.Instance.FluorineSettings.Debug != null &&
                FluorineConfiguration.Instance.FluorineSettings.Debug.Mode != Debug.Off)
            {
                MemoryStream ms = new MemoryStream();
                int bufferSize = 255;
                byte[] buffer = new byte[bufferSize];
                int byteCount = 0;
                while ((byteCount = HttpContext.Current.Request.InputStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    ms.Write(buffer, 0, byteCount);
                }
                ms.Seek(0, SeekOrigin.Begin);
                amfContext = new AMFContext(ms, HttpContext.Current.Response.OutputStream);
            }
            if( amfContext == null )
                amfContext = new AMFContext(HttpContext.Current.Request.InputStream, HttpContext.Current.Response.OutputStream);
            AMFContext.Current = amfContext;
            _filterChain.InvokeFilters(amfContext);
        }

        public override IMessage ServiceMessage(IMessage message)
        {
            if (FluorineContext.Current.Client != null)
                FluorineContext.Current.Client.Renew();

            if (message is CommandMessage)
            {
                CommandMessage commandMessage = message as CommandMessage;
                switch (commandMessage.operation)
                {
                    case CommandMessage.PollOperation:
                        {
                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Endpoint_HandleMessage, this.Id, message.ToString()));

                            if (FluorineContext.Current.Client != null)
                                FluorineContext.Current.Client.Renew();

                            //IMessage[] messages = null;
                            IList messages = null;
                            _waitingPollRequests.Increment();
                            int waitIntervalMillis = this.ChannelDefinition.Properties.WaitIntervalMillis != -1 ? this.ChannelDefinition.Properties.WaitIntervalMillis : 60000;// int.MaxValue;

                            if (commandMessage.HeaderExists(CommandMessage.FluorineSuppressPollWaitHeader))
                                waitIntervalMillis = 0;
                            //If async handling was not set long polling is not supported
                            if (!FluorineConfiguration.Instance.FluorineSettings.Runtime.AsyncHandler)
                                waitIntervalMillis = 0;
                            if (this.ChannelDefinition.Properties.MaxWaitingPollRequests <= 0 || _waitingPollRequests.Value >= this.ChannelDefinition.Properties.MaxWaitingPollRequests)
                                waitIntervalMillis = 0;

                            if (message.destination != null && message.destination != string.Empty)
                            {
                                string clientId = commandMessage.clientId as string;
                                MessageDestination messageDestination = this.GetMessageBroker().GetDestination(message.destination) as MessageDestination;
                                MessageClient client = messageDestination.SubscriptionManager.GetSubscriber(clientId);
                                client.Renew();
                                //messages = client.GetPendingMessages();
                            }
                            else
                            {
                                //if (FluorineContext.Current.Client != null)
                                //    messages = FluorineContext.Current.Client.GetPendingMessages(waitIntervalMillis);
                            }

                            if (FluorineContext.Current.Client != null)
                            {
                                IEndpointPushHandler handler = FluorineContext.Current.Client.GetEndpointPushHandler(this.Id);
                                if (handler != null)
                                    messages = handler.GetPendingMessages();
                                if (messages == null)
                                {
                                    lock (handler.SyncRoot)
                                    {
                                        Monitor.Wait(handler.SyncRoot, waitIntervalMillis);
                                    }
                                    messages = handler.GetPendingMessages();
                                }
                            }

                            _waitingPollRequests.Decrement();
                            IMessage response = null;
                            if (messages == null || messages.Count == 0)
                                response = new AcknowledgeMessage();
                            else
                            {
                                CommandMessage resultMessage = new CommandMessage();
                                resultMessage.operation = CommandMessage.ClientSyncOperation;
                                resultMessage.body = messages;
                                response = resultMessage;
                            }
                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Endpoint_Response, this.Id, response.ToString()));
                            return response;

                        }
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

                            if (this.ChannelDefinition.Properties.IsPollingEnabled)
                            {
                                //Create and forget, client will close the notifier
                                IEndpointPushHandler handler = FluorineContext.Current.Client.GetEndpointPushHandler(this.Id);
                                if( handler == null )
                                    handler = new EndpointPushNotifier(this, FluorineContext.Current.Client);
                                /*
                                lock (_endpointPushHandlers.SyncRoot)
                                {
                                    _endpointPushHandlers.Add(notifier.Id, notifier);
                                }
                                */
                            }
                        }
                        break;
                    case CommandMessage.DisconnectOperation:
                        {
                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Endpoint_HandleMessage, this.Id, message.ToString()));

                            if (FluorineContext.Current.Client != null && FluorineContext.Current.Client.IsValid)
                            {
                                IList messageClients = FluorineContext.Current.Client.MessageClients;
                                if (messageClients != null)
                                {
                                    foreach (MessageClient messageClient in messageClients)
                                    {
                                        messageClient.Invalidate();
                                    }
                                }
                                FluorineContext.Current.Client.Invalidate();
                            }
                            if (FluorineContext.Current.Session != null)
                            {
                                FluorineContext.Current.Session.Invalidate();
                            }
                            //Disconnect command is received from a client channel.
                            //The response returned by this method is not guaranteed to get to the client, which is free to terminate its physical connection at any point.
                            IMessage response = new AcknowledgeMessage();

                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Endpoint_Response, this.Id, response.ToString()));
                            return response;
                        }
                }
            }
            return base.ServiceMessage(message);
        }

        public override void Push(IMessage message, MessageClient messageClient)
        {
            if (this.ChannelDefinition.Properties.IsPollingEnabled)
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
            else
            {
                if (log.IsWarnEnabled)
                    log.Warn("Push request received for the non-polling AMF endpoint '" + this.Id + "'");
            }
        }

        public override int ClientLeaseTime
        {
            get 
            {
                int timeout = this.GetMessageBroker().FlexClientSettings.TimeoutMinutes;
                timeout = Math.Max(timeout, 1);//start with 1 minute timeout at least
                if (this.ChannelDefinition.Properties.IsPollingEnabled)
                {
                    int pollingInterval = this.ChannelDefinition.Properties.PollingIntervalSeconds / 60;
                    timeout = Math.Max(timeout, pollingInterval + 1);//set timout 1 minute longer then the polling interval in minutes
                }
                return timeout;
            }
        }
	}
}
