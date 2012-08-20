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
    /// A thread-safe version of IDictionary in which all operations that change the dictionary are implemented by 
    /// making a new copy of the underlying Hashtable.
    /// </summary>
    public class CopyOnWriteDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection, IDictionary
    {
        Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of CopyOnWriteDictionary.
        /// </summary>
        public CopyOnWriteDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }
        /// <summary>
        /// Initializes a new, empty instance of the CopyOnWriteDictionary class using the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The approximate number of elements that the CopyOnWriteDictionary object can initially contain.</param>
        public CopyOnWriteDictionary(int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }
        /// <summary>
        /// Initializes a new instance of the CopyOnWriteDictionary class by copying the elements from the specified dictionary to the new CopyOnWriteDictionary object. The new CopyOnWriteDictionary object has an initial capacity equal to the number of elements copied.
        /// </summary>
        /// <param name="d">The IDictionary object to copy to a new CopyOnWriteDictionary object.</param>
        public CopyOnWriteDictionary(IDictionary<TKey, TValue> d)
        {
            _dictionary = new Dictionary<TKey, TValue>(d);
        }
#if !(NET_1_1)
        /// <summary>
        /// Initializes a new, empty instance of the CopyOnWriteDictionary class using the default initial capacity and load factor, and the specified IEqualityComparer object.
        /// </summary>
        /// <param name="equalityComparer">The IEqualityComparer object that defines the hash code provider and the comparer to use with the CopyOnWriteDictionary object.</param>
        public CopyOnWriteDictionary(IEqualityComparer<TKey> equalityComparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(equalityComparer);
        }
