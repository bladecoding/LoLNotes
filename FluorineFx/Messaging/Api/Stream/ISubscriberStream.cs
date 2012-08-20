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
    /// ISubscriberStream is a stream from subscriber's point of view. That is, it provides methods for common stream operations like play, pause or seek.
    /// </summary>
    [CLSCompliant(false)]
    public interface ISubscriberStream : IClientStream
    {
        /// <summary>
        /// Start playing.
        /// </summary>
        void Play();
        /// <summary>
        /// Pause at a position for current playing item.
        /// </summary>
        /// <param name="position">Position for pause in millisecond.</param>
        void Pause(int position);
        /// <summary>
        /// Resume from a position for current playing item.
        /// </summary>
        /// <param name="position">Position for resume in millisecond.</param>
        void Resume(int position);
        /// <summary>
        /// Seek into a position for current playing item.
        /// </summary>
        /// <param name="position">Position for seek in millisecond.</param>
        void Seek(int position);
        /// <summary>
        /// Gets whether the stream is currently paused.
        /// </summary>
        bool IsPaused { get; }
        /// <summary>
        /// Should the stream send video to the client.
        /// </summary>
        /// <param name="receive"></param>
        void ReceiveVideo(bool receive);
        /// <summary>
        /// Should the stream send audio to the client.
        /// </summary>
        /// <param name="receive"></param>
        void ReceiveAudio(bool receive);
    }
}
