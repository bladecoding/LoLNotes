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

using FluorineFx.Messaging.Config;

namespace FluorineFx.Messaging.Services.Remoting
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class RemotingDestination : Destination
	{

        /// <summary>
        /// Initializes a new instance of the RemotingDestination class.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="destinationDefinition">Destination definition.</param>
        public RemotingDestination(IService service, DestinationDefinition destinationDefinition)
            : base(service, destinationDefinition)
		{
		}

        /// <summary>
        /// Initializes the current Destination.
        /// </summary>
        /// <param name="adapterDefinition">Adapter definition.</param>
        public override void Init(AdapterDefinition adapterDefinition)
        {
            //For remoting destinations it is ok to use the default 'dotnet' adapter if no adapter was specified for the service
            if (adapterDefinition == null && this.DestinationDefinition != null && this.DestinationDefinition.Service != null )
                adapterDefinition = this.DestinationDefinition.Service.GetDefaultAdapter();
            base.Init(adapterDefinition);
        }
	}
}
