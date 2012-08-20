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
using System.Reflection;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.IO;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Consumer;
using FluorineFx.Util;
using FluorineFx.Invocation;
using FluorineFx.Exceptions;

namespace FluorineFx.Net
{
    /// <summary>
    /// Represents the method that will handle the NetStreamVideo event of a NetStream object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A NetStreamVideoEventArgs object that contains the event data.</param>
    [CLSCompliant(false)]
    public delegate void NetStreamVideoHandler(object sender, NetStreamVideoEventArgs e);
    /// <summary>
    /// Represents the method that will handle the NetStreamAudio event of a NetStream object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A NetStreamAudioEventArgs object that contains the event data.</param>
    [CLSCompliant(false)]
    public delegate void NetStreamAudioHandler(object sender, NetStreamAudioEventArgs e);
    /// <summary>
    /// Represents the method that will handle the OnMetaData event of a NetStream object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A MetaDataEventArgs object that contains the event data.</param>
    public delegate void MetaDataHandler(object sender, MetaDataEventArgs e);
    /// <summary>
    /// Represents the method that will handle the OnPlayStatus event of a NetStream object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A PlayStatusEventArgs object that contains the event data.</param>
    public delegate void PlayStatusHandler(object sender, PlayStatusEventArgs e);

    /// <summary>
    /// The NetStream class opens a one-way streaming connection between a clinet application and a media server, or between a client application and the local file system.
    /// A NetStream object is a channel within a NetConnection object. This channel can either publish a stream, using NetStream.Publish(), or subscribe to a published stream and receive data, 
    /// using NetStream.Play(). You can publish or play live (real-time) data and previously recorded data. 
    /// You can also call the NetStream.Send() method to send text messages to all subscribed clients.
    /// </summary>
    [CLSCompliant(false)]
    public class NetStream : AbstractClientStream, IPendingServiceCallback, IEventDispatcher, INetStreamEventHandler
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(NetStream));
#endif

        NetConnection _connection;
        object _client;
        int _start;
        int _length;

        OutputStream _outputStream;
        ConnectionConsumer _connectionConsumer;

        event NetStatusHandler _netStatusHandler;
        event NetStreamVideoHandler _netStreamVideoHandler;
        event NetStreamAudioHandler _netStreamAudioHandler;
        event MetaDataHandler _metaDataHandler;
        event PlayStatusHandler _playStatusHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetStream"/> class that can be used for playing video files through the specified NetConnection object.
        /// </summary>
        /// <param name="connection">A NetConnection object.</param>
        public NetStream(NetConnection connection) : this(connection, "connectToFMS")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetStream"/> class that can be used for playing video files through the specified NetConnection object.
        /// </summary>
        /// <param name="connection">A NetConnection object.</param>
        /// <param name="peerId">Not available.</param>
        public NetStream(NetConnection connection, string peerId)
        {
            ValidationUtils.ArgumentNotNull(connection, "connection");
            _connection = connection;
            _client = this;
        }

        /// <summary>
        /// Specifies the object on which callback methods are invoked to handle streaming or FLV file data. The default object is this, the NetStream object 
        /// being created. If you set the client property to another object, callback methods are invoked on that other object. 
        /// The NetStream.Client object can call the following functions and receive an associated data object: onCuePoint(), onImageData(), onMetaData(), onPlayStatus(), onTextData(), and onXMPData().
        /// </summary>
        public Object Client
        {
            get { return _client; }
            set
            {
                ValidationUtils.ArgumentNotNull(value, "Client");
                _client = value;
            }
        }

        /// <summary>
        /// Dispatched when a NetStream object is reporting its status or error condition
        /// </summary>
        public event NetStatusHandler NetStatus
        {
            add { _netStatusHandler += value; }
            remove { _netStatusHandler -= value; }
        }

        internal void RaiseNetStatus(Exception exception)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(exception));
            }
        }

        internal void RaiseNetStatus(string code, Exception exception)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(code, exception));
            }
        }

        internal void RaiseNetStatus(ASObject info)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(info));
            }
        }

        internal void RaiseNetStatus(string message)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(message));
            }
        }

        public event NetStreamVideoHandler NetStreamVideo
        {
            add { _netStreamVideoHandler += value; }
            remove { _netStreamVideoHandler -= value; }
        }

        internal void RaiseNetStreamVideo(VideoData videoData)
        {
            if (_netStreamVideoHandler != null)
            {
                _netStreamVideoHandler(this, new NetStreamVideoEventArgs(videoData));
            }
        }

        public event NetStreamAudioHandler NetStreamAudio
        {
            add { _netStreamAudioHandler += value; }
            remove { _netStreamAudioHandler -= value; }
        }

        internal void RaiseNetStreamAudio(AudioData audioData)
        {
            if (_netStreamAudioHandler != null)
            {
                _netStreamAudioHandler(this, new NetStreamAudioEventArgs(audioData));
            }
        }
        /// <summary>
        /// Dispatched when the application receives descriptive information embedded in the video being played.
        /// </summary>
        public event MetaDataHandler OnMetaData
        {
            add { _metaDataHandler += value; }
            remove { _metaDataHandler -= value; }
        }

        internal void RaiseOnMetaData(IDictionary obj)
        {
            if (_metaDataHandler != null)
            {
                _metaDataHandler(this, new MetaDataEventArgs(obj));
            }
        }
        /// <summary>
        /// Dispatched when a NetStream object has completely played a stream.
        /// </summary>
        public event PlayStatusHandler OnPlayStatus
        {
            add { _playStatusHandler += value; }
            remove { _playStatusHandler -= value; }
        }

        internal void RaiseOnPlayStatus(IDictionary infoObject)
        {
            if (_playStatusHandler != null)
            {
                _playStatusHandler(this, new PlayStatusEventArgs(infoObject));
            }
        }

        public override void Close()
        {
            // do nothing
        }

        public override void Start()
        {
            // do nothing
        }

        public override void Stop()
        {
            // do nothing
        }

        /// <summary>
        /// Plays media files.
        /// </summary>
        /// <param name="arguments">The name of a file, start time, duration of playback, reset flag.</param>
        public void Play(params object[] arguments)
        {
            ValidationUtils.ArgumentConditionTrue(arguments != null && arguments.Length > 0, "arguments", "At least the name of a file must be specified");
            ValidationUtils.ArgumentNotNullOrEmptyOrWhitespace(arguments[0] as string, "name");
            _name = arguments[0] as string;
            _start = -2;
            _length = -1;
            if (arguments.Length > 1)
            {
                ValidationUtils.ArgumentConditionTrue(arguments[1] is int, "start", "Integer value required for the 'start' parameter");
                _start = (int)arguments[1];
                ValidationUtils.ArgumentConditionTrue(_start > -3, "start", "Allowed values are -2, -1, 0, or a positive number");
            }
            if (arguments.Length > 2)
            {
                ValidationUtils.ArgumentConditionTrue(arguments[2] is int, "len", "Integer value required for the 'len' parameter");
                _length = (int)arguments[2];
                ValidationUtils.ArgumentConditionTrue(_length > -2, "len", "Allowed values are -1, 0, or a positive number");
            }

            INetConnectionClient client = _connection.NetConnectionClient;
            RtmpConnection connection = _connection.NetConnectionClient.Connection as RtmpConnection;
            IPendingServiceCallback callback = new CreateStreamCallBack(this, connection, this);
            client.Call("createStream", callback);
        }

        class CreateStreamCallBack : IPendingServiceCallback
        {
            IPendingServiceCallback _callback;
            RtmpConnection _connection;
            NetStream _stream;

            public CreateStreamCallBack(NetStream stream, RtmpConnection connection, IPendingServiceCallback callback)
            {
                ValidationUtils.ArgumentNotNull(connection, "connection");
                _stream = stream;
                _connection = connection;
                _callback = callback;
            }

            #region IPendingServiceCallback Members

            public void ResultReceived(IPendingServiceCall call)
            {
                if (call.Result is double)
                {
                    int streamId = System.Convert.ToInt32((double)call.Result);
#if !SILVERLIGHT
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Stream id: {0}", streamId));
#endif
                    //NetStreamInternal stream = new NetStreamInternal(_stream);
                    _stream.Connection = _connection;
                    _stream.StreamId = streamId;
                    _connection.AddClientStream(_stream);
                    _stream._outputStream = _connection.CreateOutputStream(streamId);
                    _stream._connectionConsumer = new ConnectionConsumer(_connection,
                            _stream._outputStream.Video.ChannelId,
                            _stream._outputStream.Audio.ChannelId,
                            _stream._outputStream.Data.ChannelId);
                }
                _callback.ResultReceived(call);
            }

            #endregion
        }

        #region IEventDispatcher Members

        public void DispatchEvent(IEvent @event)
        {
            IRtmpEvent rtmpEvent = @event as IRtmpEvent;
            if (rtmpEvent != null)
            {
                /*
                if (rtmpEvent is IStreamData)
                {
                }
                */
                if (rtmpEvent is VideoData)
                {
                    RaiseNetStreamVideo(rtmpEvent as VideoData);
                }
                if (rtmpEvent is AudioData)
                {
                    RaiseNetStreamAudio(rtmpEvent as AudioData);
                }
            }
        }

        #endregion

        #region IPendingServiceCallback Members

        public void ResultReceived(IPendingServiceCall call)
        {
            if ("createStream".Equals(call.ServiceMethodName))
            {
                RtmpConnection connection = _connection.NetConnectionClient.Connection as RtmpConnection;
                object[] args = new object[3] { _name, _start, _length };
                PendingCall pendingCall = new PendingCall("play", args);
                connection.Invoke(pendingCall, (byte)connection.GetChannelForStreamId(this.StreamId));
            }
        }

        #endregion

        #region INetStreamEventHandler Members

        public void OnStreamEvent(Notify notify)
        {
            bool onStatus = notify.ServiceCall.ServiceMethodName.Equals("onStatus");
            if (!onStatus)
            {
                if (notify.ServiceCall.ServiceMethodName.Equals("onMetaData"))
                    RaiseOnMetaData(notify.ServiceCall.Arguments[0] as IDictionary);
                if (notify.ServiceCall.ServiceMethodName.Equals("onPlayStatus"))
                    RaiseOnPlayStatus(notify.ServiceCall.Arguments[0] as IDictionary);

                MethodInfo mi = MethodHandler.GetMethod(_client.GetType(), notify.ServiceCall.ServiceMethodName, notify.ServiceCall.Arguments, false, false);
                if (mi != null)
                {
                    ParameterInfo[] parameterInfos = mi.GetParameters();
                    object[] args = new object[parameterInfos.Length];
                    notify.ServiceCall.Arguments.CopyTo(args, 0);
                    TypeHelper.NarrowValues(args, parameterInfos);
                    try
                    {
                        InvocationHandler invocationHandler = new InvocationHandler(mi);
                        object result = invocationHandler.Invoke(_client, args);
                    }
                    catch (Exception exception)
                    {
                        notify.ServiceCall.Exception = exception;
                        notify.ServiceCall.Status = FluorineFx.Messaging.Rtmp.Service.Call.STATUS_INVOCATION_EXCEPTION;
                        //log.Error("Error while invoking method " + call.ServiceMethodName + " on client", exception);
                    }
                }
                else
                {
                    string msg = __Res.GetString(__Res.Invocation_NoSuitableMethod, notify.ServiceCall.ServiceMethodName);
                    this.RaiseNetStatus(new FluorineException(msg));
                }
            }
            else
            {
                object[] args = notify.ServiceCall.Arguments;
                ASObject statusASO = null;
                if ((args != null) && (args.Length > 0))
                    statusASO = args[0] as ASObject;
                this.RaiseNetStatus(statusASO);
            }
        }

        #endregion
    }
}
