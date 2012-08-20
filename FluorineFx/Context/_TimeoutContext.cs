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
using System.Web.Security;
using System.Web.Caching;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading;
using System.Security.Cryptography;
using log4net;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Security;

namespace FluorineFx.Context
{
    sealed class _TimeoutContext : FluorineContext
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(_TimeoutContext));

        private _TimeoutContext()
		{
		}

        internal _TimeoutContext(ISession session)
        {
            _session = session;
        }

        internal _TimeoutContext(IClient client)
        {
            _client = client;
        }

        internal _TimeoutContext(IMessageClient messageClient)
        {
            _session = messageClient.Session;
            _client = messageClient.Client;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Context_Initialized, "[not available]", _client != null ? _client.Id : "[not available]", _session != null ? _session.Id : "[not available]"));
        }

        public override string RootPath
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string RequestPath
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string RequestApplicationPath
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string PhysicalApplicationPath
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string ApplicationPath
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string AbsoluteUri
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string ActivationMode
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override IResource GetResource(string location)
        {
            return new FileSystemResource(location);
        }
    }
}
