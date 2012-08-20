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

using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Adapter
{
	/// <summary>
	/// IApplication provides lifecycle methods that most communication applications
	/// will use. This interface defines the methods that are called by the gateway through
	/// an applications life.
	/// </summary>
	[CLSCompliant(false)]
	public interface IApplication
	{
		/// <summary>
		/// Called once when application or room starts.
		/// </summary>
		/// <param name="application">Application or room level scope.</param>
		/// <returns><c>true</c> continues application run, <c>false</c> terminates</returns>
		bool OnAppStart(IScope application);
		/// <summary>
		/// Called per each client connect
		/// </summary>
		/// <param name="connection">Connection object used to provide basic connection methods.</param>
		/// <param name="parameters">List of params sent from client with NetConnection.connect call</param>
		/// <returns><c>true</c> accepts the connection, <c>false</c> rejects it</returns>
		bool OnAppConnect(IConnection connection, object[] parameters);
		/// <summary>
		/// Called every time client joins app level scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="application">Scope object.</param>
		/// <returns><code>true</code> accepts the client, <code>false</code> rejects it</returns>
		bool OnAppJoin(IClient client, IScope application);
		/// <summary>
		/// Called every time client disconnects from the application.
		/// </summary>
		/// <param name="connection">Connection object.</param>
		void OnAppDisconnect(IConnection connection);
		/// <summary>
		/// Called every time client leaves the application scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="application">Scope object.</param>
		void OnAppLeave(IClient client, IScope application);
		/// <summary>
		/// Called on application stop.
		/// </summary>
		/// <param name="application">Scope object.</param>
		void OnAppStop(IScope application);
		/// <summary>
		/// Called on application room start.
		/// </summary>
		/// <param name="room">Scope object</param>
		/// <returns><c>true</c> if scope can be started, <c>false</c> otherwise</returns>
		bool OnRoomStart(IScope room);
		/// <summary>
		/// Called every time client connects to the room.
		/// </summary>
		/// <param name="connection">Connection object.</param>
		/// <param name="parameters">List of params sent from client with NetConnection.connect call</param>
		/// <returns><c>true</c> accepts the connection, <c>false</c> rejects it.</returns>
		bool OnRoomConnect(IConnection connection, object[] parameters);
		/// <summary>
		/// Called when user joins room scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="room">Scope object.</param>
		/// <returns><c>true</c> accepts the client, <c>false</c> rejects it.</returns>
		bool OnRoomJoin(IClient client, IScope room);
		/// <summary>
		/// Called when client disconnects from room scope.
		/// </summary>
		/// <param name="connection">Connection object used to provide basic connection methods.</param>
		void OnRoomDisconnect(IConnection connection);
		/// <summary>
		/// Called when user leaves room scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="room">Scope object.</param>
		void OnRoomLeave(IClient client, IScope room);
		/// <summary>
		/// Called on room scope stop.
		/// </summary>
		/// <param name="room">Scope object.</param>
		void OnRoomStop(IScope room);
	}
}
