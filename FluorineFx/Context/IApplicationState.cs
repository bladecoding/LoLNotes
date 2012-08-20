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

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    public interface IApplicationState : IEnumerable
	{
        /// <summary>
        /// Gets the value of a single ApplicationState object by name.
        /// </summary>
        /// <param name="name">The name of the object in the collection.</param>
        /// <returns>The object referenced by name.</returns>
		object this[string name] {get; set;}
        /// <summary>
        /// Removes the named object from an ApplicationState collection.
        /// </summary>
        /// <param name="key">The name of the object to be removed from the collection.</param>
		void Remove(string key );
        /// <summary>
        /// Adds a new object to the ApplicationState collection.
        /// </summary>
        /// <param name="name">The name of the object to be added to the collection.</param>
        /// <param name="value">The value of the object.</param>
		void Add(string name, object value);
        /// <summary>
        /// Locks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        void Lock();
        /// <summary>
        /// Unlocks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        void UnLock();
	}
}
