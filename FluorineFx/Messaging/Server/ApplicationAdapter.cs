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
using FluorineFx.Collections;
using FluorineFx.Configuration;
using FluorineFx.IO;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.SO;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.IO;
using FluorineFx.Messaging.Server;
using FluorineFx.Exceptions;
using FluorineFx.Util;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Adapter
{
	/// <summary>
    /// <para>
	/// ApplicationAdapter class serves as a base class for your applications.
	/// It provides methods to work with SharedObjects and streams, as well as
	/// connections and scheduling services.
    /// </para>
    /// <para>
	/// ApplicationAdapter is an application level IScope. To handle streaming
	/// processes in your application you should implement 
	/// IStreamAwareScopeHandler interface and implement handling methods.
    /// </para>
    /// <para>
	/// Application adapter provides you with useful event handlers that can be used to intercept streams,
	/// authorize users, etc. Also, all methods added in subclasses can be called from client side with NetConnection.call
	/// method.
    /// </para>
    ///	If you want to build a server-side framework this is a place to start and wrap it around ApplicationAdapter subclass.
    /// </summary>
    /// <example>
    /// <para>Calling a method added to an ApplicationAdapter subclass</para>
    /// <code lang="Actionscript">
    /// var nc:NetConnection = new NetConnection();
    /// nc.connect(...);
    /// nc.call("serverHelloMsg", resultObject, "my message");
    /// </code>
    /// <code lang="CS">
    /// public class HelloWorldApplication : ApplicationAdapter
    /// {
    ///     public string serverHelloMsg(string helloStr)
    ///     {
    ///         return "Hello, " + helloStr + "!";
    ///     }
    /// }
    /// </code>
    /// </example>
	[CLSCompliant(false)]
    public class ApplicationAdapter : StatefulScopeWrappingAdapter, ISharedObjectService, ISharedObjectSecurityService, IStreamSecurityService
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationAdapter));

		/// <summary>
		/// List of IApplication listeners.
		/// </summary>
		private ArrayList _listeners = new ArrayList();
        /// <summary>
        /// List of handlers that protect shared objects.
        /// </summary>
        private CopyOnWriteArray _sharedObjectSecurityHandlers = new CopyOnWriteArray();
        /// <summary>
        /// List of handlers that protect stream publishing.
        /// </summary>
        private CopyOnWriteArray _publishSecurityHandlers = new CopyOnWriteArray();
        /// <summary>
        /// List of handlers that protect stream playback.
        /// </summary>
        private CopyOnWriteArray _playbackSecurityHandlers = new CopyOnWriteArray();

        /// <summary>
        /// Initializes a new instance of the ApplicationAdapter class.
        /// </summary>
		public ApplicationAdapter()
		{
		}

		/// <summary>
		/// Starts scope. Scope can be both application or room level.
		/// </summary>
		/// <param name="scope">Scope object.</param>
		/// <returns>true if scope can be started, false otherwise.</returns>
		public override bool Start(IScope scope) 
		{
			if(!base.Start(scope)) 
				return false;
			if(ScopeUtils.IsApplication(scope)) 
				return AppStart(scope);
			else if (ScopeUtils.IsRoom(scope)) 
				return RoomStart(scope);
			else 
				return false;
		}
		/// <summary>
		/// Stops scope handling (that is, stops application if given scope is app
		/// level scope and stops room handling if given scope has lower scope level).
		/// </summary>
		/// <param name="scope">Scope to stop.</param>
		public override void Stop(IScope scope) 
		{
			if(ScopeUtils.IsApplication(scope)) 
				AppStop(scope);
			else if (ScopeUtils.IsRoom(scope)) 
				RoomStop(scope);
			base.Stop(scope);
		}
		/// <summary>
		/// Adds client to scope. Scope can be both application or room. Can be
		/// applied to both application scope and scopes of lower level.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="scope">Scope object.</param>
        /// <returns>true to allow, false to deny join.</returns>
		public override bool Join(IClient client, IScope scope) 
		{
			if(!base.Join(client, scope)) 
				return false;
			if(ScopeUtils.IsApplication(scope)) 
				return AppJoin(client, scope);
			else if (ScopeUtils.IsRoom(scope)) 
				return RoomJoin(client, scope);
			else
				return false;
		}
		/// <summary>
		/// Disconnects client from scope. Can be applied to both application scope
		/// and scopes of lower level.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="scope">Scope object.</param>
		public override void Leave(IClient client, IScope scope) 
		{
			if (ScopeUtils.IsApplication(scope)) 
				AppLeave(client, scope);
			else if (ScopeUtils.IsRoom(scope)) 
				RoomLeave(client, scope);
			base.Leave(client, scope);
		}

		/// <summary>
		/// Register listener that will get notified about application events. Please
		/// note that return values (e.g. from IApplication.AppStart(IScope))
		/// will be ignored for listeners.
		/// </summary>
        /// <param name="listener">Application listener.</param>
		public void AddListener(IApplication listener) 
		{
			_listeners.Add(listener);
		}

		/// <summary>
		/// Unregister handler that will not get notified about application events
		/// any longer.
		/// </summary>
        /// <param name="listener">Application listener.</param>
		public void RemoveListener(IApplication listener) 
		{
			_listeners.Remove(listener);
		}
		/// <summary>
		/// Rejects the currently connecting client without a special error message.
		/// </summary>
		protected void RejectClient() 
		{
			throw new ClientRejectedException(null);
		}
        /// <summary>
        /// Rejects the currently connecting client with the specified reason.
        /// </summary>
        /// <param name="reason">Reason object.</param>
		protected void RejectClient(object reason) 
		{
			throw new ClientRejectedException(reason);
		}
        /// <summary>
        /// Returns connection result for given scope and parameters. 
        /// Whether the scope is room or application level scope, this method distinguishes it and acts accordingly.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="scope">Scope object.</param>
        /// <param name="parameters">List of params passed to connection handler.</param>
        /// <returns>true if connect is successful, false otherwise.</returns>
		public override bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
			if( !base.Connect (connection, scope, parameters) )
				return false;
			bool success = false;
			if(ScopeUtils.IsApplication(scope))
			{
				success = AppConnect(connection, parameters);
			}
			else if (ScopeUtils.IsRoom(scope)) 
			{
				success = RoomConnect(connection, parameters);
			}
			return success;
		}
        /// <summary>
        /// Returns disconnection result for given scope and parameters. 
        /// Whether the scope is room or application level scope, this method distinguishes it and acts accordingly.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="scope">true if disconnect is successful, false otherwise.</param>
		public override void Disconnect(IConnection connection, IScope scope) 
		{
			if (ScopeUtils.IsApplication(scope)) 
			{
				AppDisconnect(connection);
			} 
			else if (ScopeUtils.IsRoom(scope)) 
			{
				RoomDisconnect(connection);
			}
			base.Disconnect(connection, scope);
		}
        /// <summary>
        /// Handler method. Called every time new client connects (that is, new IConnection object is created after call from a SWF movie) to the application.
        /// 
        /// You override this method to pass additional data from client to server application using <code>NetConnection.connect</code> method.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="parameters">List of parameters after connection URL passed to <code>NetConnection.connect</code> method.</param>
        /// <returns>true if connect is successful, false otherwise.</returns>
        /// <example>
        /// <p><strong>Client-side:</strong><br />
        /// <code>NetConnection.connect("rtmp://localhost/app", "silver");</code></p>
        /// 
        /// <p><strong>Server-side:</strong><br />
        /// <code>if (parameters.Length > 0) Trace.WriteLine("Theme selected: " + parameters[0]);</code></p>
        /// </example>
		public virtual bool AppConnect(IConnection connection, Object[] parameters) 
		{
			log.Debug(__Res.GetString(__Res.AppAdapter_AppConnect, connection.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppConnect(connection, parameters);
			}
			return true;
		}
        /// <summary>
        /// Handler method. Called every time client disconnects from the application.
        /// </summary>
        /// <param name="connection">Disconnected connection object.</param>
		public virtual void AppDisconnect(IConnection connection) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_AppDisconnect, connection.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppDisconnect(connection);
			}
		}
        /// <summary>
        /// Handler method. Called every time new client connects (that is, new IConnection object is created after call from a SWF movie) to the application.
        /// 
        /// You override this method to pass additional data from client to server application using <c>NetConnection.connect</c> method.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="parameters">List of paramaters passed to room scope.</param>
        /// <returns>true if connect is successful, false otherwise.</returns>
		public virtual bool RoomConnect(IConnection connection, Object[] parameters) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomConnect, connection.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomConnect(connection, parameters);
			}
			return true;
		}
        /// <summary>
        /// Handler method. Called every time client disconnects from the application.
        /// </summary>
        /// <param name="connection">Disconnected connection object.</param>
		public virtual void RoomDisconnect(IConnection connection) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomDisconnect, connection.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomDisconnect(connection);
			}
		}
        /// <summary>
        /// Handler method. Called when an application scope is started.
        /// </summary>
        /// <param name="application">Application scope.</param>
        /// <returns>true if started successful, false otherwise.</returns>
		public virtual bool AppStart(IScope application) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_AppStart, application.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppStart(application);
			}
			return true;
		}
        /// <summary>
        /// Handler method. Called when a room scope is started.
        /// </summary>
        /// <param name="room">Room scope.</param>
        /// <returns>true if started successful, false otherwise.</returns>
		public virtual bool RoomStart(IScope room) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomStart, room.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomStart(room);
			}
			return true;
		}
        /// <summary>
        /// Handler method. Called when an application scope is stopped.
        /// </summary>
        /// <param name="application">Application scope.</param>
		public virtual void AppStop(IScope application) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_AppStop, application.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppStop(application);
			}
		}
        /// <summary>
        /// Handler method. Called when a room scope is stopped.
        /// </summary>
        /// <param name="room">Room scope.</param>
		public virtual void RoomStop(IScope room) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomStop, room.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomStop(room);
			}
		}
        /// <summary>
        /// Handler method. Called every time client joins application scope.
        /// </summary>
        /// <param name="client">Client object.</param>
        /// <param name="application">Application scope.</param>
        /// <returns>true if join is successful, false otherwise.</returns>
		public virtual bool AppJoin(IClient client, IScope application) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_AppJoin, client.ToString(), application.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppJoin(client, application);
			}
			return true;
		}
		/// <summary>
		/// Handler method. Called every time client leaves application scope.
		/// </summary>
		/// <param name="client">Client object that left.</param>
		/// <param name="application">Application scope.</param>
		public virtual void AppLeave(IClient client, IScope application) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_AppLeave, client.ToString(), application.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnAppLeave(client, application);
			}
		}
        /// <summary>
        /// Handler method. Called every time client joins room scope.
        /// </summary>
        /// <param name="client">Client object.</param>
        /// <param name="room">Room scope.</param>
        /// <returns>true if join is successful, false otherwise.</returns>
		public virtual bool RoomJoin(IClient client, IScope room) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomJoin, client.ToString(), room.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomJoin(client, room);
			}
			return true;
		}
		/// <summary>
		/// Handler method. Called every time client leaves room scope.
		/// </summary>
        /// <param name="client">Client object that left.</param>
		/// <param name="room">Room scope.</param>
		public virtual void RoomLeave(IClient client, IScope room) 
		{
            log.Debug(__Res.GetString(__Res.AppAdapter_RoomLeave, client.ToString(), room.ToString()));
			foreach(IApplication listener in _listeners) 
			{
				listener.OnRoomLeave(client, room);
			}
		}


		#region ISharedObjectService Members
        /// <summary>
        /// Returns a collection of available SharedObject names.
        /// </summary>
        /// <param name="scope">Scope that SharedObjects belong to.</param>
        /// <returns>Collection of available SharedObject names.</returns>
		public ICollection GetSharedObjectNames(IScope scope)
		{
			ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
			return service.GetSharedObjectNames(scope);
		}
        /// <summary>
        /// Creates a new shared object for given scope. Server-side shared objects
        /// (also known as Remote SO) are special kind of objects synchronized between clients.
        /// <para>
        /// To get an instance of RSO at client-side, use <c>SharedObject.getRemote()</c>.
        /// </para>
        /// SharedObjects can be persistent and transient. Persistent RSO are stateful, i.e. store their data between sessions.
        /// If you need to store some data on server while clients go back and forth use persistent SO, otherwise perfer usage of transient for
        /// extra performance.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <param name="persistent">Whether SharedObject instance should be persistent or not.</param>
        /// <returns>true if SO was created, false otherwise.</returns>
		public bool CreateSharedObject(IScope scope, string name, bool persistent)
		{
            ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
			return service.CreateSharedObject(scope, name, persistent);
		}
        /// <summary>
        /// Returns shared object from given scope by name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>Shared object instance with the specifed name.</returns>
		public ISharedObject GetSharedObject(IScope scope, string name)
		{
            ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
			return service.GetSharedObject(scope, name);
		}
        /// <summary>
        /// Returns shared object from given scope by name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <param name="persistent">Whether SharedObject instance should be persistent or not.</param>
        /// <returns>Shared object instance with the specifed name.</returns>
        public ISharedObject GetSharedObject(IScope scope, string name, bool persistent)
		{
            ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
			return service.GetSharedObject(scope, name, persistent);
		}
        /// <summary>
        /// Checks whether there is a SharedObject with given scope and name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>true if SharedObject exists, false otherwise.</returns>
		public bool HasSharedObject(IScope scope, string name)
		{
            ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
			return service.HasSharedObject(scope, name);
		}
        /// <summary>
        /// <para>
        /// Deletes persistent shared objects specified by name and clears all
        /// properties from active shared objects (persistent and nonpersistent). The
        /// name parameter specifies the name of a shared object, which can include a
        /// slash (/) as a delimiter between directories in the path. The last
        /// element in the path can contain wildcard patterns (for example, a
        /// question mark [?] and an asterisk [*]) or a shared object name. The
        /// ClearSharedObjects() method traverses the shared object hierarchy along
        /// the specified path and clears all the shared objects. Specifying a slash
        /// (/) clears all the shared objects associated with an application
        /// instance.
        /// </para>
        /// The following values are possible for the soPath parameter:
        /// <list type="table">
        /// <listheader>
        /// <term>soPath parameter</term>
        /// <description>action</description>
        /// </listheader>
        /// <item><term></term>
        /// <description>clears all local and persistent shared objects associated with the instance</description></item>
        /// <item><term>/foo/bar</term>
        /// <description>clears the shared object /foo/bar; if bar is a directory name, no shared objects are deleted</description></item>
        /// <item><term>/foo/bar/*</term>
        /// <description>clears all shared objects stored under the instance directory</description></item>
        /// <item><term>/foo/bar.</term>
        /// <description>the bar directory is also deleted if no persistent shared objects are in use within this namespace</description></item>
        /// <item><term>/foo/bar/XX??</term>
        /// <description>clears all shared objects that begin with XX, followed by any two characters</description></item>
        /// </list>
        /// If a directory name matches this specification, all the shared objects within this directory are cleared.
        /// 
        /// If you call the ClearSharedObjects() method and the specified path
        /// matches a shared object that is currently active, all its properties are
        /// deleted, and a "clear" event is sent to all subscribers of the shared
        /// object. If it is a persistent shared object, the persistent store is also cleared.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>true if successful, false otherwise.</returns>
		public bool ClearSharedObjects(IScope scope, string name)
		{
            ISharedObjectService service = (ISharedObjectService)ScopeUtils.GetScopeService(scope, typeof(ISharedObjectService));
            return service.ClearSharedObjects(scope, name);
        }
        /// <summary>
        /// Start service. 
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public void Start(ConfigurationSection configuration)
        {
        }
        /// <summary>
        /// Shutdown service.
        /// </summary>
        public void Shutdown()
        {
        }

		#endregion

        #region ISharedObjectSecurityService Members

        /// <summary>
        /// Adds handler that protects shared objects.
        /// </summary>
        /// <param name="handler">The ISharedObjectSecurity handler.</param>
        public void RegisterSharedObjectSecurity(ISharedObjectSecurity handler)
        {
            _sharedObjectSecurityHandlers.Add(handler);
        }
        /// <summary>
        /// Removes handler that protects shared objects.
        /// </summary>
        /// <param name="handler">The ISharedObjectSecurity handler.</param>
        public void UnregisterSharedObjectSecurity(ISharedObjectSecurity handler)
        {
            _sharedObjectSecurityHandlers.Remove(handler);
        }
        /// <summary>
        /// Gets the collection of security handlers that protect shared objects.
        /// </summary>
        /// <returns>Enumerator of ISharedObjectSecurity handlers.</returns>
        public IEnumerator GetSharedObjectSecurity()
        {
            return _sharedObjectSecurityHandlers.GetEnumerator();
        }

        #endregion

        #region IStreamSecurityService Members

        /// <summary>
        /// Add handler that protects stream publishing.
        /// </summary>
        /// <param name="handler">Handler to add.</param>
        public void RegisterStreamPublishSecurity(IStreamPublishSecurity handler)
        {
            _publishSecurityHandlers.Add(handler);
        }
        /// <summary>
        /// Remove handler that protects stream publishing.
        /// </summary>
        /// <param name="handler">Handler to remove.</param>
        public void UnregisterStreamPublishSecurity(IStreamPublishSecurity handler)
        {
            _publishSecurityHandlers.Remove(handler);
        }
        /// <summary>
        /// Returns handlers that protect stream publishing.
        /// </summary>
        /// <returns>Enumerator of handlers.</returns>
        public IEnumerator GetStreamPublishSecurity()
        {
            return _publishSecurityHandlers.GetEnumerator();
        }
        /// <summary>
        /// Add handler that protects stream playback.
        /// </summary>
        /// <param name="handler">Handler to add.</param>
        public void RegisterStreamPlaybackSecurity(IStreamPlaybackSecurity handler)
        {
            _playbackSecurityHandlers.Add(handler);
        }
        /// <summary>
        /// Remove handler that protects stream playback.
        /// </summary>
        /// <param name="handler">Handler to remove.</param>
        public void UnregisterStreamPlaybackSecurity(IStreamPlaybackSecurity handler)
        {
            _playbackSecurityHandlers.Remove(handler);
        }
        /// <summary>
        /// Returns handlers that protect stream plaback.
        /// </summary>
        /// <returns>Enumerator of handlers.</returns>
        public IEnumerator GetStreamPlaybackSecurity()
        {
            return _playbackSecurityHandlers.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Returns list of stream names broadcasted in scope. Broadcast stream name is somewhat different
        /// from server stream name. Server stream name is just an ID assigned to every created stream. Broadcast stream name
        /// is the name that is being used to subscribe to the stream at client side, that is, in <c>NetStream.play</c> call.
        /// </summary>
        /// <param name="scope">Scope to retrieve broadcasted stream names.</param>
        /// <returns>List of broadcasted stream names.</returns>
	    public IEnumerator GetBroadcastStreamNames(IScope scope) 
        {
            IProviderService service = ScopeUtils.GetScopeService(scope, typeof(IProviderService)) as IProviderService;
		    return service.GetBroadcastStreamNames(scope);
	    }

        /// <summary>
        /// Invoke clients with parameters and callback.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        protected void InvokeClients(string method, object[] arguments, IPendingServiceCallback callback)
        {
            InvokeClients(method, arguments, callback, false);
        }
        /// <summary>
        /// Invoke clients with parameters and callback.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <param name="ignoreSelf">Current client shoud be ignored.</param>
        protected void InvokeClients(string method, object[] arguments, IPendingServiceCallback callback, bool ignoreSelf)
        {
            InvokeClients(method, arguments, callback, ignoreSelf, this.Scope);
        }
        /// <summary>
        /// Invoke clients with parameters and callback.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <param name="ignoreSelf">Current client shoud be ignored.</param>
        /// <param name="targetScope">Invoke clients subscribed to the specified Scope.</param>
        protected void InvokeClients(string method, object[] arguments, IPendingServiceCallback callback, bool ignoreSelf, IScope targetScope)
        {
            try
            {
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("Invoke clients: {0}", method));
                IServiceCapableConnection connection = FluorineFx.Context.FluorineContext.Current.Connection as IServiceCapableConnection;
                IEnumerator collections = targetScope.GetConnections();
                while (collections.MoveNext())
                {
                    IConnection connectionTmp = collections.Current as IConnection;
                    if (!connectionTmp.Scope.Name.Equals(targetScope.Name))
                        continue;
                    if ((connectionTmp is IServiceCapableConnection) && (!ignoreSelf || connectionTmp != connection))
                        (connectionTmp as IServiceCapableConnection).Invoke(method, arguments, callback);
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("InvokeClients failed", ex);
                throw;
            }
            finally
            {
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("Finished invoking clients ({0})", method));
            }
        }


        /// <summary>
        /// Begins an asynchronous operation to invoke clients with parameters and callback.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// The <i>asyncCallback</i> delegate identifies the callback invoked when the messages are sent, the <i>callback</i> object identifies the callback handling client responses.
        /// </para>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvokeClients method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvokeClients method. When your application calls BeginInvokeClients, the system will use a separate thread to execute the specified callback method, and will block on EndInvokeClients until the clients are invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        protected IAsyncResult BeginInvokeClients(AsyncCallback asyncCallback, string method, object[] arguments, IPendingServiceCallback callback)
        {
            return BeginInvokeClients(asyncCallback, method, arguments, callback, false);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke clients with parameters and callback.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <param name="ignoreSelf">Current client shoud be ignored.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// The <i>asyncCallback</i> delegate identifies the callback invoked when the messages are sent, the <i>callback</i> object identifies the callback handling client responses.
        /// </para>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvokeClients method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvokeClients method. When your application calls BeginInvokeClients, the system will use a separate thread to execute the specified callback method, and will block on EndInvokeClients until the clients are invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        protected IAsyncResult BeginInvokeClients(AsyncCallback asyncCallback, string method, object[] arguments, IPendingServiceCallback callback, bool ignoreSelf)
        {
            return BeginInvokeClients(asyncCallback, method, arguments, callback, ignoreSelf, this.Scope);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke clients with parameters and callback.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="arguments">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <param name="ignoreSelf">Current client shoud be ignored.</param>
        /// <param name="targetScope">Invoke clients subscribed to the specified Scope.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// The <i>asyncCallback</i> delegate identifies the callback invoked when the messages are sent, the <i>callback</i> object identifies the callback handling client responses.
        /// </para>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvokeClients method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvokeClients method. When your application calls BeginInvokeClients, the system will use a separate thread to execute the specified callback method, and will block on EndInvokeClients until the clients are invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        protected IAsyncResult BeginInvokeClients(AsyncCallback asyncCallback, string method, object[] arguments, IPendingServiceCallback callback, bool ignoreSelf, IScope targetScope)
        {
            // Create IAsyncResult object identifying the asynchronous operation
            AsyncResultNoResult ar = new AsyncResultNoResult(asyncCallback, new InvokeData(FluorineContext.Current, method, arguments, callback, ignoreSelf, targetScope));
            // Use a thread pool thread to perform the operation
            FluorineFx.Threading.ThreadPoolEx.Global.QueueUserWorkItem(new System.Threading.WaitCallback(OnBeginInvokeClients), ar);
            // Return the IAsyncResult to the caller
            return ar;
        }

        private void OnBeginInvokeClients(object asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            try
            {
                // Perform the operation; if sucessful set the result
                InvokeData invokeData = ar.AsyncState as InvokeData;
                //Restore context
                FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, invokeData.Context);
                InvokeClients(invokeData.Method, invokeData.Arguments, invokeData.Callback, invokeData.IgnoreSelf, invokeData.TargetScope);
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
        /// Ends a pending asynchronous client invocation.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <remarks>
        /// <para>
        /// EndInvokeClients is a blocking method that completes the asynchronous client invocation request started in the BeginInvokeClients method.
        /// </para>
        /// <para>
        /// Before calling BeginInvokeClients, you can create a callback method that implements the AsyncCallback delegate. This callback method executes in a separate thread and is called by the system after BeginInvokeClients returns. 
        /// The callback method must accept the IAsyncResult returned by the BeginInvokeClients method as a parameter.
        /// </para>
        /// <para>Within the callback method you can call the EndInvokeClients method to successfully complete the invocation attempt.</para>
        /// <para>The BeginInvokeClients enables to use the fire and forget pattern too (by not implementing an AsyncCallback delegate), however if the invocation fails the EndInvokeClients method is responsible to throw an appropriate exception.
        /// Implementing the callback and calling EndInvokeClients also allows early garbage collection of the internal objects used in the asynchronous call.</para>
        /// </remarks>
        public void EndInvokeClients(IAsyncResult asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }

        /// <summary>
        /// Start a bandwidth check.
        /// </summary>
        protected virtual void CalculateClientBw()
        {
            CalculateClientBw(FluorineFx.Context.FluorineContext.Current.Connection);
        }
        /// <summary>
        /// Start a bandwidth check.
        /// </summary>
        /// <param name="client">Connection object.</param>
        protected virtual void CalculateClientBw(IConnection client)
        {
            new BWCheck(client).CalculateClientBw();
        }

        /// <summary>
        /// Returns stream length. Method added to get flv player to work.
        /// </summary>
        /// <param name="name">Stream name.</param>
        /// <returns>Returns the length of a stream, in seconds.</returns>
        public double getStreamLength(string name)
        {
            double duration = 0;
            IProviderService provider = ScopeUtils.GetScopeService(this.Scope, typeof(IProviderService)) as IProviderService;
            FileInfo file = provider.GetVODProviderFile(this.Scope, name);
            if (file != null)
            {
                IStreamableFileFactory factory = (IStreamableFileFactory)ScopeUtils.GetScopeService(this.Scope, typeof(IStreamableFileFactory)) as IStreamableFileFactory;
                IStreamableFileService service = factory.GetService(file);
                if (service != null)
                {
                    ITagReader reader = null;
                    try
                    {
                        IStreamableFile streamFile = service.GetStreamableFile(file);
                        reader = streamFile.GetReader();
                        duration = (double)reader.Duration / 1000;
                    }
                    catch (IOException ex)
                    {
                        if (log.IsErrorEnabled)
                            log.Error(string.Format("Error reading stream file {0}. {1}", file.FullName, ex.Message));
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();

                    }
                }
                else
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("No service found for {0}", file.FullName));
                }
                file = null;
            }
            return duration;
        }
    }
}
