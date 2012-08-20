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
    /// A thread-safe version of IDictionary in which all operations that change the dictionary are implemented by 
    /// making a new copy of the underlying Hashtable.
    /// </summary>
    public class CopyOnWriteDictionary: IDictionary
    {
        Hashtable _dictionary;

        /// <summary>
        /// Initializes a new instance of CopyOnWriteDictionary.
        /// </summary>
        public CopyOnWriteDictionary()
        {
            _dictionary = new Hashtable();
        }
        /// <summary>
        /// Initializes a new, empty instance of the CopyOnWriteDictionary class using the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The approximate number of elements that the CopyOnWriteDictionary object can initially contain.</param>
        public CopyOnWriteDictionary(int capacity)
        {
            _dictionary = new Hashtable(capacity);
        }
        /// <summary>
        /// Initializes a new instance of the CopyOnWriteDictionary class by copying the elements from the specified dictionary to the new CopyOnWriteDictionary object. The new CopyOnWriteDictionary object has an initial capacity equal to the number of elements copied.
        /// </summary>
        /// <param name="d">The IDictionary object to copy to a new CopyOnWriteDictionary object.</param>
        public CopyOnWriteDictionary(IDictionary d)
        {
            _dictionary = new Hashtable(d);
        }
#if !(NET_1_1)
        /// <summary>
        /// Initializes a new, empty instance of the CopyOnWriteDictionary class using the default initial capacity and load factor, and the specified IEqualityComparer object.
        /// </summary>
        /// <param name="equalityComparer">The IEqualityComparer object that defines the hash code provider and the comparer to use with the CopyOnWriteDictionary object.</param>
        public CopyOnWriteDictionary(IEqualityComparer equalityComparer)
        {
            _dictionary = new Hashtable(equalityComparer);
        }
#endif

        #region IDictionary Members
        /// <summary>
        /// Adds an element with the specified key and value into the CopyOnWriteDictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null reference (Nothing in Visual Basic).</param>
        public void Add(object key, object value)
        {
            lock (this.SyncRoot)
            {
                Hashtable dictionary = new Hashtable(_dictionary);
                dictionary.Add(key, value);
                _dictionary = dictionary;
            }
        }
        /// <summary>
        /// Removes all elements from the CopyOnWriteDictionary.
        /// </summary>
        public void Clear()
        {
            lock (this.SyncRoot)
            {
                _dictionary = new Hashtable();
            }
        }
        /// <summary>
        /// Determines whether the CopyOnWriteDictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the CopyOnWriteDictionary.</param>
        /// <returns>true if the CopyOnWriteDictionary contains an element with the specified key; otherwise, false.</returns>
        public bool Contains(object key)
        {
            return _dictionary.ContainsKey(key);
        }
        /// <summary>
        /// Returns an IDictionaryEnumerator that iterates through the CopyOnWriteDictionary.
        /// </summary>
        /// <returns>An IDictionaryEnumerator for the CopyOnWriteDictionary.</returns>
        public IDictionaryEnumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        /// <summary>
        /// Gets a value indicating whether the CopyOnWriteDictionary has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return _dictionary.IsFixedSize; }
        }
        /// <summary>
        /// Gets a value indicating whether the CopyOnWriteDictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }
        /// <summary>
        /// Gets an ICollection containing the keys in the CopyOnWriteDictionary.
        /// </summary>
        public ICollection Keys
        {
            get { return _dictionary.Keys; }
        }
        /// <summary>
        /// Removes the element with the specified key from the CopyOnWriteDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(object key)
        {
            lock (this.SyncRoot)
            {
                if (Contains(key))
                {
                    Hashtable dictionary = new Hashtable(_dictionary);
                    dictionary.Remove(key);
                    _dictionary = dictionary;
                }
            }
        }

        /// <summary>
        /// Removes and returns the item with the provided key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The item if the <see cref="T:CopyOnWriteDictionary"></see> contains an element with the key; otherwise, null.</returns>
        public object RemoveAndGet(object key)
        {
            lock (this.SyncRoot)
            {
                if (ContainsKey(key))
                {
                    Hashtable dictionary = new Hashtable(_dictionary);
                    dictionary.Remove(key);
                    object value = _dictionary[key];
                    _dictionary = dictionary;
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an ICollection containing the values in the CopyOnWriteDictionary.
        /// </summary>
        public ICollection Values
        {
            get { return _dictionary.Values; }
        }
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns nullNothingnullptra null reference (Nothing in Visual Basic), and attempting to set it creates a new element using the specified key.</returns>
        public object this[object key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    Hashtable dictionary = new Hashtable(_dictionary);
                    dictionary[key] = value;
                    _dictionary = dictionary;
                }
            }
        }

        #endregion

        #region ICollection Members
        /// <summary>
        /// Copies the CopyOnWriteDictionary elements to a one-dimensional Array instance at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the DictionaryEntry objects copied from CopyOnWriteDictionary. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            _dictionary.CopyTo(array, index);
        }
        /// <summary>
        /// Gets the number of key/value pairs contained in the CopyOnWriteDictionary.
        /// </summary>
        public int Count
        {
            get { return _dictionary.Count; }
        }
        /// <summary>
        /// Gets a value indicating whether access to the CopyOnWriteDictionary is synchronized (thread safe).
        /// </summary>
        /// <remarks>Always returns true.</remarks>
        public bool IsSynchronized
        {
            get { return true; }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the CopyOnWriteDictionary.
        /// </summary>
        public object SyncRoot
        {
            get { return _dictionary.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Determines whether the CopyOnWriteDictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the CopyOnWriteDictionary.</param>
        /// <returns>
        /// 	<c>true</c> if the CopyOnWriteDictionary contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(object key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Adds an item to the dictionary if this CopyOnWriteDictionary does not yet contain this item.
        /// </summary>
        ///<param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add. </param>
        ///<param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add. </param>
        /// <returns>The value if added, otherwise the old value in the dictionary.</returns>
        public object AddIfAbsent(object key, object value)
        {
            lock (SyncRoot)
            {
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, value);
                    return value;
                }
                else
                {
                    return _dictionary[key];
                }
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the CopyOnWriteDictionary contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(object key, out object value)
        {
            value = null;
            if (_dictionary.ContainsKey(key))
            {
                value = _dictionary[key];
                return true;
            }
            return false;
        }
    }
}
