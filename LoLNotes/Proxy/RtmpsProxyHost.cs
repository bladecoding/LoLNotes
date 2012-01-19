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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluorineFx;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Service;
using LoLNotes.Messaging;
using LoLNotes.Messaging.Messages;
using LoLNotes.Util;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyHost : SecureProxyHost, IMessageProcessor
	{
		/// <summary>
		/// Called when the IsConnected status changes.
		/// </summary>
		public event EventHandler Connected;

		public event CallHandler CallResult;
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

		public void ChangeRemote(string domain, X509Certificate cert)
		{
			RemoteAddress = domain;
			Certificate = cert;
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

		/// <summary>
		/// Invokes the first client's Call method with a FlexInvoke which will block.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns>Result or null on failed</returns>
		public Notify Call(object msg)
		{
			var inv = new FlexInvoke();
			inv.EventType = (EventType)2;
			inv.ServiceCall = new PendingCall(null, new[] { msg });
			return CallWithInvoke(inv);
		}
		/// <summary>
		/// Invokes the first client's Call method which will block.
		/// </summary>
		/// <param name="notify"></param>
		/// <returns>Result or null on failed</returns>
		public Notify CallWithInvoke(Notify notify)
		{
			RtmpsProxyClient client = null;
			lock (Clients)
			{
				if (Clients.Count < 1)
					return null;
				client = (RtmpsProxyClient)Clients[0];
			}

			return client.Call(notify);
		}

		public virtual void OnProcessObject(object sender, object obj, Int64 timestamp)
		{
			if (ProcessObject != null)
				ProcessObject(sender, obj, timestamp);
		}

		public virtual void OnCall(object sender, Notify call, Notify result)
		{
			if (CallResult != null)
				CallResult(sender, call, result);

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
			var bodies = RtmpUtil.GetBodies(results);
			foreach (var obj in bodies)
			{
				OnProcessObject(this, obj.Item1, obj.Item2);
			}
		}


	}
}
