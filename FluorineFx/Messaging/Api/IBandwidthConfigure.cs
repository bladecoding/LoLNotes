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
    /// Interface for setting/getting bandwidth configuration.
    /// 
    /// Two properties are provided for bandwidth configuration. The property
    /// "channelBandwidth" is used to configure the bandwidth of each channel.
    /// The property "channelInitialBurst" is used to configure the initial
    /// bytes that can be sent to client in each channel.
    /// </summary>
    public interface IBandwidthConfigure
    {
        /// <summary>
        /// Returns the bandwidth configure for 3 channels: audio, video, data and the overall bandwidth.
        /// The unit is bit per second. A value of -1 means "don't care" so that there's no limit on bandwidth for that channel.
        /// The last element is the overall bandwidth. If it's not -1, the value of the first three elements will be ignored.
        /// </summary>
        /// <returns>The 4-element array of bandwidth configuration.</returns>
        long[] GetChannelBandwidth();
        /// <summary>
        /// Returns the byte count of initial burst value for 3 channels: audio, video, data and the overall bandwidth.
        /// If the value is -1, the default will be used per the implementation of bandwidth controller.
        /// </summary>
        /// <returns>The 4-element array of byte count of initial burst value.</returns>
        long[] GetChannelInitialBurst();
    }
}
