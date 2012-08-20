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

using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
    /// Base IScopeHandler implementation.
	/// </summary>
	class CoreHandler : IScopeHandler
	{
		public CoreHandler()
		{
		}

		#region IScopeHandler Members

		public bool Start(IScope scope)
		{
			return true;
		}

		public void Stop(IScope scope)
		{
			//NA
		}

		public bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
            IScope connectionScope = connection.Scope;
            // Use client registry from scope the client connected to.
            IClientRegistry clientRegistry = connectionScope.Context.ClientRegistry;

            IClient client = null;
            if (connection.IsFlexClient)
            {
                if (parameters != null && parameters.Length > 2)
                {
                    string clientId = parameters[1] as string;
                    client = clientRegistry.GetClient(clientId);
                }
            }
            if (client == null)
            {
                // Prefered Client id would be the connection's id.
                client = clientRegistry.GetClient(connection.ConnectionId);
            }
            // We have a context, and a client object.. time to init the conneciton.
            (connection as BaseConnection).Initialize(client);
            if (scope.Endpoint != null)
                client.Renew(scope.Endpoint.ClientLeaseTime);
            else
                client.Renew(MessageBroker.GetMessageBroker(MessageBroker.DefaultMessageBrokerId).FlexClientSettings.TimeoutMinutes);
            // TODO: we could check for banned clients here 

            FluorineContext.Current.SetSession(connection.Session);
            FluorineContext.Current.SetClient(client);
            FluorineContext.Current.SetConnection(connection);
            //Current objects are set notify listeners.
            if (FluorineContext.Current.Session != null && FluorineContext.Current.Session.IsNew)
                FluorineContext.Current.Session.NotifyCreated();
            if (FluorineContext.Current.Session == null)
                return false;
            if (client != null)
            {
                client.RegisterSession(FluorineContext.Current.Session);
                client.NotifyCreated();
            }
            return true;
        }

		public void Disconnect(IConnection connection, IScope scope)
		{
			//NA
		}

		public bool AddChildScope(IBasicScope scope)
		{
			return true;
		}

		public void RemoveChildScope(IBasicScope scope)
		{
			//NA
		}

		public bool Join(IClient client, IScope scope)
		{
			return true;
		}

		public void Leave(IClient client, IScope scope)
		{
			//NA
		}

		public bool ServiceCall(IConnection connection, IServiceCall call)
		{
			IScopeContext context = connection.Scope.Context;
			if(call.ServiceName != null) 
			{
				context.ServiceInvoker.Invoke(call, context);
			} 
			else 
			{
				context.ServiceInvoker.Invoke(call, connection.Scope.Handler);
			}
			return true;
		}

		#endregion

		#region IEventHandler Members

		public bool HandleEvent(FluorineFx.Messaging.Api.Event.IEvent evt)
		{
			return false;
		}

		#endregion
	}
}
