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
#if !(NET_1_1)
using System.Collections.Generic;
#endif

namespace FluorineFx.Messaging.Api
{
	/// <summary>
	/// Base interface for all API objects with attributes.
	/// </summary>
    public interface IAttributeStore : IEnumerable
	{
		/// <summary>
		/// Returns the attribute names.
		/// </summary>
		/// <returns>Collection of attribute names.</returns>
#if !(NET_1_1)
        ICollection<string> GetAttributeNames();
#else
        ICollection GetAttributeNames();
#endif
		/// <summary>
		/// Sets an attribute on this object.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <param name="value">The attribute value.</param>
        /// <returns>true if the attribute value changed otherwise false</returns>
		bool SetAttribute(string name, object value);
#if !(NET_1_1)
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        void SetAttributes(IDictionary<string, object> values);
#endif
        /// <summary>
		/// Sets multiple attributes on this object.
		/// </summary>
		/// <param name="values">Dictionary of attributes.</param>
        void SetAttributes(IDictionary values);
		/// <summary>
		/// Sets multiple attributes on this object.
		/// </summary>
		/// <param name="values">Attribute store.</param>
		void SetAttributes(IAttributeStore values);
		/// <summary>
		/// Returns the value for a given attribute.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <returns>The attribute value.</returns>
		object GetAttribute(string name);
		/// <summary>
		/// Returns the value for a given attribute and sets it if it doesn't exist.
		/// </summary>
		/// <param name="name">The attribute name.</param>
		/// <param name="defaultValue">Attribute's default value.</param>
		/// <returns>The attribute value.</returns>
		object GetAttribute(string name, object defaultValue);
		/// <summary>
		/// Checks whetner the object has an attribute.
		/// </summary>
		/// <param name="name">The attribute name.</param>
        /// <returns>true if a child scope exists, otherwise false.</returns>
		bool HasAttribute(string name);
		/// <summary>
		/// Removes an attribute.
		/// </summary>
		/// <param name="name">The attribute name.</param>
        /// <returns>true if the attribute was found and removed otherwise false.</returns>
		bool RemoveAttribute(string name);
		/// <summary>
		/// Removes all attributes.
		/// </summary>
		void RemoveAttributes();
        /// <summary>
        /// Gets whether the attribute store is empty;
        /// </summary>
        bool IsEmpty { get; }
        /// <summary>
        /// Gets or sets a value by name.
        /// </summary>
        /// <param name="name">The key name of the value.</param>
        /// <returns>The value with the specified name.</returns>
        Object this[string name] { get; set; }
        /// <summary>
        /// Gets the number of attributes in the collection.
        /// </summary>
        int AttributesCount { get; }
#if !(NET_1_1)
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        void CopyTo(object[] array, int index);
#else
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        void CopyTo(Array array, int index);
#endif
    }
}
