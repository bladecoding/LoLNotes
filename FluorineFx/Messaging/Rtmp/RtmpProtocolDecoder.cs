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
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Messaging.Rtmp.Service;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Util;
using FluorineFx.Configuration;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public sealed class RtmpProtocolDecoder
	{
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmpProtocolDecoder));
#endif
        /// <summary>
        /// RTMP handshake size.
        /// </summary>
		public const int HandshakeSize = 1536;

		static RtmpProtocolDecoder()
		{
		}
        /// <summary>
        /// Initializes a new instance of the RtmpProtocolDecoder class.
        /// </summary>
		public RtmpProtocolDecoder()
		{
		}
        /// <summary>
        /// Decode channel id.
        /// </summary>
        /// <param name="headerByte">Header byte.</param>
        /// <param name="byteCount">Byte count.</param>
        /// <returns>Channel id.</returns>
		public static byte DecodeChannelId(int headerByte, int byteCount) 
		{
			if (byteCount == 1) 
				return (byte)(headerByte & 0x3f);
			else if (byteCount == 2) 
				return (byte)(64 + (headerByte & 0xff));
			else
				return (byte)(64 + ((headerByte >> 8) & 0xff) + ((headerByte & 0xff) << 8));
		}
        /// <summary>
        /// Decode header size.
        /// </summary>
        /// <param name="headerByte">Header byte.</param>
        /// <param name="byteCount">Byte count.</param>
        /// <returns>Header size.</returns>
		public static byte DecodeHeaderSize(int headerByte, int byteCount) 
		{
			if (byteCount == 1) 
				return (byte)(headerByte >> 6);
			else if (byteCount == 2) 
				return (byte)(headerByte >> 14);
			else 
				return (byte)(headerByte >> 22);
		}
        /// <summary>
        /// Returns the header length from marker value.
        /// </summary>
        /// <param name="headerSize">Header size marker value.</param>
        /// <returns>The header length.</returns>
		public static int GetHeaderLength(byte headerSize) 
		{
			switch((HeaderType)headerSize) 
			{
				case HeaderType.HeaderNew:
					return 12;
				case HeaderType.HeaderSameSource:
					return 8;
				case HeaderType.HeaderTimerChange:
					return 4;
				case HeaderType.HeaderContinue:
					return 1;
				default:
					return -1;
			}
		}

#if !(NET_1_1)
        /// <summary>
        /// Decodes all available objects in the buffer.
        /// </summary>
        /// <param name="context">State for the protocol.</param>
        /// <param name="stream">Buffer of data to be decoded.</param>
        /// <returns>A list of decoded objects, may be empty if nothing could be decoded.</returns>
		public static List<object> DecodeBuffer(RtmpContext context, ByteBuffer stream)
#else
        public static ArrayList DecodeBuffer(RtmpContext context, ByteBuffer stream)
