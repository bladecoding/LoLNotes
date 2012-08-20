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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Rtmp;
//using FluorineFx.Messaging.Rtmpt;
//using FluorineFx.Messaging.Endpoints;
using FluorineFx.Util;
using FluorineFx.Context;
using FluorineFx.Configuration;
#if !SILVERLIGHT
using FluorineFx.Threading;
#endif

namespace FluorineFx.Net
{
    class SocketBufferPool
    {
        private static BufferPool _bufferPool;

        public static BufferPool Pool
        {
            get
            {
                if (_bufferPool == null)
                {
                    lock (typeof(SocketBufferPool))
                    {
                        if (_bufferPool == null)
                            _bufferPool = new BufferPool(4096/*FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize*/);
                    }
                }
                return _bufferPool;
            }
        }
    }
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class RtmpClientConnection : RtmpConnection
    {
#if !SILVERLIGHT
        private static ILog log = LogManager.GetLogger(typeof(RtmpClientConnection));
#endif
        readonly ByteBuffer _readBuffer;
        readonly RtmpNetworkStream _rtmpNetworkStream;
        DateTime _lastAction;
        private long _readBytes;
        private long _writtenBytes;

        public RtmpClientConnection(IRtmpHandler handler, Socket socket)
            : base(handler, RtmpMode.Client, null, null)
		{
#if FXCLIENT
            //TODO
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;
#else
            socket.ReceiveBufferSize = FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize;
            socket.SendBufferSize = FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.SendBufferSize;
#endif
            _handler = handler;
            _readBuffer = ByteBuffer.Allocate(4096);
            _readBuffer.Flip();
            _rtmpNetworkStream = new RtmpNetworkStream(socket);
            Context.SetMode(RtmpMode.Client);
		}

        public override bool IsConnected
        {
            get
            {
                return State == RtmpState.Connected;
            }
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

        public void BeginHandshake()
        {
            //Handshake 1st phase
            ByteBuffer buffer = ByteBuffer.Allocate(RtmpProtocolDecoder.HandshakeSize + 1);
            buffer.Put(0x03);
            /*
            buffer.Fill((byte)0x00, RtmpProtocolDecoder.HandshakeSize);
            buffer.Flip();
             */
            int tick = Environment.TickCount;
            buffer.Put(1, (byte)((tick >> 24) & 0xff));
            buffer.Put(2, (byte)((tick >> 16) & 0xff));
            buffer.Put(3, (byte)((tick >> 8) & 0xff));
            buffer.Put(4, (byte)(tick & 0xff));

            tick = tick % 256;
            for (int i = 8; i < 1536; i += 2)
            {
                tick = (0xB8CD75 * tick + 1) % 256;
                buffer.Put(i + 1, (byte)(tick & 0xff));
            }
            Write(buffer);
            BeginReceive(false);
        }

        #region Network IO
        public void BeginReceive(bool iocpThread)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketBeginReceive, _connectionId, iocpThread));
#endif
            if (!iocpThread)
                ThreadPool.QueueUserWorkItem(BeginReceiveCallbackProcessing, null);
            else
                BeginReceiveCallbackProcessing(null);
        }

        public void BeginReceiveCallbackProcessing(object state)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketReceiveProcessing, _connectionId));
#endif
            if (!IsClosed)
            {
                byte[] buffer = null;
                try
                {
                    buffer = SocketBufferPool.Pool.CheckOut();
                    _rtmpNetworkStream.BeginRead(buffer, 0, buffer.Length, BeginReadCallbackProcessing, buffer);
                }
                catch (Exception ex)
                {
                    SocketBufferPool.Pool.CheckIn(buffer);
                    HandleError(ex);
                }
            }
        }

        private void BeginReadCallbackProcessing(IAsyncResult ar)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketBeginRead, _connectionId));
#endif

            byte[] buffer = ar.AsyncState as byte[];
            if (!IsClosed)
            {
                try
                {
                    _lastAction = DateTime.Now;
                    int readBytes = _rtmpNetworkStream.EndRead(ar);
                    _readBytes += readBytes;
                    if (readBytes > 0)
                    {
                        _readBuffer.Append(buffer, 0, readBytes);
                        //Leave IOCP thread
#if !SILVERLIGHT
                        ThreadPoolEx.Global.QueueUserWorkItem(OnReceivedCallback, null);
#else
                        ThreadPool.QueueUserWorkItem(new WaitCallback(OnReceivedCallback), null);
#endif
                    }
                    else
                        // No data to read
                        Close();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
                finally
                {
                    SocketBufferPool.Pool.CheckIn(buffer);
                }
            }
            else
            {
                SocketBufferPool.Pool.CheckIn(buffer);
            }
        }

        private void OnReceivedCallback(object state)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketReadProcessing, _connectionId));

            if (log.IsDebugEnabled)
                log.Debug("Begin handling packet " + ToString());
