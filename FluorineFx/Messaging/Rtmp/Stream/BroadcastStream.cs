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
using System.Collections.Generic;
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
using FluorineFx.Collections.Generic;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// An implementation of IBroadcastStream that allows connectionless providers to publish a stream.
    /// </summary>
    [CLSCompliant(false)]
    public class BroadcastStream : AbstractStream, IBroadcastStream, IProvider, IPipeConnectionListener
    {
        private static ILog log = LogManager.GetLogger(typeof(BroadcastStream));
        protected IPipe _livePipe;
        /// <summary>
        /// Factory object for video codecs.
        /// </summary>
        private VideoCodecFactory _videoCodecFactory = null;
        /// <summary>
        /// Listeners to get notified about received packets.
        /// </summary>
        private CopyOnWriteArraySet<IStreamListener> _listeners = new CopyOnWriteArraySet<IStreamListener>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastStream"/> class.
        /// </summary>
        /// <param name="name">The stream name.</param>
        /// <param name="scope">The stream scope.</param>
        public BroadcastStream(String name, IScope scope)
        {
            _name = name;
            _scope = scope;
            _livePipe = null;
            // We want to create a video codec when we get our first video packet.
            _codecInfo = new StreamCodecInfo();
            _creationTime = -1;
        }

        public override void Start()
        {
            try
            {
                _videoCodecFactory = this.Scope.GetService(typeof(VideoCodecFactory)) as VideoCodecFactory;
            }
            catch (Exception ex)
            {
                log.Warn("No video codec factory available.", ex);
            }
        }

        #region IMessageComponent Members

        /// <summary>
        /// Handles out-of-band control message.
        /// </summary>
        /// <param name="source">Message component source.</param>
        /// <param name="pipe">Connection pipe.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message</param>
        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
        }

        #endregion

        #region IPipeConnectionListener Members

        /// <summary>
        /// Pipe connection event handler. There are two types of pipe connection events so far,
        /// provider push connection event and provider disconnection event.
        /// </summary>
        /// <param name="evt"></param>
        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.PROVIDER_CONNECT_PUSH:
                    if (evt.Provider == this && (evt.ParameterMap == null || !evt.ParameterMap.ContainsKey("record")))
                    {
                        _livePipe = evt.Source as IPipe;
                    }

                    break;
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    if (_livePipe == evt.Source)
                        _livePipe = null;
                    break;
                case PipeConnectionEvent.CONSUMER_CONNECT_PUSH:
                    break;
                case PipeConnectionEvent.CONSUMER_DISCONNECT:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region IBroadcastStream Members

        public void SaveAs(string filePath, bool isAppend)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public string SaveFilename
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public string PublishedName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        public IProvider Provider
        {
            get { return this; }
        }

        public void AddStreamListener(IStreamListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveStreamListener(IStreamListener listener)
        {
            _listeners.Remove(listener);
        }

        public System.Collections.ICollection GetStreamListeners()
        {
            return _listeners;
        }

        #endregion

        public void DispatchEvent(IEvent @event)
        {
            try
            {
                if (@event is IRtmpEvent)
                {
                    IRtmpEvent rtmpEvent = @event as IRtmpEvent;
                    if (_livePipe != null)
                    {
                        RtmpMessage msg = new RtmpMessage();
                        msg.body = rtmpEvent;
                        if (_creationTime == -1)
                            _creationTime = rtmpEvent.Timestamp;
                        try
                        {
                            if (@event is AudioData)
                            {
                                (_codecInfo as StreamCodecInfo).HasAudio = true;
                            }
                            else if (@event is VideoData)
                            {
                                IVideoStreamCodec videoStreamCodec = null;
                                if (_codecInfo.VideoCodec == null)
                                {
                                    videoStreamCodec = _videoCodecFactory.GetVideoCodec((@event as VideoData).Data);
                                    (_codecInfo as StreamCodecInfo).VideoCodec = videoStreamCodec;
                                }
                                else if (_codecInfo != null)
                                {
                                    videoStreamCodec = _codecInfo.VideoCodec;
                                }

                                if (videoStreamCodec != null)
                                {
                                    videoStreamCodec.AddData((rtmpEvent as VideoData).Data);
                                }

                                if (_codecInfo != null)
                                    (_codecInfo as StreamCodecInfo).HasVideo = true;
                            }
                            _livePipe.PushMessage(msg);

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
                                        log.Error("Error while notifying listener " + listener, ex);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // ignore
                            log.Error("DispatchEvent exception", ex);
                        }
                    }
                }
            }
            finally
            {
            }
        }

    }
}