#endif
		{
			// >> HEADER[1] + CLIENT_HANDSHAKE[1536] 
			// << HEADER[1] + SERVER_HANDSHAKE[1536] + CLIENT_HANDSHAKE[1536];
			// >> SERVER_HANDSHAKE[1536] + AMF[n]
#if !(NET_1_1)
            List<object> result = null;
#else
			ArrayList result = null;
#endif
            try
            {
                while (true)
                {
                    long remaining = stream.Remaining;
                    if (context.CanStartDecoding(remaining))
                        context.StartDecoding();
                    else
                        break;

                    if (context.State == RtmpState.Disconnected)
                        break;

                    object decodedObject = Decode(context, stream);
                    if (context.HasDecodedObject)
                    {
#if !(NET_1_1)
                        if (result == null)
                            result = new List<object>();
#else
                        if (result == null)
                            result = new ArrayList();
#endif
                        result.Add(decodedObject);
                    }
                    else if (context.CanContinueDecoding)
                        continue;
                    else
                        break;

                    if (!stream.HasRemaining)
                        break;
                }
            }
            catch (HandshakeFailedException)
            {
                throw;
            }
            catch (Exception)
            {
                Dump(stream);
                throw;
            }
            finally
			{
			    stream.Compact();
			}
			return result;
		}

        private static void Dump(ByteBuffer buffer)
        {
#if !SILVERLIGHT
            if (FluorineConfiguration.Instance.FluorineSettings.Debug != null && FluorineContext.Current != null)
            {
                if (FluorineConfiguration.Instance.FluorineSettings.Debug.Mode != Debug.Off)
                {
                    try
                    {
                        if (FluorineConfiguration.Instance.FluorineSettings.Debug.DumpPath != null)
                        {
                            string fileName = string.Format("dump_{0}_{1}.bin", DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff"), buffer.Position);
                            if( FluorineContext.Current != null && FluorineContext.Current.Connection != null )
                                fileName = string.Format("dump_{0}_{1}_{2}.bin", FluorineContext.Current.Connection.ConnectionId, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff"), buffer.Position);
                            //string fileName = "dump_" + Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff") + ".bin";
                            IResource resource = FluorineContext.Current.GetResource(Path.Combine(FluorineConfiguration.Instance.FluorineSettings.Debug.DumpPath, fileName));
                            buffer.Dump(resource.File.FullName);
                        }
                    }
                    catch
                    {
                    }
                }
            }
#endif
        }

		private static object Decode(RtmpContext context, ByteBuffer stream)
		{
			//long start = stream.Position;
			switch(context.State)
			{					
				case RtmpState.Connected:
					return DecodePacket(context, stream);
				case RtmpState.Error:
					// Attempt to correct error 
					return null;
				case RtmpState.Connect:
				case RtmpState.Handshake:
					return DecodeHandshake(context, stream);
				default:
					return null;
			}
		}
        /// <summary>
        /// Decodes handshake message.
        /// </summary>
        /// <param name="context">RTMP protocol state.</param>
        /// <param name="stream">Buffer to be decoded.</param>
        /// <returns>Buffer with handshake response.</returns>
		public static object DecodeHandshake(RtmpContext context, ByteBuffer stream) 
		{
			long remaining = stream.Remaining;
			if(context.Mode == RtmpMode.Server)
			{
				if(context.State == RtmpState.Connect)
				{
					if(remaining < HandshakeSize + 1) 
					{
#if !SILVERLIGHT
                        if( log.IsDebugEnabled )
                            log.Debug(__Res.GetString(__Res.Rtmp_HSInitBuffering, remaining));
#endif
						context.SetBufferDecoding(HandshakeSize + 1);
						return null;
					}
					else 
					{
#if !SILVERLIGHT
                        if (log.IsDebugEnabled)
                            log.Debug("Handshake 1st phase");
#endif

						ByteBuffer hs = ByteBuffer.Allocate(HandshakeSize + 1);
						ByteBuffer.Put(hs, stream, HandshakeSize + 1);
						hs.Flip();
						context.State = RtmpState.Handshake;
						return hs;
                    }
				}
				if(context.State == RtmpState.Handshake)
				{
                    //if (log.IsDebugEnabled)
                    //    log.Debug("Handshake reply");

					if(remaining < HandshakeSize)
					{
#if !SILVERLIGHT
                        if( log.IsDebugEnabled )
							log.Debug(__Res.GetString(__Res.Rtmp_HSReplyBuffering, remaining));
#endif
						context.SetBufferDecoding(HandshakeSize);
						return null;
					}				 
					else 
					{
						ByteBuffer hs = ByteBuffer.Allocate(HandshakeSize);
						ByteBuffer.Put(hs, stream, HandshakeSize);
						hs.Flip();
						context.State = RtmpState.Connected;
						return hs;
					}
				}
			}
			else
			{
				//Client mode
				if(context.State == RtmpState.Connect)
				{
					int size = (2 * HandshakeSize) + 1;
					if(remaining < size) 
					{
#if !SILVERLIGHT
                        if( log.IsDebugEnabled )
							log.Debug(__Res.GetString(__Res.Rtmp_HSInitBuffering, remaining));
#endif
						context.SetBufferDecoding(size);
						return null;
					}
					else
					{
						ByteBuffer hs = ByteBuffer.Allocate(size);
						ByteBuffer.Put(hs, stream, size);
						hs.Flip();
						context.State = RtmpState.Connected;
						return hs;
					}
				}
			}
			return null;
		}
        /// <summary>
        /// Decodes a RTMP packet.
        /// </summary>
        /// <param name="context">RTMP protocol state.</param>
        /// <param name="stream">Buffer to be decoded.</param>
        /// <returns>The decoded RTMP packet.</returns>
		public static RtmpPacket DecodePacket(RtmpContext context, ByteBuffer stream)
		{
			int remaining = stream.Remaining;
			// We need at least one byte
			if(remaining < 1) 
            {
#if !SILVERLIGHT
                if( log.IsDebugEnabled )
                    log.Debug(__Res.GetString(__Res.Rtmp_DataBuffering, remaining, 1));
#endif
				context.SetBufferDecoding(1);
				return null;
			}
			int position = (int)stream.Position;
			byte headerByte = stream.Get();
			int headerValue;
			int byteCount;
			if((headerByte & 0x3f) == 0) 
			{
				// Two byte header
				if (remaining < 2) 
				{
					stream.Position = position;
#if !SILVERLIGHT
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.Rtmp_DataBuffering, remaining, 2));
#endif
					context.SetBufferDecoding(2);
					return null;
				}
				headerValue = ((int) headerByte & 0xff) << 8 | ((int) stream.Get() & 0xff); 
				byteCount = 2;
			} else if ((headerByte & 0x3f) == 1) {
				// Three byte header
				if (remaining < 3)
				{
					stream.Position = position;
#if !SILVERLIGHT
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.Rtmp_DataBuffering, remaining, 3));
#endif
                    context.SetBufferDecoding(3);
					return null;
				}
				headerValue = ((int) headerByte & 0xff) << 16 | ((int) stream.Get() & 0xff) << 8 | ((int) stream.Get() & 0xff); 
				byteCount = 3;
			} else {
				// Single byte header
				headerValue = (int) headerByte & 0xff;
				byteCount = 1;
			}
			byte channelId = DecodeChannelId(headerValue, byteCount);
			if (channelId < 0) 
				throw new ProtocolException("Bad channel id: " + channelId);
			byte headerSize = DecodeHeaderSize(headerValue, byteCount);
			int headerLength = GetHeaderLength(headerSize);
			headerLength += byteCount - 1;

			//if(headerLength > remaining) 
            if (headerLength + byteCount - 1 > remaining)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Rtmp_HeaderBuffering, remaining));
