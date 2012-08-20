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

namespace FluorineFx.Messaging.Api.Service
{
	/// <summary>
    /// IPendingServiceCall is a service call with a list of callbacks.
	/// </summary>
	public interface IPendingServiceCall : IServiceCall
	{
        /// <summary>
        /// Gets or sets the service call result.
        /// </summary>
		object Result{get;set;}
        /// <summary>
        /// Registers callback object that implements IPendingServiceCallback interface.
        /// </summary>
        /// <param name="callback"></param>
		void RegisterCallback(IPendingServiceCallback callback);
        /// <summary>
        /// Unregisters callback object that implements IPendingServiceCallback interface.
        /// </summary>
        /// <param name="callback"></param>
		void UnregisterCallback(IPendingServiceCallback callback);
        /// <summary>
        /// Returns list of callback objects that implements IPendingServiceCallback.
        /// </summary>
        /// <returns></returns>
		IPendingServiceCallback[] GetCallbacks();
	}
}
