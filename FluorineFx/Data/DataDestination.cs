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
using System.Text;
using log4net;
using FluorineFx.Util;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Services;
using FluorineFx.Data.Messages;

namespace FluorineFx.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DataDestination : MessageDestination
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(DataDestination));
		SequenceManager _sequenceManager;

        public DataDestination(IService service, DestinationDefinition destinationDefinition)
            : base(service, destinationDefinition)
		{
			_sequenceManager = new SequenceManager(this);
		}

		public SequenceManager SequenceManager
		{
			get{ return _sequenceManager; }
		}

        public IdentityConfiguration[] GetIdentityKeys()
		{
			if( this.DestinationDefinition.Properties.Metadata != null &&
                this.DestinationDefinition.Properties.Metadata.Identity != null)
			{
				//ArrayList identity = this.DestinationSettings.MetadataSettings.Identity;
				//return identity.ToArray(typeof(string)) as string[];
                return this.DestinationDefinition.Properties.Metadata.Identity;
			}
            return IdentityConfiguration.Empty;
			//return new string[0];
		}

		public bool AutoRefreshFill(IList parameters)
		{
			if( this.ServiceAdapter is DotNetAdapter )
				return (this.ServiceAdapter as DotNetAdapter).AutoRefreshFill(parameters);
			return false;
		}

		public override MessageClient RemoveSubscriber(string clientId)
		{
			if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.DataDestination_RemoveSubscriber, clientId));

			MessageClient messageClient = base.RemoveSubscriber(clientId);
			_sequenceManager.RemoveSubscriber(clientId);
			return messageClient;
		}


		internal override void Dump(DumpContext dumpContext)
		{
			base.Dump(dumpContext);
			dumpContext.Indent();
			_sequenceManager.Dump(dumpContext);
			dumpContext.Unindent();
		}
	}
}
