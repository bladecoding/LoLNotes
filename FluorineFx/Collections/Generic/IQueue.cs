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
using System.Collections.Generic;

namespace FluorineFx.Collections.Generic
{
    /// <summary>
    /// A variable size first-in-first-out (FIFO) collection of instances of the same type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueue<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        /// <summary>
        /// Removes an item from the queue.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        T Dequeue();
        /// <summary>
        /// Inserts the specified element at the tail of this queue.
        /// </summary>
        /// <param name="item">The item to insert in the queue.</param>
        void Enqueue(T item);
        /// <summary>
        /// Removes all elements from the queue.
        /// </summary>
        void Clear();
    }
}
