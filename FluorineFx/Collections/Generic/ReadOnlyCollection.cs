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
using System.Threading;

namespace FluorineFx.Collections.Generic
{
    /// <summary>
    /// Implements a strongly typed read-only collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlyCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        ICollection<T> _collection;

        /// <summary>
        /// Creates a ReadOnlyCollection wrapper for a specific collection.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        public ReadOnlyCollection(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            _collection = collection;
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public void Clear()
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_collection as IEnumerable).GetEnumerator();
        }

        #endregion
    }
}
