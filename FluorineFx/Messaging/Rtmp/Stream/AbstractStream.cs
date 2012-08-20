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
    /// Stream states.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Uninitialized state.
        /// </summary>
        UNINIT, 
        /// <summary>
        /// Closed state.
        /// </summary>
        CLOSED, 
        /// <summary>
        /// Stopped state.
        /// </summary>
        STOPPED, 
        /// <summary>
        /// Playing state.
        /// </summary>
        PLAYING, 
        /// <summary>
        /// Paused state.
        /// </summary>
        PAUSED
    }

    /// <summary>
    /// Abstract base implementation of IStream. Contains codec information, stream name, scope, event handling
    /// and provides stream start and stop operations.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AbstractStream : IStream
    {
        object _syncLock = new object();
        /// <summary>
        /// Stream name.
        /// </summary>
        protected string _name;
        /// <summary>
        /// Stream scope.
        /// </summary>
        protected IScope _scope;
        /// <summary>
        /// Stream audio and video codecs information.
        /// </summary>
        protected IStreamCodecInfo _codecInfo;
        /// <summary>
        /// Current state.
        /// </summary>
        protected State _state = State.UNINIT;
        /// <summary>
        /// Timestamp the stream was created.
        /// </summary>
        protected long _creationTime;

        internal State State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Gets or sets codec information.
        /// </summary>
        public IStreamCodecInfo CodecInfo
        {
            get { return _codecInfo; }
            set { _codecInfo = value; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        /// <summary>
        /// Gets the timestamp at which the stream was created.
        /// </summary>
        /// <value>The creation time.</value>
        public long CreationTime { get { return _creationTime; } }

        #region IStream Members

        /// <summary>
        /// Gets or sets stream name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Gets or sets stream scope.
        /// </summary>
        public IScope Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }
        /// <summary>
        /// Starts the stream.
        /// </summary>
        public virtual void Start()
        {
        }
        /// <summary>
        /// Stops the stream.
        /// </summary>
        public virtual void Stop()
        {
        }
        /// <summary>
        /// Closes the stream.
        /// </summary>
        public virtual void Close()
        {
        }

        #endregion

        /// <summary>
        /// Returns stream aware scope handler or null if scope is null.
        /// </summary>
        /// <returns>IStreamAwareScopeHandler implementation.</returns>
	    protected IStreamAwareScopeHandler GetStreamAwareHandler() 
        {
		    if (_scope != null) 
            {
			    IScopeHandler handler = _scope.Handler;
			    if (handler is IStreamAwareScopeHandler)
                    return handler as IStreamAwareScopeHandler;
		    }
		    return null;
	    }
    }
}
