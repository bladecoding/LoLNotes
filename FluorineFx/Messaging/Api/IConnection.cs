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

namespace FluorineFx.Messaging.Api
{
	/// <summary>
    /// The connection object. Each connection has an associated client and scope. 
    /// Connections may be persistent, polling, or transient.
	/// </summary>
	[CLSCompliant(false)]
	public interface IConnection : ICoreObject
	{
        /// <summary>
        /// Try to connect to the scope.
        /// </summary>
        /// <param name="scope">Scope object.</param>
        /// <returns>true on success, false otherwise.</returns>
		bool Connect(IScope scope);
        /// <summary>
        /// Try to connect to the scope with a list of connection parameters.
        /// </summary>
        /// <param name="scope">Scope object.</param>
        /// <param name="args">Connections parameters.</param>
        /// <returns>true on success, false otherwise.</returns>
        bool Connect(IScope scope, object[] args);
        /// <summary>
        /// Gets whether the client is connected to the scope.
        /// </summary>
		bool IsConnected { get; }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        void Timeout();
        /// <summary>
        /// Close the connection. This will disconnect the client from the associated scope.
        /// </summary>
		void Close();
        /// <summary>
        /// Returns the parameters of the "connect" call.
        /// </summary>
		IDictionary Parameters{ get; }
        /// <summary>
        /// Gets the client object associated with this connection.
        /// </summary>
		IClient Client { get; }
        /// <summary>
        /// Gets the session object associated with this connection.
        /// </summary>
        ISession Session { get; }
        /// <summary>
        /// Get the scope this client is connected to.
        /// </summary>
		IScope Scope { get; }
        /// <summary>
        /// Gets the basic scopes this connection has subscribed.  This list will
        /// contain the shared objects and broadcast streams the connection connected to.
        /// </summary>
		IEnumerator BasicScopes { get; }
        /// <summary>
        /// Gets the connection id.
        /// </summary>
        string ConnectionId { get; }
        /// <summary>
        /// Gets the session id.
        /// </summary>
		string SessionId{ get; }
        /// <summary>
        /// Gets the object encoding (AMF version) for this connection.
        /// </summary>
		ObjectEncoding ObjectEncoding{ get; }
        /// <summary>
        /// Returns a String that represents the current Object. 
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
		string ToString();
        /// <summary>
        /// Start measuring the roundtrip time for a packet on the connection.
        /// </summary>
        void Ping();
        /// <summary>
        /// Gets the total number of bytes read from the connection.
        /// </summary>
        long ReadBytes { get; }
        /// <summary>
        /// Gets the total number of bytes written to the connection.
        /// </summary>
        long WrittenBytes { get; }
        /// <summary>
        /// Gets the total number of messages read from the connection.
        /// </summary>
        long ReadMessages { get; }
        /// <summary>
        /// Gets the total number of messages written to the connection.
        /// </summary>
        long WrittenMessages { get; }
        /// <summary>
        /// Gets the total number of messages that have been dropped.
        /// </summary>
        long DroppedMessages { get;}
        /// <summary>
        /// Gets the total number of messages that are pending to be sent to the connection.
        /// </summary>
        long PendingMessages { get;}
        /// <summary>
        /// Gets the number of written bytes the client reports to have received.
        /// This is the last value of the BytesRead message received from a client.
        /// </summary>
        long ClientBytesRead { get;}
        /// <summary>
        /// Gets roundtrip time of last ping command.
        /// </summary>
        int LastPingTime { get; }
        /*
        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        int ClientLeaseTime { get; }
        */
        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        bool IsFlexClient { get; }
        /// <summary>
        /// Gets the network endpoint.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
        /// <summary>
        /// Gets the path for this connection. This is not updated if you switch scope.
        /// </summary>
        string Path { get; }
	}
}
