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
using FluorineFx.Context;
using FluorineFx.Messaging;
using log4net;

namespace FluorineFx
{
    /// <summary>
    /// This API supports the FluorineFx infrastructure and is not intended to be used directly from your code.
    /// Common interface for HTTP request handlers. Loosely follows the events of the Http Pipeline.
    /// </summary>
    interface IRequestHandler
    {
        /// <summary>
        /// Initializes a request handler and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void Init(HttpApplication context);
        /// <summary>
        /// Prepares the request handler to begin processing the request.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void BeginRequest(HttpApplication context);
        /// <summary>
        /// Called when a security module has established the identity of the user.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void AuthenticateRequest(HttpApplication context);
        /// <summary>
        /// Processes the current HTTP request.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void ProcessRequest(HttpApplication context);
        /// <summary>
        /// Ends the request processing.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void EndRequest(HttpApplication context);
        /// <summary>
        /// Called before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void PreSendRequestHeaders(HttpApplication context);
    }

    /// <summary>
    /// This API supports the FluorineFx infrastructure and is not intended to be used directly from your code.
    /// Defines the set of functionality that request handler host must implement.
    /// </summary>
    interface IRequestHandlerHost
    {
        /// <summary>
        /// Attempts to compress the current request output.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        void CompressContent(HttpApplication context);
        /// <summary>
        /// Provides access to the message server.
        /// </summary>
        /// <value>The message server.</value>
        MessageServer MessageServer { get; }
    }

