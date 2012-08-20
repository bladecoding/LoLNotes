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
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Scope type for publishing that deals with pipe connection events.
    /// </summary>
    class BroadcastScope : BasicScope, IBroadcastScope, IPipeConnectionListener
    {
        /// <summary>
        /// Simple in memory push pipe, triggered by an active provider to push messages to consumer.
        /// </summary>
        private InMemoryPushPushPipe _pipe;
        /// <summary>
        /// Number of components.
        /// </summary>
        private int _compCounter;
        /// <summary>
        /// Remove flag.
        /// </summary>
        private bool _hasRemoved;

        public BroadcastScope(IScope parent, String name):base(parent, Constants.BroadcastScopeType, name, false)
        {
            _pipe = new InMemoryPushPushPipe();
            _pipe.AddPipeConnectionListener(this);
            _compCounter = 0;
            _hasRemoved = false;
            _keepOnDisconnect = true;
        }

        #region IPipe Members

        public void AddPipeConnectionListener(IPipeConnectionListener listener)
        {
            _pipe.AddPipeConnectionListener(listener);
        }

        public void RemovePipeConnectionListener(IPipeConnectionListener listener)
        {
            _pipe.RemovePipeConnectionListener(listener);
        }

        #endregion

        #region IMessageInput Members

        public IMessage PullMessage()
        {
            return _pipe.PullMessage();
        }

        public IMessage PullMessage(long wait)
        {
            return _pipe.PullMessage(wait);
        }

#if !(NET_1_1)
        public bool Subscribe(IConsumer consumer, Dictionary<string, object> parameterMap)
#else
        public bool Subscribe(IConsumer consumer, Hashtable parameterMap)
#endif
        {
		    lock(_pipe) 
            {
                return !_hasRemoved && _pipe.Subscribe(consumer, parameterMap);
            }
        }

        public bool Unsubscribe(IConsumer consumer)
        {
            return _pipe.Unsubscribe(consumer);
        }

        public ICollection GetConsumers()
        {
            return _pipe.GetConsumers();
        }

        public void SendOOBControlMessage(IConsumer consumer, OOBControlMessage oobCtrlMsg)
        {
            _pipe.SendOOBControlMessage(consumer, oobCtrlMsg);
        }

        #endregion

        #region IMessageOutput Members

        public void PushMessage(IMessage message)
        {
            _pipe.PushMessage(message);
        }

#if !(NET_1_1)
        public bool Subscribe(IProvider provider, Dictionary<string, object> parameterMap)
#else
        public bool Subscribe(IProvider provider, Hashtable parameterMap)
#endif
        {
            lock (_pipe)
            {
                return !_hasRemoved && _pipe.Subscribe(provider, parameterMap);
            }
        }

        public bool Unsubscribe(IProvider provider)
        {
            return _pipe.Unsubscribe(provider);
        }

        public ICollection GetProviders()
        {
            return _pipe.GetProviders();
        }

        public void SendOOBControlMessage(IProvider provider, OOBControlMessage oobCtrlMsg)
        {
            _pipe.SendOOBControlMessage(provider, oobCtrlMsg);
        }

        #endregion

        #region IPipeConnectionListener Members

        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type) 
            {
			    case PipeConnectionEvent.CONSUMER_CONNECT_PULL:
			    case PipeConnectionEvent.CONSUMER_CONNECT_PUSH:
			    case PipeConnectionEvent.PROVIDER_CONNECT_PULL:
			    case PipeConnectionEvent.PROVIDER_CONNECT_PUSH:
				    _compCounter++;
				break;
			    case PipeConnectionEvent.CONSUMER_DISCONNECT:
			    case PipeConnectionEvent.PROVIDER_DISCONNECT:
				    _compCounter--;
				    if (_compCounter <= 0) 
                    {
					    // XXX should we synchronize parent before removing?
					    if (HasParent)
                        {
                            IProviderService providerService = ScopeUtils.GetScopeService(this.Parent, typeof(IProviderService)) as IProviderService;
						    providerService.UnregisterBroadcastStream(Parent, Name);
					    }
					    _hasRemoved = true;
				    }
				    break;
			    default:
				    throw new NotSupportedException("Event type not supported: "+evt.Type);
		    }
        }

        #endregion
    }
}
