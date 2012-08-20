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
using FluorineFx.Messaging.Api.Messaging;

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// A broadcast stream is a stream source to be subscribed by clients. To
    /// subscribe a stream from your client Flash application use NetStream.play
    /// method. Broadcast stream can be saved at server-side.
    /// </summary>
    [CLSCompliant(false)]
    public interface IBroadcastStream : IStream
    {
        /// <summary>
        /// Saves the broadcast stream as a file. 
        /// </summary>
        /// <param name="filePath">The path of the file relative to the scope.</param>
        /// <param name="isAppend">Whether to append to the end of file.</param>
	    void SaveAs(string filePath, bool isAppend);
        /// <summary>
        /// Gets the filename the stream is being saved as.
        /// </summary>
        /// <value>The filename relative to the scope or null if the stream is not being saved.</value>
        String SaveFilename { get; }
        /// <summary>
        /// Gets or sets stream publish name. Publish name is the value of the first parameter
        /// had been passed to <code>NetStream.publish</code> on client side in SWF.
        /// </summary>
        String PublishedName { get; set; }
        /// <summary>
        /// Gets the provider corresponding to this stream.
        /// </summary>
        IProvider Provider { get; }
        /// <summary>
        /// Add a listener to be notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        void AddStreamListener(IStreamListener listener);
        /// <summary>
        /// Remove a listener from being notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveStreamListener(IStreamListener listener);
        /// <summary>
        /// Return registered stream listeners.
        /// </summary>
        /// <returns>The registered listeners.</returns>
        ICollection GetStreamListeners();

    }
}
