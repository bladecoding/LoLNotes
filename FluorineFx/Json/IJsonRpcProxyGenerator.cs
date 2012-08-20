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
using System.Web;
using FluorineFx.Util;
using FluorineFx.Json.Services;

namespace FluorineFx.Json
{
    /// <summary>
    /// Json-Rpc proxy code generator interface.
    /// </summary>
    public interface IJsonRpcProxyGenerator
    {
        /// <summary>
        /// Generates Json-Rpc Proxy.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="writer"></param>
        /// <param name="request"></param>
        /// <remarks>
        /// A proxy must post back to request.Url
        /// </remarks>
        void WriteProxy(ServiceClass service, IndentedTextWriter writer, HttpRequest request);
    }
}
