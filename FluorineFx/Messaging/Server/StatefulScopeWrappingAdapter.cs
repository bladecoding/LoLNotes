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

using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Adapter
{
	/// <summary>
	/// StatefulScopeWrappingAdapter class wraps stateful IScope functionality. That
	/// is, it has attributes that you can work with, subscopes, associated resources
	/// and connections.
	/// </summary>
	[CLSCompliant(false)]
	public class StatefulScopeWrappingAdapter : AbstractScopeAdapter, IScopeAware, IAttributeStore
	{
		private IScope _scope;

        /// <summary>
        /// Initializes a new instance of the StatefulScopeWrappingAdapter class.
        /// </summary>
		public StatefulScopeWrappingAdapter()
		{
		}
        /// <summary>
        /// Gets the wrapped scope.
        /// </summary>
		public IScope Scope
		{
			get{ return _scope; }
		}

		#region IScopeAware Members

        /// <summary>
        /// Set the scope the object is located in. 
        /// </summary>
        /// <param name="scope">Scope for this object.</param>
		public void SetScope(IScope scope)
		{
			_scope = scope;
		}

		#endregion

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
			return _scope.GetAttributeNames();
		}
        /// <summary>
        /// Sets an attribute on this object.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>true if the attribute value changed otherwise false</returns>
        public bool SetAttribute(string name, object value)
		{
			return _scope.SetAttribute(name, value);
		}
#if !(NET_1_1)
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public void SetAttributes(IDictionary<string, object> values)
        {
            _scope.SetAttributes(values);
        }
#endif
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public void SetAttributes(IDictionary values)
		{
			_scope.SetAttributes(values);
		}
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Attribute store.</param>
        public void SetAttributes(IAttributeStore values)
		{
			_scope.SetAttributes(values);
		}
        /// <summary>
        /// Returns the value for a given attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>The attribute value.</returns>
        public object GetAttribute(string name)
		{
			return _scope.GetAttribute(name);
		}
        /// <summary>
        /// Returns the value for a given attribute and sets it if it doesn't exist.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="defaultValue">Attribute's default value.</param>
        /// <returns>The attribute value.</returns>
        public object GetAttribute(string name, object defaultValue)
		{
			return _scope.GetAttribute(name, defaultValue);
		}
        /// <summary>
        /// Checks whetner the object has an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>true if a child scope exists, otherwise false.</returns>
		public bool HasAttribute(string name)
		{
			return _scope.HasAttribute(name);
		}
        /// <summary>
        /// Removes an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>true if the attribute was found and removed otherwise false.</returns>
        public bool RemoveAttribute(string name)
		{
			return _scope.RemoveAttribute(name);
		}
        /// <summary>
        /// Removes all attributes.
        /// </summary>
        public void RemoveAttributes()
		{
			_scope.RemoveAttributes();
		}
        /// <summary>
        /// Gets whether the attribute store is empty;
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _scope.IsEmpty;
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
            get { return _scope.AttributesCount; }
        }
#if !(NET_1_1)
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        public void CopyTo(object[] array, int index)
        {
            _scope.CopyTo(array, index);
        }
#else
        /// <summary>
        /// Copies the collection of attribute values to a one-dimensional array, starting at the specified index in the array.
        /// </summary>
        /// <param name="array">The Array that receives the values.</param>
        /// <param name="index">The zero-based index in array from which copying starts.</param>
        public void CopyTo(Array array, int index)
        {
            _scope.CopyTo(array, index);
        }
#endif

        /// <summary>
        /// Returns an enumerator that iterates through children scopes.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _scope.GetEnumerator();
        }
        
		#endregion

        /// <summary>
        /// Creates child scope.
        /// </summary>
        /// <param name="name">Child scope name.</param>
        /// <returns>true on success, false otherwise.</returns>
		public bool CreateChildScope(string name)
		{
			return _scope.CreateChildScope(name);
		}
        /// <summary>
        /// Returns child scope.
        /// </summary>
        /// <param name="name">Child scope name.</param>
        /// <returns>Child scope with given name.</returns>
		public IScope GetChildScope(string name) 
		{
			return _scope.GetScope(name);
		}
        /// <summary>
        /// Returns collection of child scope names.
        /// </summary>
        /// <returns>Collection of child scope names</returns>
		public ICollection<string> GetChildScopeNames()
		{
			return _scope.GetScopeNames();
		}
        /// <summary>
        /// Returns collection of clients.
        /// </summary>
        /// <returns>Collection of clients.</returns>
		public ICollection<IClient> GetClients()
		{
			return _scope.GetClients();
		}
        /// <summary>
        /// Returns collection of connections.
        /// </summary>
        /// <returns>Collection of connections.</returns>
        public IEnumerator GetConnections()
		{
			return _scope.GetConnections();
		}
        /// <summary>
        /// Returns the wrapped scope's context.
        /// </summary>
        /// <returns>The current context.</returns>
		public IScopeContext GetContext() 
		{
			return _scope.Context;
		}
        /// <summary>
        /// Get the scopes depth, how far down the scope tree is it. The lowest depth
        /// is 0x00, the depth of Global scope. Application scope depth is 0x01. Room
        /// depth is 0x02, 0x03 and so forth.
        /// </summary>
		public int Depth
		{
			get {return _scope.Depth; }
		}
        /// <summary>
        /// Gets the name of the wrapped scope.
        /// </summary>
		public string Name
		{
			get{ return _scope.Name; }
		}
        /// <summary>
        /// Get this wrapped scope's parent.
        /// </summary>
		public IScope Parent
		{
			get{ return _scope.Parent; }
		}
        /// <summary>
        /// Gets the wrapped scope's full absolute path.
        /// </summary>
		public string Path
		{
			get{ return _scope.Path; }
		}
        /// <summary>
        /// Checks whether the wrapped scope has a child scope with given name.
        /// </summary>
        /// <param name="name">Child scope name.</param>
        /// <returns><code>true</code> if a child scope exists, otherwise <code>false</code></returns>
		public bool HasChildScope(string name) 
		{
			return _scope.HasChildScope(name);
		}
        /// <summary>
        /// Checks whether the wrapped scope has a parent.
        /// You can think of scopes as of tree items
        /// where scope may have a parent and children (child).
        /// </summary>
		public bool HasParent
		{
			get{ return _scope.HasParent; }
		}
        /// <summary>
        /// Returns collection of connections for the specified client.
        /// </summary>
        /// <param name="client">The client object.</param>
        /// <returns>Collection of connections.</returns>	
		public ICollection LookupConnections(IClient client) 
		{
			return _scope.LookupConnections(client);
		}
	}
}
