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

namespace FluorineFx.Messaging.Api
{
    /// <summary>
    /// Interface to be notified when a session is created or destroyed.
    /// </summary>
    /// <remarks>
    /// This is not the ASP.NET provided session-state but rather the server side client
    /// object representing a client-side session.
    /// </remarks>
    /// <example>
    /// 	<code lang="CS">
    ///     class ChatAdapter : MessagingAdapter, ISessionListener
    ///     {
    ///         public ChatAdapter()
    ///         {
    ///             ClientManager.AddSessionCreatedListener(this);
    ///         }
    ///  
    ///         public void SessionCreated(IClient client)
    ///         {
    ///             client.AddSessionDestroyedListener(this);
    ///         }
    ///  
    ///         public void SessionDestroyed(IClient client)
    ///         {
    ///         }
    ///     }
    /// </code>
    /// </example>
	[CLSCompliant(false)]
	public interface ISessionListener
	{
        /// <summary>
        /// Notification that a session was created.
        /// </summary>
        /// <param name="session">The session that was created.</param>
        void SessionCreated(ISession session);
        /// <summary>
        /// Notification that a session is about to be destroyed.
        /// </summary>
        /// <param name="session">The session that will be destroyed.</param>
        void SessionDestroyed(ISession session);
	}
}
