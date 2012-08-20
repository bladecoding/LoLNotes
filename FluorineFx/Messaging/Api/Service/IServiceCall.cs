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
    /// Container for a Service Call.
	/// </summary>
    public interface IServiceCall
    {
        /// <summary>
        /// Gets a value indicating if the call was successful or not.
        /// </summary>
        bool IsSuccess { get;}
        /// <summary>
        /// Gets service method name.
        /// </summary>
        string ServiceMethodName { get;}
        /// <summary>
        /// Gets service name.
        /// </summary>
        string ServiceName { get;}
        /// <summary>
        /// Gets array of service method arguments.
        /// </summary>
        object[] Arguments { get;}
        /// <summary>
        /// Gets or sets service call status.
        /// </summary>
        byte Status { get;set;}
        /// <summary>
        /// Get or sets service call exception.
        /// </summary>
        Exception Exception { get;set;}
    }
}
