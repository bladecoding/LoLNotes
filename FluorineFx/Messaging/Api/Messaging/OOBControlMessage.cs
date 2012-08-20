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
    /// Out-of-band control message used by inter-components communication
    /// which are connected with pipes.
    /// Out-of-band data is a separate data stream used for specific purposes (in TCP it's referenced as "urgent data"), like lifecycle control.
    /// </summary>
    /// <remarks>
    /// 'Target' is used to represent the receiver who may be interested for receiving. It's a string of any form.
    /// </remarks>
    public class OOBControlMessage
    {
        /// <summary>
        /// Target.
        /// </summary>
        private string _target;
        /// <summary>
        /// Service name.
        /// </summary>
        private string _serviceName;
#if !(NET_1_1)
        /// <summary>
        /// Service parameters.
        /// </summary>
        private Dictionary<string, object> _serviceParameterMap;
#else
        /// <summary>
        /// Service parameters.
        /// </summary>
        private Hashtable _serviceParameterMap;
#endif
        /// <summary>
        /// Result.
        /// </summary>
        private object _result;
        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
#if !(NET_1_1)
        /// <summary>
        /// Gets or sets service parameters.
        /// </summary>
        public Dictionary<string, object> ServiceParameterMap
        {
            get 
            {
                if (_serviceParameterMap == null)
                    _serviceParameterMap = new Dictionary<string, object>();
                return _serviceParameterMap; 
            }
            set { _serviceParameterMap = value; }
        }
#else
        /// <summary>
        /// Gets or sets service parameters.
        /// </summary>
        public Hashtable ServiceParameterMap
        {
            get 
            { 
                if (_serviceParameterMap == null)
                    _serviceParameterMap = new Hashtable();
                return _serviceParameterMap; 
            }
            set { _serviceParameterMap = value; }
        }
#endif
        /// <summary>
        /// Gets or sets target.
        /// </summary>
        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }
        /// <summary>
        /// Gets or sets result.
        /// </summary>
        public object Result
        {
            get { return _result; }
            set { _result = value; }
        }
    }
}
