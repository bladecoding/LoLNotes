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
using System.IO;
using System.Collections;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using log4net;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Api.Messaging
{
    /// <summary>
    /// A simple in-memory version of pull-pull pipe.
    /// It is triggered by an active consumer that pulls messages through it from a pullable provider.
    /// </summary>
    class InMemoryPullPullPipe : AbstractPipe
    {
        private static ILog log = LogManager.GetLogger(typeof(InMemoryPullPullPipe));

#if !(NET_1_1)
        public override bool Subscribe(IConsumer consumer, Dictionary<string, object> parameterMap)
#else
        public override bool Subscribe(IConsumer consumer, Hashtable parameterMap)
#endif
        {
            bool success = base.Subscribe(consumer, parameterMap);
            if (success)
                FireConsumerConnectionEvent(consumer, PipeConnectionEvent.CONSUMER_CONNECT_PULL, parameterMap);
            return success;
        }

#if !(NET_1_1)
        public override bool Subscribe(IProvider provider, Dictionary<string, object> parameterMap)
#else
        public override bool Subscribe(IProvider provider, Hashtable parameterMap)
#endif
        {
		    if (!(provider is IPullableProvider)) 
            {
			    throw new NotSupportedException("Non-pullable provider not supported by PullPullPipe");
		    }
            bool success = base.Subscribe(provider, parameterMap);
		    if (success) 
			    FireProviderConnectionEvent(provider, PipeConnectionEvent.PROVIDER_CONNECT_PULL, parameterMap);
		    return success;
        }

        public override IMessage PullMessage()
        {
		    IMessage message = null;
		    foreach(IProvider provider in _providers)
            {
			    if (!(provider is IPullableProvider))
				    continue;
    			
			    // choose the first available provider
			    try 
                {
				    message = (provider as IPullableProvider).PullMessage(this);
				    if (message != null)
					    break;
			    } 
                catch (Exception ex)
                {
                    if (ex is IOException)// Pass this along
					    throw ex;
				    log.Error("Exception when pulling message from provider", ex);
			    }
		    }
		    return message;
        }

        public override IMessage PullMessage(long wait)
        {
		    IMessage message = null;
		    // Divided evenly
		    int size = _providers.Count;
		    long averageWait = size > 0 ? wait / size : 0;
		    // Choose the first available provider
		    foreach (IProvider provider in _providers) 
            {
			    if (!(provider is IPullableProvider))
				    continue;
    			
			    try 
                {
                    message = (provider as IPullableProvider).PullMessage(this, averageWait);
				    if (message != null)
					    break;
			    } 
                catch (Exception ex) 
                {
				    log.Error("Exception when pulling message from provider", ex);
			    }
		    }
		    return message;
        }

        public override void PushMessage(IMessage message)
        {
            // push mode ignored
        }
    }
}
