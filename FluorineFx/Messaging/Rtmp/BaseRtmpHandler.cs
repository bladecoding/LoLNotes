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
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Context;
using FluorineFx.Util;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Rtmp
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class BaseRtmpHandler : IRtmpHandler
    {
        /// <summary>
        /// Connection Action constant.
        /// </summary>
        public const string ACTION_CONNECT = "connect";
        /// <summary>
        /// Disconnect Action constant.
        /// </summary>
        public const string ACTION_DISCONNECT = "disconnect";
        /// <summary>
        /// CreateStream Action constant.
        /// </summary>
        public const string ACTION_CREATE_STREAM = "createStream";
        /// <summary>
        /// DeleteStream Action constant.
        /// </summary>
        public const string ACTION_DELETE_STREAM = "deleteStream";
        /// <summary>
        /// CloseStream Action constant.
        /// </summary>
        public const string ACTION_CLOSE_STREAM = "closeStream";
        /// <summary>
        /// ReleaseStream Action constant.
        /// </summary>
        public const string ACTION_RELEASE_STREAM = "releaseStream";
        /// <summary>
        /// Publish Action constant.
        /// </summary>
        public const string ACTION_PUBLISH = "publish";
        /// <summary>
        /// Pause Action constant.
        /// </summary>
        public const string ACTION_PAUSE = "pause";
        /// <summary>
        /// Seek Action constant.
        /// </summary>
        public const string ACTION_SEEK = "seek";
        /// <summary>
        /// Play Action constant.
        /// </summary>
        public const string ACTION_PLAY = "play";
        /// <summary>
        /// Stop Action constant.
        /// </summary>
        public const string ACTION_STOP = "disconnect";
        /// <summary>
        /// ReceiveVideo Action constant.
        /// </summary>
        public const string ACTION_RECEIVE_VIDEO = "receiveVideo";
        /// <summary>
        /// ReceiveAudio Action constant.
        /// </summary>
        public const string ACTION_RECEIVE_AUDIO = "receiveAudio";

#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseRtmpHandler));
#endif

        /// <summary>
        /// Initializes a new instance of the BaseRtmpHandler class.
        /// </summary>
        public BaseRtmpHandler()
        {
        }

        internal static string GetHostname(string url)
        {
            string[] parts = url.Split(new char[] { '/' });
            if (parts.Length == 2)
                return "";
            else
                return parts[2];
        }

        #region IRtmpHandler Members

        /// <summary>
        /// Connection open event.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        public virtual void ConnectionOpened(RtmpConnection connection)
        {
        }
        /// <summary>
        /// Message recieved.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="obj">Message object.</param>
        public void MessageReceived(RtmpConnection connection, object obj)
        {
            IRtmpEvent message = null;
            RtmpPacket packet = null;
            RtmpHeader header = null;
            RtmpChannel channel = null;
            IClientStream stream = null;
            try
            {
                packet = obj as RtmpPacket;
                message = packet.Message;
                header = packet.Header;
                channel = connection.GetChannel(header.ChannelId);
                if( connection is IStreamCapableConnection )
                    stream = (connection as IStreamCapableConnection).GetStreamById(header.StreamId);

                // Support stream ids
#if !SILVERLIGHT
                FluorineContext.ValidateContext();
                FluorineContext.Current.Connection.SetAttribute(FluorineContext.FluorineStreamIdKey, header.StreamId);
#endif
                // Increase number of received messages
                connection.MessageReceived();

#if !SILVERLIGHT
                if (log != null && log.IsDebugEnabled)
                    log.Debug("RtmpConnection message received, type = " + header.DataType);
#endif

                if (message != null)
                    message.Source = connection;

                switch (header.DataType)
                {
                    case Constants.TypeInvoke:
                        OnInvoke(connection, channel, header, message as Invoke);
                        if (message.Header.StreamId != 0
                            && (message as Invoke).ServiceCall.ServiceName == null
                            && (message as Invoke).ServiceCall.ServiceMethodName == BaseRtmpHandler.ACTION_PUBLISH)
                        {
                            if (stream != null) //Dispatch if stream was created
                                (stream as IEventDispatcher).DispatchEvent(message);
                        }
                        break;
                    case Constants.TypeFlexInvoke:
                        OnFlexInvoke(connection, channel, header, message as FlexInvoke);
                        if (message.Header.StreamId != 0
                            && (message as Invoke).ServiceCall.ServiceName == null
                            && (message as Invoke).ServiceCall.ServiceMethodName == BaseRtmpHandler.ACTION_PUBLISH)
                        {
                            if (stream != null) //Dispatch if stream was created
                                (stream as IEventDispatcher).DispatchEvent(message);
                        }
                        break;
                    case Constants.TypeNotify:// just like invoke, but does not return
                        if ((message as Notify).Data != null && stream != null)
                        {
                            // Stream metadata
                            (stream as IEventDispatcher).DispatchEvent(message);
                        }
                        else
                            OnInvoke(connection, channel, header, message as Notify);
                        break;
                    case Constants.TypePing:
                        OnPing(connection, channel, header, message as Ping);
                        break;
                    case Constants.TypeBytesRead:
                        OnStreamBytesRead(connection, channel, header, message as BytesRead);
                        break;
                    case Constants.TypeSharedObject:
                    case Constants.TypeFlexSharedObject:
                        OnSharedObject(connection, channel, header, message as SharedObjectMessage);
                        break;
                    case Constants.TypeFlexStreamEnd:
                        if (stream != null)
                            (stream as IEventDispatcher).DispatchEvent(message);
                        break;
                    case Constants.TypeChunkSize:
                        OnChunkSize(connection, channel, header, message as ChunkSize);
                        break;
                    case Constants.TypeAudioData:
                    case Constants.TypeVideoData:
                        // NOTE: If we respond to "publish" with "NetStream.Publish.BadName",
                        // the client sends a few stream packets before stopping. We need to
                        // ignore them.
                        if (stream != null)
                            ((IEventDispatcher)stream).DispatchEvent(message);
                        break;
                    case Constants.TypeServerBandwidth:
                        OnServerBW(connection, channel, header, message as ServerBW);
                        break;
                    case Constants.TypeClientBandwidth:
                        OnClientBW(connection, channel, header, message as ClientBW);
                        break;
                    default:
#if !SILVERLIGHT
                        if (log != null && log.IsDebugEnabled)
                            log.Debug("RtmpService event not handled: " + header.DataType);
#endif
                        break;
                }
            }
            catch (Exception ex)
            {
#if !SILVERLIGHT
                if (log.IsErrorEnabled)
                {
                    log.Error(__Res.GetString(__Res.Rtmp_HandlerError), ex);
                    log.Error(__Res.GetString(__Res.Error_ContextDump));
                    //log.Error(Environment.NewLine);
                    log.Error(packet);
                }
#endif
            }
        }
        /// <summary>
        /// Message sent.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        /// <param name="message">Message object.</param>
        public virtual void MessageSent(RtmpConnection connection, object message)
        {
    		if (message is ByteBuffer)
			    return;

		    // Increase number of sent messages
		    connection.MessageSent(message as RtmpPacket);
        }
        /// <summary>
        /// Connection closed.
        /// </summary>
        /// <param name="connection">Connection object.</param>
        public virtual void ConnectionClosed(RtmpConnection connection)
        {
            connection.Close();
        }

        #endregion

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="source"></param>
        /// <param name="streamBytesRead"></param>
        protected void OnStreamBytesRead(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, BytesRead streamBytesRead)
        {
            connection.ReceivedBytesRead(streamBytesRead.Bytes);
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        protected abstract void OnChunkSize(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ChunkSize chunkSize);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="source"></param>
        /// <param name="ping"></param>
        protected abstract void OnPing(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, Ping ping);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="header"></param>
        /// <param name="invoke"></param>
        protected abstract void OnInvoke(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, Notify invoke);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="header"></param>
        /// <param name="message"></param>
        protected abstract void OnSharedObject(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, SharedObjectMessage message);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="header"></param>
        /// <param name="invoke"></param>
        protected abstract void OnFlexInvoke(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, FlexInvoke invoke);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="source"></param>
        /// <param name="serverBW"></param>
        protected abstract void OnServerBW(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ServerBW serverBW);
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <param name="source"></param>
        /// <param name="clientBW"></param>
        protected abstract void OnClientBW(RtmpConnection connection, RtmpChannel channel, RtmpHeader source, ClientBW clientBW);

        /// <summary>
        /// Handler for pending call result. Dispatches results to all pending call handlers.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="invoke">Pending call result event context.</param>
        protected void HandlePendingCallResult(RtmpConnection connection, Notify invoke)
        {
            IServiceCall call = invoke.ServiceCall;
            IPendingServiceCall pendingCall = connection.RetrievePendingCall(invoke.InvokeId);
            if (pendingCall != null)
            {
                pendingCall.Status = call.Status;
                // The client sent a response to a previously made call.
                object[] args = call.Arguments;
                if ((args != null) && (args.Length > 0))
                {
                    pendingCall.Result = args[0];
                }

                IPendingServiceCallback[] callbacks = pendingCall.GetCallbacks();
                if (callbacks != null && callbacks.Length > 0)
                {
                    foreach (IPendingServiceCallback callback in callbacks)
                    {
                        try
                        {
                            callback.ResultReceived(pendingCall);
                        }
                        catch (Exception ex)
                        {
#if !SILVERLIGHT
                            log.Error("Error while executing callback " + callback, ex);
#endif
                        }
                    }
                }
            }
        }

        internal static void Push(RtmpConnection connection, IMessage message, IMessageClient messageClient)
        {
            if (connection != null)
            {
                object response = message;
                if (message is BinaryMessage)
                {
                    BinaryMessage binaryMessage = message as BinaryMessage;
                    binaryMessage.Update(messageClient);
                    byte[] binaryContent = binaryMessage.body as byte[];
                    //byte[] destClientBinaryId = messageClient.GetBinaryId();
                    //Array.Copy(destClientBinaryId, 0, binaryContent, binaryMessage.PatternPosition, destClientBinaryId.Length);

                    RawBinary result = new RawBinary(binaryContent);
                    response = result;
                }
                else
                {
                    //This should be a clone of the original message
                    message.SetHeader(MessageBase.DestinationClientIdHeader, messageClient.ClientId);
                    message.clientId = messageClient.ClientId;
                }

                RtmpChannel channel = connection.GetChannel(3);
                FlexInvoke reply = new FlexInvoke();
                Call call = new Call("receive", new object[] { response });
                reply.ServiceCall = call;
                //reply.Cmd = "receive";
                //reply.Response = response;
                reply.InvokeId = connection.InvokeId;
                channel.Write(reply);
            }
        }
    }
}
