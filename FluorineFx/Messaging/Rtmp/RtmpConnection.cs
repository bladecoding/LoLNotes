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
using System.Collections.Generic;
using FluorineFx.Collections.Generic;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Util;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;
#if !FXCLIENT
using FluorineFx.Scheduling;
#endif

namespace FluorineFx.Messaging.Rtmp
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    enum RtmpConnectionState
    {
        Inactive,
        Active,
        Disconnectig
    }

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public abstract class RtmpConnection : BaseConnection, IServiceCapableConnection, IStreamCapableConnection
	{
#if !SILVERLIGHT
        private static ILog log = LogManager.GetLogger(typeof(RtmpConnection));
#endif
        readonly RtmpContext _context;

        private readonly BitArray _reservedStreams;

        /// <summary>
        /// Name of job that keeps connection alive.
        /// </summary>
        protected string _keepAliveJobName;
        /// <summary>
        /// Name of job that is waiting for a valid handshake.
        /// </summary>
        protected string _waitForHandshakeJob;

        /// <summary>
        /// Connection channels.
        /// Integer, Channel
        /// </summary>
        readonly CopyOnWriteDictionary<int, RtmpChannel> _channels;
        /// <summary>
        /// Client streams.
        /// Map(Integer, IClientStream)
        /// </summary>
        readonly CopyOnWriteDictionary<int, IClientStream> _streams;
        /// <summary>
        /// Map for pending video packets and stream IDs
        /// Map(Integer, AtomicInteger)
        /// </summary>
        readonly CopyOnWriteDictionary<int, AtomicInteger> _pendingVideos;
        /// <summary>
        /// Remembers stream buffer durations
        /// Map(Integer, Integer)
        /// </summary>
        readonly CopyOnWriteDictionary<int, int> _streamBuffers;
        /// <summary>
        /// Stores pending calls and ids as pairs.
        /// </summary>
        readonly CopyOnWriteDictionary<int, IServiceCall> _pendingCalls;
        /// <summary>
        /// Deferred results set.
        /// </summary>
        protected Dictionary<DeferredResult, object> _deferredResults = new Dictionary<DeferredResult, object>();

        /// <summary>
        /// Identifier for remote calls.
        /// </summary>
        readonly AtomicInteger _invokeId;

        /// <summary>
        /// Timestamp when last ping command was sent.
        /// </summary>
        protected AtomicInteger _lastPingSent = new AtomicInteger(0);
        /// <summary>
        /// Timestamp when last ping result was received.
        /// </summary>
        protected AtomicInteger _lastPongReceived = new AtomicInteger(0);
        /// <summary>
        /// Last ping timestamp.
        /// </summary>
        protected AtomicInteger _lastPingTime = new AtomicInteger(-1);
        /// <summary>
        /// Number of bytes the client reported to have received.
        /// </summary>
        private long _clientBytesRead;
        /// <summary>
        /// Data read interval
        /// </summary>
        protected int _bytesReadInterval = 120 * 1024;
        /// <summary>
        /// Number of bytes to read next.
        /// </summary>
        protected int _nextBytesRead = 120 * 1024;

        /// <summary>
        /// Rtmp handler.
        /// </summary>
        protected IRtmpHandler _handler;

        /// <summary>
        /// Bandwidth configure.
        /// </summary>
        private IConnectionBWConfig _bwConfig;
        /// <summary>
        /// Bandwidth context used by bandwidth controller.
        /// </summary>
        private IBWControlContext _bwContext;

        /// <summary>
        /// Number of streams used.
        /// </summary>
        private readonly AtomicInteger _streamCount;

        internal RtmpConnection(IRtmpHandler handler, RtmpMode mode, string path, IDictionary parameters)
            : base(path, parameters)
		{
            _handler = handler;
            _channels = new CopyOnWriteDictionary<int,RtmpChannel>(4);
            _streams = new CopyOnWriteDictionary<int,IClientStream>();
            _pendingVideos = new CopyOnWriteDictionary<int,AtomicInteger>();
            _streamCount = new AtomicInteger();
            _streamBuffers = new CopyOnWriteDictionary<int,int>();
            _reservedStreams = new BitArray(0);
            _pendingCalls = new CopyOnWriteDictionary<int, IServiceCall>();
			// We start with an anonymous connection without a scope.
			// These parameters will be set during the call of "connect" later.
            _context = new RtmpContext(mode);
            //Transaction id depends on server/client mode
            //When server mode is set we cannot push messages with transaction id = 1 (connect)
            _invokeId = mode == RtmpMode.Server ? new AtomicInteger(1) : new AtomicInteger(0);
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="newScope"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
	    public override bool Connect(IScope newScope, object[] parameters) 
        {
            try
            {
                bool success = base.Connect(newScope, parameters);
                if (success)
                {
                    try
                    {
                        ReaderWriterLock.AcquireWriterLock();
                        // XXX Bandwidth control service should not be bound to
                        // a specific scope because it's designed to control
                        // the bandwidth system-wide.
                        if (Scope != null && Scope.Context != null)
                        {
                            IBWControlService bwController = Scope.GetService(typeof(IBWControlService)) as IBWControlService;
                            _bwContext = bwController.RegisterBWControllable(this);
                        }
                        UnscheduleWaitForHandshakeJob();
                    }
                    finally
                    {
                        ReaderWriterLock.ReleaseWriterLock();
                    }
                }
                return success;
            }
            catch (ClientRejectedException)
            {
                UnscheduleWaitForHandshakeJob();
                throw;
            }
        }

        private void UnscheduleWaitForHandshakeJob()
        {
#if !FXCLIENT
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                if (_waitForHandshakeJob != null)
                {
                    ISchedulingService service = this.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                    service.RemoveScheduledJob(_waitForHandshakeJob);
                    _waitForHandshakeJob = null;
                    if( log.IsDebugEnabled )
                        log.Debug(string.Format("{0} Removed WaitForHandshakeJob", this.ConnectionId));
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
#endif
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override void Timeout()
        {
            if (!IsClosed)
            {
                if (this.IsFlexClient)
                {
                    FlexInvoke flexInvoke = new FlexInvoke();
                    StatusASO statusASO = new StatusASO(StatusASO.NC_CONNECT_CLOSED, StatusASO.STATUS, "Connection Timed Out", null, this.ObjectEncoding);
                    Call call = new Call("onstatus", new object[] { statusASO });
                    flexInvoke.ServiceCall = call;
                    //flexInvoke.Cmd = "onstatus";
                    //flexInvoke.Parameters = new object[] { statusASO };
                    RtmpChannel channel = this.GetChannel(3);
                    channel.Write(flexInvoke);
                }
                else
                {
                    StatusASO statusASO = new StatusASO(StatusASO.NC_CONNECT_CLOSED, StatusASO.ERROR, "Connection Timed Out", null, this.ObjectEncoding);
                    RtmpChannel channel = this.GetChannel(3);
                    channel.SendStatus(statusASO);
                }
            }
        }

        /// <summary>
        /// Closes the connection. This will disconnect the client from the associated scope.
        /// </summary>
        public override void Close()
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(__Res.GetString(__Res.Rtmp_ConnectionClose, _connectionId));
#endif

#if !FXCLIENT
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                if (_keepAliveJobName != null)
                {
                    ISchedulingService service = this.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                    service.RemoveScheduledJob(_keepAliveJobName);
                    _keepAliveJobName = null;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }

            IStreamService streamService = ScopeUtils.GetScopeService(this.Scope, typeof(IStreamService)) as IStreamService;
            if (streamService != null)
            {
                foreach (IClientStream stream in _streams.Values)
                {
                    if (stream != null)
                    {
#if !SILVERLIGHT
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("{0} Closing stream: {1}", _connectionId, stream.StreamId));
#endif
                        streamService.deleteStream(this, stream.StreamId);
                        _streamCount.Decrement();
                    }
                }
                _streams.Clear();
            }
#endif
            _channels.Clear();
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                if (_bwContext != null && this.Scope != null && this.Scope.Context != null)
                {
                    IBWControlService bwController = this.Scope.GetService(typeof(IBWControlService)) as IBWControlService;
                    bwController.UnregisterBWControllable(_bwContext);
                    _bwContext = null;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
            base.Close();
            _context.State = RtmpState.Disconnected;
        }


        /// <summary>
        /// Gets the RTMP state.
        /// </summary>
        /// <value>The RTMP state.</value>
        public RtmpState State
        {
            get { return _context.State; }
        }

		internal void Setup(string host, string path, IDictionary parameters)
		{
			_path = path;
			_parameters = parameters;
			if( _parameters.Contains("objectEncoding") )
			{
				int objectEncoding = System.Convert.ToInt32( _parameters["objectEncoding"] );
				_objectEncoding = (ObjectEncoding)objectEncoding;
			}
		}


        /// <summary>
        /// Gets the RTMP context.
        /// </summary>
        /// <value>The RTMP context.</value>
		public RtmpContext Context
		{
			get{ return _context; }
		}
        /// <summary>
        /// Returns channel by id.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <returns>Channel object by id.</returns>
		public RtmpChannel GetChannel(int channelId) 
		{
            RtmpChannel channel;
            if (!_channels.TryGetValue(channelId, out channel))
            {
                channel = new RtmpChannel(this, channelId);
                _channels[channelId] = channel;
            }
            return channel;
		}
        /// <summary>
        /// Closes channel.
        /// </summary>
        /// <param name="channelId">Channel id</param>
		public void CloseChannel(int channelId) 
		{
            _channels[channelId] = null;
		}
        /// <summary>
        /// Gets identifier for remote calls.
        /// </summary>
		public int InvokeId
		{ 
			get{ return _invokeId.Increment(); } 
		}
        /// <summary>
        /// Returns a stream id for given channel id.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Id of stream that channel belongs to.</returns>
		public int GetStreamIdForChannel(int channelId) 
		{
			if (channelId < 4) 
				return 0;
			return ((channelId - 4) / 5) + 1;
		}
        /// <summary>
        /// Gets pending call service by id.
        /// </summary>
        /// <param name="invokeId">Pending call service id.</param>
        /// <returns>Pending call service object.</returns>
		public IPendingServiceCall GetPendingCall(int invokeId)
		{
            IServiceCall pendingCall;
            _pendingCalls.TryGetValue(invokeId, out pendingCall);
            return pendingCall as IPendingServiceCall;
		}
        /// <summary>
        /// Retrieve pending call service by id. The call will be removed afterwards.
        /// </summary>
        /// <param name="invokeId">Pending call service id.</param>
        /// <returns>Pending call service object.</returns>
        public IPendingServiceCall RetrievePendingCall(int invokeId)
        {
            return _pendingCalls.RemoveAndGet(invokeId) as IPendingServiceCall;
        }
        /// <summary>
        /// Register pending call (remote function call that is yet to finish).
        /// </summary>
        /// <param name="invokeId">Deferred operation id.</param>
        /// <param name="call">Call service.</param>
        internal void RegisterPendingCall(int invokeId, IPendingServiceCall call)
        {
            _pendingCalls[invokeId] = call;
        }
        /// <summary>
        /// Write a RTMP packet.
        /// </summary>
        /// <param name="packet">The RTMP packet.</param>
		public abstract void Write(RtmpPacket packet);

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public abstract void Write(ByteBuffer buffer);

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public abstract void Write(byte[] buffer);


        #region IConnection Members

        /// <summary>
        /// Gets the total number of messages that are pending to be sent to the connection.
        /// </summary>
        public override long PendingMessages { get { return _pendingCalls.Count; } }

        /// <summary>
        /// Mark message as being written.
        /// </summary>
        /// <param name="packet">The RTMP packet</param>
        protected virtual void WritingMessage(RtmpPacket packet)
        {
            if (packet.Message is VideoData)
            {
                int streamId = packet.Header.StreamId;
                AtomicInteger value = new AtomicInteger();
                AtomicInteger old = _pendingVideos.AddIfAbsent(streamId, value) as AtomicInteger;
                if (old == null)
                    old = value;
                old.Increment();
            }
        }

        /// <summary>
        /// Start measuring the roundtrip time for a packet on the connection.
        /// </summary>
        public override void Ping()
        {
            int newPingTime = Environment.TickCount;
#if !SILVERLIGHT
            if( log.IsDebugEnabled )
                log.Debug(string.Format("{0} Pinging connection at {1}, last ping sent at {2}", _connectionId, newPingTime, _lastPingSent.Value));
#endif
            if(_lastPingSent.Value == 0)
                _lastPongReceived.Value = newPingTime;
            Ping pingRequest = new Ping();
            pingRequest.PingType = Event.Ping.PingClient;
            _lastPingSent.Value = newPingTime;
            int now = (int)(newPingTime & 0xffffffff);
            pingRequest.Value2 = now;
            pingRequest.Value3 = Event.Ping.Undefined;
            Ping(pingRequest);
        }

        #endregion

        #region IStreamCapableConnection Members

        /// <summary>
        /// Total number of video messages that are pending to be sent to a stream.
        /// </summary>
        /// <param name="streamId">Stream id.</param>
        /// <returns>Number of pending video messages.</returns>
        public override long GetPendingVideoMessages(int streamId)
        {
            AtomicInteger count;
            _pendingVideos.TryGetValue(streamId, out count);
            long result = count != null ? count.Value - this.StreamCount : 0;
            return result > 0 ? result : 0;
        }
        /// <summary>
        /// Get a stream by its id.
        /// </summary>
        /// <param name="id">Stream id.</param>
        /// <returns>Stream with given id.</returns>
        public IClientStream GetStreamById(int id)
        {
            if (id <= 0)
                return null;
            IClientStream stream;
            _streams.TryGetValue(id - 1, out stream);
            return stream;
        }
        /// <summary>
        /// Returns a reserved stream id for use.
        /// According to FCS/FMS regulation, the base is 1.
        /// </summary>
        /// <returns>Reserved stream id.</returns>
        public int ReserveStreamId()
        {
		    int result = -1;
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                for (int i = 0; i < _reservedStreams.Count; i++)
                {
                    if (!_reservedStreams[i])
                    {
                        _reservedStreams[i] = true;
                        result = i;
                        break;
                    }
                }
                if (result == -1)
                {
                    _reservedStreams.Length += 1;
                    result = _reservedStreams.Length - 1;
                    _reservedStreams[result] = true;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
		    return result + 1;
        }
        /// <summary>
        /// Unreserve this id for future use.
        /// </summary>
        /// <param name="streamId">ID of stream to unreserve.</param>
        public void UnreserveStreamId(int streamId)
        {
            ReaderWriterLock.AcquireWriterLock();
            try
            {
                DeleteStreamById(streamId);
                if (streamId > 0)
                    _reservedStreams[streamId - 1] = false;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Deletes the stream with the given id.
        /// </summary>
        /// <param name="streamId">Id of stream to delete.</param>
        public void DeleteStreamById(int streamId)
        {
            if (streamId > 0)
            {
                if (_streams.ContainsKey(streamId - 1))
                {
                    _pendingVideos.Remove(streamId);
                    _streamCount.Decrement();
                    _streams.Remove(streamId - 1);
                    _streamBuffers.Remove(streamId - 1);
                }
            }
        }
        /// <summary>
        /// Creates a stream that can play only one item.
        /// </summary>
        /// <param name="streamId">Stream id.</param>
        /// <returns>New subscriber stream that can play only one item.</returns>
        public ISingleItemSubscriberStream NewSingleItemSubscriberStream(int streamId)
        {
            return null;
        }
        /// <summary>
        /// Creates a stream that can play a list.
        /// </summary>
        /// <param name="streamId">Stream id.</param>
        /// <returns>New stream that can play sequence of items.</returns>
        public IPlaylistSubscriberStream NewPlaylistSubscriberStream(int streamId)
        {
#if !FXCLIENT
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_reservedStreams.Length <= streamId - 1 || !_reservedStreams[streamId - 1])
                {
                    // StreamId has not been reserved before
                    return null;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }

            IClientStream stream;
            if (_streams.TryGetValue(streamId - 1, out stream) && stream != null)
            {
                // Another stream already exists with this id
                return null;
            }
            //TODO
            PlaylistSubscriberStream pss = new PlaylistSubscriberStream();
            int buffer;
            if( _streamBuffers.TryGetValue(streamId - 1, out buffer) )
                pss.SetClientBufferDuration((int)buffer);
            pss.Name = CreateStreamName();
            pss.Connection = this;
            pss.Scope = this.Scope;
            pss.StreamId = streamId;
            RegisterStream(pss);
            _streamCount.Increment();
            return pss;
#else
            return null;
#endif
        }
        /// <summary>
        /// Generates new stream name.
        /// </summary>
        /// <returns>New stream name.</returns>
        protected string CreateStreamName()
        {
            return Guid.NewGuid().ToString();
        }
        /// <summary>
        /// Creates a broadcast stream.
        /// </summary>
        /// <param name="streamId">Stream id.</param>
        /// <returns>New broadcast stream.</returns>
        public IClientBroadcastStream NewBroadcastStream(int streamId)
        {
#if !FXCLIENT
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_reservedStreams.Length <= streamId - 1 || !_reservedStreams[streamId - 1])
                {
                    // StreamId has not been reserved before
                    return null;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }
            IClientStream stream;
            if (_streams.TryGetValue(streamId - 1, out stream) && stream != null)
            {
                // Another stream already exists with this id
                return null;
            }
            //TODO
            ClientBroadcastStream cbs = new ClientBroadcastStream();
            int buffer;
            if( _streamBuffers.TryGetValue(streamId - 1, out buffer))
                cbs.SetClientBufferDuration((int)buffer);
            cbs.StreamId = streamId;
            cbs.Connection = this;
            cbs.Name = CreateStreamName();
            cbs.Scope = this.Scope;

            RegisterStream(cbs);
            _streamCount.Increment();
            return cbs;
#else
            return null;
#endif
        }
        /// <summary>
        /// Store a stream in the connection.
        /// </summary>
        /// <param name="stream"></param>
        protected void RegisterStream(IClientStream stream)
        {
            _streams[stream.StreamId - 1] = stream;
        }
        /// <summary>
        /// Remove a stream from the connection.
        /// </summary>
        /// <param name="stream"></param>
        private void UnregisterStream(IClientStream stream)
        {
            _streams.Remove(stream.StreamId);
        }
        /// <summary>
        /// Removes the client stream.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        public void RemoveClientStream(int streamId)
        {
            UnreserveStreamId(streamId);
        }
        /// <summary>
        /// Return stream by given channel id.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public IClientStream GetStreamByChannelId(int channelId)
        {
            if (channelId < 4)
                return null;
            IClientStream stream;
            _streams.TryGetValue(GetStreamIdForChannel(channelId) - 1, out stream);
            return stream;
        }
        /// <summary>
        /// Gets the channel for stream id.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <returns>The channel for this stream id.</returns>
        public int GetChannelForStreamId(int streamId)
        {
            return (streamId - 1) * 5 + 4;
        }
        /// <summary>
        /// Gets the stream count.
        /// </summary>
        /// <value>The stream count.</value>
        protected int StreamCount
        {
            get { return _streamCount.Value; }
        }

        internal void RememberStreamBufferDuration(int streamId, int bufferDuration)
        {
            _streamBuffers.Add(streamId - 1, bufferDuration);
        }

        /// <summary>
        /// Gets collection of IClientStream.
        /// </summary>
        /// <returns></returns>
        public ICollection GetStreams()
        {
            return _streams.Values as ICollection;
        }
        /// <summary>
        /// Adds the client stream.
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void AddClientStream(IClientStream stream)
        {
            int streamId = stream.StreamId;
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                if (_reservedStreams.Length > streamId - 1 && _reservedStreams[streamId - 1])
                    return;//Already reserved
                if (_reservedStreams.Length <= streamId - 1)
                    _reservedStreams.Length = streamId;
                _reservedStreams[streamId - 1] = true;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
            _streams[streamId - 1] = stream;
            _streamCount.Increment();
        }
        /// <summary>
        /// Creates output stream object from stream id. Output stream consists of audio, data and video channels.
        /// </summary>
        /// <param name="streamId">Stream id.</param>
        /// <returns>Output stream object.</returns>
        public OutputStream CreateOutputStream(int streamId)
        {
            int channelId = (4 + ((streamId - 1) * 5));
            RtmpChannel data = GetChannel(channelId++);
            RtmpChannel video = GetChannel(channelId++);
            RtmpChannel audio = GetChannel(channelId++);
            return new OutputStream(video, audio, data);
        }

        #endregion

        #region IBWControllable Members

        /// <summary>
        /// Returns parent IBWControllable object.
        /// </summary>
        /// <returns>Parent IBWControllable.</returns>
        public IBWControllable GetParentBWControllable()
        {
            // TODO return the client object
            return null;
        }
        /// <summary>
        /// Gets or sets bandwidth configuration object.
        /// Bandwidth configuration allows you to set bandwidth size for audio, video and total amount.
        /// </summary>
        public IBandwidthConfigure BandwidthConfiguration
        {
            get
            {
                try
                {
                    ReaderWriterLock.AcquireReaderLock();
                    return _bwConfig;
                }
                finally
                {
                    ReaderWriterLock.ReleaseReaderLock();
                }
            }
            set
            {
                if (!(value is IConnectionBWConfig))
                    return;

                IConnectionBWConfig connectionBWConfig = value as IConnectionBWConfig;
                // Notify client about new bandwidth settings (in bytes per second)
                if (connectionBWConfig.DownstreamBandwidth > 0)
                {
                    ServerBW serverBW = new ServerBW((int)_bwConfig.DownstreamBandwidth / 8);
                    GetChannel((byte)2).Write(serverBW);
                }
                if (connectionBWConfig.UpstreamBandwidth > 0)
                {
                    ClientBW clientBW = new ClientBW((int)_bwConfig.UpstreamBandwidth / 8, (byte)0);
                    GetChannel((byte)2).Write(clientBW);
                    // Update generation of BytesRead messages
                    // TODO: what are the correct values here?
                    _bytesReadInterval = (int)_bwConfig.UpstreamBandwidth / 8;
                    _nextBytesRead = (int)this.WrittenBytes;
                }

                try
                {
                    ReaderWriterLock.AcquireWriterLock();
                    _bwConfig = connectionBWConfig;
                    if (_bwConfig.UpstreamBandwidth > 0)
                    {
                        // Update generation of BytesRead messages
                        // TODO: what are the correct values here?
                        _bytesReadInterval = (int)connectionBWConfig.UpstreamBandwidth / 8;
                        _nextBytesRead = (int)this.WrittenBytes;
                    }
                    if (_bwContext != null)
                    {
                        IBWControlService bwController = this.Scope.GetService(typeof(IBWControlService)) as IBWControlService;
                        bwController.UpdateBWConfigure(_bwContext);
                    }
                }
                finally
                {
                    ReaderWriterLock.ReleaseWriterLock();
                }
            }
        }

        #endregion

        /// <summary>
        /// Start measuring the roundtrip time for a packet on the connection.
        /// </summary>
        /// <param name="ping"></param>
        public void Ping(Ping ping)
        {
            GetChannel(2).Write(ping);
        }

        /// <summary>
        /// Marks that pingback was received.
        /// </summary>
        /// <param name="pong"></param>
        internal void PingReceived(Ping pong)
        {
            int now = Environment.TickCount;
            int previousReceived = _lastPongReceived.Value;
#if !SILVERLIGHT
            if( log.IsDebugEnabled )
                log.Debug(string.Format("{0} Ping received at {1} with value {2}, previous received at {3}", _connectionId, now, pong.Value2, previousReceived ));
#endif
            if (_lastPongReceived.CompareAndSet(previousReceived, now))
            {
                _lastPingTime.Value = ((int)(previousReceived & 0xffffffff)) - pong.Value2;
            }
        }
        /// <summary>
        /// Gets roundtrip time of last ping command.
        /// </summary>
        public override int LastPingTime { get { return _lastPingTime.Value; } }


		#region IServiceCapableConnection Members

        /// <summary>
        /// Invokes service using service call object.
        /// </summary>
        /// <param name="serviceCall">Service call object.</param>
		public void Invoke(IServiceCall serviceCall)
		{
			Invoke(serviceCall, (byte)3);
		}
        /// <summary>
        /// Invokes service using service call object and channel.
        /// </summary>
        /// <param name="serviceCall">Service call object.</param>
        /// <param name="channel">Channel to use.</param>
        public void Invoke(IServiceCall serviceCall, byte channel)
		{
			// We need to use Invoke for all calls to the client
			Invoke invoke = new Invoke();
			invoke.ServiceCall = serviceCall;
			invoke.InvokeId = this.InvokeId;
			if(serviceCall is IPendingServiceCall)
			{
                _pendingCalls[invoke.InvokeId] = serviceCall;
			}
			GetChannel(channel).Write(invoke);
		}
        /// <summary>
        /// Invoke method by name.
        /// </summary>
        /// <param name="method">Method name.</param>
		public void Invoke(string method)
		{
			Invoke(method, null, null);
		}
        /// <summary>
        /// Invoke method by name with callback.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        public void Invoke(string method, IPendingServiceCallback callback)
		{
			Invoke(method, null, callback);
		}
        /// <summary>
        /// Invoke method with parameters.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="parameters">Invocation parameters passed to the method.</param>
        public void Invoke(string method, object[] parameters)
		{
			Invoke(method, parameters, null);
		}
        /// <summary>
        /// Invoke method with parameters and callback.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="parameters">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        public void Invoke(string method, object[] parameters, IPendingServiceCallback callback)
		{
			IPendingServiceCall call = new PendingCall(method, parameters);
			if(callback != null) 
				call.RegisterCallback(callback);
			Invoke(call);
		}
#if !SILVERLIGHT
        /// <summary>
        /// Begins an asynchronous operation to invoke a service using service call object and channel.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="serviceCall">Service call object.</param>
        /// <param name="channel">Channel to use.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>        
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, IServiceCall serviceCall, byte channel)
        {
            // Create IAsyncResult object identifying the asynchronous operation
            AsyncResultNoResult ar = new AsyncResultNoResult(asyncCallback, new InvokeData(FluorineContext.Current, serviceCall, channel));
            // Use a thread pool thread to perform the operation
            FluorineFx.Threading.ThreadPoolEx.Global.QueueUserWorkItem(new System.Threading.WaitCallback(OnBeginInvoke), ar);
            // Return the IAsyncResult to the caller
            return ar;
        }

        private void OnBeginInvoke(object asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            try
            {
                // Perform the operation; if sucessful set the result
                InvokeData invokeData = ar.AsyncState as InvokeData;
                //Restore context
                FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, invokeData.Context);
                Invoke(invokeData.Call, invokeData.Channel);
                ar.SetAsCompleted(null, false);
            }
            catch (Exception ex)
            {
                // If operation fails, set the exception
                ar.SetAsCompleted(ex, false);
            }
            finally
            {
                FluorineWebSafeCallContext.FreeNamedDataSlot(FluorineContext.FluorineContextKey);
            }
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke a service using service call object.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="serviceCall">Service call object.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>        
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, IServiceCall serviceCall)
        {
            return BeginInvoke(asyncCallback, serviceCall, (byte)3);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke a service by name.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, string method)
        {
            return BeginInvoke(asyncCallback, method, null, null);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke a service by name and with callback.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, string method, IPendingServiceCallback callback)
        {
            return BeginInvoke(asyncCallback, method, null, callback);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke a service by name and with parameters.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="parameters">Invocation parameters passed to the method.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, string method, object[] parameters)
        {
            return BeginInvoke(asyncCallback, method, parameters, null);
        }
        /// <summary>
        /// Begins an asynchronous operation to invoke a service by name with parameters and callback.
        /// </summary>
        /// <param name="asyncCallback">The AsyncCallback delegate.</param>
        /// <param name="method">Method name.</param>
        /// <param name="parameters">Invocation parameters passed to the method.</param>
        /// <param name="callback">Callback used to handle return values.</param>
        /// <returns>An IAsyncResult that references the asynchronous invocation.</returns>
        /// <remarks>
        /// <para>
        /// You can create a callback method that implements the AsyncCallback delegate and pass its name to the BeginInvoke method.
        /// </para>
        /// <para>
        /// Your callback method should invoke the EndInvoke method. When your application calls BeginInvoke, the system will use a separate thread to execute the specified callback method, and will block on EndInvoke until the client is invoked successfully or throws an exception.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginInvoke(AsyncCallback asyncCallback, string method, object[] parameters, IPendingServiceCallback callback)
        {
            IPendingServiceCall call = new PendingCall(method, parameters);
            if (callback != null)
                call.RegisterCallback(callback);
            return BeginInvoke(asyncCallback, call);
        }
        /// <summary>
        /// Ends a pending asynchronous service invocation.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <remarks>
        /// <para>
        /// EndInvoke is a blocking method that completes the asynchronous client invocation request started in the BeginInvoke method.
        /// </para>
        /// <para>
        /// Before calling BeginInvoke, you can create a callback method that implements the AsyncCallback delegate. This callback method executes in a separate thread and is called by the system after BeginInvoke returns. 
        /// The callback method must accept the IAsyncResult returned by the BeginInvoke method as a parameter.
        /// </para>
        /// <para>Within the callback method you can call the EndInvoke method to successfully complete the invocation attempt.</para>
        /// <para>The BeginInvoke enables to use the fire and forget pattern too (by not implementing an AsyncCallback delegate), however if the invocation fails the EndInvoke method is responsible to throw an appropriate exception.
        /// Implementing the callback and calling EndInvoke also allows early garbage collection of the internal objects used in the asynchronous call.</para>
        /// </remarks>
        public void EndInvoke(IAsyncResult asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }
#endif
        /// <summary>
        /// Notifies service using service call object.
        /// </summary>
        /// <param name="serviceCall">Service call object.</param>
		public void Notify(IServiceCall serviceCall)
		{
			Notify(serviceCall, (byte)3);
		}
        /// <summary>
        /// Notifies service using service call object and channel.
        /// </summary>
        /// <param name="serviceCall">Service call object.</param>
        /// <param name="channel">Channel to use.</param>
		public void Notify(IServiceCall serviceCall, byte channel)
		{
			Notify notify = new Notify();
			notify.ServiceCall = serviceCall;
			GetChannel(channel).Write(notify);
		}
        /// <summary>
        /// Notifies method by name.
        /// </summary>
        /// <param name="method">Method name.</param>
		public void Notify(string method)
		{
			Notify(method, null);
		}
        /// <summary>
        /// Notifies method with parameters.
        /// </summary>
        /// <param name="method">Method name.</param>
        /// <param name="parameters">Parameters passed to the method.</param>
		public void Notify(string method, object[] parameters)
		{
			IServiceCall serviceCall = new Call(method, parameters);
			Notify(serviceCall);
		}

		#endregion

        /// <summary>
        /// Returns a string that represents the current RtmpConnection object.
        /// </summary>
        /// <returns>A string that represents the current RtmpConnection object.</returns>
        public override string ToString()
		{
			return "RtmpConnection " + _connectionId;
		}

        /// <summary>
        /// Increases number of read messages by one. Updates number of bytes read.
        /// </summary>
        internal void MessageReceived()
        {
            _readMessages.Increment();
            // Trigger generation of BytesRead messages            
            UpdateBytesRead();
        }

	    internal virtual void MessageSent(RtmpPacket packet) 
        {
            if (packet.Message is VideoData)
            {
                int streamId = packet.Header.StreamId;
                AtomicInteger pending = null;
                _pendingVideos.TryGetValue(streamId, out pending);
                if (pending != null)
                    pending.Decrement();
            }
		    _writtenMessages.Increment();
	    }

        /// <summary>
        /// Update number of bytes to read next value.
        /// </summary>
        protected void UpdateBytesRead()
        {
            BytesRead sbr = null;
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                long bytesRead = this.ReadBytes;
                if (bytesRead >= _nextBytesRead)
                {
                    sbr = new BytesRead((int)bytesRead);
                    //GetChannel((byte)2).Write(sbr);
                    _nextBytesRead += _bytesReadInterval;
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
            if( sbr != null )
                GetChannel((byte)2).Write(sbr);
        }
        /// <summary>
        /// Gets the total number of bytes read from the connection.
        /// </summary>
        public override long ReadBytes { get { return 0; } }
        /// <summary>
        /// Gets the total number of bytes written to the connection.
        /// </summary>
        public override long WrittenBytes { get { return 0; } }
        /// <summary>
        /// Update the number of received bytes.
        /// </summary>
        /// <param name="bytes"></param>
        internal void ReceivedBytesRead(int bytes)
        {
            try
            {
                ReaderWriterLock.AcquireWriterLock();
#if !SILVERLIGHT
                log.Info("Client received " + bytes + " bytes, written " + this.WrittenBytes + " bytes, " + this.PendingMessages + " messages pending");
#endif
                _clientBytesRead = bytes;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Gets the number of bytes the client reported to have received.
        /// </summary>
        public override long ClientBytesRead
        {
            get
            {
                try
                {
                    ReaderWriterLock.AcquireReaderLock();
                    return _clientBytesRead;
                }
                finally
                {
                    ReaderWriterLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Registers deferred result.
        /// </summary>
        /// <param name="result">Result to register.</param>
        internal void RegisterDeferredResult(DeferredResult result)
        {
            ReaderWriterLock.AcquireWriterLock();
            try
            {
                _deferredResults.Add(result, null);
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Unregister deferred result.
        /// </summary>
        /// <param name="result">Result to unregister.</param>
        internal void UnregisterDeferredResult(DeferredResult result)
        {
            ReaderWriterLock.AcquireWriterLock();
            try
            {
                _deferredResults.Remove(result);
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Start waiting for a valid handshake.
        /// </summary>
        internal virtual void StartWaitForHandshake()
        {
        }

#if !FXCLIENT
        internal class WaitForHandshakeJob : ScheduledJobBase
        {
            RtmpConnection _connection;

            public WaitForHandshakeJob(RtmpConnection connection)
            {
                _connection = connection;
            }

            public override void Execute(ScheduledJobContext context)
            {
                _connection.ReaderWriterLock.AcquireWriterLock();
                try
                {
                    FluorineRtmpContext.Initialize(_connection);
                    _connection._waitForHandshakeJob = null;
                    if (log.IsWarnEnabled)
                        log.Warn(string.Format("{0} Closing due to long handshake", _connection.ConnectionId));
                }
                finally
                {
                    _connection.ReaderWriterLock.ReleaseWriterLock();
                }
                // Client didn't send a valid handshake, disconnect.
                _connection.OnInactive();
            }
        }
#endif
        /// <summary>
        /// Starts measurement.
        /// </summary>
        internal virtual void StartRoundTripMeasurement()
        {
#if !FXCLIENT
            if (FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.PingInterval <= 0)
            {
                // Ghost detection code disabled
                return;
            }
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                if (_keepAliveJobName == null && this.Scope != null)
                {
                    ISchedulingService service = this.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                    if (service != null)
                    {
                        _keepAliveJobName = service.AddScheduledJob(FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.PingInterval, new KeepAliveJob(this));
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("{0} Keep alive job name {1}", _connectionId, _keepAliveJobName));
                    }
                }
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
#endif
        }

        /// <summary>
        /// Inactive state event handler.
        /// </summary>
        protected abstract void OnInactive();

        /// <summary>
        /// Pushes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageClient">The message client.</param>
        public abstract void Push(IMessage message, IMessageClient messageClient);

#if !FXCLIENT
        private class KeepAliveJob : ScheduledJobBase
        {
		    private AtomicLong _lastBytesRead = new AtomicLong(0);
		    private long _lastBytesReadTime = 0;

            RtmpConnection _connection;

            public KeepAliveJob(RtmpConnection connection)
            {
                _connection = connection;
            }

            public override void Execute(ScheduledJobContext context)
            {
                if (!_connection.IsConnected)
                    return;
                long thisRead = _connection.ReadBytes;
                long previousReadBytes = _lastBytesRead.Value;
                if (thisRead > previousReadBytes)
                {
                    // Client sent data since last check and thus is not dead. No need to ping.
                    if (_lastBytesRead.CompareAndSet(previousReadBytes, thisRead))
                        _lastBytesReadTime = System.Environment.TickCount;
                    return;
                }
                FluorineRtmpContext.Initialize(_connection);
                if (_connection._lastPongReceived.Value > 0 && 
                    _connection._lastPingSent.Value - _connection._lastPongReceived.Value > FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.MaxInactivity
                    && !(System.Environment.TickCount - _lastBytesReadTime < FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpConnectionSettings.MaxInactivity))
                {
                    try
                    {
                        _connection.ReaderWriterLock.AcquireWriterLock();
                        // Client didn't send response to ping command for too long, disconnect
                        if (_connection._keepAliveJobName != null)
                        {
                            if (log.IsDebugEnabled)
                                log.Debug(string.Format("{0} Keep alive job name {1}", _connection.ConnectionId, _connection._keepAliveJobName));

                            ISchedulingService service = _connection.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                            service.RemoveScheduledJob(_connection._keepAliveJobName);
                            _connection._keepAliveJobName = null;
                        }
                    }
                    finally
                    {
                        _connection.ReaderWriterLock.ReleaseWriterLock();
                    }
                    if( log.IsWarnEnabled )
                        log.Warn(string.Format("{0} Closing due to too much inactivity ({1}ms), last ping sent {2}ms ago", _connection.ConnectionId, _connection._lastPingSent.Value - _connection._lastPongReceived.Value,
                            System.Environment.TickCount - _connection._lastPingSent.Value));
                    _connection.OnInactive();
                    return;
                }
                // Send ping command to client to trigger sending of data.
                _connection.Ping();
            }
        }
#endif

    }

    #region InvokeData

#if !SILVERLIGHT
    class InvokeData
    {
        FluorineContext _context;
        string _method;
        object[] _arguments;
        IPendingServiceCallback _callback;
        bool _ignoreSelf;
        IScope _targetScope;
        
        IServiceCall _call;
        byte _channel;

        public FluorineContext Context
        {
            get { return _context; }
        }

        public string Method
        {
            get { return _method; }
        }

        public object[] Arguments
        {
            get { return _arguments; }
        }

        public IPendingServiceCallback Callback
        {
            get { return _callback; }
        }

        public bool IgnoreSelf
        {
            get { return _ignoreSelf; }
        }

        public IScope TargetScope
        {
            get { return _targetScope; }
        }

        public IServiceCall Call
        {
            get { return _call; }
        }

        public byte Channel
        {
            get { return _channel; }
        }

        public InvokeData(FluorineContext context, string method, object[] arguments, IPendingServiceCallback callback, bool ignoreSelf, IScope targetScope)
        {
            _context = context;
            _method = method;
            _arguments = arguments;
            _callback = callback;
            _ignoreSelf = ignoreSelf;
            _targetScope = targetScope;
        }

        public InvokeData(FluorineContext context, IServiceCall call, byte channel)
        {
            _context = context;
            _call = call;
            _channel = channel;
        }
    }
#endif
    #endregion InvokeData

}
