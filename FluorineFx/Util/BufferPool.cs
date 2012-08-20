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
namespace FluorineFx.Util
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class BufferPool : ObjectPool<byte[]>
    {
        private readonly int _bufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        public BufferPool()
            : this(10, 10, 4096)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer.</param>
        public BufferPool(int bufferSize)
            : this(10, 10, bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the object pool object initially contains.</param>
        /// <param name="growth">The number of elements reserved in the object pool when there are no available objects.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public BufferPool(int capacity, int growth, int bufferSize)
            : base(capacity, growth, true)
        {
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Creates a new buffer to be placed in the object pool.
        /// </summary>
        /// <returns>A new buffer.</returns>
        protected override byte[] GetObject()
        {
            return new byte[_bufferSize];
        }
    }
}