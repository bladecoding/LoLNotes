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
    /// Sorted set of unique (non-equal) objects
    /// </summary>
    public class Set : SetBase
    {
        /// <summary>
        /// Creates an empty set with default comparer; members must implement IComparable.
        /// </summary>
        public Set():base(System.Collections.Comparer.Default, false)
        {
        }

        /// <summary>
        /// Creates a set from elements of given collection; default comparer is used; members must implement IComparable.
        /// </summary>
        public Set(System.Collections.ICollection collection):this()
        {
            foreach (object obj in collection)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Creates an empty set that uses given comparer object.
        /// </summary>
        public Set(System.Collections.IComparer comparer):base(comparer, false)
        {
        }

        /// <summary>
        /// Creates a set from elements of given collection that uses given comparer object.
        /// </summary>
        public Set(System.Collections.IComparer comparer, System.Collections.ICollection collection):this(comparer)
        {
            foreach (object obj in collection)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Returns union of two sets.
        /// </summary>
        public Set Union(Set other)
        {
            return Union(this, other);
        }

        /// <summary>
        /// Returns union of two sets.
        /// </summary>
        public static Set Union(Set a, Set b)
        {
            a.CheckComparer(b);
            Set result = new Set(a.Comparer);
            SetOp.Union(a, b, a.Comparer, new Inserter(result));
            return result;
        }

        /// <summary>
        /// Returns intersection of two sets.
        /// </summary>
        /// <remarks>Intersection contains elements present in both sets.</remarks>
        public Set Intersection(Set other)
        {
            return Intersection(this, other);
        }

        /// <summary>
        /// Returns intersection of two sets.
        /// </summary>
        /// <remarks>Intersection contains elements present in both sets.</remarks>
        public static Set Intersection(Set a, Set b)
        {
            a.CheckComparer(b);
            Set result = new Set(a.Comparer);
            SetOp.Inersection(a, b, a.Comparer, new Inserter(result));
            return result;
        }

        /// <summary>
        /// Returns difference of two sets.
        /// </summary>
        /// <remarks>
        /// Difference contains elements present in first set, but not in the second.<br/>
        /// Difference is not symmetric. Difference(a,b) is not equal to Difference(b,a)
        /// </remarks>
        public Set Difference(Set other)
        {
            return Difference(this, other);
        }

        /// <summary>
        /// Returns difference of two sets.
        /// </summary>
        /// <remarks>
        /// Difference contains elements present in first set, but not in the second.<br/>
        /// Difference is not symmetric. Difference(a,b) is not equal to Difference(b,a)
        /// </remarks>
        public static Set Difference(Set a, Set b)
        {
            a.CheckComparer(b);
            Set result = new Set(a.Comparer);
            SetOp.Difference(a, b, a.Comparer, new Inserter(result));
            return result;
        }

        /// <summary>
        /// Returns symmetric difference of two sets.
        /// </summary>
        /// <remarks>
        /// Symmetric difference contains elements present in one of the sets, but not in both.
        /// </remarks>
        public Set SymmetricDifference(Set other)
        {
            return SymmetricDifference(this, other);
        }

        /// <summary>
        /// Returns symmetric difference of two sets.
        /// </summary>
        /// <remarks>
        /// Symmetric difference contains elements present in one of the sets, but not in both
        /// </remarks>
        public static Set SymmetricDifference(Set a, Set b)
        {
            a.CheckComparer(b);
            Set result = new Set(a.Comparer);
            SetOp.SymmetricDifference(a, b, a.Comparer, new Inserter(result));
            return result;
        }
    }
}
