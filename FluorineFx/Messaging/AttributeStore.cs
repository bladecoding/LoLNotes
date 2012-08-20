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
using System.Collections.Specialized;
#if !(NET_1_1)
using System.Collections.Generic;
using FluorineFx.Collections.Generic;
#endif
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public class AttributeStore : DisposableBase, IAttributeStore
	{
        /// <summary>
        /// Attribute dictionary.
        /// </summary>
#if !(NET_1_1)
        protected CopyOnWriteDictionary<string, object> _attributes;
#else
        protected CopyOnWriteDictionary _attributes;
#endif

        /// <summary>
        /// Initializes a new instance of the AttributeStore class.
        /// </summary>
        public AttributeStore()
		{
#if !(NET_1_1)
            _attributes = new CopyOnWriteDictionary<string,object>();
#else
            _attributes = new CopyOnWriteDictionary();
#endif
        }

		#region IAttributeStore Members

        /// <summary>
        /// Returns the attribute names.
        /// </summary>
        /// <returns>Collection of attribute names.</returns>
#if !(NET_1_1)
        public virtual ICollection<string> GetAttributeNames()
#else
        public virtual ICollection GetAttributeNames()
#endif
		{
            return _attributes.Keys;
        }
        /// <summary>
        /// Sets an attribute on this object.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>true if the attribute value changed otherwise false.</returns>
        public virtual bool SetAttribute(string name, object value)
        {
            if (name == null)
                return false;
            // Update with new value
            object previous = null;
            _attributes.TryGetValue(name, out previous);
            if( previous == null || !value.Equals(previous) )
                _attributes[name] = value;
            return (previous == null || !value.Equals(previous));
        }
#if !(NET_1_1)
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public virtual void SetAttributes(IDictionary<string, object> values)
        {
            foreach (KeyValuePair<string, object> entry in values)
            {
                SetAttribute(entry.Key, entry.Value);
            }
        }
#endif
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public virtual void SetAttributes(IDictionary values)
		{
            foreach (DictionaryEntry entry in values)
            {
                SetAttribute(entry.Key as string, entry.Value);
            }
		}
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Attribute store.</param>
		public virtual void SetAttributes(IAttributeStore values)
		{
            foreach (string name in values.GetAttributeNames())
            {
                object value = values.GetAttribute(name);
                SetAttribute(name, value);
            }
		}
        /// <summary>
        /// Returns the value for a given attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute value.</returns>
        public virtual object GetAttribute(string name)
        {
            if (name == null)
                return null;
            if (_attributes.ContainsKey(name))
                return _attributes[name];
            return null;
        }
        /// <summary>
        /// Returns the value for a given attribute and sets it if it doesn't exist.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="defaultValue">Attribute's default value.</param>
        /// <returns>The attribute value.</returns>
        public virtual object GetAttribute(string name, object defaultValue)
        {
            if (name == null)
                return null;
            if (defaultValue == null)
                throw new NullReferenceException("The default value may not be null.");
            if (_attributes.ContainsKey(name))
                return _attributes[name];
            else
            {
                _attributes[name] = defaultValue;
                return null;
            }
        }
        /// <summary>
        /// Checks whetner the object has an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>true if a child scope exists, otherwise false.</returns>
        public virtual bool HasAttribute(string name)
        {
            if (name == null)
                return false;
            return _attributes.ContainsKey(name);
        }
        /// <summary>
        /// Removes an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>true if the attribute was found and removed otherwise false.</returns>
		public virtual bool RemoveAttribute(string name)
		{
            if (HasAttribute(name))
            {
                _attributes.Remove(name);
                return true;
            }
            return false;
		}
        /// <summary>
        /// Removes all attributes.
        /// </summary>
        public virtual void RemoveAttributes()
        {
            _attributes.Clear();
        }
        /// <summary>
        /// Gets whether the attribute store is empty;
        /// </summary>
        public bool IsEmpty 
        {
            get
            {
                return _attributes.Count == 0;
            }
        }

        /// <summary>
        /// Gets or sets a value by name.
        /// </summary>
        /// <param name="name">The key name of the value.</param>
        /// <returns>The value with the specified name.</returns>
        public Object this[string name] 
        {
            get
            {
                return GetAttribute(name);
            }
            set
            {
                SetAttribute(name, value);
            }
        }
        /// <summary>
        /// Gets the number of attributes in the collection.
        /// </summary>
        public int AttributesCount 
        {
            get 
            {
                return _attributes.Count;
            }
        }

#if !(NET_1_1)
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        public void CopyTo(object[] array, int index)
        {
            _attributes.Values.CopyTo(array, index);
        }
#else
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        public void CopyTo(Array array, int index)
        {
            _attributes.Values.CopyTo(array, index);
        }
#endif
        /// <summary>
        /// Returns an enumerator that iterates through an AttributeStore.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

		#endregion
    }
}
