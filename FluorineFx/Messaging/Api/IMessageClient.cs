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

namespace FluorineFx.Messaging.Api
{
    /// <summary>
    /// MessageClient interface.
    /// </summary>
	[CLSCompliant(false)]
    public interface IMessageClient
    {
        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Gets the message client identity.
        /// </summary>
        /// <value>The message client identity.</value>
        string ClientId { get; }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        void Renew();
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns></returns>
        byte[] GetBinaryId();
        /// <summary>
        /// Gets whether the MessageClient is being disconnected.
        /// </summary>
        bool IsValid { get;}
        /// <summary>
        /// Gets the endpoint identity the MessageClient is subscribed to.
        /// </summary>
        string EndpointId { get; }
        /// <summary>
        /// Adds a MessageClient destroy listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        void AddMessageClientDestroyedListener(IMessageClientListener listener);
        /// <summary>
        /// Removes a MessageClient destroyed listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveMessageClientDestroyedListener(IMessageClientListener listener);
        /// <summary>
        /// Gets the Client associated with this MessageClient.
        /// </summary>
        IClient Client { get; }
        /// <summary>
        /// Gets the Session associated with this MessageClient.
        /// </summary>
        ISession Session{ get; }
        /// <summary>
        /// Invalidates the MessageClient.
        /// </summary>
        void Invalidate();
    }
}
