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
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using log4net;
using FluorineFx.IO;
using FluorineFx.Collections;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Security;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;
using FluorineFx.Util;

namespace FluorineFx.Messaging
{
	/// <summary>
    /// <para>
	/// All communication with the various services provided is mediated by the message broker.
    /// </para>
	/// <para>
	/// It has a number of endpoints which send and receive messages over the network, and it has 
	/// a number of services that are message destinations. The broker routes messages to 
	/// endpoints based on the content type of those messages, and routes decoded messages 
	/// to services based on message type.
    /// </para>
    /// <para>
	/// The broker also has a means of calling back into the endpoints in order to push messages 
	/// back through them. 
    /// </para>
	/// </summary>
	/// <example>
    /// <para>Pushing a message to connected clients (Flex Messaging)</para>
    /// <code lang="CS">
    /// MessageBroker msgBroker = MessageBroker.GetMessageBroker(null);
    /// AsyncMessage msg = new AsyncMessage();
    /// msg.destination = "chat";
    /// msg.headers.Add(AsyncMessage.SubtopicHeader, "status." + userId);
    /// msg.clientId = Guid.NewGuid().ToString("D");
    /// msg.messageId = Guid.NewGuid().ToString("D");
    /// msg.timestamp = Environment.TickCount;
    /// Hashtable body = new Hashtable();
    /// body.Add("userId", userId);
    /// body.Add("status", status);
    /// msg.body = body;
    /// msgBroker.RouteMessage(msg);
    /// </code>
    /// </example>
    [CLSCompliant(false)]
    public class MessageBroker
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageBroker));
        /// <summary>
        /// Default MessageBroker identity.
        /// </summary>
		public static string DefaultMessageBrokerId = "default";

        private static Hashtable	_messageBrokers = new Hashtable(1);
        private static object _syncLock = new object();
        private string _messageBrokerId;

		CopyOnWriteDictionary _services;
		CopyOnWriteDictionary _endpoints;
		CopyOnWriteDictionary _destinationServiceMap;
        CopyOnWriteDictionary _destinations;
        LoginManager _loginManager;
		private MessageServer		_messageServer;
        private CopyOnWriteDictionary _factories;
        ClientManager _clientManager;
        SessionManager _sessionManager;
        GlobalScope _globalScope;

		/// <summary>
		/// Initializes a new instance of the MessageBroker class.
		/// </summary>
		public MessageBroker(MessageServer messageServer)
		{
			_messageServer = messageServer;
            _services = new CopyOnWriteDictionary();
            _endpoints = new CopyOnWriteDictionary();
            _factories = new CopyOnWriteDictionary();
            _destinationServiceMap = new CopyOnWriteDictionary();
            _destinations = new CopyOnWriteDictionary();
            _clientManager = new ClientManager(this);
            _sessionManager = new SessionManager(this);
            _loginManager = new LoginManager();
		}

        /// <summary>
        /// Gets the Id for the MessageBroker.
        /// </summary>
        public string Id
        {
            get { return _messageBrokerId; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }
        /// <summary>
        /// Gets the Global scope.
        /// </summary>
        /// <remarks>
        /// The global scope is the parent of all Web scopes. For Flex Messaging applications the Global scope is accessible through the message broker.
        /// </remarks>
        public FluorineFx.Messaging.Api.IGlobalScope GlobalScope { get { return _globalScope; } }

        internal LoginManager LoginManager
		{
			get{ return _loginManager; }
		}

        internal FluorineFx.Messaging.Api.IClientRegistry ClientRegistry
        {
            get { return _clientManager; }
        }

        internal FlexClient FlexClientSettings
        {
            get { return _messageServer.ServicesConfiguration.FlexClient; }
        }

        internal ServicesConfiguration ServicesConfiguration
        {
            get { return _messageServer.ServicesConfiguration; }
        }

        internal SessionManager SessionManager
        {
            get { return _sessionManager; }
        }

        /// <summary>
        /// Registers the message broker.
        /// </summary>
		protected void RegisterMessageBroker()
		{
			if(_messageBrokerId == null)
				_messageBrokerId = DefaultMessageBrokerId;
			lock(_syncLock)
			{
				if(_messageBrokers.ContainsKey(_messageBrokerId))
				{
					throw new FluorineException(__Res.GetString(__Res.MessageBroker_RegisterError, _messageBrokerId));
				}
				_messageBrokers[_messageBrokerId] = this;
			}
		}
        /// <summary>
        /// Unregisters the message broker.
        /// </summary>
		protected void UnregisterMessageBroker()
		{
			if(_messageBrokerId == null)
				_messageBrokerId = DefaultMessageBrokerId;
			lock(_syncLock)
			{
				_messageBrokers.Remove(_messageBrokerId);
			}
		}

		internal void RegisterDestination(Destination destination, IService service)
		{
			_destinationServiceMap[destination.Id] = service.id;
            _destinations[destination.Id] = destination;
		}

		/// <summary>
		/// Gets the MessageBroker object for the current request.
		/// </summary>
		/// <param name="messageBrokerId">Ignored.</param>
        /// <returns>The MessageBroker instance if it is found; otherwise, null.</returns>
		public static MessageBroker GetMessageBroker(string messageBrokerId)
		{
			lock(_syncLock)
			{
				if( messageBrokerId == null )
					messageBrokerId = DefaultMessageBrokerId;
				return _messageBrokers[messageBrokerId] as MessageBroker;
			}		
		}

		internal MessageServer MessageServer
		{
			get{ return _messageServer; }
		}

		internal void AddService(IService service)
		{
			_services[service.id] = service;
		}

		internal IService GetService(string id)
		{
			return _services[id] as IService;
		}

		internal void AddEndpoint(IEndpoint endpoint)
		{
			_endpoints[endpoint.Id] = endpoint;
		}

		internal void AddFactory(string id, IFlexFactory factory)
		{
			_factories.Add(id, factory);
		}
        /// <summary>
        /// Returns the IFlexFactory with the specified Id.
        /// </summary>
        /// <param name="id">FlexFactory identity.</param>
        /// <returns>The FlexFactory instance if it is found; otherwise, null.</returns>
		public IFlexFactory GetFactory(string id)
		{
			return _factories[id] as IFlexFactory;
		}

		/// <summary>
		/// Start all of the broker's services.
		/// </summary>
		internal void StartServices()
		{
			foreach(DictionaryEntry entry in _services)
			{
				IService service = entry.Value as IService;
				service.Start();
			}
		}
		/// <summary>
		/// Stop all of the broker's services.
		/// </summary>
		internal void StopServices()
		{
			foreach(DictionaryEntry entry in _services)
			{
				IService service = entry.Value as IService;
				service.Stop();
			}
		}
		/// <summary>
		/// Start all of the broker's endpoints.
		/// </summary>
		internal void StartEndpoints()
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				endpoint.Start();
			}
		}
		/// <summary>
		/// Stop all of the broker's endpoints.
		/// </summary>
		internal void StopEndpoints()
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				endpoint.Stop();
			}
		}
		/// <summary>
		/// Start the message broker.
		/// </summary>
		public void Start()
		{
			RegisterMessageBroker();

            //Each Application has its own Scope hierarchy and the root scope is WebScope. 
            //There's a global scope that aims to provide common resource sharing across Applications namely GlobalScope.
            //The GlobalScope is the parent of all WebScopes. 
            //Other scopes in between are all instances of Scope. Each scope takes a name. 
            //The GlobalScope is named "default".
            //The WebScope is named per Application context name.
            //The Scope is named per path name.
            _globalScope = new GlobalScope();
            _globalScope.Name = "default";
            ScopeResolver scopeResolver = new ScopeResolver(_globalScope);
            FluorineFx.Messaging.Api.IClientRegistry clientRegistry = _clientManager;
            ServiceInvoker serviceInvoker = new ServiceInvoker();
            ScopeContext context = new ScopeContext("/", clientRegistry, scopeResolver, serviceInvoker, null);
            CoreHandler handler = new CoreHandler();
            _globalScope.Context = context;
            _globalScope.Handler = handler;
            _globalScope.Register();

			StartServices();
			StartEndpoints();
		}
		/// <summary>
		/// Stop the message broker.
		/// </summary>
		public void Stop()
		{
			StopServices();
			StopEndpoints();

            if (_globalScope != null)
            {
                _globalScope.Stop();
                _globalScope.Dispose();
                _globalScope = null;
            }

			UnregisterMessageBroker();
		}

		internal IEndpoint GetEndpoint(string endpointId)
		{
            if (endpointId == null || endpointId == string.Empty)
                return null;
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				if( endpoint.Id == endpointId )
					return endpoint;
			}
			return null;
		}

		internal IEndpoint GetEndpoint(string path, string contextPath, bool secure)
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
                if (endpoint.ChannelDefinition.Bind(path, contextPath))
					return endpoint;
			}
			return null;
		}

        internal void TraceChannelSettings()
        {
            foreach (DictionaryEntry entry in _endpoints)
            {
                IEndpoint endpoint = entry.Value as IEndpoint;
                log.Debug(endpoint.ChannelDefinition.ToString());
            }
        }

		/// <summary>
		/// Call this method in order to send a message from your code into the message routing system.
		/// The message is routed to a service that is defined to handle messages of this type.
		/// Once the service is identified, the destination property of the message is used to find a destination configured for that service.
		/// The adapter defined for that destination is used to handle the message.
		/// </summary>
		/// <param name="message">The message to be routed to a service.</param>
		/// <returns>The result of the message routing.</returns>
		public IMessage RouteMessage(IMessage message)
		{
			return RouteMessage(message, null);
		}

		/// <summary>
		/// Call this method in order to send a message from your code into the message routing system.
		/// The message is routed to a service that is defined to handle messages of this type.
		/// Once the service is identified, the destination property of the message is used to find a destination configured for that service.
		/// The adapter defined for that destination is used to handle the message.
		/// </summary>
		/// <param name="message">The message to be routed to a service.</param>
		/// <param name="endpoint">This can identify the endpoint that is sending the message but it is currently not used so you may pass in null.</param>
        /// <returns>The result of the message routing.</returns>
		internal IMessage RouteMessage(IMessage message, IEndpoint endpoint)
		{
			IService service = null;
			object result = null;
			IMessage responseMessage = null;

            if( log.IsDebugEnabled )
                log.Debug(__Res.GetString(__Res.MessageBroker_RoutingMessage, message.ToString()));

			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null && (commandMessage.operation == CommandMessage.LoginOperation || commandMessage.operation == CommandMessage.LogoutOperation) )//Login, Logout
			{
				try
				{
					service = GetService(AuthenticationService.ServiceId);
					result = service.ServiceMessage(commandMessage);
					responseMessage = result as IMessage;
				}
                catch (UnauthorizedAccessException uae)
                {
                    if (log.IsDebugEnabled)
                        log.Debug(uae.Message);
                    responseMessage = ErrorMessage.GetErrorMessage(message, uae);
                }
                catch (SecurityException exception)
                {
                    if (log.IsDebugEnabled)
                        log.Debug(exception.Message);
                    responseMessage = ErrorMessage.GetErrorMessage(message, exception);
                }
				catch(Exception exception)
				{
					if(log.IsErrorEnabled)
						log.Error(__Res.GetString(__Res.MessageBroker_RoutingError), exception);
					responseMessage = ErrorMessage.GetErrorMessage(message, exception);
				}
			}
            else if (commandMessage != null && commandMessage.operation == CommandMessage.ClientPingOperation)
            {
                responseMessage = new AcknowledgeMessage();
                responseMessage.body = true;
            }
            else
            {
				//The only case when context is not set should be when one starts a new thread in the backend
                //if( FluorineContext.Current != null )
                //    FluorineContext.Current.RestorePrincipal(this.LoginCommand);
                _loginManager.RestorePrincipal();
				service = GetService(message);
                if (service != null)
                {
                    try
                    {
                        service.CheckSecurity(message);
                        result = service.ServiceMessage(message);
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(uae.Message);
                        result = ErrorMessage.GetErrorMessage(message, uae);
                    }
                    catch (SecurityException exception)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(exception.Message);
                        result = ErrorMessage.GetErrorMessage(message, exception);
                    }
                    catch (ServiceException exception)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(exception.Message);
                        result = ErrorMessage.GetErrorMessage(message, exception);
                    }
                    catch (Exception exception)
                    {
                        if (log.IsErrorEnabled)
                            log.Error(exception.Message, exception);
                        result = ErrorMessage.GetErrorMessage(message, exception);
                    }
                }
                else
                {
                    string msg = __Res.GetString(__Res.Destination_NotFound, message.destination);
                    if (log.IsErrorEnabled)
                        log.Error(msg);
                    result = ErrorMessage.GetErrorMessage(message, new FluorineException(msg));
                }
                if (!(result is IMessage))
                {
                    responseMessage = new AcknowledgeMessage();
                    responseMessage.body = result;
                }
                else
                    responseMessage = result as IMessage;
            }

			if( responseMessage is AsyncMessage )
			{
				((AsyncMessage)responseMessage).correlationId = message.messageId;
			}
			responseMessage.destination = message.destination;
			responseMessage.clientId = message.clientId;

			//Debug
			if( message.HeaderExists(AMFHeader.DebugHeader) )
			{
				log.Debug("MessageBroker processing debug header");
				ArrayList traceStack = NetDebug.GetTraceStack();
				responseMessage.SetHeader(AMFHeader.DebugHeader, traceStack.ToArray(typeof(object)) as object[]);
				NetDebug.Clear();
			}
            //The only case when we do not have context should be when the server side initiates a push
            if( FluorineContext.Current != null && FluorineContext.Current.Client != null )
                responseMessage.SetHeader(MessageBase.FlexClientIdHeader, FluorineContext.Current.Client.Id);

            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.MessageBroker_Response, responseMessage.ToString()));

			return responseMessage;
		}

		internal IService GetService(IMessage message)
		{
			IService service = GetServiceByDestinationId(message.destination);
			if( service == null )
			{
				CommandMessage commandMessage = message as CommandMessage;
				if( commandMessage != null )
				{
					/*if( commandMessage.messageRefType != null )
					{
						foreach(DictionaryEntry entry in _services)
						{
							IService serviceTmp = entry.Value as IService;
							if( serviceTmp.IsSupportedMessageType(commandMessage.messageRefType ) )
							{
								service = serviceTmp;
								break;
							}
						}
					}*/
				}
			}
			return service;
		}

        internal IService GetServiceByDestinationId(string destinationId)
		{
			if( destinationId == null )
				return null;
			string serviceId = _destinationServiceMap[destinationId] as string;
			if( serviceId != null )
				return _services[serviceId] as IService;
			return null;
		}

        internal IService GetServiceByMessageType(string messageRef)
        {
            if (messageRef == null)
                return null;
            foreach (DictionaryEntry entry in _services)
            {
                IService serviceTmp = entry.Value as IService;
                if (serviceTmp.IsSupportedMessageType(messageRef))
                {
                    return serviceTmp;
                }
            }
            return null;
        }
        /// <summary>
        /// Gets the destination Id for the specified source.
        /// </summary>
        /// <param name="source">The destination's source property.</param>
        /// <returns>The Id if the destination is found; otherwise, null.</returns>
        public string GetDestinationBySource(string source)
        {
            foreach (DictionaryEntry entry in _destinations)
            {
                Destination destination = entry.Value as Destination;
                if (destination.Source == source)
                    return destination.Id;
            }
            return null;
        }
        /// <summary>
        /// Gets the specified destination.
        /// </summary>
        /// <param name="destinationId">The Id if the destination.</param>
        /// <returns>The destination if found; otherwise, null.</returns>
        [CLSCompliant(false)]
        public Destination GetDestination(string destinationId)
        {
            foreach (DictionaryEntry entry in _destinations)
            {
                Destination destination = entry.Value as Destination;
                if (destination.Id == destinationId)
                    return destination;
            }
            return null;
        }

        /// <summary>
        /// Gets the destination Id from the specified IMessage instance.
        /// </summary>
        /// <param name="message">The message that should be handled by the destination.</param>
        /// <returns>The Id if the destination is found; otherwise, null.</returns>
        public string GetDestinationId(IMessage message)
        {
            //If destination is specified then return
            if( message.destination != null )
                return message.destination;
            if( message is RemotingMessage )
            {
                //Search for a destination with the same source
                RemotingMessage remotingMessage = message as RemotingMessage;
                string destinationId = GetDestinationBySource(remotingMessage.source);
                if (destinationId != null)
                    return destinationId;
                //Search for a RemotingService
                Destination defaultDestination = null;
                foreach (DictionaryEntry entry in _services)
                {
                    IService serviceTmp = entry.Value as IService;
                    if (serviceTmp.IsSupportedMessage(message))
                    {
                        Destination[] destinations = serviceTmp.GetDestinations();
                        foreach (Destination destination in destinations)
                        {
                            if (destination.Source == remotingMessage.source)
                                return destination.Id;
                            if (destination.Source == "*")
                                defaultDestination = destination;
                        }
                    }
                }
                if (defaultDestination != null)
                    return defaultDestination.Id;
            }
            return null;
        }

        /// <summary>
        /// Gets the destination Id for the specified source.
        /// </summary>
        /// <param name="source">The source should be handled by the destination.</param>
        /// <returns>The Id if the destination is found; otherwise, null.</returns>
        public string GetDestinationId(string source)
        {
            ValidationUtils.ArgumentNotNullOrEmpty(source, "source");
            string destinationId = GetDestinationBySource(source);
            if (destinationId != null)
                return destinationId;
            //Search for a RemotingService
            Destination defaultDestination = null;
            foreach (DictionaryEntry entry in _services)
            {
                IService serviceTmp = entry.Value as IService;
                if (serviceTmp.IsSupportedMessageType("flex.messaging.messages.RemotingMessage"))
                {
                    Destination[] destinations = serviceTmp.GetDestinations();
                    foreach (Destination destination in destinations)
                    {
                        if (destination.Source == source)
                            return destination.Id;
                        if (destination.Source == "*")
                            defaultDestination = destination;
                    }
                }
            }
            if (defaultDestination != null)
                return defaultDestination.Id;
            return null;
        }

        internal FluorineFx.Messaging.Api.IClient GetClient(string id)
        {
            return _clientManager.LookupClient(id);
        }
	}
}
