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

namespace FluorineFx.Messaging.Api.Messaging
{
    /// <summary>
    /// Event object corresponds to the connect/disconnect events among providers/consumers and pipes.
    /// </summary>
    [CLSCompliant(false)]
    public class PipeConnectionEvent
    {
        /// <summary>
        /// A provider connects as pull mode.
        /// </summary>
	    public const int PROVIDER_CONNECT_PULL = 0;
        /// <summary>
        /// A provider connects as push mode.
        /// </summary>
	    public const int PROVIDER_CONNECT_PUSH = 1;
        /// <summary>
        /// A provider disconnects.
        /// </summary>
	    public const int PROVIDER_DISCONNECT = 2;
        /// <summary>
        /// A consumer connects as pull mode.
        /// </summary>
	    public const int CONSUMER_CONNECT_PULL = 3;
        /// <summary>
        /// A consumer connects as push mode.
        /// </summary>
	    public const int CONSUMER_CONNECT_PUSH = 4;
        /// <summary>
        /// A consumer disconnects.
        /// </summary>
	    public const int CONSUMER_DISCONNECT = 5;

        /// <summary>
        /// Provider.
        /// </summary>
        private IProvider _provider;
        /// <summary>
        /// Gets or sets pipe connection provider.
        /// </summary>
        public IProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }
        /// <summary>
        /// Consumer.
        /// </summary>
        private IConsumer _consumer;
        /// <summary>
        /// Gets or sets pipe connection consumer.
        /// </summary>
        public IConsumer Consumer
        {
            get { return _consumer; }
            set { _consumer = value; }
        }
        /// <summary>
        /// Event type.
        /// </summary>
        private int _type;
        /// <summary>
        /// Gets or sets event type.
        /// </summary>
        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }
#if !(NET_1_1)
        /// <summary>
        /// Params map.
        /// </summary>
        Dictionary<string, object> _parameterMap;
        /// <summary>
        /// Gets or sets event parameters.
        /// </summary>
        public Dictionary<string, object> ParameterMap
        {
            get { return _parameterMap; }
            set { _parameterMap = value; }
        }
#else
        /// <summary>
        /// Params map.
        /// </summary>
        Hashtable _parameterMap;
        /// <summary>
        /// Gets or sets event parameters.
        /// </summary>
        public Hashtable ParameterMap
        {
            get { return _parameterMap; }
            set { _parameterMap = value; }
        }
#endif

        object _source;
        /// <summary>
        /// Gets or sets the vent source.
        /// </summary>
        public object Source
        {
            get { return _source; }
            set { _source = value; }
        }
        /// <summary>
        /// Construct an object with the specific pipe as the source.
        /// </summary>
        /// <param name="source">A pipe that triggers this event.</param>
        public PipeConnectionEvent(Object source)
        {
            _source = source;
        }

    }
}
