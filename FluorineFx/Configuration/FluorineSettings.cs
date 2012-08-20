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
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
#if !FXCLIENT
using FluorineFx.HttpCompress;
#endif
#if !(NET_1_1)
using System.Collections.Generic;
using FluorineFx.Collections.Generic;
#endif

namespace FluorineFx.Configuration
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !FXCLIENT
	[XmlType(TypeName="settings")]
#endif
    public sealed class FluorineSettings
	{
		private ClassMappingCollection _classMappings;
		private NullableTypeCollection _nullables;
#if !FXCLIENT
		private ServiceCollection _services;
		private ImportNamespaceCollection _importedNamespaces;
		private LoginCommandCollection _loginCommandCollection;
        private HttpCompressSettings _httpCompressSettings;
		private bool _wsdlGenerateProxyClasses;
		private string _wsdlProxyNamespace;
		private CacheCollection _cache;
		private RtmpServerSettings _rtmpServerSettings;
		private SwxSettings _swxSettings;
        private StreamableFileFactorySettings _streamableFileFactorySettings;
        private BWControlServiceSettings _bwControlServiceSettings;
        private SchedulingServiceSettings _schedulingServiceSettings;
        private PlaylistSubscriberStreamSettings _playlistSubscriberStreamSettings;
        private JSonSettings _jsonSettings;
        private SilverlightSettings _silverlightSettings;
        private RuntimeSettings _runtimeSettings;
        private Debug _debug;
#endif
        private CustomErrors _customErrors;
        private bool _acceptNullValueTypes;
		private RemotingServiceAttributeConstraint _remotingServiceAttributeConstraint;
		private TimezoneCompensation _timezoneCompensation;
		private OptimizerSettings _optimizerSettings;

        /// <summary>
        /// Initializes a new instance of the FluorineSettings class.
        /// </summary>
		public FluorineSettings()
		{
			_timezoneCompensation = TimezoneCompensation.None;
			_remotingServiceAttributeConstraint = RemotingServiceAttributeConstraint.Access;
			_acceptNullValueTypes = false;
#if !FXCLIENT
			_wsdlProxyNamespace = "FluorineFx.Proxy";
			_wsdlGenerateProxyClasses = true;
			_rtmpServerSettings = new RtmpServerSettings();
			//_optimizerSettings = new OptimizerSettings();
			_swxSettings = new SwxSettings();
            _runtimeSettings = new RuntimeSettings();
#endif
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlArray("classMappings")]
		[XmlArrayItem("classMapping",typeof(ClassMapping))]
#endif
        public ClassMappingCollection ClassMappings
		{
			get
			{
				if (_classMappings == null)
					_classMappings = new ClassMappingCollection();
				return _classMappings;
			}
			//set{ _classMappings = value; }
		}

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlArray("nullable")]
		[XmlArrayItem("type",typeof(NullableType))]
#endif
        public NullableTypeCollection Nullables
		{
			get
			{
				if (_nullables == null)
					_nullables = new NullableTypeCollection();
				return _nullables;
			}
			//set{ _nullables = value; }
        }
