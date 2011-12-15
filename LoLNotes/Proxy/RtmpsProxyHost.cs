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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluorineFx;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;
using LoLNotes.Messaging;
using LoLNotes.Messaging.Messages;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyHost : SecureProxyHost, IMessageProcessor
	{
		/// <summary>
		/// Called when the IsConnected status changes.
		/// </summary>
		public event EventHandler Connected;

		public event CallHandler Call;
		public event NotifyHandler Notify;
		public event ProcessObjectHandler ProcessObject;

		bool isconnected;
		public bool IsConnected
		{
			get { return isconnected; }
			protected set
			{
				bool old = isconnected;
				isconnected = value;
				if (old != value && Connected != null)
					Connected(this, new EventArgs());
			}
		}

		public RtmpsProxyHost(int srcport, string remote, int remoteport, X509Certificate cert)
			: base(srcport, remote, remoteport, cert)
		{
		}

		public override ProxyClient NewClient(TcpClient tcp)
		{
			return new RtmpsProxyClient(this, tcp);
		}

		public override void OnConnect(ProxyClient sender)
		{
			IsConnected = true;
			base.OnConnect(sender);
		}

		public override void OnException(ProxyClient sender, Exception ex)
		{
			IsConnected = false;
			base.OnException(sender, ex);
		}



		public virtual void OnProcessObject(object sender, ASObject obj, Int64 timestamp)
		{
			if (ProcessObject != null)
				ProcessObject(sender, obj, timestamp);
		}

		public virtual void OnCall(object sender, Notify call, Notify result)
		{
			if (Call != null)
				Call(sender, call, result);

			OnProcessResults(sender, result);
		}
		public virtual void OnNotify(object sender, Notify notify)
		{
			if (Notify != null)
				Notify(sender, notify);

			OnProcessResults(this, notify);
		}
		public virtual void OnProcessResults(object sender, Notify results)
		{
			foreach (var arg in results.ServiceCall.Arguments)
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
					OnProcessObject(this, obj, timestamp);
			}
		}


	}
}
