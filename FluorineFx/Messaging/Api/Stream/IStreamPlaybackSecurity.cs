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

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// Interface for handlers that control access to stream playback.
    /// </summary>
	[CLSCompliant(false)]
    public interface IStreamPlaybackSecurity
    {
        /// <summary>
        /// Check if playback of a stream with the given name is allowed.
        /// </summary>
        /// <param name="scope">Scope the stream is about to be played back from.</param>
        /// <param name="name">Name of the stream to play.</param>
        /// <param name="start">Position to start playback from (in milliseconds).</param>
        /// <param name="length">Duration to play (in milliseconds).</param>
        /// <param name="flushPlaylist">Flush playlist.</param>
        /// <returns>true if playback is allowed, otherwise false.</returns>
        bool IsPlaybackAllowed(IScope scope, string name, long start, long length, bool flushPlaylist);
    }
}
