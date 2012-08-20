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
using FluorineFx.Util;

namespace FluorineFx.IO
{
    /// <summary>
    /// Writes tags to FLV file.
    /// </summary>
    [CLSCompliant(false)]
    public interface ITagWriter
    {
        /// <summary>
        /// Gets the file that is loaded.
        /// </summary>
        IStreamableFile File { get; }
        /// <summary>
        /// Gets the position.
        /// </summary>
        long Position { get; }
        /// <summary>
        /// Gets the amount of bytes written.
        /// </summary>
        long BytesWritten { get; }
        /// <summary>
        /// Writes the header bytes.
        /// </summary>
        void WriteHeader();
        /// <summary>
        /// Writes a Tag object.
        /// </summary>
        /// <param name="tag">Tag to write.</param>
        /// <returns>true on success, false otherwise.</returns>
        bool WriteTag(ITag tag);
        /// <summary>
        /// Write a Tag using bytes.
        /// </summary>
        /// <param name="type">Tag type.</param>
        /// <param name="data">Byte data.</param>
        /// <returns>true on success, false otherwise.</returns>
        bool WriteTag(byte type, ByteBuffer data);
        /// <summary>
        /// Write a Stream to disk using bytes.
        /// </summary>
        /// <param name="buffer">Array of bytes to write.</param>
        /// <returns>true on success, false otherwise.</returns>
        bool WriteStream(byte[] buffer);
        /// <summary>
        /// Closes a writer.
        /// </summary>
        void Close();
    }
}
