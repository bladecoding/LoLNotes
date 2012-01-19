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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class ProxyHost : IProxyHost, IDisposable
	{
		public IPAddress SourceAddress { get; set; }
		public int SourcePort { get; set; }

		public string RemoteAddress { get; set; }
		public int RemotePort { get; set; }

		public TcpListener Listener { get; protected set; }
		public List<ProxyClient> Clients { get; protected set; }

		public bool IsListening
		{
			get { return Listener != null; }
		}

		public ProxyHost(int srcport, string remote, int remoteport)
			: this(IPAddress.Loopback, srcport, remote, remoteport)
		{
		}

		public ProxyHost(IPAddress src, int srcport, string remote, int remoteport)
		{
			Clients = new List<ProxyClient>();
			SourceAddress = src;
			SourcePort = srcport;
			RemoteAddress = remote;
			RemotePort = remoteport;
			Listener = null;
		}

		public virtual void Start()
		{
			if (!IsListening)
			{
				Listener = new TcpListener(SourceAddress, SourcePort);
				Listener.Start();
				Listener.BeginAcceptTcpClient(OnAccept, null);
			}
		}

		public virtual void Stop()
		{
			if (IsListening)
			{
				Listener.Stop();
				Listener = null;

				lock (Clients)
				{
					for (int i = 0; i < Clients.Count; i++)
						Clients[i].Dispose();
					Clients.Clear();
				}
			}
		}

		public virtual ProxyClient NewClient(TcpClient tcp)
		{
			return new ProxyClient(this, tcp);
		}

		protected virtual void OnAccept(IAsyncResult ar)
		{
			ProxyClient client = null;
			try
			{
				if (!IsListening)
					return;

				client = NewClient(Listener.EndAcceptTcpClient(ar));
				Listener.BeginAcceptTcpClient(OnAccept, null);

				lock (Clients)
					Clients.Add(client);

				client.Start(RemoteAddress, RemotePort);

				if (client.SourceTcp.Client != null)
					StaticLogger.Info(string.Format("Client {0} connected", client.SourceTcp.Client.RemoteEndPoint));
			}
			catch (Exception ex)
			{
				if (client != null)
				{
					OnException(client, ex);
				}
				else
				{
					//Ignore objectdisposed, happens when stopping
					if (!(ex is ObjectDisposedException))
						StaticLogger.Error(ex);
				}
			}

		}

		public virtual void OnSend(ProxyClient sender, byte[] buffer, int idx, int len)
		{
		}

		public virtual void OnReceive(ProxyClient sender, byte[] buffer, int idx, int len)
		{
		}


		public virtual void OnException(ProxyClient sender, Exception ex)
		{
			lock (Clients)
			{
				int idx = Clients.IndexOf(sender);
				if (idx != -1)
					Clients.RemoveAt(idx);
			}
			sender.Dispose();
			StaticLogger.Debug(ex);
		}

		public virtual Stream GetStream(TcpClient tcp)
		{
			return tcp.GetStream(); 
		}

		public virtual void OnConnect(ProxyClient sender)
		{
			
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
			}
		}
		~ProxyHost()
		{
			Dispose(false);
		}
	}
}
