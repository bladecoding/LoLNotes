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

using System.Collections.Generic;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Net
{
    /// <summary>
    /// The Fault class represents a fault in a remote procedure call (RPC) service invocation.
    /// </summary>
    public class Fault
    {
        readonly string _faultCode;
        readonly string _faultDetail;
        readonly string _faultString;
        readonly object _rootCause;
        readonly object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fault"/> class.
        /// </summary>
        /// <param name="faultCode">A simple code describing the fault.</param>
        /// <param name="faultDetail">Additional details describing the fault.</param>
        /// <param name="faultString">Text description of the fault.</param>
        /// <param name="rootCause">The cause of the fault.</param>
        /// <param name="content">The the raw content of the fault.</param>
        internal Fault(string faultCode, string faultDetail, string faultString, object rootCause, object content)
        {
            _faultCode = faultCode;
            _content = content;
            _rootCause = rootCause;
            _faultString = faultString;
            _faultDetail = faultDetail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fault"/> class.
        /// </summary>
        /// <param name="status">A status object.</param>
        internal Fault(object status)
        {
            _content = status;
            IDictionary<string, object> statusAso = status as IDictionary<string, object>;
            if (statusAso != null)
            {
                object faultCode;
                if (statusAso.TryGetValue("code", out faultCode))
                    _faultCode = (faultCode ?? string.Empty) as string;
                object faultDetail;
                if (statusAso.TryGetValue("details", out faultDetail))
                    _faultDetail = (faultDetail ?? string.Empty) as string;
                object faultString;
                if (statusAso.TryGetValue("description", out faultString))
                    _faultString = (faultString ?? string.Empty) as string;
                statusAso.TryGetValue("rootcause", out _rootCause);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fault"/> class.
        /// </summary>
        /// <param name="errorMessage">A Flex error message.</param>
        internal Fault(ErrorMessage errorMessage)
        {
            _content = errorMessage;
            _faultCode = errorMessage.faultCode;
            _faultDetail = errorMessage.faultDetail;
            _faultString = errorMessage.faultString;
            _rootCause = errorMessage.rootCause;
        }

        /// <summary>
        /// Gets the raw content of the fault (if available).
        /// </summary>
        /// <value>The raw content of the fault.</value>
        public object Content
        {
            get { return _content; }
        }

        /// <summary>
        /// Gets the root cause.
        /// </summary>
        /// <value>The cause of the fault. The value will be null if the cause is unknown or whether this fault represents the root itself.</value>
        public object RootCause
        {
            get { return _rootCause; }
        }

        /// <summary>
        /// Gets the fault string.
        /// </summary>
        /// <value>Text description of the fault.</value>
        public string FaultString
        {
            get { return _faultString; }
        }

        /// <summary>
        /// Gets the fault detail.
        /// </summary>
        /// <value>Any extra details of the fault.</value>
        public string FaultDetail
        {
            get { return _faultDetail; }
        }

        /// <summary>
        /// Gets the fault code.
        /// </summary>
        /// <value>A simple code describing the fault.</value>
        public string FaultCode
        {
            get { return _faultCode; }
        }
    }
}
