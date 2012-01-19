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
using System.Net.Sockets;
using NotMissing.Logging;

namespace LoLNotes.Proxy
{
	public class ProxyClient : IDisposable
	{
		const int BufferSize = 65535;

		protected bool _disposed = false;

		public TcpClient SourceTcp { get; protected set; }
		public Stream SourceStream { get; protected set; }

		public TcpClient RemoteTcp { get; protected set; }
		public Stream RemoteStream { get; protected set; }

		public IProxyHost Host { get; protected set; }

		byte[] SourceBuffer { get; set; }
		byte[] RemoteBuffer { get; set; }

		protected ProcessQueue<Tuple<byte[], int, int>> SourceQueue = new ProcessQueue<Tuple<byte[], int, int>>();
		protected ProcessQueue<Tuple<byte[], int, int>> RemoteQueue = new ProcessQueue<Tuple<byte[], int, int>>();

		public ProxyClient(IProxyHost host, TcpClient src)
		{
			Host = host;
			SourceTcp = src;
			SourceBuffer = new byte[BufferSize];
			RemoteBuffer = new byte[BufferSize];
			RemoteTcp = new TcpClient();
			SourceQueue.Process += SourceQueue_Process;
			RemoteQueue.Process += RemoteQueue_Process;
		}

		protected virtual void ConnectRemote(string remote, int remoteport)
		{
			RemoteTcp.Connect(remote, remoteport);

			SourceStream = Host.GetStream(SourceTcp);
			RemoteStream = Host.GetStream(RemoteTcp);
		}

		public virtual void Start(string remote, int remoteport)
		{
			try
			{
				ConnectRemote(remote, remoteport);

				Host.OnConnect(this);

				SourceStream.BeginRead(SourceBuffer, 0, BufferSize, BeginReceive, SourceStream);
				RemoteStream.BeginRead(RemoteBuffer, 0, BufferSize, BeginReceive, RemoteStream);
			}
			catch (Exception ex)
			{
				Host.OnException(this, ex);
			}
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
												StaticLogger.Trace(ex);
											}
										};

			runandlog(SourceTcp.Close);
			runandlog(RemoteTcp.Close);
		}

		protected virtual void BeginReceive(IAsyncResult ar)
		{
			try
			{
				var stream = (Stream)ar.AsyncState;

				int read = stream.EndRead(ar);
				if (read == 0)
					throw new EndOfStreamException(string.Format("{0} socket closed", stream == SourceStream ? "Source" : "Remote"));

				if (stream == SourceStream)
				{
					OnSend(SourceBuffer, 0, read);
				}
				else
				{
					OnReceive(RemoteBuffer, 0, read);
				}

				stream.BeginRead(
					stream == SourceStream ? SourceBuffer : RemoteBuffer,
					0,
					BufferSize,
					BeginReceive,
					stream
				);
			}
			catch (Exception ex)
			{
				Host.OnException(this, ex);
			}
		}

		protected virtual void OnSend(byte[] buffer, int idx, int len)
		{
			Host.OnSend(this, buffer, idx, len);
			RemoteQueue.Enqueue(Tuple.Create(buffer, idx, len));
		}

		protected virtual void OnReceive(byte[] buffer, int idx, int len)
		{
			Host.OnReceive(this, buffer, idx, len);
			SourceQueue.Enqueue(Tuple.Create(buffer, idx, len));
		}

		void SourceQueue_Process(object sender, ProcessQueueEventArgs<Tuple<byte[], int, int>> e)
		{
			var ar = SourceStream.BeginWrite(e.Item.Item1, e.Item.Item2, e.Item.Item3, null, null);
			using (ar.AsyncWaitHandle)
			{
				if (ar.AsyncWaitHandle.WaitOne(-1))
					SourceStream.EndWrite(ar);
			}
		}
		void RemoteQueue_Process(object sender, ProcessQueueEventArgs<Tuple<byte[], int, int>> e)
		{
			var ar = RemoteStream.BeginWrite(e.Item.Item1, e.Item.Item2, e.Item.Item3, null, null);
			using (ar.AsyncWaitHandle)
			{
				if (ar.AsyncWaitHandle.WaitOne(-1))
					RemoteStream.EndWrite(ar);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				Stop();
				SourceQueue.Dispose();
				RemoteQueue.Dispose();
			}
		}
		~ProxyClient()
		{
			Dispose(false);
		}
	}
}
