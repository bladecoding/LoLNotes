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
using FluorineFx.Collections;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Api.Messaging
{
    /// <summary>
    /// Abstract pipe that books providers/consumers and listeners.
    /// Aim to ease the implementation of concrete pipes. For more
    /// information on what pipe is, see IPipe interface documentation.
    /// </summary>
    abstract class AbstractPipe : IPipe
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AbstractPipe));

        protected CopyOnWriteArray _consumers = new CopyOnWriteArray();
        protected CopyOnWriteArray _providers = new CopyOnWriteArray();
        protected CopyOnWriteArray _listeners = new CopyOnWriteArray();

        #region IPipe Members

        /// <summary>
        /// Registers pipe connect events listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void AddPipeConnectionListener(IPipeConnectionListener listener)
        {
            _listeners.Add(listener);
        }
        /// <summary>
        /// Removes pipe connection listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void RemovePipeConnectionListener(IPipeConnectionListener listener)
        {
            _listeners.Remove(listener);
        }

        public CopyOnWriteArray Listeners
        {
            get { return _listeners; }
            set { _listeners = value; }
        }

        #endregion

        #region IMessageInput Members

        public virtual IMessage PullMessage()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual IMessage PullMessage(long wait)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        /// <summary>
        /// Connect consumer to this pipe. Doesn't allow to connect one consumer twice.
        /// Does register event listeners if instance of IPipeConnectionListener is given.
        /// </summary>
        /// <param name="consumer">Consumer</param>
        /// <param name="parameterMap">Parameters passed with connection, used in concrete pipe implementations.</param>
        /// <returns>true if consumer was added, false otherwise</returns>
#if !(NET_1_1)
        public virtual bool Subscribe(IConsumer consumer, Dictionary<string, object> parameterMap)
#else
        public virtual bool Subscribe(IConsumer consumer, Hashtable parameterMap)
#endif
        {
		    // Pipe is possibly used by dozens of Threads at once (like many subscribers for one server stream) so make it synchronized
    	    lock(_consumers.SyncRoot) 
            {
                // Can't add one consumer twice
                if (_consumers.Contains(consumer))
				    return false;			
			    _consumers.Add(consumer);
    	    }
            // If consumer is listener object register it as listener
            if (consumer is IPipeConnectionListener)
			    _listeners.Add(consumer as IPipeConnectionListener);
		    return true;
        }
        /// <summary>
        /// Disconnects consumer from this pipe. Fires pipe connection event.
        /// </summary>
        /// <param name="consumer">Consumer that should be removed.</param>
        /// <returns>true on success, false otherwise.</returns>
        public bool Unsubscribe(IConsumer consumer)
        {
            lock (_consumers.SyncRoot)
            {
                if (!_consumers.Contains(consumer))
                    return false;
                _consumers.Remove(consumer);
            }
            FireConsumerConnectionEvent(consumer, PipeConnectionEvent.CONSUMER_DISCONNECT, null);
            if (consumer is IPipeConnectionListener)
                _listeners.Remove(consumer);
            return true;
        }

        public ICollection GetConsumers()
        {
            return _consumers;
        }
        /// <summary>
        /// Sends out-of-band ("special") control message to all providers
        /// </summary>
        /// <param name="consumer">Consumer, may be used in concrete implementations.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message.</param>
        public void SendOOBControlMessage(IConsumer consumer, OOBControlMessage oobCtrlMsg)
        {
            foreach (IProvider provider in _providers)
            {
                try
                {
                    provider.OnOOBControlMessage(consumer, this, oobCtrlMsg);
                }
                catch (Exception ex)
                {
                    log.Error("Exception when passing OOBCM from consumer to providers", ex);
                }
            }
        }

        #endregion

        #region IMessageOutput Members

        public virtual void PushMessage(IMessage message)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        /// <summary>
        /// Connect provider to this pipe. Doesn't allow to connect one provider twice.
        /// Does register event listeners if instance of IPipeConnectionListener is given.
        /// </summary>
        /// <param name="provider">Provider.</param>
        /// <param name="parameterMap">Parameters passed with connection, used in concrete pipe implementations.</param>
        /// <returns>true if provider was added, false otherwise.</returns>
