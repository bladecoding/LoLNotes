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
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Config;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// The Destination class is a source and sink for messages sent through 
	/// a service destination and uses an adapter to process messages.
	/// </summary>
    [CLSCompliant(false)]
    public class Destination
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(Destination));

        /// <summary>
        /// Service managing this Destination.
        /// </summary>
		protected IService				_service;
        /// <summary>
        /// Destination settings.
        /// </summary>
		protected DestinationDefinition _destinationDefinition;
        /// <summary>
        /// ServiceAdapter for the Destination.
        /// </summary>
		protected ServiceAdapter		_adapter;
		private FactoryInstance			_factoryInstance;
        private bool _initialized;

        private Destination()
        {
        }

        internal Destination(IService service, DestinationDefinition destinationDefinition)
		{
			_service = service;
            _destinationDefinition = destinationDefinition;
            _initialized = false;
		}
        /// <summary>
        /// Gets the Destination identity.
        /// </summary>
		public string Id
		{
            get { return _destinationDefinition.Id; }
		}
        /// <summary>
        /// Gets the Destination's factory property.
        /// </summary>
		public string FactoryId
		{
			get
			{
                if (_destinationDefinition.Properties.Factory != null)
                    return _destinationDefinition.Properties.Factory;
				return DotNetFactory.Id;
			}
		}
        /// <summary>
        /// Returns the Service managing this Destination.
        /// </summary>
		public IService Service{ get{ return _service; } }

        /// <summary>
        /// Initializes the current Destination.
        /// </summary>
        /// <param name="adapterDefinition">Adapter definition.</param>
        public virtual void Init(AdapterDefinition adapterDefinition)
		{
            if (_initialized)
                throw new NotSupportedException(__Res.GetString(__Res.Destination_Reinit, this.Id, this.GetType().Name));
            _initialized = true;
            if (adapterDefinition != null)
            {
                string typeName = adapterDefinition.Class;
                Type type = ObjectFactory.Locate(typeName);
                if (type != null)
                {
                    _adapter = ObjectFactory.CreateInstance(type) as ServiceAdapter;
                    _adapter.SetDestination(this);
                    _adapter.SetAdapterSettings(adapterDefinition);
                    _adapter.SetDestinationSettings(_destinationDefinition);
                    _adapter.Init();

                }
                else
                    log.Error(__Res.GetString(__Res.Type_InitError, adapterDefinition.Class));
            }
            else
            {
                log.Error(__Res.GetString(__Res.MessageServer_MissingAdapter, this.Id, this.GetType().Name));
            }
            MessageBroker messageBroker = this.Service.GetMessageBroker();
            messageBroker.RegisterDestination(this, _service);

            //If the source has application scope create an instance here, so the service can listen for SessionCreated events for the first request
            if (this.Scope == "application")
            {
                FactoryInstance factoryInstance = GetFactoryInstance();
                object inst = factoryInstance.Lookup();
            }
        }
        /// <summary>
        /// Gets the ServiceAdapter used by the Destination for message processing.
        /// </summary>
        public ServiceAdapter ServiceAdapter { get { return _adapter; } }
        /// <summary>
        /// Gets the Destination settings.
        /// </summary>
        public DestinationDefinition DestinationDefinition { get { return _destinationDefinition; } }

        /// <summary>
        /// Gets the source property where applicable.
        /// </summary>
        public string Source
        {
            get
            {
                return _destinationDefinition.Properties.Source;
            }
        }
        /// <summary>
        /// Gets the scope property where applicable.
        /// </summary>
        public string Scope
        {
            get
            {
                return _destinationDefinition.Properties.Source;
                //return "request";
            }
        }

		internal virtual void Dump(DumpContext dumpContext)
		{
			dumpContext.AppendLine("Destination Id = " + this.Id);
		}
        /// <summary>
        /// Returns the FactoryInstance used by the Destination to create object instances.
        /// </summary>
        /// <returns></returns>
		public FactoryInstance GetFactoryInstance()
		{
			if( _factoryInstance != null )
				return _factoryInstance;

			MessageBroker messageBroker = this.Service.GetMessageBroker();
			IFlexFactory factory = messageBroker.GetFactory(this.FactoryId);
			_factoryInstance = factory.CreateFactoryInstance(this.Id, _destinationDefinition.Properties);
			return _factoryInstance;
		}

	}
}
