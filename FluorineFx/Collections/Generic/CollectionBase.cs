using System;
using System.Collections;
using System.Collections.Generic;

#if !(NET_1_1)

namespace FluorineFx.Collections.Generic
{
    /// <summary>
    /// Provides a base class for a strongly typed collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionBase<T> :
        IList<T>, IList,
        ICollection<T>, ICollection,
        IEnumerable<T>, IEnumerable
    {

        private List<T> _innerList;

        /// <summary>
        /// Initializes a new instance of the CollectionBase class with the default initial capacity.
        /// </summary>
        public CollectionBase() : this(10) { }
        /// <summary>
        /// Initializes a new instance of the CollectionBase class with the specified capacity.
        /// </summary>
        /// <param name="initialCapacity">The number of elements that the new list can initially store.</param>
        public CollectionBase(int initialCapacity)
        {
            _innerList = new List<T>(initialCapacity);
        }
        /// <summary>
        /// Removes all objects from the CollectionBase instance.
        /// </summary>
        public virtual void Clear()
        {
            if (!this.OnClear()) { return; }
            this._innerList.Clear();
            this.OnClearComplete();
        }

        #region Notification Events

        /// <summary>
        /// Performs additional custom processes when clearing the contents of the CollectionBase instance.
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnClear()
        {
            return true;
        }
        /// <summary>
        /// Performs additional custom processes after clearing the contents of the CollectionBase instance.
        /// </summary>
        protected virtual void OnClearComplete()
        {
        }
        /// <summary>
        /// Performs additional custom processes before inserting a new element into the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert value.</param>
        /// <param name="value">The new value of the element at index.</param>
        /// <returns></returns>
        protected virtual bool OnInsert(int index, T value)
        {
            return true;
        }
        /// <summary>
        /// Performs additional custom processes after inserting a new element into the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert value.</param>
        /// <param name="value">The new value of the element at index.</param>
        protected virtual void OnInsertComplete(int index, T value)
        {
        }
        /// <summary>
        /// Performs additional custom processes when removing an element from the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which value can be found.</param>
        /// <param name="value">The value of the element to remove from index.</param>
        /// <returns></returns>
        protected virtual bool OnRemove(int index, T value)
        {

            return true;
        }
        /// <summary>
        /// Performs additional custom processes after removing an element from the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which value can be found.</param>
        /// <param name="value">The value of the element to remove from index.</param>
        protected virtual void OnRemoveComplete(int index, T value)
        {
        }
        /// <summary>
        /// Performs additional custom processes before setting a value in the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which oldValue can be found.</param>
        /// <param name="oldValue">The value to replace with newValue.</param>
        /// <param name="value">The new value of the element at index.</param>
        /// <returns></returns>
        protected virtual bool OnSet(int index, T oldValue, T value)
        {
            return true;
        }
        /// <summary>
        /// Performs additional custom processes after setting a value in the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index at which oldValue can be found.</param>
        /// <param name="oldValue">The value to replace with newValue.</param>
        /// <param name="newValue">The new value of the element at index.</param>
        protected virtual void OnSetComplete(int index, T oldValue, T newValue)
        {
        }
        /// <summary>
        /// Performs additional custom processes when validating a value.
        /// </summary>
        /// <param name="value">The object to validate.</param>
        /// <returns></returns>
        protected virtual bool OnValidate(T value)
        {
            return true;
        }
        #endregion

        #region IList<T> Members

        /// <summary>
        /// Searches for the specified Object and returns the zero-based index of the first occurrence within the entire CollectionBase.
        /// </summary>
        /// <param name="item">The Object to locate in the CollectionBase.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.</returns>
        public virtual int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }
        /// <summary>
        /// Inserts an element into the CollectionBase at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="item">The Object to insert.</param>
        public virtual void Insert(int index, T item)
        {
            if (!OnValidate(item)) return;
            if (!OnInsert(index, item)) return;
            _innerList.Insert(index, item);
            OnInsertComplete(index, item);
        }
        /// <summary>
        /// Removes the element at the specified index of the CollectionBase instance.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public virtual void RemoveAt(int index)
        {
            T value = _innerList[index];

            if (!OnValidate(value)) return;
            if (!OnRemove(index, value)) return;
            _innerList.RemoveAt(index);
            OnRemoveComplete(index, value);
        }
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public virtual T this[int index]
        {
            get
            {
                return _innerList[index];
            }

            set
            {
                T oldValue = _innerList[index];

                if (!OnValidate(value)) return;
                if (!OnSet(index, oldValue, value)) return;
                _innerList[index] = value;
                OnSetComplete(index, oldValue, value);
            }
        }
        #endregion

