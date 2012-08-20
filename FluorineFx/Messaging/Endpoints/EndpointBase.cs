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
using System.Text;
using System.Collections;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using FluorineFx.Util;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services;
using FluorineFx.Security;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Endpoints
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	abstract class EndpointBase : IEndpoint
	{
		protected MessageBroker _messageBroker;
        protected ChannelDefinition _channelDefinition;
        object _syncLock = new object();

        public EndpointBase(MessageBroker messageBroker, ChannelDefinition channelDefinition)
		{
			_messageBroker = messageBroker;
            _channelDefinition = channelDefinition;
		}

		#region IEndpoint Members

		public string Id
		{
			get
			{
                return _channelDefinition.Id;
			}

		}

		public MessageBroker GetMessageBroker()
		{
			return _messageBroker;
		}

        public ChannelDefinition ChannelDefinition
		{
            get { return _channelDefinition; }
		}

		public virtual void Start()
		{
		}

		public virtual void Stop()
		{
		}

		public virtual void Push(IMessage message, MessageClient messageClient)
		{
			throw new NotSupportedException();
		}

		public virtual void Service()
		{
		}

		public virtual IMessage ServiceMessage(IMessage message)
		{
			ValidationUtils.ArgumentNotNull(message, "message");
			IMessage response = null;
			response = _messageBroker.RouteMessage(message, this);
			return response;
		}

		public virtual bool IsSecure
		{
            get
            {
                return false;
            }
		}

		#endregion

        public bool IsLegacyCollection
        {
            get
            {
                if( _channelDefinition.Properties != null && _channelDefinition.Properties.Serialization != null )
                    return _channelDefinition.Properties.Serialization.IsLegacyCollection;
                return false;
            }
        }

        public bool IsLegacyThrowable
        {
            get
            {
                if (_channelDefinition.Properties != null && _channelDefinition.Properties.Serialization != null)
                    return _channelDefinition.Properties.Serialization.IsLegacyThrowable;
                return false;
            }
        }

        /// <summary>
        /// This property supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract int ClientLeaseTime { get; }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the connection.
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }
	}
}
