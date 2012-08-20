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
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using log4net;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Context;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// Represents a Flex/Flash client application instance on the server.
    /// </summary>
    [CLSCompliant(false)]
    public class Client : AttributeStore, IClient
    {
        private const int FlexClientInvalidated = 10027;
        private const int EndpointPushHandlerAlreadyRegistered = 10033;

        private static readonly ILog log = LogManager.GetLogger(typeof(Client));
        private object _syncLock = new object();

        private string _id;
        private int _clientLeaseTime;
        ClientManager _clientManager;
        /// <summary>
        /// List of message clients.
        /// </summary>
        private CopyOnWriteArray _messageClients;
        /// <summary>
        /// Scopes keyed by connections.
        /// </summary>
        protected CopyOnWriteDictionary _connectionToScope = new CopyOnWriteDictionary();
        /// <summary>
        /// List of registered Client created listeners.
        /// </summary>
        private static CopyOnWriteArray _createdListeners = new CopyOnWriteArray();
        /// <summary>
        /// List of registered Client destroyed listeners.
        /// </summary>
        private CopyOnWriteArray _destroyedListeners;
        /// <summary>
        /// Associated Sessions that represent the connections the Client makes to the server.
        /// </summary>
        private CopyOnWriteArray _sessions = new CopyOnWriteArray();
        /// <summary>
        /// EndpointPushHandlers keyed by endpointId (string, IEndpointPushHandler).
        /// </summary>
        private CopyOnWriteDictionary _endpointPushHandlers;

        IPrincipal _principal;

        /// <summary>
        /// State bit field.
        /// 1 IsValid
        /// 2 IsInvalidating
        /// 4 IsPolling
        /// 8 IsNew
        /// 16 
        /// 32
        /// 64
        /// </summary>
        protected byte __fields;

        internal Client(ClientManager clientManager, string id)
        {
            _clientManager = clientManager;
            _id = id;
            _clientLeaseTime = 0;
            SetIsNew(true);
            SetIsValid(true);
        }

        /// <summary>
        /// Gets whether the client is disconnected.
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
        /// Gets whether the client is being disconnected.
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
        /// Gets whether the client is polling.
        /// </summary>
        public bool IsPolling
        {
            get { return (__fields & 4) == 4; }
        }

        internal void SetIsPolling(bool value)
        {
            __fields = (value) ? (byte)(__fields | 4) : (byte)(__fields & ~4);
        }

        /// <summary>
        /// Gets whether the client is newly instantiated.
        /// </summary>
        public bool IsNew
        {
            get { return (__fields & 8) == 8; }
        }

        private void SetIsNew(bool value)
        {
            __fields = (value) ? (byte)(__fields | 8) : (byte)(__fields & ~8);
        }

        /// <summary>
        /// Gets the MessageClients associated with this Client.
        /// </summary>
        public IList MessageClients
        {
            get
            {
                return GetMessageClients();
            }
        }

        internal IList GetMessageClients()
        {
            if (_messageClients == null)
            {
                lock (this.SyncRoot)
                {
                    if (_messageClients == null)
                        _messageClients = new CopyOnWriteArray();
                }
            }
            return _messageClients;
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        public void Register(IConnection connection)
        {
            _connectionToScope.Add(connection, connection.Scope);
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        public void Unregister(IConnection connection)
        {
            _connectionToScope.Remove(connection);
            if (_connectionToScope.Count == 0)
            {
                // This client is not connected to any scopes, remove from registry.
                //Invalidate();//through Sessions
            }
        }

        internal void SetClientLeaseTime(int value)
        {
            _clientLeaseTime = value;
        }

        internal void SetPrincipal(IPrincipal principal)
        {
            _principal = principal;
        }

        #region IClient Members

        /// <summary>
        /// Gets the client identity.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// This will be generated by the server if not passed upon connection from client-side Flex/Flash app.
        /// </remarks>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <value></value>
        public int ClientLeaseTime 
        {
            get { return _clientLeaseTime; }
        }
        /// <summary>
        /// Gets or sets security information for the client.
        /// </summary>
        /// <remarks>Available only when perClientAuthentication is in use.</remarks>
        public IPrincipal Principal
        {
            get { return _principal; }
            set 
            { 
                _principal = value;
                System.Threading.Thread.CurrentPrincipal = value;
            }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        /// <summary>
        /// Get a set of scopes the client is connected to.
        /// </summary>
        /// <value></value>
        public ICollection Scopes
        {
            get { return _connectionToScope.Values; }
        }

        /// <summary>
        /// Get a set of connections of a given scope.
        /// </summary>
        /// <value></value>
        public ICollection Connections
        {
            get { return _connectionToScope.Keys; }
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="messageClient"></param>
        public void RegisterMessageClient(IMessageClient messageClient)
        {
            if (!this.GetMessageClients().Contains(messageClient))
            {
                this.GetMessageClients().Add(messageClient);
                if (_endpointPushHandlers != null)
                {
                    IEndpointPushHandler handler = GetEndpointPushHandler(messageClient.EndpointId);
                    if (handler != null)
                        handler.RegisterMessageClient(messageClient);
                }
            }
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="messageClient"></param>
        public void UnregisterMessageClient(IMessageClient messageClient)
        {
            //This operation was possibly initiated by this client
            //if (messageClient.IsDisconnecting)
            //    return;
            if (this.MessageClients != null && this.MessageClients.Contains(messageClient))
            {
                this.MessageClients.Remove(messageClient);
                if (_endpointPushHandlers != null)
                {
                    IEndpointPushHandler handler = _endpointPushHandlers[messageClient.EndpointId] as IEndpointPushHandler;
                    if (handler != null)
                        handler.UnregisterMessageClient(messageClient);
                }
                /*
                if (this.MessageClients.Count == 0)
                {
                    Disconnect();
                }
                */
            }
        }

        /*
    internal void Disconnect(bool timeout)
    {
        lock (this.SyncRoot)
        {
            if (this.IsDisconnecting || this.IsDisconnected)
                return;
            try
            {
                SetIsDisconnecting(true);
                //restore context
                IConnection currentConnection = null;
                if (this.Connections != null && this.Connections.Count > 0)
                {
                    IEnumerator enumerator = this.Connections.GetEnumerator();
                    enumerator.MoveNext();
                    currentConnection = enumerator.Current as IConnection;
                }
                if (FluorineContext.Current == null)
                {
                    _TimeoutContext context = new _TimeoutContext(currentConnection, this);
                    //WebSafeCallContext.SetData(FluorineContext.FluorineContextKey, context);
                    try
                    {
                        // See if we're running in full trust
                        new SecurityPermission(PermissionState.Unrestricted).Demand();
                        WebSafeCallContext.SetData(FluorineContext.FluorineContextKey, context);
                    }
                    catch (SecurityException)
                    {
                        System.Web.HttpContext ctx = System.Web.HttpContext.Current;
                        if (ctx != null)
                            ctx.Items[FluorineContext.FluorineContextKey] = context;
                    }
                }
                _clientManager.RemoveSubscriber(this);
                // Unregister from all sessions.
                if (_sessions != null && _sessions.Count != 0)
                {
                    foreach (ISession session in _sessions)
                        UnregisterSession(session);
                }        
                //Invalidate associated MessageClient subscriptions.
                if (_messageClients != null)
                {
                    foreach (MessageClient messageClient in _messageClients)
                    {
                        messageClient.RemoveMessageClientDestroyedListener(this);
                        if (timeout)
                            messageClient.Timeout();
                        else
                            messageClient.Disconnect();
                    }
                    _messageClients.Clear();
                }
                //Notify destroy listeners.
                if (_destroyedListeners != null)
                {
                    foreach (IClientListener listener in _destroyedListeners)
                    {
                        listener.ClientDestroyed(this);
                    }
                }
                // Close any registered push handlers.
                if (_endpointPushHandlers != null && _endpointPushHandlers.Count != 0)
                {
                    foreach (IEndpointPushHandler handler in _endpointPushHandlers.Values)
                    {
                        handler.Close();
                    }
                    _endpointPushHandlers = null;
                }

                foreach (IConnection connection in this.Connections)
                {
                    if (timeout)
                        connection.Timeout();
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error(string.Format("Disconnect Client {0}", this.Id), ex);
            }
            finally
            {
                SetIsDisconnecting(false);
                SetIsDisconnected(true);
            }
        }
    }
        */

        /*
        public IMessage[] GetPendingMessages(int waitIntervalMillis)
        {
            ArrayList messages = new ArrayList();
            try
            {
                SetIsPolling(true);
                do
                {
                    _clientManager.LookupClient(this._id);//renew

                    if (_messageClients != null)
                    {
                        foreach (MessageClient messageClient in _messageClients)
                        {
                            messageClient.Renew();
                            messages.AddRange(messageClient.GetPendingMessages());
                        }
                    }
                    if (waitIntervalMillis == 0)
                    {
                        return messages.ToArray(typeof(IMessage)) as IMessage[];
                    }
                    if (messages.Count > 0)
                    {
                        return messages.ToArray(typeof(IMessage)) as IMessage[];
                    }
                    System.Threading.Thread.Sleep(500);
                    waitIntervalMillis -= 500;
                    if (waitIntervalMillis <= 0)
                        SetIsPolling(false);
                }
                while (this.IsPolling && !this.IsInvalidating && !this.IsValid);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error(string.Format("GetPendingMessages Client {0}", this.Id), ex);
            }
            finally
            {
                SetIsPolling(false);
            }
            return messages.ToArray(typeof(IMessage)) as IMessage[];
        }
        */

        /// <summary>
        /// Adds a client destroy listener that will be notified when the client is destroyed.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddClientDestroyedListener(IClientListener listener)
        {
            if (listener != null)
            {
                if (_destroyedListeners == null)
                {
                    lock (this.SyncRoot)
                    {
                        if (_destroyedListeners == null)
                            _destroyedListeners = new CopyOnWriteArray();
                    }
                }
                _destroyedListeners.AddIfAbsent(listener);
            }
        }
        /// <summary>
        /// Removes a client destroy listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveClientDestroyedListener(IClientListener listener)
        {
            if (listener != null)
            {
                if (_destroyedListeners != null)
                {
                    _destroyedListeners.Remove(listener);
                }
            }
        }
        /// <summary>
        /// Notification that a session was created.
        /// </summary>
        /// <param name="session">The session that was created.</param>
        public void SessionCreated(ISession session)
        {
            //NOP
        }
        /// <summary>
        /// Notification that a session is about to be destroyed.
        /// </summary>
        /// <param name="session">The session that will be destroyed.</param>
        public void SessionDestroyed(ISession session)
        {
            UnregisterSession(session);
        }
        /// <summary>
        /// Associates a Session with this Client.
        /// </summary>
        /// <param name="session">The Session to associate with this Client.</param>
        public void RegisterSession(ISession session)
        {
            if (_sessions.AddIfAbsent(session))
            {
                session.AddSessionDestroyedListener(this);
                session.RegisterClient(this);
            }
        }
        /// <summary>
        /// Disassociates a Session from this Client.
        /// </summary>
        /// <param name="session">The Session to disassociate from this Client.</param>
        public void UnregisterSession(ISession session)
        {
            if (_sessions.RemoveIfPresent(session))
            {
                session.RemoveSessionDestroyedListener(this);
                session.UnregisterClient(this);
                // Once all client sessions/connections terminate; shut down.
                if (_sessions.Count == 0)
                    Invalidate();
            }
        }
        /// <summary>
        /// Notification that a MessageClient instance was created.
        /// </summary>
        /// <param name="messageClient">The MessageClient that was created.</param>
        public void MessageClientCreated(IMessageClient messageClient)
        {
            //NOP
        }
        /// <summary>
        /// Notification that a MessageClient is about to be destroyed.
        /// </summary>
        /// <param name="messageClient">The MessageClient that will be destroyed.</param>
        public void MessageClientDestroyed(IMessageClient messageClient)
        {
            UnregisterMessageClient(messageClient);
        }

        /// <summary>
        /// Renews a lease.
        /// </summary>
        public void Renew()
        {
            _clientManager.LookupClient(_id);
        }
        /// <summary>
        /// Renews a lease.
        /// </summary>
        /// <param name="clientLeaseTime">The amount of time in minutes before client times out.</param>
        public void Renew(int clientLeaseTime)
        {
            _clientManager.Renew(this, clientLeaseTime);
        }

        /// <summary>
        /// Registers an IEndpointPushHandler for the specified endpoint to handle pushing messages.
        /// </summary>
        /// <param name="handler">The IEndpointPushHandler to register.</param>
        /// <param name="endpointId">The endpoint identity to register for.</param>
        public void RegisterEndpointPushHandler(IEndpointPushHandler handler, string endpointId)
        {
            if (_endpointPushHandlers == null)
            {
                lock (this.SyncRoot)
                {
                    if (_endpointPushHandlers == null)
                        _endpointPushHandlers = new CopyOnWriteDictionary(1);
                }
            }
            if (_endpointPushHandlers.ContainsKey(endpointId))
            {
                MessageException me = new MessageException();
                me.FaultCode = EndpointPushHandlerAlreadyRegistered.ToString();
                throw me;
            }
            _endpointPushHandlers.Add(endpointId, handler);
        }
        /// <summary>
        /// Unregisters an IEndpointPushHandler from the specified endpoint.
        /// </summary>
        /// <param name="handler">The IEndpointPushHandler to unregister.</param>
        /// <param name="endpointId">The endpoint identity to unregister from.</param>
        public void UnregisterEndpointPushHandler(IEndpointPushHandler handler, string endpointId)
        {
            lock (this.SyncRoot)
            {
                if (_endpointPushHandlers == null)
                    return;
                if (_endpointPushHandlers[endpointId] == handler)
                    _endpointPushHandlers.Remove(endpointId);
            }
        }

        /// <summary>
        /// Returns the push handler registered with the Client with the supplied endpoint id, or null if no push handler was registered with the Client
        /// </summary>
        /// <param name="endpointId">Endpoint identity.</param>
        /// <returns>The push handler registered with the Client with the supplied endpoint id, or null if no push handler was registered with the Client for that endpoint.</returns>
        public IEndpointPushHandler GetEndpointPushHandler(string endpointId)
        {
            lock (this.SyncRoot)
            {
                if (_endpointPushHandlers != null && _endpointPushHandlers.ContainsKey(endpointId))
                    return _endpointPushHandlers[endpointId] as IEndpointPushHandler;
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Adds a create listener that will be notified when new clients are created.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public static void AddClientCreatedListener(IClientListener listener)
        {
            if (listener != null)
                _createdListeners.AddIfAbsent(listener);
        }
        /// <summary>
        /// Removes a Client created listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveClientCreatedListener(IClientListener listener)
        {
            if (listener != null)
                _createdListeners.Remove(listener);
        }

        /// <summary>
        /// Notifies client listeners.
        /// </summary>
        public void NotifyCreated()
        {
            if (IsNew)
            {
                SetIsNew(false);
                if (_createdListeners.Count != 0)
                {
                    foreach (IClientListener listener in _createdListeners)
                        listener.ClientCreated(this);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Client " + _id.ToString();
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Timeout()
        {
            Invalidate();
        }

        /// <summary>
        /// Invalidates the client.
        /// </summary>
        public void Invalidate()
        {
            lock (this.SyncRoot)
            {
                if (!IsValid || IsInvalidating)
                    return; // Already shutting down.

                SetIsInvalidating(true);
            }
            _clientManager.RemoveSubscriber(this);

            // Unregister from all Sessions.
            if (_sessions != null && _sessions.Count != 0)
            {
                foreach (ISession session in _sessions)
                    UnregisterSession(session);
            }
            // Invalidate associated MessageClient subscriptions.
            if (_messageClients != null && _messageClients.Count != 0)
            {
                foreach (MessageClient messageClient in _messageClients)
                {
                    messageClient.RemoveMessageClientDestroyedListener(this);
                    messageClient.Invalidate();
                }
                _messageClients.Clear();
            }
            // Notify destroy listeners that we're shutting the FlexClient down.
            if (_destroyedListeners != null && _destroyedListeners.Count != 0)
            {
                foreach (IClientListener listener in _destroyedListeners)
                {
                    listener.ClientDestroyed(this);
                }
                _destroyedListeners.Clear();
            }
            // Close any registered push handlers.
            if (_endpointPushHandlers != null && _endpointPushHandlers.Count != 0)
            {
                foreach (IEndpointPushHandler handler in _endpointPushHandlers.Values)
                {
                    handler.Close();
                }
                _endpointPushHandlers = null;
            }
            lock (this.SyncRoot)
            {
                SetIsValid(false);
                SetIsInvalidating(false);
            }
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Client_Invalidated, _id));
        }
    }
}