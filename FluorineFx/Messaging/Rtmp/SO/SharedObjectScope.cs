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
using System.Collections.Specialized;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using System.Threading;
using System.Reflection;
using log4net;
using FluorineFx.Configuration;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Collections.Generic;
using FluorineFx.Invocation;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Persistence;
using FluorineFx.Messaging.Api.SO;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.Persistence;

namespace FluorineFx.Messaging.Rtmp.SO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SharedObjectScope : BasicScope, ISharedObject
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(SharedObjectScope));

        /// <summary>
        /// Scoped shared object
        /// </summary>
		protected SharedObject _so;
        /// <summary>
        /// Server-side listeners (ISharedObjectListener)
        /// </summary>
        private CopyOnWriteArray _serverListeners = new CopyOnWriteArray();
        /// <summary>
        /// Event handlers (String, Object)
        /// </summary>
        private CopyOnWriteDictionary<string, object> _handlers = new CopyOnWriteDictionary<string, object>();
        /// <summary>
        /// Security handlers (ISharedObjectSecurity)
        /// </summary>
        private CopyOnWriteArray _securityHandlers = new CopyOnWriteArray();

        /// <summary>
        /// Creates shared object with given parent scope, name, persistence flag state and store object.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="persistent"></param>
        /// <param name="store"></param>
		public SharedObjectScope(IScope parent, string name, bool persistent, IPersistenceStore store):base(parent, SharedObjectService.ScopeType, name, persistent)
		{
            string path = parent.ContextPath;
            if (!path.StartsWith("/"))
                path = "/" + path;
			// Create shared object wrapper around the attributes
            _so = store.Load(name) as SharedObject;
			if(_so == null) 
			{
                _so = new SharedObject(_attributes, name, path, persistent, store);
				store.Save(_so);
			} 
			else 
			{
				_so.Name = name;
				_so.Path = parent.ContextPath;
				_so.Store = store;
			}
		}

		#region ISharedObject Members

		public int Version
		{
			get
			{
				return _so.Version;
			}
		}

		public bool IsPersistentObject
		{
			get
			{
				return _so.IsPersistentObject;
			}
		}

		public void SendMessage(string handler, IList arguments)
		{
            BeginUpdate();
            try
            {
                _so.SendMessage(handler, arguments);
            }
            finally
            {
                EndUpdate();
            }

			// Invoke method on registered handler
			string serviceName, serviceMethod;
			int dotPos = handler.LastIndexOf(".");
			if(dotPos != -1) 
			{
				serviceName = handler.Substring(0, dotPos);
				serviceMethod = handler.Substring(dotPos + 1);
			} 
			else 
			{
				serviceName = string.Empty;
				serviceMethod = handler;
			}

			object soHandler = GetServiceHandler(serviceName);
			if(soHandler == null && this.HasParent) 
			{
				// No custom handler, check for service defined in the scope's context
				IScopeContext context = this.Parent.Context;
				try 
				{
                    //Search for a handler only if there is a service name specified
                    if (serviceName != string.Empty)
                    {
                        // The type must have a name of "SharedObjectName.DottedServiceName"
                        soHandler = ObjectFactory.CreateInstance(_so.Name + "." + serviceName);
                    }
				} 
				catch(Exception) 
				{
					// No such type.
                    log.Debug(__Res.GetString(__Res.Type_InitError, _so.Name + "." + serviceName));
				}
			}

			if(soHandler != null) 
			{
				MethodInfo mi = MethodHandler.GetMethod(soHandler.GetType(), serviceMethod, arguments);
				if(mi != null) 
				{
					ParameterInfo[] parameterInfos = mi.GetParameters();
					object[] args = new object[parameterInfos.Length];
					arguments.CopyTo(args, 0);
					TypeHelper.NarrowValues( args, parameterInfos);
					try 
					{
						InvocationHandler invocationHandler = new InvocationHandler(mi);
						object result = invocationHandler.Invoke(soHandler, args);
					} 
					catch(Exception exception) 
					{
						log.Error(__Res.GetString(__Res.ServiceHandler_InvocationFailed, serviceMethod, handler), exception);
					}
				}
			}

			// Notify server listeners
			foreach(ISharedObjectListener listener in _serverListeners)
			{
				listener.OnSharedObjectSend(this, handler, arguments);
			}		
		}

        /// <summary>
        /// Indicates that the value of a property in the shared object has changed.
        /// In most cases, such as when the value of a property is a primitive type like String or Number, you can call SetAttribute() instead of calling SetDirty(). However, when the value of a property is an object that contains its own properties, call SetDirty() to indicate when a value within the object has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        public void SetDirty(string propertyName)
        {
            BeginUpdate();
            try
            {
                _so.SetDirty(propertyName);
            }
            finally
            {
                EndUpdate();
            }
        }

		public void BeginUpdate()
		{
			Monitor.Enter(this.SyncRoot);
			_so.BeginUpdate();
		}

		public void BeginUpdate(IEventListener source)
		{
            Monitor.Enter(this.SyncRoot);
			_so.BeginUpdate(source);
		}

		public void EndUpdate()
		{
			_so.EndUpdate();
            Monitor.Exit(this.SyncRoot);
		}

		public void AddSharedObjectListener(ISharedObjectListener listener)
		{
			_serverListeners.Add(listener);
		}

		public void RemoveSharedObjectListener(ISharedObjectListener listener)
		{
			_serverListeners.Remove(listener);
		}

		public bool IsLocked
		{
			get
			{
                bool result = Monitor.TryEnter(this.SyncRoot);
				if( result )
                    Monitor.Exit(this.SyncRoot);
				return result;
			}
		}

        public bool Clear()
        {
            bool success;
            BeginUpdate();
            try
            {
                success = _so.Clear();
            }
            finally
            {
                EndUpdate();
            }

            if (success)
            {
                foreach (ISharedObjectListener listener in _serverListeners)
                {
                    listener.OnSharedObjectClear(this);
                }
            }
            return success;
        }

		public void Close()
		{
            lock (this.SyncRoot)
            {
                _so.Close();
                _so = null;
            }
		}

		#endregion

		#region IBasicScope Members

		public override string Type
		{
			get
			{
				return _so.Type;
			}
		}

		#endregion

		#region IAttributeStore Members

