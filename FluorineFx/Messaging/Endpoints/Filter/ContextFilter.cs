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
using System.Security.Principal;
using System.Security;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using System.Reflection;
using log4net;
using FluorineFx.Exceptions;
using FluorineFx.Diagnostic;
using FluorineFx.IO;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Services;
using FluorineFx.Security;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Endpoints.Filter
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class ContextFilter : AbstractFilter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ContextFilter));
        IEndpoint _endpoint;

        public ContextFilter(IEndpoint endpoint)
		{
			_endpoint = endpoint;
		}

		#region IFilter Members

        public override void Invoke(AMFContext context)
        {
			MessageOutput messageOutput = context.MessageOutput;
            for (int i = 0; i < context.AMFMessage.BodyCount; i++)
            {
                AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
                //Check for Flex2 messages
                if (amfBody.IsEmptyTarget)
                {
                    object content = amfBody.Content;
                    if (content is IList)
                        content = (content as IList)[0];
                    IMessage message = content as IMessage;
                    if (message != null)
                    {
                        Client client = null;
                        HttpSession session = null;
                        if (FluorineContext.Current.Client == null)
                        {
                            IClientRegistry clientRegistry = _endpoint.GetMessageBroker().ClientRegistry;
                            string clientId = message.GetFlexClientId();
                            if (!clientRegistry.HasClient(clientId))
                            {
                                lock (clientRegistry.SyncRoot)
                                {
                                    if (!clientRegistry.HasClient(clientId))
                                    {
                                        client = _endpoint.GetMessageBroker().ClientRegistry.GetClient(clientId) as Client;
                                    }
                                }
                            }
                            if (client == null)
                                client = _endpoint.GetMessageBroker().ClientRegistry.GetClient(clientId) as Client;
                            FluorineContext.Current.SetClient(client);
                        }
                        session = _endpoint.GetMessageBroker().SessionManager.GetHttpSession(HttpContext.Current);
                        FluorineContext.Current.SetSession(session);
                        //Context initialized, notify listeners.
                        if (session != null && session.IsNew)
                            session.NotifyCreated();
                        if (client != null)
                        {
                            if (session != null)
                                client.RegisterSession(session);
                            if (client.IsNew)
                            {
                                client.Renew(_endpoint.ClientLeaseTime);
                                client.NotifyCreated();
                            }
                        }
                        /*
                        RemotingConnection remotingConnection = null;
                        foreach (IConnection connection in client.Connections)
                        {
                            if (connection is RemotingConnection)
                            {
                                remotingConnection = connection as RemotingConnection;
                                break;
                            }
                        }
                        if (remotingConnection == null)
                        {
                            remotingConnection = new RemotingConnection(_endpoint, null, client.Id, null);
                            remotingConnection.Initialize(client, session);
                        }
                        FluorineContext.Current.SetConnection(remotingConnection);
                        */
                    }
                }
                else
                {
                    //Flash remoting
                    AMFHeader amfHeader = context.AMFMessage.GetHeader(AMFHeader.AMFDSIdHeader);
                    string amfDSId = null;
                    if (amfHeader == null )
                    {
                        amfDSId = Guid.NewGuid().ToString("D");
                        ASObject asoObjectDSId = new ASObject();
                        asoObjectDSId["name"] = AMFHeader.AMFDSIdHeader;
                        asoObjectDSId["mustUnderstand"] = false;
                        asoObjectDSId["data"] = amfDSId;//set
                        AMFHeader headerDSId = new AMFHeader(AMFHeader.RequestPersistentHeader, true, asoObjectDSId);
                        context.MessageOutput.AddHeader(headerDSId);
                    }
                    else
                        amfDSId = amfHeader.Content as string;

                    Client client = null;
                    HttpSession session = null;
                    if (FluorineContext.Current.Client == null)
                    {
                        IClientRegistry clientRegistry = _endpoint.GetMessageBroker().ClientRegistry;
                        string clientId = amfDSId;
                        if (!clientRegistry.HasClient(clientId))
                        {
                            lock (clientRegistry.SyncRoot)
                            {
                                if (!clientRegistry.HasClient(clientId))
                                {
                                    client = _endpoint.GetMessageBroker().ClientRegistry.GetClient(clientId) as Client;
                                }
                            }
                        }
                        if (client == null)
                            client = _endpoint.GetMessageBroker().ClientRegistry.GetClient(clientId) as Client;
                    }
                    FluorineContext.Current.SetClient(client);
                    session = _endpoint.GetMessageBroker().SessionManager.GetHttpSession(HttpContext.Current);
                    FluorineContext.Current.SetSession(session);
                    //Context initialized, notify listeners.
                    if (session != null && session.IsNew)
                        session.NotifyCreated();
                    if (client != null)
                    {
                        if (session != null)
                            client.RegisterSession(session);
                        if (client.IsNew)
                        {
                            client.Renew(_endpoint.ClientLeaseTime);
                            client.NotifyCreated();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
