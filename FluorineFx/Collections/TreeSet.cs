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

namespace FluorineFx.Collections
{
    /// <summary>
    /// A sorted set.
    /// </summary>
    public interface ISortedSet : ICollection, IList
    {
        /// <summary>
        /// Returns a portion of the list whose elements are greater than the limit object parameter.
        /// </summary>
        /// <param name="limit">The start element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
        ISortedSet TailSet(object limit);
    }

    [Serializable]
    class TreeSet : ArrayList, ISortedSet
    {
        private readonly IComparer _comparer = System.Collections.Comparer.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet"/> class.
        /// </summary>
        public TreeSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet"/> class.
        /// </summary>
        /// <param name="c">The <see cref="T:System.Collections.ICollection"/> whose elements are copied to the new list.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="c"/> is <see langword="null"/>.</exception>
        public TreeSet(ICollection c)
        {
            AddAll(c);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeSet"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public TreeSet(IComparer c)
        {
            _comparer = c;
        }

        /// <summary>
        /// Unmodifiables the tree set.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static TreeSet UnmodifiableTreeSet(ICollection collection)
        {
            ArrayList items = new ArrayList(collection);
            items = ReadOnly(items);
            return new TreeSet(items);
        }

        /// <summary>
        /// Gets the IComparer object used to sort this set.
        /// </summary>
        public IComparer Comparer
        {
            get { return _comparer; }
        }

        private bool AddWithoutSorting(object obj)
        {
            bool inserted;
            if (!(inserted = Contains(obj)))
            {
                base.Add(obj);
            }
            return !inserted;
        }

        /// <summary>
        /// Adds a new element to the ArrayList if it is not already present and sorts the ArrayList.
        /// </summary>
        /// <param name="obj">Element to insert to the ArrayList.</param>
        /// <returns>true if the new element was inserted, false otherwise.</returns>
        public new bool Add(object obj)
        {
            bool inserted = AddWithoutSorting(obj);
            Sort(_comparer);
            return inserted;
        }

        /// <summary>
        /// Adds all the elements of the specified collection that are not present to the list.
        /// </summary>		
        /// <param name="c">Collection where the new elements will be added</param>
        /// <returns>Returns true if at least one element was added to the collection.</returns>
        public bool AddAll(ICollection c)
        {
            IEnumerator e = new ArrayList(c).GetEnumerator();
            bool added = false;
            while (e.MoveNext())
            {
                if (AddWithoutSorting(e.Current))
                {
                    added = true;
                }
            }
            Sort(_comparer);
            return added;
        }

        /// <summary>
        /// Returns the first item in the set.
        /// </summary>
        /// <returns>First object.</returns>
        public object First()
        {
            return this[0];
        }

        /// <summary>
        /// Determines whether an element is in the the current TreeSetSupport collection. The IComparer defined for 
        /// the current set will be used to make comparisons between the elements already inserted in the collection and 
        /// the item specified.
        /// </summary>
        /// <param name="item">The object to be locatet in the current collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public override bool Contains(object item)
        {
            IEnumerator tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (_comparer.Compare(tempEnumerator.Current, item) == 0)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns a portion of the list whose elements are greater than the limit object parameter.
        /// </summary>
        /// <param name="limit">The start element of the portion to extract.</param>
        /// <returns>The portion of the collection whose elements are greater than the limit object parameter.</returns>
        public ISortedSet TailSet(object limit)
        {
            ISortedSet newList = new TreeSet();
            int i = 0;
            while ((i < Count) && (_comparer.Compare(this[i], limit) < 0))
            {
                i++;
            }
            for (; i < Count; i++)
            {
                newList.Add(this[i]);
            }
            return newList;
        }
    }
}
