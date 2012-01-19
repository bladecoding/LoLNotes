/*
copyright (C) 2011-2012 by high828@gmail.com

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
using LoLNotes.Util;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyClient : ProxyClient
	{
		const bool encode = true;
		const bool logtofiles = false;

		public new RtmpsProxyHost Host { get; protected set; }
		protected RtmpContext sourcecontext = new RtmpContext(RtmpMode.Server) { ObjectEncoding = ObjectEncoding.AMF0 };
		protected RtmpContext remotecontext = new RtmpContext(RtmpMode.Client) { ObjectEncoding = ObjectEncoding.AMF0 };
		protected ByteBuffer postbuffer = new ByteBuffer(new MemoryStream());

		protected readonly ByteBuffer sendbuffer = new ByteBuffer(new MemoryStream());
		protected readonly ByteBuffer receivebuffer = new ByteBuffer(new MemoryStream());
		/// <summary>
		/// This invoke list is used to record call/returns by the client.
		/// </summary>
		protected readonly List<Notify> InvokeList = new List<Notify>();
		/// <summary>
		/// This invoke list is used to record our call/returns.
		/// </summary>
		protected List<CallResultWait> WaitInvokeList = new List<CallResultWait>();
		protected readonly object WaitLock = new object();

		protected readonly Dictionary<int, int> InvokeIds = new Dictionary<int, int>();
		protected AtomicInteger CurrentInvoke = new AtomicInteger();

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

		/// <summary>
		/// Call blocks until the result is received. Use Send for a nonblocking call.
		/// </summary>
		/// <param name="notify">Call</param>
		/// <returns>Result or null if failed</returns>
		public Notify Call(Notify notify)
		{
			var callresult = new CallResultWait(notify, true);
			lock (WaitLock)
			{
				if (WaitInvokeList == null)
					return null;
				WaitInvokeList.Add(callresult);
			}
			notify.InvokeId = CurrentInvoke.Increment();

			InternalSend(notify, false);

			callresult.Wait.WaitOne(-1);
			return callresult.Result;
		}

		/// <summary>
		/// Send does not block and returns immediately. Use Call for a blocking call
		/// </summary>
		/// <param name="notify">Call</param>
		public void Send(Notify notify)
		{
			//Might as well use the waitlist so InternalReceive doesn't freak out about the invoke id not being found.
			lock (WaitLock)
			{
				if (WaitInvokeList == null)
					return;
				WaitInvokeList.Add(new CallResultWait(notify, false));
			}

			notify.InvokeId = CurrentInvoke.Increment();

			InternalSend(notify, false);
		}

		/// <summary>
		/// Replaces the invokeid
		/// </summary>
		/// <param name="notify"></param>
		protected void InternalReceive(Notify notify)
		{
			if (RtmpUtil.IsResult(notify))
			{
				int ourid = notify.InvokeId;

				CallResultWait callresult = null;
				lock (WaitLock)
				{
					if (WaitInvokeList != null)
					{
						int idx = WaitInvokeList.FindIndex(crw => crw.Call.InvokeId == ourid);
						if (idx != -1)
						{
							callresult = WaitInvokeList[idx];
							WaitInvokeList.RemoveAt(idx);
						}
					}
				}

				//InvokeId was found in the waitlist, that means its one of our calls.
				if (callresult != null)
				{
					callresult.Result = notify;
					callresult.Wait.Set();

					//Not blocking, lets send it to the handler instead.
					if (!callresult.Blocking)
						OnCall(callresult.Call, callresult.Result);

					return; //Return, we don't want LoL receiving the message.
				}

				int theirid;
				lock (InvokeIds)
				{
					if (!InvokeIds.TryGetValue(ourid, out theirid))
					{
						StaticLogger.Error(string.Format("Call id not found for {0}", notify));
						return;
					}
					InvokeIds.Remove(ourid);
				}
				notify.InvokeId = theirid;

			}

			var buf = RtmpProtocolEncoder.Encode(remotecontext, CreatePacket(notify));
			if (buf == null)
			{
				StaticLogger.Fatal("Unable to encode " + notify);
			}
			else
			{
				var buff = buf.ToArray();
				if (logtofiles)
				{
					using (var fs = File.Open("recv.dmp", FileMode.Append, FileAccess.Write))
					{
						fs.Write(buff, 0, buff.Length);
					}
				}
				if (encode)
					base.OnReceive(buff, 0, buff.Length);
			}
		}
		/// <summary>
		/// Replaces the invokeid
		/// </summary>
		/// <param name="notify"></param>
		protected void InternalSend(Notify notify, bool overwriteid)
		{
			if (overwriteid)
			{
				int current = CurrentInvoke.Increment();
				lock (InvokeIds)
					InvokeIds.Add(current, notify.InvokeId);
				notify.InvokeId = current;
			}

			var buf = RtmpProtocolEncoder.Encode(sourcecontext, CreatePacket(notify));
			if (buf == null)
			{
				StaticLogger.Fatal("Unable to encode " + notify);
			}
			else
			{
				var buff = buf.ToArray();
				if (logtofiles)
				{
					using (var fs = File.Open("send.dmp", FileMode.Append, FileAccess.Write))
					{
						fs.Write(buff, 0, buff.Length);
					}
				}
				if (encode)
					base.OnSend(buff, 0, buff.Length);
			}
		}

		public RtmpPacket CreatePacket(Notify notify)
		{
			var header = new RtmpHeader
			{
				ChannelId = 3,
				DataType = notify.DataType,
				Timer = notify.Timestamp
			};
			return new RtmpPacket(header, notify);
		}

		protected override void OnSend(byte[] buffer, int idx, int len)
		{
			if (postbuffer != null)
			{
				postbuffer.Append(buffer, idx, len);
				if (postbuffer.Length > 4)
				{
					int num = postbuffer.GetInt();
					postbuffer.Dispose();
					postbuffer = null;
					if (num == 0x504f5354)
					{
						StaticLogger.Trace(string.Format("Rejecting POST request", len));
						Stop();
						return;
					}
				}
			}

			StaticLogger.Trace(string.Format("Send {0} bytes", len));

			if (logtofiles)
			{
				using (var fs = File.Open("realsend.dmp", FileMode.Append, FileAccess.Write))
				{
					fs.Write(buffer, idx, len);
				}
			}

			sendbuffer.Append(buffer, idx, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(sourcecontext, sendbuffer);
			if (objs != null)
			{
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
					else if (obj is ByteBuffer)
					{
						//Just handshakes, ignore
					}
					else
					{
						StaticLogger.Warning(string.Format("Unknown object {0}", obj.GetType()));
					}

					if (obj != null && encode)
					{
						if (pck != null && pck.Message is Notify)
						{
							InternalSend((Notify)pck.Message, true);
							sourcecontext.ObjectEncoding = ObjectEncoding.AMF3;
						}
						else
						{
							var buf = RtmpProtocolEncoder.Encode(sourcecontext, obj);
							if (buf == null)
							{
								StaticLogger.Fatal("Unable to encode " + obj);
							}
							else
							{
								var buff = buf.ToArray();
								if (logtofiles)
								{
									using (var fs = File.Open("send.dmp", FileMode.Append, FileAccess.Write))
									{
										fs.Write(buff, 0, buff.Length);
									}
								}
								if (encode)
									base.OnSend(buff, 0, buff.Length);
							}
						}
					}
				}
			}

			if (!encode)
				base.OnSend(buffer, idx, len);
		}

		protected override void OnReceive(byte[] buffer, int idx, int len)
		{
			StaticLogger.Trace(string.Format("Recv {0} bytes", len));

			if (logtofiles)
			{
				using (var fs = File.Open("realrecv.dmp", FileMode.Append, FileAccess.Write))
				{
					fs.Write(buffer, idx, len);
				}
			}

			receivebuffer.Append(buffer, idx, len);

			var objs = RtmpProtocolDecoder.DecodeBuffer(remotecontext, receivebuffer);
			if (objs != null)
			{
				foreach (var obj in objs)
				{
					var pck = obj as RtmpPacket;
					if (pck != null)
					{
						var result = pck.Message as Notify;
						if (result != null)
						{
							Notify inv = null;
							if (RtmpUtil.IsResult(result))
							{
								lock (InvokeList)
								{
									int fidx = InvokeList.FindIndex(i => i.InvokeId == result.InvokeId);
									if (fidx != -1)
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
							else
							{
								OnNotify(result);
							}
						}
						else
						{
							StaticLogger.Trace(string.Format("Recv {0} (Id:{1})", pck.Message, pck.Header.ChannelId));
						}
					}
					else if (obj is ByteBuffer)
					{
						//Just handshakes, ignore
					}
					else
					{
						StaticLogger.Warning(string.Format("Unknown object {0}", obj.GetType()));
					}

					if (obj != null && encode)
					{

						if (pck != null && pck.Message is Notify)
						{
							InternalReceive((Notify)pck.Message);
							remotecontext.ObjectEncoding = ObjectEncoding.AMF3;
						}
						else
						{
							var buf = RtmpProtocolEncoder.Encode(remotecontext, obj);
							if (buf == null)
							{
								StaticLogger.Fatal("Unable to encode " + obj);
							}
							else
							{
								var buff = buf.ToArray();
								if (logtofiles)
								{
									using (var fs = File.Open("recv.dmp", FileMode.Append, FileAccess.Write))
									{
										fs.Write(buff, 0, buff.Length);
									}
								}
								if (encode)
									base.OnReceive(buff, 0, buff.Length);
							}
						}
					}
				}
			}

			if (!encode)
				base.OnReceive(buffer, idx, len);

		}

		protected virtual void OnCall(Notify call, Notify result)
		{
			Host.OnCall(this, call, result);
		}
		protected virtual void OnNotify(Notify notify)
		{
			Host.OnNotify(this, notify);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				//Lets release all waits when the client is disposed.
				lock (WaitLock)
				{
					if (WaitInvokeList != null)
					{
						foreach (var wait in WaitInvokeList)
							wait.Wait.Set();
						WaitInvokeList = null;
					}
				}
			}
			base.Dispose(disposing);
		}
	}
}
