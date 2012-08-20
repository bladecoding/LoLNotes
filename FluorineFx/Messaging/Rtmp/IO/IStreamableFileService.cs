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
using System.IO;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Rtmp.IO
{
    /// <summary>
    /// Provides access to files that can be streamed.
    /// </summary>
    [CLSCompliant(false)]
    public interface IStreamableFileService
    {
        /// <summary>
        /// Gets prefix. Prefix is used in filename composition to fetch real file name.
        /// </summary>
        string Prefix { get; }
        /// <summary>
        /// Gets for extension of file
        /// </summary>
        string Extension { get; }
        /// <summary>
        /// Prepair given string to conform filename requirements, for example, add
        /// extension to the end if missing.
        /// </summary>
        /// <param name="name">String to format.</param>
        /// <returns>Filename.</returns>
        string PrepareFilename(string name);
        /// <summary>
        /// Checks whether file can be used by file service, that is, it does exist and have valid extension.
        /// </summary>
        /// <param name="file">FileInfo object.</param>
        /// <returns>true if file exist and has valid extension, false otherwise</returns>
        bool CanHandle(FileInfo file);
        /// <summary>
        /// Returns streamable file reference. For FLV files returned streamable file already has generated metadata injected.
        /// </summary>
        /// <param name="file">File resource.</param>
        /// <returns>Streamable file resource.s</returns>
        IStreamableFile GetStreamableFile(FileInfo file);
    }
}
