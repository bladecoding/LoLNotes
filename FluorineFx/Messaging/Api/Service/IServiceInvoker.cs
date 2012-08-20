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
    /// Interface for objects that execute service calls (remote calls from client).
	/// </summary>
	[CLSCompliant(false)]
	public interface IServiceInvoker
	{
		/// <summary>
		/// Execute the passed service call in the given scope.  This looks up the
		/// handler for the call in the scope and the context of the scope.
		/// </summary>
        /// <param name="call">The call to invoke.</param>
        /// <param name="scope">The scope to search for a handler.</param>
        /// <returns><code>true</code> if the call was performed, otherwise <code>false</code>.</returns>
        bool Invoke(IServiceCall call, IScope scope);
		/// <summary>
		/// Execute the passed service call in the given object.
		/// </summary>
        /// <param name="call">The call to invoke.</param>
        /// <param name="service">The service to use.</param>
        /// <returns><code>true</code> if the call was performed, otherwise <code>false</code>.</returns>
        bool Invoke(IServiceCall call, object service);
	}
}
