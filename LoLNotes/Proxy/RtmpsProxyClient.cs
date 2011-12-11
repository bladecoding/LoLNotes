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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using FluorineFx;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Util;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyClient : ProxyClient
	{
		protected RtmpContext sourcecontext = new RtmpContext(RtmpMode.Server) { ObjectEncoding = ObjectEncoding.AMF0 };
		protected RtmpContext remotecontext = new RtmpContext(RtmpMode.Client) { ObjectEncoding = ObjectEncoding.AMF0 };
		readonly ByteBuffer sendbuffer = new ByteBuffer(new MemoryStream());
		readonly ByteBuffer receivebuffer = new ByteBuffer(new MemoryStream());
		readonly List<Invoke> InvokeList = new List<Invoke>();

		public RtmpsProxyClient(IProxyHost host, TcpClient src)
			: base(host, src)
		{
		}
		public RtmpsProxyClient()
			: base(null, null)
		{
		}

		const bool encode = true;

		protected override void OnSend(byte[] buffer, int len)
		{
			if (!encode)
				base.OnSend(buffer, len);

			StaticLogger.Trace(string.Format("Send {0} bytes", len));

#if !TESTING
			using (var fs = File.Open("realsend.dmp", FileMode.Append, FileAccess.Write))
			{
				fs.Write(buffer, 0, len);
			}
#endif

#if TESTING
			buffer = File.ReadAllBytes("realsend.dmp");
			len = buffer.Length;
#endif

			sendbuffer.Append(buffer, 0, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(sourcecontext, sendbuffer);
			if (objs == null || objs.Count < 1)
				return;

#if TESTING
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
					var inv = pck.Message as Invoke;
					if (inv != null)
					{
						lock (InvokeList)
						{
							InvokeList.Add(inv);
						}
						StaticLogger.Info(
							string.Format("Call {0}({1}) (Id:{2})",
								inv.ServiceCall.ServiceMethodName,
								string.Join(", ", inv.ServiceCall.Arguments.Select(o => o.ToString())),
								pck.Header.ChannelId
							)
						);
					}
					else
					{
						StaticLogger.Info(string.Format("Sent {0} (Id:{1})", pck.Message.GetType(), pck.Header.ChannelId));
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
							fs.Write(buff, 0, buff.Length);
						}
						if (encode)
							base.OnSend(buff, buff.Length);
					}
				}
			}

		}

		protected override void OnReceive(byte[] buffer, int len)
		{
			if (!encode)
				base.OnReceive(buffer, len);

			StaticLogger.Trace(string.Format("Recv {0} bytes", len));

#if !TESTING
			using (var fs = File.Open("realrecv.dmp", FileMode.Append, FileAccess.Write))
			{
				fs.Write(buffer, 0, len);
			}
#endif

#if TESTING
			buffer = File.ReadAllBytes("realrecv.dmp");
			len = buffer.Length;
#endif

			receivebuffer.Append(buffer, 0, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(remotecontext, receivebuffer);
			if (objs == null || objs.Count < 1)
				return;

#if TESTING
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
					var result = pck.Message as Invoke;
					if (result != null)
					{
						if (result.ServiceCall.ServiceMethodName == "_result" || result.ServiceCall.ServiceMethodName == "_error")
						{
							Invoke inv = null;
							lock (InvokeList)
							{
								int idx = InvokeList.FindIndex(i => i.InvokeId == result.InvokeId);
								if (idx == -1)
								{
									StaticLogger.Error(string.Format("Call not found for {0} (Id:{1})", result.InvokeId, pck.Header.ChannelId));
								}
								else
								{
									inv = InvokeList[idx];
									InvokeList.RemoveAt(idx);
								}
							}

							if (inv != null)
							{
								StaticLogger.Info(
									string.Format(
										"Ret  ({0}) (Id:{1})",
										string.Join(", ", inv.ServiceCall.Arguments.Select(o => o.ToString())),
										pck.Header.ChannelId
									)
								);
							}
						}
					}
					else
					{
						StaticLogger.Info(string.Format("Recv {0} (Id:{1})", pck.Message, pck.Header.ChannelId));
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
							fs.Write(buff, 0, buff.Length);
						}
						if (encode)
							base.OnReceive(buff, buff.Length);
					}
				}
			}

		}
	}
}
