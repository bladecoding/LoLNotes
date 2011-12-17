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
using System.Globalization;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Messaging.Rtmp.Event;

namespace LoLNotes.Util
{
	public static class RtmpUtil
	{
		[ThreadStatic]
		static Random _rand;
		static Random Rand { get { return _rand ?? (_rand = new Random()); }
		}


		public static bool IsResult(Notify notify)
		{
			return notify.ServiceCall != null &&
				(notify.ServiceCall.ServiceMethodName == "_result" || notify.ServiceCall.ServiceMethodName == "_error");
		}

		public static string FromByteArray(ByteArray obj)
		{
			if (obj == null)
				return null;

			var bytes = obj.MemoryStream.ToArray();
			if (bytes.Length < 16)
				return null;

			var ret = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				if (i == 4 || i == 6 || i == 8 || i == 10)
				{
					ret.Append('-');
				}
				ret.AppendFormat("{0:X2}", bytes[i]);
			}

			return ret.ToString();
		}

		public static ByteArray ToByteArray(string str)
		{
			if (!IsUid(str))
				return null;

			str = str.Replace("-", "");

			var ret = new ByteArray();
			for (int i = 0; i < str.Length; i += 2)
			{
				byte num;
				if (!byte.TryParse(str.Substring(i, 2), NumberStyles.HexNumber, null, out num))
					return null;
				ret.WriteByte(num);
			}
			ret.Position = 0;
			return ret;
		}

		public static string RandomUidString()
		{
			return FromByteArray(RandomUid());
		}

		public static ByteArray RandomUid()
		{
			var arr = new byte[16];
			Rand.NextBytes(arr);
			return new ByteArray(arr);
		}

		public static bool IsUid(string str)
		{
			if (str != null && str.Length == 36)
			{
				int idx = 0;
				while (idx < 36)
				{

					var c = str[idx];
					if (idx == 8 || idx == 13 || idx == 18 || idx == 23)
					{
						if (c != 45)
						{
							return false;
						}
					}
					else if (c < 48 || c > 70 || c > 57 && c < 65)
					{
						return false;
					}
					idx++;
				}
				return true;
			}
			return false;
		}
	}
}
