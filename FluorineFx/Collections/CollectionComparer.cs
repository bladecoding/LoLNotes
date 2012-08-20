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
    /// Lexicographical compare for <b>IEnumerable</b>s.
    /// </summary>
    /// <remarks>
    /// Performs memberwise compare of two collections using given comparer. 
    /// See <see cref="Compare">Compare</see>.
    /// </remarks>
    public class CollectionComparer : IComparer
    {
        private IComparer _comparer;

        /// <summary>
        /// Default collection comparer, based on Comparer.Default.
        /// </summary>
        public static CollectionComparer Default
        {
            get { return new CollectionComparer(); }
        }

        /// <summary>
        /// Creates new instance of collection comparer based on Comparer.Default.
        /// </summary>
        public CollectionComparer():this(Comparer.Default)
        {
        }

        /// <summary>
        /// Creates new instance of collection comparer based on given comparer object.
        /// </summary>
        public CollectionComparer(IComparer comparer)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// Compares two collections.
        /// </summary>
        /// <returns>
        /// <list type="table">
        /// <listheader><item><term>Condition</term><description>Value</description></item></listheader>
        /// <item><term>x&lt;y</term><description>-1</description></item>
        /// <item><term>x equivalent to y</term><description>0</description></item>
        /// <item><term>x&gt;y</term><description>1</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>
        /// If neither x nor y implements <b>IEnumerable</b>,
        /// they are compared using underlying comparer object. 
        /// </para>
        /// <para>
        /// If one of x,y is an <b>IEnumerable</b> and the other is not, enumerable
        /// is greater than plain object.
        /// </para>
        /// <para>
        /// If both x,y implement <b>IEnumerable</b>, they are compared member by member
        /// using underlying comparer object. First pair that compares unequal
        /// determines the outcome of the whole comparison.
        /// </para>
        /// <para>
        /// If x is a proper subset of y, y is greater than x 
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>{1, 10} &gt; { 1, 2, 100, 1000 }</para>
        /// <para>{1, 2 } &lt; { 1, 2, 3 }</para>
        /// </example>
        public int Compare(object x, object y)
        {
            IEnumerable enumerable1 = x as IEnumerable;
            IEnumerable enumerable2 = y as IEnumerable;

            bool xIsEnumerable = (enumerable1 != null);
            bool yIsEnumerable = (enumerable2 != null);

            // if one object is an enumerable, and the other is not,
            // enumerable object is deemed greater
            int result = CompareBool(xIsEnumerable, yIsEnumerable);
            if (result != 0) return result;

            System.Diagnostics.Debug.Assert(xIsEnumerable == yIsEnumerable);

            if (!xIsEnumerable)
            {
                // both are not enumerable - compare as plain objects
                return _comparer.Compare(x, y);
            }

            System.Diagnostics.Debug.Assert(xIsEnumerable);
            System.Diagnostics.Debug.Assert(yIsEnumerable);

            IEnumerator enum1 = enumerable1.GetEnumerator();
            IEnumerator enum2 = enumerable2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                result = _comparer.Compare(enum1.Current, enum2.Current);
                if (result != 0) return result;

                have1 = enum1.MoveNext();
                have2 = enum2.MoveNext();
            }

            // if we got here, one collection is a subset of another
            // longer collection wins
            return CompareBool(have1, have2);
        }

        private int CompareBool(bool bool1, bool bool2)
        {
            if (bool1)
            {
                return bool2 ? 0 : 1;
            }
            else
            {
                return bool2 ? -1 : 0;
            }
        }
    }
}
