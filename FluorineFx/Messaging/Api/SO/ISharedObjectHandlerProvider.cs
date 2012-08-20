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
using FluorineFx.Messaging.Api.Service;

namespace FluorineFx.Messaging.Api.SO
{
    /// <summary>
    /// Supports registration and lookup of shared object handlers.
    /// <para>
    /// Shared object handlers will be called through remoteSO.send(<i>handler</i>, <i>args</i>) from a Flash client or the corresponding serverside call. The calls will be be mapped to shared object handler methods.
    /// </para>
    /// </summary>
    public interface ISharedObjectHandlerProvider : IServiceHandlerProvider
    {
        /// <summary>
        /// Registers an object that provides an unnamed shared object handler that will handle calls.
        /// </summary>
        /// <param name="handler">The handler object.</param>
        void RegisterServiceHandler(Object handler);
        /// <summary>
        /// Unregisters a shared object handler without a service name.
        /// </summary>
        void UnregisterServiceHandler();
    }
}