#if !(NET_1_1)
        public override ICollection<string> GetAttributeNames()
#else
        public override ICollection GetAttributeNames()
#endif
        {
            lock (this.SyncRoot)
            {
                return _so.GetAttributeNames();
            }
		}

		public override bool SetAttribute(string name, object value)
		{
			bool success = false;
            BeginUpdate();
            try
            {
                success = _so.SetAttribute(name, value);
            }
            finally
            {
                EndUpdate();
            }

			if(success) 
			{
				foreach(ISharedObjectListener listener in _serverListeners)
				{
					listener.OnSharedObjectUpdate(this, name, value);
				}
			}
			return success;
		}

#if !(NET_1_1)
        public override void SetAttributes(IDictionary<string, object> values)
        {
            BeginUpdate();
            try
            {
                _so.SetAttributes(values);
            }
            finally
            {
                EndUpdate();
            }
            foreach (ISharedObjectListener listener in _serverListeners)
            {
                listener.OnSharedObjectUpdate(this, values);
            }
        }
#else
        public override void SetAttributes(IDictionary values)
		{
            BeginUpdate();
            try
			{
				_so.SetAttributes(values);
			}
            finally
            {
                EndUpdate();
            }
			foreach(ISharedObjectListener listener in _serverListeners)
			{
				listener.OnSharedObjectUpdate(this, values);
			}
		}
