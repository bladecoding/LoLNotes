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
using log4net;
using FluorineFx.Context;
using FluorineFx.Messaging;

namespace FluorineFx
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class RtmptHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmptHandler));

        private MessageServer GetMessageServer(HttpContext context)
        {
            return context.Application[FluorineGateway.FluorineMessageServerKey] as MessageServer;
        }

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }
        /// <summary>
        /// Processing of RTMPT HTTP Web requests.
        /// </summary>
        /// <param name="context">An HttpContext object that provides references to the intrinsic server objects used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            HttpApplication httpApplication = context.ApplicationInstance;
            if (httpApplication.Request.ContentType == ContentType.RTMPT)
            {
                httpApplication.Response.Clear();
                httpApplication.Response.ContentType = ContentType.RTMPT;

                log4net.ThreadContext.Properties["ClientIP"] = System.Web.HttpContext.Current.Request.UserHostAddress;
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Amf_Begin));

                try
                {
                    //FluorineWebContext.Initialize();

                    MessageServer messageServer = GetMessageServer(context);
                    if (messageServer != null)
                        messageServer.ServiceRtmpt();
                    else
                        log.Fatal(__Res.GetString(__Res.MessageServer_AccessFail));

                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.Amf_End));

                    httpApplication.CompleteRequest();
                }
                catch (Exception ex)
                {
                    log.Fatal(__Res.GetString(__Res.Amf_Fatal), ex);
                    httpApplication.Response.Clear();
                    httpApplication.Response.ClearHeaders();
                    httpApplication.Response.Status = __Res.GetString(__Res.Amf_Fatal404) + " " + ex.Message;
                    httpApplication.CompleteRequest();
                }
            }
        }

        #endregion
    }
}
