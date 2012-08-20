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
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
//using FluorineFx.Messaging.Rtmp.IO;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream.Consumer
{
    /// <summary>
    /// RTMP connection consumer.
    /// </summary>
    class ConnectionConsumer : IPushableConsumer, IPipeConnectionListener
    {
#if !SILVERLIGHT
        private static ILog log = LogManager.GetLogger(typeof(ConnectionConsumer));
#endif
        /// <summary>
        /// Connection object.
        /// </summary>
        private RtmpConnection _connection;
        /// <summary>
        /// Video channel.
        /// </summary>
        private RtmpChannel _video;
        /// <summary>
        /// Audio channel.
        /// </summary>
        private RtmpChannel _audio;
        /// <summary>
        /// Data channel.
        /// </summary>
        private RtmpChannel _data;
        /// <summary>
        /// Chunk size. Packets are sent chunk-by-chunk.
        /// </summary>
        private int _chunkSize = 1024; //TODO: Not sure of the best value here
        /// <summary>
        /// Whether or not the chunk size has been sent. This seems to be required for h264.
        /// </summary>
        private bool _chunkSizeSent;

        /// <summary>
        /// Modifies time stamps on messages as needed.
        /// </summary>
        private TimeStamper _timeStamper;

        public ConnectionConsumer(RtmpConnection connection, int videoChannel, int audioChannel, int dataChannel)
        {
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
                log.Debug(string.Format("Channel ids - video: {0} audio: {1} data: {2}", videoChannel, audioChannel, dataChannel));
#endif
            _connection = connection;
            _video = connection.GetChannel(videoChannel);
            _audio = connection.GetChannel(audioChannel);
            _data = connection.GetChannel(dataChannel);
            _timeStamper = new TimeStamper();
        }


        #region IPushableConsumer Members

        public void PushMessage(IPipe pipe, IMessage message)
        {
		    if (message is ResetMessage) 
            {
			    _timeStamper.Reset();
            } 
            else if (message is StatusMessage) 
            {
			    StatusMessage statusMsg = message as StatusMessage;
			    _data.SendStatus(statusMsg.body as StatusASO);
		    }
            else if (message is RtmpMessage)
            {
                // Make sure chunk size has been sent
                if (!_chunkSizeSent)
                    SendChunkSize();

                RtmpMessage rtmpMsg = message as RtmpMessage;
                IRtmpEvent msg = rtmpMsg.body;

                int eventTime = msg.Timestamp;
#if !SILVERLIGHT
                if(log.IsDebugEnabled)
                    log.Debug(string.Format("Message timestamp: {0}", eventTime));
#endif
                if (eventTime < 0)
                {
#if !SILVERLIGHT
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Message has negative timestamp: {0}", eventTime));
#endif
                    return;
                }
                byte dataType = msg.DataType;
                // Create a new header for the consumer
                RtmpHeader header = _timeStamper.GetTimeStamp(dataType, eventTime);

                switch (msg.DataType)
                {
                    case Constants.TypeStreamMetadata:
                        Notify notify = new Notify((msg as Notify).Data);
                        notify.Header = header;
                        notify.Timestamp = header.Timer;
                        _data.Write(notify);
                        break;
                    case Constants.TypeFlexStreamEnd:
                        // TODO: okay to send this also to AMF0 clients?
                        FlexStreamSend send = new FlexStreamSend((msg as Notify).Data);
                        send.Header = header;
                        send.Timestamp = header.Timer;
                        _data.Write(send);
                        break;
                    case Constants.TypeVideoData:
                        VideoData videoData = new VideoData((msg as VideoData).Data);
                        videoData.Header = header;
                        videoData.Timestamp = header.Timer;
                        _video.Write(videoData);
                        break;
                    case Constants.TypeAudioData:
                        AudioData audioData = new AudioData((msg as AudioData).Data);
                        audioData.Header = header;
                        audioData.Timestamp = header.Timer;
                        _audio.Write(audioData);
                        break;
                    case Constants.TypePing:
                        Ping ping = new Ping((msg as Ping).PingType, (msg as Ping).Value2, (msg as Ping).Value3, (msg as Ping).Value4);
                        ping.Header = header;
                        _connection.Ping(ping);
                        break;
                    case Constants.TypeBytesRead:
                        BytesRead bytesRead = new BytesRead((msg as BytesRead).Bytes);
                        bytesRead.Header = header;
                        bytesRead.Timestamp = header.Timer;
                        _connection.GetChannel((byte)2).Write(bytesRead);
                        break;
                    default:
                        _data.Write(msg);
                        break;
                }
            }
        }

        #endregion

        #region IMessageComponent Members

        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
            if (!"ConnectionConsumer".Equals(oobCtrlMsg.Target))
                return;

            if ("pendingCount".Equals(oobCtrlMsg.ServiceName))
            {
                oobCtrlMsg.Result = _connection.PendingMessages;
            }
            else if ("pendingVideoCount".Equals(oobCtrlMsg.ServiceName))
            {
                IClientStream stream = null;
                if (_connection is IStreamCapableConnection)
                    stream = (_connection as IStreamCapableConnection).GetStreamByChannelId(_video.ChannelId);
                if (stream != null)
                {
                    oobCtrlMsg.Result = _connection.GetPendingVideoMessages(stream.StreamId);
                }
                else
                {
                    oobCtrlMsg.Result = (long)0;
                }
            }
            else if ("writeDelta".Equals(oobCtrlMsg.ServiceName))
            {
                long maxStream = 0;
                IBWControllable bwControllable = _connection as IBWControllable;
                // Search FC containing valid BWC
                while (bwControllable != null && bwControllable.BandwidthConfiguration == null)
                {
                    bwControllable = bwControllable.GetParentBWControllable();
                }
                if (bwControllable != null && bwControllable.BandwidthConfiguration != null)
                {
                    IBandwidthConfigure bwc = bwControllable.BandwidthConfiguration;
                    if (bwc is IConnectionBWConfig)
                    {
                        maxStream = (bwc as IConnectionBWConfig).DownstreamBandwidth / 8;
                    }
                }
                if (maxStream <= 0)
                {
                    // Use default value
                    // TODO: this should be configured somewhere and sent to the client when connecting
                    maxStream = 120 * 1024;
                }
                // Return the current delta between sent bytes and bytes the client
                // reported to have received, and the interval the client should use
                // for generating BytesRead messages (half of the allowed bandwidth).
                oobCtrlMsg.Result = new long[] { _connection.WrittenBytes - _connection.ClientBytesRead, maxStream / 2 };
            }
            else if ("chunkSize".Equals(oobCtrlMsg.ServiceName))
            {
                int newSize = (int)oobCtrlMsg.ServiceParameterMap["chunkSize"];
                if (newSize != _chunkSize)
                {
                    _chunkSize = newSize;
                    SendChunkSize();
                }
            }
        }

        #endregion

        #region IPipeConnectionListener Members

        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    // XXX should put the channel release code in ConsumerService
                    _connection.CloseChannel(_video.ChannelId);
                    _connection.CloseChannel(_audio.ChannelId);
                    _connection.CloseChannel(_data.ChannelId);
                    break;
                default:
                    break;
            }
        }

        #endregion

        private class TimeStamper
        {
            /// <summary>
            /// Stores timestamp for last event.
            /// </summary>
            private int _lastEventTime = 0;
            /// <summary>
            /// Timestamp of last audio packet.
            /// </summary>
            private int _lastAudioTime = 0;
            /// <summary>
            /// Timestamp of last video packet.
            /// </summary>
            private int _lastVideoTime = 0;
            /// <summary>
            /// Timestamp of last notify or invoke.
            /// </summary>
            private int _lastNotifyTime = 0;

            /// <summary>
            /// Reset timestamps.
            /// </summary>
            public void Reset()
            {
#if !SILVERLIGHT
                if( log.IsDebugEnabled )
                    log.Debug("Reset timestamps");
#endif
                _lastEventTime = 0;
                _lastAudioTime = 0;
                _lastNotifyTime = 0;
                _lastVideoTime = 0;
            }

            /// <summary>
            /// Gets a header with the appropriate timestamp.
            /// </summary>
            /// <param name="dataType">Type of the event.</param>
            /// <param name="eventTime">The event time.</param>
            /// <returns></returns>
            public RtmpHeader GetTimeStamp(byte dataType, int eventTime)
            {
                RtmpHeader header = new RtmpHeader();
#if !SILVERLIGHT
                if( log.IsDebugEnabled )
                    log.Debug(string.Format("GetTimeStamp - event time: {0} last event: {1} audio: {2} video: {3} data: {4}", eventTime, _lastEventTime, _lastAudioTime, _lastVideoTime, _lastNotifyTime));
#endif
                switch (dataType)
                {
                    case Constants.TypeAudioData:
                        if (_lastAudioTime > 0)
                        {
                            //set a relative value
                            header.Timer = eventTime - _lastAudioTime;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Relative audio");
#endif
                        }
                        else
                        {
                            //use absolute value
                            header.Timer = eventTime;
                            header.IsTimerRelative = false;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Absolute audio");
#endif
                        }
                        _lastAudioTime = eventTime;
                        break;
                    case Constants.TypeVideoData:
                        if (_lastVideoTime > 0)
                        {
                            //set a relative value
                            header.Timer = eventTime - _lastVideoTime;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Relative video");
#endif
                        }
                        else
                        {
                            //use absolute value
                            header.Timer = eventTime;
                            header.IsTimerRelative = false;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Absolute video");
#endif
                        }
                        _lastVideoTime = eventTime;
                        break;
                    case Constants.TypeNotify:
                    case Constants.TypeInvoke:
                    case Constants.TypeFlexStreamEnd:
                        if (_lastNotifyTime > 0)
                        {
                            //set a relative value
                            header.Timer = eventTime - _lastNotifyTime;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Relative notify");
#endif
                        }
                        else
                        {
                            //use absolute value
                            header.Timer = eventTime;
                            header.IsTimerRelative = false;
#if !SILVERLIGHT
                            if (log.IsDebugEnabled)
                                log.Debug("Absolute notify");
#endif
                        }
                        _lastNotifyTime = eventTime;
                        break;
                    case Constants.TypeBytesRead:
                    case Constants.TypePing:
                        header.Timer = eventTime;
                        header.IsTimerRelative = false;
                        break;
                    default:
                        // ignore other types
#if !SILVERLIGHT
                        if (log.IsDebugEnabled)
                            log.Debug(string.Format("Unmodified type: {0} timestamp: {1}", dataType, eventTime));
#endif
                        break;
                }
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug(string.Format("Event time: {0} current ts: {1}", eventTime, header.Timer));
#endif
                _lastEventTime = eventTime;
                return header;
            }
        }

        /// <summary>
        /// Send the chunk size.
        /// </summary>
        private void SendChunkSize()
        {
#if !SILVERLIGHT
            if(log.IsDebugEnabled )
                log.Debug(string.Format("Sending chunk size: {0}", _chunkSize));
#endif
            ChunkSize chunkSizeMsg = new ChunkSize(_chunkSize);
            _connection.GetChannel((byte)2).Write(chunkSizeMsg);
            _chunkSizeSent = true;
        }    
    }
}
