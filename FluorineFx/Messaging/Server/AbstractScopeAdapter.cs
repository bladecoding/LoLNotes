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
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Adapter
{
	/// <summary>
    /// Base scope handler implementation.
	/// </summary>
	[CLSCompliant(false)]
	public abstract class AbstractScopeAdapter : IScopeHandler
	{
		private bool _canStart = true;
		private bool _canConnect = true;
		private bool _canJoin = true;
		private bool _canCallService = true;
		private bool _canAddChildScope = true;
		private bool _canHandleEvent = true;

        /// <summary>
        /// Initializes a new instance of the AbstractScopeAdapter class.
        /// </summary>
		public AbstractScopeAdapter()
		{
		}
        /// <summary>
        /// Sets whether the scope is ready to be activated.
        /// </summary>
		public bool CanStart 
		{
			set{ _canStart = value; }
		}
        /// <summary>
        /// Sets whether remote service calls are allowed for the scope.
        /// </summary>
		public bool CanCallService
		{
			set{ _canCallService = value; }
		}
        /// <summary>
        /// Sets whether connections to scope are allowed.
        /// </summary>
		public bool CanConnect
		{
			set{ _canConnect = value; }
		}
        /// <summary>
        ///  Sets whether scope may be joined by users.
        /// </summary>
		public bool CanJoin
		{
			set{ _canJoin = value; }
		}

		#region IScopeHandler Members

        /// <summary>
        /// Called when a scope is created for the first time.
        /// </summary>
        /// <param name="scope">The new scope object.</param>
        /// <returns>true to allow, false to deny.</returns>
		public virtual bool Start(IScope scope)
		{
			return _canStart;
		}
        /// <summary>
        /// Called just before a scope is disposed.
        /// </summary>
        /// <param name="scope"> Scope that is disposed.</param>
		public virtual void Stop(IScope scope)
		{
			// NA
		}
        /// <summary>
        /// Called just before every connection to a scope. You can pass additional params from client using NetConnection.connect method.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="scope">Scope object.</param>
        /// <param name="parameters">List of params passed from client via NetConnection.connect method. All parameters but the first one passed to NetConnection.connect method are available as parameters array.</param>
        /// <returns>true to allow, false to deny.</returns>
		virtual public bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
			return _canConnect;
		}
        /// <summary>
        /// Called just after the a connection is disconnected.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="scope">Scope object.</param>
		virtual public void Disconnect(IConnection connection, IScope scope)
		{
			// NA
		}
        /// <summary>
        /// Called just before a client enters the scope.
        /// </summary>
        /// <param name="client">Client object.</param>
        /// <param name="scope">Scope that is joined by client.</param>
        /// <returns>true to allow, false to deny.</returns>
        public virtual bool Join(IClient client, IScope scope)
        {
            return _canJoin;
        }
        /// <summary>
        /// Called just after the client leaves the scope.
        /// </summary>
        /// <param name="client">Client object.</param>
        /// <param name="scope">Scope object.</param>
        public virtual void Leave(IClient client, IScope scope)
        {
            // NA
        }
        /// <summary>
        /// Called when a service is called.
        /// </summary>
        /// <param name="connection">The connection object.</param>
        /// <param name="call">The call object.</param>
        /// <returns>true to allow, false to deny.</returns>
        public bool ServiceCall(IConnection connection, FluorineFx.Messaging.Api.Service.IServiceCall call)
        {
            return _canCallService;
        }
        /// <summary>
        /// Called just before a child scope is added.
        /// </summary>
        /// <param name="scope">Scope that will be added.</param>
        /// <returns>true to allow, false to deny.</returns>
		public bool AddChildScope(IBasicScope scope)
		{
			return _canAddChildScope;
		}
        /// <summary>
        /// Called just after a child scope has been removed.
        /// </summary>
        /// <param name="scope">Scope that has been removed.</param>
		public void RemoveChildScope(IBasicScope scope)
		{
			// NA
		}



		#endregion

		#region IEventHandler Members
        /// <summary>
        /// Handle an event.
        /// </summary>
        /// <param name="evt">Event to handle.</param>
        /// <returns>true if event was handled, false if it should bubble.</returns>
		public bool HandleEvent(FluorineFx.Messaging.Api.Event.IEvent evt)
		{
			return _canHandleEvent;
		}

		#endregion
	}
}