    class AmfRequestHandler : IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AmfRequestHandler));

        readonly IRequestHandlerHost _host;

        public AmfRequestHandler(IRequestHandlerHost host)
        {
            _host = host;
        }

        #region IRequestHandler Members

        public void Init(HttpApplication context)
        {
        }

        public void BeginRequest(HttpApplication context)
        {
            if (context.Request.ContentType == ContentType.AMF)
                context.Context.SkipAuthorization = true;
        }

        /// <summary>
        /// Called when a security module has established the identity of the user.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void AuthenticateRequest(HttpApplication context)
        {
            if (context.Request.ContentType == ContentType.AMF)
                context.Context.SkipAuthorization = true;
        }

        public void ProcessRequest(HttpApplication context)
        {
            if (_host == null)
                return;
            if (context.Request.ContentType == ContentType.AMF)
            {
                _host.CompressContent(context);
                context.Response.Clear();
                context.Response.ContentType = ContentType.AMF;
                ThreadContext.Properties["ClientIP"] = HttpContext.Current.Request.UserHostAddress;
                if (Log.IsDebugEnabled)
                    Log.Debug(__Res.GetString(__Res.Amf_Begin));
                try
                {
                    FluorineWebContext.Initialize();

                    if (_host.MessageServer != null)
                        _host.MessageServer.Service();
                    else
                    {
                        if (Log.IsFatalEnabled)
                            Log.Fatal(__Res.GetString(__Res.MessageServer_AccessFail));
                    }

                    if (Log.IsDebugEnabled)
                        Log.Debug(__Res.GetString(__Res.Amf_End));

                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
                catch (Exception ex)
                {
                    Log.Fatal(__Res.GetString(__Res.Amf_Fatal), ex);
                    context.Response.Clear();
                    context.Response.ClearHeaders();//FluorineHttpApplicationContext modifies headers
                    context.Response.Status = __Res.GetString(__Res.Amf_Fatal404) + " " + ex.Message;
                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
            }
        }

        public void EndRequest(HttpApplication context)
        {
        }
        /// <summary>
        /// Called before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void PreSendRequestHeaders(HttpApplication context)
        {
        }

        #endregion
    }


    class StreamingAmfRequestHandler : IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StreamingAmfRequestHandler));

        readonly IRequestHandlerHost _host;

        public StreamingAmfRequestHandler(IRequestHandlerHost host)
        {
            _host = host;
        }

        #region IRequestHandler Members

        public void Init(HttpApplication context)
        {
        }

        public void BeginRequest(HttpApplication context)
        {
            if (_host == null)
                return;
            if (context.Request.ContentType == ContentType.XForm)
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[HttpSession.AspNetSessionIdCookie];
                string command = HttpContext.Current.Request.Params[Messaging.Endpoints.StreamingAmfEndpoint.CommandParameterName];
                if (cookie != null && Messaging.Endpoints.StreamingAmfEndpoint.OpenCommand.Equals(command))
                {
                    HttpContext.Current.Request.Cookies.Remove(HttpSession.AspNetSessionIdCookie);
                    context.Context.Items[Session.FxASPNET_SessionId] = cookie;
                }
            }
        }
        /// <summary>
        /// Called when a security module has established the identity of the user.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void AuthenticateRequest(HttpApplication context)
        {
        }

        public void ProcessRequest(HttpApplication context)
        {
            if (_host == null)
                return;
            if (context.Request.ContentType == ContentType.XForm)
            {
                string command = context.Request.Params[Messaging.Endpoints.StreamingAmfEndpoint.CommandParameterName];
                if (!Messaging.Endpoints.StreamingAmfEndpoint.OpenCommand.Equals(command) && !Messaging.Endpoints.StreamingAmfEndpoint.CloseCommand.Equals(command))
                    return;
                if (context.Request.UrlReferrer != null && !context.Request.UrlReferrer.ToString().EndsWith(".swf"))
                    return;

                context.Response.Clear();
                ThreadContext.Properties["ClientIP"] = HttpContext.Current.Request.UserHostAddress;
                if (Log.IsDebugEnabled)
                    Log.Debug(__Res.GetString(__Res.Amf_Begin));

                try
                {
                    FluorineWebContext.Initialize();

                    if (_host.MessageServer != null)
                        _host.MessageServer.Service();
                    else
                    {
                        if (Log.IsFatalEnabled)
                            Log.Fatal(__Res.GetString(__Res.MessageServer_AccessFail));
                    }
                    if (Log.IsDebugEnabled)
                        Log.Debug(__Res.GetString(__Res.Amf_End));

                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
                catch (Exception ex)
                {
                    Log.Fatal(__Res.GetString(__Res.Amf_Fatal), ex);
                    context.Response.Clear();
                    context.Response.ClearHeaders();//FluorineHttpApplicationContext modifies headers
                    context.Response.Status = __Res.GetString(__Res.Amf_Fatal404) + " " + ex.Message;
                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
            }
        }

        public void EndRequest(HttpApplication context)
        {
        }
        /// <summary>
        /// Called before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void PreSendRequestHeaders(HttpApplication context)
        {
            if (context.Request.ContentType == ContentType.XForm)
            {
                if (context.Context.Items.Contains(Session.FxASPNET_SessionId))
                {
                    context.Response.Cookies.Remove(HttpSession.AspNetSessionIdCookie);
                    HttpCookie cookie = context.Context.Items[Session.FxASPNET_SessionId] as HttpCookie;
                    if (cookie != null)
                        context.Response.Cookies.Add(cookie);
                }
            }
        }

        #endregion
    }

    class JsonRpcRequestHandler : IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonRpcRequestHandler));

        readonly IRequestHandlerHost _host;

        public JsonRpcRequestHandler(IRequestHandlerHost host)
        {
            _host = host;
        }

        private static string GetPageName(string requestPath)
        {
            if (requestPath.IndexOf('?') != -1)
                requestPath = requestPath.Substring(0, requestPath.IndexOf('?'));
            return requestPath.Remove(0, requestPath.LastIndexOf("/") + 1);
        }

        #region IRequestHandler Members

        public void Init(HttpApplication context)
        {
        }

        public void BeginRequest(HttpApplication context)
        {
        }
        /// <summary>
        /// Called when a security module has established the identity of the user.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void AuthenticateRequest(HttpApplication context)
        {
        }

        public void ProcessRequest(HttpApplication context)
        {
            if (_host == null)
                return;
            string page = GetPageName(context.Request.RawUrl);
            if (page.ToLower() == "jsongateway.aspx")
            {
                context.Response.Clear();
                ThreadContext.Properties["ClientIP"] = HttpContext.Current.Request.UserHostAddress;
                if (Log.IsDebugEnabled)
                    Log.Debug(__Res.GetString(__Res.Json_Begin));

                try
                {
                    FluorineWebContext.Initialize();

                    Json.Rpc.JsonRpcHandler handler = new Json.Rpc.JsonRpcHandler(context.Context);
                    handler.ProcessRequest();

                    if (Log.IsDebugEnabled)
                        Log.Debug(__Res.GetString(__Res.Json_End));

                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
                catch (Exception ex)
                {
                    Log.Fatal(__Res.GetString(__Res.Json_Fatal), ex);
                    context.Response.Clear();
                    context.Response.ClearHeaders();
                    context.Response.Status = __Res.GetString(__Res.Json_Fatal404) + " " + ex.Message;
                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
            }
        }

        public void EndRequest(HttpApplication context)
        {
        }
        /// <summary>
        /// Called before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void PreSendRequestHeaders(HttpApplication context)
        {
        }

        #endregion
    }

    class RtmptRequestHandler : IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RtmptRequestHandler));

        readonly IRequestHandlerHost _host;

        public RtmptRequestHandler(IRequestHandlerHost host)
        {
            _host = host;
        }

        #region IRequestHandler Members

        public void Init(HttpApplication context)
        {
        }
        /// <summary>
        /// Called when a security module has established the identity of the user.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void AuthenticateRequest(HttpApplication context)
        {
        }

        public void BeginRequest(HttpApplication context)
        {
            if (_host == null)
                return;
            if (context.Request.ContentType == ContentType.RTMPT)
            {
                context.Response.Clear();
                context.Response.ContentType = ContentType.RTMPT;
                ThreadContext.Properties["ClientIP"] = HttpContext.Current.Request.UserHostAddress;
                if (Log.IsDebugEnabled)
                    Log.Debug(__Res.GetString(__Res.Rtmpt_Begin));

                try
                {
                    FluorineWebContext.Initialize();
                    if (context.Request.Headers["RTMPT-command"] != null)
                    {
                        Log.Debug(string.Format("ISAPI rewrite, original URL {0}", context.Request.Headers["RTMPT-command"]));
                    }

                    if (_host.MessageServer != null)
                        _host.MessageServer.ServiceRtmpt();
                    else
                    {
                        if (Log.IsFatalEnabled)
                            Log.Fatal(__Res.GetString(__Res.MessageServer_AccessFail));
                    }
                    if (Log.IsDebugEnabled)
                        Log.Debug(__Res.GetString(__Res.Rtmpt_End));
                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
                catch (Exception ex)
                {
                    Log.Fatal(__Res.GetString(__Res.Rtmpt_Fatal), ex);
                    context.Response.Clear();
                    context.Response.ClearHeaders();
                    context.Response.Status = __Res.GetString(__Res.Rtmpt_Fatal404) + " " + ex.Message;
                    // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event
                    context.CompleteRequest();
                }
            }
        }

        public void ProcessRequest(HttpApplication context)
        {
        }

        public void EndRequest(HttpApplication context)
        {
        }
        /// <summary>
        /// Called before ASP.NET sends HTTP headers to the client.
        /// </summary>
        /// <param name="context">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void PreSendRequestHeaders(HttpApplication context)
        {
        }

        #endregion
    }
    
}
