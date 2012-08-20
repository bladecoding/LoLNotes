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
    /// A scope handler that is stream aware.
    /// </summary>
    [CLSCompliant(false)]
    public interface IStreamAwareScopeHandler : IScopeHandler
    {
        /// <summary>
        /// A broadcast stream starts being published. This will be called when the first video packet has been received.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamPublishStart(IBroadcastStream stream);
        /// <summary>
        /// A broadcast stream starts being recorded. This will be called when the first video packet has been received.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamRecordStart(IBroadcastStream stream);
        /// <summary>
        /// Notified when a broadcaster starts.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamBroadcastStart(IBroadcastStream stream);
        /// <summary>
        /// Notified when a broadcaster closes.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamBroadcastClose(IBroadcastStream stream);
        /// <summary>
        /// Notified when a subscriber starts.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamSubscriberStart(ISubscriberStream stream);
        /// <summary>
        /// Notified when a subscriber closes.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        void StreamSubscriberClose(ISubscriberStream stream);
        /// <summary>
        /// Notified when a playlist item plays.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        /// <param name="item">Playitem.</param>
        /// <param name="isLive"></param>
        void StreamPlaylistItemPlay(IPlaylistSubscriberStream stream, IPlayItem item, bool isLive);
        /// <summary>
        /// Notified when a playlist item stops.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="item"></param>
        void StreamPlaylistItemStop(IPlaylistSubscriberStream stream, IPlayItem item);
        /// <summary>
        /// Notified when a playlist vod item pauses.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="item"></param>
        /// <param name="position"></param>
        void StreamPlaylistVODItemPause(IPlaylistSubscriberStream stream, IPlayItem item, int position);
        /// <summary>
        /// Notified when a playlist vod item resumes.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="item"></param>
        /// <param name="position"></param>
        void StreamPlaylistVODItemResume(IPlaylistSubscriberStream stream, IPlayItem item, int position);
        /// <summary>
        /// Notified when a playlist vod item seeks.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="item"></param>
        /// <param name="position"></param>
        void StreamPlaylistVODItemSeek(IPlaylistSubscriberStream stream, IPlayItem item, int position);
    }
}