#endif

		public override void SetAttributes(IAttributeStore values)
		{
            BeginUpdate();
            try
			{
				_so.SetAttributes(values);
			}
            finally
            {
                EndUpdate();
            }
			foreach(ISharedObjectListener listener in _serverListeners)
			{
				listener.OnSharedObjectUpdate(this, values);
			}		
		}

		public override object GetAttribute(string name)
		{
            lock (this.SyncRoot)
            {
                return _so.GetAttribute(name);
            }
		}

		public override bool HasAttribute(string name)
		{
            lock (this.SyncRoot)
            {
                return _so.HasAttribute(name);
            }
		}

		public override bool RemoveAttribute(string name)
		{
			bool success = false;
            BeginUpdate();
            try
			{
				success = _so.RemoveAttribute(name);
			}
            finally
            {
                EndUpdate();
            }
			if(success) 
			{
				foreach(ISharedObjectListener listener in _serverListeners)
				{
					listener.OnSharedObjectDelete(this, name);
				}
			}
			return success;
		}

		public override void RemoveAttributes()
		{
            BeginUpdate();
            try
			{
				_so.RemoveAttributes();
			}
            finally
            {
                EndUpdate();
            }
			foreach(ISharedObjectListener listener in _serverListeners)
			{
				listener.OnSharedObjectClear(this);
			}
		}

		#endregion

		#region IEventDispatcher Members

        public override void DispatchEvent(IEvent evt)
        {
            if (evt.EventType != EventType.SHARED_OBJECT || !(evt is ISharedObjectMessage))
            {
                // Don't know how to handle this event.
                base.DispatchEvent(evt);
                return;
            }

            ISharedObjectMessage msg = (ISharedObjectMessage)evt;
            if (msg.HasSource)
                BeginUpdate(msg.Source);
            else
                BeginUpdate();

            try
            {
                foreach (ISharedObjectEvent sharedObjectEvent in msg.Events)
                {
                    switch (sharedObjectEvent.Type)
                    {
                        case SharedObjectEventType.SERVER_CONNECT:
                            if (!IsConnectionAllowed())
                            {
                                _so.ReturnError(StatusASO.SO_NO_READ_ACCESS);
                            }
                            else if (msg.HasSource)
                            {
                                IEventListener source = msg.Source;
                                if (source is RtmpConnection)
                                    (source as RtmpConnection).RegisterBasicScope(this);
                                else
                                    AddEventListener(source);

                            }
                            break;
                        case SharedObjectEventType.SERVER_DISCONNECT:
                            if (msg.HasSource)
                            {
                                IEventListener source = msg.Source;
                                if (source is RtmpConnection)
                                    (source as RtmpConnection).UnregisterBasicScope(this);
                                else
                                    RemoveEventListener(source);
                            }
                            break;
                        case SharedObjectEventType.SERVER_SET_ATTRIBUTE:
                            if (!IsWriteAllowed(sharedObjectEvent.Key, sharedObjectEvent.Value))
                            {
                                _so.ReturnAttributeValue(sharedObjectEvent.Key);
                                _so.ReturnError(StatusASO.SO_NO_WRITE_ACCESS);
                            }
                            else
                                SetAttribute(sharedObjectEvent.Key, sharedObjectEvent.Value);
                            break;
                        case SharedObjectEventType.SERVER_DELETE_ATTRIBUTE:
                            if (!IsDeleteAllowed(sharedObjectEvent.Key))
                            {
                                _so.ReturnAttributeValue(sharedObjectEvent.Key);
                                _so.ReturnError(StatusASO.SO_NO_WRITE_ACCESS);
                            }
                            else
                                RemoveAttribute(sharedObjectEvent.Key);
                            break;
                        case SharedObjectEventType.SERVER_SEND_MESSAGE:
                            // Ignore request silently if not allowed
                            if (IsSendAllowed(sharedObjectEvent.Key, sharedObjectEvent.Value as IList))
                                SendMessage(sharedObjectEvent.Key, sharedObjectEvent.Value as IList);
                            break;
                        default:
                            log.Warn("Unknown SO event: " + sharedObjectEvent.Type.ToString());
                            break;
                    }
                }
            }
            finally
            {
                EndUpdate();
            }
        }

		#endregion


		#region IEventObservable Members

		public override void AddEventListener(IEventListener listener)
		{
			base.AddEventListener(listener);
			_so.Register(listener);

			foreach(ISharedObjectListener sharedObjectListener in _serverListeners)
			{
				sharedObjectListener.OnSharedObjectConnect(this);
			}
		}

		public override void RemoveEventListener(IEventListener listener)
		{
			_so.Unregister(listener);
			base.RemoveEventListener(listener);
			if(!_so.IsPersistentObject && ( _so.Listeners == null || _so.Listeners.Count == 0)) 
			{
				this.Parent.RemoveChildScope(this);
			}
			foreach(ISharedObjectListener sharedObjectListener in _serverListeners)
			{
				sharedObjectListener.OnSharedObjectDisconnect(this);
			}
		}

		#endregion

		#region IPersistable Members

		public override bool IsPersistent
		{
			get
			{
				return _so.IsPersistent;
			}
			set
			{
				_so.IsPersistent = value;
			}
		}

		public override string Name
		{
			get
			{
				return _so.Name;
			}
			set
			{
				_so.Name = value;
			}
		}

		public override string Path
		{
			get
			{
				return _so.Path;
			}
			/*
			set
			{
				_so.Path = value;
			}
			*/
		}
		
		public void SetPath(string value)
		{
			_so.Path = value;
		}

		public override long LastModified
		{
			get
			{
				return _so.LastModified;
			}
		}

		public override IPersistenceStore Store
		{
			get
			{
				return _so.Store;
			}
			set
			{
				_so.Store = value;
			}
		}

		#endregion

        #region ISharedObjectHandlerProvider Members

        public void RegisterServiceHandler(object handler)
        {
            RegisterServiceHandler(string.Empty, handler);
        }

        public void UnregisterServiceHandler()
        {
            UnregisterServiceHandler(string.Empty);
        }

        #endregion ISharedObjectHandlerProvider Members

        #region IServiceHandlerProvider Members

        public void RegisterServiceHandler(string name, object handler)
        {
            if (name == null)
                name = string.Empty;
            _handlers.Add(name, handler);
        }

        public void UnregisterServiceHandler(string name)
        {
            if (name == null)
                name = string.Empty;
            if( _handlers.ContainsKey(name) )
                _handlers.Remove(name);
        }

        public object GetServiceHandler(string name)
        {
            if (name == null)
                name = string.Empty;
            return _handlers.ContainsKey(name) ? _handlers[name] : null;
        }

        public ICollection<String> GetServiceHandlerNames()
        {
            return new ReadOnlyCollection<string>(_handlers.Keys);
        }

        #endregion IServiceHandlerProvider Members

        #region ISharedObjectSecurityService Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        public void RegisterSharedObjectSecurity(ISharedObjectSecurity handler)
        {
            _securityHandlers.Add(handler);
        }

        public void UnregisterSharedObjectSecurity(ISharedObjectSecurity handler)
        {
            _securityHandlers.Remove(handler);
        }

        public IEnumerator GetSharedObjectSecurity()
        {
            return _securityHandlers.GetEnumerator();
        }

        #endregion ISharedObjectSecurityService Members

        /// <summary>
        /// Returns security handlers for this shared object or null if none are found.
        /// </summary>
        /// <returns>Collection of ISharedObjectSecurity objects.</returns>
        private IEnumerator GetSecurityHandlers() 
        {
            ISharedObjectSecurityService security = ScopeUtils.GetScopeService(this.Parent, typeof(ISharedObjectSecurityService)) as ISharedObjectSecurityService;
    	    if (security == null)
    		    return null;    	
    	    return security.GetSharedObjectSecurity();
        }

        /// <summary>
        /// Call handlers and check if connection to the existing SO is allowed.
        /// </summary>
        /// <returns></returns>
        protected bool IsConnectionAllowed()
        {
            // Check internal handlers first
            foreach (ISharedObjectSecurity handler in _securityHandlers)
            {
                if (!handler.IsConnectionAllowed(this))
                    return false;
            }
            // Check global SO handlers next
            IEnumerator handlers = GetSecurityHandlers();
            if (handlers == null)
                return true;

            while (handlers.MoveNext())
            {
                ISharedObjectSecurity handler = handlers.Current as ISharedObjectSecurity;
                if (!handler.IsConnectionAllowed(this))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Call handlers and check if writing to the SO is allowed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool IsWriteAllowed(string key, object value)
        {
            // Check internal handlers first
            foreach (ISharedObjectSecurity handler in _securityHandlers)
            {
                if (!handler.IsWriteAllowed(this, key, value))
                    return false;

            }
            // Check global SO handlers next
            IEnumerator handlers = GetSecurityHandlers();
            if (handlers == null)
                return true;
            while (handlers.MoveNext())
            {
                ISharedObjectSecurity handler = handlers.Current as ISharedObjectSecurity;
                if (!handler.IsWriteAllowed(this, key, value))
                    return false;

            }
            return true;
        }
        /// <summary>
        /// Call handlers and check if deleting a property from the SO is allowed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool IsDeleteAllowed(String key)
        {
            // Check internal handlers first
            foreach (ISharedObjectSecurity handler in _securityHandlers)
            {
                if (!handler.IsDeleteAllowed(this, key))
                    return false;

            }
            // Check global SO handlers next
            IEnumerator handlers = GetSecurityHandlers();
            if (handlers == null)
                return true;
            while (handlers.MoveNext())
            {
                ISharedObjectSecurity handler = handlers.Current as ISharedObjectSecurity;
                if (!handler.IsDeleteAllowed(this, key))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Call handlers and check if sending a message to the clients connected to the SO is allowed.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected bool IsSendAllowed(string message, IList arguments)
        {
            // Check internal handlers first
            foreach (ISharedObjectSecurity handler in _securityHandlers)
            {
                if (!handler.IsSendAllowed(this, message, arguments))
                    return false;

            }
            // Check global SO handlers next
            IEnumerator handlers = GetSecurityHandlers();
            if (handlers == null)
                return true;
            while (handlers.MoveNext())
            {
                ISharedObjectSecurity handler = handlers.Current as ISharedObjectSecurity;
                if (!handler.IsSendAllowed(this, message, arguments))
                    return false;

            }
            return true;
        }
	}
}
