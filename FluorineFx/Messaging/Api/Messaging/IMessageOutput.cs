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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Api.Messaging
{
    /// <summary>
    /// Output Endpoint for a provider to connect.
    /// </summary>
    [CLSCompliant(false)]
    public interface IMessageOutput
    {
        /// <summary>
        /// Push a message to this output endpoint. May block the pusher when output can't handle the message at the time.
        /// </summary>
        /// <param name="message">Message to be pushed.</param>
        void PushMessage(IMessage message);
#if !(NET_1_1)
        /// <summary>
        /// Connect to a provider. Note that parameters passed have nothing to deal with NetConnection.connect in client-side Flex/Flash RIA.
        /// </summary>
        /// <param name="provider">Provider object.</param>
        /// <param name="parameterMap">Parameters passed with connection</param>
        /// <returns>true when successfully subscribed, false otherwise.</returns>
        bool Subscribe(IProvider provider, Dictionary<string, object> parameterMap);
#else
        /// <summary>
        /// Connect to a provider. Note that parameters passed have nothing to deal with NetConnection.connect in client-side Flex/Flash RIA.
        /// </summary>
        /// <param name="provider">Provider object.</param>
        /// <param name="parameterMap">Parameters passed with connection</param>
        /// <returns>true when successfully subscribed, false otherwise.</returns>
        bool Subscribe(IProvider provider, Hashtable parameterMap);
#endif
        /// <summary>
        /// Disconnect from a provider.
        /// </summary>
        /// <param name="provider">Provider object.</param>
        /// <returns>true when successfully unsubscribed, false otherwise.</returns>
        bool Unsubscribe(IProvider provider);
        /// <summary>
        /// Returns collection of providers.
        /// </summary>
        /// <returns>Collection of IProvider objects.</returns>
        ICollection GetProviders();
        /// <summary>
        /// Send OOB Control Message to all consumers on the other side of pipe.
        /// </summary>
        /// <param name="provider">The provider that sends the message.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message.</param>
        void SendOOBControlMessage(IProvider provider, OOBControlMessage oobCtrlMsg);
    }
}
