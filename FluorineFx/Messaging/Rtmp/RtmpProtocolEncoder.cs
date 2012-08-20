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
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Util;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpProtocolEncoder
	{
#if !SILVERLIGHT
        static private ILog _log = LogManager.GetLogger(typeof(RtmpProtocolEncoder));
#endif

        static RtmpProtocolEncoder()
		{
		}

		/// <summary>
		/// Determine type of header to use.
		/// </summary>
		/// <param name="header">RTMP message header.</param>
		/// <param name="lastHeader">Previous header.</param>
		/// <returns>Header type to use.</returns>
		private static HeaderType GetHeaderType(RtmpHeader header, RtmpHeader lastHeader) 
		{
			HeaderType headerType;
			if (lastHeader == null
				|| header.StreamId != lastHeader.StreamId
				|| !header.IsTimerRelative) 
			{
				// New header mark if header for another stream
				headerType = HeaderType.HeaderNew;
			} 
			else if (header.Size != lastHeader.Size
				|| header.DataType != lastHeader.DataType) 
			{
				// Same source header if last header data type or size differ
				headerType = HeaderType.HeaderSameSource;
			} 
			else if (header.Timer != lastHeader.Timer) 
			{
				// Timer change marker if there's time gap between headers timestamps
				headerType = HeaderType.HeaderTimerChange;
			} 
			else 
			{
				// Continue encoding
				headerType = HeaderType.HeaderContinue;
			}
			return headerType;
		}
		/// <summary>
		/// Calculate number of bytes necessary to encode the header.
		/// </summary>
		/// <param name="header">RTMP message header</param>
		/// <param name="lastHeader">Previous header</param>
		/// <returns>Calculated size</returns>
		private static int CalculateHeaderSize(RtmpHeader header, RtmpHeader lastHeader) 
		{
			HeaderType headerType = GetHeaderType(header, lastHeader);
			int channelIdAdd;
			if (header.ChannelId > 320)
				channelIdAdd = 2;
			else if (header.ChannelId > 63)
				channelIdAdd = 1;
			else
				channelIdAdd = 0;
		
			return RtmpHeader.GetHeaderLength(headerType) + channelIdAdd;
		}

		public static ByteBuffer Encode(RtmpContext context, object message)
		{		
			try
			{
				if (message is ByteBuffer) 
					return (ByteBuffer) message;
				else 
					return EncodePacket(context, message as RtmpPacket);
			}			 
			catch(Exception ex) 
			{
#if !SILVERLIGHT
                if( _log != null )
					_log.Fatal("Error encoding object. ", ex);
#endif
			}
			return null;
		}

		public static ByteBuffer EncodePacket(RtmpContext context, RtmpPacket packet) 
		{
			RtmpHeader header = packet.Header;
			int channelId = header.ChannelId;
			IRtmpEvent message = packet.Message;
			ByteBuffer data;

			if (message is ChunkSize)
			{
				ChunkSize chunkSizeMsg = (ChunkSize)message;
				context.SetWriteChunkSize(chunkSizeMsg.Size);
			}

			data = EncodeMessage(context, header, message);

			if(data.Position != 0) 
				data.Flip();
			else 
				data.Rewind();
			header.Size = (int)data.Limit;

			RtmpHeader lastHeader = context.GetLastWriteHeader(channelId);
			int headerSize = CalculateHeaderSize(header, lastHeader);
			context.SetLastWriteHeader(channelId, header);
			context.SetLastWritePacket(channelId, packet);

			int chunkSize = context.GetWriteChunkSize();
			int chunkHeaderSize = 1;
			if (header.ChannelId > 320)
				chunkHeaderSize = 3;
			else if (header.ChannelId > 63)
				chunkHeaderSize = 2;
			int numChunks = (int)Math.Ceiling(header.Size / (float)chunkSize);
			int bufSize = (int)header.Size + headerSize + (numChunks > 0 ? (numChunks - 1) * chunkHeaderSize : 0);
			ByteBuffer output = ByteBuffer.Allocate(bufSize);
			
			EncodeHeader(header, lastHeader, output);

			if (numChunks == 1) 
			{
				// we can do it with a single copy
				ByteBuffer.Put(output, data, output.Remaining);
			}
			else 
			{
				for(int i = 0; i < numChunks - 1; i++) 
				{
					ByteBuffer.Put(output, data, chunkSize);
					EncodeHeaderByte(output, (byte)HeaderType.HeaderContinue, header.ChannelId);
				}
				ByteBuffer.Put(output, data, output.Remaining);
			}
			//data.Close();
			output.Flip();
			return output;
		}
		/// <summary>
		/// Encodes header size marker and channel id into header marker
		/// </summary>
		/// <param name="output"></param>
		/// <param name="headerSize">Header size marker</param>
		/// <param name="channelId">Channel used</param>
		/// <returns></returns>
		public static void EncodeHeaderByte(ByteBuffer output, byte headerSize, int channelId) 
		{
			if(channelId <= 63) 
			{
				output.Put((byte) ((headerSize << 6) + channelId));
			}
			else if (channelId <= 320) 
			{
				output.Put((byte) (headerSize << 6));
				output.Put((byte) (channelId - 64));
			} 
			else 
			{
				output.Put((byte) ((headerSize << 6) | 1));
				channelId -= 64;
				output.Put((byte) (channelId & 0xff));
				output.Put((byte) (channelId >> 8));
			}
		}
		/// <summary>
		/// Encode RTMP header into given ByteBuffer
		/// </summary>
		/// <param name="header">RTMP message header</param>
		/// <param name="lastHeader">Previous header</param>
		/// <param name="buffer">Buffer to write encoded header to</param>
		/// <returns>Encoded header data</returns>
		public static ByteBuffer EncodeHeader(RtmpHeader header, RtmpHeader lastHeader, ByteBuffer buffer)
		{
			HeaderType headerType = GetHeaderType(header, lastHeader);
			EncodeHeaderByte(buffer, (byte)headerType, header.ChannelId);
			switch(headerType) 
			{
				case HeaderType.HeaderNew:
                    if (header.Timer < 0xffffff)
                        buffer.WriteMediumInt(header.Timer);
                    else
                        buffer.WriteMediumInt(0xffffff);
					buffer.WriteMediumInt(header.Size);
					buffer.Put((byte)header.DataType);
					buffer.WriteReverseInt(header.StreamId);
					break;
				case HeaderType.HeaderSameSource:
                    if (header.Timer < 0xffffff)
                        buffer.WriteMediumInt(header.Timer);
                    else
                        buffer.WriteMediumInt(0xffffff);
                    buffer.WriteMediumInt(header.Size);
					buffer.Put((byte)header.DataType);
					break;
				case HeaderType.HeaderTimerChange:
                    if (header.Timer < 0xffffff)
                        buffer.WriteMediumInt(header.Timer);
                    else
                        buffer.WriteMediumInt(0xffffff);
                    break;
				case HeaderType.HeaderContinue:
					break;
			}

            if (header.Timer >= 0xffffff)
                buffer.PutInt(header.Timer);

			return buffer;
		}

		public static ByteBuffer EncodeMessage(RtmpContext context, RtmpHeader header, IRtmpEvent message) 
		{
			switch(header.DataType) 
			{
				case Constants.TypeChunkSize:
                    return EncodeChunkSize(context, message as ChunkSize);
				case Constants.TypeInvoke:
					return EncodeInvoke(context, message as Invoke);
                case Constants.TypeFlexInvoke:
					return EncodeFlexInvoke(context, message as FlexInvoke);
                case Constants.TypeSharedObject:
					return EncodeSharedObject(context, message as ISharedObjectMessage);
                case Constants.TypeFlexSharedObject:
					return EncodeFlexSharedObject(context, message as ISharedObjectMessage);
                case Constants.TypeNotify:
                    if ((message as Notify).ServiceCall == null)
                    {
                        return EncodeStreamMetadata(context, message as Notify);
                    }
                    else
                    {
                        return EncodeNotify(context, message as Notify);
                    }
                case Constants.TypePing:
                    return EncodePing(context, message as Ping);
                case Constants.TypeBytesRead:
                    return EncodeBytesRead(context, message as BytesRead);
                case Constants.TypeAudioData:
                    return EncodeAudioData(context, message as AudioData);
                case Constants.TypeVideoData:
                    return EncodeVideoData(context, message as VideoData);
                case Constants.TypeServerBandwidth:
                    return EncodeServerBW(context, message as ServerBW);
                case Constants.TypeClientBandwidth:
                    return EncodeClientBW(context, message as ClientBW);
                case Constants.TypeFlexStreamEnd:
                    return EncodeFlexStreamSend(context, message as FlexStreamSend);
                default:
#if !SILVERLIGHT
                    if( _log.IsErrorEnabled )
						_log.Error("Unknown object type: " + header.DataType);
#endif
					return null;
			}
		}

        /// <summary>
        /// Encode server-side bandwidth event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="serverBW"></param>
        /// <returns></returns>
        static ByteBuffer EncodeServerBW(RtmpContext context, ServerBW serverBW)
        {
            ByteBuffer output = ByteBuffer.Allocate(4);
            output.PutInt(serverBW.Bandwidth);
            return output;
        }

        static ByteBuffer EncodeClientBW(RtmpContext context, ClientBW clientBW)
        {
            ByteBuffer output = ByteBuffer.Allocate(5);
            output.PutInt(clientBW.Bandwidth);
            output.Put(clientBW.Value2);
            return output;
        }

        static ByteBuffer EncodeChunkSize(RtmpContext context, ChunkSize chunkSize) 
		{
			ByteBuffer output = ByteBuffer.Allocate(4);
			output.PutInt(chunkSize.Size);
			return output;
		}

        static ByteBuffer EncodePing(RtmpContext context, Ping ping)
        {
            int len = 6;
            if (ping.Value3 != Ping.Undefined)
            {
                len += 4;
            }
            if (ping.Value4 != Ping.Undefined)
            {
                len += 4;
            }
            ByteBuffer output = ByteBuffer.Allocate(len);
            output.PutShort(ping.PingType);
            output.PutInt(ping.Value2);
            if (ping.Value3 != Ping.Undefined)
            {
                output.PutInt(ping.Value3);
            }
            if (ping.Value4 != Ping.Undefined)
            {
                output.PutInt(ping.Value4);
            }
            return output;
        }

        static ByteBuffer EncodeBytesRead(RtmpContext context, BytesRead bytesRead)
        {
            ByteBuffer output = ByteBuffer.Allocate(4);
            output.PutInt(bytesRead.Bytes);
            return output;
        }

        static ByteBuffer EncodeAudioData(RtmpContext context, AudioData audioData)
        {
            ByteBuffer output = audioData.Data;
            return output;
        }

        static ByteBuffer EncodeVideoData(RtmpContext context, VideoData videoData)
        {
            ByteBuffer output = videoData.Data;
            return output;
        }

        static ByteBuffer EncodeStreamMetadata(RtmpContext context, Notify metaData)
        {
            ByteBuffer output = metaData.Data;
            return output;
        }


        static ByteBuffer EncodeFlexStreamSend(RtmpContext context, FlexStreamSend msg)
        {
            ByteBuffer output = msg.Data;
            return output;
        }

		static ByteBuffer EncodeInvoke(RtmpContext context, Invoke invoke) 
		{
			return EncodeNotifyOrInvoke(context, invoke);
		}

        static ByteBuffer EncodeNotify(RtmpContext context,  Notify notify)
        {
            return EncodeNotifyOrInvoke(context, notify);
        }

		static ByteBuffer EncodeNotifyOrInvoke(RtmpContext context, Notify invoke)
		{
			//MemoryStreamEx output = new MemoryStreamEx();
			ByteBuffer output = ByteBuffer.Allocate(1024);
			output.AutoExpand = true;
			RtmpWriter writer = new RtmpWriter(output);
            //Set legacy collection flag from context
            writer.UseLegacyCollection = context.UseLegacyCollection;
            writer.UseLegacyThrowable = context.UseLegacyThrowable;

			IServiceCall serviceCall = invoke.ServiceCall;
			bool isPending = serviceCall.Status == Call.STATUS_PENDING;
			if (!isPending) 
			{
				//log.debug("Call has been executed, send result");
                writer.WriteData(context.ObjectEncoding, serviceCall.IsSuccess ? "_result" : "_error");
			}
			else
			{
				//log.debug("This is a pending call, send request");
				string action = (serviceCall.ServiceName == null) ? serviceCall.ServiceMethodName : serviceCall.ServiceName + "." + serviceCall.ServiceMethodName;
				writer.WriteData(context.ObjectEncoding, action);
			}
			if(invoke is Invoke)
			{
				writer.WriteData(context.ObjectEncoding, invoke.InvokeId);
				writer.WriteData(context.ObjectEncoding, invoke.ConnectionParameters);
			}
			if(!isPending && (invoke is Invoke)) 
			{
				IPendingServiceCall pendingCall = (IPendingServiceCall)serviceCall;
                if (!serviceCall.IsSuccess && !(pendingCall.Result is StatusASO))
                {
                    StatusASO status = GenerateErrorResult(StatusASO.NC_CALL_FAILED, serviceCall.Exception);
                    pendingCall.Result = status;
                }
				writer.WriteData(context.ObjectEncoding, pendingCall.Result);
			}
			else
			{
				//log.debug("Writing params");
				object[] args = invoke.ServiceCall.Arguments;
				if (args != null) 
				{
					foreach(object element in args)
					{
						writer.WriteData(context.ObjectEncoding, element);
					}
				}
			}
			return output;
		}

		static ByteBuffer EncodeFlexInvoke(RtmpContext context, FlexInvoke invoke)
		{
			ByteBuffer output = ByteBuffer.Allocate(1024);
			output.AutoExpand = true;
			RtmpWriter writer = new RtmpWriter(output);
            //Set legacy collection flag from context
            writer.UseLegacyCollection = context.UseLegacyCollection;
            writer.UseLegacyThrowable = context.UseLegacyThrowable;
			
			writer.WriteByte(0);
			//writer.WriteData(context.ObjectEncoding, invoke.Cmd);
            IServiceCall serviceCall = invoke.ServiceCall;
            bool isPending = serviceCall.Status == Call.STATUS_PENDING;
            if (!isPending)
            {
                //log.debug("Call has been executed, send result");
                if (serviceCall.IsSuccess)
                    writer.WriteData(context.ObjectEncoding, "_result");
                else
                    writer.WriteData(context.ObjectEncoding, "_error");
            }
            else
            {
                //log.debug("This is a pending call, send request");
                writer.WriteData(context.ObjectEncoding, serviceCall.ServiceMethodName);
            }
			writer.WriteData(context.ObjectEncoding, invoke.InvokeId);
			writer.WriteData(context.ObjectEncoding, invoke.CmdData);
			//object response = invoke.Response;
			//writer.WriteData(context.ObjectEncoding, response);
            if (!isPending)
            {
                IPendingServiceCall pendingCall = (IPendingServiceCall)serviceCall;
                /*
                if (!serviceCall.IsSuccess)
                {
                    StatusASO status = GenerateErrorResult(StatusASO.NC_CALL_FAILED, serviceCall.Exception);
                    pendingCall.Result = status;
                }
                */
                writer.WriteData(context.ObjectEncoding, pendingCall.Result);
            }
            else
            {
                //log.debug("Writing params");
                object[] args = invoke.ServiceCall.Arguments;
                if (args != null)
                {
                    foreach (object element in args)
                    {
                        writer.WriteData(context.ObjectEncoding, element);
                    }
                }
            }
			return output;
		}

		static ByteBuffer EncodeFlexSharedObject(RtmpContext context, ISharedObjectMessage so)
		{
			ByteBuffer output = ByteBuffer.Allocate(128);
			output.AutoExpand = true;
			output.Put((byte)0);
			EncodeSharedObject(context, so, output);
			return output;
		}

		static ByteBuffer EncodeSharedObject(RtmpContext context, ISharedObjectMessage so)
		{
			ByteBuffer output = ByteBuffer.Allocate(128);
			output.AutoExpand = true;
			EncodeSharedObject(context, so, output);
			return output;
		}

		static void EncodeSharedObject(RtmpContext context, ISharedObjectMessage so, ByteBuffer output)
		{
			RtmpWriter writer = new RtmpWriter(output);
            //Set legacy collection flag from context
            writer.UseLegacyCollection = context.UseLegacyCollection;
            writer.UseLegacyThrowable = context.UseLegacyThrowable;

			writer.WriteUTF(so.Name);
			// SO version
			writer.WriteInt32(so.Version);
			// Encoding (this always seems to be 2 for persistent shared objects)
			writer.WriteInt32(so.IsPersistent ? 2 : 0);
			// unknown field
			writer.WriteInt32(0);
			
			int mark, len = 0;

			foreach(ISharedObjectEvent sharedObjectEvent in so.Events)
			{
				byte type = SharedObjectTypeMapping.ToByte(sharedObjectEvent.Type);
				switch(sharedObjectEvent.Type) 
				{
                    case SharedObjectEventType.SERVER_CONNECT:
                    case SharedObjectEventType.CLIENT_INITIAL_DATA:
					case SharedObjectEventType.CLIENT_CLEAR_DATA:
						writer.WriteByte(type);
						writer.WriteInt32(0);
						break;
                    case SharedObjectEventType.SERVER_DELETE_ATTRIBUTE:
                    case SharedObjectEventType.CLIENT_DELETE_DATA:
					case SharedObjectEventType.CLIENT_UPDATE_ATTRIBUTE:
						writer.WriteByte(type);
						mark = (int)output.Position;
						output.Skip(4); // we will be back
						writer.WriteUTF(sharedObjectEvent.Key);
						len = (int)output.Position - mark - 4;
						output.PutInt(mark, len);
						break;
					case SharedObjectEventType.SERVER_SET_ATTRIBUTE:
					case SharedObjectEventType.CLIENT_UPDATE_DATA:
						if (sharedObjectEvent.Key == null) 
						{
							// Update multiple attributes in one request
							IDictionary initialData = sharedObjectEvent.Value as IDictionary;
							foreach(DictionaryEntry entry in initialData)
							{
								writer.WriteByte(type);
								mark = (int)output.Position;
								output.Skip(4); // we will be back
								string key = entry.Key as string;
								object value = entry.Value;
								writer.WriteUTF(key);
								writer.WriteData(context.ObjectEncoding, value);
								
								len = (int)output.Position - mark - 4;
								output.PutInt(mark, len);
							}
						} 
						else 
						{
							writer.WriteByte(type);
							mark = (int)output.Position;
							output.Skip(4); // we will be back
							writer.WriteUTF(sharedObjectEvent.Key);
							writer.WriteData(context.ObjectEncoding, sharedObjectEvent.Value);
							//writer.WriteData(sharedObjectEvent.Value);

							len = (int)output.Position - mark - 4;
							output.PutInt(mark, len);
						}
						break;
					case SharedObjectEventType.CLIENT_SEND_MESSAGE:
					case SharedObjectEventType.SERVER_SEND_MESSAGE:
						// Send method name and value
						writer.WriteByte(type);
						mark = (int)output.Position;
						output.Skip(4); // we will be back

						// Serialize name of the handler to call
						writer.WriteData(context.ObjectEncoding, sharedObjectEvent.Key);
						//writer.WriteUTF(sharedObjectEvent.Key);
						// Serialize the arguments
						foreach(object arg in sharedObjectEvent.Value as IList)
						{
							writer.WriteData(context.ObjectEncoding, arg);
						}
						//writer.WriteData(sharedObjectEvent.Value as IList);
						len = (int)output.Position - mark - 4;
						//output.PutInt(mark, len);
						output.PutInt(mark, len);
						break;
					case SharedObjectEventType.CLIENT_STATUS:
						writer.WriteByte(type);
						mark = (int)output.Position;
						output.Skip(4); // we will be back
						writer.WriteUTF(sharedObjectEvent.Key);
						writer.WriteUTF(sharedObjectEvent.Value as string);
						len = (int)output.Position - mark - 4;
						output.PutInt(mark, len);
						break;
                    case SharedObjectEventType.SERVER_DISCONNECT:
                        writer.WriteByte(type);
                        output.PutInt((int)output.Position, 0);
                        break;
					default:
#if !SILVERLIGHT
                        _log.Error("Unknown event " + sharedObjectEvent.Type.ToString());
#endif
						writer.WriteByte(type);
						mark = (int)output.Position;
						output.Skip(4); // we will be back
                        if (sharedObjectEvent.Key != null)
                        {
                            writer.WriteUTF(sharedObjectEvent.Key);
                            writer.WriteData(context.ObjectEncoding, sharedObjectEvent.Value);
                        }
						len = (int)output.Position - mark - 4;
						output.PutInt(mark, len);
						break;
				}
			}
		}
        /// <summary>
        /// Generate error object to return for given exception.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        static StatusASO GenerateErrorResult(string code, Exception exception)
        {
            string message = "";
            if (exception != null && exception.Message != null)
                message = exception.Message;
            StatusASO status = new StatusASO(code, "error", message);
            return status;
        }
	}
}