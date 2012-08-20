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
using System.Net;
using System.Web;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using log4net;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmpt;
using FluorineFx.Messaging.Adapter;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Messaging.Api;
using FluorineFx.Context;
using FluorineFx.Configuration;

namespace FluorineFx.Messaging.Endpoints
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class RtmptEndpoint : EndpointBase
    {
        public const string FluorineRtmptEndpointId = "__@fluorinertmpt";

        private static readonly ILog log = LogManager.GetLogger(typeof(RtmptEndpoint));
        //static object _objLock = new object();
        RtmptServer _rtmptServer;

        public RtmptEndpoint(MessageBroker messageBroker, ChannelDefinition channelDefinition)
            : base(messageBroker, channelDefinition)
		{
        }

        public override void Start()
        {
            _rtmptServer = new RtmptServer(this);
        }

        public override void Stop()
        {
        }

        public override void Service()
        {
            _rtmptServer.Service(HttpContext.Current.Request, HttpContext.Current.Response);
        }

        public void Service(RtmptRequest rtmptRequest)
        {
            _rtmptServer.Service(rtmptRequest);
        }

        public override int ClientLeaseTime
        {
            get 
            {
                int timeout = this.GetMessageBroker().FlexClientSettings.TimeoutMinutes;
                timeout = Math.Max(timeout, 1);//start with 1 minute timeout at least
                return timeout;
            }
        }
    }
}
