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
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Api
{
    /// <summary>
    /// Defines the interface for a handler to push messages to a connected client.
    /// </summary>
	[CLSCompliant(false)]
    public interface IEndpointPushHandler
    {
        /// <summary>
        /// Gets the handler identity.
        /// </summary>
        string Id { get ; }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the connection.
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Closes the handler.
        /// </summary>
        void Close();
        /// <summary>
        /// Pushes messages to the client.
        /// </summary>
        /// <param name="messages">The list of messages to push.</param>
        void PushMessages(IMessage[] messages);
        /// <summary>
        /// Pushes a message to the client.
        /// </summary>
        /// <param name="message">The message to push.</param>
        void PushMessage(IMessage message);
        /// <summary>
        /// Invoked to notify the handler that the MessageClient subscription is using this handler.
        /// </summary>
        /// <param name="messageClient">The MessageClient subscription using this handler.</param>
        void RegisterMessageClient(IMessageClient messageClient);
        /// <summary>
        /// Invoked to notify the handler that a MessageClient subscription that was using it has been invalidated.
        /// </summary>
        /// <param name="messageClient">The MessageClient subscription no longer using this handler</param>
        void UnregisterMessageClient(IMessageClient messageClient);
        /// <summary>
        /// Gets pending messages.
        /// </summary>
        /// <returns>List of pending messages.</returns>
        IMessage[] GetPendingMessages();
        
    }
}
