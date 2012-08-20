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
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public interface IStream
	{
        /// <summary>
        /// Gets the name of the stream. The name is unique across the server. This is
        /// just an id of the stream and NOT the name that is used at client side to
        /// subscribe to the stream. For that name, use IBroadcastStream.PublishedName.
        /// </summary>
		string Name{ get; }
        /// <summary>
        /// Gets the scope this stream is associated with.
        /// </summary>
		IScope Scope{ get; }
        /// <summary>
        /// Starts the stream.
        /// </summary>
		void Start();
        /// <summary>
        /// Stops the stream.
        /// </summary>
        void Stop();
        /// <summary>
        /// Closes the stream.
        /// </summary>
		void Close();
        /// <summary>
        /// Gets Codec info for a stream.
        /// </summary>
        IStreamCodecInfo CodecInfo { get; }
        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Gets the timestamp at which the stream was created.
        /// </summary>
        /// <value>The creation time.</value>
        long CreationTime { get; }
	}
}
