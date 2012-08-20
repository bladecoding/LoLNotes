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
    /// The bandwidth configure for connection that has an extra
    /// property "upstreamBandwidth" which is not used by Bandwidth Control Framework.
    /// </summary>
    public interface IConnectionBWConfig : IBandwidthConfigure
    {
        /// <summary>
        /// Gets or sets the upstream bandwidth to be notified to the client.
        /// Upstream is the data that is sent from the client to the server.
        /// </summary>
        long UpstreamBandwidth { get; set; }
        /// <summary>
        /// Gets downstream bandwidth.
        /// </summary>
        /// <value>Downstream bandwidth, from server to client.</value>
        long DownstreamBandwidth { get; }
    }
}
