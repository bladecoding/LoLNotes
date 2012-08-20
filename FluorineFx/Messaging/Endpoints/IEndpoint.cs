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

using FluorineFx.Messaging;
#if !FXCLIENT
using FluorineFx.Messaging.Config;
#endif
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Endpoints
{
	/// <summary>
    /// <para>
	/// Endpoint interface.
    /// </para>
    /// <para>
    /// An endpoint receives messages from clients and decodes them, then sends them on to a MessageBroker for routing to a service.
    /// The endpoint also encodes messages and delivers them to clients. Endpoints are specific to a message format and network transport, 
    /// and are defined by the named URI path on which they are located. 
    /// </para>
	/// </summary>
    [CLSCompliant(false)]
	public interface IEndpoint
	{
        /// <summary>
        /// Gets the endpoint id.
        /// </summary>
        /// <remarks>All endpoints are referenceable by an id that is unique among all the endpoints registered to a single broker instance.</remarks>
		string Id{ get; }
        /// <summary>
        /// Starts the endpoint.
        /// </summary>
		void Start();
        /// <summary>
        /// Stops and destroys the endpoint.
        /// </summary>
		void Stop();
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		void Service();
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
		IMessage ServiceMessage(IMessage message);
        /// <summary>
        /// Specifies whether this protocol requires the secure HTTPS protocol.
        /// </summary>
		bool IsSecure{ get; }
        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        int ClientLeaseTime { get; }
#if !FXCLIENT
        /// <summary>
        /// Returns a reference to the message broker managing this endpoint.
        /// </summary>
        /// <returns></returns>
		MessageBroker GetMessageBroker();
        /// <summary>
        /// Gets channel defintion settings.
        /// </summary>
        /// <returns></returns>
        ChannelDefinition ChannelDefinition { get; }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageclient"></param>
		void Push(IMessage message, MessageClient messageclient);
#endif
    }
}
