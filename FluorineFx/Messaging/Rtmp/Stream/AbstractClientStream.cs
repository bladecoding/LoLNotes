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
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Abstract base for client streams.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AbstractClientStream : AbstractStream, IClientStream
    {
        /// <summary>
        /// Stream identifier. Unique across server.
        /// </summary>
        private int _streamId;
        /// <summary>
        /// Connection that works with streams.
        /// </summary>
        private WeakReference _streamCapableConnection;
        /// <summary>
        /// Bandwidth configuration.
        /// </summary>
        private IBandwidthConfigure _bwConfig;
        /// <summary>
        /// Buffer duration in ms as requested by the client.
        /// </summary>
        private int _clientBufferDuration;

        /// <summary>
        /// Gets duration in ms as requested by the client.
        /// </summary>
        public int ClientBufferDuration 
        { 
            get { return _clientBufferDuration; } 
        }


        #region IClientStream Members

        /// <summary>
        /// Gets stream id.
        /// </summary>
        public int StreamId
        {
            get { return _streamId; }
            set { _streamId = value; }
        }

        /// <summary>
        /// Set the buffer duration for this stream as requested by the client.
        /// </summary>
        /// <param name="bufferTime">Duration in ms the client wants to buffer.</param>
        public void SetClientBufferDuration(int bufferTime)
        {
            _clientBufferDuration = bufferTime;
        }

        /// <summary>
        /// Gets connection associated with stream.
        /// </summary>
        public IStreamCapableConnection Connection
        {
            get 
            {
                if (_streamCapableConnection != null && _streamCapableConnection.IsAlive)
                    return _streamCapableConnection.Target as IStreamCapableConnection;
                return null;
            }
            set
            {
                _streamCapableConnection = new WeakReference(value);
            }
        }

        #endregion

        #region IBWControllable Members

        /// <summary>
        /// Gets the parent IBWControllable object.
        /// </summary>
        /// <returns></returns>
        public IBWControllable GetParentBWControllable()
        {
            return this.Connection;
        }
        /// <summary>
        /// Gets or sets stream bandwidth configuration.
        /// </summary>
        public virtual IBandwidthConfigure BandwidthConfiguration
        {
            get
            {
                return _bwConfig;
            }
            set
            {
                _bwConfig = value;
            }
        }

        #endregion
    }
}
