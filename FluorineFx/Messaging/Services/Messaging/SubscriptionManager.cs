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

//http://msdn2.microsoft.com/en-us/library/ms978504.aspx

using System;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Threading;

using log4net;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Util;
using FluorineFx.Messaging.Messages;
using FluorineFx.Exceptions;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Services.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SubscriptionManager
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(SubscriptionManager));

		MessageDestination	_messageDestination;
		object _objLock = new object();

		Hashtable			_subscribers;

		public SubscriptionManager(MessageDestination messageDestination)
		{
			_messageDestination = messageDestination;
			_subscribers = new Hashtable();
		}

		public MessageClient GetSubscriber(string clientId)
		{
			if( clientId == null )
				return null;
			
			lock(_objLock)
			{
				MessageClient messageClient = null;
				if( _subscribers.Contains(clientId) )
				{
					messageClient = _subscribers[clientId] as MessageClient;
					HttpRuntime.Cache.Get(clientId);
				}
				return messageClient;
			}
		}

        public MessageClient AddSubscriber(string clientId, string endpointId, Subtopic subtopic, Selector selector)
		{
			lock(_objLock)
			{
                if (subtopic != null)
                {
                    MessagingAdapter messagingAdapter = _messageDestination.ServiceAdapter as MessagingAdapter;
                    if (messagingAdapter != null)
                    {
                        if (!messagingAdapter.AllowSubscribe(subtopic))
                        {
                            ASObject aso = new ASObject();
                            aso["subtopic"] = subtopic.Value;
                            throw new MessageException(aso);
                        }
                    }
                }
                if (!_subscribers.Contains(clientId))
                {
                    //Set in RtmpService
                    MessageClient messageClient = new MessageClient(clientId, _messageDestination, endpointId);
                    messageClient.Subtopic = subtopic;
                    messageClient.Selector = selector;
                    messageClient.AddSubscription(selector, subtopic);
                    AddSubscriber(messageClient);
                    messageClient.NotifyCreatedListeners();
                    return messageClient;
                }
                else
                {
                    MessageClient messageClient = _subscribers[clientId] as MessageClient;
                    IClient client = FluorineContext.Current.Client;
                    if (client != null && !client.Id.Equals(messageClient.Client.Id))
                    {
                        throw new MessageException("Duplicate subscriptions from multiple Flex Clients");
                    }
                    //Reset the endpoint push state for the subscription to make sure its current because a resubscribe could be arriving over a new endpoint or a new session.
                    messageClient.ResetEndpoint(endpointId);
                    return messageClient;
                }
			}
		}

		private void AddSubscriber(MessageClient messageClient)
		{
            lock (_objLock)
            {
                if (!_subscribers.Contains(messageClient.ClientId))
                {
                    _subscribers[messageClient.ClientId] = messageClient;
                    // Add the MessageClient to the Cache with the expiration item
                    int timeOutMinutes = 20;
                    if (_messageDestination.DestinationDefinition.Properties.Network != null)
                        timeOutMinutes = _messageDestination.DestinationDefinition.Properties.Network.SessionTimeout;


                    HttpRuntime.Cache.Insert(messageClient.ClientId, messageClient, null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, timeOutMinutes, 0),
                        CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(this.RemovedCallback));

                    /*
                    HttpRuntime.Cache.Insert( messageClient.ClientId, messageClient, null, 
                        DateTime.Now.AddSeconds(timeOutMinutes),
                        TimeSpan.Zero,
                        CacheItemPriority.Default, new CacheItemRemovedCallback(this.RemovedCallback) );
                    */
                }
            }
		}

		public MessageClient RemoveSubscriber(MessageClient messageClient)
		{
			if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.SubscriptionManager_Remove, messageClient.ClientId));

			lock(_objLock)
			{
				RemoveSubscriber(messageClient.ClientId);
				return messageClient;
			}
		}

		public MessageClient RemoveSubscriber(string clientId)
		{
			lock(_objLock)
			{
				MessageClient messageClient = _subscribers[clientId] as MessageClient;
				HttpRuntime.Cache.Remove(clientId);
				_subscribers.Remove(clientId);
				return messageClient;
			}
		}

        public void CancelTimeout(MessageClient messageClient)
        {
            lock (_objLock)
            {
                HttpRuntime.Cache.Remove(messageClient.ClientId);
            }
        }

		public IList GetSubscribers()
		{
			lock(_objLock)
			{
				ArrayList subscribers = new ArrayList(_subscribers.Keys);
				return subscribers;
			}
		}

        public IList GetSubscribers(IMessage message)
        {
            return GetSubscribers(message, true);
        }

        public IList GetSubscribers(IMessage message, bool evalSelector)
		{
			lock(_objLock)
			{
				bool filter = true;
                if (message.headers == null || message.headers.Count == 0)
					filter = false;

				if( !filter )
					return GetSubscribers();
				else
				{
					Subtopic subtopic = null;
                    if (message.headers.ContainsKey(AsyncMessage.SubtopicHeader))
                    {
                        subtopic = new Subtopic(message.headers[AsyncMessage.SubtopicHeader] as string);
                        MessagingAdapter messagingAdapter = _messageDestination.ServiceAdapter as MessagingAdapter;
                        if (messagingAdapter != null)
                        {
                            if (!messagingAdapter.AllowSend(subtopic))
                                return null;
                        }
                    }
					
					ArrayList subscribers = new ArrayList();
					foreach(MessageClient messageClient in _subscribers.Values)
					{
						bool include = true;
						if( subtopic != null && messageClient.Subtopic != null )
							include = include && subtopic.Matches(messageClient.Subtopic);
                        if (messageClient.Selector != null && evalSelector)
							include = include && messageClient.Selector.Evaluate(null, message.headers);

						if(include)
							subscribers.Add(messageClient.ClientId);
					}
					return subscribers;
				}
			}
		}

		public void RemovedCallback(string key, object value, CacheItemRemovedReason callbackReason) 
		{
			if( callbackReason == CacheItemRemovedReason.Expired )
			{
				lock(_objLock)
				{
					if( _subscribers.Contains(key) )
					{
						try
						{
							MessageClient messageClient = GetSubscriber(key);
                            if (messageClient != null)
                            {
                                if (log.IsDebugEnabled)
                                    log.Debug(__Res.GetString(__Res.SubscriptionManager_CacheExpired, messageClient.ClientId));
                                if (_messageDestination != null)
                                {
                                    //MessageBroker messageBroker = _messageDestination.Service.GetMessageBroker();
                                    _TimeoutContext context = new _TimeoutContext(messageClient);
                                    FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, context);
                                    messageClient.Timeout();
                                    FluorineWebSafeCallContext.FreeNamedDataSlot(FluorineContext.FluorineContextKey);
                                }
                            }
						}
						catch(Exception ex)
						{
							if( log.IsErrorEnabled )
                                log.Error(__Res.GetString(__Res.SubscriptionManager_CacheExpired, string.Empty), ex);
						}
					}
				}
			}
		}
	}
}
