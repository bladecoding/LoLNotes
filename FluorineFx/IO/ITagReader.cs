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

namespace FluorineFx.IO
{
    /// <summary>
    /// Tag reader interface.
    /// </summary>
    [CLSCompliant(false)]
    public interface ITagReader
    {
        /// <summary>
        /// Gets the file that is loaded.
        /// </summary>
        IStreamableFile File { get; }
        /// <summary>
        /// Gets the offet length.
        /// </summary>
        int Offset { get; }
        /// <summary>
        /// Gets the amount of bytes read.
        /// </summary>
        long BytesRead { get; }
        /// <summary>
        /// Gets length in milliseconds.
        /// </summary>
        long Duration { get; }
        /// <summary>
        /// Decode the header of the stream.
        /// </summary>
        void DecodeHeader();
        /// <summary>
        /// Moves the reader pointer to given position in file.
        /// </summary>
        long Position { get; set; }
        /// <summary>
        /// Returns a boolean stating whether the FLV has more tags.
        /// </summary>
        /// <returns></returns>
        bool HasMoreTags();
        /// <summary>
        /// Returns a Tag object.
        /// </summary>
        /// <returns>Tag.</returns>
        ITag ReadTag();
        /// <summary>
        /// Closes the reader and free any allocated memory.
        /// </summary>
        void Close();
        /// <summary>
        /// Checks if the reader also has video tags.
        /// </summary>
        /// <returns></returns>
        bool HasVideo();
    }
}
