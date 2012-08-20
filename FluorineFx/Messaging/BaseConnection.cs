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
using System.Net;
#if !(NET_1_1)
using System.Collections.Generic;
using FluorineFx.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Threading;
using FluorineFx.Util;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// Base abstract class for connections.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class BaseConnection : AttributeStore, IConnection
    {
#if !SILVERLIGHT
        private static ILog log = LogManager.GetLogger(typeof(BaseConnection));
#endif

        private FastReaderWriterLock _readerWriterLock;

        //object _syncLock = new object();
        /// <summary>
        /// Connection id.
        /// </summary>
        protected string _connectionId;
        /// <summary>
        /// AMF version.
        /// </summary>
        protected ObjectEncoding _objectEncoding;
        /// <summary>
        /// Path of scope client connected to.
        /// </summary>
        protected string _path;
        /// <summary>
        /// Number of read messages.
        /// </summary>
        protected AtomicLong _readMessages;
        /// <summary>
        /// Number of written messages.
        /// </summary>
        protected AtomicLong _writtenMessages;
        /// <summary>
        /// Number of dropped messages.
        /// </summary>
        protected AtomicLong _droppedMessages;
        /// <summary>
        /// Connection params passed from client with NetConnection.connect call.
        /// </summary>
        protected IDictionary _parameters;
        /// <summary>
        /// Client bound to connection.
        /// </summary>
        protected IClient _client;
        /// <summary>
        /// Session bound to connection.
        /// </summary>
        protected ISession _session;
        /// <summary>
        /// Scope that connection belongs to.
        /// </summary>
        private IScope _scope;
#if !(NET_1_1)
        /// <summary>
        /// Set of basic scopes.
        /// </summary>
        protected CopyOnWriteArraySet<IBasicScope> _basicScopes = new CopyOnWriteArraySet<IBasicScope>();
#else
        /// <summary>
        /// Set of basic scopes.
        /// </summary>
        protected CopyOnWriteArraySet _basicScopes = new CopyOnWriteArraySet();
#endif

        /// <summary>
        /// State bit field.
        /// 1 IsClosed
        /// 2 IsClosing
        /// 4 
        /// 8 IsFlexClient
        /// 16 IsTunnelingDetected
        /// 32 IsTunneled
        /// 64 RtmpServerConnection: IsDisconnecting
        /// 128 
        /// </summary>
        protected byte __fields;

        /// <summary>
        /// Initializes a new instance of the BaseConnection class.
        /// </summary>
        /// <param name="path">Scope path on server.</param>
        /// <param name="parameters">Parameters passed from client.</param>
        public BaseConnection(string path, IDictionary parameters)
            :this(path, Guid.NewGuid().ToString("N").Remove(12, 1), parameters)
        {
            //V4 GUID should be safe to remove the 4 so we can use the id for rtmpt
        }

        /// <summary>
        /// Initializes a new instance of the BaseConnection class.
        /// </summary>
        /// <param name="path">Scope path on server.</param>
        /// <param name="connectionId">Connection id.</param>
        /// <param name="parameters">Parameters passed from client.</param>
        internal BaseConnection(string path, string connectionId, IDictionary parameters)
        {
            _readerWriterLock = new FastReaderWriterLock();
            _readMessages = new AtomicLong();
            _writtenMessages = new AtomicLong();
            _droppedMessages = new AtomicLong();
            //V4 GUID should be safe to remove the 4 so we can use the id for rtmpt
            _connectionId = connectionId;
            _objectEncoding = ObjectEncoding.AMF0;
            _path = path;
            _parameters = parameters;
            SetIsClosed(false);
        }
        /// <summary>
        /// Gets the network endpoint.
        /// </summary>
        public abstract IPEndPoint RemoteEndPoint { get; }
        /// <summary>
        /// Gets the path for this connection. This is not updated if you switch scope.
        /// </summary>
        public string Path { get { return _path; } }

        /// <summary>
        /// Gets whether the connection is closed.
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
        /// Gets whether the connection is being closed.
        /// </summary>
        public bool IsClosing
        {
            get { return (__fields & 2) == 2; }
        }

        internal void SetIsClosing(bool value)
        {
            __fields = (value) ? (byte)(__fields | 2) : (byte)(__fields & ~2);
        }

        /// <summary>
        /// Gets whether the connected client is a Flex client (swf).
        /// </summary>
        public bool IsFlexClient
        {
            get { return (__fields & 8) == 8; }
        }

        internal void SetIsFlexClient(bool value)
        {
            __fields = (value) ? (byte)(__fields | 8) : (byte)(__fields & ~8);
        }

        /// <summary>
        /// Initializes client.
        /// </summary>
        /// <param name="client">Client bound to connection.</param>
        public void Initialize(IClient client)
        {
            try
            {
                _readerWriterLock.AcquireWriterLock();
                if (this.Client != null)
                {
                    // Unregister old client
                    this.Client.Unregister(this);
                }
                _client = client;
                // Register new client
                _client.Register(this);
            }
            finally
            {
                _readerWriterLock.ReleaseWriterLock();
            }
        }

        #region IConnection Members

        /// <summary>
        /// Connect to another scope on server.
        /// </summary>
        /// <param name="scope">New scope.</param>
        /// <returns>true on success, false otherwise.</returns>
        public bool Connect(IScope scope)
        {
            return Connect(scope, null);
        }
        /// <summary>
        /// Connect to another scope on server with given parameters.
        /// </summary>
        /// <param name="scope">New scope.</param>
        /// <param name="parameters">Parameters to connect with.</param>
        /// <returns>true on success, false otherwise.</returns>
        public virtual bool Connect(IScope scope, object[] parameters)
        {
            try
            {
                _readerWriterLock.AcquireWriterLock();
                IScope oldScope = _scope;
                _scope = scope;
                if (_scope.Connect(this, parameters))
                {
                    if (oldScope != null)
                    {
                        oldScope.Disconnect(this);
                    }
                    return true;
                }
                else
                {
                    _scope = oldScope;
                    return false;
                }
            }
            finally
            {
                _readerWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Checks whether connection is alive.
        /// </summary>
        public virtual bool IsConnected
        {
            get { return _scope != null; }
        }
        /// <summary>
        /// Closes connection.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                _readerWriterLock.AcquireWriterLock();

                if (IsClosed)
                    return;
                SetIsClosed(true);
            }
            finally
            {
                _readerWriterLock.ReleaseWriterLock();
            }
#if !SILVERLIGHT
            log.Debug("Close, disconnect from scope, and children");
#endif
            if (_basicScopes != null)
            {
                try
                {
                    //Close, disconnect from scope, and children
                    foreach (IBasicScope basicScope in _basicScopes)
                    {
                        UnregisterBasicScope(basicScope);
                    }
                }
                catch (Exception ex)
                {
#if !SILVERLIGHT
                    log.Error(__Res.GetString(__Res.Scope_UnregisterError), ex);
#endif
                }
            }
            try
            {
                _readerWriterLock.AcquireWriterLock();
                if (_scope != null)
                {
                    try
                    {
                        _scope.Disconnect(this);
                    }
                    catch (Exception ex)
                    {
#if !SILVERLIGHT
                        log.Error(__Res.GetString(__Res.Scope_DisconnectError, _scope), ex);
#endif
                    }
                }
                if (_client != null)
                {
                    _client.Unregister(this);
                    _client = null;
                }
                if (_session != null)
                {
                    _session.Invalidate();
                    _session = null;
                }
                _scope = null;
            }
            finally
            {
                _readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public virtual void Timeout()
        {
        }

        /*
        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public virtual int ClientLeaseTime { get { return 0; } }
        */
        /// <summary>
        /// Gets connection parameters.
        /// </summary>
        public IDictionary Parameters { get { return _parameters; } }
        /// <summary>
        /// Gets the client object associated with this connection.
        /// </summary>
        public IClient Client
        {
            get{ return _client; }
        }
        /// <summary>
        /// Gets the session object associated with this connection.
        /// </summary>
        public ISession Session
        {
            get { return _session; }
        }
        /// <summary>
        /// Get the scope this client is connected to.
        /// </summary>
        public IScope Scope
        {
            get{ return _scope; }
        }
        /// <summary>
        /// Gets the basic scopes this connection has subscribed.  This list will
        /// contain the shared objects and broadcast streams the connection connected to.
        /// </summary>
        public IEnumerator BasicScopes
        {
            get{ return _basicScopes.GetEnumerator(); }
        }
        /// <summary>
        /// Gets the connection id.
        /// </summary>
        public string ConnectionId { get { return _connectionId; } }
        /// <summary>
        /// Gets the session id.
        /// </summary>
        public string SessionId { get { return _connectionId; } }
        /// <summary>
        /// Gets the object encoding (AMF version) for this connection.
        /// </summary>
        public ObjectEncoding ObjectEncoding
        {
            get { return _objectEncoding; }
        }
        
        /// <summary>
        /// Gets an object that can be used to synchronize access to the connection.
        /// </summary>
        //public object SyncRoot { get { return _syncLock; } }

        internal FastReaderWriterLock ReaderWriterLock
        {
            get { return _readerWriterLock; }
        }

        /// <summary>
        /// Start measuring the roundtrip time for a packet on the connection.
        /// </summary>
        public virtual void Ping()
        {
        }

        #endregion

        #region IEventDispatcher Members

        /// <summary>
        /// Dispatches event.
        /// </summary>
        /// <param name="evt">Event.</param>
        public virtual void DispatchEvent(FluorineFx.Messaging.Api.Event.IEvent evt)
        {
        }

        #endregion

        #region IEventHandler Members

        /// <summary>
        /// Handles event
        /// </summary>
        /// <param name="evt">Event.</param>
        /// <returns>true if associated scope was able to handle event, false otherwise.</returns>
        public virtual bool HandleEvent(FluorineFx.Messaging.Api.Event.IEvent evt)
        {
            return this.Scope.HandleEvent(evt);
        }

        #endregion

        #region IEventListener Members

        /// <summary>
        /// Notified on event.
        /// </summary>
        /// <param name="evt">Event.</param>
        public virtual void NotifyEvent(FluorineFx.Messaging.Api.Event.IEvent evt)
        {
        }

        #endregion

        /// <summary>
        /// Registers basic scope.
        /// </summary>
        /// <param name="basicScope">Basic scope to register.</param>
        public void RegisterBasicScope(IBasicScope basicScope)
        {
            _basicScopes.Add(basicScope);
            basicScope.AddEventListener(this);
        }
        /// <summary>
        /// Unregister basic scope.
        /// </summary>
        /// <param name="basicScope">Unregister basic scope.</param>
        public void UnregisterBasicScope(IBasicScope basicScope)
        {
            _basicScopes.Remove(basicScope);
            basicScope.RemoveEventListener(this);
        }
        /// <summary>
        /// Gets the total number of bytes read from the connection.
        /// </summary>
        public abstract long ReadBytes { get; }
        /// <summary>
        /// Gets the total number of bytes written to the connection.
        /// </summary>
        public abstract long WrittenBytes { get; }
        /// <summary>
        /// Gets the total number of messages read from the connection.
        /// </summary>
        public long ReadMessages { get { return _readMessages.Value; } }
        /// <summary>
        /// Gets the total number of messages written to the connection.
        /// </summary>
        public long WrittenMessages { get { return _writtenMessages.Value; } }
        /// <summary>
        /// Gets the total number of messages that have been dropped.
        /// </summary>
        public long DroppedMessages { get { return _droppedMessages.Value; } }
        /// <summary>
        /// Gets the total number of messages that are pending to be sent to the connection.
        /// </summary>
        public virtual long PendingMessages { get { return 0; } }
        /// <summary>
        /// Get the total number of video messages that are pending to be sent to a stream.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <returns>Number of pending video messages.</returns>
        public virtual long GetPendingVideoMessages(int streamId)
        {
            return 0;
        }
        /// <summary>
        /// Gets the number of written bytes the client reports to have received.
        /// This is the last value of the BytesRead message received from a client.
        /// </summary>
        public virtual long ClientBytesRead { get { return 0; } }
        /// <summary>
        /// Gets roundtrip time of last ping command.
        /// </summary>
        public abstract int LastPingTime { get; }
    }
}
