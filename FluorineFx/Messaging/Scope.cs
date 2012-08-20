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
#if !(NET_1_1)
using System.Collections.Generic;
using System.Threading;
using FluorineFx.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Context;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Api.Statistics;

namespace FluorineFx.Messaging
{
	/// <summary>
    /// The scope object.
    /// 
    /// A statefull object shared between a group of clients connected to the same
    /// context path. Scopes are arranged in a hierarchical way, so its possible for
    /// a scope to have a parent. If a client is connect to a scope then they are
    /// also connected to its parent scope. The scope object is used to access
    /// resources, shared object, streams, etc.
    /// 
    /// The following are all names for scopes: application, room, place, lobby.
	/// </summary>
    [CLSCompliant(false)]
    public class Scope : BasicScope, IScope, IScopeStatistics
	{
#if !SILVERLIGHT
        static ILog log = LogManager.GetLogger(typeof(Scope));
#endif

		private const string ScopeType = "scope";
		public static string Separator = ":";

		private IScopeContext _context;
		private IScopeHandler _handler;
		private bool _autoStart = true;
		private bool _enabled = true;
		private bool _running;
        private readonly ServiceContainer _serviceContainer;
        /// <summary>
        /// Timestamp the scope was created.
        /// </summary>
        private readonly long _creationTime;

        /// <summary>
        /// String, IBasicScope
        /// </summary>
        //private Dictionary<string, IBasicScope> _children = new Dictionary<string, IBasicScope>();
        private readonly CopyOnWriteDictionary<string, IBasicScope> _children = new CopyOnWriteDictionary<string,IBasicScope>();
        /// <summary>
        /// IClient, CopyOnWriteArraySet(IConnection)
        /// </summary>
        private readonly CopyOnWriteDictionary<IClient, CopyOnWriteArraySet<IConnection>> _clients = new CopyOnWriteDictionary<IClient, CopyOnWriteArraySet<IConnection>>();

        /// <summary>
        /// Statistics about clients connected to the scope.
        /// </summary>
	    protected StatisticsCounter _clientStats = new StatisticsCounter();
        /// <summary>
        /// Statistics about connections to the scope.
        /// </summary>
	    protected StatisticsCounter _connectionStats = new StatisticsCounter();
        /// <summary>
        /// Statistics about sub-scopes.
        /// </summary>
	    protected StatisticsCounter _subscopeStats = new StatisticsCounter();	


        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> class.
        /// </summary>
        protected Scope():this(string.Empty)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> class.
        /// </summary>
        /// <param name="name">The scope name.</param>
        public Scope(string name)
            : base(null, ScopeType, name, false)
		{
            _serviceContainer = new ServiceContainer();
            _creationTime = Environment.TickCount;
        }

