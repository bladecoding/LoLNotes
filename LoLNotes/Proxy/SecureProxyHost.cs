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
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LoLNotes.Proxy
{
	public class SecureProxyHost : ProxyHost
	{
		public X509Certificate Certificate { get; protected set; }
		public SecureProxyHost(int srcport, string remote, int remoteport, X509Certificate cert)
			: base(srcport, remote, remoteport)
		{
			Certificate = cert;
		}

		public override System.IO.Stream GetStream(TcpClient tcp)
		{
			return new SslStream(base.GetStream(tcp), false, AcceptAllCertificates) { ReadTimeout = 50000, WriteTimeout = 50000 };
		}

		public override void OnConnect(ProxyClient sender)
		{
			var source = (SslStream)sender.SourceStream;
			var remote = (SslStream)sender.RemoteStream;

			source.AuthenticateAsServer(Certificate, false, SslProtocols.Default, false);
			remote.AuthenticateAsClient(RemoteAddress);

			base.OnConnect(sender);

		}

		bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