#endif
                stream.Position = position;
                //context.SetBufferDecoding(headerLength);
                context.SetBufferDecoding(headerLength + byteCount - 1);
                return null;
            }
			// Move the position back to the start
			stream.Position = position;

			RtmpHeader header = DecodeHeader(context, context.GetLastReadHeader(channelId), stream);
#if !SILVERLIGHT
            log.Debug("Decoded " + header);
#endif

			if (header == null) 
				throw new ProtocolException("Header is null, check for error");

			// Save the header
			context.SetLastReadHeader(channelId, header);
			// Check to see if this is a new packet or continue decoding an existing one.
			RtmpPacket packet = context.GetLastReadPacket(channelId);
			if(packet == null) 
			{
				packet = new RtmpPacket(header);
				context.SetLastReadPacket(channelId, packet);
			}

			ByteBuffer buf = packet.Data;
			int addSize = (header.Timer == 0xffffff ? 4 : 0);
            //int addSize = 0;
			int readRemaining = header.Size + addSize - (int)buf.Position;
			int chunkSize = context.GetReadChunkSize();
			int readAmount = (readRemaining > chunkSize) ? chunkSize : readRemaining;
			if(stream.Remaining < readAmount) 
			{
#if !SILVERLIGHT
                if( log.IsDebugEnabled )
					log.Debug(__Res.GetString(__Res.Rtmp_ChunkSmall, stream.Remaining, readAmount));
#endif
				//Skip the position back to the start
				stream.Position = position;
				context.SetBufferDecoding(headerLength + readAmount);
				
				//string path = FluorineFx.Context.FluorineContext.Current.GetFullPath(@"log\chunk.bin");
				//stream.Dump(path);
				return null;
			}
			
			//http://osflash.org/pipermail/free_osflash.org/2005-September/000261.html
			//http://www.acmewebworks.com/Downloads/openCS/091305-initialMeeting.txt
			ByteBuffer.Put(buf, stream, readAmount);
			if(buf.Position < header.Size + addSize) 
			{
				context.ContinueDecoding();
				return null;
			}
            if (buf.Position > header.Size + addSize)
            {
#if !SILVERLIGHT
                log.Warn(string.Format("Packet size expanded from {0} to {1} ({2})", header.Size + addSize, buf.Position, header));
#endif
            }

			buf.Flip();

            try
            {
                IRtmpEvent message = DecodeMessage(context, packet.Header, buf);
                packet.Message = message;

                if (message is ChunkSize)
                {
                    ChunkSize chunkSizeMsg = message as ChunkSize;
                    context.SetReadChunkSize(chunkSizeMsg.Size);
                }
            }
            finally
            {
                context.SetLastReadPacket(channelId, null);
            }
