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
using FluorineFx.Messaging.Config;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This class is used by the IFlexFactory to store the configuration for an instance created by the factory.
	/// There is one of these for each destination currently since only destinations create these components.
	/// </summary>
	public class FactoryInstance
	{
        /// <summary>
        /// Request scope constant.
        /// </summary>
        public const string RequestScope = "request";
        /// <summary>
        /// Session scope constant.
        /// </summary>
        public const string SessionScope = "session";
        /// <summary>
        /// Application scope constant.
        /// </summary>
        public const string ApplicationScope = "application";

		IFlexFactory _factory;
		string _id;
        DestinationProperties _properties;
		string _scope;
		string _source;
		string _attributeId;

		/// <summary>
        /// Initializes a new instance of the FactoryInstance class.
		/// </summary>
		/// <param name="factory">The IFlexFactory this FactoryInstance is created from.</param>
		/// <param name="id">The Destination's id.</param>
		/// <param name="properties">The configuration properties for this destination.</param>
        public FactoryInstance(IFlexFactory factory, string id, DestinationProperties properties)
		{
			_factory = factory;
			_id = id;
			_properties = properties;
		}
        /// <summary>
        /// Gets the Destination identity.
        /// </summary>
		public string Id
		{
			get{ return _id; }
		}
        /// <summary>
        /// Gets or sets the FactoryInstance scope.
        /// </summary>
		public virtual string Scope
		{
			get{ return _scope; }
			set{ _scope = value; }
		}
        /// <summary>
        /// Gets or sets the FactoryInstance source.
        /// </summary>
		public virtual string Source
		{
			get{ return _source; }
			set{ _source = value; }
		}
        /// <summary>
        /// Gets or sets the FactoryInstance attribute identity.
        /// </summary>
        /// <remarks>Which attribute the component is stored in.</remarks>
		public string AttributeId
		{
			get{ return _attributeId; }
			set{ _attributeId = value; }
		}
        /// <summary>
        /// Gets the configuration properties for this destination.
        /// </summary>
        public DestinationProperties Properties
		{
			get{ return _properties; }
		}
		/// <summary>
		/// If possible, returns the class for the underlying configuration. 
		/// This method can return null if the class is not known until the lookup method is called. 
		/// The goal is so the factories which know the class at startup time can provide earlier error detection. 
		/// If the class is not known, this method can return null and validation will wait until the first lookup call.
		/// </summary>
        /// <returns>A Type instance.</returns>
		public virtual Type GetInstanceClass()
		{
			return null;
		}
		/// <summary>
		/// Return an instance as appropriate for this instance of the given factory. This just calls the lookup method on the factory 
		/// that this instance was created on. You override this method to return the specific component for this destination. 
		/// </summary>
        /// <returns>Instance of the given factory.</returns>
		public virtual object Lookup()
		{
			return _factory.Lookup(this);
		}
		/// <summary>
		/// When the caller is done with the instance, this method is called. For session scoped components, this gives you the opportunity to 
		/// update any state modified in the instance in a remote persistence store. 
		/// </summary>
        /// <param name="instance">Instance of the given factory.</param>
		public virtual void OnOperationComplete(object instance)
		{
		}
	}
}
