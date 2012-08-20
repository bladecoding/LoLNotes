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
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// IServerStream represents a stream broadcasted from the server.
    /// </summary>
    [CLSCompliant(false)]
    public interface IServerStream : IPlaylist, IBroadcastStream
    {
        /// <summary>
        /// Toggles the paused state.
        /// </summary>
        void Pause();
        /// <summary>
        /// Seek to a given position in the stream.
        /// </summary>
        /// <param name="position">New playback position in milliseconds.</param>
        void Seek(int position);
    }
}
