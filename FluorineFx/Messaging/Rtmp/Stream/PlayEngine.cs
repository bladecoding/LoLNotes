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
using System.IO;
using log4net;
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Statistics;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.Messaging;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Codec;
using FluorineFx.Messaging.Rtmp.Stream.Consumer;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
using FluorineFx.Messaging.Messages;
using FluorineFx.Scheduling;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// A play engine for playing an IPlayItem.
    /// </summary>
    class PlayEngine : IFilter, IPushableConsumer, IPipeConnectionListener, ITokenBucketCallback
    {
        private static ILog log = LogManager.GetLogger(typeof(PlayEngine));

        private IMessageInput _msgIn;
        private IMessageOutput _msgOut;
        private bool _isPullMode;
        /// <summary>
        /// Receive video?
        /// </summary>
        private bool _receiveVideo = true;
        /// <summary>
        /// Receive audio?
        /// </summary>
        private bool _receiveAudio = true;	

        private ISchedulingService _schedulingService;
        private IConsumerService _consumerService;
        private IProviderService _providerService;

        private string _waitLiveJob;
        private bool _waiting;
        /// <summary>
        /// Timestamp of first sent packet.
        /// </summary>
        private int _streamStartTS;
        private IPlayItem _currentItem;
        private ITokenBucket _audioBucket;
        private ITokenBucket _videoBucket;
        private RtmpMessage _pendingMessage;
        private bool _waitingForToken = false;
        private bool _checkBandwidth = true;

        IBWControlService _bwController;
        IBWControlContext _bwContext;

        /// <summary>
        /// Interval in ms to check for buffer underruns in VOD streams.
        /// </summary>
        private int _bufferCheckInterval = 0;
        /// <summary>
        /// Number of pending messages at which a <code>NetStream.Play.InsufficientBW</code> message is generated for VOD streams.
        /// </summary>
        private int _underrunTrigger = 10;
        /// <summary>
        /// State machine for video frame dropping in live streams
        /// </summary>
        private IFrameDropper _videoFrameDropper = new VideoFrameDropper();
        private int _timestampOffset = 0;
        /// <summary>
        /// Last message sent to the client.
        /// </summary>
        private IRtmpEvent _lastMessage;
        /// <summary>
        /// Number of bytes sent.
        /// </summary>
        private long _bytesSent = 0;
        /// <summary>
        /// Start time of stream playback.
        /// It's not a time when the stream is being played but
        /// the time when the stream should be played if it's played
        /// from the very beginning.
        /// Eg. A stream is played at timestamp 5s on 1:00:05. The
        /// playbackStart is 1:00:00.
        /// </summary>
        private long _playbackStart;
        
        /// <summary>
        /// Scheduled future job that makes sure messages are sent to the client.
        /// </summary>
        //System.Timers.Timer _pullAndPushTimer;
        private string _pullAndPushJob;

        /// <summary>
        /// Offset in ms the stream started.
        /// </summary>
        private int _streamOffset;
        /// <summary>
        /// Timestamp when buffer should be checked for underruns next.
        /// </summary>
        private long _nextCheckBufferUnderrun;
        /// <summary>
        /// Send blank audio packet next?
        /// </summary>
        private bool _sendBlankAudio;
        /// <summary>
        /// Decision: 0 for Live, 1 for File, 2 for Wait, 3 for N/A
        /// </summary>
        private int _playDecision = 3;

        PlaylistSubscriberStream _playlistSubscriberStream;

        object _syncLock = new object();

        private PlayEngine(Builder builder)
        {
            _playlistSubscriberStream = builder.PlaylistSubscriberStream;
            _schedulingService = builder.SchedulingService;
            _consumerService = builder.ConsumerService;
            _providerService = builder.ProviderService;
            //_playlistSubscriberStream = stream;
            //_schedulingService = _playlistSubscriberStream.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
        }

        /// <summary>
        /// Builder pattern
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Required for play engine.
            /// </summary>
            readonly PlaylistSubscriberStream _playlistSubscriberStream;

            internal PlaylistSubscriberStream PlaylistSubscriberStream
            {
                get { return _playlistSubscriberStream; }
            } 

            /// <summary>
            /// Required for play engine.
            /// </summary>
            readonly ISchedulingService _schedulingService;

            public ISchedulingService SchedulingService
            {
                get { return _schedulingService; }
            } 

            /// <summary>
            /// Required for play engine.
            /// </summary>
            readonly IConsumerService _consumerService;

            public IConsumerService ConsumerService
            {
                get { return _consumerService; }
            } 

            /// <summary>
            /// Required for play engine.
            /// </summary>
            readonly IProviderService _providerService;

            public IProviderService ProviderService
            {
                get { return _providerService; }
            } 


            public Builder(PlaylistSubscriberStream playlistSubscriberStream, ISchedulingService schedulingService, IConsumerService consumerService, IProviderService providerService)
            {
                _playlistSubscriberStream = playlistSubscriberStream;
                _schedulingService = schedulingService;
                _consumerService = consumerService;
                _providerService = providerService;
            }

            public PlayEngine Build()
            {
                return new PlayEngine(this);
            }
        }

        public object SyncRoot { get { return _syncLock; } }

        public int StreamId
        {
            get { return _playlistSubscriberStream.StreamId; }
        }

        public bool SendBlankAudio
        {
            get { return _sendBlankAudio; }
            set { _sendBlankAudio = value; }
        }

        public long PlaybackStart
        {
            get { return _playbackStart; }
        }

        public int BufferCheckInterval
        {
            get { return _bufferCheckInterval; }
            set { _bufferCheckInterval = value; }
        }

        public int UnderrunTrigger
        {
            get { return _underrunTrigger; }
            set { _underrunTrigger = value; }
        }

        public bool IsPullMode
        {
            get { return _isPullMode; }
        }

        public bool IsPaused
        {
            get { return _playlistSubscriberStream.State == State.PAUSED; }
        }

        public IRtmpEvent LastMessage
        {
            get { return _lastMessage; }
            set { _lastMessage = value; }
        }

        /// <summary>
        /// Returns true if the engine currently receives video and sets the new value.
        /// </summary>
        /// <param name="receive">New value.</param>
        /// <returns>Old value.</returns>
        public bool ReceiveVideo(bool receive)
        {
            bool oldValue = _receiveVideo;
            //Set new value
            if (_receiveVideo != receive)
                _receiveVideo = receive;
            return oldValue;
        }
        /// <summary>
        /// Returns true if the engine currently receives audio and sets the new value.
        /// </summary>
        /// <param name="receive">New value.</param>
        /// <returns>Old value.</returns>
        public bool ReceiveAudio(bool receive)
        {
            bool oldValue = _receiveAudio;
            //Set new value
            if (_receiveAudio != receive)
                _receiveAudio = receive;
            return oldValue;
        }

        public void SetBandwidthController(IBWControlService bwController, IBWControlContext bwContext)
        {
            _bwController = bwController;
            _bwContext = bwContext;
        }
        /// <summary>
        /// Update bandwidth configuration
        /// </summary>
        public void UpdateBandwithConfigure()
        {
            _bwController.UpdateBWConfigure(_bwContext);
        }

        /// <summary>
        /// Start stream.
        /// </summary>
        public void Start()
        {
            lock (this.SyncRoot)
            {
                if (_playlistSubscriberStream.State != State.UNINIT)
                    throw new IllegalStateException();
                _playlistSubscriberStream.State = State.STOPPED;
                if (_msgOut == null)
                {
                    _msgOut = _consumerService.GetConsumerOutput(_playlistSubscriberStream);
                    _msgOut.Subscribe(this, null);
                }
                _audioBucket = _bwController.GetAudioBucket(_bwContext);
                _videoBucket = _bwController.GetVideoBucket(_bwContext);
            }
        }

        /// <summary>
        /// Stop playback
        /// </summary>
        public void Stop()
        {
            lock (this.SyncRoot)
            {
                if (_playlistSubscriberStream.State != State.PLAYING && _playlistSubscriberStream.State != State.PAUSED)
                    throw new IllegalStateException();
                _playlistSubscriberStream.State = State.STOPPED;
                if (_msgIn != null && !_isPullMode)
                {
                    _msgIn.Unsubscribe(this);
                    _msgIn = null;
                }
                _playlistSubscriberStream.NotifyItemStop(_currentItem);
                ClearWaitJobs();
                if (!_playlistSubscriberStream.HasMoreItems)
                {
                    ReleasePendingMessage();
                    _bwController.ResetBuckets(_bwContext);
                    _waitingForToken = false;
                    if (_playlistSubscriberStream.ItemSize > 0)
                        SendCompleteStatus();
                    _bytesSent = 0;
                    SendClearPing();
                    SendStopStatus(_currentItem);
                }
                else
                {
                    if (_lastMessage != null)
                    {
                        // Remember last timestamp so we can generate correct headers in playlists.
                        _timestampOffset = _lastMessage.Timestamp;
                    }
                    _playlistSubscriberStream.NextItem();
                }
            }
        }

        /// <summary>
        /// Close stream
        /// </summary>
        public void Close()
        {
            lock (this.SyncRoot)
            {
                if (_msgIn != null)
                {
                    _msgIn.Unsubscribe(this);
                    _msgIn = null;
                }
                _playlistSubscriberStream.State = State.CLOSED;
                ClearWaitJobs();
                ReleasePendingMessage();
                _lastMessage = null;
                SendClearPing();
            }
        }

        /// <summary>
        /// Pause at position
        /// </summary>
        /// <param name="position"></param>
        public void Pause(int position)
        {
            lock (this.SyncRoot)
            {
                if ((_playlistSubscriberStream.State != State.PLAYING && _playlistSubscriberStream.State != State.STOPPED) || _currentItem == null)
                {
                    throw new IllegalStateException();
                }
                _playlistSubscriberStream.State = State.PAUSED;
                ReleasePendingMessage();
                ClearWaitJobs();
                SendClearPing();
                SendPauseStatus(_currentItem);
                _playlistSubscriberStream.NotifyItemPause(_currentItem, position);
            }
        }
        /// <summary>
        /// Resume playback
        /// </summary>
        /// <param name="position"></param>
        public void Resume(int position)
        {
            lock (this.SyncRoot)
            {
                if (_playlistSubscriberStream.State != State.PAUSED)
                    throw new IllegalStateException();

                _playlistSubscriberStream.State = State.PLAYING;
                SendReset();
                SendResumeStatus(_currentItem);
                if (_isPullMode)
                {
                    SendVODSeekCM(_msgIn, position);
                    _playlistSubscriberStream.NotifyItemResume(_currentItem, position);
                    _playbackStart = System.Environment.TickCount - position;
                    if (_currentItem.Length >= 0 && (position - _streamOffset) >= _currentItem.Length)
                    {
                        // Resume after end of stream
                        Stop();
                    }
                    else
                    {
                        EnsurePullAndPushRunning();
                    }
                }
                else
                {
                    _playlistSubscriberStream.NotifyItemResume(_currentItem, position);
                    _videoFrameDropper.Reset(FrameDropperState.SEND_KEYFRAMES_CHECK);
                }
            }
        }

        /// <summary>
        /// Seek position in file
        /// </summary>
        /// <param name="position"></param>
        public void Seek(int position)
        {
            lock (this.SyncRoot)
            {
                if (_playlistSubscriberStream.State != State.PLAYING && _playlistSubscriberStream.State != State.PAUSED && _playlistSubscriberStream.State != State.STOPPED)
                {
                    throw new IllegalStateException();
                }
                if (!_isPullMode)
                {
                    throw new NotSupportedException();
                }

                ReleasePendingMessage();
                ClearWaitJobs();
                _bwController.ResetBuckets(_bwContext);
                _waitingForToken = false;
                SendClearPing();
                SendReset();
                SendSeekStatus(_currentItem, position);
                SendStartStatus(_currentItem);
                int seekPos = SendVODSeekCM(_msgIn, position);
                // We seeked to the nearest keyframe so use real timestamp now
                if (seekPos == -1)
                {
                    seekPos = position;
                }
                _playbackStart = System.Environment.TickCount - seekPos;
                _playlistSubscriberStream.NotifyItemSeek(_currentItem, seekPos);
                bool messageSent = false;
                bool startPullPushThread = false;
                if ((_playlistSubscriberStream.State == State.PAUSED || _playlistSubscriberStream.State == State.STOPPED) && SendCheckVideoCM(_msgIn))
                {
                    // we send a single snapshot on pause.
                    // XXX we need to take BWC into account, for
                    // now send forcefully.
                    IMessage msg;
                    try
                    {
                        msg = _msgIn.PullMessage();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error while pulling message.", ex);
                        msg = null;
                    }
                    while (msg != null)
                    {
                        if (msg is RtmpMessage)
                        {
                            RtmpMessage rtmpMessage = (RtmpMessage)msg;
                            IRtmpEvent body = rtmpMessage.body;
                            if (body is VideoData && ((VideoData)body).FrameType == FrameType.Keyframe)
                            {
                                body.Timestamp = seekPos;
                                DoPushMessage(rtmpMessage);
                                //rtmpMessage.body.Release();
                                messageSent = true;
                                _lastMessage = body;
                                break;
                            }
                        }

                        try
                        {
                            msg = _msgIn.PullMessage();
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error while pulling message.", ex);
                            msg = null;
                        }
                    }
                }
                else
                {
                    startPullPushThread = true;
                }

                if (!messageSent)
                {
                    // Send blank audio packet to notify client about new position
                    AudioData audio = new AudioData();
                    audio.Timestamp = seekPos;
                    audio.Header = new RtmpHeader();
                    audio.Header.Timer = seekPos;
                    audio.Header.IsTimerRelative = false;
                    RtmpMessage audioMessage = new RtmpMessage();
                    audioMessage.body = audio;
                    _lastMessage = audio;
                    DoPushMessage(audioMessage);
                }

                if (startPullPushThread)
                {
                    EnsurePullAndPushRunning();
                }

                if (_playlistSubscriberStream.State != State.STOPPED && _currentItem.Length >= 0 && (position - _streamOffset) >= _currentItem.Length)
                {
                    // Seeked after end of stream
                    Stop();
                    return;
                }
            }
        }

        /// <summary>
        /// Releases pending message body, nullifies pending message object
        /// </summary>
        private void ReleasePendingMessage()
        {
            lock (this.SyncRoot)
            {
                if (_pendingMessage != null)
                {
                    IRtmpEvent body = _pendingMessage.body;
                    if (body is IStreamData && ((IStreamData)body).Data != null)
                    {
                        //((IStreamData)body).Data.Release(); 
                    }
                    _pendingMessage.body = null;
                    _pendingMessage = null;
                }
            }
        }

        /// <summary>
        /// Make sure the pull and push processing is running.
        /// </summary>
        private void EnsurePullAndPushRunning()
        {
            if (!_isPullMode)
            {
                // We don't need this for live streams
                return;
            }
            lock (this.SyncRoot)
            {
                if (_pullAndPushJob == null)
                {
                    PullAndPushJob job = new PullAndPushJob(this);
                    _pullAndPushJob = _schedulingService.AddScheduledJob(10, job);
                    job.Execute(null);
                }
            }
            /*
            if (_pullAndPushTimer == null)
            {
                lock (this.SyncRoot)
                {
                    if (_pullAndPushTimer == null)
                    {
                        _pullAndPushTimer = new System.Timers.Timer();
                        _pullAndPushTimer.Elapsed += new System.Timers.ElapsedEventHandler(PullAndPushTimer_Elapsed);
                        _pullAndPushTimer.Interval = 10;
                        _pullAndPushTimer.AutoReset = true;
                        _pullAndPushTimer.Enabled = true;
                    }
                }
            }
            */
        }

        void PullAndPushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                PullAndPush();
            }
            catch (IOException ex)
            {
                // We couldn't get more data, stop stream.
                log.Error("Error while getting message.", ex);
                this.Stop();
            }
        }

        /// <summary>
        /// Recieve then send if message is data (not audio or video)
        /// </summary>
        internal void PullAndPush()
        {
            lock (this.SyncRoot)
            {
                if (_playlistSubscriberStream.State == State.PLAYING && _isPullMode && !_waitingForToken)
                {
                    if (_pendingMessage != null)
                    {
                        IRtmpEvent body = _pendingMessage.body;
                        if (!OkayToSendMessage(body))
                            return;

                        SendMessage(_pendingMessage);
                        ReleasePendingMessage();
                    }
                    else
                    {
                        while (true)
                        {
                            IMessage msg = _msgIn.PullMessage();
                            if (msg == null)
                            {
                                // No more packets to send
                                Stop();
                                break;
                            }
                            else
                            {
                                if (msg is RtmpMessage)
                                {
                                    RtmpMessage rtmpMessage = (RtmpMessage)msg;
                                    IRtmpEvent body = rtmpMessage.body;
                                    if (!_receiveAudio && body is AudioData)
                                    {
                                        // The user doesn't want to get audio packets
                                        //((IStreamData) body).Data.Release();
                                        if (_sendBlankAudio)
                                        {
                                            // Send reset audio packet
                                            _sendBlankAudio = false;
                                            body = new AudioData();
                                            // We need a zero timestamp
                                            if (_lastMessage != null)
                                            {
                                                body.Timestamp = _lastMessage.Timestamp - _timestampOffset;
                                            }
                                            else
                                            {
                                                body.Timestamp = -_timestampOffset;
                                            }
                                            rtmpMessage.body = body;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else if (!_receiveVideo && body is VideoData)
                                    {
                                        // The user doesn't want to get video packets
                                        //((IStreamData) body).Data.Release();
                                        continue;
                                    }

                                    // Adjust timestamp when playing lists
                                    body.Timestamp = body.Timestamp + _timestampOffset;
                                    if (OkayToSendMessage(body))
                                    {
                                        if( log.IsDebugEnabled )
                                            log.Debug(string.Format("ts: {0}", rtmpMessage.body.Timestamp));
                                        SendMessage(rtmpMessage);
                                        //((IStreamData) body).Data.Release();
                                    }
                                    else
                                    {
                                        _pendingMessage = rtmpMessage;
                                    }
                                    EnsurePullAndPushRunning();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if it's okay to send the client more data. This takes the configured
        /// bandwidth as well as the requested client buffer into account.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool OkayToSendMessage(IRtmpEvent message)
        {
            if (!(message is IStreamData))
            {
                string itemName = "Undefined";
                //If current item exists get the name to help debug this issue
                if (_currentItem != null)
                    itemName = _currentItem.Name;
                throw new ApplicationException(string.Format("Expected IStreamData but got {0} (type {1}) for {2}" + message.GetType().ToString(), message.EventType, itemName));
            }
            long now = System.Environment.TickCount;
            // Check client buffer length when we've already sent some messages
            if (_lastMessage != null)
            {
                // Duration the stream is playing / playback duration
                long delta = now - _playbackStart;
                // Buffer size as requested by the client
                long buffer = _playlistSubscriberStream.ClientBufferDuration;
                // Expected amount of data present in client buffer
                long buffered = _lastMessage.Timestamp - delta;
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("OkayToSendMessage timestamp {0} delta {1} buffered {2} buffer {3}", _lastMessage.Timestamp, delta, buffered, buffer));
                }
                //T his sends double the size of the client buffer
                if (buffer > 0 && buffered > (buffer*2))
                {
                    // Client is likely to have enough data in the buffer
                    return false;
                }
            }

            long pending = GetPendingMessagesCount();
            if (_bufferCheckInterval > 0 && now >= _nextCheckBufferUnderrun)
            {
                if (pending > _underrunTrigger)
                {
                    // Client is playing behind speed, notify him
                    SendInsufficientBandwidthStatus(_currentItem);
                }
                _nextCheckBufferUnderrun = now + _bufferCheckInterval;
            }

            if (pending > _underrunTrigger)
            {
                // Too many messages already queued on the connection
                return false;
            }

            ByteBuffer ioBuffer = ((IStreamData)message).Data;
            if (ioBuffer != null)
            {
                int size = ioBuffer.Limit;
                if (message is VideoData)
                {
                    if (_checkBandwidth && !_videoBucket.AcquireTokenNonblocking(size, this))
                    {
                        _waitingForToken = true;
                        return false;
                    }
                }
                else if (message is AudioData)
                {
                    if (_checkBandwidth && !_audioBucket.AcquireTokenNonblocking(size, this))
                    {
                        _waitingForToken = true;
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Get number of pending messages to be sent
        /// </summary>
        /// <returns></returns>
        private long GetPendingMessagesCount()
        {
            return _playlistSubscriberStream.Connection.PendingMessages;
        }

        /// <summary>
        /// Get number of pending video messages
        /// </summary>
        /// <returns></returns>
        private long GetPendingVideoMessageCount()
        {
            OOBControlMessage pendingRequest = new OOBControlMessage();
            pendingRequest.Target = "ConnectionConsumer";
            pendingRequest.ServiceName = "pendingVideoCount";
            _msgOut.SendOOBControlMessage(this, pendingRequest);
            if (pendingRequest.Result != null)
            {
                return (long)pendingRequest.Result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get informations about bytes send and number of bytes the client reports to have received.
        /// </summary>
        /// <returns>Written bytes and number of bytes the client received</returns>
        private long[] GetWriteDelta()
        {
            OOBControlMessage pendingRequest = new OOBControlMessage();
            pendingRequest.Target = "ConnectionConsumer";
            pendingRequest.ServiceName = "writeDelta";
            _msgOut.SendOOBControlMessage(this, pendingRequest);
            if (pendingRequest.Result != null)
            {
                return pendingRequest.Result as long[];
            }
            else
            {
                return new long[] { 0, 0 };
            }
        }

        /// <summary>
        /// Clear all scheduled waiting jobs
        /// </summary>
        private void ClearWaitJobs()
        {
            lock (this.SyncRoot)
            {
                /*
                if (_pullAndPushTimer != null)
                {
                    _pullAndPushTimer.Enabled = false;
                    _pullAndPushTimer.Elapsed -= new System.Timers.ElapsedEventHandler(PullAndPushTimer_Elapsed);
                    _pullAndPushTimer = null;
                }
                */
                if (_pullAndPushJob != null)
                {
                    _schedulingService.RemoveScheduledJob(_pullAndPushJob);
                    _pullAndPushJob = null;
                }
                if (_waitLiveJob != null)
                {
                    _schedulingService.RemoveScheduledJob(_waitLiveJob);
                    _waitLiveJob = null;
                }
            }
        }

        /// <summary>
        /// Play stream.
        /// </summary>
        /// <param name="item">Playlist item.</param>
        public void Play(IPlayItem item)
        {
            Play(item, true);
        }

        internal class PlaylistSubscriberStreamJob1 : ScheduledJobBase
        {
            PlayEngine _engine;
            string _itemName;

            public PlaylistSubscriberStreamJob1(PlayEngine engine, string itemName)
            {
                _engine = engine;
                _itemName = itemName;
            }

            public override void Execute(ScheduledJobContext context)
            {
				//set the msgIn if its null
				if (_engine._msgIn == null) {
                    _engine.ConnectToProvider(_itemName);
				}	
                _engine._waitLiveJob = null;
                _engine._waiting = false;
                _engine._playlistSubscriberStream.OnItemEnd();
            }
        }

        internal class PlaylistSubscriberStreamJob2 : ScheduledJobBase
        {
            PlayEngine _engine;
            string _itemName;

            public PlaylistSubscriberStreamJob2(PlayEngine engine, string itemName)
            {
                _engine = engine;
                _itemName = itemName;
            }

            public override void Execute(ScheduledJobContext context)
            {
				//set the msgIn if its null
				if (_engine._msgIn == null) {
                    _engine.ConnectToProvider(_itemName);
				}	
                _engine._waitLiveJob = null;
                _engine._waiting = false;
            }
        }

        internal class PullAndPushJob : ScheduledJobBase
        {
            PlayEngine _engine;

            public PullAndPushJob(PlayEngine engine)
            {
                _engine = engine;
            }

            public override void Execute(ScheduledJobContext context)
            {
                try
                {
                    _engine.PullAndPush();
                }
                catch (Exception ex)
                {
                    // We couldn't get more data, stop stream.
                    log.Error("Error while getting message.", ex);
                    _engine.Stop();
                }
            }
        }

        /// <summary>
        /// Play stream
        /// </summary>
        /// <param name="item">Playlist item.</param>
        /// <param name="withReset">Send reset status before playing.</param>
        public void Play(IPlayItem item, bool withReset)
        {
            lock (this.SyncRoot)
            {
                // Can't play if state is not stopped
                if (_playlistSubscriberStream.State != State.STOPPED)
                    throw new IllegalStateException();
                if (_msgIn != null)
                {
                    _msgIn.Unsubscribe(this);
                    _msgIn = null;
                }
                // Play type determination
                // http://livedocs.adobe.com/flex/3/langref/flash/net/NetStream.html#play%28%29
                // The start time, in seconds. Allowed values are -2, -1, 0, or a positive number. 
                // The default value is -2, which looks for a live stream, then a recorded stream, 
                // and if it finds neither, opens a live stream. 
                // If -1, plays only a live stream. 
                // If 0 or a positive number, plays a recorded stream, beginning start seconds in.
                //
                // -2: live then recorded, -1: live, >=0: recorded
                int type = (int)(item.Start / 1000);
                // see if it's a published stream
                IScope thisScope = _playlistSubscriberStream.Scope;
                string itemName = item.Name;
                //check for input and type
                InputType sourceType = _providerService.LookupProviderInputType(thisScope, itemName);

                bool isPublishedStream = sourceType == InputType.Live;
                bool isFileStream = sourceType == InputType.Vod;
                bool sendNotifications = true;

                // decision: 0 for Live, 1 for File, 2 for Wait, 3 for N/A
                switch (type)
                {
                    case -2:
                        if (isPublishedStream)
                            _playDecision = 0;
                        else if (isFileStream)
                            _playDecision = 1;
                        else
                            _playDecision = 2;
                        break;
                    case -1:
                        if (isPublishedStream)
                            _playDecision = 0;
                        else
                            _playDecision = 2;
                        break;
                    default:
                        if (isFileStream)
                            _playDecision = 1;
                        break;
                }
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("Play decision is {0} (0=Live, 1=File, 2=Wait, 3=N/A)", _playDecision));
                _currentItem = item;
                long itemLength = item.Length;
                switch (_playDecision)
                {
                    case 0:
                        //get source input without create
                        _msgIn = _providerService.GetLiveProviderInput(thisScope, itemName, false);
                        // Drop all frames up to the next keyframe
                        _videoFrameDropper.Reset(FrameDropperState.SEND_KEYFRAMES_CHECK);
                        if (_msgIn is IBroadcastScope)
                        {
                            // Send initial keyframe
                            IClientBroadcastStream stream = (_msgIn as IBroadcastScope).GetAttribute(Constants.BroadcastScopeStreamAttribute) as IClientBroadcastStream;
                            if (stream != null && stream.CodecInfo != null)
                            {
                                IVideoStreamCodec videoCodec = stream.CodecInfo.VideoCodec;
                                if (videoCodec != null)
                                {
                                    if (withReset)
                                    {
                                        SendReset();
                                        SendResetStatus(item);
                                        SendStartStatus(item);
                                    }
                                    sendNotifications = false;
                                    //send decoder configuration if it exists
                                    ByteBuffer config = videoCodec.GetDecoderConfiguration();
                                    if (config != null)
                                    {
                                        VideoData conf = new VideoData(config);
                                        try
                                        {
                                            conf.Timestamp = 0;
                                            RtmpMessage confMsg = new RtmpMessage();
                                            confMsg.body = conf;
                                            _msgOut.PushMessage(confMsg);
                                        }
                                        finally
                                        {
                                            //conf.release();
                                        }
                                    }
                                    //Check for a keyframe to send
                                    ByteBuffer keyFrame = videoCodec.GetKeyframe();
                                    if (keyFrame != null)
                                    {
                                        VideoData video = new VideoData(keyFrame);
                                        try
                                        {
                                            video.Timestamp = 0;
                                            RtmpMessage videoMsg = new RtmpMessage();
                                            videoMsg.body = video;
                                            _msgOut.PushMessage(videoMsg);
                                            // Don't wait for keyframe
                                            _videoFrameDropper.Reset();
                                        }
                                        finally
                                        {
                                            //video.release();
                                        }
                                    }
                                }
                            }
                        }
                        _msgIn.Subscribe(this, null);
                        break;
                    case 2:
                        //get source input with create
                        _msgIn = _providerService.GetLiveProviderInput(thisScope, itemName, true);
                        _msgIn.Subscribe(this, null);
                        _waiting = true;
                        if (type == -1 && itemLength >= 0)
                        {
                            //log.debug("Creating wait job");
                            // Wait given timeout for stream to be published
                            PlaylistSubscriberStreamJob1 job = new PlaylistSubscriberStreamJob1(this, itemName);
                            _waitLiveJob = _schedulingService.AddScheduledOnceJob(item.Length, job);
                        }
                        else if (type == -2)
                        {
                            //log.debug("Creating wait job");
                            // Wait x seconds for the stream to be published
                            PlaylistSubscriberStreamJob2 job = new PlaylistSubscriberStreamJob2(this, itemName);
                            _waitLiveJob = _schedulingService.AddScheduledOnceJob(15000, job);
                        }
                        else
                        {
                            ConnectToProvider(itemName);
                        }
                        break;
                    case 1:
                        _msgIn = _providerService.GetVODProviderInput(thisScope, itemName);
                        if (_msgIn == null)
                        {
                            SendStreamNotFoundStatus(_currentItem);
                            throw new StreamNotFoundException(itemName);
                        }
                        if (!_msgIn.Subscribe(this, null))
                        {
                            log.Error("Input source subscribe failed");
                        }
                        break;
                    default:
                        SendStreamNotFoundStatus(_currentItem);
                        throw new StreamNotFoundException(itemName);
                }
                _playlistSubscriberStream.State = State.PLAYING;
                IMessage msg = null;
                _streamOffset = 0;
                _streamStartTS = -1;
                if (_playDecision == 1)
                {
                    if (withReset)
                    {
                        ReleasePendingMessage();
                    }
                    SendVODInitCM(_msgIn, item);
                    // Don't use pullAndPush to detect IOExceptions prior to sending NetStream.Play.Start
                    if (item.Start > 0)
                    {
                        _streamOffset = SendVODSeekCM(_msgIn, (int)item.Start);
                        // We seeked to the nearest keyframe so use real timestamp now
                        if (_streamOffset == -1)
                        {
                            _streamOffset = (int)item.Start;
                        }
                    }
                    msg = _msgIn.PullMessage();
                    if (msg is RtmpMessage)
                    {
                        IRtmpEvent body = ((RtmpMessage)msg).body;
                        if (itemLength == 0)
                        {
                            // Only send first video frame
                            body = ((RtmpMessage)msg).body;
                            while (body != null && !(body is VideoData))
                            {
                                msg = _msgIn.PullMessage();
                                if (msg == null)
                                    break;
                                if (msg is RtmpMessage)
                                    body = ((RtmpMessage)msg).body;
                            }
                        }
                        if (body != null)
                        {
                            // Adjust timestamp when playing lists
                            body.Timestamp = body.Timestamp + _timestampOffset;
                        }
                    }
                }
                if (sendNotifications)
                {
                    if (withReset)
                    {
                        SendReset();
                        SendResetStatus(item);
                    }
                    SendStartStatus(item);
                    if (!withReset)
                    {
                        SendSwitchStatus();
                    }
                }
                if (msg != null)
                {
                    SendMessage((RtmpMessage)msg);
                }
                _playlistSubscriberStream.NotifyItemPlay(_currentItem, !_isPullMode);
                if (withReset)
                {
                    long currentTime = System.Environment.TickCount;
                    _playbackStart = currentTime - _streamOffset;
                    _nextCheckBufferUnderrun = currentTime + _bufferCheckInterval;
                    if (_currentItem.Length != 0)
                    {
                        EnsurePullAndPushRunning();
                    }
                }
            }
        }

        private void ConnectToProvider(string itemName)
        {
            if (log.IsDebugEnabled)
                log.Debug(string.Format("Attempting connection to {0}", itemName));
            IScope thisScope = _playlistSubscriberStream.Scope;
            _msgIn = _providerService.GetLiveProviderInput(thisScope, itemName, true);
            if (_msgIn != null)
            {
                log.Debug(string.Format("Provider: {0}", _msgIn));
                if (_msgIn.Subscribe(this, null))
                {
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Subscribed to {0} provider", itemName));
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.Warn(string.Format("Subscribe to {0} provider failed", itemName));
                }
            }
            else
            {
                if (log.IsWarnEnabled)
                    log.Warn(string.Format("Provider was not found for {0}", itemName));
            }
        }


        #region IMessageComponent Members

        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
            if ("ConnectionConsumer".Equals(oobCtrlMsg.Target))
            {
                if (source is IProvider)
                {
                    _msgOut.SendOOBControlMessage((IProvider)source, oobCtrlMsg);
                }
            }
        }

        #endregion

        #region IPushableConsumer Members

        public void PushMessage(IPipe pipe, IMessage message)
        {
            lock (this.SyncRoot)
            {
                if (message is ResetMessage)
                {
                    SendReset();
                    return;
                }
                if (message is RtmpMessage)
                {
                    RtmpMessage rtmpMessage = (RtmpMessage)message;
                    IRtmpEvent body = rtmpMessage.body;
                    if (!(body is IStreamData))
                    {
                        throw new ApplicationException("expected IStreamData but got " + body.GetType().FullName);
                    }

                    int size = ((IStreamData)body).Data.Limit;
                    if (body is VideoData)
                    {
                        IVideoStreamCodec videoCodec = null;
                        if (_msgIn is IBroadcastScope)
                        {
                            IClientBroadcastStream stream = ((IBroadcastScope)_msgIn).GetAttribute(Constants.BroadcastScopeStreamAttribute) as IClientBroadcastStream;
                            if (stream != null && stream.CodecInfo != null)
                            {
                                videoCodec = stream.CodecInfo.VideoCodec;
                            }
                        }

                        if (videoCodec == null || videoCodec.CanDropFrames)
                        {
                            if (_playlistSubscriberStream.State == State.PAUSED)
                            {
                                // The subscriber paused the video
                                _videoFrameDropper.DropPacket(rtmpMessage);
                                return;
                            }

                            // Only check for frame dropping if the codec supports it
                            long pendingVideos = GetPendingVideoMessageCount();
                            if (!_videoFrameDropper.CanSendPacket(rtmpMessage, pendingVideos))
                            {
                                // Drop frame as it depends on other frames that were dropped before.
                                return;
                            }

                            bool drop = !_videoBucket.AcquireToken(size, 0);
                            if (!_receiveVideo || drop)
                            {
                                // The client disabled video or the app doesn't have enough bandwidth
                                // allowed for this stream.
                                _videoFrameDropper.DropPacket(rtmpMessage);
                                return;
                            }

                            long[] writeDelta = GetWriteDelta();
                            if (pendingVideos > 1 /*|| writeDelta[0] > writeDelta[1]*/)
                            {
                                // We drop because the client has insufficient bandwidth.
                                long now = System.Environment.TickCount;
                                if (_bufferCheckInterval > 0 && now >= _nextCheckBufferUnderrun)
                                {
                                    // Notify client about frame dropping (keyframe)
                                    SendInsufficientBandwidthStatus(_currentItem);
                                    _nextCheckBufferUnderrun = now + _bufferCheckInterval;
                                }
                                _videoFrameDropper.DropPacket(rtmpMessage);
                                return;
                            }

                            _videoFrameDropper.SendPacket(rtmpMessage);
                        }
                    }
                    else if (body is AudioData)
                    {
                        if (!_receiveAudio && _sendBlankAudio)
                        {
                            // Send blank audio packet to reset player
                            _sendBlankAudio = false;
                            body = new AudioData();
                            if (_lastMessage != null)
                            {
                                body.Timestamp = _lastMessage.Timestamp;
                            }
                            else
                            {
                                body.Timestamp = 0;
                            }
                            rtmpMessage.body = body;
                        }
                        else if (_playlistSubscriberStream.State == State.PAUSED || !_receiveAudio || !_audioBucket.AcquireToken(size, 0))
                        {
                            return;
                        }
                    }
                    if (body is IStreamData && ((IStreamData)body).Data != null)
                    {
                        _bytesSent += ((IStreamData)body).Data.Limit;
                    }
                    _lastMessage = body;
                }
                _msgOut.PushMessage(message);
            }
        }

        #endregion

        #region IPipeConnectionListener Members

        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.PROVIDER_CONNECT_PUSH:
                    if (evt.Provider != this)
                    {
                        if (_waiting)
                        {
                            _schedulingService.RemoveScheduledJob(_waitLiveJob);
                            _waitLiveJob = null;
                            _waiting = false;
                        }
                        SendPublishedStatus(_currentItem);
                    }
                    break;
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    if (_isPullMode)
                    {
                        SendStopStatus(_currentItem);
                    }
                    else
                    {
                        SendUnpublishedStatus(_currentItem);
                    }
                    break;
                case PipeConnectionEvent.CONSUMER_CONNECT_PULL:
                    if (evt.Consumer == this)
                    {
                        _isPullMode = true;
                    }
                    break;
                case PipeConnectionEvent.CONSUMER_CONNECT_PUSH:
                    if (evt.Consumer == this)
                    {
                        _isPullMode = false;
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region ITokenBucketCallback Members

        public void Available(ITokenBucket bucket, long tokenCount)
        {
            lock (this.SyncRoot)
            {
                _waitingForToken = false;
                _checkBandwidth = false;
                try
                {
                    PullAndPush();
                }
                catch (Exception ex)
                {
                    log.Error("Error while pulling message.", ex);
                }
                _checkBandwidth = true;
            }
        }

        public void Reset(ITokenBucket bucket, long tokenCount)
        {
            _waitingForToken = false;
        }

        #endregion

        /// <summary>
        /// Send message to output stream and handle exceptions.
        /// </summary>
        /// <param name="message"></param>
        private void DoPushMessage(AsyncMessage message)
        {
            try
            {
                _msgOut.PushMessage(message);
                if (message is RtmpMessage)
                {
                    IRtmpEvent body = ((RtmpMessage)message).body;
                    if (body is IStreamData && ((IStreamData)body).Data != null)
                    {
                        _bytesSent += ((IStreamData)body).Data.Limit;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }

        /// <summary>
        /// Send RTMP message
        /// </summary>
        /// <param name="message"></param>
        private void SendMessage(RtmpMessage message)
        {
            //TDJ / live relative timestamp
            if (_playDecision == 0 && _streamStartTS > 0)
            {
                message.body.Timestamp = message.body.Timestamp - _streamStartTS;
            }
            int ts = message.body.Timestamp;
            if( log.IsDebugEnabled )
                log.Debug(string.Format("SendMessage: streamStartTS={0}, length={1}, streamOffset={2}, timestamp={3}", _streamStartTS, _currentItem.Length, _streamOffset, ts));		
            if (_streamStartTS == -1)
            {
                if (log.IsDebugEnabled)
                    log.Debug("SendMessage: resetting streamStartTS");
                _streamStartTS = ts;
                message.body.Timestamp = 0;
            }
            else
            {
                if (_currentItem.Length >= 0)
                {
                    int duration = ts - _streamStartTS;
                    if (duration - _streamOffset >= _currentItem.Length)
                    {
                        // Sent enough data to client
                        Stop();
                        return;
                    }
                }
            }
            _lastMessage = message.body;
            DoPushMessage(message);
        }

        /// <summary>
        /// Send clear ping, that is, just to check if connection is alive
        /// </summary>
        private void SendClearPing()
        {
            Ping ping1 = new Ping();
            ping1.PingType = (short)Ping.StreamPlayBufferClear;
            ping1.Value2 = this.StreamId;
            RtmpMessage ping1Msg = new RtmpMessage();
            ping1Msg.body = ping1;
            DoPushMessage(ping1Msg);
        }

        /// <summary>
        /// Send reset message
        /// </summary>
        private void SendReset()
        {
            if (_isPullMode)
            {
                Ping ping1 = new Ping();
                ping1.PingType = (short)Ping.RecordedStream;
                ping1.Value2 = this.StreamId;

                RtmpMessage ping1Msg = new RtmpMessage();
                ping1Msg.body = ping1;
                DoPushMessage(ping1Msg);
            }

            Ping ping2 = new Ping();
            ping2.PingType = (short)Ping.StreamBegin;
            ping2.Value2 = this.StreamId;

            RtmpMessage ping2Msg = new RtmpMessage();
            ping2Msg.body = ping2;
            DoPushMessage(ping2Msg);

            ResetMessage reset = new ResetMessage();
            DoPushMessage(reset);
        }

        /// <summary>
        /// Send reset status for item
        /// </summary>
        /// <param name="item"></param>
        private void SendResetStatus(IPlayItem item)
        {
            StatusASO reset = new StatusASO(StatusASO.NS_PLAY_RESET);
            reset.clientid = this.StreamId;
            reset.details = item.Name;
            reset.description = "Playing and resetting " + item.Name + '.';
            StatusMessage resetMsg = new StatusMessage();
            resetMsg.body = reset;
            DoPushMessage(resetMsg);
        }

        /// <summary>
        /// Send playback start status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendStartStatus(IPlayItem item)
        {
            StatusASO start = new StatusASO(StatusASO.NS_PLAY_START);
            start.clientid = this.StreamId;
            start.details = item.Name;
            start.description = "Started playing " + item.Name + '.';

            StatusMessage startMsg = new StatusMessage();
            startMsg.body = start;
            DoPushMessage(startMsg);
        }

        /// <summary>
        /// Send playback stoppage status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendStopStatus(IPlayItem item)
        {
            StatusASO stop = new StatusASO(StatusASO.NS_PLAY_STOP);
            stop.clientid = this.StreamId;
            stop.description = "Stopped playing " + item.Name + ".";
            stop.details = item.Name;

            StatusMessage stopMsg = new StatusMessage();
            stopMsg.body = stop;
            DoPushMessage(stopMsg);
        }

        private void SendOnPlayStatus(String code, int duration, long bytes)
        {
            MemoryStream ms = new MemoryStream();
            AMFWriter writer = new AMFWriter(ms);
            writer.WriteString("onPlayStatus");
            Hashtable props = new Hashtable();
            props.Add("code", code);
            props.Add("level", "status");
            props.Add("duration", duration);
            props.Add("bytes", bytes);
            writer.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            ByteBuffer buffer = new ByteBuffer(ms);
            IRtmpEvent evt = new Notify(buffer);
            if (_lastMessage != null)
            {
                int timestamp = _lastMessage.Timestamp;
                evt.Timestamp = timestamp;
            }
            else
            {
                evt.Timestamp = 0;
            }
            RtmpMessage msg = new RtmpMessage();
            msg.body = evt;
            DoPushMessage(msg);
        }

        /// <summary>
        /// Send playlist switch status notification
        /// </summary>
        private void SendSwitchStatus()
        {
            // TODO: find correct duration to sent
            int duration = 1;
            SendOnPlayStatus(StatusASO.NS_PLAY_SWITCH, duration, _bytesSent);
        }

        /// <summary>
        /// Send playlist complete status notification
        /// </summary>
        private void SendCompleteStatus()
        {
            // TODO: find correct duration to sent
            int duration = 1;
            SendOnPlayStatus(StatusASO.NS_PLAY_COMPLETE, duration, _bytesSent);
        }

        /// <summary>
        /// Send seek status notification
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        private void SendSeekStatus(IPlayItem item, int position)
        {
            StatusASO seek = new StatusASO(StatusASO.NS_SEEK_NOTIFY);
            seek.clientid = this.StreamId;
            seek.details = item.Name;
            seek.description = "Seeking " + position + " (stream ID: " + this.StreamId + ").";
            StatusMessage seekMsg = new StatusMessage();
            seekMsg.body = seek;
            DoPushMessage(seekMsg);
        }

        /// <summary>
        /// Send pause status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendPauseStatus(IPlayItem item)
        {
            StatusASO pause = new StatusASO(StatusASO.NS_PAUSE_NOTIFY);
            pause.clientid = this.StreamId;
            pause.details = item.Name;

            StatusMessage pauseMsg = new StatusMessage();
            pauseMsg.body = pause;
            DoPushMessage(pauseMsg);
        }

        /// <summary>
        /// Send resume status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendResumeStatus(IPlayItem item)
        {
            StatusASO resume = new StatusASO(StatusASO.NS_UNPAUSE_NOTIFY);
            resume.clientid = this.StreamId;
            resume.details = item.Name;

            StatusMessage resumeMsg = new StatusMessage();
            resumeMsg.body = resume;
            DoPushMessage(resumeMsg);
        }

        /// <summary>
        /// Send published status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendPublishedStatus(IPlayItem item)
        {
            StatusASO published = new StatusASO(StatusASO.NS_PLAY_PUBLISHNOTIFY);
            published.clientid = this.StreamId;
            published.details = item.Name;

            StatusMessage unpublishedMsg = new StatusMessage();
            unpublishedMsg.body = published;
            DoPushMessage(unpublishedMsg);
        }

        /// <summary>
        /// Send unpublished status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendUnpublishedStatus(IPlayItem item)
        {
            StatusASO unpublished = new StatusASO(StatusASO.NS_PLAY_UNPUBLISHNOTIFY);
            unpublished.clientid = this.StreamId;
            unpublished.details = item.Name;

            StatusMessage unpublishedMsg = new StatusMessage();
            unpublishedMsg.body = unpublished;
            DoPushMessage(unpublishedMsg);
        }

        /// <summary>
        /// Stream not found status notification
        /// </summary>
        /// <param name="item"></param>
        private void SendStreamNotFoundStatus(IPlayItem item)
        {
            StatusASO notFound = new StatusASO(StatusASO.NS_PLAY_STREAMNOTFOUND);
            notFound.clientid = this.StreamId;
            notFound.level = StatusASO.ERROR;
            notFound.details = item.Name;

            StatusMessage notFoundMsg = new StatusMessage();
            notFoundMsg.body = notFound;
            DoPushMessage(notFoundMsg);
        }

        /// <summary>
        /// Insufficient bandwidth notification
        /// </summary>
        /// <param name="item"></param>
        private void SendInsufficientBandwidthStatus(IPlayItem item)
        {
            StatusASO insufficientBW = new StatusASO(StatusASO.NS_PLAY_INSUFFICIENT_BW);
            insufficientBW.clientid = this.StreamId;
            insufficientBW.level = StatusASO.WARNING;
            insufficientBW.details = item.Name;
            insufficientBW.description = "Data is playing behind the normal speed.";

            StatusMessage insufficientBWMsg = new StatusMessage();
            insufficientBWMsg.body = insufficientBW;
            DoPushMessage(insufficientBWMsg);
        }

        /// <summary>
        /// Send VOD init control message
        /// </summary>
        /// <param name="msgIn"></param>
        /// <param name="item"></param>
        private void SendVODInitCM(IMessageInput msgIn, IPlayItem item)
        {
            OOBControlMessage oobCtrlMsg = new OOBControlMessage();
            oobCtrlMsg.Target = typeof(IPassive).Name;
            oobCtrlMsg.ServiceName = "init";
            oobCtrlMsg.ServiceParameterMap.Add("startTS", item.Start);
            _msgIn.SendOOBControlMessage(this, oobCtrlMsg);
        }

        /// <summary>
        /// Send VOD seek control message
        /// </summary>
        /// <param name="msgIn"></param>
        /// <param name="position"></param>
        /// <returns></returns>            
        private int SendVODSeekCM(IMessageInput msgIn, int position)
        {
            OOBControlMessage oobCtrlMsg = new OOBControlMessage();
            oobCtrlMsg.Target = typeof(ISeekableProvider).Name;
            oobCtrlMsg.ServiceName = "seek";
            oobCtrlMsg.ServiceParameterMap.Add("position", position);
            msgIn.SendOOBControlMessage(this, oobCtrlMsg);
            if (oobCtrlMsg.Result is int)
            {
                return (int)oobCtrlMsg.Result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Send VOD check video control message
        /// </summary>
        /// <param name="msgIn"></param>
        /// <returns></returns>
        private bool SendCheckVideoCM(IMessageInput msgIn)
        {
            OOBControlMessage oobCtrlMsg = new OOBControlMessage();
            oobCtrlMsg.Target = typeof(IStreamTypeAwareProvider).Name;
            oobCtrlMsg.ServiceName = "hasVideo";
            msgIn.SendOOBControlMessage(this, oobCtrlMsg);
            if (oobCtrlMsg.Result is Boolean)
            {
                return (Boolean)oobCtrlMsg.Result;
            }
            else
            {
                return false;
            }
        }
    }
}
