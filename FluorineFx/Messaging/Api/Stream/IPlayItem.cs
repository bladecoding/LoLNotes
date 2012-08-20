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
using FluorineFx.Messaging.Api.Messaging;

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// Playlist item. Each playlist item has name, start time, length in milliseconds and message input source.
    /// </summary>
    [CLSCompliant(false)]
    public interface IPlayItem
    {
        /// <summary>
        /// Gets the name of item.
        /// The VOD or Live stream provider is found according to this name.
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Gets the start time in millisecond.
        /// </summary>
        long Start { get; }
        /// <summary>
        /// Gets the play length in millisecond.
        /// </summary>
        long Length { get; }
        /// <summary>
        /// Gets a message input for play.
        /// This object overrides the default algorithm for finding
        /// the appropriate VOD or Live stream provider according to the item name.
        /// </summary>
        IMessageInput MessageInput { get; }
    }
}
