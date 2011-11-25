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

using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LoLNotes.Proxy
{
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
}
