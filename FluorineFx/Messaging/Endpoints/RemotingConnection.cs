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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using log4net;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Util;
using FluorineFx.Context;
using FluorineFx.Scheduling;
using FluorineFx.Collections;
using FluorineFx.Configuration;


namespace FluorineFx.Messaging.Endpoints
{
    class RemotingConnection : BaseConnection
    {
        private static ILog log = LogManager.GetLogger(typeof(RemotingConnection));
        IEndpoint _endpoint;

        public RemotingConnection(IEndpoint endpoint, ISession session, string path, string connectionId, Hashtable parameters)
            : base(path, connectionId, parameters)
        {
            _endpoint = endpoint;
            _session = session;
        }

        public override IPEndPoint RemoteEndPoint
        {
            get 
            {
                IPAddress ipAddress = IPAddress.Parse(System.Web.HttpContext.Current.Request.UserHostAddress);
                return new IPEndPoint(ipAddress, 80);
            }
        }

        public IEndpoint Endpoint { get { return _endpoint; } }

        public override long ReadBytes
        {
            get { return 0; }
        }

        public override long WrittenBytes
        {
            get { return 0; }
        }

        public override int LastPingTime
        {
            get { return -1; }
        }

        /*
        public override int ClientLeaseTime
        {
            get
            {
                int timeout = this.Endpoint.GetMessageBroker().FlexClientSettings.TimeoutMinutes;
                if (this.Endpoint is AMFEndpoint)
                {
                    timeout = Math.Max(timeout, 1);//start with 1 minute timeout at least
                    AMFEndpoint amfEndpoint = this.Endpoint as AMFEndpoint;
                    Debug.Assert(amfEndpoint.GetSettings().IsPollingEnabled);
                    int pollingInterval = amfEndpoint.GetSettings().PollingIntervalSeconds / 60;
                    timeout = Math.Max(timeout, pollingInterval + 1);//set timout 1 minute longer then the polling interval in minutes
                }
                return timeout;
            }
        }
        */
    }
}
