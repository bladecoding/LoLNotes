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
using System.Text;
using System.Collections;
using System.Diagnostics;
using log4net;
using FluorineFx.Util;
using FluorineFx.Context;
using FluorineFx.Collections;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Services.Messaging;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// Represents a client-side MessageAgent instance. 
    /// A server-side MessageClient is only created if its client-side counterpart has subscribed to a destination for pushed data (e.g. Consumer).
    /// </summary>
    /// <remarks>
    /// 	<para>Client-side Producers do not result in the creation of corresponding
    ///     server-side MessageClient instances.</para>
    /// 	<para></para>
    /// 	<para>Each MessageClient instance is bound to a client class (session) and when the
    ///     client is invalidated any associated MessageClient instances are invalidated as
    ///     well.</para>
    /// 	<para>MessageClient instances may also be timed out on a per-destination basis and
    ///     based on subscription inactivity. If no messages are pushed to the MessageClient
    ///     within the destination's subscription timeout period the MessageClient will be
    ///     shutdown.</para>
    /// 	<para>Per-destination subscription timeout should be used when inactive
    ///     subscriptions should be shut down opportunistically to preserve server
    ///     resources.</para>
    /// </remarks>
    [CLSCompliant(false)]
    public sealed class MessageClient : IMessageClient
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageClient));
        private object _syncLock = new object();

		string				_clientId;
		byte[]				_binaryId;

		MessageDestination	_messageDestination;
		string				_endpointId;
        IClient _client;
        Session _session;

		Subtopic			_subtopic;
		Selector			_selector;

        static CopyOnWriteDictionary _messageClientCreatedListeners = new CopyOnWriteDictionary();
        CopyOnWriteDictionary _messageClientDestroyedListeners;
        /// <summary>
        /// A set of all of the subscriptions managed by this message client.
        /// </summary>
        CopyOnWriteArraySet _subscriptions = new CopyOnWriteArraySet();

        /// <summary>
        /// State bit field.
        /// 1 IsValid
        /// 2 IsInvalidating
        /// 4 IsTimingOut
        /// 8 
        /// 16 
        /// 32
        /// 64
        /// </summary>
        byte __fields;

        private MessageClient()
        {
        }

        internal MessageClient(string clientId, MessageDestination destination, string endpointId)
            : this(clientId, destination, endpointId, true)
        {
        }

        internal MessageClient(string clientId, MessageDestination messageDestination, string endpointId, bool useSession)
		{
            SetIsValid(true);
            _clientId = clientId;
            Debug.Assert(messageDestination != null);
            _messageDestination = messageDestination;
            _endpointId = endpointId;
            if (useSession)
            {
                _session = FluorineContext.Current.Session as Session;
                if (_session != null)
                    _session.RegisterMessageClient(this);
                _client = FluorineContext.Current.Client;
                Debug.Assert(_client != null);
                _client.RegisterMessageClient(this);
            }
            else
            {
                _session = null;
                _client = null;
            }
            if (log.IsDebugEnabled)
            {
                string msg = string.Format("MessageClient created with clientId {0} for destination {1}", _clientId, _messageDestination.Id);
                log.Debug(msg);
            }
		}

        /// <summary>
        /// Gets whether the MessageClient is valid.
        /// </summary>
        public bool IsValid
        {
            get { return (__fields & 1) == 1; }
        }

        internal void SetIsValid(bool value)
        {
            __fields = (value) ? (byte)(__fields | 1) : (byte)(__fields & ~1);
        }

        /// <summary>
        /// Gets whether the MessageClient is being invalidated.
        /// </summary>
        public bool IsInvalidating
        {
            get { return (__fields & 2) == 2; }
        }

        internal void SetIsInvalidating(bool value)
        {
            __fields = (value) ? (byte)(__fields | 2) : (byte)(__fields & ~2);
        }
        /// <summary>
        /// Gets a value indicating whether the MessageClient has timed out.
        /// </summary>
        public bool IsTimingOut
        {
            get { return (__fields & 4) == 4; }
        }

        internal void SetIsTimingOut(bool value)
        {
            __fields = (value) ? (byte)(__fields | 4) : (byte)(__fields & ~4);
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

		internal MessageDestination Destination{ get{ return _messageDestination; } }

        /// <summary>
        /// Gets the destination identity the MessageClient is subscribed to.
        /// </summary>
        public string DestinationId { get { return _messageDestination.Id; } }

        //internal IMessageConnection MessageConnection { get { return _connection; } }
        
        /// <summary>
        /// Gets the endpoint identity the MessageClient is subscribed to.
        /// </summary>
        public string EndpointId { get { return _endpointId; } }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBinaryId()
		{
			if( _binaryId == null )
			{
				UTF8Encoding utf8Encoding = new UTF8Encoding();
				_binaryId = utf8Encoding.GetBytes(_clientId);
			}
			return _binaryId;
		}
        /// <summary>
        /// Gets the message client identity.
        /// </summary>
        /// <value>The message client identity.</value>
		public string ClientId
		{
			get
			{
				return _clientId;
			}
		}
        /// <summary>
        /// Gets the Client associated with this MessageClient.
        /// </summary>
        public IClient Client 
        {
            get { return _client; }
        }

        /// <summary>
        /// Gets the Session associated with this MessageClient.
        /// </summary>
        public ISession Session { get { return _session; } }
        
		internal Selector Selector
		{
			get{ return _selector; }
			set{ _selector = value; }
		}
        /// <summary>
        /// Gets the MessageClient subtopic.
        /// </summary>
        /// <value>The MessageClient subtopic.</value>
		public Subtopic Subtopic
		{
			get{ return _subtopic; }
			set{ _subtopic = value; }
		}

        /// <summary>
        /// Adds a MessageClient created listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public static void AddMessageClientCreatedListener(IMessageClientListener listener)
        {
            _messageClientCreatedListeners[listener] = null;
        }
        /// <summary>
        /// Removes a MessageClient created listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveMessageClientCreatedListener(IMessageClientListener listener)
        {
            if (_messageClientCreatedListeners.Contains(listener))
                _messageClientCreatedListeners.Remove(listener);
        }
        /// <summary>
        /// Adds a MessageClient destroy listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddMessageClientDestroyedListener(IMessageClientListener listener)
        {
            if (_messageClientDestroyedListeners == null)
            {
                lock (this.SyncRoot)
                {
                    if (_messageClientDestroyedListeners == null)
                        _messageClientDestroyedListeners = new CopyOnWriteDictionary(1);
                }
            }
            _messageClientDestroyedListeners[listener] = null;
        }
        /// <summary>
        /// Removes a MessageClient destroyed listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveMessageClientDestroyedListener(IMessageClientListener listener)
        {
            if (_messageClientDestroyedListeners != null)
            {
                if (_messageClientDestroyedListeners.Contains(listener))
                    _messageClientDestroyedListeners.Remove(listener);
            }
        }

        internal void NotifyCreatedListeners()
        {
            foreach (IMessageClientListener listener in _messageClientCreatedListeners.Keys)
                listener.MessageClientCreated(this);
        }

        internal void ResetEndpoint(string endpointId)
        {
            string oldEndpointId = null;
            Session oldSession = null;
            Session newSession = FluorineContext.Current.Session as Session;
            lock (this.SyncRoot)
            {
                // If anything is null, or nothing has changed, no need for a reset.
                if (_endpointId == null || endpointId == null || _session == null || newSession == null || (_endpointId.Equals(endpointId) && _session.Equals(newSession)))
                    return;

                oldEndpointId = _endpointId;
                _endpointId = endpointId;

                oldSession = _session;
                _session = newSession;
            }
            // Unregister in order to reset the proper push settings in the re-registration below once the session association has been patched.
            if (_client != null)
                _client.UnregisterMessageClient(this);
            // Clear out any reference to this subscription that the previously associated session has.
            if (oldSession != null)
                oldSession.UnregisterMessageClient(this);
            // Associate the current session with this subscription.
            if (_session != null)
                _session.RegisterMessageClient(this);
            // Reset proper push settings.
            if (_client != null)
                _client.RegisterMessageClient(this);

            if (log.IsDebugEnabled)
            {
                string msg = string.Format("MessageClient created with clientId {0} for destination {1} has been reset as a result of a resubscribe.", _clientId, _messageDestination.Id);
                if (oldEndpointId != null && !oldEndpointId.Equals(endpointId))
                    msg += " Endpoint changed from " + oldEndpointId + " to " + endpointId;
                if ((oldSession != null) && (newSession != null) && (oldSession != newSession))
                    msg += " Session changed from " + oldSession.Id + " to " + newSession.Id;
                log.Debug(msg);
            }    
        }

        /*
        //Rtmpconnection.Close -> Disconnect -> Unsubscribe
        internal void Disconnect()
		{
			if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.MessageClient_Disconnect, this.ClientId));
            lock (this.SyncRoot)
            {
                this.SetIsDisconnecting(true);
                Unsubscribe(false);
            }
		}

		/// <summary>
        /// Timeout -> Unsubscribe
        /// Client -> Unsubscribe
		/// </summary>
        internal void Unsubscribe()
		{
			if( log.IsDebugEnabled )
                log.Debug(__Res.GetString(__Res.MessageClient_Unsubscribe, this.ClientId));

            if (_messageClientDestroyedListeners != null)
            {
                foreach (IMessageClientListener listener in _messageClientDestroyedListeners.Keys)
                    listener.MessageClientDestroyed(this);
            }

            if (_messageDestination != null)
                _messageDestination.RemoveSubscriber(this.ClientId);
            _client.UnregisterMessageClient(this);
            _messageDestination = null;
        }

		internal void Timeout()
		{
			try
			{
                lock (this.SyncRoot)
                {
                    if (this.IsValid || _messageDestination == null)
                        return;
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.MessageClient_Timeout, this.ClientId));

                    //Timeout
                    CommandMessage commandMessage = new CommandMessage();
                    commandMessage.destination = this.Destination.Id;
                    commandMessage.clientId = this.ClientId;
                    //Indicate that the client's session with a remote destination has timed out
                    commandMessage.operation = CommandMessage.SessionInvalidateOperation;
                    commandMessage.headers[MessageBase.FlexClientIdHeader] = _client.Id;

                    MessageService messageService = _messageDestination.Service as MessageService;
                    object[] subscribers = new object[] { commandMessage.clientId };
                    messageService.PushMessageToClients(subscribers, commandMessage);
                    Unsubscribe(true);
                }
			}
			catch(Exception ex)
			{
				if( log.IsErrorEnabled )
					log.Error(__Res.GetString(__Res.MessageClient_Timeout, this.ClientId), ex);                
			}
		}

        private void Unsubscribe(bool timeout)
        {
            MessageService messageService = _messageDestination.Service as MessageService;
            CommandMessage commandMessageUnsubscribe = new CommandMessage();
            commandMessageUnsubscribe.destination = this.Destination.Id;
            commandMessageUnsubscribe.operation = CommandMessage.UnsubscribeOperation;
            commandMessageUnsubscribe.clientId = this.ClientId;
            if (timeout)
            {
                commandMessageUnsubscribe.headers[CommandMessage.SessionInvalidatedHeader] = true;
                commandMessageUnsubscribe.headers[CommandMessage.FluorineMessageClientTimeoutHeader] = true;
                commandMessageUnsubscribe.headers[MessageBase.FlexClientIdHeader] = _client.Id;
            }
            messageService.ServiceMessage(commandMessageUnsubscribe);
        }
        */

        /// <summary>
        /// Inform the object that it has timed out.
        /// </summary>
        public void Timeout()
        {
            Invalidate(true /* notify client */);
        }
        /// <summary>
        /// Invalidates the MessageClient.
        /// </summary>
        public void Invalidate()
        {
            Invalidate(false /* don't attempt to notify the client */);
        }
        /// <summary>
        /// Invalidates the MessageClient.
        /// </summary>
        /// <param name="notifyClient">Push a subscription invalidation message to the client.</param>
        public void Invalidate(bool notifyClient)
        {
            lock (this.SyncRoot)
            {
                if (!IsValid || IsInvalidating)
                    return; // Already shutting down.

                SetIsInvalidating(true);
                _messageDestination.SubscriptionManager.CancelTimeout(this);
            }

            // Build a subscription invalidation message and push to the client if it is still valid.
            if (notifyClient && _client != null && _client.IsValid)
            {
                CommandMessage commandMessage = new CommandMessage();
                commandMessage.destination= _messageDestination.Id;
                commandMessage.clientId = _clientId;
                commandMessage.operation = CommandMessage.SessionInvalidateOperation;

                MessageService messageService = _messageDestination.Service as MessageService;
                object[] subscribers = new object[] { commandMessage.clientId };
                try
                {
                    messageService.PushMessageToClients(subscribers, commandMessage);
                }
                catch (MessageException) 
                { }
            }

            // Notify listeners that we're being invalidated.
            if (_messageClientDestroyedListeners != null && _messageClientDestroyedListeners.Count != 0)
            {
                foreach (IMessageClientListener listener in _messageClientDestroyedListeners.Keys )
                {
                    listener.MessageClientDestroyed(this);
                }
                _messageClientDestroyedListeners.Clear();
            }

            // Generate unsubscribe messages for all of the MessageClient's subscriptions and 
            // route them to the destination this MessageClient is subscribed to.
            // Some adapters manage their own subscription state.
            ArrayList unsubscribeMessages = new ArrayList();
            lock (this.SyncRoot)
            {
                foreach(SubscriptionInfo subscription in _subscriptions)
                {
                    CommandMessage unsubscribeMessage = new CommandMessage();
                    unsubscribeMessage.destination = _messageDestination.Id;
                    unsubscribeMessage.clientId = _clientId;
                    unsubscribeMessage.operation = CommandMessage.UnsubscribeOperation;
                    unsubscribeMessage.SetHeader(CommandMessage.SessionInvalidatedHeader, true);
                    unsubscribeMessage.SetHeader(CommandMessage.SelectorHeader, subscription.Selector);
                    unsubscribeMessage.SetHeader(AsyncMessage.SubtopicHeader, subscription.Subtopic);
                    unsubscribeMessages.Add(unsubscribeMessage);
                }
            }
            // Release the lock and send the unsub messages.
            foreach (CommandMessage commandMessage in unsubscribeMessages)
            {
                try
                {
                    _messageDestination.Service.ServiceMessage(commandMessage);
                }
                catch (MessageException me)
                {
                    if (log.IsDebugEnabled)
                        log.Debug("MessageClient: " + _clientId + " issued an unsubscribe message during invalidation that was not processed but will continue with invalidation.", me);
                }
            }

            //TODO
            RemoveSubscription(this.Selector, this.Subtopic);

            lock (this.SyncRoot)
            {
                // If we didn't clean up all subscriptions log an error and continue with shutdown.
                int remainingSubscriptionCount = _subscriptions.Count;
                if (remainingSubscriptionCount > 0 && log.IsErrorEnabled)
                    log.Error("MessageClient: " + _clientId + " failed to remove " + remainingSubscriptionCount + " subscription(s) during invalidation");
            }

            _messageDestination.SubscriptionManager.RemoveSubscriber(this);

            lock (this.SyncRoot)
            {
                SetIsValid(false);
                SetIsInvalidating(false);
            }
        }  


        /// <summary>
        /// Renews a lease.
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Renew()
        {
            _messageDestination.SubscriptionManager.GetSubscriber(_clientId);
        }

        internal void AddSubscription(Selector selector, Subtopic subtopic)
        {
            _subscriptions.Add(new SubscriptionInfo(selector, subtopic));
        }

        internal void RemoveSubscription(Selector selector, Subtopic subtopic)
        {
            _subscriptions.Remove(new SubscriptionInfo(selector, subtopic));
        }

	}

    class SubscriptionInfo : IComparable
    {
        readonly Selector _selector;

        public Selector Selector
        {
            get { return _selector; }
        }

        readonly Subtopic _subtopic;

        public Subtopic Subtopic
        {
            get { return _subtopic; }
        }

        public SubscriptionInfo(Selector selector, Subtopic subtopic)
        {
            _selector = selector;
            _subtopic = subtopic;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is SubscriptionInfo)
            {
                SubscriptionInfo other = (SubscriptionInfo)obj;
                //return (string.Equals(other.Selector, _selector) && string.Equals(other.Subtopic, _subtopic)) ? 0 : -1;
                int result = CompareUtils.Compare(other.Selector, _selector) * -1;
                if (result == 0) 
                    result = CompareUtils.Compare(other.Subtopic, _subtopic);
                return result;
            }
            return -1;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
