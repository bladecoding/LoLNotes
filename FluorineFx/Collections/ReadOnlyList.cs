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
using System.Threading;

namespace FluorineFx.Collections
{
    /// <summary>
    /// Implements a read-only collection.
    /// </summary>
    public class ReadOnlyList : IList, ICollection, IEnumerable
    {
        IList _list;
        object _syncRoot;

        /// <summary>
        /// Creates a ReadOnlyList wrapper for a specific IList.
        /// </summary>
        /// <param name="list">The IList to wrap.</param>
        public ReadOnlyList(IList list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            _list = list;
        }
 

        #region IList Members
        /// <summary>
        /// Not supported. Throws NotSupportedException.
        /// </summary>
        /// <param name="value">The Object to add to the ReadOnlyCollection.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not supported. Throws NotSupportedException.
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Determines whether the ReadOnlyCollection contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the ReadOnlyCollection.</param>
        /// <returns>true if the Object is found in the ReadOnlyCollection; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return _list.Contains(value);
        }
        /// <summary>
        /// Determines the index of a specific item in the ReadOnlyCollection. 
        /// </summary>
        /// <param name="value">The Object to locate in the ReadOnlyCollection.</param>
        /// <returns>The index of value if found in the ReadOnlyCollection; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return _list.IndexOf(value);
        }
        /// <summary>
        /// Not supported. Throws NotSupportedException.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the ReadOnlyCollection.</param>
        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets a value indicating whether the ReadOnlyCollection has a fixed size. Always return <b>true</b>.
        /// </summary>
        public bool IsFixedSize
        {
            get { return true; }
        }
        /// <summary>
        /// Gets a value indicating whether the ReadOnlyCollection is read-only. Always return <b>true</b>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }
        /// <summary>
        /// Not supported. Throws NotSupportedException.
        /// </summary>
        /// <param name="value">The Object to remove from the ReadOnlyCollection.</param>
        public void Remove(object value)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Not supported. Throws NotSupportedException.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets the element at the specified index.
        /// Setting this property is not supported. Throws NotSupportedException.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public object this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ReadOnlyCollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ReadOnlyCollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }
        /// <summary>
        /// Gets the number of elements contained in the ReadOnlyCollection.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }
        /// <summary>
        /// Gets a value indicating whether access to the ReadOnlyCollection is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the ReadOnlyCollection.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    ICollection list = _list as ICollection;
                    if (_list != null)
                        _syncRoot = _list.SyncRoot;
                    else
                        Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through an ReadOnlyCollection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
