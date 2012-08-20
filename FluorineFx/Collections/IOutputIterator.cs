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
    /// Write-only forward-only iterator.
    /// </summary>
    /// <remarks>
    /// <p>.NET has built-in abstraction for read-only forward-only iterator, which is
    /// <c>IEnumerator</c>. Parallel concept in STL is "input iterator".
    /// Unfortunately, there is no built-in abstraction for	write-only, forward-only behavior,
    /// a.k.a output iterator.</p>
    /// <p>Objects are written to the output iterator one by one using <c>Add</c> method.
    /// There is no way to retrieve added object or undo the addition.</p>
    /// </remarks>
    public interface IOutputIterator
    {
        /// <summary>
        /// Adds an object to the output.
        /// </summary>
        /// <remarks>
        /// <c>Add()</c> can put an object in the end of a collection, or in the beginning
        /// of a collection, or output a string representatino of the object to
        /// a named pipe, etc.
        /// </remarks>
        void Add(object obj);
    }
}
