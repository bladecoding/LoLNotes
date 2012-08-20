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

namespace FluorineFx.Collections
{
    /// <summary>
    /// Sorted set of objects; depending on constructor parameters, implements
    /// regular set (equal members are not permitted), or multiset (equal members are OK).
    /// </summary>
    public abstract class SetBase : ISet, IComparable
    {
        private System.Collections.IComparer _comparer;
        private bool _allowDuplicates;
        private RbTree _tree;
        private int _count = 0;

        /// <summary>
        /// Creates an instance of SetBase.
        /// </summary>
        /// <param name="comparer">Comparer that specifies sort order of the elements.</param>
        /// <param name="allowDuplicates">Whether multiple duplicate (equivalent) elements are allowed.</param>
        public SetBase(System.Collections.IComparer comparer, bool allowDuplicates)
        {
            _comparer = comparer;
            _allowDuplicates = allowDuplicates;
            _tree = new RbTree(_comparer);
        }

        /// <summary>
        /// Compares this to parameter.
        /// </summary>
        /// <remarks>
        /// If obj is not enumerable, returns false.
        /// If obj is enumerable, compares all members using <c>Comparer.Default</c>.
        /// </remarks>
        public override bool Equals(object obj)
        {
            return CollectionComparer.Default.Compare(this, obj) == 0;
        }

        /// <summary>
        /// Exists just to silence compiler warning.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that can iterate through the set.
        /// </summary>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return new SetEnumerator(_tree);
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Indicates whether access to the set is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns number of elements in the set.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Copies set to an array.
        /// </summary>
        /// <remarks>
        /// <para>All items in the set must be castable to the type of the array. Otherwise,
        /// <c>InvalidCastException</c> will be thrown.</para>
        /// <para><b>array</b> must be one-dimensional. Otherwise <c>ArgumentException</c> is thrown.</para>
        /// <para><b>index</b> must be within valid range for the <b>array</b>. Otherwise <c>ArgumentOutOfRangeException</c> is thrown.</para>
        /// <para><b>array</b> must have enough space after <b>index</b> to fit all elements of the set. Otherwise <c>ArgumentOutOfRangeException</c> is thrown.</para>
        /// <para>Elements are put into the array in ascending sort order.</para>
        /// </remarks>
        /// <param name="array">Array to copy to.</param>
        /// <param name="index">Index to start from.</param>
        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (array.Rank != 1) throw new ArgumentException("Cannot copy to multidimensional array", "array");

            if (index < 0) throw new ArgumentOutOfRangeException("index", index, "index cannot be negative");

            if (index >= array.Length)
            {
                throw new ArgumentOutOfRangeException
                (
                    "index",
                    index,
                    String.Format("Passed array of length {0}, index cannot exceed {1}", array.Length, array.Length - 1)
                );
            }

            int count = Count;
            if (array.Length - index < count)
            {
                throw new ArgumentOutOfRangeException
                (
                    "index",
                    index,
                    String.Format( "Not enough room in the array to copy the collection. Array length {0}, start index {1}, items in collection {2}", array.Length, index, count)
                );
            }

            int i = index;
            foreach (object item in this)
            {
                array.SetValue(item, i);
                ++i;
            }
        }

        /// <summary>
        /// An object that can be used to synchronize access to the set.
        /// </summary>
        /// <remarks>
        /// <para>Returns <c>this</c> for set objects that store their members themselves.</para>
        /// <para>Returns underlying object for decorators that are wrappers around other objects.</para>
        /// </remarks>
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region IModifiableCollection Members

        /// <summary>
        /// Indicates whether the set is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds object to the set, preserving order.
        /// </summary>
        public bool Add(object key)
        {
            if (key == null) return false;

            RbTree.InsertResult result = _tree.Insert(key, _allowDuplicates, true);

            if (result.NewNode)
                ++_count;
            return result.NewNode;
        }

        /// <summary>
        /// Adds object to the set only if the set contains no equal object(s).
        /// </summary>
        public bool AddIfNotContains(object key)
        {
            if (key == null) return false;

            RbTree.InsertResult result = _tree.Insert(key, false, false);

            if (result.NewNode)
            {
                ++_count;
            }

            return result.NewNode;
        }

        /// <summary>
        /// Removes object(s) from the set.
        /// </summary>
        /// <remarks>
        /// All objects equal to <b>key</b> are removed.
        /// </remarks>
        public int Remove(object key)
        {
            if (key == null) return 0;
            int result = _tree.Erase(key);
            _count -= result;
            return result;
        }

        /// <summary>
        /// Removes all objects from the set.
        /// </summary>
        public void Clear()
        {
            _tree = new RbTree(_comparer);
            _count = 0;
        }

        /// <summary>
        /// Finds object in the set.
        /// </summary>
        /// <returns>First object equal to <b>obj</b>, or null if not found.</returns>
        public object Find(object key)
        {
            if (key == null) return null;
            RbTreeNode node = _tree.LowerBound(key);

            if (node.IsNull) return null;

            if (Comparer.Compare(node.Value, key) == 0)
            {
                return node.Value;
            }
            return null;
        }

        /// <summary>
        /// Finds object(s) in the set.
        /// </summary>
        /// <returns>Collection of objects equal to <b>obj</b>.</returns>
        /// <remarks>
        /// If no elements equal to <b>obj</b> are found in the set, returns 
        /// valid reference to an empty collection.
        /// </remarks>
        public System.Collections.ICollection FindAll(object key)
        {
            System.Collections.ArrayList result = new System.Collections.ArrayList();
            if (key == null) return result;

            RbTreeNode lower = _tree.LowerBound(key);
            RbTreeNode upper = _tree.UpperBound(key);

            if (lower == upper) return result;

            for (RbTreeNode node = lower; node != upper; node = _tree.Next(node))
            {
                result.Add(node.Value);
            }

            return result;
        }

        #endregion

        #region IReversible Members

        /// <summary>
        /// Enumerable whose enumerator traverses the set in reversed order.
        /// </summary>
        public System.Collections.IEnumerable Reversed
        {
            get
            {
                return new ReversedTree(_tree);
            }
        }

        #endregion

        #region ISet Members

        /// <summary>
        /// Comparer object that defines sort order for the set.
        /// </summary>
        public System.Collections.IComparer Comparer
        {
            get { return _comparer; }
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares this set to another object.
        /// </summary>
        /// <remarks>
        /// If obj is not an enumerable, returns -1 (this &lt; obj)
        /// Otherwise performs member-to-member lexicographical comparison of two enumerables using default comparer.
        /// </remarks>
        public int CompareTo(object obj)
        {
            return CollectionComparer.Default.Compare(this, obj);
        }

        #endregion

        /// <summary>
        /// Checks that the other set has compatible comparer.
        /// </summary>
        /// <remarks>
        /// Tries to check that comparer object of the other set is the same
        /// as comparer object of this set. Since comparers don't provide adequate
        /// imlpementation of Equals(), we just check that comparer types are the same.
        /// Throws exception if there is an incompatibility.
        /// </remarks>
        protected void CheckComparer(ISet other)
        {
            if (this.Comparer.GetType() != other.Comparer.GetType())
            {
                throw new ArgumentException("Sets have incompatible comparer objects", "other");
            }
        }
    }
}
