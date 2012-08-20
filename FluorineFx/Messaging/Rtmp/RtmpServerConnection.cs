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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmpt;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Util;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Collections;
using FluorineFx.Threading;
using FluorineFx.Scheduling;

namespace FluorineFx.Messaging.Rtmp
{
    /*
    class SocketBufferPool
    {
        private static BufferPool bufferPool;

        public static BufferPool Pool
        {
            get
            {
                if (bufferPool == null)
                {
                    lock (typeof(SocketBufferPool))
                    {
                        if (bufferPool == null)
                            bufferPool = new BufferPool(FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize);
                    }
                }
                return bufferPool;
            }
        }
    }
    */
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class RtmpServerConnection : RtmpConnection//, IStreamCapableConnection
    {
        private static ILog log = LogManager.GetLogger(typeof(RtmpServerConnection));

        RtmpServer _rtmpServer;
        ByteBuffer _readBuffer;
        RtmpNetworkStream _rtmpNetworkStream;
        DateTime _lastAction;
        //volatile RtmpConnectionState _state;
        IEndpoint _endpoint;

        /// <summary>
        /// Number of read bytes
        /// </summary>
        protected AtomicLong _readBytes;
        /// <summary>
        /// Number of written bytes
        /// </summary>
        protected AtomicLong _writtenBytes;

        FastReaderWriterLock _lock;

        // <summary>
        // Name of job that is waiting for a valid handshake.
        // </summary>
        //private string _waitForHandshakeJob;
        
        public RtmpServerConnection(RtmpServer rtmpServer, RtmpNetworkStream stream)
            : base(rtmpServer.RtmpHandler, RtmpMode.Server, null, null)
		{
            _lock = new FastReaderWriterLock();
            _endpoint = rtmpServer.Endpoint;
            _readBuffer = ByteBuffer.Allocate(4096);
            _readBuffer.Flip();

			// We start with an anonymous connection without a scope.
			// These parameters will be set during the call of "connect" later.
            _rtmpServer = rtmpServer;
            _rtmpNetworkStream = stream;
            //_state = RtmpConnectionState.Active;
            SetIsTunneled(false);
            IsTunnelingDetected = false;

            _readBytes = new AtomicLong();
            _writtenBytes = new AtomicLong();

            //Set the legacy collection flag from the endpoint channel settings
            this.Context.UseLegacyCollection = (_endpoint as RtmpEndpoint).IsLegacyCollection;
            this.Context.UseLegacyThrowable = (_endpoint as RtmpEndpoint).IsLegacyThrowable;
		}

        public bool IsTunneled
        {
            get { return (__fields & 32) == 32; }
        }

        internal void SetIsTunneled(bool value)
        {
            __fields = (value) ? (byte)(__fields | 32) : (byte)(__fields & ~32);
        }

        internal bool IsTunnelingDetected
        {
            get { return (__fields & 16) == 16; }
            set { __fields = (value) ? (byte)(__fields | 16) : (byte)(__fields & ~16); }
        }

        /// <summary>
        /// Gets whether the connection is being closed.
        /// </summary>
        public bool IsDisconnecting
        {
            get { return (__fields & 64) == 64; }
        }

        internal void SetIsDisconnecting(bool value)
        {
            __fields = (value) ? (byte)(__fields | 64) : (byte)(__fields & ~64);
        }

        public DateTime LastAction
        {
            get { return _lastAction; }
            set { _lastAction = value; }
        }

        public override IPEndPoint RemoteEndPoint
        {
            get { return _rtmpNetworkStream.Socket.RemoteEndPoint as IPEndPoint; }
        }

        public IEndpoint Endpoint { get { return _endpoint; } }

        internal BufferPool SocketBufferPool { get { return _rtmpServer.BufferPool; } }

        #region Network IO
        public void BeginReceive(bool IOCPThread)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketBeginReceive, _connectionId, IOCPThread));

            if (!IOCPThread)
                //ThreadPool.QueueUserWorkItem(new WaitCallback(BeginReceiveCallbackProcessing), null);
                ThreadPoolEx.Global.QueueUserWorkItem(new WaitCallback(BeginReceiveCallbackProcessing), null);
            else
                BeginReceiveCallbackProcessing(null);
        }

        public void BeginReceiveCallbackProcessing(object state)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketReceiveProcessing, _connectionId));

            byte[] buffer = null;
            try
            {
                buffer = SocketBufferPool.CheckOut();
                _rtmpNetworkStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(BeginReadCallbackProcessing), buffer);
            }
            catch (ObjectDisposedException)
            {
                //The underlying socket may be closed
            }
            catch (Exception ex)
            {
                SocketBufferPool.CheckIn(buffer);
                HandleError(ex);
            }
        }

        private void BeginReadCallbackProcessing(IAsyncResult ar)
        {
            byte[] buffer = ar.AsyncState as byte[];
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                {
                    SocketBufferPool.CheckIn(buffer);
                    return; // Already shutting down.
                }
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketBeginRead, _connectionId));

            try
            {
                _lastAction = DateTime.Now;
                int readBytes = _rtmpNetworkStream.EndRead(ar);
                _readBytes.Increment(readBytes);
                if (readBytes > 0)
                {
                    _readBuffer.Append(buffer, 0, readBytes);
                    while (_rtmpNetworkStream.DataAvailable)
                    {
                        readBytes = _rtmpNetworkStream.Read(buffer, 0, buffer.Length);
                        _readBuffer.Append(buffer, 0, readBytes);
                        _readBytes.Increment(readBytes);
                    }
                    //Leave IOCP thread
                    ThreadPoolEx.Global.QueueUserWorkItem(new WaitCallback(OnReceivedCallback), null);
                }
                else
                    // No data to read
                    //Close();
                    BeginDisconnect();
            }
            catch (ObjectDisposedException)
            {
                //The underlying socket may be closed
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                SocketBufferPool.CheckIn(buffer);
            }
        }

        private void OnReceivedCallback(object state)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketReadProcessing, _connectionId));
            if (!IsTunnelingDetected)
            {
                IsTunnelingDetected = true;
                byte rtmpDetect = _readBuffer.Get(0);
                bool encrypted = (rtmpDetect == 0x06); //rtmpe?
                if (!encrypted)
                    SetIsTunneled(rtmpDetect != 0x3);

                if (!IsTunneled)
                {
                    //For tunneled connections we do not really need a session for this connection
                    _session = _endpoint.GetMessageBroker().SessionManager.CreateSession(this);
                }
            }
            try
            {
                if (!IsTunneled)
                {
                    FluorineRtmpContext.Initialize(this);

#if !(NET_1_1)
                    List<object> result = null;
#else
                    ArrayList result = null;
#endif
                    try
                    {
                        result = RtmpProtocolDecoder.DecodeBuffer(this.Context, _readBuffer);
                    }
                    catch (HandshakeFailedException hfe)
                    {
#if !SILVERLIGHT
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("Handshake failed: {0}", hfe.Message));
#endif
                        // Clear buffer if something is wrong in protocol decoding.
                        _readBuffer.Clear();
                        this.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Catch any exception in the decoding then clear the buffer to eliminate memory leaks when we can't parse protocol
                        // Also close Connection because we can't parse data from it
#if !SILVERLIGHT
                        log.Error("Error decoding buffer", ex);
#endif
                        // Clear buffer if something is wrong in protocol decoding.
                        _readBuffer.Clear();
                        this.Close();
                        return;
                    }

                    if (result != null && result.Count > 0)
                    {
                        foreach (object obj in result)
                        {
                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Rtmp_BeginHandlePacket, _connectionId));
                            if (obj is ByteBuffer)
                            {
                                ByteBuffer buf = obj as ByteBuffer;
                                Write(buf);
                            }
                            else if (obj is byte[])
                            {
                                Write(obj as byte[]);
                            }
                            else
                            {
                                _rtmpServer.RtmpHandler.MessageReceived(this, obj);
                            }
                            if (log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Rtmp_EndHandlePacket, _connectionId));
                        }
                    }
                }
                else
                {
                    //FluorineRtmpContext.Initialize(this);
                    RtmptRequest rtmptRequest = RtmptProtocolDecoder.DecodeBuffer(this, _readBuffer);
                    if( rtmptRequest != null )
                        HandleRtmpt(rtmptRequest);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            //Ready to receive again
            BeginReceive(false);
        }

        private void HandleError(Exception exception)
        {
            SocketException socketException = exception as SocketException;
            if (exception.InnerException != null && exception.InnerException is SocketException)
                socketException = exception.InnerException as SocketException;

            bool error = true;
            if (socketException != null && socketException.ErrorCode == 10054)//WSAECONNRESET
            {
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionReset, _connectionId, socketException.ErrorCode));
                error = false;
            }
            if (socketException != null && socketException.ErrorCode == 10053)//WSAECONNABORTED
            {
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionAborted, _connectionId, socketException.ErrorCode));
                error = false;
            }
            if (socketException != null && socketException.ErrorCode == 995)
            {
                //The I/O operation has been aborted because of either a thread exit or an application request
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionAborted, _connectionId, socketException.ErrorCode));
                error = false;
            }
            //Both of these error codes indicate that an actual network timeout (WSAETIMEDOUT) or network round-trip (WSAECONNREFUSED) has taken place
            if (socketException != null && socketException.ErrorCode == 10060)//WSAETIMEDOUT
            {
                //WSAETIMEDOUT / 10060 - This happens when trying to connect to a valid address that doesn't respond (e.g., a powered-off server or intermediate router). This may also be caused by a firewall on the remote side.
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionTimeout, _connectionId, socketException.ErrorCode));
                error = false;
            }
            if (socketException != null && socketException.ErrorCode == 10061)//WSAECONNREFUSED
            {
                //WSAECONNREFUSED / 10061 - Indicates that the connection request got to a valid address that is powered on, but there is no program listening on that port. 
                //Usually, this is an indication that the server software is not running, though the computer is on. 
                //This may also be caused by a firewall, though most firewalls drop the packet (causing WSAETIMEDOUT) instead of actively refusing the connection (causing WSAECONNREFUSED).
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionTimeout, _connectionId, socketException.ErrorCode));
                error = false;
            }

            if (error && log.IsErrorEnabled)
            {
                if (socketException != null)
                    log.Error(string.Format("{0} socket exception, error code {1}", this.ConnectionId.ToString(), socketException.ErrorCode), exception);
                else
                    log.Error(this.ConnectionId.ToString(), exception);
            }
            BeginDisconnect();
        }

        internal void BeginDisconnect()
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
                SetIsDisconnecting(true);
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_BeginDisconnect, _connectionId));
            try
            {
                //Leave IOCP thread
                //_state = RtmpConnectionState.Disconnectig;
                ThreadPoolEx.Global.QueueUserWorkItem(new WaitCallback(OnDisconnectCallback), null);
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error(this.ConnectionId.ToString(), ex);
            }
        }

        private void OnDisconnectCallback(object state)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            log4net.ThreadContext.Properties["ClientIP"] = this.RemoteEndPoint;
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketDisconnectProcessing, _connectionId));
            try
            {
                FluorineRtmpContext.Initialize(this);
                _rtmpServer.RtmpHandler.ConnectionClosed(this);
                //BaseRtmpHandler -> Close();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error(this.ConnectionId.ToString(), ex);
            }
        }

        public override void Write(ByteBuffer buffer)
        {
            byte[] buf = buffer.ToArray();
            Write(buf);
        }

        public override void Write(byte[] buffer)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketSend, _connectionId));
            try
            {
                //No need to lock, RtmpNetworkStream will handle Write locking
                _rtmpNetworkStream.Write(buffer, 0, buffer.Length);
                _writtenBytes.Increment(buffer.Length);
            }
            catch (ObjectDisposedException)
            {
                //After EndWrite, the underlying socket may be closed
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            _lastAction = DateTime.Now;
        }

        #endregion Network IO

        public override void Close()
        {
            _lock.AcquireWriterLock();
            try
            {
                if (IsClosed || IsClosing)
                    return; // Already shutting down.
                SetIsClosing(true);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
            FluorineRtmpContext.Initialize(this);
            base.Close();
            _rtmpServer.OnConnectionClose(this);
            _rtmpNetworkStream.Close();
            _lock.AcquireWriterLock();
            try
            {
                SetIsClosed(true);
                SetIsClosing(false);
                SetIsDisconnecting(false);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

		public override void Write(RtmpPacket packet)
		{
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_WritePacket, _connectionId, packet));

            if (!this.IsTunneled)
            {
                //encode
                WritingMessage(packet);
                ByteBuffer outputStream = null;
                _lock.AcquireWriterLock();
                try
                {
                    outputStream = RtmpProtocolEncoder.Encode(this.Context, packet);
                }
                finally
                {
                    _lock.ReleaseWriterLock();
                }
                Write(outputStream);
                _rtmpServer.RtmpHandler.MessageSent(this, packet);
            }
            else
            {
                //We should never get here
                System.Diagnostics.Debug.Assert(false);
            }
		}

        public override void Push(IMessage message, IMessageClient messageClient)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing || IsDisconnecting)
                    return; // Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            RtmpHandler.Push(this, message, messageClient);
        }

        protected override void OnInactive()
        {
            if (!this.IsTunneled)
            {
                this.Timeout();
                this.Close();
            }
        }

        /// <summary>
        /// Gets the total number of bytes read from the connection.
        /// </summary>
        public override long WrittenBytes
        {
            get{ return _writtenBytes.Value; }
        }
        /// <summary>
        /// Gets the total number of bytes written to the connection.
        /// </summary>
        public override long ReadBytes
        {
            get{ return _readBytes.Value; }
        }

        /// <summary>
        /// Start waiting for a valid handshake.
        /// </summary>
        internal override void StartWaitForHandshake()
        {
            _lock.AcquireWriterLock();
            try
            {
                if (FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.MaxHandshakeTimeout > 0)
                {
                    ISchedulingService schedulingService = _endpoint.GetMessageBroker().GlobalScope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                    _waitForHandshakeJob = schedulingService.AddScheduledOnceJob(FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.MaxHandshakeTimeout, new WaitForHandshakeJob(this));
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }            
        }

        #region RTMPT Handling

        private void HandleRtmpt(RtmptRequest rtmptRequest)
        {
            IEndpoint endpoint = this.Endpoint.GetMessageBroker().GetEndpoint(RtmptEndpoint.FluorineRtmptEndpointId);
            RtmptEndpoint rtmptEndpoint = endpoint as RtmptEndpoint;
            if (rtmptEndpoint != null)
            {
                rtmptEndpoint.Service(rtmptRequest);
            }
        }

        #endregion RTMPT Handling

    }
}
