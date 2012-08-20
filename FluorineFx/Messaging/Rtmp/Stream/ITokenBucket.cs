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
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Token bucket is used to control the bandwidth used by a stream or a connection or a client.
    /// There's a background thread that distributes tokens to the buckets in the system according
    /// to the configuration of the bucket. The configuration includes how fast the tokens are distributed.
    /// When a stream, for example, needs to send out a packet, the packet's byte count is calculated and
    /// each byte corresponds to a token in the bucket. The stream is assigned a bucket and the tokens in
    /// the bucket are acquired before the packet can be sent out. So if the speed(or bandwidth) in
    /// configuration is low, the stream can't send out packets fast. 
    /// </summary>
    public interface ITokenBucket
    {
        /// <summary>
        /// Acquire tokens amount of tokenCount. Waiting wait milliseconds if token not available.
        /// </summary>
        /// <param name="tokenCount">The count of tokens to acquire.</param>
        /// <param name="wait">Milliseconds to wait. 0 means no wait and any value below zero means wait forever. </param>
        /// <returns>true if successfully acquired or false otherwise.</returns>
        bool AcquireToken(long tokenCount, long wait);
        /// <summary>
        /// Nonblockingly acquire token. If the token is not available the callback will be executed when the token
        /// is available. The tokens are not consumed automatically before callback,
        /// so it's recommended to acquire token again in callback function.
        /// </summary>
        /// <param name="tokenCount">Number of tokens.</param>
        /// <param name="callback">Callback.</param>
        /// <returns>true if successfully acquired or false otherwise.</returns>
        bool AcquireTokenNonblocking(long tokenCount, ITokenBucketCallback callback);
        /// <summary>
        /// Nonblockingly acquire token. The upper limit is specified. If
        /// not enough tokens are left in bucket, all remaining will be returned.
        /// </summary>
        /// <param name="upperLimitCount">Upper limit of aquisition.</param>
        /// <returns>Remaining tokens from bucket.</returns>
        long AcquireTokenBestEffort(long upperLimitCount);
        /// <summary>
        /// Gets the capacity of this bucket in bytes.
        /// </summary>
        /// <returns></returns>
        long Capacity { get; }
        /// <summary>
        /// Gets the amount of tokens increased per millisecond.
        /// </summary>
        double Speed { get; }
        /// <summary>
        /// Reset this token bucket. All pending threads are woken up with false
        /// returned for acquiring token and callback is removed without calling back.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Callback for tocket bucket.
    /// </summary>
    public interface ITokenBucketCallback
    {
        /// <summary>
        /// Being called when the tokens requested are available.
        /// </summary>
        /// <param name="bucket">Bucket.</param>
        /// <param name="tokenCount">Number of tokens.</param>
        void Available(ITokenBucket bucket, long tokenCount);
        /// <summary>
        /// Resets tokens in bucket.
        /// </summary>
        /// <param name="bucket">Bucket.</param>
        /// <param name="tokenCount">Number of tokens.</param>
        void Reset(ITokenBucket bucket, long tokenCount);
    }

}
