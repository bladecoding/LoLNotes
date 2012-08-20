/*
	Fluorine Projector SWF2Exe open source library based on Flash Remoting
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
using System.Security.Principal;
using log4net;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Context
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class Session : ISession, IClientListener, IMessageClientListener
    {
		internal const string FxASPNET_SessionId = "__@ASP.NET_SessionId";

        private static readonly ILog log = LogManager.GetLogger(typeof(Session));

        private object _syncLock = new object();
        private string _id;
        private SessionManager _sessionManager;

        /// <summary>
        /// State bit field.
        /// 1 IsValid
        /// 2 IsInvalidating
        /// 4 IsNew
        /// 8 CanStream
        /// 16 
        /// 32
        /// 64
        /// </summary>
        protected byte __fields;

        private static CopyOnWriteArray _sessionCreatedListeners = new CopyOnWriteArray();
        private CopyOnWriteArray _sessionDestroyedListeners;
        /// <summary>
        /// The associated MessageClients.
        /// </summary>
        private CopyOnWriteArray _messageClients;
        /// <summary>
        /// The associated Clients.
        /// </summary>
        private CopyOnWriteArray _clients = new CopyOnWriteArray();
        /// <summary>
        /// Security information for this session.
        /// </summary>
        private IPrincipal _principal;
        /// <summary>
        /// Enforces session level streaming connection limits.
        /// </summary>
        private int _maxConnectionsPerSession = 1;
        /// <summary>
        /// Tracks open connections per session.
        /// </summary>
        private int _streamingConnectionsCount;

        internal Session(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
            _id = Guid.NewGuid().ToString("D");
            SetIsNew(true);
            SetIsValid(true);
            this.CanStream = true;
        }

        internal Session(SessionManager sessionManager, string id)
        {
            _sessionManager = sessionManager;
            _id = id;
            SetIsNew(true);
            SetIsValid(true);
            this.CanStream = true;
        }

        /// <summary>
        /// Gets whether the session is valid.
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
        /// Gets whether the session is being invalidated.
        /// </summary>
        public bool IsInvalidating
        {
            get { return (__fields & 2) == 2; }
        }

        internal void SetIsInvalidating(bool value)
        {
            __fields = (value) ? (byte)(__fields | 2) : (byte)(__fields & ~2);
        }

        internal void SetIsNew(bool value)
        {
            __fields = (value) ? (byte)(__fields | 4) : (byte)(__fields & ~4);
        }

        internal bool CanStream
        {
            get { return (__fields & 8) == 8; }
            set { __fields = (value) ? (byte)(__fields | 8) : (byte)(__fields & ~8); }
        }

        internal int MaxConnectionsPerSession
        {
            get { return _maxConnectionsPerSession; }
            set { _maxConnectionsPerSession = value; }
        }

        internal int StreamingConnectionsCount
        {
            get { return _streamingConnectionsCount; }
            set { _streamingConnectionsCount = value; }
        }

        /// <summary>
        /// Adds a session create listener that will be notified when the session is created.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public static void AddSessionCreatedListener(ISessionListener listener)
        {
            if (listener == null)
                return;
            _sessionCreatedListeners.AddIfAbsent(listener);
        }
        /// <summary>
        /// Removes a session create listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveSessionCreatedListener(ISessionListener listener)
        {
            if (listener == null)
                return;
            _sessionCreatedListeners.Remove(listener);
        }
        /// <summary>
        /// Adds a session destroy listener that will be notified when the session is destroyed.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddSessionDestroyedListener(ISessionListener listener)
        {
            if (listener != null)
            {
                if (_sessionDestroyedListeners == null)
                {
                    lock (this.SyncRoot)
                    {
                        if (_sessionDestroyedListeners == null)
                            _sessionDestroyedListeners = new CopyOnWriteArray();
                    }
                }
                _sessionDestroyedListeners.AddIfAbsent(listener);
            }
        }
        /// <summary>
        /// Removes a session destroy listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveSessionDestroyedListener(ISessionListener listener)
        {
            if (listener != null)
            {
                if (_sessionDestroyedListeners != null)
                    _sessionDestroyedListeners.Remove(listener);
            }
        }


        #region ISession Members

        /// <summary>
        /// Adds a new item to the session-state collection
        /// </summary>
        /// <param name="name">The name of the item to add to the session-state collection.</param>
        /// <param name="value">The value of the item to add to the session-state collection.</param>
        public virtual void Add(string name, object value)
        {
        }
        /// <summary>
        /// Removes all keys and values from the session-state collection. 
        /// </summary>
        public virtual void Clear()
        {
        }
        /// <summary>
        /// Deletes an item from the session-state collection.
        /// </summary>
        /// <param name="name">The name of the item to delete from the session-state collection.</param>
        /// <remarks>If the session-state collection does not contain an element with the specified name, the collection remains unchanged. No exception is thrown.</remarks>
        public virtual void Remove(string name)
        {
        }
        /// <summary>
        /// Removes all keys and values from the session-state collection. 
        /// </summary>
        public virtual void RemoveAll()
        {
        }
        /// <summary>
        /// Gets the unique identifier for the session. 
        /// </summary>
        public /*virtual*/ string Id
        {
            get
            {
                return _id;
            }
        }
        /// <summary>
        /// Gets whether this is a newly created session.
        /// </summary>
        public bool IsNew
        {
            get { return (__fields & 4) == 4; }
        }
        /// <summary>
        /// Gets or sets a session value by name.
        /// </summary>
        /// <param name="name">The key name of the session value.</param>
        /// <returns>The session-state value with the specified name.</returns>
        public abstract object this[string name]
        {
            get ; set ;
        }
        /// <summary>
        /// Invalidates session upon timeout.
        /// </summary>
        public void Timeout()
        {
            Invalidate();
        }
        /// <summary>
        /// Invalidates the Session.
        /// </summary>
        public virtual void Invalidate()
        {
            lock (this.SyncRoot)
            {
                if (!IsValid || IsInvalidating)
                    return; // Already shutting down.
                SetIsInvalidating(true);
            }
            _sessionManager.CancelTimeout(this);
            // Unregister all Clients.
            if (_clients != null && _clients.Count != 0)
            {
                foreach(IClient client in _clients )
                    UnregisterClient(client);
            }
            // Invalidate associated MessageClient subscriptions.
            if (_messageClients != null && _messageClients.Count != 0)
            {
                foreach (IMessageClient messageClient in _messageClients )
                {
                    messageClient.RemoveMessageClientDestroyedListener(this);
                    messageClient.Invalidate();
                }
                _messageClients.Clear();
            }
            // Notify sessionDestroyed listeners that the session is being invalidated.
            if (_sessionDestroyedListeners != null && _sessionDestroyedListeners.Count != 0)
            {
                foreach(ISessionListener listener in _sessionDestroyedListeners)
                {
                    listener.SessionDestroyed(this);
                }
                _sessionDestroyedListeners.Clear();
            }
            _principal = null;
            lock (this.SyncRoot)
            {
                SetIsValid(false);
                SetIsInvalidating(false);
            }
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Session_Invalidated, _id));
        }

        /// <summary>
        /// Pushes a message to a remote client.
        /// </summary>
        /// <param name="message">Message to push.</param>
        /// <param name="messageClient">The MessageClient subscription that this message targets.</param>
        public virtual void Push(IMessage message, IMessageClient messageClient)
        {
            throw new NotSupportedException(__Res.GetString(__Res.PushNotSupported));
        }

        #endregion

        #region ICollection Members
        /// <summary>
        /// Copies the elements of the Session to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Session. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index)
        {
        }
        /// <summary>
        /// Gets the number of elements contained in the Session.
        /// </summary>
        public abstract int Count
        {
            get ;
        }
        /// <summary>
        /// Gets a value indicating whether access to the Session is synchronized (thread safe).
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot
        {
            get { return _syncLock; }
        }
        /// <summary>
        /// Gets or sets security information for the session.
        /// </summary>
        /// <remarks>Available only when perClientAuthentication is not in use.</remarks>
        public virtual IPrincipal Principal 
        {
            get
            {
                return _principal;
            }
            set
            {
                _principal = value;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a Session.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the Session.</returns>
        public virtual IEnumerator GetEnumerator()
        {
            return null;
        }

        #endregion

        #region IClientListener Members

        /// <summary>
        /// Notification that a Client was created.
        /// </summary>
        /// <param name="client">The Client instance.</param>
        public void ClientCreated(IClient client)
        {
            //NOP
        }
        /// <summary>
        /// Notification that a Client was destroyed.
        /// </summary>
        /// <param name="client">The Client instance.</param>
        public void ClientDestroyed(IClient client)
        {
            UnregisterClient(client);
        }

        #endregion

        #region IMessageClientListener Members

        /// <summary>
        /// Notification that a MessageClient was created.
        /// </summary>
        /// <param name="messageClient">The MessageClient instance.</param>
        public void MessageClientCreated(IMessageClient messageClient)
        {
            //NOP
        }
        /// <summary>
        /// Notification that a MessageClient was destroyed.
        /// </summary>
        /// <param name="messageClient">The MessageClient instance.</param>
        public void MessageClientDestroyed(IMessageClient messageClient)
        {
            UnregisterMessageClient(messageClient);
        }

        #endregion

        /// <summary>
        /// Associates a Client with the Session.
        /// </summary>
        /// <param name="client">The Client to assocaite with the session.</param>
        public void RegisterClient(IClient client)
        {
            if (_clients.AddIfAbsent(client))
            {
                client.AddClientDestroyedListener(this);
                client.RegisterSession(this);
            }
        }
        /// <summary>
        /// Disassociates a Client from the Session.
        /// </summary>
        /// <param name="client">The Client to disassociate from the session.</param>
        public void UnregisterClient(IClient client)
        {
            if (_clients.RemoveIfPresent(client))
            {
                client.RemoveClientDestroyedListener(this);
                client.UnregisterSession(this);
            }
        }
        /// <summary>
        /// Associates a MessagClient (subscription) with the Session.
        /// </summary>
        /// <param name="messageClient">The MessageClient to associate with the session.</param>
        internal void RegisterMessageClient(IMessageClient messageClient)
        {
            if (_messageClients == null)
            {
                lock (this.SyncRoot)
                {
                    if (_messageClients == null)
                        _messageClients = new CopyOnWriteArray();
                }
            }

            if (_messageClients.AddIfAbsent(messageClient))
                messageClient.AddMessageClientDestroyedListener(this);
        }
        /// <summary>
        /// Disassociates a MessageClient (subscription) from a Session.
        /// </summary>
        /// <param name="messageClient">The MessageClient to disassociate from the session.</param>
        internal void UnregisterMessageClient(IMessageClient messageClient)
        {
            if (_messageClients != null)
            {
                _messageClients.Remove(messageClient);
                messageClient.RemoveMessageClientDestroyedListener(this);
            }
        }
        /// <summary>
        /// Notifies listeners that a new Session was created.
        /// </summary>
        public void NotifyCreated()
        {
            if (this.IsNew)
            {
                SetIsNew(false);
                foreach (ISessionListener listener in _sessionCreatedListeners)
                    listener.SessionCreated(this);
            }
        }
    }
}
