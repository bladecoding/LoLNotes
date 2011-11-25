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
using System.Net;
using System.Net.Sockets;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class ProxyHost : IProxyHost
	{
		public IPAddress Source { get; set; }
		public int SourcePort { get; set; }

		public string Remote { get; set; }
		public int RemotePort { get; set; }

		public TcpListener Listener { get; protected set; }
		public List<ProxyClient> Clients { get; protected set; }

		public ProxyHost(int srcport, string remote, int remoteport)
			: this(IPAddress.Loopback, srcport, remote, remoteport)
		{
		}

		public ProxyHost(IPAddress src, int srcport, string remote, int remoteport)
		{
			Clients = new List<ProxyClient>();
			Source = src;
			SourcePort = srcport;
			Remote = remote;
			RemotePort = remoteport;
			Listener = null;
		}

		public virtual void Start()
		{
			Listener = new TcpListener(Source, SourcePort);
			Listener.Start();
			Listener.BeginAcceptTcpClient(OnAccept, null);
		}

		public virtual void Stop()
		{
			Listener.Stop();
		}

		public virtual ProxyClient NewClient(TcpClient tcp)
		{
			return new ProxyClient(this, tcp);
		}

		protected virtual void OnAccept(IAsyncResult ar)
		{
			try
			{
				var client = NewClient(Listener.EndAcceptTcpClient(ar));

				lock (Clients)
					Clients.Add(client);

				client.Start(Remote, RemotePort);

				StaticLogger.Info(string.Format("Client {0} connected", client.SourceTcp.Client.RemoteEndPoint));

				Listener.BeginAcceptTcpClient(OnAccept, null);
			}
			catch (Exception ex)
			{
				StaticLogger.Error(ex);
			}

		}

		public virtual void OnSend(ProxyClient sender, byte[] buffer, int len)
		{
			StaticLogger.Info(string.Format("Sent {0} bytes", len));
			try
			{
				using (var fs = File.Open(string.Format("{0}_{1}_Sent.txt", Remote, RemotePort), FileMode.Append, FileAccess.Write))
				{
					fs.Write(buffer, 0, len);
				}
			}
			catch (Exception ex)
			{
			}
		}

		public virtual void OnReceive(ProxyClient sender, byte[] buffer, int len)
		{
			StaticLogger.Info(string.Format("Received {0} bytes", len));
			try
			{
				using (var fs = File.Open(string.Format("{0}_{1}_Received.txt", Remote, RemotePort), FileMode.Append, FileAccess.Write))
				{
					fs.Write(buffer, 0, len);
				}
			}
			catch (Exception ex)
			{
			}
		}


		public virtual void OnException(ProxyClient sender, Exception ex)
		{
			lock (Clients)
			{
				Clients.Remove(sender);
			}
			StaticLogger.Warning(ex);
		}
	}
}
