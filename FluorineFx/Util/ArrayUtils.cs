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
using System.Text;

namespace FluorineFx.Util
{
    /// <summary>
    /// Array utility class.
    /// </summary>
    public abstract class ArrayUtils
    {
        private ArrayUtils() { }

        /// <summary>
        /// Changes the size of an array to the specified new size.
        /// </summary>
        /// <param name="array">The one-dimensional, zero-based array to resize.</param>
        /// <param name="newSize">The size of the new array.</param>
        /// <returns>The resized array.</returns>
        public static Array Resize(Array array, int newSize)
        {
            Type type = array.GetType();
            Array newArray = Array.CreateInstance(type.GetElementType(), newSize);
            Array.Copy(array, 0, newArray, 0, Math.Min(array.Length, newSize));
            return newArray;
        }

        /*
        static unsafe bool Equals(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2))
                        return false;
                if ((l & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                        return false;
                    x1 += 4; x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*((short*)x1) != *((short*)x2))
                        return false;
                    x1 += 2; x2 += 2;
                }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2))
                        return false;
                return true;
            }
        }
        */
        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <param name="a1">First byte array.</param>
        /// <param name="a2">Second byte array.</param>
        /// <returns><c>true</c> if the byte arrays have the same length and the elemtns are identical; otherwise, <c>false</c>.</returns>
        public static bool Equals(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                    return false;
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i]) return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents a list.
        /// </summary>
        /// <param name="array">A list instance.</param>
        /// <returns>A <see cref="System.String"/> that represents the list instance.</returns>
        public static string ArrayToString(IList array)
        {
            return ArrayToString(array, ",");
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents a list.
        /// </summary>
        /// <param name="array">A list instance.</param>
        /// <param name="delimeter">String delimiter.</param>
        /// <returns>A <see cref="System.String"/> that represents the list instance.</returns>
        public static string ArrayToString(IList array, string delimeter)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < array.Count; i++)
            {
                if( i > 0 )
                    sb.Append(delimeter);
                object obj = array[i];
                if( obj != null )
                    sb.Append(obj.ToString());
                else
                    sb.Append("null");
            }
            return sb.ToString();
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents a list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="array">A list instance.</param>
        /// <returns>A <see cref="System.String"/> that represents the list instance.</returns>
        public static string ArrayToString<T>(IList<T> array)
        {
            return ArrayToString<T>(array, ",");
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents a list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="array">A list instance.</param>
        /// <param name="delimeter">String delimiter.</param>
        /// <returns>A <see cref="System.String"/> that represents the list instance.</returns>
        public static string ArrayToString<T>(IList<T> array, string delimeter)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Count; i++)
            {
                if (i > 0)
                    sb.Append(delimeter);
                T obj = array[i];
                if (obj != null)
                    sb.Append(obj.ToString());
                else
                    sb.Append("null");
            }
            return sb.ToString();
        }
    }
}
