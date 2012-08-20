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
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Configuration;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class WebScope : Scope
	{
        private RtmpEndpoint _endpoint;
        ApplicationConfiguration _appConfig;
		protected string	_contextPath;
        /// <summary>
        /// Is the scope currently shutting down?
        /// </summary>
        protected bool _isShuttingDown;
        /// <summary>
        /// Has the web scope been registered?
        /// </summary>
        protected bool _isRegistered;

        internal WebScope(RtmpEndpoint endpoint, IGlobalScope globalScope, ApplicationConfiguration appConfig)
            : base(null)
        {
            _endpoint = endpoint;
            _appConfig = appConfig;
            base.Parent = globalScope;
        }

		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				throw new InvalidOperationException("Cannot set name, you must set context path");
			}
		}

		public override IScope Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				throw new InvalidOperationException("Cannot set parent, you must set global scope");
			}
		}

		public override string ContextPath
		{
			get{ return _contextPath; }
		}

        public override IEndpoint Endpoint
        {
            get
            {
                return _endpoint;
            }
        }

		public void SetContextPath(string contextPath)
		{
			_contextPath = contextPath;
			base.Name = _contextPath.Substring(1);
		}

		public void Register() 
		{
            lock (this.SyncRoot)
            {
                if (_isRegistered)
                {
                    // Already registered
                    return;
                }
                //Start services
                FluorineFx.Messaging.Api.Stream.IStreamFilenameGenerator streamFilenameGenerator = ObjectFactory.CreateInstance(_appConfig.StreamFilenameGenerator.Type) as FluorineFx.Messaging.Api.Stream.IStreamFilenameGenerator;
                AddService(typeof(FluorineFx.Messaging.Api.Stream.IStreamFilenameGenerator), streamFilenameGenerator, false);
                streamFilenameGenerator.Start(_appConfig.StreamFilenameGenerator);
                FluorineFx.Messaging.Api.SO.ISharedObjectService sharedObjectService = ObjectFactory.CreateInstance(_appConfig.SharedObjectServiceConfiguration.Type) as FluorineFx.Messaging.Api.SO.ISharedObjectService;
                AddService(typeof(FluorineFx.Messaging.Api.SO.ISharedObjectService), sharedObjectService, false);
                sharedObjectService.Start(_appConfig.SharedObjectServiceConfiguration);
                FluorineFx.Messaging.Rtmp.Stream.IProviderService providerService = ObjectFactory.CreateInstance(_appConfig.ProviderServiceConfiguration.Type) as FluorineFx.Messaging.Rtmp.Stream.IProviderService;
                AddService(typeof(FluorineFx.Messaging.Rtmp.Stream.IProviderService), providerService, false);
                providerService.Start(_appConfig.ProviderServiceConfiguration);
                FluorineFx.Messaging.Rtmp.Stream.IConsumerService consumerService = ObjectFactory.CreateInstance(_appConfig.ConsumerServiceConfiguration.Type) as FluorineFx.Messaging.Rtmp.Stream.IConsumerService;
                AddService(typeof(FluorineFx.Messaging.Rtmp.Stream.IConsumerService), consumerService, false);
                consumerService.Start(_appConfig.ConsumerServiceConfiguration);
                FluorineFx.Messaging.Api.Stream.IStreamService streamService = ObjectFactory.CreateInstance(_appConfig.StreamService.Type) as FluorineFx.Messaging.Api.Stream.IStreamService;
                AddService(typeof(FluorineFx.Messaging.Api.Stream.IStreamService), streamService, false);
                streamService.Start(_appConfig.StreamService);
                if (_appConfig.SharedObjectSecurityService.Type != null)
                {
                    FluorineFx.Messaging.Api.SO.ISharedObjectSecurityService sharedObjectSecurityService = ObjectFactory.CreateInstance(_appConfig.SharedObjectSecurityService.Type) as FluorineFx.Messaging.Api.SO.ISharedObjectSecurityService;
                    AddService(typeof(FluorineFx.Messaging.Api.SO.ISharedObjectSecurityService), sharedObjectSecurityService, false);
                    sharedObjectSecurityService.Start(_appConfig.SharedObjectSecurityService);
                }
                Init();
                // We don't want to have configured scopes to get freed when a client disconnects.
                _keepOnDisconnect = true;
                _isRegistered = true;
            }
		}

        public void Unregister()
        {
            lock (this.SyncRoot)
            {
                if (!_isRegistered)
                {
                    // Not registered
                    return;
                }

                _isShuttingDown = true;
                _keepOnDisconnect = false;
                Uninit();
                // We need to disconnect all clients before unregistering
                IEnumerator enumerator = GetConnections();
                while (enumerator.MoveNext())
                {
                    IConnection connection = enumerator.Current as IConnection;
                    connection.Close();
                }
                // Various cleanup tasks
                //setStore(null);
                base.Parent = null;
                //setServer(null);
                _isRegistered = false;
                _isShuttingDown = false;
            }
        }

        public bool IsShuttingDown
        {
            get { return _isShuttingDown; }
        }
	}
}
