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
    /// Union, intersection, and other operations on sorted sequences.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements efficient operation on sorted sequences:
    /// Union, Merge, Intersection, Difference, and SymmetricDifference.
    /// </para>
    /// <para>All methods of the class are static</para>
    /// <para>The methods operate on "sorted sequences" which can be any enumerables,
    /// provided they are properly sorted. The sequences are allowed to contiain duplicates.</para>
    /// </remarks>
    public sealed class SetOp
    {
        private SetOp() { } // disable construction

        /// <summary>
        /// Computes union of two sorted sequences.
        /// </summary>
        /// <remarks>
        /// <para>Both set1 and set2 must be sorted in ascending order with respect to comparer.</para>
        /// <para>Union contains elements present in one or both ranges.</para>
        /// <para>Result is written to the output iterator one member at a time</para>
        /// 
        /// <para>Union differs from <see cref="Merge">Merge</see> for multisets.</para>
        /// 
        /// <para>If k equal elements are present in set1 and m elements equal to those k
        /// are present in set2,then k elements from set1 are included in the output, 
        /// followed by max(m-k, 0) elements from set2. The total of max(k,m) are
        /// added to the output. If you'd like to have m+k elements, use Merge function.
        /// </para>
        /// <para>Complexity: linear on combined number of items in both sequences</para>
        /// </remarks>
        /// <example>
        /// <para>set1 = { "a", "test", "Test", "z" }</para>
        /// <para>set2 = { "b", "tEst", "teSt", "TEST", "Z" }</para>
        /// <para>comparer is a case-insensitive comparer</para>
        /// <para>The following elements will be added to output:
        /// {"a", "b", "test", "Test", "TEST", "z" }</para>
        /// </example>
        public static void Union(IEnumerable set1, IEnumerable set2, IComparer comparer, IOutputIterator output)
        {
            IEnumerator enum1 = set1.GetEnumerator();
            IEnumerator enum2 = set2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                int compare = comparer.Compare(enum1.Current, enum2.Current);

                if (compare < 0)
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                }
                else if (compare > 0)
                {
                    output.Add(enum2.Current);
                    have2 = enum2.MoveNext();
                }
                else
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                    have2 = enum2.MoveNext();
                }
            }

            while (have1)
            {
                output.Add(enum1.Current);
                have1 = enum1.MoveNext();
            }

            while (have2)
            {
                output.Add(enum2.Current);
                have2 = enum2.MoveNext();
            }
        }

        /// <summary>
        /// Computes merge of two sorted sequences.
        /// </summary>
        /// <remarks>
        /// <para>Both set1 and set2 must be sorted in ascending order with respect to comparer.</para>
        /// <para>Merge contains elements present in one or both ranges.</para>
        /// <para>Result is written to the output iterator one member at a time</para>
        /// 
        /// <para>Merge differs from <see cref="Union">Union()</see> for multisets.</para>
        /// 
        /// <para>If k equal elements are present in set1 and m elements equal to those k
        /// are present in set2, then k elements from set1 are included in the output, 
        /// followed by m elements from set2, for the total of k+m equal elements. 
        /// If you'd like to have max(k,m) m+k elements, use Union function.</para>
        /// <para>Complexity: linear on combined number of items in both sequences</para>
        /// </remarks>
        /// <example>
        /// <para>set1 = { "a", "test", "Test", "z" }</para>
        /// <para>set2 = { "b", "tEst", "teSt", "TEST", "Z" }</para>
        /// <para>comparer is a case-insensitive comparer</para>
        /// <para>The following elements will be added to output:
        /// {"a", "b", "test", "Test", "tEst", "teSt", "TEST", "z", "Z" }</para>
        /// </example>
        public static
        void Merge(IEnumerable set1, IEnumerable set2, IComparer comparer, IOutputIterator output)
        {
            IEnumerator enum1 = set1.GetEnumerator();
            IEnumerator enum2 = set2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                int compare = comparer.Compare(enum1.Current, enum2.Current);

                if (compare < 0)
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                }
                else
                {
                    output.Add(enum2.Current);
                    have2 = enum2.MoveNext();
                }
            }

            while (have1)
            {
                output.Add(enum1.Current);
                have1 = enum1.MoveNext();
            }

            while (have2)
            {
                output.Add(enum2.Current);
                have2 = enum2.MoveNext();
            }
        }

        /// <summary>
        /// Computes intersection of two sorted sequences.
        /// </summary>
        /// <remarks>
        /// <para>Both set1 and set2 must be sorted in ascending order with respect to comparer.</para>
        /// <para>Intersection contains elements present in both set1 and set2.</para>
        /// <para>Result is written to the output iterator one member at a time</para>
        /// 
        /// <para>For multisets, if set1 contains k equal elements, and set2 contains
        /// m elements equal to those k, then min(k,m) elements from set1 are
        /// included in the output.</para>
        /// <para>Complexity: linear on combined number of items in both sequences</para>
        /// </remarks>
        /// <example>
        /// <para>set1 = {"a", "b", "test", "tEst", "z" }</para>
        /// <para>set2 = {"a", "TEST", "z", "Z" }</para>
        /// <para>comparer = case insensitive comparer</para>
        /// <para>output = {"a", "test", "z"}</para>
        /// </example>
        public static
        void Inersection(IEnumerable set1, IEnumerable set2, IComparer comparer, IOutputIterator output)
        {
            IEnumerator enum1 = set1.GetEnumerator();
            IEnumerator enum2 = set2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                int compare = comparer.Compare(enum1.Current, enum2.Current);
                if (compare < 0)
                {
                    have1 = enum1.MoveNext();
                }
                else if (compare > 0)
                {
                    have2 = enum2.MoveNext();
                }
                else
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                    have2 = enum2.MoveNext();
                }
            }
        }

        /// <summary>
        /// Computes difference of two sorted sequences.
        /// </summary>
        /// <remarks>
        /// <para>Both set1 and set2 must be sorted in ascending order with respect to comparer.</para>
        /// <para>Difference contains elements present in set1, but not in set2.</para>
        /// <para>Result is written to the output iterator one member at a time</para>
        /// 
        /// <para>For multisets, if set1 contains k equal elements, and set2 contains
        /// m elements equal to those k, then max(k-m,0) elements from set1 are
        /// included in the output.</para>
        /// <para>Complexity: linear on combined number of items in both sequences</para>
        /// </remarks>
        /// <example>
        /// <para>set1 = {"a", "b", "test", "tEst", "z" }</para>
        /// <para>set2 = {"a", "TEST", "z", "Z" }</para>
        /// <para>comparer = case insensitive comparer</para>
        /// <para>output = {"b", "tEst"}</para>
        /// </example>
        public static
        void Difference(IEnumerable set1, IEnumerable set2, IComparer comparer, IOutputIterator output)
        {
            IEnumerator enum1 = set1.GetEnumerator();
            IEnumerator enum2 = set2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                int compare = comparer.Compare(enum1.Current, enum2.Current);
                if (compare < 0)
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                }
                else if (compare > 0)
                {
                    have2 = enum2.MoveNext();
                }
                else
                {
                    have1 = enum1.MoveNext();
                    have2 = enum2.MoveNext();
                }
            }

            while (have1)
            {
                output.Add(enum1.Current);
                have1 = enum1.MoveNext();
            }
        }

        /// <summary>
        /// Computes symmetric difference (XOR) of two sorted sequences.
        /// </summary>
        /// <remarks>
        /// <para>Both set1 and set2 must be sorted in ascending order with respect to comparer.</para>
        /// <para>Symmetric difference contains elements present in exactly one set, but not in both.</para>
        /// <para>Result is written to the output iterator one member at a time</para>
        /// 
        /// <para>For multisets, if set1 contains k equal elements, and set2 contains
        /// m elements equal to those k, then if k&gt;=m, k-m last elements from set1
        /// are included in the output. If k&lt;m, m-k last elements from set2 are included
        /// in the output.</para>
        /// <para>Complexity: linear on combined number of items in both sequences</para>
        /// </remarks>
        /// <example>
        /// <para>set1 = {"a", "b", "test", "tEst", "z" }</para>
        /// <para>set2 = {"a", "TEST", "z", "Z" }</para>
        /// <para>comparer = case insensitive comparer</para>
        /// <para>output = {"b", "tEst", "Z"}</para>
        /// </example>
        public static
        void SymmetricDifference(IEnumerable set1, IEnumerable set2, IComparer comparer, IOutputIterator output)
        {
            IEnumerator enum1 = set1.GetEnumerator();
            IEnumerator enum2 = set2.GetEnumerator();

            bool have1 = enum1.MoveNext();
            bool have2 = enum2.MoveNext();

            while (have1 && have2)
            {
                int compare = comparer.Compare(enum1.Current, enum2.Current);
                if (compare < 0)
                {
                    output.Add(enum1.Current);
                    have1 = enum1.MoveNext();
                }
                else if (compare > 0)
                {
                    output.Add(enum2.Current);
                    have2 = enum2.MoveNext();
                }
                else
                {
                    have1 = enum1.MoveNext();
                    have2 = enum2.MoveNext();
                }
            }

            while (have1)
            {
                output.Add(enum1.Current);
                have1 = enum1.MoveNext();
            }

            while (have2)
            {
                output.Add(enum2.Current);
                have2 = enum2.MoveNext();
            }
        }
    }
}