#endif

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds an element with the specified key and value into the CopyOnWriteDictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null reference (Nothing in Visual Basic).</param>
        public void Add(TKey key, TValue value)
        {
            lock (this.SyncRoot)
            {
                Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
                dictionary.Add(key, value);
                _dictionary = dictionary;
            }
        }
        /// <summary>
        /// Determines whether the CopyOnWriteDictionary contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the CopyOnWriteDictionary.</param>
        /// <returns>true if the CopyOnWriteDictionary contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:CopyOnWriteDictionary"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from the CopyOnWriteDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="key"/> is null.
        /// </exception>
        public bool Remove(TKey key)
        {
            lock (this.SyncRoot)
            {
                if (ContainsKey(key))
                {
                    Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
                    dictionary.Remove(key);
                    _dictionary = dictionary;
                    return true;
                }
            }
            return false;
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
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:CopyOnWriteDictionary"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value>The value associated with the specified key.</value>
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
                    dictionary[key] = value;
                    _dictionary = dictionary;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an item to the CopyOnWriteDictionary.
        /// </summary>
        /// <param name="item">The object to add to the CopyOnWriteDictionary.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        /// <summary>
        /// Removes all elements from the CopyOnWriteDictionary.
        /// </summary>
        public void Clear()
        {
            lock (this.SyncRoot)
            {
                _dictionary = new Dictionary<TKey, TValue>();
            }
        }
        /// <summary>
        /// Determines whether the CopyOnWriteDictionary contains a specific key.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the CopyOnWriteDictionary contains an element with the specified key; otherwise, false.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return (_dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:CopyOnWriteDictionary"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:CopyOnWriteDictionary"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.
        /// -or-
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source <see cref="T:CopyOnWriteDictionary"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// -or-
        /// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (_dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:CopyOnWriteDictionary"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:CopyOnWriteDictionary"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:CopyOnWriteDictionary"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return (_dictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:CopyOnWriteDictionary"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:CopyOnWriteDictionary"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:CopyOnWriteDictionary"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:CopyOnWriteDictionary"/> is read-only.
        /// </exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Returns an enumerator that iterates through the CopyOnWriteDictionary.
        /// </summary>
        /// <returns>An enumerator for the CopyOnWriteDictionary.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is less than zero.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.
        /// -or-
        /// <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(Array array, int index)
        {
            (_dictionary as ICollection).CopyTo(array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:CopyOnWriteDictionary"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:CopyOnWriteDictionary"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:CopyOnWriteDictionary"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:CopyOnWriteDictionary"/>.
        /// </returns>
        public object SyncRoot
        {
            get { return (_dictionary as ICollection).SyncRoot; }
        }

        #endregion

        /// <summary>
        /// Adds an item to the dictionary if this CopyOnWriteDictionary does not yet contain this item.
        /// </summary>
        ///<param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add. </param>
        ///<param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add. </param>
        /// <returns>The value if added, otherwise the old value in the dictionary.</returns>
        public TValue AddIfAbsent(TKey key, TValue value)
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
        /// Removes and returns the item with the provided key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The item if the <see cref="T:CopyOnWriteDictionary"></see> contains an element with the key; otherwise, null.</returns>
        public object RemoveAndGet(TKey key)
        {
            lock (this.SyncRoot)
            {
                if (ContainsKey(key))
                {
                    Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
                    dictionary.Remove(key);
                    object value = _dictionary[key];
                    _dictionary = dictionary;
                    return value;
                }
            }
            return null;
        }

        #region IDictionary Members

        ///<summary>
        ///Adds an element with the provided key and value to the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</summary>
        ///<param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add. </param>
        ///<param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add. </param>
        ///<exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:CopyOnWriteDictionary"></see> object. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:CopyOnWriteDictionary"></see> is read-only.-or- The <see cref="T:CopyOnWriteDictionary"></see> has a fixed size. </exception><filterpriority>2</filterpriority>
        public void Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }
        ///<summary>
        ///Determines whether the <see cref="T:CopyOnWriteDictionary"></see> object contains an element with the specified key.
        ///</summary>
        ///<returns>
        ///true if the <see cref="T:CopyOnWriteDictionary"></see> contains an element with the key; otherwise, false.
        ///</returns>
        ///<param name="key">The key to locate in the <see cref="T:CopyOnWriteDictionary"></see> object.</param>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public bool Contains(object key)
        {
            return ContainsKey((TKey)key);
        }
        ///<summary>
        ///Returns an <see cref="T:CopyOnWriteDictionaryEnumerator"></see> object for the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:CopyOnWriteDictionaryEnumerator"></see> object for the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (_dictionary as IDictionary).GetEnumerator();
        }
        ///<summary>
        ///Gets a value indicating whether the <see cref="T:CopyOnWriteDictionary"></see> object has a fixed size.
        ///</summary>
        ///<returns>
        ///true if the <see cref="T:CopyOnWriteDictionary"></see> object has a fixed size; otherwise, false.
        ///</returns>
        public bool IsFixedSize
        {
            get { return false; }
        }
        ///<summary>
        ///Gets an <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</returns>
        ICollection IDictionary.Keys
        {
            get { return (_dictionary as IDictionary).Keys; }
        }
        ///<summary>
        ///Removes the element with the specified key from the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</summary>
        ///<param name="key">The key of the element to remove. </param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:CopyOnWriteDictionary"></see> object is read-only.-or- The <see cref="T:CopyOnWriteDictionary"></see> has a fixed size. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public void Remove(object key)
        {
            Remove((TKey)key);
        }
        ///<summary>
        ///Gets an <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:CopyOnWriteDictionary"></see> object.
        ///</returns>
        ICollection IDictionary.Values
        {
            get { return (_dictionary as IDictionary).Values; }
        }
        ///<summary>
        ///Gets or sets the element with the specified key.
        ///</summary>
        ///<returns>
        ///The element with the specified key.
        ///</returns>
        ///<param name="key">The key of the element to get or set. </param>
        ///<exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:CopyOnWriteDictionary"></see> object is read-only.-or- The property is set, key does not exist in the collection, and the <see cref="T:CopyOnWriteDictionary"></see> has a fixed size. </exception>
        ///<exception cref="T:System.ArgumentNullException">key is null. </exception><filterpriority>2</filterpriority>
        public object this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        #endregion
    }
}
