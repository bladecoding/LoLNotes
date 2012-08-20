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
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Security;
using System.Security.Permissions;
using System.Web.Hosting;
using FluorineFx.Util;
using log4net;
using FluorineFx.Browser;
using FluorineFx.Configuration;
using FluorineFx.Context;
using FluorineFx.HttpCompress;
using FluorineFx.Messaging;

//Compressing http content based on "The open compression engine for ASP.NET"
//http://www.blowery.org/code/HttpCompressionModule.html
//http://www.ondotnet.com/pub/a/dotnet/2003/10/20/httpfilter.html
//http://aspnetresources.com/articles/HttpFilters.aspx
//
// Checks the Accept-Encoding HTTP header to determine if the
// client actually supports any notion of compression.  Currently
// the deflate (zlib) and gzip compression schemes are supported.
// Compress is not implemented because it uses lzw which requires a license from 
// Unisys.  For more information about the common compression types supported,
// see http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.11 for details.

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class FluorineGateway : IHttpModule, IRequiresSessionState, IRegisteredObject, IRequestHandlerHost
	{
        private static readonly ILog Log = LogManager.GetLogger(typeof(FluorineGateway));

		internal const string FluorineHttpCompressKey = "__@fluorinehttpcompress";
        internal const string FluorineMessageServerKey = "__@fluorinemessageserver";


		static int _unhandledExceptionCount;
		static string _sourceName;
		static object _objLock = new object();
		static bool _initialized;

        static MessageServer _messageServer;
        static IServiceBrowserRenderer _serviceBrowserRenderer;

        static readonly string[] PossibleConfigFolderNames = { Path.Combine("App_Data", "flex"), Path.Combine("WEB-INF", "flex"), "App_Data" };
        static List<IRequestHandler> _handlers = new List<IRequestHandler>();

	    #region IHttpModule Members

		/// <summary>
		/// Initializes the module and prepares it to handle requests.
		/// </summary>
		/// <param name="application">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
		public void Init(HttpApplication application)
		{
			//http://support.microsoft.com/kb/911816
			// Do this one time for each AppDomain.
			if (!_initialized) 
			{
				lock (_objLock) 
				{
					if (!_initialized) 
					{
                        if (Log.IsInfoEnabled)
                        {
                            Log.Info("************************************");
                            Log.Info(__Res.GetString(__Res.Fluorine_Start));
                            Log.Info(__Res.GetString(__Res.Fluorine_Version, Assembly.GetExecutingAssembly().GetName().Version));
                            Log.Info(string.Format("Common language runtime version {0}", Environment.Version));
                            Log.Info("************************************");
                            Log.Info(__Res.GetString(__Res.MessageServer_Create));
                        }
                        try
                        {
                            // See if we're running in full trust
                            new PermissionSet(PermissionState.Unrestricted).Demand();
                            //LinkDemands and InheritenceDemands Occur at JIT Time
                            //http://blogs.msdn.com/shawnfa/archive/2006/01/11/511716.aspx
                            WireAppDomain();
                            RegisterObject();
                        }
                        catch (MethodAccessException){}
                        catch (SecurityException){}

                        FluorineWebContext.Initialize();

                        Log.Info(__Res.GetString(__Res.ServiceBrowser_Aquire));
                        try
                        {
                            Type type = ObjectFactory.Locate("FluorineFx.ServiceBrowser.ServiceBrowserRenderer");
                            if (type != null)
                            {
                                _serviceBrowserRenderer = Activator.CreateInstance(type) as IServiceBrowserRenderer;
                                if (_serviceBrowserRenderer != null)
                                    Log.Info(__Res.GetString(__Res.ServiceBrowser_Aquired));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal(__Res.GetString(__Res.ServiceBrowser_AquireFail), ex);
                        }

                        try
                        {
                            _messageServer = new MessageServer();
                            string[] possibleConfigFolderPaths = new string[PossibleConfigFolderNames.Length];
                            for (int i = 0; i < PossibleConfigFolderNames.Length; i++)
                            {
                                string configPath = Path.Combine(HttpRuntime.AppDomainAppPath, PossibleConfigFolderNames[i]);
                                possibleConfigFolderPaths[i] = configPath;
                            }
                            _messageServer.Init(possibleConfigFolderPaths, _serviceBrowserRenderer != null);
                            _messageServer.Start();
                            Log.Info(__Res.GetString(__Res.MessageServer_Started));
                            HttpContext.Current.Application[FluorineMessageServerKey] = _messageServer;
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal(__Res.GetString(__Res.MessageServer_StartError), ex);
                        }

                        _handlers.Add(new AmfRequestHandler(this));
                        _handlers.Add(new StreamingAmfRequestHandler(this));
                        _handlers.Add(new RtmptRequestHandler(this));
                        _handlers.Add(new JsonRpcRequestHandler(this));
                        _initialized = true;
					}
				}
			}

			//Wire up the HttpApplication events.
			//
			//BeginRequest 
			//AuthenticateRequest 
			//AuthorizeRequest 
			//ResolveRequestCache 
			//A handler (a page corresponding to the request URL) is created at this point.
			//AcquireRequestState ** Session State ** 
			//PreRequestHandlerExecute 
			//[The handler is executed.] 
			//PostRequestHandlerExecute 
			//ReleaseRequestState 
			//Response filters, if any, filter the output.
			//UpdateRequestCache 
			//EndRequest 

			application.BeginRequest += ApplicationBeginRequest;
            if (!FluorineConfiguration.Instance.FluorineSettings.Runtime.AsyncHandler)
            {
                application.PreRequestHandlerExecute += ApplicationPreRequestHandlerExecute;
            }
            else
            {
                application.AddOnPreRequestHandlerExecuteAsync(BeginPreRequestHandlerExecute, EndPreRequestHandlerExecute);
            }

			application.AuthenticateRequest += ApplicationAuthenticateRequest;

			//This implementation hooks the ReleaseRequestState and PreSendRequestHeaders events to 
			//figure out as late as possible if we should install the filter.
			//The Post Release Request State is the event most fitted for the task of adding a filter
			//Everything else is too soon or too late. At this point in the execution phase the entire 
			//response content is created and the page has fully executed but still has a few modules to go through
			//from an ASP.NET perspective.  We filter the content here and all of the javascript renders correctly.
			//application.PostReleaseRequestState += new EventHandler(this.CompressContent);
			application.ReleaseRequestState += ApplicationReleaseRequestState;
			application.PreSendRequestHeaders += ApplicationPreSendRequestHeaders;
			application.EndRequest += ApplicationEndRequest;
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements IHttpModule.
		/// </summary>
		public void Dispose()
		{
		}

		#endregion

        void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                Unreferenced.Parameter(ex);
            }
        }

		/// <summary>
		/// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
		/// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ApplicationBeginRequest(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			HttpRequest httpRequest = httpApplication.Request;

            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].BeginRequest(httpApplication);

			if( _serviceBrowserRenderer != null )
			{
				if( _serviceBrowserRenderer.CanRender(httpRequest) )
				{
					CompressContent(httpApplication);

                    FluorineWebContext.Initialize();
					httpApplication.Response.Clear();
					//httpApplication.Response.ClearHeaders();
					_serviceBrowserRenderer.Render(httpApplication);
					httpApplication.CompleteRequest();
					return;
				}
			}
		}

		/// <summary>
		/// Occurs just before ASP.NET begins executing a handler such as a page or XML Web service.
		/// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ApplicationPreRequestHandlerExecute(object sender, EventArgs e)
		{
			HttpApplication context = (HttpApplication)sender;
            ProcessRequest(context);
		}

        internal void ProcessRequest(HttpApplication context)
        {
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].ProcessRequest(context);
        }

        IAsyncResult BeginPreRequestHandlerExecute(Object source, EventArgs e, AsyncCallback cb, Object state)
        {
            HttpApplication httpApplication = (HttpApplication)source;
            AsyncHandler asyncHandler = new AsyncHandler(cb, this, httpApplication, state);
            asyncHandler.Start();
            return asyncHandler;
        }

        void EndPreRequestHandlerExecute(IAsyncResult ar)
        {
            AsyncHandler asyncHandler = ar as AsyncHandler;
            Unreferenced.Parameter(asyncHandler);
        }

		/// <summary>
		/// Occurs when a security module has established the identity of the user.
		/// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ApplicationAuthenticateRequest(object sender, EventArgs e)
		{
            HttpApplication context = (HttpApplication)sender;
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].AuthenticateRequest(context);
		}
        /// <summary>
        /// Occurs after ASP.NET finishes executing all request event handlers. This event causes state modules to save the current state data.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ApplicationReleaseRequestState(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			CompressContent(httpApplication);
		}

        /// <summary>
        /// Occurs just before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ApplicationPreSendRequestHeaders(object sender, EventArgs e)
		{
            HttpApplication context = (HttpApplication)sender;
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].PreSendRequestHeaders(context);
            CompressContent(context);
		}

		private void ApplicationEndRequest(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			if( httpApplication.Response.Filter is CompressingFilter || httpApplication.Response.Filter is ThresholdFilter)
			{
				CompressingFilter compressingFilter;
				if( httpApplication.Response.Filter is ThresholdFilter )
					compressingFilter = (httpApplication.Response.Filter as ThresholdFilter).CompressingFilter;
				else
					compressingFilter = httpApplication.Response.Filter as CompressingFilter;

				if( compressingFilter != null && Log != null && Log.IsDebugEnabled )
				{
					float ratio = 0;
					if( compressingFilter.TotalIn != 0 )
						ratio = (float)(compressingFilter.TotalOut * 100) / compressingFilter.TotalIn;
					string realPath = Path.GetFileName(httpApplication.Request.Path);
                    if (httpApplication.Request.ContentType == ContentType.AMF)
						realPath += "(x-amf)";
					string msg = __Res.GetString(__Res.Compress_Info, realPath, ratio);
					Log.Debug(msg);
				}
			}
		}

        #region IRequestHandlerHost Members


        public MessageServer MessageServer
        {
            get { return _messageServer; }
        }

		#region Compress

		/// <summary>
		/// EventHandler that gets ahold of the current request context and attempts to compress the output.
		/// </summary>
		/// <param name="httpApplication">The <see cref="HttpApplication"/> that is firing this event.</param>
		public void CompressContent(HttpApplication httpApplication) 
		{
			// Only do this if we haven't already attempted an install.  This prevents PreSendRequestHeaders from
			// trying to add this item way to late.  We only want the first run through to do anything.
			// also, we use the context to store whether or not we've attempted an add, as it's thread-safe and
			// scoped to the request.  An instance of this module can service multiple requests at the same time,
			// so we cannot use a member variable.
            if (!httpApplication.Context.Items.Contains(FluorineHttpCompressKey))
            {

                // Log the install attempt in the HttpContext. Must do this first as several IF statements below skip full processing of this method
                httpApplication.Context.Items.Add(FluorineHttpCompressKey, 1);
                // Get the config settings
                HttpCompressSettings settings = FluorineConfiguration.Instance.HttpCompressSettings;
                // Skip if no request can be handled
                if (settings.HandleRequest == HandleRequest.None)
                    return;
                // Skip if only AMF is compressed and we do not have an AMF request
                if (settings.HandleRequest == HandleRequest.Amf && httpApplication.Request.ContentType != ContentType.AMF)
                    return;
                // Skip if the CompressionLevel is set to 'None'
                if (settings.CompressionLevel == CompressionLevels.None)
                    return;
                //string realPath = httpApplication.Request.Path.Remove(0, httpApplication.Request.ApplicationPath.Length+1);
                string realPath = Path.GetFileName(httpApplication.Request.Path);
                if (settings.IsExcludedPath(realPath))
                {
                    // Skip if the file path excludes compression
                    return;
                }
                // Skip if the MimeType excludes compression
                if (httpApplication.Response.ContentType == null || settings.IsExcludedMimeType(httpApplication.Response.ContentType))
                    return;

                // Fix to handle caching appropriately, see http://www.pocketsoap.com/weblog/2003/07/1330.html
                // This header is added only when the request has the possibility of being compressed
                // i.e. it is not added when the request is excluded from compression by CompressionLevel, Path, or MimeType
                httpApplication.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;

                // Grab an array of algorithm;q=x, algorith;q=x style values
                string acceptedTypes = httpApplication.Request.Headers["Accept-Encoding"];
                // If we couldn't find the header, bail out
                if (acceptedTypes == null)
                    return;

                // The actual types could be , delimited.  split 'em out.
                string[] types = acceptedTypes.Split(',');

                CompressingFilter filter = GetFilterForScheme(types, httpApplication.Response.Filter, settings);
                // If we didn't find a filter, bail out
                if (filter == null)
                    return;
                // If we get here, we found a viable filter.
                // Set the filter and change the Content-Encoding header to match so the client can decode the response
                if (httpApplication.Request.ContentType == ContentType.AMF)
                    httpApplication.Response.Filter = new ThresholdFilter(filter, httpApplication.Response.Filter, settings.Threshold);
                else
                    httpApplication.Response.Filter = filter;
            }
		}

		/// <summary>
		/// Get ahold of a <see cref="CompressingFilter"/> for the given encoding scheme.
		/// If no encoding scheme can be found, it returns null.
		/// </summary>
		/// <remarks>
		/// See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3 for details
		/// on how clients are supposed to construct the Accept-Encoding header.  This
		/// implementation follows those rules, though we allow the server to override
		/// the preference given to different supported algorithms.  I'm doing this as 
		/// I would rather give the server control over the algorithm decision than 
		/// the client.  If the clients send up * as an accepted encoding with highest
		/// quality, we use the preferred algorithm as specified in the config file.
		/// </remarks>
		internal static CompressingFilter GetFilterForScheme(string[] schemes, Stream output, HttpCompressSettings prefs) 
		{
			bool foundDeflate = false;
			bool foundGZip = false;
			bool foundStar = false;
      
			float deflateQuality = 0f;
			float gZipQuality = 0f;
			float starQuality = 0f;

		    for (int i = 0; i<schemes.Length;i++) 
			{
				string acceptEncodingValue = schemes[i].Trim().ToLower();

				if (acceptEncodingValue.StartsWith("deflate")) 
				{
					foundDeflate = true;
		  
					float newDeflateQuality = GetQuality(acceptEncodingValue);
					if (deflateQuality < newDeflateQuality)
						deflateQuality = newDeflateQuality;
				}
				else if (acceptEncodingValue.StartsWith("gzip") || acceptEncodingValue.StartsWith("x-gzip")) 
				{
					foundGZip = true;
		  
					float newGZipQuality = GetQuality(acceptEncodingValue);
					if (gZipQuality < newGZipQuality)
						gZipQuality = newGZipQuality;
				}
				else if (acceptEncodingValue.StartsWith("*")) 
				{
					foundStar = true;
		  
					float newStarQuality = GetQuality(acceptEncodingValue);
					if (starQuality < newStarQuality)
						starQuality = newStarQuality;
				}
			}

			bool isAcceptableStar = foundStar && (starQuality > 0);
			bool isAcceptableDeflate = (foundDeflate && (deflateQuality > 0)) || (!foundDeflate && isAcceptableStar);
			bool isAcceptableGZip = (foundGZip && (gZipQuality > 0)) || (!foundGZip && isAcceptableStar);

			if (isAcceptableDeflate && !foundDeflate)
				deflateQuality = starQuality;

			if (isAcceptableGZip && !foundGZip)
				gZipQuality = starQuality;


			// Do they support any of our compression methods?
			if(!(isAcceptableDeflate || isAcceptableGZip || isAcceptableStar)) 
				return null;
      
			// If deflate is better according to client
			if (isAcceptableDeflate && (!isAcceptableGZip || (deflateQuality > gZipQuality)))
				return new DeflateFilter(output, prefs.CompressionLevel);
      
			// If gzip is better according to client
			if (isAcceptableGZip && (!isAcceptableDeflate || (deflateQuality < gZipQuality)))
				return new GZipFilter(output);

			// If we're here, the client either didn't have a preference or they don't support compression
			if(isAcceptableDeflate && (prefs.PreferredAlgorithm == Algorithms.Deflate || prefs.PreferredAlgorithm == Algorithms.Default))
				return new DeflateFilter(output, prefs.CompressionLevel);
			if(isAcceptableGZip && prefs.PreferredAlgorithm == Algorithms.GZip)
				return new GZipFilter(output);

			if(isAcceptableDeflate || isAcceptableStar)
				return new DeflateFilter(output, prefs.CompressionLevel);
			if(isAcceptableGZip)
				return new GZipFilter(output);

			// return null.  we couldn't find a filter.
			return null;
		}
	
		static float GetQuality(string acceptEncodingValue) 
		{
			int qParam = acceptEncodingValue.IndexOf("q=");

			if (qParam >= 0) 
			{
				float val = 0.0f;
				try 
				{
					val = float.Parse(acceptEncodingValue.Substring(qParam+2, acceptEncodingValue.Length - (qParam+2)));
				} 
				catch(FormatException) 
				{
				}
				return val;
			}
		    return 1;
		}

		#endregion Compress

        #endregion

        #region kb911816

        private void WireAppDomain()
		{
			string webenginePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "webengine.dll");
			if (File.Exists(webenginePath))
			{
				//This requires .NET Framework 2.0
				FileVersionInfo ver = FileVersionInfo.GetVersionInfo(webenginePath);
				_sourceName = string.Format(CultureInfo.InvariantCulture, "ASP.NET {0}.{1}.{2}.0", ver.FileMajorPart, ver.FileMinorPart, ver.FileBuildPart);
				if (EventLog.SourceExists(_sourceName))
				{
					//This requires .NET Framework 2.0
					AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
				}
			}

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            Log.Info("Adding handler for the DomainUnload event");
            //If we do not have permission to handle DomainUnload but in this case the socket server will not start either
		}

        private void RegisterObject()
        {
#if !MONO
            HostingEnvironment.RegisterObject(this);
            Log.Info("FluorineFx registered in the hosting environment");
#endif
        }

		void OnUnhandledException(object o, UnhandledExceptionEventArgs e) 
		{
			// Let this occur one time for each AppDomain.
			if (Interlocked.Exchange(ref _unhandledExceptionCount, 1) != 0)
				return;

			StringBuilder message = new StringBuilder("\r\n\r\nUnhandledException logged by UnhandledExceptionModule.dll:\r\n\r\nappId=");

			string appId = (string) AppDomain.CurrentDomain.GetData(".appId");
			if (appId != null) 
				message.Append(appId);

            for (Exception currentException = (Exception)e.ExceptionObject; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("\r\n\r\ntype={0}\r\n\r\nmessage={1}\r\n\r\nstack=\r\n{2}\r\n\r\n",
                    currentException.GetType().FullName,
                    currentException.Message,
                    currentException.StackTrace);
            }
			EventLog eventLog = new EventLog();
            eventLog.Source = _sourceName;
            eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error);
            Log.Fatal(message.ToString());
        }

		#endregion kb911816

        #region IRegisteredObject Members

        /// <summary>
        /// Requests a registered object to unregister.
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting environment before returning; otherwise, false.</param>
        public void Stop(bool immediate)
        {
            try
            {
                Stop();
            }
            catch(Exception ex)
            { 
                Unreferenced.Parameter(ex); 
            }
            HostingEnvironment.UnregisterObject(this);
        }

        #endregion

        /// <summary>
        /// Stops the message server.
        /// </summary>
        private void Stop()
        {
            lock (_objLock)
            {
                if (_messageServer != null)
                    _messageServer.Stop();
                _messageServer = null;
                Log.Info("Stopped FluorineFx Gateway");
            }
        }
    }
}
