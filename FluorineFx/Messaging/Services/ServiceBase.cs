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
using System.Threading;

using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Messages;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;

namespace FluorineFx.Messaging.Services
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class ServiceBase : IService
	{
        /// <summary>
        /// Reference to the MessageBroker.
        /// </summary>
		protected MessageBroker	_messageBroker;
        /// <summary>
        /// Service settings.
        /// </summary>
        protected ServiceDefinition _serviceDefinition;
        /// <summary>
        /// Destinations in this service.
        /// </summary>
        protected Hashtable _destinations;
        object _objLock = new object();
        /// <summary>
        /// Reference to the default Destination is available.
        /// </summary>
        protected Destination _defaultDestination;

        internal ServiceBase()
        {
        }

        internal ServiceBase(MessageBroker messageBroker, ServiceDefinition serviceDefinition)
		{
			_messageBroker = messageBroker;
            _serviceDefinition = serviceDefinition;

			_destinations = new Hashtable();
            if (this.ServiceDefinition.Destinations != null)
            {
                foreach (DestinationDefinition destinationDefinition in this.ServiceDefinition.Destinations)
                {
                    AdapterDefinition adapterDefinition = null;
                    AdapterRef adapterRef = destinationDefinition.AdapterRef;
                    if (adapterRef != null)
                        adapterDefinition = serviceDefinition.GetAdapterByRef(adapterRef.Ref);
                    else
                        adapterDefinition = serviceDefinition.GetDefaultAdapter();
                    CreateDestination(destinationDefinition, adapterDefinition);
                }
            }
		}
        /// <summary>
        /// Creates a new Destination.
        /// </summary>
        /// <param name="destinationDefinition">Destination settings.</param>
        /// <returns>The new Destination instance.</returns>
        protected virtual Destination NewDestination(DestinationDefinition destinationDefinition)
		{
            return new Destination(this, destinationDefinition);
		}
        /// <summary>
        /// Gets service settings.
        /// </summary>
        public ServiceDefinition ServiceDefinition { get { return _serviceDefinition; } }

		#region IService Members

        /// <summary>
        /// Gets the service identity.
        /// </summary>
		public string id
		{
			get
			{
                return _serviceDefinition.Id;
			}
		}
        /// <summary>
        /// Retrievs the MessageBroker managing this service.
        /// This MessageBroker is used to push a message to one or more endpoints managed by the broker. 
        /// </summary>
        /// <returns>The MessageBroker managing this service.</returns>
		public MessageBroker GetMessageBroker()
		{
			return _messageBroker;
		}
        /// <summary>
        /// Retrieves the destination in this service for which the given message is intended.
        /// </summary>
        /// <param name="message">The message that should be handled by the destination.</param>
        /// <returns>The destination if it is found; otherwise, null.</returns>
		public Destination GetDestination(IMessage message)
		{
			lock(_objLock)
			{
				return _destinations[message.destination] as Destination;
			}
		}
        /// <summary>
        /// Returns all destinations in this service.
        /// </summary>
        /// <returns>The collection of destinations.</returns>
		public Destination[] GetDestinations()
		{
			lock(_objLock)
			{
				ArrayList result = new ArrayList(_destinations.Values);
				return result.ToArray(typeof(Destination)) as Destination[];
			}
		}
        /// <summary>
        /// Returns the destination for the specified source.
        /// </summary>
        /// <param name="source">The destination's source property.</param>
        /// <returns>The destination if it is found; otherwise, null.</returns>
		public Destination GetDestinationWithSource(string source)
		{
			lock(_objLock)
			{
				foreach(Destination destination in _destinations.Values)
				{
					string sourceTmp = destination.DestinationDefinition.Properties.Source;
					if( source == sourceTmp )
						return destination;
				}
				return null;
			}
		}
        /// <summary>
        /// Gets the default destination if available.
        /// </summary>
		public Destination DefaultDestination
		{
			get{ return _defaultDestination; }
		}
        /// <summary>
        /// Returns the destination with the specified Id.
        /// </summary>
        /// <param name="id">The destination's identity.</param>
        /// <returns>The destination if it is found; otherwise, null.</returns>
        public Destination GetDestination(string id)
		{
			lock(_objLock)
			{
				return _destinations[id] as Destination;
			}
		}
        /// <summary>
        /// Handles a message routed to the service by the MessageBroker.
        /// </summary>
        /// <param name="message">The message that should be handled by the service.</param>
        /// <returns>The result of the message processing.</returns>
		public virtual object ServiceMessage(IMessage message)
		{
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null && commandMessage.operation == CommandMessage.ClientPingOperation )
				return true;
			throw new NotSupportedException();
		}
        /// <summary>
        /// Returns whether this Service is capable of handling the given Message instance.
        /// </summary>
        /// <param name="message">The message that should be handled by the service.</param>
        /// <returns>true if the Service is capable of handling the message; otherwise, false.</returns>
		public bool IsSupportedMessage(IMessage message)
		{
			return IsSupportedMessageType(message.GetType().FullName);
		}
        /// <summary>
        /// Returns whether this Service is capable of handling messages of a given type.
        /// </summary>
        /// <param name="messageClassName">The message type.</param>
        /// <returns>true if the Service is capable of handling messages of a given type; otherwise, false.</returns>
        public bool IsSupportedMessageType(string messageClassName)
		{
            return this.ServiceDefinition.IsSupportedMessageType(messageClassName);
		}

        /// <summary>
        /// Performs any startup actions necessary after the service has been added to the broker.
        /// </summary>
        public virtual void Start()
		{
		}
        /// <summary>
        /// Performs any actions necessary before removing the service from the broker.
        /// </summary>
        public virtual void Stop()
		{
		}

		#endregion

        /// <summary>
        /// Creates a destination with the specified settings.
        /// </summary>
        /// <param name="destinationDefinition">Destination settings.</param>
        /// <param name="adapterDefinition">Adapter settings.</param>
        /// <returns>The created destination instance.</returns>
        public virtual Destination CreateDestination(DestinationDefinition destinationDefinition, AdapterDefinition adapterDefinition)
		{
			lock(_objLock)
			{
                if (!_destinations.ContainsKey(destinationDefinition.Id))
				{
                    Destination destination = NewDestination(destinationDefinition);
                    destination.Init(adapterDefinition);
                    /*
                    if (destinationDefinition.Adapter != null)
                        destination.Init(destinationSettings.Adapter);
					else
                        destination.Init(_serviceSettings.DefaultAdapter);
                    */
					_destinations[destination.Id] = destination;
					
					string source = destination.DestinationDefinition.Properties.Source;
					//TODO: warn if more then one "*" source occurs.
					if( source != null && source == "*" )
						_defaultDestination = destination;
					return destination;
				}
				else
                    return _destinations[destinationDefinition.Id] as Destination;
			}
		}
        /// <summary>
        /// Checks security before handling the given Message instance.
        /// </summary>
        /// <param name="message">The message that should be handled by the service.</param>
		public virtual void CheckSecurity(IMessage message)
		{
			Destination destination = this.GetDestination(message);
			if( destination == null )
				throw new FluorineException(__Res.GetString(__Res.Invalid_Destination, message.destination));
			CheckSecurity(destination);
		}
        /// <summary>
        /// Checks security for the given destination instance.
        /// </summary>
        /// <param name="destination">The destination that should process messages.</param>
		public virtual void CheckSecurity(Destination destination)
		{
            this.GetMessageBroker().LoginManager.CheckSecurity(destination);
		}
	}
}
