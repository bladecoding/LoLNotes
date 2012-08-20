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
    /// Interface for generating filenames for streams.
    /// </summary>
	[CLSCompliant(false)]
    public interface IStreamFilenameGenerator : IScopeService
    {
        /// <summary>
        /// Generate a filename without an extension.
        /// </summary>
        /// <param name="scope">Scope to use.</param>
        /// <param name="name">Stream name.</param>
        /// <param name="type">Generation strategy (either playback or record).</param>
        /// <returns>Filename.</returns>
        string GenerateFilename(IScope scope, string name, GenerationType type);
        /// <summary>
        /// Generate a filename with an extension.
        /// </summary>
        /// <param name="scope">Scope to use.</param>
        /// <param name="name">Stream name.</param>
        /// <param name="extension">Extension.</param>
        /// <param name="type">Generation strategy (either playback or record).</param>
        /// <returns>Filename with extension.</returns>
        string GenerateFilename(IScope scope, string name, string extension, GenerationType type);
        /// <summary>
        /// Gets whether generated filename is an absolute path.
        /// </summary>
        /// <remarks>
        /// True if returned filename is an absolute path, else relative to application.
        /// If relative to application, you need to use
        /// <code>scope.Context.GetResources(fileName)[0].File</code> to resolve this to a file.
        /// 
        /// If absolute (ie returns true) simply use <code>new FileInfo(GenerateFilename(scope, name))</code>
        /// </remarks>
        bool ResolvesToAbsolutePath { get; }
    }
}
