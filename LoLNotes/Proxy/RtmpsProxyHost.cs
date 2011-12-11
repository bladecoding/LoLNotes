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
using LoLNotes.Messaging;

namespace LoLNotes.Proxy
{
	public class RtmpsProxyHost : SecureProxyHost, IMessageProcessor
	{
		/// <summary>
		/// Called when the IsConnected status changes.
		/// </summary>
		public event Action<object> Connected;

		bool isconnected;
		public bool IsConnected
		{
			get { return isconnected; }
			protected set
			{
				bool old = isconnected;
				isconnected = value;
				if (old != value && Connected != null)
					Connected(this);
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

		public event ProcessObjectD ProcessObject;
		public virtual void OnProcessObject(ASObject obj, Int64 timestamp)
		{
			if (ProcessObject != null)
				ProcessObject(obj, timestamp);
		}
	}
}
