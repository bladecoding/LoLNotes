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
using FluorineFx.IO;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;

namespace FluorineFx.IO
{
    /// <summary>
    /// Interface represents streamable file with tag reader and writers (one for plain mode and one for append).
    /// </summary>
    [CLSCompliant(false)]
    public interface IStreamableFile
    {
        /// <summary>
        /// Returns a reader to parse and read the tags inside the file.
        /// </summary>
        /// <returns>Tag reader.</returns>
        ITagReader GetReader();
        /// <summary>
        /// Returns a writer that creates a new file or truncates existing contents. 
        /// </summary>
        /// <returns>Tag writer.</returns>
        ITagWriter GetWriter();
        /// <summary>
        /// Returns a Writer which is setup to append to the file.
        /// </summary>
        /// <returns>Tag writer used for append mode.</returns>
        ITagWriter GetAppendWriter();
    }
}
