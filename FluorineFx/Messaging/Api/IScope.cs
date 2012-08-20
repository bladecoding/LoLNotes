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
using System.Collections.Generic;
using FluorineFx.Messaging.Api.Statistics;
using FluorineFx.Messaging.Endpoints;

namespace FluorineFx.Messaging.Api
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
    public interface IScope : IBasicScope, IServiceContainer
    {
        /// <summary>
        /// Adds given connection to the scope.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <returns>true on success, false if the specified connection already belongs to this scope</returns>
        bool Connect(IConnection connection);
        /// <summary>
        /// Adds given connection to the scope.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="parameters">Parameters passed.</param>
        /// <returns>true on success, false if the specified connection already belongs to this scope</returns>
        bool Connect(IConnection connection, object[] parameters);
        /// <summary>
        /// Removes the specified connection from list of scope connections. This disconnects
        /// all clients of the specified connection from the scope.
        /// </summary>
        /// <param name="conn">Connection object.</param>
        void Disconnect(IConnection conn);
        /// <summary>
        /// Returns scope context.
        /// </summary>
        IScopeContext Context { get; }
        /// <summary>
        /// Check to see if this scope has a child scope matching a given name.
        /// </summary>
        /// <param name="name">The name of the child scope.</param>
        /// <returns>true if a child scope exists, otherwise false.</returns>
        bool HasChildScope(string name);
        /// <summary>
        /// Checks whether scope has a child scope with given name and type.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <param name="name">Child scope name.</param>
        /// <returns>true if a child scope exists, otherwise false.</returns>
        bool HasChildScope(string type, string name);
        /// <summary>
        /// Creates child scope with name given and returns success value.
        /// </summary>
        /// <param name="name">New child scope name.</param>
        /// <returns>true if child scope was successfully creates, false otherwise.</returns>
        bool CreateChildScope(string name);
        /// <summary>
        /// Adds scope as a child scope.
        /// </summary>
        /// <param name="scope">Add the specified scope.</param>
        /// <returns>true if child scope was successfully added, false otherwise.</returns>
        bool AddChildScope(IBasicScope scope);
        /// <summary>
        /// Removes scope from the children scope list.
        /// </summary>
        /// <param name="scope">Removes the specified scope.</param>
        void RemoveChildScope(IBasicScope scope);
        /// <summary>
        /// Gets the child scope names.
        /// </summary>
        /// <returns>Collection of child scope names.</returns>
        ICollection<String> GetScopeNames();
        /// <summary>
        /// Returns an iterator of basic scope names.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <returns>An iterator of basic scope names.</returns>
        IEnumerator GetBasicScopeNames(string type);
        /// <summary>
        /// Gets a child scope by name.
        /// </summary>
        /// <param name="type">Child scope type.</param>
        /// <param name="name">Name of the child scope.</param>
        /// <returns>Scope object.</returns>
        IBasicScope GetBasicScope(string type, string name);
        /// <summary>
        /// Returns scope by name.
        /// </summary>
        /// <param name="name">Scope name.</param>
        /// <returns>Scope with the specified name.</returns>
        IScope GetScope(string name);
        /// <summary>
        /// Gets a set of connected clients.
        /// </summary>
        /// <returns>Collection of connected clients.</returns>
        ICollection<IClient> GetClients();
        /// <summary>
        /// Returns an enumerator that iterates through connections.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the connections.</returns>
        IEnumerator<IConnection> GetConnections();
        /// <summary>
        /// Gets a value indicating whether this instance has handler.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has context; otherwise, <c>false</c>.
        /// </value>
        bool HasHandler { get; }
        /// <summary>
        /// Gets or sets handler of the scope.
        /// </summary>
        /// <value>Scope handler object.</value>
        IScopeHandler Handler { get; }
        /// <summary>
        /// Gets context path.
        /// </summary>
        /// <value>The context path.</value>
        string ContextPath { get; }
        /// <summary>
        /// Returns collection of connections for the specified client.
        /// </summary>
        /// <param name="client">The client object.</param>
        /// <returns>Collection of connections.</returns>
        ICollection LookupConnections(IClient client);
        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        IEndpoint Endpoint { get; }
        /// <summary>
        /// Gets statistics information about the scope.
        /// </summary>
        /// <value>Scope statistics information.</value>
        IScopeStatistics Statistics { get; }
    }
}