#if !SILVERLIGHT
            if (log.IsDebugEnabled)
            {
                log.Debug("Decoded " + packet.ToString());
            }
#endif
			return packet;
		}
        /// <summary>
        /// Decodes RTMP packet header.
        /// </summary>
        /// <param name="context">RTMP protocol state.</param>
        /// <param name="lastHeader">Previous header.</param>
        /// <param name="stream">Buffer to be decoded.</param>
        /// <returns>Decoded RTMP header.</returns>
		public static RtmpHeader DecodeHeader(RtmpContext context, RtmpHeader lastHeader, ByteBuffer stream)
		{
			byte headerByte = stream.Get();
			int headerValue;
			int byteCount = 1;
			if ((headerByte & 0x3f) == 0) 
			{
				// Two byte header
				headerValue = ((int) headerByte & 0xff) << 8 | ((int) stream.Get() & 0xff); 
				byteCount = 2;
			} 
			else if ((headerByte & 0x3f) == 1) 
			{
				// Three byte header
				headerValue = ((int) headerByte & 0xff) << 16 | ((int) stream.Get() & 0xff) << 8 | ((int) stream.Get() & 0xff); 
				byteCount = 3;
			} 
			else 
			{
				// Single byte header
				headerValue = (int) headerByte & 0xff;
				byteCount = 1;
			}
			byte channelId = DecodeChannelId(headerValue, byteCount);
			byte headerSize = DecodeHeaderSize(headerValue, byteCount);
			RtmpHeader header = new RtmpHeader();
			header.ChannelId = channelId;
			header.IsTimerRelative = (HeaderType)headerSize != HeaderType.HeaderNew;

            if ((HeaderType)headerSize != HeaderType.HeaderNew && lastHeader == null)
            {
#if !SILVERLIGHT
                if(log.IsErrorEnabled)
                    log.Error(string.Format("Last header null not new, headerSize: {0}, channelId {1}", headerSize, channelId));
#endif
                lastHeader = new RtmpHeader();
                lastHeader.ChannelId = channelId;
                lastHeader.IsTimerRelative = (HeaderType)headerSize != HeaderType.HeaderNew;
            }


#if !SILVERLIGHT
            if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.Rtmp_DecodeHeader, Enum.GetName(typeof(HeaderType), (HeaderType)headerSize)));
