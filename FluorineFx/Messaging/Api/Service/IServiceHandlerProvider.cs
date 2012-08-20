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
using System.Collections.Generic;

namespace FluorineFx.Messaging.Api.Service
{
    /// <summary>
    /// Supports registration and lookup of service handlers.
    /// </summary>
    /// <example>
    /// If you registered a handler with the name "<code>one.two</code>" that
    /// provides a method "<code>callMe</code>", you can call the method with "<code>one.two.callMe</code>" from the client.
    /// </example>
    public interface IServiceHandlerProvider
    {
        /// <summary>
        /// Registers an object that provides methods which can be called from a client.
        /// </summary>
        /// <param name="name">The name of the handler.</param>
        /// <param name="handler">The handler object.</param>
        void RegisterServiceHandler(string name, object handler);
        /// <summary>
        /// Unregisters service handler.
        /// </summary>
        /// <param name="name">The name of the handler.</param>
        void UnregisterServiceHandler(string name);
        /// <summary>
        /// Returns a previously registered service handler.
        /// </summary>
        /// <param name="name">The name of the handler to return.</param>
        /// <returns>The previously registered handler.</returns>
        object GetServiceHandler(string name);
        /// <summary>
        /// Gets a list of registered service handler names.
        /// </summary>
        /// <returns>The names of the registered handlers.</returns>
        ICollection<String> GetServiceHandlerNames();
    }
}
