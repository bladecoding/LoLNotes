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

namespace FluorineFx.Messaging.Api.Statistics
{
    /// <summary>
    /// Statistical informations about a stream that is broadcasted by a client.
    /// </summary>
    public interface IClientBroadcastStreamStatistics : IStreamStatistics
    {
        /// <summary>
        /// Gets the filename the stream is being saved as.
        /// </summary>
        /// <value>The filename relative to the scope or <code>null</code> if the stream is not being saved.</value>
        String SaveFilename { get; }
        /// <summary>
        /// Gets stream publish name. Publish name is the value of the first parameter
        /// had been passed to <code>NetStream.publish</code> on client side in SWF.
        /// </summary>
        String PublishedName { get; }
        /// <summary>
        /// Gets the total number of subscribers.
        /// </summary>
        int TotalSubscribers { get; }
        /// <summary>
        /// Gets the maximum number of concurrent subscribers.
        /// </summary>
        int MaxSubscribers { get; }
        /// <summary>
        /// Gets the current number of subscribers.
        /// </summary>
        int ActiveSubscribers { get; }
        /// <summary>
        /// Gets the total number of bytes received from client for this stream.
        /// </summary>
        long BytesReceived { get; }
    }
}