#endif

			switch((HeaderType)headerSize)
			{
				case HeaderType.HeaderNew:
                    header.Timer = stream.ReadUInt24();// ReadUnsignedMediumInt();
                    header.Size = stream.ReadUInt24();// ReadMediumInt();
					header.DataType = stream.Get();
					header.StreamId = stream.ReadReverseInt();
					break;
				case HeaderType.HeaderSameSource:
                    header.Timer = stream.ReadUInt24();// ReadUnsignedMediumInt();
                    header.Size = stream.ReadUInt24();// ReadMediumInt();
					header.DataType = stream.Get();
					header.StreamId = lastHeader.StreamId;
					break;
				case HeaderType.HeaderTimerChange:
                    header.Timer = stream.ReadUInt24();//ReadUnsignedMediumInt();
                    header.Size = lastHeader.Size;
                    header.DataType = lastHeader.DataType;
                    header.StreamId = lastHeader.StreamId;
					break;
				case HeaderType.HeaderContinue:
                    header.Timer = lastHeader.Timer;
                    header.Size = lastHeader.Size;
                    header.DataType = lastHeader.DataType;
                    header.StreamId = lastHeader.StreamId;
                    header.IsTimerRelative = lastHeader.IsTimerRelative;
					break;
				default:
#if !SILVERLIGHT
                    log.Error("Unexpected header size: " + headerSize);
#endif
					return null;
			}
            if (header.Timer >= 0xffffff)
            {
                //Extended timestamp
                header.Timer = stream.GetInt();
            }

			return header;
		}
        /// <summary>
        /// Decodes RTMP message event.
        /// </summary>
        /// <param name="context">RTMP protocol state.</param>
        /// <param name="header">RTMP header.</param>
        /// <param name="stream">Buffer to be decoded.</param>
        /// <returns>Decoded RTMP event.</returns>
		public static IRtmpEvent DecodeMessage(RtmpContext context, RtmpHeader header, ByteBuffer stream)
		{
			IRtmpEvent message = null;
            /*
			if(header.Timer == 0xffffff) 
			{
				// Skip first four bytes
                byte[] extendedTimestamp = new byte[4];
                stream.Read(extendedTimestamp, 0, 4);
                log.Warn("Discarding extended timestamp");
				//int unknown = stream.ReadInt32();
			}
            */
			switch(header.DataType) 
			{
                case Constants.TypeChunkSize:
					message = DecodeChunkSize(stream);
					break;
                case Constants.TypeInvoke:
					message = DecodeInvoke(stream);
					break;
                case Constants.TypeFlexInvoke:
					message = DecodeFlexInvoke(stream);
					break;
                case Constants.TypeNotify:
					if( header.StreamId == 0 )
						message = DecodeNotify(stream, header);
					else
						message = DecodeStreamMetadata(stream);
					break;
                case Constants.TypePing:
					message = DecodePing(stream);
					break;
                case Constants.TypeBytesRead:
					message = DecodeBytesRead(stream);
					break;
                case Constants.TypeAudioData:
					message = DecodeAudioData(stream);
					break;
                case Constants.TypeVideoData:
					message = DecodeVideoData(stream);
					break;
                case Constants.TypeSharedObject:
					message = DecodeSharedObject(stream);
					break;
                case Constants.TypeFlexSharedObject:
					message = DecodeFlexSharedObject(stream);
					break;
                case Constants.TypeServerBandwidth:
					message = DecodeServerBW(stream);
					break;
                case Constants.TypeClientBandwidth:
					message = DecodeClientBW(stream);
					break;
				default:
#if !SILVERLIGHT
                    log.Warn("Unknown object type: " + header.DataType);
#endif
					message = DecodeUnknown(stream);
					break;
			}
			message.Header = header;
			message.Timestamp = header.Timer;
			return message;
		}

		static Ping DecodePing(ByteBuffer stream)
		{
			Ping ping = new Ping();
			ping.PingType = stream.GetShort();
			ping.Value2 = stream.GetInt();
			if(stream.HasRemaining)
				ping.Value3 = stream.GetInt();
			if(stream.HasRemaining)
				ping.Value4 = stream.GetInt();
			return ping;
		}

		static AudioData DecodeAudioData(ByteBuffer stream)
		{
			return new AudioData(stream);
		}

		static VideoData DecodeVideoData(ByteBuffer stream)
		{
			return new VideoData(stream);
		}

		static ChunkSize DecodeChunkSize(ByteBuffer stream) 
		{
			return new ChunkSize(stream.GetInt());
		}

		static BytesRead DecodeBytesRead(ByteBuffer stream)
		{
            return new BytesRead(stream.GetInt());
		}

		static ServerBW DecodeServerBW(ByteBuffer stream) 
		{
			return new ServerBW(stream.GetInt());
		}

		static ClientBW DecodeClientBW(ByteBuffer stream) 
		{
			return new ClientBW(stream.GetInt(), stream.Get());
		}

		static Unknown DecodeUnknown(ByteBuffer stream) 
		{
			return new Unknown(stream);
		}

		static Invoke DecodeInvoke(ByteBuffer stream)
		{
			return DecodeNotifyOrInvoke(new Invoke(), stream, null) as Invoke;
		}

		public static void test()
		{
			var data = File.ReadAllBytes("C:\\Users\\Admin\\Desktop\\packet.dmp");
			DecodeFlexInvoke(ByteBuffer.Wrap(data));
		}

		static FlexInvoke DecodeFlexInvoke(ByteBuffer stream)
		{
			int version = stream.ReadByte();
			RtmpReader reader = new RtmpReader(stream);
            string action = reader.ReadData() as string;
			int invokeId = System.Convert.ToInt32(reader.ReadData());
			object cmdData = reader.ReadData();

            object[] parameters = Call.EmptyArguments;
            if (stream.HasRemaining)
            {
#if !(NET_1_1)
                List<object> paramList = new List<object>();
#else
            ArrayList paramList = new ArrayList();
#endif
                while (stream.HasRemaining)
                {
                    object obj = reader.ReadData();
                    paramList.Add(obj);
                }
                parameters = paramList.ToArray();
            }
            /*
            int dotIndex = action == null ? -1 : action.LastIndexOf(".");
            string serviceName = (action == -1) ? null : action.Substring(0, dotIndex);
            string serviceMethod = (dotIndex == -1) ? action : action.Substring(dotIndex + 1, action.Length - dotIndex - 1);
            */
            PendingCall call = new PendingCall(null, action, parameters);
            FlexInvoke invoke = new FlexInvoke(invokeId, cmdData);
            invoke.ServiceCall = call;
			return invoke;
		}

		static Notify DecodeStreamMetadata(ByteBuffer stream) 
		{
            RtmpReader reader = new RtmpReader(stream);
            string action = reader.ReadData() as string;
            object obj = reader.ReadData();
            Notify notify = new Notify();
			int dotIndex = action.LastIndexOf(".");
			string serviceName = (dotIndex == -1) ? null : action.Substring(0, dotIndex);
            string serviceMethod = (dotIndex == -1) ? action : action.Substring(dotIndex + 1, action.Length - dotIndex - 1);
            object[] parameters = new object[1]{obj};
			Call call = new Call(serviceName, serviceMethod, parameters);
			notify.ServiceCall = call;
            return notify;
		}

		static Notify DecodeNotify(ByteBuffer stream, RtmpHeader header)
		{
			return DecodeNotifyOrInvoke(new Notify(), stream, header);
		}


		static Notify DecodeNotifyOrInvoke(Notify notify, ByteBuffer stream, RtmpHeader header)
		{
			long start = stream.Position;
			RtmpReader reader = new RtmpReader(stream);
			string action = reader.ReadData() as string;

			if(!(notify is Invoke))
			{
				//Don't decode "NetStream.send" requests
				stream.Position = start;
                notify.Data = ByteBuffer.Allocate(stream.Remaining);
                notify.Data.Put(stream);
				//notify.setData(in.asReadOnlyBuffer());
				return notify;
			}

			if(header == null || header.StreamId == 0) 
			{
				double invokeId = (double)reader.ReadData();
				notify.InvokeId = (int)invokeId;
			}

            object[] parameters = Call.EmptyArguments;
			if(stream.HasRemaining)
			{
#if !(NET_1_1)
                List<object> paramList = new List<object>();
#else
                ArrayList paramList = new ArrayList();
#endif
                object obj = reader.ReadData();

				if (obj is IDictionary)
				{
					// for connect we get a map
					notify.ConnectionParameters = obj as IDictionary;
				} 
				else if (obj != null) 
				{
					paramList.Add(obj);
				}

				while(stream.HasRemaining)
				{
					paramList.Add(reader.ReadData());
				}
				parameters = paramList.ToArray();
			}

			int dotIndex = action.LastIndexOf(".");
			string serviceName = (dotIndex == -1) ? null : action.Substring(0, dotIndex);
            string serviceMethod = (dotIndex == -1) ? action : action.Substring(dotIndex + 1, action.Length - dotIndex - 1);

			if (notify is Invoke)
			{
				PendingCall call = new PendingCall(serviceName, serviceMethod, parameters);
                notify.ServiceCall = call;
			} 
			else 
			{
				Call call = new Call(serviceName, serviceMethod, parameters);
				notify.ServiceCall = call;
			}
			return notify;
		}

		static void DecodeSharedObject(SharedObjectMessage so, ByteBuffer stream, RtmpReader reader)
		{
			// Parse request body
			while(stream.HasRemaining)
			{
				byte typeCode = reader.ReadByte();
				SharedObjectEventType type = SharedObjectTypeMapping.ToType(typeCode);
				string key = null;
				object value = null;

				int length = stream.GetInt();//reader.ReadInt32();
				switch(type)
				{
					case SharedObjectEventType.CLIENT_STATUS:
						// Status code
						key = reader.ReadString();
						// Status level
						value = reader.ReadString();
						break;
					case SharedObjectEventType.CLIENT_UPDATE_DATA:
					{
                        key = reader.ReadString();
                        value = reader.ReadData();
                        /*
						key = null;
						// Map containing new attribute values
						Hashtable map = new Hashtable();
						int start = (int)stream.Position;
						while((int)stream.Position - start < length) 
						{
							string tmp = reader.ReadString();
							map[tmp] = reader.ReadData();
						}
						value = map;
                        */
					}
						break;
					default:
						if (type != SharedObjectEventType.SERVER_SEND_MESSAGE && type != SharedObjectEventType.CLIENT_SEND_MESSAGE) 
						{
							if (length > 0) 
							{
								key = reader.ReadString();
								if(length > key.Length + 2) 
								{
									value = reader.ReadData();
								}
							}
						} 
						else 
						{
							int start = (int)stream.Position;
							// the "send" event seems to encode the handler name
							// as complete AMF string including the string type byte
							key = reader.ReadData() as string;
							// read parameters
#if !(NET_1_1)
                            List<object> paramList = new List<object>();
#else
							ArrayList paramList = new ArrayList();
#endif
                            while (stream.Position - start < length)
							{
								object tmp = reader.ReadData();
								paramList.Add(tmp);
							}
							value = paramList;
						}
						break;
				}
				so.AddEvent(type, key, value);
			}
		}

		static ISharedObjectMessage DecodeSharedObject(ByteBuffer stream)
		{
			RtmpReader reader = new RtmpReader(stream);
			string name = reader.ReadString();
			// Read version of SO to modify
			int version = reader.ReadInt32();
			// Read persistence informations
			bool persistent = reader.ReadInt32() == 2;
			// Skip unknown bytes
			//skip(4);
			reader.ReadInt32();

			SharedObjectMessage so = new SharedObjectMessage(null, name, version, persistent);
			DecodeSharedObject(so, stream, reader);
			return so;
		}

		static ISharedObjectMessage DecodeFlexSharedObject(ByteBuffer stream)
		{
			// Unknown byte, always 0?
			stream.Skip(1);
			RtmpReader reader = new RtmpReader(stream);
			string name = reader.ReadString();
			// Read version of SO to modify
			int version = reader.ReadInt32();
			// Read persistence informations
			bool persistent = reader.ReadInt32() == 2;
			// Skip unknown bytes
			reader.ReadInt32();

			SharedObjectMessage so = new FlexSharedObjectMessage(null, name, version, persistent);
			DecodeSharedObject(so, stream, reader);
			return so;
		}
	}
}