#endif

            try
            {
                List<object> result;
                try
                {
                    result = RtmpProtocolDecoder.DecodeBuffer(Context, _readBuffer);
                }
                catch (HandshakeFailedException hfe)
                {
#if !SILVERLIGHT
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Handshake failed: {0}", hfe.Message));
#endif
                    // Clear buffer if something is wrong in protocol decoding.
                    _readBuffer.Clear();
                    Close();
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
                    Close();
                    return;
                }

                if (Context.State == RtmpState.Handshake)
                {
                    ByteBuffer resultBuffer = result[0] as ByteBuffer;
                    //Handshake 3d phase
                    if (resultBuffer != null)
                    {
                        resultBuffer.Skip(1);
                        resultBuffer.Compact();
                        resultBuffer.Limit = RtmpProtocolDecoder.HandshakeSize;
                    }
                    ByteBuffer buffer = ByteBuffer.Allocate(RtmpProtocolDecoder.HandshakeSize);
                    buffer.Put(resultBuffer);
                    Write(buffer);
                    Context.State = RtmpState.Connected;
                    _handler.ConnectionOpened(this);
                }
                else
                {
                    if (result != null && result.Count > 0)
                    {
                        foreach (object obj in result)
                        {
                            if (obj is ByteBuffer)
                            {
                                ByteBuffer buf = obj as ByteBuffer;
                                Write(buf);
                            }
                            else
                            {
#if !SILVERLIGHT
                                FluorineRtmpContext.Initialize(this);
#endif
                                _handler.MessageReceived(this, obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug("End handling packet " + ToString());
#endif
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
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_SocketConnectionReset, _connectionId));
#endif
                error = false;
            }

#if !SILVERLIGHT
            if (error && log.IsErrorEnabled)
                log.Error("Error " + ToString(), exception);
#endif
            BeginDisconnect();
        }

        internal void BeginDisconnect()
        {
            if (!IsClosed)
            {
                try
                {
                    //Leave IOCP thread
#if !SILVERLIGHT
                    ThreadPoolEx.Global.QueueUserWorkItem(OnDisconnectCallback, null);
#else
                    ThreadPool.QueueUserWorkItem(new WaitCallback(OnDisconnectCallback), null);
#endif
                }
                catch (Exception ex)
                {
#if !SILVERLIGHT
                    if (log.IsErrorEnabled)
                        log.Error("BeginDisconnect " + ToString(), ex);
#endif
                }
            }
        }

        private void OnDisconnectCallback(object state)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_SocketDisconnectProcessing, _connectionId));
#endif
            try
            {
                //_handler.ConnectionClosed(this);
                Close();
            }
            catch (Exception ex)
            {
#if !SILVERLIGHT
                if (log.IsErrorEnabled)
                    log.Error("OnDisconnectCallback " + ToString(), ex);
#endif
            }
            //Close(); -> IRtmpHandler
        }

        public override void Write(ByteBuffer buffer)
        {
            byte[] buf = buffer.ToArray();
            Write(buf);
        }

        public override void Write(byte[] buffer)
        {
            ReaderWriterLock.AcquireWriterLock();
            try
            {
                if (!IsClosed)
                {
                    try
                    {
                        _rtmpNetworkStream.Write(buffer, 0, buffer.Length);
                        _writtenBytes += buffer.Length;
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex);
                    }
                    _lastAction = DateTime.Now;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }


        #endregion Network IO

        public override void Close()
        {
            ReaderWriterLock.AcquireWriterLock();
            try
            {
                if (!IsClosing && !IsClosed)
                {
                    base.Close();
                    _handler.ConnectionClosed(this);
                    _rtmpNetworkStream.Close();
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }

        public override void Write(RtmpPacket packet)
        {
            if (!IsClosed)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug("Write " + packet.Header);
#endif
                //encode
                WritingMessage(packet);
                ByteBuffer outputStream = RtmpProtocolEncoder.Encode(Context, packet);
                Write(outputStream);
                _handler.MessageSent(this, packet);
            }
        }

        public override void Push(IMessage message, IMessageClient messageClient)
        {
            if (IsClosed)
            {
                BaseRtmpHandler.Push(this, message, messageClient);
            }
        }

        protected override void OnInactive()
        {
        }

        public override long WrittenBytes
        {
            get
            {
                return _writtenBytes;
            }
        }

        public override long ReadBytes
        {
            get
            {
                return _readBytes;
            }
        }

    }
}
