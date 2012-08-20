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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using System.IO;
using System.Threading;
using FluorineFx;
using FluorineFx.IO;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Collections;
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
    class EndpointPushNotifier : IEndpointPushHandler, ISessionListener, IMessageClientListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EndpointPushNotifier));

        object _syncLock = new object();
        IEndpoint _endpoint;
        ISession _session;
        IClient _flexClient;
        string _id;
        IMessage[] _messages;

        /// <summary>
        /// List of MessageClient subscriptions using this endpoint push notifier.
        /// </summary>
        CopyOnWriteArray _messageClients = new CopyOnWriteArray();
        /// <summary>
        /// State bit field.
        /// 1 IsClosed
        /// 2 IsClosing
        /// 4
        /// 8 
        /// 16 
        /// 32 
        /// 64
        /// </summary>
        protected byte __fields;

        public EndpointPushNotifier(IEndpoint endpoint, IClient flexClient)
        {
            _id = Guid.NewGuid().ToString("D");
            _endpoint = endpoint;
            _flexClient = flexClient;
            _session = FluorineContext.Current.Session;
            if (_session != null)
                _session.AddSessionDestroyedListener(this);
            flexClient.RegisterEndpointPushHandler(this, endpoint.Id);
        }

        /// <summary>
        /// Gets whether the notifier is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return (__fields & 1) == 1; }
        }

        internal void SetIsClosed(bool value)
        {
            __fields = (value) ? (byte)(__fields | 1) : (byte)(__fields & ~1);
        }

        /// <summary>
        /// Gets whether the notifier is being closed.
        /// </summary>
        public bool IsClosing
        {
            get { return (__fields & 2) == 2; }
        }

        internal void SetIsClosing(bool value)
        {
            __fields = (value) ? (byte)(__fields | 2) : (byte)(__fields & ~2);
        }

        #region ISessionListener Members

        /// <summary>
        /// Notification that a session was created.
        /// </summary>
        /// <param name="session">The session that was created.</param>
        public void SessionCreated(ISession session)
        {
            //We are only concerned in the session's destruction.
            session.AddSessionDestroyedListener(this);
        }
        /// <summary>
        /// Notification that a session is about to be destroyed.
        /// </summary>
        /// <param name="session">The session that will be destroyed.</param>
        public void SessionDestroyed(ISession session)
        {
            if (log.IsInfoEnabled)
                log.Info("Endpoint with id '" + _endpoint.Id + "' is closing a streaming connection for the FlexClient with id '" + _flexClient.Id + "' since its associated session has been destroyed.");
            Close();
        }

        #endregion

        #region IEndpointPushHandler Members

        /// <summary>
        /// Gets the handler identity.
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the connection.
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        /// <summary>
        /// Closes the handler.
        /// </summary>
        public void Close()
        {
            lock (this.SyncRoot)
            {
                if (IsClosed || IsClosing)
                    return;
                SetIsClosing(true);
            }
            if (_session != null)
                _session.RemoveSessionDestroyedListener(this);
            _flexClient.UnregisterEndpointPushHandler(this, _endpoint.Id);

#if !(NET_1_1)
            List<IMessage> list = new List<IMessage>(1);
#else
            ArrayList list = new ArrayList(1);
#endif
            CommandMessage disconnect = new CommandMessage(CommandMessage.DisconnectOperation);
            PushMessage(disconnect);

            // Invalidate associated subscriptions. This doesn't attempt to notify the client.
            foreach (MessageClient messageClient in _messageClients)
                messageClient.Invalidate();

            lock (this.SyncRoot)
            {
                SetIsClosed(true);
                SetIsClosing(false);
            }
            lock (this.SyncRoot)
            {
                Monitor.PulseAll(this.SyncRoot);
            }
        }
        /// <summary>
        /// Pushes messages to the client.
        /// </summary>
        /// <param name="messages">The list of messages to push.</param>
        public void PushMessages(IMessage[] messages)
        {
            if (messages != null && messages.Length != 0)
            {
                lock (this.SyncRoot)
                {
                    // Push these straight on through; notify immediately.
                    if (_messages == null)
                        _messages = messages;
                    else
                    {
                        int destinationIndex = _messages.Length;
                        _messages = ArrayUtils.Resize(_messages, _messages.Length + messages.Length) as IMessage[];
                        Array.Copy(messages, 0, _messages, destinationIndex, messages.Length);
                    }

                    // If the notifier isn’t closing, notify; otherwise just add and the close will
                    // notify once it completes.
                    if (!IsClosing)
                      Monitor.PulseAll(this.SyncRoot);
                }
            }
        }
        /// <summary>
        /// Pushes a message to the client.
        /// </summary>
        /// <param name="message">The message to push.</param>
        public void PushMessage(IMessage message)
        {
            if (message != null)
            {
                lock (this.SyncRoot)
                {
                    // Push these straight on through; notify immediately.
                    if (_messages == null)
                    {
                        _messages = new IMessage[1];
                        _messages[0] = message;
                    }
                    else
                    {
                        _messages = ArrayUtils.Resize(_messages, _messages.Length + 1) as IMessage[];
                        _messages[_messages.Length - 1] = message;
                    }
                    // If the notifier isn’t closing, notify; otherwise just add and the close will
                    // notify once it completes.
                    if (!IsClosing)
                        Monitor.PulseAll(this.SyncRoot);
                }
            }
        }
        /// <summary>
        /// Gets pending messages.
        /// </summary>
        /// <returns>List of pending messages.</returns>
        public IMessage[] GetPendingMessages()
        {
            lock (this.SyncRoot)
            {
                IMessage[] messages = _messages;
                _messages = null;
                return messages;
            }
        }
        /// <summary>
        /// Invoked to notify the handler that the MessageClient subscription is using this handler.
        /// </summary>
        /// <param name="messageClient">The MessageClient subscription using this handler.</param>
        public void RegisterMessageClient(IMessageClient messageClient)
        {
            if (messageClient != null)
            {
                if (!_messageClients.Contains(messageClient))
                {
                    _messageClients.Add(messageClient);
                    messageClient.AddMessageClientDestroyedListener(this);
                }
            }
        }
        /// <summary>
        /// Invoked to notify the handler that a MessageClient subscription that was using it has been invalidated.
        /// </summary>
        /// <param name="messageClient">The MessageClient subscription no longer using this handler</param>
        public void UnregisterMessageClient(IMessageClient messageClient)
        {
            if (messageClient != null)
            {
                messageClient.RemoveMessageClientDestroyedListener(this);
                _messageClients.Remove(messageClient);
            }
        }

        #endregion


        #region IMessageClientListener Members

        public void MessageClientCreated(IMessageClient messageClient)
        {
        }

        public void MessageClientDestroyed(IMessageClient messageClient)
        {
            UnregisterMessageClient(messageClient);
        }

        #endregion

    }
}
