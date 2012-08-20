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

namespace FluorineFx.Messaging.Api.Stream
{
    /// <summary>
    /// Interface for handlers that control access to stream publishing.
    /// </summary>
	[CLSCompliant(false)]
    public interface IStreamPublishSecurity
    {
        /// <summary>
        /// Check if publishing a stream with the given name is allowed.
        /// </summary>
        /// <param name="scope">Scope the stream is about to be published in.</param>
        /// <param name="name">Name of the stream to publish.</param>
        /// <param name="mode">Publishing mode.</param>
        /// <returns>true if publishing is allowed, otherwise false.</returns>
        bool IsPublishAllowed(IScope scope, string name, string mode);
    }
}
