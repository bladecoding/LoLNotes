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
using System.Reflection;

using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Services.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class MessagingAdapter : ServiceAdapter
	{
        /// <summary>
        /// Initializes a new instance of the MessagingAdapter class.
        /// </summary>
        public MessagingAdapter()
		{
		}

		/// <summary>
		/// This method is responsible for handling the message and returning a result (if any).
		/// The return value of this message is used as the body of the acknowledge message returned to the client. It may be null if there is no data being returned for this message. 
		/// </summary>
		/// <param name="message">The message received from the client.</param>
		/// <returns>The body of the acknowledge message (or null if there is no body).</returns>
		public override object Invoke(IMessage message)
		{
			MessageService messageService = this.Destination.Service as MessageService;
			messageService.PushMessageToClients(message);
			return null;
		}

		/// <summary>
		/// Invoked before a client message is sent to a subtopic.
		/// </summary>
        /// <param name="subtopic">The subtopic the client is attempting to send a message to.</param>
        /// <returns>true to allow the message to be sent, false to prevent it.</returns>
		public bool AllowSend(Subtopic subtopic) 
		{
			return true;
		}
		/// <summary>
		/// Invoked before a client subscribe request is processed.
		/// </summary>
        /// <param name="subtopic">The subtopic the client is attempting to subscribe to.</param>
		/// <returns>true to allow the subscription, false to prevent it.</returns>
		public bool AllowSubscribe(Subtopic subtopic)
		{
			return true;
		}
	}
}
