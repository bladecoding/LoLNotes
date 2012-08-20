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

using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Api.Persistence;

namespace FluorineFx.Messaging.Api
{
	/// <summary>
	/// Base interface for all scope objects.
	/// </summary>
	[CLSCompliant(false)]
	public interface IBasicScope : ICoreObject, IEventObservable, IPersistable, IEnumerable
	{
		/// <summary>
        /// Checks whether the scope has a parent.
		/// You can think of scopes as of tree items
		/// where scope may have a parent and children (child).
		/// </summary>
		bool HasParent { get; }
		/// <summary>
		/// Get this scope's parent.
		/// </summary>
		IScope Parent { get; }
		/// <summary>
		/// Get the scopes depth, how far down the scope tree is it. The lowest depth
		/// is 0x00, the depth of Global scope. Application scope depth is 0x01. Room
		/// depth is 0x02, 0x03 and so forth.
		/// </summary>
		int Depth { get; }
		/// <summary>
		/// Gets the name of this scope.
		/// </summary>
		new string Name { get; }
		/// <summary>
		/// Gets the full absolute path.
		/// </summary>
		new string Path { get; }
		/// <summary>
		/// Gets the type of the scope.
		/// </summary>
		string Type{ get; }
        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        object SyncRoot { get; }
	}
}
