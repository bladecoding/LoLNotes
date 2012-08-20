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
using System.Net;
using System.Web;
using log4net;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Messages;
using FluorineFx.Threading;

namespace FluorineFx.Messaging.Rtmpt
{
    class PendingData
    {
        private object _buffer;
        private RtmpPacket _packet;

        public PendingData(object buffer, RtmpPacket packet)
        {
            _buffer = buffer;
            _packet = packet;
        }

        public PendingData(object buffer)
        {
            _buffer = buffer;
        }

        public object Buffer
        {
            get { return _buffer; }
        }

        public RtmpPacket Packet
        {
            get { return _packet; }
        }
    }

    class RtmptConnection : RtmpConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmptConnection));

        /// <summary>
        /// Try to generate responses that contain at least 32768 bytes data.
        /// Increasing this value results in better stream performance, but also increases the latency.
        /// </summary>
        internal static int RESPONSE_TARGET_SIZE = 32768;

        /// <summary>
        /// Start to increase the polling delay after this many empty results
        /// </summary>
        protected static long INCREASE_POLLING_DELAY_COUNT = 10;
        /// <summary>
        /// Polling delay to start with.
        /// </summary>
        protected static byte INITIAL_POLLING_DELAY = 0;
        /// <summary>
        /// Maximum polling delay.
        /// </summary>
        protected static byte MAX_POLLING_DELAY = 32;
        /// <summary>
        /// Polling delay value
        /// </summary>
        protected byte _pollingDelay = INITIAL_POLLING_DELAY;
        /// <summary>
        /// Timeframe without pending messages. If this time is greater then polling delay, then polling delay increased
        /// </summary>
        protected long _noPendingMessages;
        /// <summary>
        /// List of pending messages (PendingData)
        /// </summary>
        protected LinkedList _pendingMessages;
        /// <summary>
        /// Number of read bytes
        /// </summary>
        protected AtomicLong _readBytes;
        /// <summary>
        /// Number of written bytes
        /// </summary>
        protected AtomicLong _writtenBytes;

        protected ByteBuffer _buffer;

        IPEndPoint _remoteEndPoint;
        RtmptServer _rtmptServer;
        FastReaderWriterLock _lock;

        public RtmptConnection(RtmptServer rtmptServer, IPEndPoint ipEndPoint, string path, Hashtable parameters)
            : base(rtmptServer.RtmpHandler, RtmpMode.Server, path, parameters)
        {
            _lock = new FastReaderWriterLock();
            _remoteEndPoint = ipEndPoint;
            _rtmptServer = rtmptServer;
            _readBytes = new AtomicLong();
            _writtenBytes = new AtomicLong();
            _session = rtmptServer.Endpoint.GetMessageBroker().SessionManager.CreateSession(this);
        }
        /*
        public RtmptConnection(RtmptServer rtmptServer, IPEndPoint ipEndPoint, ISession session, string path, Hashtable parameters)
            : base(rtmptServer.RtmpHandler, path, parameters)
        {
            _lock = new FastReaderWriterLock();
            _remoteEndPoint = ipEndPoint;
            _rtmptServer = rtmptServer;
            _readBytes = new AtomicLong();
            _writtenBytes = new AtomicLong();
            _session = session;
        }
        */
        public override IPEndPoint RemoteEndPoint
        {
            get 
            {
                if( _remoteEndPoint != null )
                    return _remoteEndPoint;
                else
                {
                    if (HttpContext.Current != null)
                    {
                        IPAddress ipAddress = IPAddress.Parse(HttpContext.Current.Request.UserHostAddress);
                        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, 80);
                        return remoteEndPoint;
                    }
                }
                return null;
            }
        }

        public IEndpoint Endpoint { get { return _rtmptServer.Endpoint; } }

        public override long ReadBytes
        {
            get { return _readBytes.Value; }
        }

        public override long WrittenBytes
        {
            get { return _writtenBytes.Value; }
        }

        public byte PollingDelay
        {
            get
            {
                try
                {
                    _lock.AcquireReaderLock();
                    if (this.State == RtmpState.Disconnected)
                    {
                        // Special value to notify client about a closed connection.
                        return (byte)0;
                    }
                    return (byte)(_pollingDelay + 1);
                }
                finally
                {
                    _lock.ReleaseReaderLock();
                }
            }
        }

        public ByteBuffer GetPendingMessages(int targetSize)
        {
            ByteBuffer result = null;
            LinkedList toNotify = new LinkedList();
            try
            {
                _lock.AcquireWriterLock();
                if (_pendingMessages == null || _pendingMessages.Count == 0)
                {
                    _noPendingMessages += 1;
                    if (_noPendingMessages > INCREASE_POLLING_DELAY_COUNT)
                    {
                        if (_pollingDelay == 0)
                            _pollingDelay = 1;
                        _pollingDelay = (byte)(_pollingDelay * 2);
                        if (_pollingDelay > MAX_POLLING_DELAY)
                            _pollingDelay = MAX_POLLING_DELAY;
                    }
                    return null;
                }
                _noPendingMessages = 0;
                _pollingDelay = INITIAL_POLLING_DELAY;

                if (_pendingMessages.Count == 0)
                    return null;
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmpt_ReturningMessages, _pendingMessages.Count));
                result = ByteBuffer.Allocate(2048);
                while (_pendingMessages.Count > 0)
                {
                    PendingData pendingData = _pendingMessages[0] as PendingData;
                    _pendingMessages.RemoveAt(0);
                    if (pendingData.Buffer is ByteBuffer)
                        result.Put(pendingData.Buffer as ByteBuffer);
                    if (pendingData.Buffer is byte[])
                        result.Put(pendingData.Buffer as byte[]);
                    if (pendingData.Packet != null)
                        toNotify.Add(pendingData.Packet);

                    if ((result.Position > targetSize))
                        break;
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
            if (toNotify != null)
            {
                foreach (object message in toNotify)
                {
                    try
                    {
                        _handler.MessageSent(this, message);
                    }
                    catch (Exception ex)
                    {
                        log.Error(__Res.GetString(__Res.Rtmpt_NotifyError), ex);
                        continue;
                    }
                }
            }
            result.Flip();
            _writtenBytes.Increment(result.Limit);
            return result;
        }

        public override void Write(RtmpPacket packet)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            try
            {
                _lock.AcquireWriterLock();
                ByteBuffer data;
                try
                {
                    data = RtmpProtocolEncoder.Encode(this.Context, packet);
                }
                catch (Exception ex)
                {
                    log.Error("Could not encode message " + packet, ex);
                    return;
                }
                // Mark packet as being written
                WritingMessage(packet);
                if (_pendingMessages == null)
                    _pendingMessages = new LinkedList();
                _pendingMessages.Add(new PendingData(data, packet));
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public override void Write(ByteBuffer buffer)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            try
            {
                _lock.AcquireWriterLock();
                if (_pendingMessages == null)
                    _pendingMessages = new LinkedList();
                _pendingMessages.Add(new PendingData(buffer));
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public override void Write(byte[] buffer)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            try
            {
                _lock.AcquireWriterLock();
                if (_pendingMessages == null)
                    _pendingMessages = new LinkedList();
                _pendingMessages.Add(new PendingData(buffer));
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }            
        }

        public IList Decode(ByteBuffer data)
        {
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return Internal.EmptyIList;// Already shutting down.
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            _readBytes.Increment(data.Limit);
            if( _buffer == null )
                _buffer = ByteBuffer.Allocate(2048);
            _buffer.Put(data);
            _buffer.Flip();
            try
            {
                IList result = RtmpProtocolDecoder.DecodeBuffer(this.Context, _buffer);
                return result;
            }
            catch (HandshakeFailedException hfe)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("Handshake failed: {0}", hfe.Message));
#endif

                // Clear buffer if something is wrong in protocol decoding.
                _buffer.Clear();
                this.Close();
            }
            catch (Exception ex)
            {
                // Catch any exception in the decoding then clear the buffer to eliminate memory leaks when we can't parse protocol
                // Also close Connection because we can't parse data from it
#if !SILVERLIGHT
                log.Error("Error decoding buffer", ex);
#endif
                // Clear buffer if something is wrong in protocol decoding.
                _buffer.Clear();
                this.Close();
            }
            return null;
        }

        public override void Close()
        {
            // Defer actual closing so we can send back pending messages to the client.
            _lock.AcquireReaderLock();
            try
            {
                if (IsClosed || IsClosing)
                    return; // Already shutting down.
                try
                {
                    _lock.UpgradeToWriterLock();
                    SetIsClosing(true);
                    _lock.DowngradeToReaderLock();
                }
                catch (ApplicationException)
                {
                    //Some other thread did an upgrade
                    return;
                }
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public void RealClose()
        {
            _lock.AcquireReaderLock();
            try
            {
                if (!IsClosing)
                    return;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
            try
            {
                _lock.AcquireWriterLock();
                if (_buffer != null)
                {
                    _buffer.Dispose();
                    _buffer = null;
                }
                if (_pendingMessages != null)
                {
                    _pendingMessages.Clear();
                    _pendingMessages = null;
                }
                _rtmptServer.RemoveConnection(this.ConnectionId);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
            base.Close();
            _lock.AcquireWriterLock();
            try
            {
                SetIsClosed(true);
                SetIsClosing(false);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }


        public override void Push(IMessage message, IMessageClient messageClient)
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
            RtmpHandler.Push(this, message, messageClient);
            /*
            IMessage messageClone = message.Clone() as IMessage;
            messageClone.SetHeader(MessageBase.DestinationClientIdHeader, messageClient.ClientId);
            messageClone.clientId = messageClient.ClientId;
            messageClient.AddMessage(messageClone);
            */
        }

        protected override void OnInactive()
        {
            if( log.IsDebugEnabled )
                log.Debug(string.Format("Inactive connection {0}, closing", this.ConnectionId));
            //this.Timeout();
            Close();
            RealClose();
        }

        public override string ToString()
        {
            return "RtmptConnection " + _connectionId;
        }
    }
}
