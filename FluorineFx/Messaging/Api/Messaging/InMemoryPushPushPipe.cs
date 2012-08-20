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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using log4net;
using FluorineFx.Util;
using FluorineFx.Messaging.Messages;
using FluorineFx.Exceptions;

namespace FluorineFx.Messaging.Api.Messaging
{
    /// <summary>
    /// A simple in-memory version of push-push pipe.
    /// It is triggered by an active provider to push messages through it to an event-driven consumer.
    /// </summary>
    class InMemoryPushPushPipe : AbstractPipe
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InMemoryPushPushPipe));

#if !(NET_1_1)
        public override bool Subscribe(IConsumer consumer, Dictionary<string, object> parameterMap)
#else
        public override bool Subscribe(IConsumer consumer, Hashtable parameterMap)
#endif
        {
		    if (!(consumer is IPushableConsumer)) 
			    throw new FluorineException("Non-pushable consumer not supported by PushPushPipe");
            bool success = base.Subscribe(consumer, parameterMap);
		    if (success) 
			    FireConsumerConnectionEvent(consumer, PipeConnectionEvent.CONSUMER_CONNECT_PUSH, parameterMap);		
		    return success;
        }

#if !(NET_1_1)
        public override bool Subscribe(IProvider provider, Dictionary<string, object> parameterMap)
#else
        public override bool Subscribe(IProvider provider, Hashtable parameterMap)
#endif
        {
            bool success = base.Subscribe(provider, parameterMap);
            if (success)
                FireProviderConnectionEvent(provider, PipeConnectionEvent.PROVIDER_CONNECT_PUSH, parameterMap);
            return success;
        }

        public override IMessage PullMessage()
        {
            return null;
        }

        public override IMessage PullMessage(long wait)
        {
            return null;
        }
        /// <summary>
        /// Pushes a message out to all the PushableConsumers.
        /// </summary>
        /// <param name="message"></param>
        public override void PushMessage(IMessage message)
        {
            foreach (IConsumer consumer in GetConsumers())
            {
                try
                {
                    (consumer as IPushableConsumer).PushMessage(this, message);
                }
                catch (Exception ex)
                {
                    if (ex is System.IO.IOException)
                        throw ex;
                    log.Error("Exception when pushing message to consumer", ex);
                }
            }
        }
    }
}
