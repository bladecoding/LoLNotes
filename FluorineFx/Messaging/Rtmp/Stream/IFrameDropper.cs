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
using System.IO;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    enum FrameDropperState
    {
        /// <summary>
        /// Send keyframes, interframes and disposable interframes.
        /// </summary>
        SEND_ALL = 0,
        /// <summary>
        /// Send keyframes and interframes.
        /// </summary>
        SEND_INTERFRAMES = 1,
        /// <summary>
        /// Send keyframes only.
        /// </summary>
        SEND_KEYFRAMES = 2,
        /// <summary>
        /// Send keyframes only and switch to SEND_INTERFRAMES later.
        /// </summary>
        SEND_KEYFRAMES_CHECK = 3
    }

    /// <summary>
    /// Interface for classes that implement logic to drop frames.
    /// </summary>
    interface IFrameDropper
    {
        /// <summary>
        /// Checks if a message may be sent to the subscriber.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <param name="pending">The number of pending messages.</param>
        /// <returns><code>true</code> if the packet may be sent, otherwise <code>false</code>.</returns>
	    bool CanSendPacket(RtmpMessage message, long pending);
        /// <summary>
        /// Notify that a packet has been dropped.
        /// </summary>
        /// <param name="message">The message that was dropped.</param>
        void DropPacket(RtmpMessage message);
        /// <summary>
        /// Notify that a message has been sent.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        void SendPacket(RtmpMessage message);
	    /// <summary>
        /// Reset the frame dropper.
	    /// </summary>
	    void Reset();
        /// <summary>
        /// Reset the frame dropper to a given state.
        /// </summary>
        /// <param name="state">The state to reset the frame dropper to.</param>
        void Reset(FrameDropperState state);
    }
}