#if !(NET_1_1)
        public virtual bool Subscribe(IProvider provider, Dictionary<string, object> parameterMap)
#else
        public virtual bool Subscribe(IProvider provider, Hashtable parameterMap)
#endif
        {
            // Pipe is possibly used by dozens of Threads at once (like many subscribers for one server stream) so make it synchronized
            lock (_providers.SyncRoot)
            {
                // Can't add one consumer twice
                if (_providers.Contains(provider))
                    return false;
                _providers.Add(provider);
            }
            // If consumer is listener object register it as listener
            if (provider is IPipeConnectionListener)
                _listeners.Add(provider as IPipeConnectionListener);
            return true;
        }
        /// <summary>
        /// Disconnects provider from this pipe. Fires pipe connection event.
        /// </summary>
        /// <param name="provider">Provider that should be removed.</param>
        /// <returns>true on success, false otherwise.</returns>
        public bool Unsubscribe(IProvider provider)
        {
            lock (_providers.SyncRoot)
            {
                if (!_providers.Contains(provider))
                    return false;
                _providers.Remove(provider);
            }
            FireProviderConnectionEvent(provider, PipeConnectionEvent.PROVIDER_DISCONNECT, null);
            if (provider is IPipeConnectionListener)
                _listeners.Remove(provider);
            return true;
        }

        public ICollection GetProviders()
        {
            return _providers;
        }
        /// <summary>
        /// Sends out-of-band ("special") control message to all consumers.
        /// </summary>
        /// <param name="provider">Provider, may be used in concrete implementations.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message.</param>
        public void SendOOBControlMessage(IProvider provider, OOBControlMessage oobCtrlMsg)
        {
		    foreach(IConsumer consumer in _consumers) 
            {
			    try 
                {
				    consumer.OnOOBControlMessage(provider, this, oobCtrlMsg);
			    } 
                catch(Exception ex)
                {
				    log.Error("Exception when passing OOBCM from provider to consumers", ex);
			    }
		    }
        }

        #endregion

        /// <summary>
        /// Broadcast consumer connection event.
        /// </summary>
        /// <param name="consumer">Consumer that has connected.</param>
        /// <param name="type">Event type.</param>
        /// <param name="parameterMap">Parameters passed with connection.</param>
#if !(NET_1_1)
        protected void FireConsumerConnectionEvent(IConsumer consumer, int type, Dictionary<string, object> parameterMap) 
#else
        protected void FireConsumerConnectionEvent(IConsumer consumer, int type, Hashtable parameterMap) 
#endif
        {
            // Create event object
            PipeConnectionEvent evt = new PipeConnectionEvent(this);
            // Fill it up
            evt.Consumer = consumer;
		    evt.Type = type;
		    evt.ParameterMap = parameterMap;
            // Fire it
            FirePipeConnectionEvent(evt);
	    }
        /// <summary>
        /// Broadcast provider connection event.
        /// </summary>
        /// <param name="provider">Provider that has connected.</param>
        /// <param name="type">Event type.</param>
        /// <param name="parameterMap">Parameters passed with connection.</param>
#if !(NET_1_1)
        protected void FireProviderConnectionEvent(IProvider provider, int type, Dictionary<string, object> parameterMap)
#else
        protected void FireProviderConnectionEvent(IProvider provider, int type, Hashtable parameterMap)
#endif
        {
		    PipeConnectionEvent evt = new PipeConnectionEvent(this);
		    evt.Provider = provider;
		    evt.Type = type;
		    evt.ParameterMap = parameterMap;
		    FirePipeConnectionEvent(evt);
	    }
        /// <summary>
        /// Fire any pipe connection event.
        /// </summary>
        /// <param name="evt">Pipe connection event.</param>
        protected void FirePipeConnectionEvent(PipeConnectionEvent evt)
        {
            foreach (IPipeConnectionListener element in _listeners)
            {
                try
                {
                    element.OnPipeConnectionEvent(evt);
                }
                catch (Exception ex)
                {
                    log.Error("Exception when handling pipe connection event", ex);
                }
            }
        }
    }
}
