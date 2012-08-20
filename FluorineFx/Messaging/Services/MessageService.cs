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
using log4net;
using FluorineFx.Context;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services.Messaging;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Api;
using FluorineFx.IO;
using FluorineFx.Util;

namespace FluorineFx.Messaging.Services
{
	/// <summary>
	/// The MessageService class is the Service implementation that manages point-to-point and publish-subscribe messaging.
	/// </summary>
    [CLSCompliant(false)]
    public class MessageService : ServiceBase
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageService));

        private MessageService()
        {
        }
		/// <summary>
        /// Initializes a new instance of the MessageService class.
		/// </summary>
		/// <param name="messageBroker"></param>
        /// <param name="serviceDefinition"></param>
        public MessageService(MessageBroker messageBroker, ServiceDefinition serviceDefinition)
            : base(messageBroker, serviceDefinition)
		{
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="destinationDefinition"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        protected override Destination NewDestination(DestinationDefinition destinationDefinition)
		{
            return new MessageDestination(this, destinationDefinition);
		}
        /// <summary>
        /// Handles a message routed to the service by the MessageBroker.
        /// </summary>
        /// <param name="message">The message that should be handled by the service.</param>
        /// <returns>The result of the message processing.</returns>
		public override object ServiceMessage(IMessage message)
		{
			CommandMessage commandMessage = message as CommandMessage;
			MessageDestination messageDestination = GetDestination(message) as MessageDestination;
			if( commandMessage != null )
			{
				string clientId = commandMessage.clientId as string;
                MessageClient messageClient = messageDestination.SubscriptionManager.GetSubscriber(clientId);
                AcknowledgeMessage acknowledgeMessage = null;
                switch (commandMessage.operation)
                {
                    case CommandMessage.SubscribeOperation:
                        if (messageClient == null)
                        {
                            if (clientId == null)
                                clientId = Guid.NewGuid().ToString("D");

                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.MessageServiceSubscribe, messageDestination.Id, clientId));

                            string endpointId = commandMessage.GetHeader(MessageBase.EndpointHeader) as string;
                            if (_messageBroker.GetEndpoint(endpointId) == null)
                            {
                                ServiceException serviceException = new ServiceException("Endpoint was not specified");
                                serviceException.FaultCode = "Server.Processing.MissingEndpoint";
                                throw serviceException;
                            }
                            commandMessage.clientId = clientId;

                            if (messageDestination.ServiceAdapter != null && messageDestination.ServiceAdapter.HandlesSubscriptions)
                            {
                                try
                                {
                                    acknowledgeMessage = messageDestination.ServiceAdapter.Manage(commandMessage) as AcknowledgeMessage;
                                }
                                catch (MessageException me)
                                {
                                    acknowledgeMessage = me.GetErrorMessage();
                                    //Leave, do not subscribe
                                    return acknowledgeMessage;
                                }
                                catch (Exception ex)
                                {
                                    //Guard against service adapter failure
                                    acknowledgeMessage = ErrorMessage.GetErrorMessage(commandMessage, ex);
                                    if (log.IsErrorEnabled)
                                        log.Error(__Res.GetString(__Res.ServiceAdapter_ManageFail, this.id, messageDestination.Id, commandMessage), ex);
                                    //Leave, do not subscribe
                                    return acknowledgeMessage;
                                }
                            }

                            Subtopic subtopic = null;
                            Selector selector = null;
                            if (commandMessage.headers != null)
                            {
                                if (commandMessage.headers.ContainsKey(CommandMessage.SelectorHeader))
                                {
                                    selector = Selector.CreateSelector(commandMessage.headers[CommandMessage.SelectorHeader] as string);
                                }
                                if (commandMessage.headers.ContainsKey(AsyncMessage.SubtopicHeader))
                                {
                                    subtopic = new Subtopic(commandMessage.headers[AsyncMessage.SubtopicHeader] as string);
                                }
                            }
                            IClient client = FluorineContext.Current.Client;
                            client.Renew();
                            messageClient = messageDestination.SubscriptionManager.AddSubscriber(clientId, endpointId, subtopic, selector);
                            if (acknowledgeMessage == null)
                                acknowledgeMessage = new AcknowledgeMessage();
                            acknowledgeMessage.clientId = clientId;
                        }
                        else
                        {
                            acknowledgeMessage = new AcknowledgeMessage();
                            acknowledgeMessage.clientId = clientId;
                        }
                        return acknowledgeMessage;
                    case CommandMessage.UnsubscribeOperation:
                        if (log.IsDebugEnabled)
                            log.Debug(__Res.GetString(__Res.MessageServiceUnsubscribe, messageDestination.Id, clientId));

                        if (messageDestination.ServiceAdapter != null && messageDestination.ServiceAdapter.HandlesSubscriptions)
                        {
                            try
                            {
                                acknowledgeMessage = messageDestination.ServiceAdapter.Manage(commandMessage) as AcknowledgeMessage;
                            }
                            catch (MessageException me)
                            {
                                acknowledgeMessage = me.GetErrorMessage();
                            }
                            catch (Exception ex)
                            {
                                //Guard against service adapter failure
                                acknowledgeMessage = ErrorMessage.GetErrorMessage(commandMessage, ex);
                                if (log.IsErrorEnabled)
                                    log.Error(__Res.GetString(__Res.ServiceAdapter_ManageFail, this.id, messageDestination.Id, commandMessage), ex);
                            }
                        }
                        if (messageClient != null)
                            messageDestination.SubscriptionManager.RemoveSubscriber(messageClient);
                        if (acknowledgeMessage == null)
                            acknowledgeMessage = new AcknowledgeMessage();
                        return acknowledgeMessage;
                    case CommandMessage.PollOperation:
                        {
                            if (messageClient == null)
                            {
                                ServiceException serviceException = new ServiceException(string.Format("MessageClient is not subscribed to {0}", commandMessage.destination));
                                serviceException.FaultCode = "Server.Processing.NotSubscribed";
                                throw serviceException;
                            }
                            IClient client = FluorineContext.Current.Client;
                            client.Renew();
                            try
                            {
                                acknowledgeMessage = messageDestination.ServiceAdapter.Manage(commandMessage) as AcknowledgeMessage;
                            }
                            catch (MessageException me)
                            {
                                acknowledgeMessage = me.GetErrorMessage();
                            }
                            catch (Exception ex)
                            {
                                //Guard against service adapter failure
                                acknowledgeMessage = ErrorMessage.GetErrorMessage(commandMessage, ex);
                                if (log.IsErrorEnabled)
                                    log.Error(__Res.GetString(__Res.ServiceAdapter_ManageFail, this.id, messageDestination.Id, commandMessage), ex);
                            }
                            if (acknowledgeMessage == null)
                                acknowledgeMessage = new AcknowledgeMessage();
                            return acknowledgeMessage;
                        }
                    case CommandMessage.ClientPingOperation:
                        if (messageDestination.ServiceAdapter != null && messageDestination.ServiceAdapter.HandlesSubscriptions)
                        {
                            try
                            {
                                messageDestination.ServiceAdapter.Manage(commandMessage);
                            }
                            catch (MessageException)
                            {
                                return false;
                            }
                            catch (Exception ex)
                            {
                                //Guard against service adapter failure
                                if (log.IsErrorEnabled)
                                    log.Error(__Res.GetString(__Res.ServiceAdapter_ManageFail, this.id, messageDestination.Id, commandMessage), ex);
                                return false;
                            }
                        }
                        return true;
                    default:
                        if (log.IsDebugEnabled)
                            log.Debug(__Res.GetString(__Res.MessageServiceUnknown, commandMessage.operation, messageDestination.Id));
                        try
                        {
                            acknowledgeMessage = messageDestination.ServiceAdapter.Manage(commandMessage) as AcknowledgeMessage;
                        }
                        catch (MessageException me)
                        {
                            acknowledgeMessage = me.GetErrorMessage();
                        }
                        catch (Exception ex)
                        {
                            //Guard against service adapter failure
                            acknowledgeMessage = ErrorMessage.GetErrorMessage(commandMessage, ex);
                            if (log.IsErrorEnabled)
                                log.Error(__Res.GetString(__Res.ServiceAdapter_ManageFail, this.id, messageDestination.Id, commandMessage), ex);
                        }
                        if (acknowledgeMessage == null)
                            acknowledgeMessage = new AcknowledgeMessage();
                        return acknowledgeMessage;
                }
			}
			else
			{
				if (log.IsDebugEnabled)
					log.Debug(__Res.GetString(__Res.MessageServiceRoute, messageDestination.Id, message.clientId));

                if (FluorineContext.Current != null && FluorineContext.Current.Client != null)//Not set when user code initiates push
                {
                    IClient client = FluorineContext.Current.Client;
                    client.Renew();
                }
                object result = messageDestination.ServiceAdapter.Invoke(message);
				return result;
			}
		}

        /// <summary>
        /// Returns a collection of client Ids of the clients subscribed to receive this message.
        /// If the message has a subtopic header, the subtopics are used to filter the subscribers. 
        /// If there is no subtopic header, subscribers to the destination with no subtopic are used.
        /// Selector expressions if available will be evaluated to filter the subscribers.
        /// </summary>
        /// <param name="message">The message to send to subscribers.</param>
        /// <returns>Collection of subscribers.</returns>
        public ICollection GetSubscriber(IMessage message)
        {
            return GetSubscriber(message, true);
        }
        /// <summary>
        /// Returns a collection of client Ids of the clients subscribed to receive this message.
        /// If the message has a subtopic header, the subtopics are used to filter the subscribers. 
        /// If there is no subtopic header, subscribers to the destination with no subtopic are used. 
        /// If a subscription has a selector expression associated with it and evalSelector is true, 
        /// the subscriber is only returned if the selector expression evaluates to true.
        /// </summary>
        /// <param name="message">The message to send to subscribers.</param>
        /// <param name="evalSelector">Indicates whether evaluate selector expressions.</param>
        /// <returns>Collection of subscribers.</returns>
        /// <remarks>
        /// Use this method to do additional processing to the subscribers list.
        /// </remarks>
        public ICollection GetSubscriber(IMessage message, bool evalSelector)
        {
            MessageDestination destination = GetDestination(message) as MessageDestination;
            SubscriptionManager subscriptionManager = destination.SubscriptionManager;
            ICollection subscribers = subscriptionManager.GetSubscribers(message, evalSelector);
            return subscribers;
        }
        /// <summary>
        /// Pushes a message to all clients that are subscribed to the destination targeted by this message.
        /// </summary>
        /// <param name="message">The Message to push to the destination's subscribers.</param>
		public void PushMessageToClients(IMessage message)
		{
			MessageDestination destination = GetDestination(message) as MessageDestination;
			SubscriptionManager subscriptionManager = destination.SubscriptionManager;
			ICollection subscribers = subscriptionManager.GetSubscribers(message);
			if( subscribers != null && subscribers.Count > 0 )
			{
				PushMessageToClients(subscribers, message);

                //Asynchronous invocation sample:
                //BeginPushMessageToClients(new AsyncCallback(OnPushEnd), subscribers, message);
			}
		}

        /*
        void OnPushEnd(IAsyncResult result)
        {
            EndPushMessageToClients(result);
        }
        */

        /// <summary>
        /// Pushes a message to the specified clients (subscribers).
        /// </summary>
        /// <param name="subscribers">Collection of subscribers.</param>
        /// <param name="message">The Message to push to the subscribers.</param>
        /// <remarks>
        /// The Collection of subscribers is a collection of client Id strings.
        /// </remarks>
        public void PushMessageToClients(ICollection subscribers, IMessage message)
		{
			MessageDestination destination = GetDestination(message) as MessageDestination;
			SubscriptionManager subscriptionManager = destination.SubscriptionManager;
			if( subscribers != null && subscribers.Count > 0 )
			{
				IMessage messageClone = message.Copy() as IMessage;
                /*
				if( subscribers.Count > 1 )
				{
					messageClone.SetHeader(MessageBase.DestinationClientIdHeader, BinaryMessage.DestinationClientGuid);
					messageClone.clientId = BinaryMessage.DestinationClientGuid;
					//Cache the message
					MemoryStream ms = new MemoryStream();
					AMFSerializer amfSerializer = new AMFSerializer(ms);
					//TODO this should depend on endpoint settings 
					amfSerializer.UseLegacyCollection = false;
					amfSerializer.WriteData(ObjectEncoding.AMF3, messageClone);
					amfSerializer.Flush();
					byte[] cachedContent = ms.ToArray();
					ms.Close();
					BinaryMessage binaryMessage = new BinaryMessage();
					binaryMessage.body = cachedContent;
					//binaryMessage.Prepare();
					messageClone = binaryMessage;
				}
                */
				foreach(string clientId in subscribers)
				{
					MessageClient client = subscriptionManager.GetSubscriber(clientId);
					if( client == null )
						continue;
					if (log.IsDebugEnabled)
					{
						if( messageClone is BinaryMessage )
							log.Debug(__Res.GetString(__Res.MessageServicePushBinary, message.GetType().Name, clientId));
						else
                            log.Debug(__Res.GetString(__Res.MessageServicePush, message.GetType().Name, clientId));
					}

					IEndpoint endpoint = _messageBroker.GetEndpoint(client.EndpointId);
                    if (endpoint != null)
                        endpoint.Push(messageClone, client);
                    else
                    {
                        //We should never get here
                        if( log.IsErrorEnabled)
                            log.Error(string.Format("Missing endpoint for message client {0}", client.ClientId));
                    }
				}
			}
		}

        /// <summary>
        /// Begins an asynchronous operation to push a message to the specified clients (subscribers).
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="subscribers">Collection of subscribers.</param>
        /// <param name="message">The Message to push to the subscribers.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// The Collection of subscribers is a collection of client Id strings.
        /// </para>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginPushMessageToClients method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndPushMessageToClients method. When your application calls EndPushMessageToClients, the system will use a separate thread to execute the specified callback method, and will block on EndPushMessageToClients until the message is pushed successfully or throws an exception.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginPushMessageToClients(AsyncCallback asyncCallback, ICollection subscribers, IMessage message)
        {
            // Create IAsyncResult object identifying the asynchronous operation
            AsyncResultNoResult ar = new AsyncResultNoResult(asyncCallback, new PushData(FluorineContext.Current, subscribers, message));
            // Use a thread pool thread to perform the operation
            FluorineFx.Threading.ThreadPoolEx.Global.QueueUserWorkItem(new System.Threading.WaitCallback(OnBeginPushMessageToClients), ar);
            // Return the IAsyncResult to the caller
            return ar;
        }

        private void OnBeginPushMessageToClients(object asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            try
            {
                // Perform the operation; if sucessful set the result
                PushData pushData = ar.AsyncState as PushData;
                //Restore context
                FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, pushData.Context);
                PushMessageToClients(pushData.Subscribers, pushData.Message);
                ar.SetAsCompleted(null, false);
            }
            catch (Exception ex)
            {
                // If operation fails, set the exception
                ar.SetAsCompleted(ex, false);
            }
            finally
            {
                FluorineWebSafeCallContext.FreeNamedDataSlot(FluorineContext.FluorineContextKey);
            }
        }
        /// <summary>
        /// Ends a pending asynchronous message push.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <remarks>
        /// <para>
        /// EndInvoke is a blocking method that completes the asynchronous message push request started in the BeginPushMessageToClients method.
        /// </para>
        /// <para>
        /// Before calling BeginPushMessageToClients, you can create a callback method that implements the AsyncCallback delegate. This callback method executes in a separate thread and is called by the system after BeginPushMessageToClients returns. 
        /// The callback method must accept the IAsyncResult returned by the BeginPushMessageToClients method as a parameter.
        /// </para>
        /// <para>Within the callback method you can call the EndPushMessageToClients method to successfully complete the invocation attempt.</para>
        /// <para>The BeginPushMessageToClients enables to use the fire and forget pattern too (by not implementing an AsyncCallback delegate), however if the invocation fails the EndPushMessageToClients method is responsible to throw an appropriate exception.
        /// Implementing the callback and calling EndPushMessageToClients also allows early garbage collection of the internal objects used in the asynchronous call.</para>
        /// </remarks>
        public void EndPushMessageToClients(IAsyncResult asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }
	}

    #region PushData

#if !SILVERLIGHT
    class PushData
    {
        FluorineContext _context;
        ICollection _subscribers;
        IMessage _message;

        public FluorineContext Context
        {
            get { return _context; }
        }

        public ICollection Subscribers
        {
            get { return _subscribers; }
        }

        public IMessage Message
        {
            get { return _message; }
        }

        public PushData(FluorineContext context, ICollection subscribers, IMessage message)
        {
            _context = context;
            _subscribers = subscribers;
            _message = message;
        }
    }
#endif
    #endregion PushData
}