        internal ServiceContainer ServiceContainer
        {
            get { return _serviceContainer; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
		public bool IsEnabled
		{
			get{ return _enabled; }
			set{ _enabled = value; }
		}

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
		public bool IsRunning
		{
			get{ return _running; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether automatically start the scope.
        /// </summary>
        /// <value><c>true</c> if automatically start the scope; otherwise, <c>false</c>.</value>
		public bool AutoStart
		{
			get{ return _autoStart; }
			set{ _autoStart = value; }
		}

        /// <summary>
        /// Gets a value indicating whether this instance has context.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has context; otherwise, <c>false</c>.
        /// </value>
		public bool HasContext
		{
			get{ return _context != null; }
		}
        /// <summary>
        /// Gets the creation time.
        /// </summary>
        /// <value>The creation time.</value>
        public long CreationTime
        {
            get { return _creationTime; }
        }

        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public virtual IEndpoint Endpoint { get { return null; } }

        /// <summary>
        /// Inits this instance.
        /// </summary>
		public void Init()
		{
			if(HasParent) 
			{
				if(!Parent.HasChildScope(Name)) 
				{
					if(!Parent.AddChildScope(this)) 
					{
						return;
					}
				}
			}
			if(AutoStart) 
			{
				Start();
			}
		}

        /// <summary>
        /// Uninitialize scope and unregister from parent.
        /// </summary>
        public void Uninit()
        {
            foreach (IBasicScope child in _children.Values)
            {
                if (child is Scope)
                    ((Scope)child).Uninit();
            }
            Stop();
            if (HasParent)
            {
                if (Parent.HasChildScope(Name))
                {
                    Parent.RemoveChildScope(this);
                }
            }
        }

        /// <summary>
        /// Adds the specified service to the scope.
        /// </summary>
        /// <param name="serviceType">The type of service to add.</param>
        /// <param name="service">An instance of the service type to add.</param>
        public void AddService(Type serviceType, object service)
        {
            _serviceContainer.AddService(serviceType, service);
        }
        /// <summary>
        /// Adds the specified service to the scope.
        /// </summary>
        /// <param name="serviceType">The type of service to add.</param>
        /// <param name="service">An instance of the service type to add.</param>
        /// <param name="promote"></param>
        public void AddService(Type serviceType, object service, bool promote)
        {
            _serviceContainer.AddService(serviceType, service, promote);
        }
        /// <summary>
        /// Removes the specified service type from the service container.
        /// </summary>
        /// <param name="serviceType">The type of service to remove.</param>
        public void RemoveService(Type serviceType)
        {
            _serviceContainer.RemoveService(serviceType);
        }
        /// <summary>
        /// Removes the specified service type from the service container.
        /// </summary>
        /// <param name="serviceType">The type of service to remove.</param>
        /// <param name="promote"></param>
        public void RemoveService(Type serviceType, bool promote)
        {
            _serviceContainer.RemoveService(serviceType, promote);
        }
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType or a null reference (Nothing in Visual Basic) if there is no service object of type serviceType.</returns>
        public virtual object GetService(Type serviceType)
        {
            return _serviceContainer.GetService(serviceType);
        }


        /// <summary>
        /// Starts scope.
        /// </summary>
        /// <returns><code>true</code> if scope has handler and it's start method returned true, <code>false</code> otherwise.</returns>
        public bool Start() 
		{
            lock (SyncRoot)
            {
                bool result = false;
                if (IsEnabled && !IsRunning)
                {
                    if (HasHandler)
                    {
                        // Only start if scope handler allows it
                        try
                        {
                            // If we dont have a handler of our own dont try to start it
                            if (_handler != null)
                                result = _handler.Start(this);
                        }
                        catch (Exception ex)
                        {
#if !SILVERLIGHT
                            log.Error("Could not start scope " + this, ex);
#endif
                        }
                    }
                    else
                    {
                        // Always start scopes without handlers
#if !SILVERLIGHT
                        log.Debug(string.Format("Scope {0} has no handler, allowing start.", this));
#endif
                        result = true;
                    }
                    _running = result;
                }
                return result;
            }
		}
        /// <summary>
        /// Stops scope.
        /// </summary>
        public void Stop()
        {
            lock (SyncRoot)
            {
                if (IsEnabled && IsRunning && HasHandler)
                {
                    try
                    {
                        //If we dont have a handler of our own don't try to stop it
                        if (_handler != null)
                        {
                            _handler.Stop(this);
                        }
                    }
                    catch (Exception ex)
                    {
#if !SILVERLIGHT
                        log.Error("Could not stop scope " + this, ex);
#endif
                    }
                }
                _serviceContainer.Shutdown();
                _running = false;
            }
        }

        /// <summary>
        /// Free managed resources.
        /// </summary>
		protected override void Free()
		{
			if(HasParent) 
			{
				Parent.RemoveChildScope(this);
			}
			if(HasHandler) 
			{
				Handler.Stop(this);
			}
            // Kill all child scopes
            foreach(KeyValuePair<string, IBasicScope> entry in _children)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled) log.Debug(string.Format("Stopping child scope: {0}", entry.Key));
#endif
                IBasicScope basicScope = entry.Value;
                if (basicScope is Scope)
                    ((Scope)basicScope).Uninit();
            }
		}

		#region IScope Members

        /// <summary>
        /// Adds given connection to the scope.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <returns>
        /// true on success, false if the specified connection already belongs to this scope.
        /// </returns>
		public bool Connect(IConnection connection)
		{
			return Connect(connection, null);
		}

        /// <summary>
        /// Adds given connection to the scope.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="parameters">Parameters passed.</param>
        /// <returns>
        /// true on success, false if the specified connection already belongs to this scope.
        /// </returns>
		public bool Connect(IConnection connection, object[] parameters)
		{
			if(HasParent && !Parent.Connect(connection, parameters)) 
				return false;
			if(HasHandler && !Handler.Connect(connection, this, parameters))
				return false;
			IClient client = connection.Client;
            if (!connection.IsConnected)
            {
                // Timeout while connecting client
                return false;
            }
            //We would not get this far if there is no handler
            if (HasHandler && !Handler.Join(client, this))
            {
                return false;
            }
            if (!connection.IsConnected)
            {
                // Timeout while connecting client
                return false;
            }

            CopyOnWriteArraySet<IConnection> connections;
            if (_clients.ContainsKey(client))
                connections = _clients[client];
            else
            {
                connections = new CopyOnWriteArraySet<IConnection>();
                _clients[client] = connections;
            }
            connections.Add(connection);
            _clientStats.Increment();
            AddEventListener(connection);
            _connectionStats.Increment();
			return true;
		}

        /// <summary>
        /// Disconnects the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
		public void Disconnect(IConnection connection)
		{
			// We call the disconnect handlers in reverse order they were called during connection, i.e. roomDisconnect is called before appDisconnect.
			IClient client = connection.Client;
            if (client == null)
            {
                // Early bail out
                RemoveEventListener(connection);
                _connectionStats.Decrement();
                if(HasParent)
                    Parent.Disconnect(connection);
                return;
            }

			if(_clients.ContainsKey(client)) 
			{
                CopyOnWriteArraySet<IConnection> connections = _clients[client];
				connections.Remove(connection);
				IScopeHandler handler = null;
				if(HasHandler) 
				{
					handler = Handler;
					try 
					{
						handler.Disconnect(connection, this);
					} 
					catch(Exception ex)
					{
#if !SILVERLIGHT
                        if( log != null && log.IsErrorEnabled )
							log.Error("Error while executing \"disconnect\" for connection " + connection + " on handler " + handler, ex);
#endif
					}
				}

				if(connections.Count == 0)
				{
					_clients.Remove(client);
                    _clientStats.Decrement();
					if(handler != null) 
					{
						try 
						{
							// there may be a timeout here ?
							handler.Leave(client, this);
						} 
						catch (Exception ex)
						{
#if !SILVERLIGHT
                            if( log != null && log.IsErrorEnabled )
								log.Error("Error while executing \"leave\" for client " + client + " on handler " + handler, ex);
#endif
						}
					}
				}
				RemoveEventListener(connection);
                _connectionStats.Decrement();
			}
			if(HasParent)
				Parent.Disconnect(connection);
		}

        /// <summary>
        /// Returns scope context.
        /// </summary>
        /// <value>The scope context object.</value>
		public IScopeContext Context
		{
			get
			{
			    if(!HasContext && HasParent) 
					return Parent.Context;
			    return _context;
			}
            set
			{
				_context = value;
			}
		}

        /// <summary>
        /// Check to see if this scope has a child scope matching a given name.
        /// </summary>
        /// <param name="name">The name of the child scope.</param>
        /// <returns>
        /// true if a child scope exists, otherwise false.
        /// </returns>
		public bool HasChildScope(string name)
		{
            // Synchronize removal and retrieval of child scopes
            Monitor.Enter(SyncRoot);
            try
            {
                return _children.ContainsKey(ScopeType + Separator + name);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
		}

        /// <summary>
        /// Checks whether scope has a child scope with given name and type.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <param name="name">Child scope name.</param>
        /// <returns>
        /// true if a child scope exists, otherwise false.
        /// </returns>
		public bool HasChildScope(string type, string name)
		{
            // Synchronize removal and retrieval of child scopes
            Monitor.Enter(SyncRoot);
            try
            {

                return _children.ContainsKey(type + Separator + name);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
		}

        /// <summary>
        /// Creates child scope with name given and returns success value.
        /// </summary>
        /// <param name="name">New child scope name.</param>
        /// <returns>
        /// true if child scope was successfully creates, false otherwise.
        /// </returns>
		public bool CreateChildScope(string name)
		{
			Scope scope = new Scope(name);
			scope.Parent = this;
			return AddChildScope(scope);
		}

        /// <summary>
        /// Adds scope as a child scope.
        /// </summary>
        /// <param name="scope">Add the specified scope.</param>
        /// <returns>
        /// true if child scope was successfully added, false otherwise.
        /// </returns>
		public bool AddChildScope(IBasicScope scope)
		{
            if (HasChildScope(scope.Type, scope.Name))
            {
#if !SILVERLIGHT
                log.Warn(string.Format("Child scope {0} already exists", scope.Name));
#endif
                return false;
            }

			if(HasHandler && !Handler.AddChildScope(scope)) 
			{
#if !SILVERLIGHT
                if( log != null && log.IsDebugEnabled )
					log.Debug("Failed to add child scope: " + scope + " to " + this);
#endif
				return false;
			}
			if(scope is IScope) 
			{
				// Start the scope
				if(HasHandler && !Handler.Start((IScope) scope)) 
				{
#if !SILVERLIGHT
                    if( log != null && log.IsDebugEnabled )
						log.Debug("Failed to start child scope: " + scope + " in " + this);
#endif
					return false;
				}
			}
#if !SILVERLIGHT
            if (scope is Scope)
            {
                //Chain service containers
                (scope as Scope).ServiceContainer.Container = ServiceContainer;
            }
            if( log != null && log.IsDebugEnabled )
				log.Debug("Add child scope: " + scope + " to " + this);
#endif
			_children[scope.Type + Separator + scope.Name] = scope;
            _subscopeStats.Increment();
			return true;
		}

        /// <summary>
        /// Removes scope from the children scope list.
        /// </summary>
        /// <param name="scope">Removes the specified scope.</param>
		public void RemoveChildScope(IBasicScope scope)
		{
            //Synchronize retrieval of the child scope (with removal)
            Monitor.Enter(SyncRoot);
            try
            {
			// Don't remove if reference if we have another one
			    if (HasChildScope(scope.Name) && GetScope(scope.Name) != scope) 
                {
#if !SILVERLIGHT
				    log.Warn(string.Format("Being asked to remove wrong scope reference child scope is {0} not {1}", GetScope(scope.Name), scope));
#endif
                    return;
                }
#if !SILVERLIGHT
                if( log != null && log.IsDebugEnabled )
					log.Debug(string.Format("Remove child scope: {0} path: {1}", scope, scope.Path));
#endif
                if (scope is IScope)
                {
                    if (HasHandler)
                        Handler.Stop((IScope)scope);
    			    _subscopeStats.Decrement();
                }
                string child = scope.Type + Separator + scope.Name;
                if (_children.ContainsKey(child))
                    _children.Remove(child);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }

			if(HasHandler)
			{
#if !SILVERLIGHT
                if( log != null && log.IsDebugEnabled )
					log.Debug("Remove child scope");
#endif
				Handler.RemoveChildScope(scope);
			}
            if (scope is Scope)
            {
                //Chain service containers
                (scope as Scope).ServiceContainer.Container = null;
            }
		}

        /// <summary>
        /// Gets the child scope names.
        /// </summary>
        /// <returns>Collection of child scope names.</returns>
		public ICollection<String> GetScopeNames()
		{
			return _children.Keys;
		}

        /// <summary>
        /// Returns an iterator of basic scope names.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <returns>An iterator of basic scope names.</returns>
        public IEnumerator GetBasicScopeNames(string type)
        {
            if (type == null) 
				return _children.Keys.GetEnumerator();
            return new PrefixFilteringStringEnumerator(_children.Keys, type + Separator);
        }

	    /// <summary>
        /// Gets a child scope by name.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <param name="name">Name of the child scope.</param>
        /// <returns>Scope object.</returns>
        public IBasicScope GetBasicScope(string type, string name)
        {
            string child = type + Separator + name;
            if (_children.ContainsKey(child))
                return _children[child];
            return null;
        }

        /// <summary>
        /// Returns scope by name.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>Scope with the specified name.</returns>
		public IScope GetScope(string name)
		{
            // Synchronize removal and retrieval of child scopes
            Monitor.Enter(SyncRoot);
            try
            {
                string child = ScopeType + Separator + name;
                if (_children.ContainsKey(child))
                    return _children[child] as IScope;
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
            return null;
        }

        /// <summary>
        /// Gets a set of connected clients.
        /// </summary>
        /// <returns>Collection of connected clients.</returns>
		public ICollection<IClient> GetClients()
		{
			return _clients.Keys;
		}
        /// <summary>
        /// Gets a value indicating whether this instance has handler.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has context; otherwise, <c>false</c>.
        /// </value>
		public bool HasHandler
		{
			get
			{
				return (_handler != null || (HasParent && Parent.HasHandler));
			}
		}
        /// <summary>
        /// Gets or sets handler of the scope.
        /// </summary>
        /// <value>Scope handler object.</value>
		public IScopeHandler Handler
		{
			get
			{
			    if (_handler != null)
			        return _handler;
			    return HasParent ? Parent.Handler : null;
			}
            set
			{ 
				_handler = value; 
				if (_handler is IScopeAware)
					(_handler as IScopeAware).SetScope(this);
			}
		}
        /// <summary>
        /// Gets context path.
        /// </summary>
        /// <value>The context path.</value>
		public virtual string ContextPath
		{
			get
			{
			    if (HasContext)
			        return string.Empty;
			    if (HasParent){
			        return Parent.ContextPath + "/" + Name;				
			    }
			    return null;
			}
		}

        /// <summary>
        /// Returns an enumerator that iterates through connections.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the connections.</returns>
        public IEnumerator<IConnection> GetConnections()
		{
            return new ConnectionIterator(this);
		}

		/// <summary>
		/// Returns scope context.
		/// </summary>
		/// <returns></returns>
		public IScopeContext GetContext()
		{
		    if(!HasContext && HasParent) 
				return Parent.Context;
		    return _context;
		}

	    /// <summary>
        /// Returns collection of connections for the specified client.
        /// </summary>
        /// <param name="client">The client object.</param>
        /// <returns>Collection of connections.</returns>
		public ICollection LookupConnections(IClient client)
	    {
	        if( _clients.ContainsKey(client) )
                return _clients[client];
	        return null;
	    }

	    /// <summary>
        /// Gets statistics information about the scope.
        /// </summary>
        /// <value>Scope statistics information.</value>
        public IScopeStatistics Statistics { get { return this; } }

		#endregion

		#region IBasicScope Members
        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through children scopes.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through children scopes.</returns>
        public override IEnumerator GetEnumerator()
        {
            return _children.Values.GetEnumerator();
        }

		#endregion


		sealed class PrefixFilteringStringEnumerator : IEnumerator
		{
			private readonly string _prefix;
			private int _index;
			private readonly string[] _enumerable;
			private string _currentElement;


			internal PrefixFilteringStringEnumerator(ICollection<string> enumerable, string prefix)
			{
				_prefix = prefix;
				_index = -1;
                _enumerable = new string[enumerable.Count];
				enumerable.CopyTo(_enumerable, 0);
			}

			#region IEnumerator Members

			public void Reset()
			{
				_currentElement = null;
				_index = -1;
			}

		    object IEnumerator.Current
			{
				get
				{
					if(_index == -1)
						throw new InvalidOperationException("Enum not started.");
					if(_index >= _enumerable.Length)
						throw new InvalidOperationException("Enumeration ended.");
					return _currentElement;
				}
			}

			public bool MoveNext()
			{
				while(_index < _enumerable.Length - 1)
				{
					_index++;

					string element = _enumerable[_index];
					if( element.StartsWith(_prefix) )
					{
						_currentElement = element;
						return true;
					}
				}
				_index = _enumerable.Length;
				return false;
			}

			#endregion
		}

        sealed class ConnectionIterator : IEnumerator<IConnection>
        {
            IEnumerator _connectionIterator;
            readonly IEnumerator<KeyValuePair<IClient, CopyOnWriteArraySet<IConnection>>> _setIterator;

            public ConnectionIterator(Scope scope)
            {
                _setIterator = scope._clients.GetEnumerator();
            }

            #region IEnumerator<IConnection> Members

            public IConnection Current
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return _connectionIterator.Current; }
            }

            public bool MoveNext()
            {
                if (_connectionIterator != null && _connectionIterator.MoveNext())
                {
                    // More connections for this client
                    return true;
                }
                if (!_setIterator.MoveNext())
                {
                    // No more clients
                    return false;
                }
                _connectionIterator = _setIterator.Current.Value.GetEnumerator();
                while (_connectionIterator != null)
                {
                    if (_connectionIterator.MoveNext())
                    {
                        // Found client with connections
                        return true;
                    }
                    if (!_setIterator.MoveNext())
                    {
                        // No more clients
                        return false;
                    }
                    // Advance to next client
                    _connectionIterator = _setIterator.Current.Value.GetEnumerator();
                }
                return false;                
            }

            public void Reset()
            {
                _connectionIterator = null;
                _setIterator.Reset();
            }

            #endregion
        }
        /// <summary>
        /// Returns an <see cref="FluorineFx.Context.IResource"/> handle for the specified path.
        /// </summary>
        /// <param name="path">The resource location.</param>
        /// <returns>An appropriate <see cref="FluorineFx.Context.IResource"/> handle.</returns>
        public IResource GetResource(string path)
        {
            if (HasContext)
                return _context.GetResource(path);
            return Context.GetResource(ContextPath + '/' + path);
        }


        #region IScopeStatistics Members


        public int TotalConnections
        {
            get { return _connectionStats.Total; }
        }

        public int MaxConnections
        {
            get { return _connectionStats.Max; }
        }

        public int ActiveConnections
        {
            get { return _connectionStats.Current; }
        }

        public int TotalClients
        {
            get { return _clientStats.Total; }
        }

        public int MaxClients
        {
            get { return _clientStats.Max; }
        }

        public int ActiveClients
        {
            get { return _clientStats.Current; }
        }

        public int TotalSubscopes
        {
            get { return _subscopeStats.Total; }
        }

        public int MaxSubscopes
        {
            get { return _subscopeStats.Max; }
        }

        public int ActiveSubscopes
        {
            get { return _subscopeStats.Current; }
        }

        #endregion
    }
}
