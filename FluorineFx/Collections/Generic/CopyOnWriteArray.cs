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
#if !(NET_1_1)
using System.Collections.Generic;
#endif

namespace FluorineFx.Collections.Generic
{
    /// <summary>
    /// A thread-safe variant of ArrayList in which all mutative operations (add, set, and so on) are implemented by making a fresh
    /// copy of the underlying array.
    /// 
    /// This is ordinarily too costly, but may be more efficient than alternatives when traversal operations vastly outnumber
    /// mutations, and is useful when you cannot or don't want to synchronize traversals, yet need to preclude interference among
    /// concurrent threads.  The "snapshot" style iterator method uses a reference to the state of the array at the point that the iterator
    /// was created. This array never changes during the lifetime of the iterator, so interference is impossible.
    /// 
    /// The iterator will not reflect additions, removals, or changes to the list since the iterator was created.
    /// </summary>
    public class CopyOnWriteArray<T> : IList<T>, IList,
        ICollection<T>, ICollection,
        IEnumerable<T>, IEnumerable
    {
        T[] _array;
        private object _objLock = new object();

        /// <summary>
        /// Initializes a new instance of the CopyOnWriteArray class.
        /// </summary>
        public CopyOnWriteArray()
        {
            _array = new T[0];
        }

        /// <summary>
        /// Creates a CopyOnWriteArray wrapper for a specific IList.
        /// </summary>
        /// <param name="list">The IList to wrap.</param>
        public CopyOnWriteArray(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            _array = new T[list.Count];
            int i = 0;
            foreach (T element in list)
                _array[i++] = element;
        }

        private void Copy(T[] src, int offset, int count)
        {
            lock (this.SyncRoot)
            {
                _array = new T[count];
                Array.Copy(src, offset, _array, 0, count);
            }
        }

        /// <summary>
        /// Static version allows repeated call without needed to grab lock for array each time.
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="elementData"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int IndexOf(T elem, T[] elementData, int length)
        {
            if (elem == null)
            {
                for (int i = 0; i < length; i++)
                    if (elementData[i] == null)
                        return i;
            }
            else
            {
                for (int i = 0; i < length; i++)
                    if (elem.Equals(elementData[i]))
                        return i;
            }
            return -1;
        }

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item in the CopyOnWriteArray. 
        /// </summary>
        /// <param name="item">The Object to locate in the CopyOnWriteArray.</param>
        /// <returns>The index of value if found in the CopyOnWriteArray; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            T[] elementData = _array;
            int length = elementData.Length;
            return IndexOf(item, elementData, length);
        }
        /// <summary>
        /// Inserts an element into the CopyOnWriteArray at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="item">The Object to insert into the CopyOnWriteArray.</param>
        public void Insert(int index, T item)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                T[] newArray = new T[len + 1];
                Array.Copy(_array, 0, newArray, 0, index);
                newArray[index] = item;
                Array.Copy(_array, index, newArray, index + 1, len - index);
                _array = newArray;
            }
        }
        /// <summary>
        /// Removes the element at the specified index of the CopyOnWriteArray.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                T oldValue = _array[index];
                T[] newArray = new T[len - 1];
                Array.Copy(_array, 0, newArray, 0, index);
                int numMoved = len - index - 1;
                if (numMoved > 0)
                    Array.Copy(_array, index + 1, newArray, index, numMoved);
                _array = newArray;
            }
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                return _array[index];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    T oldValue = _array[index];
                    bool isSame = (value != null && value.Equals(oldValue));
                    if (!isSame)
                    {
                        T[] newArray = new T[_array.Length];
                        Array.Copy(_array, 0, newArray, 0, _array.Length);
                        newArray[index] = value;
                        _array = newArray;
                    }
                }
            }
        }

        #endregion

        #region ICollection<T> Members
        /// <summary>
        /// Adds an object to the end of the CopyOnWriteArray. 
        /// </summary>
        /// <param name="item">The Object to add to the CopyOnWriteArray.</param>
        public void Add(T item)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                T[] newArray = new T[len + 1];
                Array.Copy(_array, 0, newArray, 0, len);
                newArray[len] = item;
                _array = newArray;
            }
        }
        /// <summary>
        /// Removes all elements from the CopyOnWriteArray.
        /// </summary>
        public void Clear()
        {
            lock (this.SyncRoot)
            {
                _array = new T[0];
            }
        }
        /// <summary>
        /// Determines whether the CopyOnWriteArray contains a specific value.
        /// </summary>
        /// <param name="item">The Object to locate in the CopyOnWriteArray.</param>
        /// <returns>true if the Object is found in the CopyOnWriteArray; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }
        /// <summary>
        /// Copies the elements of the CopyOnWriteArray to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CopyOnWriteArray. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index)
        {
            Array.Copy(_array, 0, array, index, array.Length - index);
        }
        /// <summary>
        /// Gets the number of elements contained in the CopyOnWriteArray.
        /// </summary>
        public int Count
        {
            get { return _array.Length; }
        }
        /// <summary>
        /// Gets a value indicating whether the CopyOnWriteArray is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the CopyOnWriteArray.
        /// </summary>
        /// <param name="item">The Object to remove from the CopyOnWriteArray.</param>
        public bool Remove(T item)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                if (len == 0)
                    return false;
                // Copy while searching for element to remove
                // This wins in the normal case of element being present
                int newlen = len - 1;
                T[] newArray = new T[newlen];

                for (int i = 0; i < newlen; ++i)
                {
                    if (item.Equals(_array[i]))
                    {
                        // found one;  copy remaining and exit
                        for (int k = i + 1; k < len; ++k)
                            newArray[k - 1] = _array[k];
                        _array = newArray;
                        return true;
                    }
                    else
                        newArray[i] = _array[i];
                }
                // special handling for last cell
                if (item.Equals(_array[newlen]))
                {
                    _array = newArray;
                }
                return true;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        internal struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private T[] list;
            private int index;
            private T current;
            internal Enumerator(T[] list)
            {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < list.Length)
                {
                    current = list[index];
                    index++;
                    return true;
                }
                index = list.Length + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((index == 0) || (index == (list.Length + 1)))
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through an CopyOnWriteArray.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the CopyOnWriteArray.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_array);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        #endregion

        #region IList Members

        /// <summary>
        /// Adds an object to the end of the CopyOnWriteArray. 
        /// </summary>
        /// <param name="value">The Object to add to the CopyOnWriteArray.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                T[] newArray = new T[len + 1];
                Array.Copy(_array, 0, newArray, 0, len);
                newArray[len] = (T)value;
                _array = newArray;
                return len;
            }
        }
        /// <summary>
        /// Determines whether the CopyOnWriteArray contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the CopyOnWriteArray.</param>
        /// <returns>true if the Object is found in the CopyOnWriteArray; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return IndexOf(value) > -1;
        }
        /// <summary>
        /// Determines the index of a specific item in the CopyOnWriteArray. 
        /// </summary>
        /// <param name="value">The Object to locate in the CopyOnWriteArray.</param>
        /// <returns>The index of value if found in the CopyOnWriteArray; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            T[] elementData = _array;
            int length = elementData.Length;
            return IndexOf((T)value, elementData, length);
        }
        /// <summary>
        /// Inserts an element into the CopyOnWriteArray at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the CopyOnWriteArray.</param>
        public void Insert(int index, object value)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                T[] newArray = new T[len + 1];
                Array.Copy(_array, 0, newArray, 0, index);
                newArray[index] = (T)value;
                Array.Copy(_array, index, newArray, index + 1, len - index);
                _array = newArray;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the CopyOnWriteArray has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the CopyOnWriteArray.
        /// </summary>
        /// <param name="value">The Object to remove from the CopyOnWriteArray.</param>
        public void Remove(object value)
        {
            lock (this.SyncRoot)
            {
                int len = _array.Length;
                if (len == 0)
                    return;
                // Copy while searching for element to remove
                // This wins in the normal case of element being present
                int newlen = len - 1;
                T[] newArray = new T[newlen];

                for (int i = 0; i < newlen; ++i)
                {
                    if (value.Equals(_array[i]))
                    {
                        // found one;  copy remaining and exit
                        for (int k = i + 1; k < len; ++k)
                            newArray[k - 1] = _array[k];
                        _array = newArray;
                        return;
                    }
                    else
                        newArray[i] = _array[i];
                }
                // special handling for last cell
                if (value.Equals(_array[newlen]))
                {
                    _array = newArray;
                }
            }
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        object IList.this[int index]
        {
            get
            {
                return _array[index];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    T oldValue = _array[index];
                    bool isSame = value.Equals(oldValue);
                    if (!isSame)
                    {
                        T[] newArray = new T[_array.Length];
                        Array.Copy(_array, 0, newArray, 0, _array.Length);
                        newArray[index] = (T)value;
                        _array = newArray;
                    }
                }
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the CopyOnWriteArray to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CopyOnWriteArray. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            Array.Copy(_array, 0, array, index, array.Length - index);
        }
        /// <summary>
        /// Gets a value indicating whether access to the CopyOnWriteArray is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return true; }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the CopyOnWriteArray.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return _objLock;
            }
        }

        #endregion
    }
}
