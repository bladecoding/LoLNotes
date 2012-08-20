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
using FluorineFx.Util;

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// Video codec info used by stream codec.
    /// </summary>
    [CLSCompliant(false)]
    public interface IVideoStreamCodec
    {
        /// <summary>
        /// Gets the name of the video codec.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Resets the codec to its initial state.
        /// </summary>
        void Reset();
        /// <summary>
        /// Gets whether the codec supports frame dropping.
        /// </summary>
        bool CanDropFrames { get; }
        /// <summary>
        /// Returns true if the codec knows how to handle the passed stream data.
        /// </summary>
        /// <param name="data">Stream data.</param>
        /// <returns><code>true</code> if codec can handle data, <code>false</code> otherwise</returns>
        bool CanHandleData(ByteBuffer data);
        /// <summary>
        /// Update the state of the codec with the passed data.
        /// </summary>
        /// <param name="data">Stream data.</param>
        /// <returns><code>true</code> if codec can handle data, <code>false</code> otherwise</returns>
        bool AddData(ByteBuffer data);
        /// <summary>
        /// Returns the data for a keyframe.
        /// </summary>
        /// <returns>Data.</returns>
        ByteBuffer GetKeyframe();
        /// <summary>
        /// Gets the decoder configuration.
        /// </summary>
        /// <returns>Data for decoder setup.</returns>
        ByteBuffer GetDecoderConfiguration();
    }
}
