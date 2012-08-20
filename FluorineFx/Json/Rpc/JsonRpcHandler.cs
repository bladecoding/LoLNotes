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
using System.Reflection;
using System.Web;
using System.Security;
using log4net;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Json.Services;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Json.Rpc
{
    class JsonRpcHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonRpcHandler));

        HttpContext _context;
        static Hashtable Features;

        static JsonRpcHandler()
        {
            Features = new Hashtable();
        }

        public JsonRpcHandler(HttpContext context)
        {
            _context = context;
        }

        public void ProcessRequest()
        {
            HttpRequest request = _context.Request;
            string verb = request.RequestType;
            string feature = null;
            if (StringUtils.CaselessEquals(verb, "GET") || StringUtils.CaselessEquals(verb, "HEAD"))
            {
                feature = request.QueryString[null];
            }
            else if (StringUtils.CaselessEquals(verb, "POST"))
            {
                //POST means RPC.
                feature = "rpc";
            }
            IHttpHandler handler = GetFeature(feature) as IHttpHandler;
            handler.ProcessRequest(_context);
        }

        private object GetFeature(string feature)
        {
            if (Features.Contains(feature))
                return Features[feature];
            lock (Features.SyncRoot)
            {
                if (!Features.Contains(feature))
                {
                    MessageBroker messageBroker = MessageBroker.GetMessageBroker(MessageBroker.DefaultMessageBrokerId);
                    if (feature == "proxy")
                    {
                        IHttpHandler handler = new JsonRpcProxyGenerator(messageBroker);
                        Features[feature] = handler;
                        return handler;
                    }
                    if (feature == "rpc")
                    {
                        IHttpHandler handler = new JsonRpcExecutive(messageBroker);
                        Features[feature] = handler;
                        return handler;
                    }
                }
                else
                    return Features[feature];
            }
            throw new NotImplementedException(string.Format("The requested feature {0} is not implemented ", feature));
        }

    }
}
