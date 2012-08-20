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
    /// 	<para>Bandwidth controller service interface.</para>
    /// 	<para>The bandwidth controllable is registered in the bandwidth controller which
    ///     provides the three token buckets used for bandwidth control.</para>
    /// 	<para>The bandwidth controller manages the token buckets assigned to the bandwidth
    ///     controllable and distributes the tokens to the buckets in an
    ///     implementation-specific way. (eg timely distribute the tokens according to the
    ///     bandwidth config of the controllable).</para>
    /// </summary>
    public interface IBWControlService : IService
    {
        /// <summary>
        /// Register a bandwidth controllable. The necessary resources will be allocated and assigned to the controllable.
        /// </summary>
        /// <param name="bc">The bandwidth controllable.</param>
        /// <returns>The registry context. It's used in the subsequent calls to controller's method.</returns>
        IBWControlContext RegisterBWControllable(IBWControllable bc);
        /// <summary>
        /// Unregister the bandwidth controllable. The resources that were allocated will be freed.
        /// </summary>
        /// <param name="context">The registry context.</param>
        void UnregisterBWControllable(IBWControlContext context);
        /// <summary>
        /// Lookup the registry context according to the controllable.
        /// </summary>
        /// <param name="bc">The bandwidth controllable.</param>
        /// <returns>The registry context.</returns>
        IBWControlContext LookupContext(IBWControllable bc);
        /// <summary>
        /// Update the bandwidth configuration of a controllable.
        /// Each time when the controllable changes the bandwidth config
        /// and wants to make the changes take effect, this method should be called.
        /// </summary>
        /// <param name="context">The registry context.</param>
        void UpdateBWConfigure(IBWControlContext context);
        /// <summary>
        /// Reset all the token buckets for a controllable. All the callback
        /// will be reset and all blocked threads will be woken up.
        /// </summary>
        /// <param name="context">The registry context.</param>
        void ResetBuckets(IBWControlContext context);
        /// <summary>
        /// Return the token bucket for audio channel.
        /// </summary>
        /// <param name="context">The registry context.</param>
        /// <returns>Token bucket for audio channel.</returns>
        ITokenBucket GetAudioBucket(IBWControlContext context);
        /// <summary>
        /// Return the token bucket for video channel.
        /// </summary>
        /// <param name="context">The registry context.</param>
        /// <returns>Token bucket for video channel.</returns>
        ITokenBucket GetVideoBucket(IBWControlContext context);
        /// <summary>
        /// Return the token bucket for data channel.
        /// </summary>
        /// <param name="context">The registry context.</param>
        /// <returns>Token bucket for data channel.</returns>
        ITokenBucket GetDataBucket(IBWControlContext context);
    }
}
