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
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading;
using log4net;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;
using FluorineFx.Security;
using FluorineFx.Messaging.Rtmp;

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class FluorineRtmpContext : FluorineContext
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(FluorineRtmpContext));

        private FluorineRtmpContext(IConnection connection)
        {
            _connection = connection;
            _session = connection.Session;
            _client = connection.Client;
            if (_client != null)
                _client.Renew();
        }

        internal static void Initialize(IConnection connection)
        {
            FluorineRtmpContext fluorineContext = new FluorineRtmpContext(connection);
            WebSafeCallContext.SetData(FluorineContext.FluorineContextKey, fluorineContext);
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Context_Initialized, connection.ConnectionId, connection.Client != null ? connection.Client.Id : "[not set]", connection.Session != null ? connection.Session.Id : "[not set]"));
        }

        public FluorineRtmpContext()
		{
		}

		/// <summary>
		/// Gets the physical drive path of the application directory for the application hosted in the current application domain.
		/// </summary>
		public override string RootPath
		{ 
			get
			{
				//return HttpRuntime.AppDomainAppPath;
                return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		/// <summary>
		/// Gets the virtual path of the current request.
		/// </summary>
		public override string RequestPath 
		{ 
			get { return null; }
		}
		/// <summary>
		/// Gets the ASP.NET application's virtual application root path on the server.
		/// </summary>
		public override string RequestApplicationPath
		{ 
			get { return null; }
		}

        public override string ApplicationPath
        {
            get
            {
                return null;
            }
        }

		/// <summary>
		/// Gets the absolute URI from the URL of the current request.
		/// </summary>
		public override string AbsoluteUri
		{ 
			get{ return null; }
		}

		public override string ActivationMode
		{
			get
			{
				return null;
			}
		}

		public override string PhysicalApplicationPath
		{
			get
			{
				//return HttpRuntime.AppDomainAppPath;
                return AppDomain.CurrentDomain.BaseDirectory;
			}
		}
    }
}
