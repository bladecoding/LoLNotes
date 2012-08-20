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
#if !NET_1_1
using System.Collections.Generic;
#endif
using System.IO;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Rtmp.IO
{
    /// <summary>
    /// Scope service extension that provides method to get streamable file services set.
    /// </summary>
    [CLSCompliant(false)]
    public interface IStreamableFileFactory : IScopeService
    {
        /// <summary>
        /// Returns a streamable file service.
        /// </summary>
        /// <param name="file">File to be streamed.</param>
        /// <returns>A streamable file service.</returns>
        IStreamableFileService GetService(FileInfo file);

        /// <summary>
        /// Returns streamable file services.
        /// </summary>
        /// <returns>Set of streamable file services.</returns>
#if !NET_1_1
        ICollection<IStreamableFileService> GetServices();
#else
        ICollection GetServices();
#endif
    }
}
