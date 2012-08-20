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
using System.Web;
using System.IO;
using log4net;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Services;
using FluorineFx.Security;
using FluorineFx.Configuration;
using FluorineFx.Context;
using FluorineFx.Util;
using FluorineFx.Exceptions;
using FluorineFx.Silverlight;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public sealed class MessageServer : DisposableBase
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageServer));

        private readonly object _syncLock = new object();
        ServicesConfiguration _servicesConfiguration;
		MessageBroker	_messageBroker;
        PolicyServer _policyServer;

	    /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        internal ServicesConfiguration ServicesConfiguration
        {
            get { return _servicesConfiguration; }
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="configPath"></param>
        public void Init(string configPath)
        {
            string[] configPaths = { configPath };
            Init(configPaths, false);
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="configPath">Configuration file location.</param>
        /// <param name="serviceBrowserAvailable">Indicates whether the service browser is avaliable.</param>
        /// <remarks>This method is called from the Hosting library</remarks>
        public void Init(string configPath, bool serviceBrowserAvailable)
        {
            string[] configPaths = { configPath };
            Init(configPaths, serviceBrowserAvailable);
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="configFolderPaths">Possible configuration file locations.</param>
        /// <param name="serviceBrowserAvailable">Indicates whether the service browser is avaliable.</param>
        public void Init(string[] configFolderPaths, bool serviceBrowserAvailable)
		{
			_messageBroker = new MessageBroker(this);

            string servicesConfigFile = null;
            for (int i = 0; i < configFolderPaths.Length; i++)
            {
                servicesConfigFile = Path.Combine(configFolderPaths[i], "services-config.xml");
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.MessageServer_TryingServiceConfig, servicesConfigFile));
                if (File.Exists(servicesConfigFile))
                    break;
            }
            if (servicesConfigFile != null && File.Exists(servicesConfigFile))
            {
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.MessageServer_LoadingServiceConfig, servicesConfigFile));
                _servicesConfiguration = ServicesConfiguration.Load(servicesConfigFile);
            }
            else
            {
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.MessageServer_LoadingConfigDefault));
                _servicesConfiguration = ServicesConfiguration.CreateDefault();
            }

            foreach (ChannelDefinition channelDefinition in _servicesConfiguration.Channels)
            {
                Type type = ObjectFactory.Locate(FluorineConfiguration.Instance.ClassMappings.GetType(channelDefinition.Endpoint.Class));
                if (type != null)
                {
                    IEndpoint endpoint = ObjectFactory.CreateInstance(type, new object[] { _messageBroker, channelDefinition }) as IEndpoint;
                    if (endpoint != null)
                        _messageBroker.AddEndpoint(endpoint);
                }
                else
                    log.Error(__Res.GetString(__Res.Type_InitError, channelDefinition.Class));
            }
            ChannelDefinition rtmptChannelDefinition = new ChannelDefinition();
            rtmptChannelDefinition.Id = RtmptEndpoint.FluorineRtmptEndpointId;
            IEndpoint rtmptEndpoint = new RtmptEndpoint(_messageBroker, rtmptChannelDefinition);
            _messageBroker.AddEndpoint(rtmptEndpoint);

            if (_servicesConfiguration.Factories != null)
            {
                foreach (Factory factory in _servicesConfiguration.Factories)
                {
                    Type type = ObjectFactory.Locate(FluorineConfiguration.Instance.ClassMappings.GetType(factory.Class));
                    if (type != null)
                    {
                        IFlexFactory flexFactory = ObjectFactory.CreateInstance(type, new object[0]) as IFlexFactory;
                        if (flexFactory != null)
                            _messageBroker.AddFactory(factory.Id, flexFactory);
                    }
                    else
                        log.Error(__Res.GetString(__Res.Type_InitError, factory.Class));
                }
            }
            //Add the dotnet Factory
            _messageBroker.AddFactory(DotNetFactory.Id, new DotNetFactory());
            
            if (serviceBrowserAvailable)
            {
                ServiceDefinition serviceConfiguration = _servicesConfiguration.GetServiceByClass("flex.messaging.services.RemotingService");
                if (serviceConfiguration != null)
                {
                    AdapterDefinition adapter = serviceConfiguration.GetAdapterByClass(typeof(Remoting.RemotingAdapter).FullName);
                    if (adapter != null)
                        InstallServiceBrowserDestinations(serviceConfiguration, adapter);
                    else
                    {
                        adapter = serviceConfiguration.GetDefaultAdapter();
                        if (adapter != null)
                            InstallServiceBrowserDestinations(serviceConfiguration, adapter);
                    }
                }
            }
            if (_servicesConfiguration.Services.ServiceDefinitions != null)
            {
                foreach (ServiceDefinition serviceDefinition in _servicesConfiguration.Services.ServiceDefinitions)
                {
                    Type type = ObjectFactory.Locate(FluorineConfiguration.Instance.ClassMappings.GetType(serviceDefinition.Class));//current assembly only
                    if (type != null)
                    {
                        IService service = ObjectFactory.CreateInstance(type, new object[] { _messageBroker, serviceDefinition }) as IService;
                        if (service != null)
                            _messageBroker.AddService(service);
                    }
                    else
                        log.Error(__Res.GetString(__Res.Type_InitError, serviceDefinition.Class));
                }
            }
            if (_servicesConfiguration.Services.Includes != null)
            {
                foreach (ServiceInclude include in _servicesConfiguration.Services.Includes)
                {
                    ServiceDefinition serviceDefinition = include.ServiceDefinition;
                    Type type = ObjectFactory.Locate(FluorineConfiguration.Instance.ClassMappings.GetType(serviceDefinition.Class));//current assembly only
                    if (type != null)
                    {
                        IService service = ObjectFactory.CreateInstance(type, new object[] { _messageBroker, serviceDefinition }) as IService;
                        if (service != null)
                            _messageBroker.AddService(service);
                    }
                    else
                        log.Error(__Res.GetString(__Res.Type_InitError, serviceDefinition.Class));
                }
            }
            if (_servicesConfiguration.Security != null)
            {
                if (_servicesConfiguration.Security.LoginCommands != null && _servicesConfiguration.Security.LoginCommands.Length > 0)
                {
                    LoginCommand loginCommand = _servicesConfiguration.Security.GetLoginCommand(LoginCommand.FluorineLoginCommand);
                    if (loginCommand != null)
                    {
                        Type type = ObjectFactory.Locate(FluorineConfiguration.Instance.ClassMappings.GetType(loginCommand.Class));
                        if (type != null)
                        {
                            ILoginCommand loginCommandObj = ObjectFactory.CreateInstance(type, new object[] { }) as ILoginCommand;
                            _messageBroker.LoginManager.LoginCommand = loginCommandObj;
                            _messageBroker.LoginManager.IsPerClientAuthentication = loginCommand.IsPerClientAuthentication;
                        }
                        else
                            log.Error(__Res.GetString(__Res.Type_InitError, loginCommand.Class));
                    }
                    else
                        log.Error(__Res.GetString(__Res.Type_InitError, "<<LoginCommand class>>"));
                }
            }
            InitAuthenticationService();

            try
            {
                if (FluorineConfiguration.Instance.FluorineSettings.Silverlight.PolicyServerSettings != null &&
                    FluorineConfiguration.Instance.FluorineSettings.Silverlight.PolicyServerSettings.Enable)
                {
                    IResource resource;
                    if (FluorineContext.Current != null)
                        resource = FluorineContext.Current.GetResource(FluorineConfiguration.Instance.FluorineSettings.Silverlight.PolicyServerSettings.PolicyFile);
                    else
                        resource = new FileSystemResource(FluorineConfiguration.Instance.FluorineSettings.Silverlight.PolicyServerSettings.PolicyFile);
                    if (resource.Exists)
                    {
                        log.Info(__Res.GetString(__Res.Silverlight_StartPS, resource.File.FullName));
                        _policyServer = new PolicyServer(resource.File.FullName);
                    }
                    else
                        throw new FileNotFoundException("Policy file not found", FluorineConfiguration.Instance.FluorineSettings.Silverlight.PolicyServerSettings.PolicyFile);
                }
            }
            catch (Exception ex)
            {
                log.Error(__Res.GetString(__Res.Silverlight_PSError), ex);
            }
		}

        private void InstallServiceBrowserDestinations(ServiceDefinition service, AdapterDefinition adapter)
        {
            //ServiceBrowser destinations
            DestinationDefinition destination = new DestinationDefinition(service);
            destination.Id = DestinationDefinition.FluorineServiceBrowserDestination;
            destination.Properties.Source = DestinationDefinition.FluorineServiceBrowserDestination;
            destination.AdapterRef = new AdapterRef(adapter);
            service.AddDestination(destination);

            destination = new DestinationDefinition(service);
            destination.Id = DestinationDefinition.FluorineManagementDestination;
            destination.Properties.Source = DestinationDefinition.FluorineManagementDestination;
            destination.AdapterRef = new AdapterRef(adapter);
            service.AddDestination(destination);

            destination = new DestinationDefinition(service);
            destination.Id = DestinationDefinition.FluorineCodeGeneratorDestination;
            destination.Properties.Source = DestinationDefinition.FluorineCodeGeneratorDestination;
            destination.AdapterRef = new AdapterRef(adapter);
            service.AddDestination(destination);

            destination = new DestinationDefinition(service);
            destination.Id = DestinationDefinition.FluorineSqlServiceDestination;
            destination.Properties.Source = DestinationDefinition.FluorineSqlServiceDestination;
            destination.AdapterRef = new AdapterRef(adapter);
            service.AddDestination(destination);
        }

		private void InitAuthenticationService()
		{
            ServiceDefinition serviceDefinition = new ServiceDefinition(_servicesConfiguration);
            serviceDefinition.Id = AuthenticationService.ServiceId;
            serviceDefinition.Class = typeof(AuthenticationService).FullName;
            serviceDefinition.MessageTypes = "flex.messaging.messages.AuthenticationMessage";
            _servicesConfiguration.Services.AddService(serviceDefinition);
            AuthenticationService service = new AuthenticationService(_messageBroker, serviceDefinition);
			_messageBroker.AddService(service);
		}
        /// <summary>
        /// Gets the message broker started by this server.
        /// </summary>
		public MessageBroker MessageBroker{ get { return _messageBroker; } }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public void Start()
		{
            lock (SyncRoot)
            {
                if (log.IsInfoEnabled)
                    log.Info(__Res.GetString(__Res.MessageServer_Start));
                if (_messageBroker != null)
                {
                    _messageBroker.Start();
                }
                else
                    log.Error(__Res.GetString(__Res.MessageServer_StartError));
            }
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public void Stop()
		{
            lock (SyncRoot)
            {
                if (_messageBroker != null)
                {
                    if (log.IsInfoEnabled)
                        log.Info(__Res.GetString(__Res.MessageServer_Stop));
                    _messageBroker.Stop();
                    _messageBroker = null;
                    if (_policyServer != null)
                    {
                        _policyServer.Close();
                        _policyServer = null;
                    }
                }
            }
		}

		#region IDisposable Members

        /// <summary>
        /// Free managed resources.
        /// </summary>
		protected override void Free()
		{
            lock (SyncRoot)
            {
                if (_messageBroker != null)
                {
                    Stop();
                }
            }
		}

        /// <summary>
        /// Free unmanaged resources.
        /// </summary>
		protected override void FreeUnmanaged()
		{
            lock (SyncRoot)
            {
                if (_messageBroker != null)
                {
                    Stop();
                }
            }
		}


		#endregion
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public void Service()
		{
			if( _messageBroker == null )
			{
                string msg = __Res.GetString(__Res.MessageBroker_NotAvailable);
                log.Fatal(msg);
                throw new FluorineException(msg);
			}

			//This is equivalent to request.getContextPath() (Java) or the HttpRequest.ApplicationPath (.Net).
			string contextPath = HttpContext.Current.Request.ApplicationPath;
			string endpointPath = HttpContext.Current.Request.Path;
			bool isSecure = HttpContext.Current.Request.IsSecureConnection;

			if( log.IsDebugEnabled )
				log.Debug( __Res.GetString(__Res.Endpoint_Bind, endpointPath, contextPath));

			//http://www.adobe.com/cfusion/knowledgebase/index.cfm?id=e329643d&pss=rss_flex_e329643d
			IEndpoint endpoint = _messageBroker.GetEndpoint(endpointPath, contextPath, isSecure);
			if( endpoint != null )
			{
				endpoint.Service();
			}
			else
			{
				string msg = __Res.GetString(__Res.Endpoint_BindFail, endpointPath, contextPath);
                log.Fatal(msg);
                _messageBroker.TraceChannelSettings();
                throw new FluorineException(msg);
			}
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void ServiceRtmpt()
        {
            IEndpoint endpoint = _messageBroker.GetEndpoint(RtmptEndpoint.FluorineRtmptEndpointId);
            if (endpoint != null)
            {
                endpoint.Service();
            }
            else
            {
                string msg = __Res.GetString(__Res.Endpoint_BindFail, RtmptEndpoint.FluorineRtmptEndpointId, "");
                log.Fatal(msg);
                _messageBroker.TraceChannelSettings();
                throw new FluorineException(msg);
            }
        }
	}
}