#if !FXCLIENT
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("services")]
		[XmlArrayItem("service",typeof(ServiceConfiguration))]
        public ServiceCollection Services
		{
			get
			{
				if (_services == null)
					_services = new ServiceCollection();
				return _services;
			}
			//set{ _services = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "httpCompress")]
		public HttpCompressSettings HttpCompressSettings
		{
			get{ return _httpCompressSettings; }
			set{ _httpCompressSettings = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "wsdlGenerateProxyClasses")]
		public bool WsdlGenerateProxyClasses
		{
			get{ return _wsdlGenerateProxyClasses; }
			set{ _wsdlGenerateProxyClasses = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "wsdlProxyNamespace")]
		public string WsdlProxyNamespace
		{
			get{ return _wsdlProxyNamespace; }
			set{ _wsdlProxyNamespace = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("security")]
		[XmlArrayItem("login-command",typeof(LoginCommandSettings))]
        public LoginCommandCollection LoginCommands
		{
			get
			{ 
				if (_loginCommandCollection == null)
					_loginCommandCollection = new LoginCommandCollection();
				return _loginCommandCollection;
			}
			//set{ _loginCommandCollection = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("cache")]
		[XmlArrayItem("cachedService",typeof(CachedService))]
        public CacheCollection Cache
		{
			get
			{
				if (_cache == null)
					_cache = new CacheCollection();
				return _cache;
			}
			//set{ _cache = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("importNamespaces")]
		[XmlArrayItem("add",typeof(ImportNamespace))]
        public ImportNamespaceCollection ImportNamespaces
		{
			get
			{
				if (_importedNamespaces == null)
					_importedNamespaces = new ImportNamespaceCollection();
				return _importedNamespaces;
			}
			//set{ _importedNamespaces = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "streamableFileFactory")]
        public StreamableFileFactorySettings StreamableFileFactory
        {
            get
            {
                if (_streamableFileFactorySettings == null)
                    _streamableFileFactorySettings = new StreamableFileFactorySettings();
                return _streamableFileFactorySettings;
            }
            set { _streamableFileFactorySettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "bwControlService")]
        public BWControlServiceSettings BWControlService
        {
            get
            {
                if (_bwControlServiceSettings == null)
                    _bwControlServiceSettings = new BWControlServiceSettings();
                return _bwControlServiceSettings;
            }
            set { _bwControlServiceSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "schedulingService")]
        public SchedulingServiceSettings SchedulingService
        {
            get
            {
                if (_schedulingServiceSettings == null)
                    _schedulingServiceSettings = new SchedulingServiceSettings();
                return _schedulingServiceSettings;
            }
            set { _schedulingServiceSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "playlistSubscriberStream")]
        public PlaylistSubscriberStreamSettings PlaylistSubscriberStream
        {
            get
            {
                if (_playlistSubscriberStreamSettings == null)
                    _playlistSubscriberStreamSettings = new PlaylistSubscriberStreamSettings();
                return _playlistSubscriberStreamSettings;
            }
            set { _playlistSubscriberStreamSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "rtmpServer")]
        public RtmpServerSettings RtmpServer
		{
			get{ return _rtmpServerSettings; }
			set{ _rtmpServerSettings = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "swx")]
        public SwxSettings SwxSettings
		{
			get{ return _swxSettings; }
			set{ _swxSettings = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "json")]
        public JSonSettings JSonSettings
        {
            get
            {
                if (_jsonSettings == null)
                    _jsonSettings = new JSonSettings();
                return _jsonSettings;
            }
            set { _jsonSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "silverlight")]
        public SilverlightSettings Silverlight
        {
            get
            {
                if (_silverlightSettings == null)
                    _silverlightSettings = new SilverlightSettings();
                return _silverlightSettings;
            }
            set { _silverlightSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "runtime")]
        public RuntimeSettings Runtime
        {
            get { return _runtimeSettings; }
            set { _runtimeSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "debug")]
        public Debug Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(ElementName = "timezoneCompensation")]
#endif
        public TimezoneCompensation TimezoneCompensation
		{
			get{ return _timezoneCompensation; }
			set{ _timezoneCompensation = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(ElementName = "acceptNullValueTypes")]
#endif
        public bool AcceptNullValueTypes
		{
			get{ return _acceptNullValueTypes; }
			set{ _acceptNullValueTypes = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(ElementName = "remotingServiceAttribute")]
#endif
        public RemotingServiceAttributeConstraint RemotingServiceAttribute
		{
			get{ return _remotingServiceAttributeConstraint; }
			set{ _remotingServiceAttributeConstraint = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(ElementName = "optimizer")]
#endif
        public OptimizerSettings Optimizer
		{
			get{ return _optimizerSettings; }
			set{ _optimizerSettings = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(ElementName = "customErrors")]
#endif
        public CustomErrors CustomErrors
        {
            get
            {
                if (_customErrors == null)
                    _customErrors = new CustomErrors();
                return _customErrors;
            }
            set { _customErrors = value; }
        }

	}

	#region LoginCommandCollection

#if !FXCLIENT

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class LoginCommandCollection : CollectionBase<LoginCommandSettings>
#else
    public sealed class LoginCommandCollection : CollectionBase
#endif
    {
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public LoginCommandCollection()
		{
		}
#if (NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( LoginCommandSettings value )  
		{
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( LoginCommandSettings value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, LoginCommandSettings value )  
		{
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( LoginCommandSettings value )  
		{
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( LoginCommandSettings value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public LoginCommandSettings this[ int index ]  
		{
			get  
			{
				return List[index] as LoginCommandSettings;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
		public string GetLoginCommand(string server)
		{
            foreach (LoginCommandSettings loginCommandSettings in this)
                if (loginCommandSettings.Server == server)
                    return loginCommandSettings.Type;
			return null;
		}
	}
#endif

	#endregion LoginCommandCollection

	#region LoginCommandSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [XmlType(TypeName = "login-command")]
    public sealed class LoginCommandSettings
	{
		private string _type;
		private string _server;
        bool _perClientAuthentication = false;

        /// <summary>
        /// Login Command implementation for ASP.NET ("server" attribute).
        /// </summary>
        public const string FluorineLoginCommand = "asp.net";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public LoginCommandSettings()
		{
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "class")]
		public string Type
		{
			get{return _type;}
			set{_type = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "server")]
		public string Server
		{
			get{return _server;}
			set{_server = value;}
		}
        /// <summary>
        /// Set to true to enable per-client authentication. The default value is false.
        /// </summary>
        [XmlElement("per-client-authentication")]
        public bool PerClientAuthentication
        {
            get { return _perClientAuthentication; }
            set { _perClientAuthentication = value; }
        }
	}
#endif
	#endregion LoginCommandSettings

	#region ClassMappingCollection

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class ClassMappingCollection : CollectionBase<ClassMapping>
#else
    public sealed class ClassMappingCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        private Dictionary<string, string> _typeToCustomClass = new Dictionary<string, string>();
        private Dictionary<string, string> _customClassToType = new Dictionary<string, string>();
#else
		private Hashtable _typeToCustomClass = new Hashtable();
        private Hashtable _customClassToType = new Hashtable();
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ClassMappingCollection()
		{
			Add(typeof(FluorineFx.AMF3.ArrayCollection).FullName, "flex.messaging.io.ArrayCollection");
			Add(typeof(FluorineFx.AMF3.ByteArray).FullName, "flex.messaging.io.ByteArray");
			Add(typeof(FluorineFx.AMF3.ObjectProxy).FullName, "flex.messaging.io.ObjectProxy");

			Add(typeof(FluorineFx.Messaging.Messages.CommandMessage).FullName, "flex.messaging.messages.CommandMessage");
			Add(typeof(FluorineFx.Messaging.Messages.RemotingMessage).FullName, "flex.messaging.messages.RemotingMessage");
			Add(typeof(FluorineFx.Messaging.Messages.AsyncMessage).FullName, "flex.messaging.messages.AsyncMessage");
			Add(typeof(FluorineFx.Messaging.Messages.AcknowledgeMessage).FullName, "flex.messaging.messages.AcknowledgeMessage");
            Add(typeof(FluorineFx.Messaging.Messages.ErrorMessage).FullName, "flex.messaging.messages.ErrorMessage");
			//Add(typeof(FluorineFx.Messaging.Messages.RPCMessage).FullName, "flex.messaging.messages.RPCMessage");			
			
#if !FXCLIENT
			Add(typeof(FluorineFx.Data.Messages.DataMessage).FullName, "flex.data.messages.DataMessage");
			Add(typeof(FluorineFx.Data.Messages.PagedMessage).FullName, "flex.data.messages.PagedMessage");
			Add(typeof(FluorineFx.Data.Messages.UpdateCollectionMessage).FullName, "flex.data.messages.UpdateCollectionMessage");
			Add(typeof(FluorineFx.Data.Messages.SequencedMessage).FullName, "flex.data.messages.SequencedMessage");
            Add(typeof(FluorineFx.Data.Messages.DataErrorMessage).FullName, "flex.data.messages.DataErrorMessage");
			Add(typeof(FluorineFx.Data.UpdateCollectionRange).FullName, "flex.data.UpdateCollectionRange");			

            Add(typeof(FluorineFx.Messaging.Services.RemotingService).FullName, "flex.messaging.services.RemotingService");
			Add(typeof(FluorineFx.Messaging.Services.MessageService).FullName, "flex.messaging.services.MessageService");
			Add(typeof(FluorineFx.Data.DataService).FullName, "flex.data.DataService");
            Add(typeof(FluorineFx.Messaging.Endpoints.RtmpEndpoint).FullName, "flex.messaging.endpoints.RTMPEndpoint");
            Add(typeof(FluorineFx.Messaging.Endpoints.AMFEndpoint).FullName, "flex.messaging.endpoints.AMFEndpoint");
            Add(typeof(FluorineFx.Messaging.Endpoints.StreamingAmfEndpoint).FullName, "flex.messaging.endpoints.StreamingAMFEndpoint");
            Add(typeof(FluorineFx.Messaging.Endpoints.SecureAmfEndpoint).FullName, "flex.messaging.endpoints.SecureAMFEndpoint");
            Add(typeof(FluorineFx.Messaging.Endpoints.SecureRtmpEndpoint).FullName, "flex.messaging.endpoints.SecureRTMPEndpoint");
			Add(typeof(FluorineFx.DotNetAdapter).FullName, "flex.messaging.services.remoting.adapters.JavaAdapter");
#endif
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="customClass"></param>
		public void Add(string type, string customClass)
		{
			ClassMapping classMapping = new ClassMapping();
			classMapping.Type = type;
			classMapping.CustomClass = customClass;
			this.Add(classMapping);
		}
#if (NET_1_1)

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( ClassMapping value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( ClassMapping value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public ClassMapping this[ int index ]  
		{
			get  
			{
				return List[index] as ClassMapping;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif

#if (NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(ClassMapping value)
        {
            _typeToCustomClass[value.Type] = value.CustomClass;
            _customClassToType[value.CustomClass] = value.Type;

            return List.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, ClassMapping value )  
		{
			_typeToCustomClass[value.Type] = value.CustomClass;
			_customClassToType[value.CustomClass] = value.Type;

			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( ClassMapping value )  
		{
			_typeToCustomClass.Remove(value.Type);
			_customClassToType.Remove(value.CustomClass);

			List.Remove(value);
		}
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(ClassMapping value)
        {
            _typeToCustomClass[value.Type] = value.CustomClass;
            _customClassToType[value.CustomClass] = value.Type;
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, ClassMapping value)
        {
            _typeToCustomClass[value.Type] = value.CustomClass;
            _customClassToType[value.CustomClass] = value.Type;
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public override bool Remove(ClassMapping value)
        {
            _typeToCustomClass.Remove(value.Type);
            _customClassToType.Remove(value.CustomClass);
            return base.Remove(value);
        }
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public string GetCustomClass(string type)
		{
			if( _typeToCustomClass.ContainsKey( type ) )
				return _typeToCustomClass[type] as string;
			else
				return type;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="customClass"></param>
        /// <returns></returns>
		public string GetType(string customClass)
		{
			if( customClass == null )
				return null;
			if( _customClassToType.ContainsKey(customClass) )
				return _customClassToType[customClass] as string;
			else
				return customClass;
		}
	}

	#endregion ClassMappingCollection

	#region ClassMapping

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !FXCLIENT
    [XmlType(TypeName = "classMapping")]
#endif
    public sealed class ClassMapping
	{
		private string _type;
		private string _customClass;
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ClassMapping()
		{
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(DataType = "string", ElementName = "type")]
#endif
		public string Type
		{
			get{return _type;}
			set{_type = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlElement(DataType = "string", ElementName = "customClass")]
#endif
		public string CustomClass
		{
			get{return _customClass;}
			set{_customClass = value;}
		}
	}

	#endregion ClassMapping

	#region ServiceCollection
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class ServiceCollection : CollectionBase<ServiceConfiguration>
#else
    public sealed class ServiceCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        private Dictionary<string, ServiceConfiguration> _serviceNames = new Dictionary<string, ServiceConfiguration>(5);
        private Dictionary<string, ServiceConfiguration> _serviceLocations = new Dictionary<string, ServiceConfiguration>(5);
#else
        private Hashtable _serviceNames = new Hashtable(5);
        private Hashtable _serviceLocations = new Hashtable(5);
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ServiceCollection()
		{
		}
#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(ServiceConfiguration value)
        {
            _serviceNames[value.Name] = value;
            _serviceLocations[value.ServiceLocation] = value;
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, ServiceConfiguration value)
        {
            _serviceNames[value.Name] = value;
            _serviceLocations[value.ServiceLocation] = value;
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public override bool Remove(ServiceConfiguration value)
        {
            _serviceNames.Remove(value.Name);
            _serviceLocations.Remove(value.ServiceLocation);
            return base.Remove(value);
        }
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( ServiceConfiguration value )  
		{
			_serviceNames[value.Name] = value;
			_serviceLocations[value.ServiceLocation] = value;
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, ServiceConfiguration value )  
		{
			_serviceNames[value.Name] = value;
			_serviceLocations[value.ServiceLocation] = value;
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( ServiceConfiguration value )
		{
            _serviceNames.Remove(value.Name);
            _serviceLocations.Remove(value.ServiceLocation);
			List.Remove(value);
		}
#endif

#if (NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( ServiceConfiguration value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( ServiceConfiguration value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public ServiceConfiguration this[ int index ]  
		{
			get  
			{
				return List[index] as ServiceConfiguration;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
		public bool Contains(string serviceName)
		{
			return _serviceNames.ContainsKey(serviceName);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
		public string GetServiceLocation(string serviceName)
		{
			if( _serviceNames.ContainsKey(serviceName) )
				return (_serviceNames[serviceName] as ServiceConfiguration).ServiceLocation;
			else
				return serviceName;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="serviceLocation"></param>
        /// <returns></returns>
		public string GetServiceName(string serviceLocation)
		{
			if( _serviceLocations.ContainsKey(serviceLocation) )
				return (_serviceLocations[serviceLocation] as ServiceConfiguration).Name;
			else
				return serviceLocation;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
		public string GetMethod(string serviceName, string name)
		{
            ServiceConfiguration serviceConfiguration = null;
            if( _serviceNames.ContainsKey(serviceName) )
                serviceConfiguration = _serviceNames[serviceName] as ServiceConfiguration;
			if( serviceConfiguration != null )
				return serviceConfiguration.Methods.GetMethod(name);
			return name;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="serviceLocation"></param>
        /// <param name="method"></param>
        /// <returns></returns>
		public string GetMethodName(string serviceLocation, string method)
		{
            ServiceConfiguration serviceConfiguration = null;
            if( _serviceLocations.ContainsKey(serviceLocation) )
                serviceConfiguration = _serviceLocations[serviceLocation] as ServiceConfiguration;
			if( serviceConfiguration != null )
				return serviceConfiguration.Methods.GetMethodName(method);
			return method;
		}
	}
#endif
	#endregion ServiceCollection

	#region ServiceConfiguration
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [XmlType(TypeName = "service")]
    public sealed class ServiceConfiguration
	{
		private string _name;
		private string _serviceLocation;
		private RemoteMethodCollection _remoteMethodCollection;
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ServiceConfiguration()
		{
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(DataType = "string", ElementName = "name")]
        public string Name
		{
			get{return _name;}
			set{_name = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(DataType = "string", ElementName = "service-location")]
        public string ServiceLocation
		{
			get{return _serviceLocation;}
			set{_serviceLocation = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("methods")]
		[XmlArrayItem("remote-method",typeof(RemoteMethod))]
		public RemoteMethodCollection Methods
		{
			get
			{
				if (_remoteMethodCollection == null)
					_remoteMethodCollection = new RemoteMethodCollection();
				return _remoteMethodCollection;
			}
			//set{ _remoteMethodCollection = value; }
		}
	}
#endif	
	#endregion ServiceConfiguration

	#region RemoteMethodCollection
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class RemoteMethodCollection : CollectionBase<RemoteMethod>
#else
    public sealed class RemoteMethodCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        Dictionary<string, string> _methods = new Dictionary<string, string>(3);
        Dictionary<string, string> _methodsNames = new Dictionary<string, string>(3);
#else
        Hashtable _methods = new Hashtable(3);
        Hashtable _methodsNames = new Hashtable(3);
#endif

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public RemoteMethodCollection()
		{
		}

#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(RemoteMethod value)
        {
            _methods[value.Name] = value.Method;
            _methodsNames[value.Method] = value.Name;
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, RemoteMethod value)
        {
            _methods[value.Name] = value.Method;
            _methodsNames[value.Method] = value.Name;
            base.Insert(index, value);
        }
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( RemoteMethod value )  
		{
			_methods[value.Name] = value.Method;
			_methodsNames[value.Method] = value.Name;
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( RemoteMethod value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, RemoteMethod value )  
		{
			_methods[value.Name] = value.Method;
			_methodsNames[value.Method] = value.Name;

			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( RemoteMethod value )  
		{
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( RemoteMethod value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public RemoteMethod this[ int index ]  
		{
			get  
			{
				return List[index] as RemoteMethod;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public string GetMethod(string name)
		{
			if( _methods.ContainsKey(name) )
				return _methods[name] as string;
			return name;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
		public string GetMethodName(string method)
		{
			if( _methodsNames.ContainsKey(method) )
				return _methodsNames[method] as string;
			return method;
		}
	}
#endif
	#endregion RemoteMethodCollection

	#region RemoteMethod
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RemoteMethod
	{
		private string _name;
		private string _method;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(DataType = "string", ElementName = "name")]
        public string Name
		{
			get{return _name;}
			set{_name = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(DataType = "string", ElementName = "method")]
        public string Method
		{
			get{return _method;}
			set{_method = value;}
		}
	}
#endif
	#endregion RemoteMethod

	#region NullableTypeCollection

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class NullableTypeCollection : CollectionBase<NullableType>
#else
    public sealed class NullableTypeCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        Dictionary<string, NullableType> _nullableDictionary = new Dictionary<string, NullableType>(5);
#else
        Hashtable _nullableDictionary = new Hashtable(5);
#endif

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public NullableTypeCollection()
		{
		}
#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(NullableType value)
        {
            _nullableDictionary[value.TypeName] = value;
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, NullableType value)
        {
            _nullableDictionary[value.TypeName] = value;
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public override bool Remove(NullableType value)
        {
            _nullableDictionary.Remove(value.TypeName);
            return base.Remove(value);
        }
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( NullableType value )  
		{
			_nullableDictionary[value.TypeName] = value;
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( NullableType value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, NullableType value )  
		{
			_nullableDictionary[value.TypeName] = value;
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( NullableType value )  
		{
			_nullableDictionary.Remove(value.TypeName);
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( NullableType value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public NullableType this[ int index ]  
		{
			get  
			{
				return List[index] as NullableType;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public bool ContainsKey( Type type )  
		{
			return _nullableDictionary.ContainsKey(type.FullName);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public object this[ Type type ]  
		{
			get  
			{
				if( _nullableDictionary.ContainsKey(type.FullName))
					return (_nullableDictionary[type.FullName] as NullableType).NullValue;
				return null;
			}
		}
	}

	#endregion NullableTypeCollection

	#region NullableType

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !FXCLIENT
    [XmlType(TypeName = "type")]
#endif
    public sealed class NullableType
	{
		private string _typeName;
		private string _value;
		private object _nullValue;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "name")]
#endif
        public string TypeName
		{
			get{return _typeName;}
			set
			{
				_typeName = value;
				Init();
			}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "value")]
#endif
        public string Value
		{
			get{return _value;}
			set
			{
				_value = value;
				Init();
			}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlIgnore]
#endif
        public object NullValue
		{
			get{return _nullValue;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		private void Init()
		{
			if( _typeName == null || _value == null )
				return;

			Type type = Type.GetType(_typeName);
			// Check if this is a static field of the type
			FieldInfo fi = type.GetField(_value, BindingFlags.Public | BindingFlags.Static);
			if( fi != null )
				_nullValue = fi.GetValue(null);
			else
#if (NET_1_1)
				_nullValue = System.Convert.ChangeType(_value, type);
#else
                _nullValue = System.Convert.ChangeType(_value, type, null);
#endif
		}
	}

	#endregion NullableType

	#region CacheCollection
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class CacheCollection : CollectionBase<CachedService>
#else
    public sealed class CacheCollection : CollectionBase
#endif
    {
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public CacheCollection()
		{
		}
#if (NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( CachedService value )  
		{
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( CachedService value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, CachedService value )  
		{
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( CachedService value )  
		{
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( CachedService value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public CachedService this[ int index ]  
		{
			get  
			{
				return List[index] as CachedService;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
	}
#endif
	#endregion CacheCollection

	#region CachedService
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [XmlType(TypeName = "cachedService")]
	public sealed class CachedService
	{
		private int _timeout;
		private bool _slidingExpiration;
		private string _type;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public CachedService()
		{
			_timeout = 30;
			_slidingExpiration = false;
		}

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "timeout")]
		public int Timeout
		{
			get{return _timeout;}
			set{_timeout = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "boolean", AttributeName = "slidingExpiration")]
		public bool SlidingExpiration
		{
			get{return _slidingExpiration;}
			set{_slidingExpiration = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "type")]
		public string Type
		{
			get{return _type;}
			set{_type = value;}
		}
	}
#endif
	#endregion CachedService

	#region ImportNamespaceCollection
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class ImportNamespaceCollection : CollectionBase<ImportNamespace>
#else
    public sealed class ImportNamespaceCollection : CollectionBase
#endif
    {
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ImportNamespaceCollection()
		{
		}
#if (NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( ImportNamespace value )  
		{
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( ImportNamespace value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, ImportNamespace value )  
		{
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( ImportNamespace value )  
		{
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( ImportNamespace value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public ImportNamespace this[ int index ]  
		{
			get  
			{
				return List[index] as ImportNamespace;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
	}
#endif
	#endregion ImportNamespaceCollection

	#region ImportNamespace
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class ImportNamespace
	{
		private string _namespace;
		private string _assembly;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public ImportNamespace()
		{
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "namespace")]
		public string Namespace
		{
			get{return _namespace;}
			set{_namespace = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "assembly")]
		public string Assembly
		{
			get{return _assembly;}
			set{_assembly = value;}
		}
	}
#endif
	#endregion ImportNamespace

    #region StreamableFileFactorySettings
#if !FXCLIENT
    /// <summary>
    /// StreamableFileFactory settings.
    /// </summary>
    public class StreamableFileFactorySettings
    {
        private string _type;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public StreamableFileFactorySettings()
        {
            _type = typeof(FluorineFx.Messaging.Rtmp.IO.StreamableFileFactory).FullName;
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
#endif
    #endregion StreamableFileFactorySettings

    #region BWControlServiceSettings
#if !FXCLIENT

    /// <summary>
    /// BWControlServiceSettings settings.
    /// </summary>
    public class BWControlServiceSettings
    {
        private string _type;
        private int _interval;
        private int _defaultCapacity;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public BWControlServiceSettings()
        {
            _type = typeof(FluorineFx.Messaging.Rtmp.Stream.DummyBWControlService).FullName;
            _interval = 100;
            _defaultCapacity = 104857600;
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "interval")]
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "defaultCapacity")]
        public int DefaultCapacity
        {
            get { return _defaultCapacity; }
            set { _defaultCapacity = value; }
        }
    }
#endif
    #endregion BWControlServiceSettings

    #region SchedulingServiceSettings
#if !FXCLIENT
    /// <summary>
    /// SchedulingServiceSettings settings.
    /// </summary>
    public class SchedulingServiceSettings
    {
        private string _type;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public SchedulingServiceSettings()
        {
            _type = typeof(FluorineFx.Scheduling.SchedulingService).FullName;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
#endif
    #endregion SchedulingServiceSettings

    #region PlaylistSubscriberStreamSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class PlaylistSubscriberStreamSettings
    {
        private int _underrunTrigger;
        private int _bufferCheckInterval;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public PlaylistSubscriberStreamSettings()
        {
            _underrunTrigger = 5000;
            _bufferCheckInterval = 60000;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "underrunTrigger")]
        public int UnderrunTrigger
        {
            get { return _underrunTrigger; }
            set { _underrunTrigger = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "bufferCheckInterval")]
        public int BufferCheckInterval
        {
            get { return _bufferCheckInterval; }
            set { _bufferCheckInterval = value; }
        }
    }
#endif
    #endregion PlaylistSubscriberStreamSettings

    #region HttpCompressSettings

#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class HttpCompressSettings
	{
		private HandleRequest _handleRequest;
		private Algorithms _preferredAlgorithm;
		private CompressionLevels _compressionLevel;
		private MimeTypeEntryCollection _excludedTypes;
		private PathEntryCollection _excludedPaths;
		int _threshold;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public HttpCompressSettings()
		{
			_handleRequest = HandleRequest.None;
			_preferredAlgorithm = Algorithms.Default;
			_compressionLevel = CompressionLevels.Default;
			_excludedTypes = new MimeTypeEntryCollection();
			_excludedPaths = new PathEntryCollection();
			_threshold = 20480;
		}

		/// <summary>
		/// The default settings.  Deflate + normal.
		/// </summary>
		public static HttpCompressSettings Default 
		{
			get { return new HttpCompressSettings(); }
		}

		/// <summary>
		/// Request type to handle.
		/// </summary>
		[XmlAttribute(AttributeName="handleRequest")]
		public HandleRequest HandleRequest
		{
			get { return _handleRequest; }
			set { _handleRequest = value; }
		}

		/// <summary>
		/// The preferred algorithm to use for compression.
		/// </summary>
		[XmlAttribute(AttributeName="preferredAlgorithm")]
		public Algorithms PreferredAlgorithm
		{
			get { return _preferredAlgorithm; }
			set { _preferredAlgorithm = value; }
		}

		/// <summary>
		/// Compress responses larger then threshold bytes.
		/// </summary>
		[XmlElement(ElementName="threshold")]
		public int Threshold
		{
			get{ return _threshold; }
			set{ _threshold = value; }
		}

		/// <summary>
		/// The preferred compression level.
		/// </summary>
		[XmlAttribute(AttributeName="compressionLevel")]
		public CompressionLevels CompressionLevel 
		{
			get { return _compressionLevel; }
			set { _compressionLevel = value; }
		}
        /// <summary>
        /// Mime types to exclude from compression.
        /// Mime types can include wildcards like image/* or */xml.
        /// </summary>
		[XmlArray("excludedMimeTypes")]
		[XmlArrayItem("add",typeof(MimeTypeEntry))]
		public MimeTypeEntryCollection ExcludedMimeTypes 
		{
			get 
			{ 
				if( _excludedTypes == null )
					_excludedTypes = new MimeTypeEntryCollection();
				return _excludedTypes; 
			}
			//set { _excludedTypes = value; }
		}
        /// <summary>
        /// Paths to exclude from compression.
        /// </summary>
		[XmlArray("excludedPaths")]
		[XmlArrayItem("add",typeof(PathEntry))]
		public PathEntryCollection ExcludedPaths 
		{
			get 
			{ 
				if( _excludedPaths == null )
					_excludedPaths = new PathEntryCollection();
				return _excludedPaths; 
			}
			//set { _excludedPaths = value; }
		}

		/// <summary>
		/// Checks a given mime type to determine if it has been excluded from compression
		/// </summary>
		/// <param name="mimetype">The MimeType to check.  Can include wildcards like image/* or */xml.</param>
		/// <returns>true if the mime type passed in is excluded from compression, false otherwise</returns>
		public bool IsExcludedMimeType(string mimetype) 
		{
			return _excludedTypes.Contains(mimetype.ToLower());
		}

		/// <summary>
		/// Looks for a given path in the list of paths excluded from compression.
		/// </summary>
		/// <param name="relUrl">the relative url to check</param>
		/// <returns>true if excluded, false if not</returns>
		public bool IsExcludedPath(string relUrl) 
		{
			return _excludedPaths.Contains(relUrl.ToLower());
		}
	}
#endif
	#endregion HttpCompressSettings

	#region MimeTypeEntryCollection
#if !FXCLIENT

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class MimeTypeEntryCollection : CollectionBase<MimeTypeEntry>
#else
    public sealed class MimeTypeEntryCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        Dictionary<string, MimeTypeEntry> _excludedTypes = new Dictionary<string, MimeTypeEntry>(StringComparer.OrdinalIgnoreCase);
#else
		Hashtable _excludedTypes = new Hashtable(null, CaseInsensitiveComparer.Default);
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public MimeTypeEntryCollection()
		{
		}

#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(MimeTypeEntry value)
        {
            _excludedTypes.Add(value.Type, value);
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, MimeTypeEntry value)
        {
            _excludedTypes.Add(value.Type, value);
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public override bool Remove( MimeTypeEntry value )  
		{
			_excludedTypes.Remove(value.Type);
			return base.Remove(value);
		}
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( MimeTypeEntry value )  
		{
			_excludedTypes.Add(value.Type, value);
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( MimeTypeEntry value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, MimeTypeEntry value )  
		{
			_excludedTypes.Add(value.Type, value);
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( MimeTypeEntry value )  
		{
			_excludedTypes.Remove(value.Type);
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( MimeTypeEntry value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public MimeTypeEntry this[ int index ]  
		{
			get  
			{
				return List[index] as MimeTypeEntry;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public bool Contains( string type )  
		{
			return _excludedTypes.ContainsKey(type);
		}
	}
#endif
	#endregion MimeTypeEntryCollection

	#region PathEntryCollection
#if !FXCLIENT

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class PathEntryCollection : CollectionBase<PathEntry>
#else
    public sealed class PathEntryCollection : CollectionBase
#endif
    {
#if !(NET_1_1)
        Dictionary<string, PathEntry> _excludedPaths = new Dictionary<string, PathEntry>(StringComparer.OrdinalIgnoreCase);
#else
        Hashtable _excludedPaths = new Hashtable(null, CaseInsensitiveComparer.Default);
#endif

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public PathEntryCollection()
		{
		}

#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(PathEntry value)
        {
            _excludedPaths.Add(value.Path, value);
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, PathEntry value)
        {
            _excludedPaths.Add(value.Path, value);
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public override bool Remove(PathEntry value)
        {
            _excludedPaths.Remove(value.Path);
            return base.Remove(value);
        }
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int Add( PathEntry value )  
		{
			_excludedPaths.Add(value.Path, value);
			return List.Add(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public int IndexOf( PathEntry value )  
		{
			return List.IndexOf(value) ;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
		public void Insert( int index, PathEntry value )  
		{
			_excludedPaths.Add(value.Path, value);
			List.Insert(index, value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
		public void Remove( PathEntry value )  
		{
			_excludedPaths.Remove(value.Path);
			List.Remove(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains( PathEntry value )  
		{
			return List.Contains(value);
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public PathEntry this[ int index ]  
		{
			get  
			{
				return List[index] as PathEntry;
			}
			set  
			{
				List[index] = value;
			}
		}
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
		public bool Contains( string path )  
		{
			return _excludedPaths.ContainsKey(path);
		}
	}
#endif
	#endregion PathEntryCollection

	#region MimeTypeEntry
#if !FXCLIENT

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class MimeTypeEntry
	{
		private string _type;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}
	}
#endif
	#endregion MimeTypeEntry

	#region PathEntry
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class PathEntry
	{
		private string _path;

        /// <summary>
        /// Gets or sets the to exclude from compression.
        /// The path is a relative url.
        /// </summary>
        [XmlAttribute(AttributeName = "path")]
		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}
	}
#endif
	#endregion PathEntry

	#region RtmpServerSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RtmpServerSettings
	{
        private RtmpConnectionSettings _rtmpConnectionSettings;
        private RtmptConnectionSettings _rtmptConnectionSettings;
        private RtmpTransportSettings _rtmpTransportSettings;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public RtmpServerSettings()
		{
            _rtmpConnectionSettings = new RtmpConnectionSettings();
            _rtmptConnectionSettings = new RtmptConnectionSettings();
            _rtmpTransportSettings = new RtmpTransportSettings();
		}

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "rtmpConnection")]
        public RtmpConnectionSettings RtmpConnectionSettings
        {
            get { return _rtmpConnectionSettings; }
            set { _rtmpConnectionSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "rtmptConnection")]
        public RtmptConnectionSettings RtmptConnectionSettings
        {
            get { return _rtmptConnectionSettings; }
            set { _rtmptConnectionSettings = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "rtmpTransport")]
        public RtmpTransportSettings RtmpTransportSettings
        {
            get { return _rtmpTransportSettings; }
            set { _rtmpTransportSettings = value; }
        }
	}
#endif
	#endregion RtmpServerSettings


    #region RtmpConnectionSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RtmpConnectionSettings
    {
        private int _pingInterval;
        private int _maxInactivity;
        private int _maxHandshakeTimeout;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public RtmpConnectionSettings()
        {
            _pingInterval = 5000;
            _maxInactivity = 60000;
            _maxHandshakeTimeout = 5000;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "pingInterval")]
        public int PingInterval
        {
            get { return _pingInterval; }
            set { _pingInterval = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "maxInactivity")]
        public int MaxInactivity
        {
            get { return _maxInactivity; }
            set { _maxInactivity = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "maxHandshakeTimeout")]
        public int MaxHandshakeTimeout
        {
            get { return _maxHandshakeTimeout; }
            set { _maxHandshakeTimeout = value; }
        }
    }
#endif
    #endregion RtmpConnectionSettings

    #region RtmptConnectionSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RtmptConnectionSettings
    {
        private int _pingInterval;
        private int _maxInactivity;
        private int _maxHandshakeTimeout;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public RtmptConnectionSettings()
        {
            _pingInterval = 5000;
            _maxInactivity = 60000;
            _maxHandshakeTimeout = 5000;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "pingInterval")]
        public int PingInterval
        {
            get { return _pingInterval; }
            set { _pingInterval = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "maxInactivity")]
        public int MaxInactivity
        {
            get { return _maxInactivity; }
            set { _maxInactivity = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "maxHandshakeTimeout")]
        public int MaxHandshakeTimeout
        {
            get { return _maxHandshakeTimeout; }
            set { _maxHandshakeTimeout = value; }
        }
    }
#endif
    #endregion RtmptConnectionSettings

    #region RtmpTransportSettings
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RtmpTransportSettings
    {
        private int _receiveBufferSize;
        private int _sendBufferSize;
        private bool _tcpNoDelay;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public RtmpTransportSettings()
        {
            _receiveBufferSize = 4096;
            _sendBufferSize = 4096;
            _tcpNoDelay = true;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "int", AttributeName = "receiveBufferSize")]
#endif
        public int ReceiveBufferSize
        {
            get { return _receiveBufferSize; }
            set { _receiveBufferSize = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "int", AttributeName = "sendBufferSize")]
#endif
        public int SendBufferSize
        {
            get { return _sendBufferSize; }
            set { _sendBufferSize = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "boolean", AttributeName = "tcpNoDelay")]
#endif
        public bool TcpNoDelay
        {
            get { return _tcpNoDelay; }
            set { _tcpNoDelay = value; }
        }
    }

    #endregion RtmpTransportSettings

    #region OptimizerSettings

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class OptimizerSettings
	{
		private string _provider;
		private bool _debug;
        private bool _typeCheck;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public OptimizerSettings()
		{
            _provider = "codedom";
			_debug = true;
            //Generate type checking by default
            _typeCheck = true;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "provider")]
#endif
        public string Provider
		{
			get{return _provider;}
			set{_provider = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "boolean", AttributeName = "debug")]
#endif
        public bool Debug
		{
			get{return _debug;}
			set{_debug = value;}
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "boolean", AttributeName = "typeCheck")]
#endif
        public bool TypeCheck
        {
            get { return _typeCheck; }
            set { _typeCheck = value; }
        }
	}

	#endregion OptimizerSettings

    #region CustomErrors

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class CustomErrors
    {
        private string _mode;
        private bool _stackTrace;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public CustomErrors()
        {
            _mode = "On";
            _stackTrace = false;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "mode")]
#endif
        public string Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "boolean", AttributeName = "stackTrace")]
#endif
        public bool StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }
    }

    #endregion CustomErrors

    #region SwxSettings
#if !FXCLIENT
    /// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class SwxSettings
	{
		private bool _allowDomain;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public SwxSettings()
		{
			_allowDomain = true;
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "boolean", AttributeName = "allowDomain")]
		public bool AllowDomain
		{
			get{return _allowDomain;}
			set{_allowDomain = value;}
		}
	}
#endif
	#endregion SwxSettings

	#region JSonSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class JSonSettings
	{
        private JsonRpcGeneratorCollection _jsonRpcGeneratorCollection;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public JSonSettings()
        {
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlArray("jsonRpcGenerators")]
        [XmlArrayItem("add", typeof(JsonRpcGenerator))]
        public JsonRpcGeneratorCollection JsonRpcGenerators
        {
            get 
            {
                if (_jsonRpcGeneratorCollection == null)
                    _jsonRpcGeneratorCollection = new JsonRpcGeneratorCollection();
                return _jsonRpcGeneratorCollection;
            }
            set { _jsonRpcGeneratorCollection = value; }
        }
    }
#endif
    #endregion JSonSettings

    #region JsonRpcGeneratorCollection
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
#if !(NET_1_1)
    public sealed class JsonRpcGeneratorCollection : CollectionBase<JsonRpcGenerator>
#else
    public sealed class JsonRpcGeneratorCollection : CollectionBase
#endif
    {

#if !(NET_1_1)
        Dictionary<string, JsonRpcGenerator> _generators = new Dictionary<string, JsonRpcGenerator>();
#else
        Hashtable _generators = new Hashtable();
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public JsonRpcGeneratorCollection()
        {
        }

#if !(NET_1_1)
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Add(JsonRpcGenerator value)
        {
            _generators.Add(value.Name, value);
            base.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public override void Insert(int index, JsonRpcGenerator value)
        {
            _generators.Add(value.Name, value);
            base.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public override bool Remove(JsonRpcGenerator value)
        {
            _generators.Remove(value.Name);
            return base.Remove(value);
        }
#else
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(JsonRpcGenerator value)
        {
            _generators.Add(value.Name, value);
            return List.Add(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(JsonRpcGenerator value)
        {
            return List.IndexOf(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, JsonRpcGenerator value)
        {
            _generators.Add(value.Name, value);
            List.Insert(index, value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        public void Remove(JsonRpcGenerator value)
        {
            _generators.Remove(value.Name);
            List.Remove(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(JsonRpcGenerator value)
        {
            return List.Contains(value);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JsonRpcGenerator this[int index]
        {
            get
            {
                return List[index] as JsonRpcGenerator;
            }
        }
#endif
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return _generators.ContainsKey(name);
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonRpcGenerator this[string name]
        {
            get
            {
                if( _generators.ContainsKey(name) )
                    return _generators[name] as JsonRpcGenerator;
                return null;
            }
        }
    }
#endif
    #endregion JsonRpcGeneratorCollection

    #region JsonRpcGenerator
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class JsonRpcGenerator
    {
        private string _name;
        private string _type;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public JsonRpcGenerator()
        {
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
#endif
    #endregion JsonRpcGenerator

    #region RuntimeSettings

#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class RuntimeSettings
    {
        private bool _asyncHandler;
        private int _minWorkerThreads;
        private int _maxWorkerThreads;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public RuntimeSettings()
        {
            _asyncHandler = false;
            _minWorkerThreads = FluorineFx.Threading.ThreadPoolEx.DefaultMinWorkerThreads;
            _maxWorkerThreads = FluorineFx.Threading.ThreadPoolEx.DefaultMaxWorkerThreads;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "minWorkerThreads")]
        public int MinWorkerThreads
        {
            get { return _minWorkerThreads; }
            set { _minWorkerThreads = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "int", AttributeName = "maxWorkerThreads")]
        public int MaxWorkerThreads
        {
            get { return _maxWorkerThreads; }
            set { _maxWorkerThreads = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "boolean", AttributeName = "asyncHandler")]
        public bool AsyncHandler
        {
            get { return _asyncHandler; }
            set { _asyncHandler = value; }
        }
    }
#endif

    #endregion RuntimeSettings

    #region SilverlightSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class SilverlightSettings
    {
        private PolicyServerSettings _policyServerSettings;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public SilverlightSettings()
        {
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlElement(ElementName = "policyServer")]
        public PolicyServerSettings PolicyServerSettings
        {
            get 
            {
                if (_policyServerSettings == null)
                    _policyServerSettings = new PolicyServerSettings();
                return _policyServerSettings; 
            }
            set { _policyServerSettings = value; }
        }
    }
#endif
    #endregion SilverlightSettings

    #region PolicyServerSettings
#if !FXCLIENT
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class PolicyServerSettings
    {
        private bool _enable;
        private string _policyFile;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public PolicyServerSettings()
        {
            _enable = false;
            _policyFile = "clientaccesspolicy.xml";
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "boolean", AttributeName = "enable")]
        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; }
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [XmlAttribute(DataType = "string", AttributeName = "policyFile")]
        public string PolicyFile
        {
            get { return _policyFile; }
            set { _policyFile = value; }
        }
    }
#endif
    #endregion PolicyServerSettings

    #region Debug

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class Debug
    {
        /// <summary>
        /// Debug mode off.
        /// </summary>
        public const string Off = "Off";

        private string _mode;
        private string _dumpPath;

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public Debug()
        {
            _mode = Debug.Off;
        }
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "mode")]
#endif
        public string Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
#if !FXCLIENT
        [XmlAttribute(DataType = "string", AttributeName = "dumpPath")]
#endif
        public string DumpPath
        {
            get { return _dumpPath; }
            set { _dumpPath = value; }
        }
    }

    #endregion Debug
}
