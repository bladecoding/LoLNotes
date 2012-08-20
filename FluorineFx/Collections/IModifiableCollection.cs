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
    /// Modifiable and searchable collection.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Unlike <b>IList</b>, modifiable collection may not support access to
    /// its elements by index.
    /// </p>
    /// <p>
    /// In order for <b>IModifiableCollection</b> methods to work adequately,
    /// member objects must at least properly implement Object.Equals().
    /// It must be 
    /// <list type="number">
    /// <item>Reflexive: <c>a.Equals(a)</c> is always true).</item>
    /// <item>Commutative: <c>a.Equals(b)</c> returns the same value as <c>b.Equals(a))</c></item> 
    /// <item>Transitive: if <c>a.Equals(b)</c> and <c>b.Equals(c)</c> then <c>a.Equals(c)</c>.</item>
    /// </list>
    /// </p>
    /// <p>
    /// Implementing classes may impose further requirements on member objects.
    /// </p>
    /// </remarks>
    public interface IModifiableCollection : System.Collections.ICollection
    {
        /// <summary>
        /// Returns true if collection is read-only and modifications are disabled.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Adds object to collection.
        /// Add(null) is a no-op.
        /// </summary>
        /// <param name="key">Object to add.</param>
        /// <returns>
        /// Returns true if new object has been added to the collection (Count has been incremented).
        /// Returns false if old object equal to key has been replaced with key (Count has not changed).
        /// The latter may happen with collection like set.
        /// </returns>
        bool Add(object key);

        /// <summary>
        /// Adds key to the collection only if it does not already contain an equal key.
        /// AddIfNotContains(null) is a no-op.
        /// </summary>
        /// <param name="key">Object to add</param>
        /// <returns>true if object has been added and Count has been incremented; false otherwise</returns>
        bool AddIfNotContains(object key);

        /// <summary>
        /// Removes from collection all objects eqial to key.
        /// Remove(null) is a no-op.
        /// </summary>
        /// <param name="key">Object to remove.</param>
        /// <returns>Total number of objects removed from the collection (may be 0, 1, or more).</returns>
        int Remove(object key);

        /// <summary>
        /// Removes all objects from the collection. 
        /// When this method returns, Count is 0.
        /// </summary>
        void Clear();

        /// <summary>
        /// Finds first object in the collection that is equal to key.
        /// </summary>
        /// <param name="key">Object to look for.</param>
        /// <returns>First object that equals to key, or null if no such object exists.
        /// Find(null) always returns null.</returns>
        object Find(object key);

        /// <summary>
        /// Finds all objects in the collection that are equal to key.
        /// </summary>
        /// <param name="key">Object to look for.</param>
        /// <returns>ICollection that iterates through objects equal to key.
        /// Resulting collection may be empty.
        /// Find(null) returns an valid reference to an empty collection.</returns>
        System.Collections.ICollection FindAll(object key);
    }
}
