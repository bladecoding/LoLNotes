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
    /// A play list controller that controls the order of play items.
    /// </summary>
    [CLSCompliant(false)]
    public interface IPlaylistController
    {
        /// <summary>
        /// Get next item to play.
        /// </summary>
        /// <param name="playlist">The play list.</param>
        /// <param name="itemIndex">The current item index. -1 indicates to retrieve the first item for play.</param>
        /// <returns>The next item index to play. -1 reaches the end.</returns>
        int NextItem(IPlaylist playlist, int itemIndex);
        /// <summary>
        /// Get previous item to play.
        /// </summary>
        /// <param name="playlist">The play list.</param>
        /// <param name="itemIndex">The current item index. IPlaylist.Count indicates to retrieve the last item for play.</param>
        /// <returns>The previous item index to play. -1 reaches the beginning.</returns>
        int PreviousItem(IPlaylist playlist, int itemIndex);
    }
}
