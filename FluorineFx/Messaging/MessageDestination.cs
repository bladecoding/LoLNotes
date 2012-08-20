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
// Import log4net classes.
using log4net;

using FluorineFx.Messaging;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Services.Messaging;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class MessageDestination : Destination
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageDestination));
        SubscriptionManager _subscriptionManager;

        public MessageDestination(IService service, DestinationDefinition destinationDefinition)
            : base(service, destinationDefinition)
		{
			_subscriptionManager = new SubscriptionManager(this);
		}

		public SubscriptionManager SubscriptionManager{ get { return _subscriptionManager; } }

		public virtual MessageClient RemoveSubscriber(string clientId)
		{
			if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.MessageDestination_RemoveSubscriber, clientId));

			return _subscriptionManager.RemoveSubscriber(clientId);
		}
	}
}
