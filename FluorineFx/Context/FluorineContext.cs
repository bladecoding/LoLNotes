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
#if !FXCLIENT
using System.Web;
#endif
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.Collections.Specialized;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Security;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging;
using FluorineFx.Configuration;

namespace FluorineFx.Context
{
    /// <summary>
    /// Similary to the ASP.NET HttpContext class you can access the Fluorine context for the current request from any code inside the same application domain.
    /// The context information is accessed through the static property Current on the FluorineContext class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For an AMF channel (Http request) the Fluorine context wrapps the underlying HttpContext.
    /// </para>
    /// <para>
    /// The Fluorine context is available only when client requests are handled (both HTTP and RTMP) and is not avaliable in a newly started thread.
    /// </para>
    /// <para>
    /// It is recommended to use FluorineContext instead of HttpContext if you do not want to tie your application to ASP.NET that would otherwise work without change with a RTMP channel (both APS.NET hosted or FluorineFx Windows Service hosted).
    /// If you are using both AMF and RTMP channels from the same Flex application do not expect that the Session will always access the underlying HttpSession object. For RTMP calls the ASP.NET HttpSession object is not accessible and the Session in this case references the RTMP connection's attribute store.
    /// In this scenario the Client object can be used for identification and common storage (Flex only).
    /// </para>
    /// </remarks>
    /// <example>
    /// 	<code lang="CS">
    ///     string clientId = FluorineContext.Current.ClientId;
    /// </code>
    /// </example>
    [CLSCompliant(false)]
	public abstract class FluorineContext
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(FluorineContext));

        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineContextKey = "__@fluorinecontext";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineTicket = "fluorineauthticket";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorinePrincipalAttribute = "__@fluorineprincipal";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineConnectionKey = "__@fluorineconnection";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineClientKey = "__@fluorineclient";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineStreamIdKey = "__@fluorinestreamid";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineDataServiceTransaction = "__@fluorinedataservicetransaction";

        static ApplicationState AppState = new ApplicationState();

        /// <summary>
        /// The current session object.
        /// </summary>
        protected ISession _session;
        /// <summary>
        /// The current client object.
        /// </summary>
        protected IClient _client;
        /// <summary>
        /// The current connection object.
        /// </summary>
        protected IConnection _connection;

		internal FluorineContext()
		{
		}

        /// <summary>
        /// Gets the Session instance for the current request.
        /// </summary>
        public ISession Session { get { return _session; } }
        internal virtual void SetSession(ISession session)
        {
            _session = session;
        }
        /// <summary>
        /// Gets the current Connection object.
        /// </summary>
        public IConnection Connection{ get { return _connection; } }
        internal virtual void SetConnection(IConnection connection)
        {
            _connection = connection;
        }
        /// <summary>
        /// Gets the current Client object.
        /// </summary>
        public IClient Client { get { return _client; } }
        /// <summary>
        /// Gets the current Client identity.
        /// </summary>
        public string ClientId
        {
            get
            {
                if (this.Client != null)
                    return this.Client.Id;
                return null;
            }
        }
        internal virtual void SetClient(IClient client)
        {
            _client = client;
        }

        /// <summary>Gets the FluorineContext object for the current HTTP/RTMP request.</summary>
		static public FluorineContext Current
		{
			get
			{
                return FluorineWebSafeCallContext.GetData(FluorineContext.FluorineContextKey) as FluorineContext;
			}
		}

		/// <summary>
		/// Enables sharing of global information across multiple sessions and requests within an application.
		/// </summary>
		public IApplicationState ApplicationState 
        {
            get
            {
                return AppState;
            }
        }
        
        /// <summary>
        /// Gets security information for the current request.
        /// </summary>
		public IPrincipal User 
        {            
            get 
            { 
                //return _principal; 
                MessageBroker messageBroker = MessageBroker.GetMessageBroker(MessageBroker.DefaultMessageBrokerId);
                return messageBroker.LoginManager.Principal;
            }
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns><c>true</c> if logging out succeeded; otherwise, <c>false</c>.</returns>
        public bool Logout()
        {
            MessageBroker messageBroker = MessageBroker.GetMessageBroker(MessageBroker.DefaultMessageBrokerId);
            return messageBroker.LoginManager.Logout();
        }

		/// <summary>
		/// Gets the base directory for this <see cref="AppDomain"/>
		/// </summary>
		public virtual string ApplicationBaseDirectory
		{
			get{ return AppDomain.CurrentDomain.BaseDirectory; }
		}

		/// <summary>
		/// Converts a path into a fully qualified local file path.
		/// If the path is relative it is taken as relative from the application base directory.
		/// </summary>
		/// <param name="path">The path to convert.</param>
		/// <returns>The fully qualified path.</returns>
		public virtual string GetFullPath(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			string baseDirectory = "";
			try
			{
				string applicationBaseDirectory = this.ApplicationBaseDirectory;
				if (applicationBaseDirectory != null)
				{
					//applicationBaseDirectory may be a URI not a local file path
					Uri applicationBaseDirectoryUri = new Uri(applicationBaseDirectory);
					if (applicationBaseDirectoryUri.IsFile)
					{
						baseDirectory = applicationBaseDirectoryUri.LocalPath;
					}
				}
			}
			catch
			{
				// Ignore URI exceptions & SecurityExceptions
			}

			if (baseDirectory != null && baseDirectory.Length > 0)
			{
				// Note that Path.Combine will return the second path if it is rooted
				return Path.GetFullPath(Path.Combine(baseDirectory, path));
			}
			return Path.GetFullPath(path);
		}

		/// <summary>
		/// Gets the physical drive path of the application directory for the application hosted in the current application domain.
		/// </summary>
		public abstract string RootPath { get; }
		/// <summary>
		/// Gets the virtual path of the current request.
		/// </summary>
		public abstract string RequestPath { get; }
		/// <summary>
		/// Gets the ASP.NET application's virtual application root path on the server.
		/// </summary>
		public abstract string RequestApplicationPath { get; }
        /// <summary>
        /// Gets the physical file system path of the currently executing server application's root directory.
        /// </summary>
		public abstract string PhysicalApplicationPath { get; }
        /// <summary>
        /// Gets the ASP.NET application's application root path on the server.
        /// </summary>
        public abstract string ApplicationPath { get; }

		/// <summary>
		/// Gets the absolute URI from the URL of the current request.
		/// </summary>
		public abstract string AbsoluteUri { get; }

        /// <summary>
        /// Gets activation mode passed through the HTTP query string.
        /// </summary>
		public abstract string ActivationMode{ get; }

        /// <summary>
        /// Return an <see cref="FluorineFx.Context.IResource"/> handle for the specified location.
        /// </summary>
        /// <param name="location">The resource location.</param>
        /// <returns>An appropriate <see cref="FluorineFx.Context.IResource"/> handle.</returns>
        public virtual IResource GetResource(string location)
        {
            return new FileSystemResource(location);
        }

        /// <summary>
        /// Validates the context.
        /// </summary>
        public static void ValidateContext()
        {
            if (Current == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn(__Res.GetString(__Res.Context_MissingError));
            }
        }
	}
}
