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
using System.Web.Caching;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using log4net;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Util;
using FluorineFx.Messaging.Messages;
using FluorineFx.Exceptions;
using FluorineFx.Context;
using FluorineFx.Messaging.Endpoints;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// ClientManager manages clients connected to the FluorineFx server.
    /// </summary>
    /// <example>
    /// 	<code lang="CS">
    /// classChatAdapter : MessagingAdapter, ISessionListener
    /// {
    ///     private Hashtable _clients;
    ///  
    ///     public ChatAdapter()
    ///     {
    ///         _clients = new Hashtable();
    ///         ClientManager.AddSessionCreatedListener(this);
    ///     }
    ///  
    ///     public void SessionCreated(IClient client)
    ///     {
    ///         lock (_clients.SyncRoot)
    ///         {
    ///             _clients.Add(client.Id, client);
    ///         }
    ///         client.AddSessionDestroyedListener(this);
    ///     }
    ///  
    ///     public void SessionDestroyed(IClient client)
    ///     {
    ///         lock (_clients.SyncRoot)
    ///         {
    ///             _clients.Remove(client.Id);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
	[CLSCompliant(false)]
    public class ClientManager : IClientRegistry
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientManager));
        object _objLock = new object();

        MessageBroker _messageBroker;
        Hashtable _clients;

        private ClientManager()
        {
        }

        internal ClientManager(MessageBroker messageBroker)
		{
            _messageBroker = messageBroker;
            _clients = new Hashtable();
		}

        internal string GetNextId()
        {
            return Guid.NewGuid().ToString("D");
        }

        #region IClientRegistry Members

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _objLock; } }

        /// <summary>
        /// Returns an existing client from the message header transporting the global FlexClient Id value or creates a new one if not found.
        /// </summary>
        /// <param name="message">Message sent from client.</param>
        /// <returns>The client object.</returns>
        public IClient GetClient(IMessage message)
        {
            lock (this.SyncRoot)
            {
                IClient client = GetClient(message.GetFlexClientId());
                if (message is MessageBase)
                    (message as MessageBase).SetFlexClientId(client.Id);
                else
                    Debug.Assert(false);
                return client;
            }
        }
        /// <summary>
        /// Returns an existing client from a client id or creates a new one if not found.
        /// </summary>
        /// <param name="id">The identity of the client to return.</param>
        /// <returns>The client object.</returns>
        public IClient GetClient(string id)
        {
            lock (this.SyncRoot)
            {
                if (_clients.ContainsKey(id))
                {
                    HttpRuntime.Cache.Get(id);
                    return _clients[id] as Client;
                }
                if (id == null || id == "nil" || id == string.Empty)
                    id = Guid.NewGuid().ToString("D");
                Client client = new Client(this, id);
                _clients[id] = client;
                int clientLeaseTime = 1;
                log.Debug(__Res.GetString(__Res.Client_Create, id));
                Renew(client, clientLeaseTime);
                //client.NotifyCreated();
                return client;
            }
        }
        /// <summary>
        /// Check if a client with a given id exists.
        /// </summary>
        /// <param name="id">The identity of the client to check for.</param>
        /// <returns><c>true</c> if the client exists, <c>false</c> otherwise.</returns>
        public bool HasClient(string id)
        {
            if (id == null)
                return false;
            lock (this.SyncRoot)
            {
                return _clients.ContainsKey(id);
            }
        }
        /// <summary>
        /// Returns an existing client from a client id.
        /// </summary>
        /// <param name="clientId">The identity of the client to return.</param>
        /// <returns>The client object if exists, null otherwise.</returns>
        public IClient LookupClient(string clientId)
        {
            if (clientId == null)
                return null;

            lock (this.SyncRoot)
            {
                Client client = null;
                if (_clients.Contains(clientId))
                {
                    client = _clients[clientId] as Client;
                    HttpRuntime.Cache.Get(clientId);
                }
                return client;
            }
        }


        #endregion

        internal void Renew(Client client, int clientLeaseTime)
        {
            if (client.ClientLeaseTime == clientLeaseTime)
            {
                //Keep the client in the cache.
                HttpRuntime.Cache.Get(client.Id);
                return;
            }
            lock (this.SyncRoot)
            {
                if (client.ClientLeaseTime < clientLeaseTime)
                {
                    log.Debug(__Res.GetString(__Res.Client_Lease, client.Id, client.ClientLeaseTime, clientLeaseTime));
                    client.SetClientLeaseTime(clientLeaseTime);
                }
                if (clientLeaseTime == 0)
                {
                    log.Debug(__Res.GetString(__Res.Client_Lease, client.Id, client.ClientLeaseTime, clientLeaseTime));
                    client.SetClientLeaseTime(0);
                }
                if (client.ClientLeaseTime != 0)
                {
                    HttpRuntime.Cache.Remove(client.Id);
                    // Add the FlexClient to the Cache with the expiration item
                    HttpRuntime.Cache.Insert(client.Id, client, null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, client.ClientLeaseTime, 0),
                        CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(this.RemovedCallback));
                }
                else
                    HttpRuntime.Cache.Remove(client.Id);
            }
        }

        internal IClient RemoveSubscriber(IClient client)
        {
            lock (this.SyncRoot)
            {
                if (_clients.ContainsKey(client.Id))
                {
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.ClientManager_Remove, client.Id));
                    HttpRuntime.Cache.Remove(client.Id);
                    _clients.Remove(client.Id);
                }
                return client;
            }
        }

        /// <summary>
        /// Cancels the timeout.
        /// </summary>
        /// <param name="client">The client.</param>
        public void CancelTimeout(IClient client)
        {
            HttpRuntime.Cache.Remove(client.Id);
        }

        internal void RemovedCallback(string key, object value, CacheItemRemovedReason callbackReason)
        {
            if (callbackReason == CacheItemRemovedReason.Expired)
            {
                lock (this.SyncRoot)
                {
                    if (_clients.Contains(key))
                    {
                        try
                        {
                            IClient client = LookupClient(key);
                            if (client != null)
                            {
                                if (log.IsDebugEnabled)
                                    log.Debug(__Res.GetString(__Res.ClientManager_CacheExpired, client.Id));

                                _TimeoutContext context = new _TimeoutContext(client);
                                FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, context);
                                client.Timeout();
                                RemoveSubscriber(client);
                                FluorineWebSafeCallContext.FreeNamedDataSlot(FluorineContext.FluorineContextKey);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (log.IsErrorEnabled)
                                log.Error(__Res.GetString(__Res.ClientManager_CacheExpired, key), ex);
                        }
                    }
                }
            }
        }
    }
}
