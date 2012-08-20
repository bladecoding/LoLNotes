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

namespace FluorineFx.Messaging.Api.SO
{
    /// <summary>
    /// Service that supports protecting access to shared objects.
    /// </summary>
	[CLSCompliant(false)]
    public interface ISharedObjectSecurityService : IService
    {
        /// <summary>
        /// Adds handler that protects shared objects.
        /// </summary>
        /// <param name="handler">The ISharedObjectSecurity handler.</param>
        void RegisterSharedObjectSecurity(ISharedObjectSecurity handler);
        /// <summary>
        /// Removes handler that protects shared objects.
        /// </summary>
        /// <param name="handler">The ISharedObjectSecurity handler.</param>
        void UnregisterSharedObjectSecurity(ISharedObjectSecurity handler);
        /// <summary>
        /// Gets the collection of security handlers that protect shared objects.
        /// </summary>
        /// <returns>Enumerator of ISharedObjectSecurity handlers.</returns>
        IEnumerator GetSharedObjectSecurity();
    }
}
