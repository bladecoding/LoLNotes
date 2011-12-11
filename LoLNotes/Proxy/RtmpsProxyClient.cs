/*
copyright (C) 2011 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

//#define FILETESTING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using FluorineFx;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Util;
using LoLNotes.Messaging.Messages;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyClient : ProxyClient
	{
		const bool encode = false;

		protected RtmpContext sourcecontext = new RtmpContext(RtmpMode.Server) { ObjectEncoding = ObjectEncoding.AMF0 };
		protected RtmpContext remotecontext = new RtmpContext(RtmpMode.Client) { ObjectEncoding = ObjectEncoding.AMF0 };
		readonly ByteBuffer sendbuffer = new ByteBuffer(new MemoryStream());
		readonly ByteBuffer receivebuffer = new ByteBuffer(new MemoryStream());
		readonly List<Notify> InvokeList = new List<Notify>();

		public new RtmpsProxyHost Host { get; protected set; }

		protected ByteBuffer postbuffer = new ByteBuffer(new MemoryStream());


		public RtmpsProxyClient(IProxyHost host, TcpClient src)
			: base(host, src)
		{
			if (!(host is RtmpsProxyHost))
				throw new ArgumentException("Expected RtmpsProxyHost, got " + host.GetType());

			Host = (RtmpsProxyHost)host;
		}
		/// <summary>
		/// Only for test driven.
		/// </summary>
		RtmpsProxyClient()
			: base(null, null)
		{
		}

		int SkipPost(byte[] buf, int idx, int len)
		{
			postbuffer.Append(buf, idx, len);
			if (postbuffer.GetInt() == 0x504f5354) //POST
			{
				var find = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A, 0x00 };
				int fidx = 0;
				while (postbuffer.Position < postbuffer.Length)
				{
					fidx = postbuffer.Get() == find[fidx] ? fidx + 1 : 0;
					if (fidx == find.Length)
					{
						//Found the end of a post request. Return how much of the buffer to skip.
						return (int)(postbuffer.Position - (postbuffer.Length - len));
					}
				}

				//End of post was not found, return len to prevent OnSend from decoding it.
				postbuffer.Rewind();
				return len;
			}

			//Post buffer did not start with POST. Stop looking for it.
			postbuffer = null;
			return idx;
		}

		protected override void OnSend(byte[] buffer, int idx, int len)
		{
			//if (postbuffer != null)
			//{
			//    idx = SkipPost(buffer, idx, len);
			//    len -= idx;

			//    //Post was found, lets tell the client to continue
			//    if (postbuffer == null && idx != 0)
			//    {
			//        var str = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=UTF-8\r\nVary: Accept-Encoding\r\nPragma: no-cache\r\nCache-Control: no-cache\r\nContent-Encoding: gzip\r\nServer: None\r\nContent-Length: 1\r\nDate: Sun, 11 Dec 2011 21:43:57 GMT\r\nConnection: keep-alive\r\n\r\n\0";
			//        base.OnReceive(Encoding.ASCII.GetBytes(str), 0, str.Length);
			//    }
			//}

			if (!encode)
				base.OnSend(buffer, idx, len);

			StaticLogger.Trace(string.Format("Send {0} bytes", len));

#if !FILETESTING
			using (var fs = File.Open("realsend.dmp", FileMode.Append, FileAccess.Write))
			{
				fs.Write(buffer, idx, len);
			}
#endif

#if FILETESTING
			buffer = File.ReadAllBytes("realsend.dmp");
			len = buffer.Length;
#endif

			sendbuffer.Append(buffer, idx, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(sourcecontext, sendbuffer);
			if (objs == null || objs.Count < 1)
				return;

#if FILETESTING
			sourcecontext.ObjectEncoding = ObjectEncoding.AMF3;
			for (int i = 0; i < objs.Count; i++)
				RtmpProtocolEncoder.Encode(sourcecontext, objs[i]);

			//var bufferf = buffs.ToArray();
			//using (var fs = File.Open("send.dmp", FileMode.Append, FileAccess.Write))
			//{
			//    fs.Write(bufferf, 0, bufferf.Length);
			//}
			return;
#endif

			foreach (var obj in objs)
			{
				var pck = obj as RtmpPacket;
				if (pck != null)
				{
					var inv = pck.Message as Notify;
					if (inv != null)
					{
						lock (InvokeList)
						{
							InvokeList.Add(inv);
						}
						StaticLogger.Trace(
							string.Format("Call {0}({1}) (Id:{2})",
								inv.ServiceCall.ServiceMethodName,
								string.Join(", ", inv.ServiceCall.Arguments.Select(o => o.ToString())),
								pck.Header.ChannelId
							)
						);
					}
					else
					{
						StaticLogger.Trace(string.Format("Sent {0} (Id:{1})", pck.Message.GetType(), pck.Header.ChannelId));
					}
				}

				if (obj != null)
				{
					var buf = RtmpProtocolEncoder.Encode(sourcecontext, obj);
					if (pck != null && pck.Message is Notify)
					{
						sourcecontext.ObjectEncoding = ObjectEncoding.AMF3;
					}
					if (buf == null)
					{
						StaticLogger.Fatal("Unable to encode " + obj);
					}
					else
					{
						var buff = buf.ToArray();
						using (var fs = File.Open("send.dmp", FileMode.Append, FileAccess.Write))
						{
							fs.Write(buff, idx, buff.Length);
						}
						if (encode)
							base.OnSend(buff, idx, buff.Length);
					}
				}
			}

		}

		protected override void OnReceive(byte[] buffer, int idx, int len)
		{
			if (!encode)
				base.OnReceive(buffer, idx, len);

			StaticLogger.Trace(string.Format("Recv {0} bytes", len));

#if !FILETESTING
			using (var fs = File.Open("realrecv.dmp", FileMode.Append, FileAccess.Write))
			{
				fs.Write(buffer, idx, len);
			}
#endif

#if FILETESTING
			buffer = File.ReadAllBytes("realrecv.dmp");
			len = buffer.Length;
#endif

			receivebuffer.Append(buffer, idx, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(remotecontext, receivebuffer);
			if (objs == null || objs.Count < 1)
				return;

#if FILETESTING
			remotecontext.ObjectEncoding = ObjectEncoding.AMF3;
			for (int i = 0; i < objs.Count; i++)
				RtmpProtocolEncoder.Encode(remotecontext, objs[i]);

			//var bufferf = buffs.ToArray();
			//using (var fs = File.Open("recv.dmp", FileMode.Append, FileAccess.Write))
			//{
			//    fs.Write(bufferf, 0, bufferf.Length);
			//}
			return;
#endif

			foreach (var obj in objs)
			{
				var pck = obj as RtmpPacket;
				if (pck != null)
				{
					var result = pck.Message as Notify;
					if (result != null)
					{
						Notify inv = null;
						if (result.ServiceCall.ServiceMethodName == "_result" || result.ServiceCall.ServiceMethodName == "_error")
						{
							lock (InvokeList)
							{
								int fidx = InvokeList.FindIndex(i => i.InvokeId == result.InvokeId);
								if (fidx == -1)
								{
									StaticLogger.Warning(string.Format("Call not found for {0} (Id:{1})", result.InvokeId, pck.Header.ChannelId));
								}
								else
								{
									inv = InvokeList[fidx];
									InvokeList.RemoveAt(fidx);
								}
							}

							if (inv != null)
							{
								OnCall(inv, result);

								StaticLogger.Trace(
									string.Format(
										"Ret  ({0}) (Id:{1})",
										string.Join(", ", inv.ServiceCall.Arguments.Select(o => o.ToString())),
										pck.Header.ChannelId
									)
								);
							}
						}

						//Call was not found. Most likely a receive message.
						if (inv != null)
						{
							OnNotify(result);
						}
					}
					else
					{
						StaticLogger.Trace(string.Format("Recv {0} (Id:{1})", pck.Message, pck.Header.ChannelId));
					}
				}

				if (obj != null)
				{
					var buf = RtmpProtocolEncoder.Encode(remotecontext, obj);
					if (pck != null && pck.Message is Notify)
					{
						remotecontext.ObjectEncoding = ObjectEncoding.AMF3;
					}
					if (buf == null)
					{
						StaticLogger.Fatal("Unable to encode " + obj);
					}
					else
					{
						var buff = buf.ToArray();
						using (var fs = File.Open("recv.dmp", FileMode.Append, FileAccess.Write))
						{
							fs.Write(buff, idx, buff.Length);
						}
						if (encode)
							base.OnReceive(buff, idx, buff.Length);
					}
				}
			}
		}

		protected virtual void OnCall(Notify call, Notify result)
		{
			OnNotify(result);
		}
		protected virtual void OnNotify(Notify result)
		{
			foreach (var arg in result.ServiceCall.Arguments)
			{
				Int64 timestamp = 0;
				ASObject obj = null;
				if (arg is AbstractMessage)
				{
					var msg = (AbstractMessage)arg;
					obj = msg.Body as ASObject;
					timestamp = msg.TimeStamp;
				}
				else if (arg is MessageBase)
				{
					var msg = (MessageBase)arg;
					obj = msg.body as ASObject;
					timestamp = msg.timestamp;
				}

				if (obj != null)
					Host.OnProcessObject(obj, timestamp);
			}
		}
	}
}
