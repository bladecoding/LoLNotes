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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NotMissing.Logging;

namespace LoLNotes
{
	public class SecureProxyHost : ProxyHost
	{
		public X509Certificate Certificate { get; protected set; }
		public SecureProxyHost(int srcport, string remote, int remoteport, X509Certificate cert)
			: base(srcport, remote, remoteport)
		{
			Certificate = cert;
		}

		public override ProxyClient NewClient(TcpClient tcp)
		{
			return new SecureProxyClient(this, tcp, Certificate);
		}
	}

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

	public interface IProxyHost
	{
		void OnSend(ProxyClient sender, byte[] buffer, int len);
		void OnReceive(ProxyClient sender, byte[] buffer, int len);
		void OnException(ProxyClient sender, Exception ex);
	}

	public class SecureProxyClient : ProxyClient
	{
		public X509Certificate Certificate { get; protected set; }

		public SecureProxyClient(IProxyHost host, TcpClient src, X509Certificate cert)
			: base(host, src)
		{
			Certificate = cert;
		}

		protected override Stream GetStream(TcpClient tcp)
		{
			return new SslStream(base.GetStream(tcp), false, AcceptAllCertificates) { ReadTimeout = 50000, WriteTimeout = 50000 };
		}

		protected override void ConnectRemote(string remoteip, int remoteport)
		{
			base.ConnectRemote(remoteip, remoteport);

			var source = (SslStream)SourceStream;
			var remote = (SslStream)RemoteStream;

			source.AuthenticateAsServer(Certificate, false, SslProtocols.Default, false);
			remote.AuthenticateAsClient(remoteip);
		}

		bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}

	public class ProxyClient
	{
		const int BufferSize = 65535;

		public TcpClient SourceTcp { get; protected set; }
		public Stream SourceStream { get; protected set; }

		public TcpClient RemoteTcp { get; protected set; }
		public Stream RemoteStream { get; protected set; }

		public IProxyHost Host { get; protected set; }

		byte[] SourceBuffer { get; set; }
		byte[] RemoteBuffer { get; set; }

		public ProxyClient(IProxyHost host, TcpClient src)
		{
			Host = host;
			SourceTcp = src;
			SourceBuffer = new byte[BufferSize];
			RemoteBuffer = new byte[BufferSize];
			RemoteTcp = new TcpClient();
		}

		protected virtual Stream GetStream(TcpClient tcp)
		{
			return tcp.GetStream();
		}

		protected virtual void ConnectRemote(string remote, int remoteport)
		{
			RemoteTcp.Connect(remote, remoteport);

			SourceStream = GetStream(SourceTcp);
			RemoteStream = GetStream(RemoteTcp);
		}

		public virtual void Start(string remote, int remoteport)
		{
			ConnectRemote(remote, remoteport);

			SourceStream.BeginRead(SourceBuffer, 0, BufferSize, OnReceive, SourceStream);
			RemoteStream.BeginRead(RemoteBuffer, 0, BufferSize, OnReceive, RemoteStream);
		}

		public virtual void Stop()
		{
			Action<Action> runandlog = delegate(Action act)
			{
				try
				{
					act();
				}
				catch (Exception ex)
				{
					StaticLogger.Warning(ex);
				}
			};

			runandlog(SourceTcp.Close);
			runandlog(RemoteTcp.Close);
		}

		protected virtual void OnReceive(IAsyncResult ar)
		{
			try
			{
				var stream = (Stream)ar.AsyncState;

				int read = stream.EndRead(ar);
				if (read == 0)
					throw new EndOfStreamException(string.Format("{0} socket closed", stream == SourceStream ? "Source" : "Remote"));

				if (stream == SourceStream)
				{
					Host.OnSend(this, SourceBuffer, read);
					RemoteStream.Write(SourceBuffer, 0, read);
				}
				else
				{
					Host.OnReceive(this, RemoteBuffer, read);
					SourceStream.Write(RemoteBuffer, 0, read);
				}

				stream.BeginRead(
						stream == SourceStream ? SourceBuffer : RemoteBuffer,
						0,
						BufferSize,
						OnReceive,
						stream
				);
			}
			catch (Exception ex)
			{
				Stop();
				Host.OnException(this, ex);
			}
		}
	}
}
