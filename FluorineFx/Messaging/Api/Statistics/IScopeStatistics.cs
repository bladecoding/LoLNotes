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

namespace FluorineFx.Messaging.Api.Statistics
{
    /// <summary>
    /// Statistical informations about a scope.
    /// </summary>
    public interface IScopeStatistics : IStatisticsBase 
    {
        /// <summary>
        /// Get the name of this scope. Eg. <code>someroom</code>.
        /// </summary>
        /// <value>The name of the scope.</value>
        String Name{ get; }
        /// <summary>
        /// Gets the full absolute path. Eg. <code>host/myapp/someroom</code>.
        /// </summary>
        /// <value>Absolute scope path.</value>
        String Path { get;}
        /// <summary>
        /// Get the scopes depth, how far down the scope tree is it. The lowest depth is 0x00, the depth of Global scope. Application scope depth is 0x01. 
        /// Room depth is 0x02, 0x03 and so forth.
        /// </summary>
        /// <value>The depth of the scope.</value>
        int Depth { get; }
        /// <summary>
        /// Gets the total number of connections to the scope.
        /// </summary>
        /// <value>The number of connections.</value>
        int TotalConnections { get; }
        /// <summary>
        /// Gets the maximum number of concurrent connections to the scope.
        /// </summary>
        /// <value>The maximum number of concurrent connections.</value>
        int MaxConnections { get; }
        /// <summary>
        /// Gets the current number of connections to the scope.
        /// </summary>
        /// <value>The number of active connections.</value>
        int ActiveConnections { get; }
        /// <summary>
        /// Gets the total number of clients connected to the scope.
        /// </summary>
        /// <value>The number of clients.</value>
        int TotalClients { get; }
        /// <summary>
        /// Gets maximum number of clients concurrently connected to the scope.
        /// </summary>
        /// <value>The maximum number of clients.</value>
        int MaxClients { get; }
        /// <summary>
        /// Gets the current number of clients connected to the scope.
        /// </summary>
        /// <value>The number of active clients.</value>
        int ActiveClients { get; }
        /// <summary>
        /// Gets the total number of subscopes created.
        /// </summary>
        /// <value>The number of subscopes created.</value>
        int TotalSubscopes { get; }
        /// <summary>
        /// Gets the maximum number of concurrently existing subscopes.
        /// </summary>
        /// <value>The number of subscopes.</value>
        int MaxSubscopes { get; }
        /// <summary>
        /// Gets the number of currently existing subscopes.
        /// </summary>
        /// <value>The number of subscopes.</value>
        int ActiveSubscopes { get; }
    }
}
