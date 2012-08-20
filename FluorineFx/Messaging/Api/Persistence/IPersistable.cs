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
using System.IO;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Api.Persistence
{
	/// <summary>
	/// Base interface for objects that can be made persistent.
	/// 
	/// Every object that complies to this interface must provide either a
	/// constructor that takes an input stream as only parameter or an empty
	/// constructor so it can be loaded from the persistence store.
	/// 
	/// However this is not required for objects that are created by the application
	/// and initialized afterwards.
	/// </summary>
	public interface IPersistable
	{
        /// <summary>
        /// Gets or sets a value indicating whether the object is persistent.
        /// </summary>
        /// <value>A value indicating whether the object is persistent.</value>
		bool IsPersistent{ get; set; }
        /// <summary>
        /// Gets or sets the name of the persistent object.
        /// </summary>
        /// <value>The name of the persistent object.</value>
		string Name{ get; set; }
        /// <summary>
        /// Gets or sets the path of the persistent object.
        /// </summary>
        /// <value>The path of the persistent object.</value>
		string Path{ get; set; }
        /// <summary>
        /// Gets the timestamp when the object was last modified.
        /// </summary>
        /// <value>The timestamp when the object was last modified.</value>
		long LastModified{ get; }
        /// <summary>
        /// Gets or sets the persistence store this object is stored in.
        /// </summary>
        /// <value>The persistence store this object is stored in.</value>
		IPersistenceStore Store{ get; set; }
        /// <summary>
        /// Writes the object to the specified output stream.
        /// </summary>
        /// <param name="writer">Writer to write to.</param>
		void Serialize(AMFWriter writer);
        /// <summary>
        /// Loads the object from the specified input stream.
        /// </summary>
        /// <param name="reader">Reader to load from.</param>
		void Deserialize(AMFReader reader);
	}
}
