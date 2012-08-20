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

namespace FluorineFx.Messaging.Api.Service
{
    /// <summary>
    /// Utility functions to invoke methods on connections.
    /// </summary>
	[CLSCompliant(false)]
    public class ServiceUtils
    {
        /// <summary>
        /// Invoke a method on the current connection.
        /// </summary>
        /// <param name="method">Name of the method to invoke.</param>
        /// <param name="args">Parameters to pass to the method.</param>
        /// <returns><code>true</code> if the connection supports method calls, otherwise <code>false</code></returns>
	    public static bool InvokeOnConnection(string method, object[] args) 
        {
            return InvokeOnConnection(method, args, null);
	    }
        /// <summary>
        /// Invoke a method on the current connection and handle result.
        /// </summary>
        /// <param name="method">Name of the method to invoke.</param>
        /// <param name="args">Parameters to pass to the method.</param>
        /// <param name="callback">Object to notify when result is received.</param>
        /// <returns><code>true</code> if the connection supports method calls, otherwise <code>false</code></returns>
        public static bool InvokeOnConnection(string method, object[] args, IPendingServiceCallback callback)
        {
            IConnection connection = FluorineFx.Context.FluorineContext.Current.Connection;
            return InvokeOnConnection(connection, method, args, callback);
        }
        /// <summary>
        /// Invoke a method on a given connection.
        /// </summary>
        /// <param name="connection">Connection to invoke method on.</param>
        /// <param name="method">Name of the method to invoke.</param>
        /// <param name="args">Parameters to pass to the method.</param>
        /// <returns><code>true</code> if the connection supports method calls, otherwise <code>false</code></returns>
        public static bool InvokeOnConnection(IConnection connection, string method, object[] args)
        {
            return InvokeOnConnection(connection, method, args, null);
        }
        /// <summary>
        /// Invoke a method on a given connection and handle result.
        /// </summary>
        /// <param name="connection">Connection to invoke method on.</param>
        /// <param name="method">Name of the method to invoke.</param>
        /// <param name="args">Parameters to pass to the method.</param>
        /// <param name="callback">Object to notify when result is received.</param>
        /// <returns><code>true</code> if the connection supports method calls, otherwise <code>false</code></returns>
        public static bool InvokeOnConnection(IConnection connection, string method, object[] args, IPendingServiceCallback callback)
        {
            if (connection is IServiceCapableConnection)
            {
                if (callback == null)
                    (connection as IServiceCapableConnection).Invoke(method, args);
                else
                    (connection as IServiceCapableConnection).Invoke(method, args, callback);
                return true;
            }
            else
                return false;
        }
    }
}
