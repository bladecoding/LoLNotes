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
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;

namespace FluorineFx.Messaging.Rtmp
{
    /// <summary>
    /// Indicates that returning the result of the invoked method will be delayed.
    /// </summary>
    [CLSCompliant(false)]
    public class DeferredResult
    {
        /// <summary>
        /// Weak reference to the used channel
        /// </summary>
        private WeakReference _channel;
        /// <summary>
        /// Pending call object
        /// </summary>
        private IPendingServiceCall _call;
        /// <summary>
        /// Invocation id
        /// </summary>
        private int _invokeId;
        /// <summary>
        /// Results sent flag
        /// </summary>
        private bool _resultSent = false;

        /// <summary>
        /// Sets the result.
        /// </summary>
        /// <value>The result.</value>
        public object Result
        {
            set
            {
                if (_resultSent)
                    throw new Exception("You can only set the result once.");
                _resultSent = true;
                if (_channel.IsAlive)
                {
                    RtmpChannel channel = _channel.Target as RtmpChannel;
                    Invoke reply = new Invoke();
                    _call.Result = value;
                    reply.ServiceCall = _call;
                    reply.InvokeId = _invokeId;
                    channel.Write(reply);
                    channel.Connection.UnregisterDeferredResult(this);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether result was sent.
        /// </summary>
        /// <value><c>true</c> if result was sent; otherwise, <c>false</c>.</value>
        public bool ResultSent
        {
            get { return _resultSent; }
        }

        /// <summary>
        /// Gets or sets the invoke id.
        /// </summary>
        /// <value>The invoke id.</value>
        public int InvokeId
        {
            get { return _invokeId; }
            set { _invokeId = value; }
        }

        /// <summary>
        /// Gets or sets the service call.
        /// </summary>
        /// <value>The service call.</value>
        public IPendingServiceCall ServiceCall
        {
            get { return _call; }
            set { _call = value; }
        }

        /// <summary>
        /// Sets the channel.
        /// </summary>
        /// <value>The channel.</value>
        public RtmpChannel Channel
        {
            set 
            {
                _channel = new WeakReference(value);
            }
        }
    }
}
