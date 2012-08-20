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
using System.Threading;

namespace FluorineFx.Util
{
    /// <summary>
    /// Threading utility class.
    /// </summary>
    public abstract class ThreadingUtils
    {
        private ThreadingUtils() { }

        /// <summary>
        /// Compares a value in a location, and swaps it with a new value if the comparand is equal to original value.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="location">The location of the value to check.</param>
        /// <param name="comparand">The value to compare against the original location.</param>
        /// <param name="newValue">The value to replace the original value with.</param>
        /// <returns>true if the swap succeeded, false if another thread pre-empted the operation.</returns>
        public static bool CompareAndSwap<T>(ref T location, T comparand, T newValue) where T : class
        {
            return ReferenceEquals(comparand, Interlocked.CompareExchange(ref location, newValue, comparand));
        }

    }
}
