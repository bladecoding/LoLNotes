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
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Util;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyClient : ProxyClient
	{
		protected RtmpContext sourcecontext = new RtmpContext(RtmpMode.Server) { ObjectEncoding = ObjectEncoding.AMF3 };
		protected RtmpContext remotecontext = new RtmpContext(RtmpMode.Client) { ObjectEncoding = ObjectEncoding.AMF3 };
		ByteBuffer sendbuffer = new ByteBuffer(new MemoryStream());
		ByteBuffer receivebuffer = new ByteBuffer(new MemoryStream());

		public RtmpsProxyClient(IProxyHost host, TcpClient src)
			: base(host, src)
		{
		}

		protected override void OnSend(byte[] buffer, int len)
		{
			sendbuffer.Append(buffer, 0, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(sourcecontext, sendbuffer);
			foreach (var obj in objs)
			{
				var pck = obj as RtmpPacket;
				if (pck != null)
				{
					StaticLogger.Info(string.Format("Sent {0} (Id:{1})", pck.Message, pck.Header.ChannelId));
				}
			}

			base.OnSend(buffer, len);
		}

		string ObjectToString(object obj, int level = 0)
		{
			var kvs = obj as ASObject;
			if (kvs != null)
			{
				return kvs.Aggregate(
					"",
					(current, kv) => current + string.Format(
						"{0}{1} = {2}\n",
						"".PadLeft(level, '\t'),
						kv.Key,
						kv.Value is ASObject ? ((ASObject)kv.Value).TypeName + "\n" + ObjectToString(kv.Value, level + 1) : kv.Value
					)
				);
			}
			return obj.ToString();
		}

		protected override void OnReceive(byte[] buffer, int len)
		{
			receivebuffer.Append(buffer, 0, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(remotecontext, receivebuffer);
			foreach (var obj in objs)
			{
				var pck = obj as RtmpPacket;
				if (pck != null)
				{
					var inv = pck.Message as FlexInvoke;
					if (inv != null && inv.Parameters.Length == 1)
					{
						var msg = inv.Parameters[0] as DSK;
						if (msg != null)
						{
							var ao = msg.Body as ASObject;
							if (ao != null)
							{
								string str = ObjectToString(ao);
								StaticLogger.Info(string.Format("Recv DSK  (Id:{0}) [\n{1}\n]", pck.Header.ChannelId, str));
							}
						}
					}
					StaticLogger.Info(string.Format("Recv {0} (Id:{1})", pck.Message, pck.Header.ChannelId));
				}
			}

			if (remotecontext.State == RtmpState.Handshake)
				remotecontext.State = RtmpState.Connected;

			base.OnReceive(buffer, len);
		}
	}
}
