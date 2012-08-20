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
    /// Interface for handlers that control access to shared objects.
    /// </summary>
	[CLSCompliant(false)]
    public interface ISharedObjectSecurity
    {
        /// <summary>
        /// Checks if the a shared object may be created in the given scope.
        /// </summary>
        /// <param name="scope">The scope object.</param>
        /// <param name="name">The shared object name.</param>
        /// <param name="persistent">Is persistent shared object.</param>
        /// <returns>true to allow, false to deny.</returns>
        bool IsCreationAllowed(IScope scope, String name, bool persistent);
        /// <summary>
        /// Checks if a connection to the given existing shared object is allowed.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <returns>true to allow, false to deny.</returns>
        bool IsConnectionAllowed(ISharedObject so);
        /// <summary>
        /// Checks if a modification is allowed on the given shared object.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="key">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns>true to allow, false to deny.</returns>
        bool IsWriteAllowed(ISharedObject so, String key, Object value);
        /// <summary>
        /// Checks if the deletion of a property is allowed on the given shared object.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="key">The property.</param>
        /// <returns>true to allow, false to deny.</returns>
        bool IsDeleteAllowed(ISharedObject so, String key);
        /// <summary>
        /// Checks if sending a message to the shared object is allowed.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="message">The message.</param>
        /// <param name="arguments">Arguments.</param>
        /// <returns>true to allow, false to deny.</returns>
        bool IsSendAllowed(ISharedObject so, String message, IList arguments);
    }
}
