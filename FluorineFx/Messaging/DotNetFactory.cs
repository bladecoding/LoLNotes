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
using FluorineFx.Context;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class DotNetFactory : IFlexFactory
	{
        public const string Id = "dotnet";
        /// <summary>
        /// Initializes a new instance of the DotNetFactory class.
        /// </summary>
        public DotNetFactory()
		{
		}

		#region IFlexFactory Members

        /// <summary>
        /// Creates a FactoryInstance.
        /// </summary>
        /// <param name="id">The Destination identity.</param>
        /// <param name="properties">Configuration properties for the destination.</param>
        /// <returns>A FactoryInstance instance.</returns>
		public FactoryInstance CreateFactoryInstance(string id, DestinationProperties properties)
		{
			DotNetFactoryInstance factoryInstance = new DotNetFactoryInstance(this, id, properties);
            if (properties != null)
            {
                factoryInstance.Source = properties.Source;
                factoryInstance.Scope = properties.Scope;
                factoryInstance.AttributeId = properties.AttributeId;
            }
			if( factoryInstance.Scope == null )
				factoryInstance.Scope = FactoryInstance.RequestScope;
			return factoryInstance;
		}
        /// <summary>
        /// Return an instance as appropriate for this instance of the given factory.
        /// </summary>
        /// <param name="factoryInstance">FactoryInstance used to retrieve the object instance.</param>
        /// <returns>The Object instance to use for the given operation for the current destination.</returns>
		public object Lookup(FactoryInstance factoryInstance)
		{
			DotNetFactoryInstance dotNetFactoryInstance = factoryInstance as DotNetFactoryInstance;
			switch(dotNetFactoryInstance.Scope)
			{
                case FactoryInstance.ApplicationScope:
                    {
                        object instance = dotNetFactoryInstance.ApplicationInstance;
                        if (FluorineContext.Current != null && FluorineContext.Current.ApplicationState != null && dotNetFactoryInstance.AttributeId != null)
                            FluorineContext.Current.ApplicationState[dotNetFactoryInstance.AttributeId] = instance;
                        return instance;
                    }
				case FactoryInstance.SessionScope:
					if( FluorineContext.Current.Session != null )
					{
						object instance = FluorineContext.Current.Session[dotNetFactoryInstance.AttributeId];
						if( instance == null )
						{
							instance = dotNetFactoryInstance.CreateInstance();
							FluorineContext.Current.Session[dotNetFactoryInstance.AttributeId] = instance;
						}
                        return instance;
                    }
					break;
				default:
					return dotNetFactoryInstance.CreateInstance();
			}
			return null;
		}

		#endregion
	}
}
