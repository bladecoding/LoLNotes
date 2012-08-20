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

using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Config;

namespace FluorineFx.Messaging.Services
{
	/// <summary>
	/// The MessageBroker has endpoints on one end and services on the other.
	/// The Service interface defines the contract between the MessageBroker 
	/// and all Service implementations.
	/// </summary>
    [CLSCompliant(false)]
    public interface IService
	{
        /// <summary>
        /// Gets the service identity.
        /// </summary>
		string id { get; }
		/// <summary>
        /// Retrieves the MessageBroker managing this service.
        /// This MessageBroker is used to push a message to one or more endpoints managed by the broker. 
		/// </summary>
        /// <returns>The MessageBroker managing this service.</returns>
		MessageBroker GetMessageBroker();
		/// <summary>
		/// Retrieves the destination in this service for which the given message is intended.
		/// </summary>
		/// <param name="message"></param>
		/// <returns>The requested Destination.</returns>
        [CLSCompliant(false)]
        Destination GetDestination(IMessage message);
		/// <summary>
		/// Handles a message routed to the service by the MessageBroker.
		/// </summary>
		/// <param name="message">The message sent by the MessageBroker.</param>
		/// <returns></returns>
		object ServiceMessage(IMessage message);
		/// <summary>
		/// Determines whether this Service is capable of handling a given Message instance.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		bool IsSupportedMessage(IMessage message);
		/// <summary>
		/// Determines whether this Service is capable of handling messages of a given type.
		/// </summary>
		/// <param name="messageClassName"></param>
		/// <returns></returns>
		bool IsSupportedMessageType(string messageClassName);
		/// <summary>
		/// Performs any startup actions necessary after the service has been added to the broker.
		/// </summary>
		void Start();
		/// <summary>
		/// Performs any actions necessary before removing the service from the broker.
		/// </summary>
		void Stop();
        /// <summary>
        /// Retrieves the destination in this service for with the given destination identity.
        /// </summary>
        /// <param name="id">Destination identity.</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        Destination GetDestination(string id);
        /// <summary>
        /// Performs programmatic, custom authentication.
        /// </summary>
        /// <param name="message"></param>
		void CheckSecurity(IMessage message);
        /// <summary>
        /// Performs programmatic, custom authentication.
        /// </summary>
        /// <param name="destination"></param>
        [CLSCompliant(false)]
        void CheckSecurity(Destination destination);
        /// <summary>
        /// Retrieves the list of destinations in this service.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        Destination[] GetDestinations();
	}
}
