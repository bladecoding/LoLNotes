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
using System.IO;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using log4net;
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
using FluorineFx.Collections;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Represents live stream broadcasted from client. As Flash Media Server, supports
    /// recording mode for live streams, that is, broadcasted stream has broadcast mode. It can be either
    /// "live" or "record" and latter causes server-side application to record broadcasted stream.
    /// 
    /// Note that recorded streams are recorded as FLV files. The same is correct for audio, because
    /// NellyMoser codec that Flash Player uses prohibits on-the-fly transcoding to audio formats like MP3
    /// without paying of licensing fee or buying SDK.
    /// 
    /// This type of stream uses two different pipes for live streaming and recording.
    /// </summary>
    class ClientBroadcastStream : AbstractClientStream,
        IClientBroadcastStream, IFilter, IPushableConsumer, IPipeConnectionListener, IEventDispatcher, IClientBroadcastStreamStatistics
    {
        private static ILog log = LogManager.GetLogger(typeof(ClientBroadcastStream));

        /// <summary>
        /// Stores absolute time for video stream.
        /// </summary>
        private int _audioTime = -1;
        /// <summary>
        /// Total number of bytes received.
        /// </summary>
        private long _bytesReceived;
        /// <summary>
        /// Is there need to check video codec?
        /// </summary>
        private bool _checkVideoCodec = false;
        /// <summary>
        /// Data is sent by chunks, each of them has size.
        /// </summary>
        private int _chunkSize = 0;
        /// <summary>
        /// Is this stream still active?
        /// </summary>
        private bool _closed = false;
        /// <summary>
        /// Output endpoint that providers use.
        /// </summary>
        private IMessageOutput _connMsgOut;
        /// <summary>
        /// Stores absolute time for data stream.
        /// </summary>
        private int _dataTime = -1;
        /// <summary>
        /// Stores timestamp of first packet.
        /// </summary>
        private int _firstPacketTime = -1;
        /// <summary>
        /// Pipe for live streaming
        /// </summary>
        private IPipe _livePipe;
        /// <summary>
        /// Stream published name.
        /// </summary>
        private string _publishedName;
        /// <summary>
        /// Whether we are recording or not.
        /// </summary>
        private bool _recording = false;
        /// <summary>
        /// FileConsumer used to output recording to disk.
        /// </summary>
        private FileConsumer _recordingFile;
        /// <summary>
        /// The filename we are recording to.
        /// </summary>
        private string _recordingFilename;
        /// <summary>
        /// Pipe for recording.
        /// </summary>
        private IPipe _recordPipe;
        /// <summary>
        /// Is there need to send start notification?
        /// </summary>
        private bool _sendStartNotification = true;
        /// <summary>
        /// Stores statistics about subscribers.
        /// </summary>
        private StatisticsCounter _subscriberStats = new StatisticsCounter();
        /// <summary>
        /// Factory object for video codecs.
        /// </summary>
        private VideoCodecFactory _videoCodecFactory = null;
        /// <summary>
        /// Stores absolute time for audio stream.
        /// </summary>
        private int _videoTime = -1;
        /// <summary>
        /// Minimum stream time
        /// </summary>
        private int _minStreamTime = 0;
        /// <summary>
        /// Listeners to get notified about received packets.
        /// Set(IStreamListener)
        /// </summary>
        private CopyOnWriteArraySet _listeners = new CopyOnWriteArraySet();

        /// <summary>
        /// Gets or sets minimum stream time.
        /// </summary>
        public int MinStreamTime
        {
            get { return _minStreamTime; }
            set { _minStreamTime = value; }
        }

	    private void CheckSendNotifications(IEvent evt) 
        {
		    IEventListener source = evt.Source;
		    SendStartNotifications(source);
	    }

	    private void SendStartNotifications(IEventListener source) 
        {
		    if (_sendStartNotification) 
            {
			    // Notify handler that stream starts recording/publishing
			    _sendStartNotification = false;
			    if (source is IConnection) 
                {
				    IScope scope = (source as IConnection).Scope;
				    if (scope.HasHandler) 
                    {
					    Object handler = scope.Handler;
					    if (handler is IStreamAwareScopeHandler) 
                        {
						    if (_recording) 
                            {
                                (handler as IStreamAwareScopeHandler).StreamRecordStart(this);
						    } 
                            else 
                            {
                                (handler as IStreamAwareScopeHandler).StreamPublishStart(this);
						    }
					    }
				    }
			    }
			    // Send start notifications
			    SendPublishStartNotify();
			    if (_recording) 
                {
				    SendRecordStartNotify();
			    }
			    NotifyBroadcastStart();
		    }
	    }
        /// <summary>
        /// Sends publish start notifications.
        /// </summary>
        private void SendPublishStartNotify()
        {
            StatusASO publishStatus = new StatusASO(StatusASO.NS_PUBLISH_START);
            publishStatus.clientid = this.StreamId;
            publishStatus.details = this.PublishedName;
            StatusMessage startMsg = new StatusMessage();
            startMsg.body = publishStatus;
            try
            {
                _connMsgOut.PushMessage(startMsg);
            }
            catch(System.IO.IOException ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }
        /// <summary>
        /// Sends publish stop notifications.
        /// </summary>
        private void SendPublishStopNotify()
        {
            StatusASO stopStatus = new StatusASO(StatusASO.NS_UNPUBLISHED_SUCCESS);
            stopStatus.clientid = this.StreamId;
            stopStatus.details = this.PublishedName;

            StatusMessage stopMsg = new StatusMessage();
            stopMsg.body = stopStatus;
            try
            {
                _connMsgOut.PushMessage(stopMsg);
            }
            catch (System.IO.IOException ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }
        /// <summary>
        /// Sends record start notifications.
        /// </summary>
        private void SendRecordStartNotify()
        {
            StatusASO recordStatus = new StatusASO(StatusASO.NS_RECORD_START);
            recordStatus.clientid = this.StreamId;
            recordStatus.details = this.PublishedName;

            StatusMessage startMsg = new StatusMessage();
            startMsg.body = recordStatus;
            try
            {
                _connMsgOut.PushMessage(startMsg);
            }
            catch (System.IO.IOException ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }
        /// <summary>
        /// Sends record stop notifications.
        /// </summary>
        private void SendRecordStopNotify()
        {
            StatusASO stopStatus = new StatusASO(StatusASO.NS_RECORD_STOP);
            stopStatus.clientid = this.StreamId;
            stopStatus.details = this.PublishedName;

            StatusMessage startMsg = new StatusMessage();
            startMsg.body = stopStatus;
            try
            {
                _connMsgOut.PushMessage(startMsg);
            }
            catch (System.IO.IOException ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }
        /// <summary>
        /// Sends record failed notifications.
        /// </summary>
        /// <param name="reason"></param>
        private void SendRecordFailedNotify(string reason)
        {
            StatusASO failedStatus = new StatusASO(StatusASO.NS_RECORD_FAILED);
            failedStatus.level = StatusASO.ERROR;
            failedStatus.clientid = this.StreamId;
            failedStatus.details = this.PublishedName;
            failedStatus.description = reason;

            StatusMessage failedMsg = new StatusMessage();
            failedMsg.body = failedStatus;
            try
            {
                _connMsgOut.PushMessage(failedMsg);
            }
            catch (IOException ex)
            {
                log.Error("Error while pushing message.", ex);
            }
        }
        /// <summary>
        /// Notifies handler on stream broadcast start.
        /// </summary>
        private void NotifyBroadcastStart()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamBroadcastStart(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamBroadcastStart", ex);
                }
            }
        }
        /// <summary>
        /// Notifies handler on stream broadcast stop.
        /// </summary>
        private void NotifyBroadcastClose()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamBroadcastClose(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamBroadcastStop", ex);
                }
            }
        }
        /// <summary>
        /// Send OOB control message with chunk size.
        /// </summary>
        private void NotifyChunkSize()
        {
            if (_chunkSize > 0 && _livePipe != null)
            {
                OOBControlMessage setChunkSize = new OOBControlMessage();
                setChunkSize.Target = "ConnectionConsumer";
                setChunkSize.ServiceName = "chunkSize";
                setChunkSize.ServiceParameterMap["chunkSize"] = _chunkSize;
                _livePipe.SendOOBControlMessage(this.Provider, setChunkSize);
            }
        }
        /// <summary>
        /// Closes stream, unsubscribes provides, sends stoppage notifications and broadcast close notification.
        /// </summary>
        public override void Close()
        {
            lock (this.SyncRoot)
            {
                if (_closed)
                    return;// Already closed
                _closed = true;
                if (_livePipe != null)
                    _livePipe.Unsubscribe(this as IProvider);
                if (_recordPipe != null)
                    _recordPipe.Unsubscribe(this as IProvider);
                if (_recording)
                    SendRecordStopNotify();
                SendPublishStopNotify();
                // TODO: can we send the client something to make sure he stops sending data?
                _connMsgOut.Unsubscribe(this);
                NotifyBroadcastClose();
            }
        }


        #region IClientBroadcastStream Members

        public void StartPublishing()
        {
            // We send the start messages before the first packet is received.
            // This is required so FME actually starts publishing.
            SendStartNotifications(FluorineFx.Context.FluorineContext.Current.Connection);
        }

        public IClientBroadcastStreamStatistics Statistics
        {
            get { return this; }
        }

        #endregion

        #region IBroadcastStream Members

        /// <summary>
        /// Saves broadcasted stream.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAppend"></param>
        public void SaveAs(string name, bool isAppend)
        {
            if (log.IsDebugEnabled)
                log.Debug("SaveAs - name: " + name + " append: " + isAppend);
            // Get stream scope
            IStreamCapableConnection connection = this.Connection;
            if (connection == null)
            {
                // TODO: throw other exception here?
                throw new IOException("Stream is no longer connected");
            }
            IScope scope = connection.Scope;
            // Get stream filename generator
            IStreamFilenameGenerator generator = ScopeUtils.GetScopeService(scope, typeof(IStreamFilenameGenerator)) as IStreamFilenameGenerator;
            // Generate filename
            string filename = generator.GenerateFilename(scope, name, ".flv", GenerationType.RECORD);
            // Get file for that filename
            FileInfo file;
            if (generator.ResolvesToAbsolutePath)
                file = new FileInfo(filename);
            else
                file = scope.Context.GetResource(filename).File;
            // If append mode is on...
            if (!isAppend)
            {
                if (file.Exists)
                {
                    // Per livedoc of FCS/FMS:
                    // When "live" or "record" is used,
                    // any previously recorded stream with the same stream URI is deleted.
                    file.Delete();
                }
            }
            else
            {
                if (!file.Exists)
                {
                    // Per livedoc of FCS/FMS:
                    // If a recorded stream at the same URI does not already exist,
                    // "append" creates the stream as though "record" was passed.
                    isAppend = false;
                }
            }
            //Requery
            file = new FileInfo(file.FullName);
            if (!file.Exists)
            {
                // Make sure the destination directory exists
                string directory = Path.GetDirectoryName(file.FullName);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            if (!file.Exists)
            {
                using (FileStream fs = file.Create()) { }
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("Recording file: " + file.FullName);
            }
            _recordingFile = new FileConsumer(scope, file);
#if !(NET_1_1)
            Dictionary<string, object> parameterMap = new Dictionary<string, object>();
#else
            Hashtable parameterMap = new Hashtable();
#endif
            if (isAppend)
            {
                parameterMap.Add("mode", "append");
            }
            else
            {
                parameterMap.Add("mode", "record");
            }
            _recordPipe.Subscribe(_recordingFile, parameterMap);
            _recording = true;
            _recordingFilename = filename;
        }

        public string SaveFilename
        {
            get { return _recordingFilename; }
        }

        /// <summary>
        /// Name that used for publishing. Set at client side when begin to broadcast with NetStream#publish.
        /// </summary>
        public string PublishedName
        {
            get
            {
                return _publishedName;
            }
            set
            {
		        if (log.IsDebugEnabled)
			        log.Debug("setPublishedName: " + value);
		        _publishedName = value;
            }
        }

        public IProvider Provider
        {
            get { return this; }
        }
        /// <summary>
        /// Add a listener to be notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddStreamListener(IStreamListener listener)
        {
            _listeners.Add(listener);
        }
        /// <summary>
        /// Return registered stream listeners.
        /// </summary>
        /// <returns>The registered listeners.</returns>
        public ICollection GetStreamListeners()
        {
            return _listeners;
        }
        /// <summary>
        /// Remove a listener from being notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveStreamListener(IStreamListener listener)
        {
            _listeners.Remove(listener);
        }

        #endregion

        #region IMessageComponent Members

        /// <summary>
        /// Out-of-band control message handler.
        /// </summary>
        /// <param name="source">OOB message source.</param>
        /// <param name="pipe">Pipe that used to send OOB message.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message.</param>
        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
            if (!"ClientBroadcastStream".Equals(oobCtrlMsg.Target))
                return;
            if ("chunkSize".Equals(oobCtrlMsg.ServiceName))
            {
                _chunkSize = (int)oobCtrlMsg.ServiceParameterMap["chunkSize"];
                NotifyChunkSize();
            }
        }

        #endregion

        #region IPushableConsumer Members

        public void PushMessage(IPipe pipe, IMessage message)
        {
            //not implemented
        }

        #endregion

        #region IPipeConnectionListener Members

        /// <summary>
        /// Pipe connection event handler.
        /// </summary>
        /// <param name="evt">Pipe connection event.</param>
        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.PROVIDER_CONNECT_PUSH:
                    if (evt.Provider == this && evt.Source != _connMsgOut && (evt.ParameterMap == null || !evt.ParameterMap.ContainsKey("record")))
                    {
                        _livePipe = evt.Source as IPipe;
                        foreach (IConsumer consumer in _livePipe.GetConsumers())
                        {
                            _subscriberStats.Increment();
                        }
                    }
                    break;
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    if (_livePipe == evt.Source)
                    {
                        _livePipe = null;
                    }
                    break;
                case PipeConnectionEvent.CONSUMER_CONNECT_PUSH:
                    if (_livePipe == evt.Source)
                    {
                        NotifyChunkSize();
                    }
                    _subscriberStats.Increment();
                    break;
                case PipeConnectionEvent.CONSUMER_DISCONNECT:
                    _subscriberStats.Decrement();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region IEventDispatcher Members

        public void DispatchEvent(IEvent evt)
        {
            if (!(evt is IRtmpEvent)
                    && (evt.EventType != EventType.STREAM_CONTROL)
                    && (evt.EventType != EventType.STREAM_DATA) || _closed)
            {
                // ignored event
                if (log.IsDebugEnabled)
                {
                    log.Debug("DispatchEvent: " + evt.EventType);
                }
                return;
            }

            // Get stream codec
            IStreamCodecInfo codecInfo = this.CodecInfo;
            StreamCodecInfo info = null;
            if (codecInfo is StreamCodecInfo)
            {
                info = codecInfo as StreamCodecInfo;
            }

            IRtmpEvent rtmpEvent = evt as IRtmpEvent;
            if (rtmpEvent == null)
            {
                if (log.IsDebugEnabled)
                    log.Debug("IRtmpEvent expected in event dispatch");
                return;
            }
            int eventTime = -1;
            // If this is first packet save it's timestamp
            if (_firstPacketTime == -1)
            {
                _firstPacketTime = rtmpEvent.Timestamp;
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("CBS: {0} firstPacketTime={1} {2}", this.Name, _firstPacketTime, rtmpEvent.Header.IsTimerRelative ? "(rel)" : "(abs)"));
            }
            if (rtmpEvent is IStreamData && (rtmpEvent as IStreamData).Data != null)
            {
                _bytesReceived += (rtmpEvent as IStreamData).Data.Limit;
            }

            if (rtmpEvent is AudioData)
            {
                if (info != null)
                {
                    info.HasAudio = true;
                }
                if (rtmpEvent.Header.IsTimerRelative)
                {
				    if (_audioTime == 0)
					    log.Warn(string.Format("First Audio timestamp is relative! {0}", rtmpEvent.Timestamp));
                    _audioTime += rtmpEvent.Timestamp;
                }
                else
                {
                    _audioTime = rtmpEvent.Timestamp;
                }
                eventTime = _audioTime;
            }
            else if (rtmpEvent is VideoData)
            {
                IVideoStreamCodec videoStreamCodec = null;
                if (_videoCodecFactory != null && _checkVideoCodec)
                {
                    videoStreamCodec = _videoCodecFactory.GetVideoCodec((rtmpEvent as VideoData).Data);
                    if (codecInfo is StreamCodecInfo)
                    {
                        (codecInfo as StreamCodecInfo).VideoCodec = videoStreamCodec;
                    }
                    _checkVideoCodec = false;
                }
                else if (codecInfo != null)
                {
                    videoStreamCodec = codecInfo.VideoCodec;
                }

                if (videoStreamCodec != null)
                {
                    videoStreamCodec.AddData((rtmpEvent as VideoData).Data);
                }

                if (info != null)
                {
                    info.HasVideo = true;
                }
                if (rtmpEvent.Header.IsTimerRelative)
                {
                    if (_videoTime == 0)
                        log.Warn(string.Format("First Video timestamp is relative! {0}", rtmpEvent.Timestamp));
                    _videoTime += rtmpEvent.Timestamp;
                }
                else
                {
                    _videoTime = rtmpEvent.Timestamp;
                    // Flash player may send first VideoData with old-absolute timestamp.
                    // This ruins the stream's timebase in FileConsumer.
                    // We don't want to discard the packet, as it may be a video keyframe.
                    // Generally a Data or Audio packet has set the timebase to a reasonable value,
                    // Eventually a new/correct absolute time will come on the video channel.
                    // We could put this logic between livePipe and filePipe;
                    // This would work for Audio Data as well, but have not seen the need.
                    int cts = Math.Max(_audioTime, _dataTime);
                    cts = Math.Max(cts, _minStreamTime);
                    int fudge = 20;
                    // Accept some slightly (20ms) retro timestamps [this may not be needed,
                    // the publish Data should strictly precede the video data]
                    if (_videoTime + fudge < cts)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("DispatchEvent: adjust archaic videoTime, from: {0} to {1}", _videoTime, cts));
                        _videoTime = cts;
                    }
                }
                eventTime = _videoTime;
            }
            else if (rtmpEvent is Invoke)
            {
                if (rtmpEvent.Header.IsTimerRelative)
                {
                    if (_dataTime == 0)
                        log.Warn(string.Format("First data [Invoke] timestamp is relative! {0}", rtmpEvent.Timestamp));
                    _dataTime += rtmpEvent.Timestamp;
                }
                else
                {
                    _dataTime = rtmpEvent.Timestamp;
                }
                return;
            }
            else if (rtmpEvent is Notify)
            {
                if (rtmpEvent.Header.IsTimerRelative)
                {
                    if (_dataTime == 0)
                        log.Warn(string.Format("First data [Notify] timestamp is relative! {0}", rtmpEvent.Timestamp));
                    _dataTime += rtmpEvent.Timestamp;
                }
                else
                {
                    _dataTime = rtmpEvent.Timestamp;
                }
                eventTime = _dataTime;
            }

            // Notify event listeners
            CheckSendNotifications(evt);

            // Create new RTMP message, initialize it and push through pipe
            FluorineFx.Messaging.Rtmp.Stream.Messages.RtmpMessage msg = new FluorineFx.Messaging.Rtmp.Stream.Messages.RtmpMessage();
            msg.body = rtmpEvent;
            msg.body.Timestamp = eventTime;
            try
            {
                if (_livePipe != null)
                    _livePipe.PushMessage(msg);
                if( _recordPipe != null )
                    _recordPipe.PushMessage(msg);
            }
            catch (System.IO.IOException ex)
            {
                SendRecordFailedNotify(ex.Message);
                Stop();
            }

		// Notify listeners about received packet
            if (rtmpEvent is IStreamPacket)
            {
                foreach (IStreamListener listener in GetStreamListeners())
                {
                    try
                    {
                        listener.PacketReceived(this, rtmpEvent as IStreamPacket);
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Error while notifying listener {0}", listener), ex);
                    }
                }
            }
        }

        #endregion

        #region IClientBroadcastStreamStatistics Members


        public int TotalSubscribers
        {
            get { return _subscriberStats.Total; }
        }

        public int MaxSubscribers
        {
            get { return _subscriberStats.Max; }
        }

        public int ActiveSubscribers
        {
            get { return _subscriberStats.Current; }
        }

        public long BytesReceived
        {
            get { return _bytesReceived; }
        }

        #endregion

        #region IStreamStatistics Members

        public int CurrentTimestamp
        {
            get { return Math.Max(Math.Max(_videoTime, _audioTime), _dataTime); }
        }

        #endregion

        #region IStatisticsBase Members

        #endregion


        public override void Start()
        {
            lock (this.SyncRoot)
            {
                IConsumerService consumerManager = this.Scope.GetService(typeof(IConsumerService)) as IConsumerService;
                try
                {
                    //_videoCodecFactory = new VideoCodecFactory();
                    _videoCodecFactory = this.Scope.GetService(typeof(VideoCodecFactory)) as VideoCodecFactory;
                    _checkVideoCodec = true;
                }
                catch (Exception ex)
                {
                    log.Warn("No video codec factory available.", ex);
                }
                _firstPacketTime = _audioTime = _videoTime = _dataTime = -1;
                _connMsgOut = consumerManager.GetConsumerOutput(this);
                _connMsgOut.Subscribe(this, null);
                _recordPipe = new InMemoryPushPushPipe();
#if !(NET_1_1)
                Dictionary<string, object> recordParameterMap = new Dictionary<string, object>();
#else
                Hashtable recordParameterMap = new Hashtable();
#endif
                // Clear record flag
                recordParameterMap.Add("record", null);
                _recordPipe.Subscribe(this as IProvider, recordParameterMap);
                _recording = false;
                _recordingFilename = null;
                this.CodecInfo = new StreamCodecInfo();
                _closed = false;
                _bytesReceived = 0;
                _creationTime = System.Environment.TickCount;
            }
        }

        public override void Stop()
        {
            lock (this.SyncRoot)
            {
                StopRecording();
                Close();
            }
        }
        /// <summary>
        /// Stops any currently active recordings.
        /// </summary>
        public void StopRecording()
        {
            if (_recording)
            {
                _recording = false;
                _recordingFilename = null;
                _recordPipe.Unsubscribe(_recordingFile);
                SendRecordStopNotify();
            }
        }
    }
}
