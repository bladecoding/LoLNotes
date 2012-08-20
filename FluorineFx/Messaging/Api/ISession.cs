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
using System.Security.Principal;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Api
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
	[CLSCompliant(false)]
    public interface ISession : ICollection, IEnumerable
    {
        /// <summary>
        /// Adds a new item to the session-state collection
        /// </summary>
        /// <param name="name">The name of the item to add to the session-state collection.</param>
        /// <param name="value">The value of the item to add to the session-state collection.</param>
        void Add(string name, Object value);
        /// <summary>
        /// Removes all keys and values from the session-state collection. 
        /// </summary>
        void Clear();
        /// <summary>
        /// Deletes an item from the session-state collection.
        /// </summary>
        /// <param name="name">The name of the item to delete from the session-state collection.</param>
        /// <remarks>If the session-state collection does not contain an element with the specified name, the collection remains unchanged. No exception is thrown.</remarks>
        void Remove(string name);
        /// <summary>
        /// Removes all keys and values from the session-state collection. 
        /// </summary>
        void RemoveAll();
        /// <summary>
        /// Gets the unique identifier for the session. 
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets or sets a session value by name.
        /// </summary>
        /// <param name="name">The key name of the session value.</param>
        /// <returns>The session-state value with the specified name.</returns>
        Object this[string name] { get; set; }
        /// <summary>
        /// Gets whether this is a newly created session.
        /// </summary>
        bool IsNew{ get; } 
        /// <summary>
        /// Adds a session destroy listener that will be notified when the session is destroyed.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        void AddSessionDestroyedListener(ISessionListener listener);
        /// <summary>
        /// Removes a session destroy listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveSessionDestroyedListener(ISessionListener listener);
        /// <summary>
        /// Associates a Client with the Session.
        /// </summary>
        /// <param name="client">The Client to assocaite with the session.</param>
        void RegisterClient(IClient client);
        /// <summary>
        /// Disassociates a Client from the Session.
        /// </summary>
        /// <param name="client">The Client to disassociate from the session.</param>
        void UnregisterClient(IClient client);
        /// <summary>
        /// Notifies session listeners.
        /// </summary>
        void NotifyCreated();
        /// <summary>
        /// Invalidates the Session.
        /// </summary>
        void Invalidate();
        /// <summary>
        /// Invalidates session upon timeout.
        /// </summary>
        void Timeout();
        /// <summary>
        /// Gets whether the session is valid.
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// Pushes a message to a remote client.
        /// </summary>
        /// <param name="message">Message to push.</param>
        /// <param name="messageClient">The MessageClient subscription that this message targets.</param>
        void Push(IMessage message, IMessageClient messageClient);
        /// <summary>
        /// Gets or sets security information for the session.
        /// </summary>
        /// <remarks>Available only when perClientAuthentication is not in use.</remarks>
        IPrincipal Principal { get; set; }
    }
}