        #region ICollection<T> Members
        /// <summary>
        /// Adds an object to the end of the CollectionBase.
        /// </summary>
        /// <param name="item">The Object to be added to the end of the CollectionBase.</param>
        public virtual void Add(T item)
        {
            if (!OnValidate(item)) return;
            if (!OnInsert(_innerList.Count, item)) return;
            _innerList.Add(item);
            OnInsertComplete(_innerList.Count - 1, item);
        }
        /// <summary>
        /// Determines whether the CollectionBase contains a specific element.
        /// </summary>
        /// <param name="item">The Object to locate in the CollectionBase.</param>
        /// <returns>true if the CollectionBase contains the specified value; otherwise, false.</returns>
        public virtual bool Contains(T item)
        {
            return _innerList.Contains(item);
        }
        /// <summary>
        /// Copies the entire CollectionBase to a compatible one-dimensional Array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CollectionBase. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of elements contained in the CollectionBase instance.
        /// </summary>
        public virtual int Count
        {
            get { return _innerList.Count; }
        }
        /// <summary>
        /// Gets a value indicating whether the CollectionBase is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return ((ICollection<T>)_innerList).IsReadOnly; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the CollectionBase.
        /// </summary>
        /// <param name="item">The Object to remove from the CollectionBase.</param>
        /// <returns></returns>
        public virtual bool Remove(T item)
        {
            int index = _innerList.IndexOf(item);

            if (index < 0) return false;

            if (!OnValidate(item)) return false;
            if (!OnRemove(index, item)) return false;
            _innerList.Remove(item);
            OnRemoveComplete(index, item);
            return true;
        }
        #endregion

        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator that iterates through the CollectionBase instance.
        /// </summary>
        /// <returns>An IEnumerator for the CollectionBase instance.</returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
        #endregion

        #region IList Members
        /// <summary>
        /// Adds an object to the end of the CollectionBase.
        /// </summary>
        /// <param name="value">The Object to be added to the end of the CollectionBase.</param>
        /// <returns>The CollectionBase index at which the value has been added.</returns>
        public virtual int Add(object value)
        {
            int index = _innerList.Count;

            if (!OnValidate((T)value)) return -1;
            if (!OnInsert(index, (T)value)) return -1;

            index = ((IList)_innerList).Add(value);
            OnInsertComplete(index, (T)value);
            return index;
        }
        /// <summary>
        /// Determines whether the CollectionBase contains a specific element.
        /// </summary>
        /// <param name="value">The Object to locate in the CollectionBase.</param>
        /// <returns>true if the CollectionBase contains the specified value; otherwise, false.</returns>
        public virtual bool Contains(object value)
        {
            return ((IList)_innerList).Contains(value);
        }
        /// <summary>
        /// Searches for the specified Object and returns the zero-based index of the first occurrence within the entire CollectionBase.
        /// </summary>
        /// <param name="value">The Object to locate in the CollectionBase.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.</returns>
        public virtual int IndexOf(object value)
        {
            return ((IList)_innerList).IndexOf(value);
        }
        /// <summary>
        /// Inserts an element into the CollectionBase at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert.</param>
        public virtual void Insert(int index, object value)
        {
            if (!OnValidate((T)value)) return;
            if (!OnInsert(index, (T)value)) return;
            ((IList)_innerList).Insert(index, value);
            OnInsertComplete(index, (T)value);
        }
        /// <summary>
        /// Gets a value indicating whether the CollectionBase has a fixed size.
        /// </summary>
        public virtual bool IsFixedSize
        {
            get { return ((IList)_innerList).IsFixedSize; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the CollectionBase.
        /// </summary>
        /// <param name="value">The Object to remove from the CollectionBase.</param>
        public virtual void Remove(object value)
        {
            int index = _innerList.IndexOf((T)value);

            if (index < 0) return;

            if (!OnValidate((T)value)) return;
            if (!OnRemove(index, (T)value)) return;
            ((IList)_innerList).Remove(value);
            OnRemoveComplete(index, (T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return _innerList[index];
            }

            set
            {
                T oldValue = _innerList[index];
                if (!OnValidate((T)value)) return;
                if (!OnSet(index, oldValue, (T)value)) return;
                _innerList[index] = (T)value;
                OnSetComplete(index, oldValue, (T)value);
            }
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the entire CollectionBase to a compatible one-dimensional Array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CollectionBase. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index)
        {
            ((ICollection)_innerList).CopyTo(array, index);
        }
        /// <summary>
        /// Gets a value indicating whether access to the CollectionBase is synchronized (thread safe).
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return ((ICollection)_innerList).IsSynchronized; }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the CollectionBase.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return ((ICollection)_innerList).SyncRoot; }
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through the CollectionBase instance.
        /// </summary>
        /// <returns>An IEnumerator for the CollectionBase instance.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerList).GetEnumerator();
        }
        #endregion
    }
}

#endif
