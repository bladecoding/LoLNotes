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
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using LoLNotes.Extensions;

namespace LoLNotes.Messaging
{
	public class AbstractMessage : IExternalizable
	{
		public const uint MESSAGE_ID_FLAG = 16;
		public const uint TIME_TO_LIVE_FLAG = 64;
		public const uint TIMESTAMP_FLAG = 32;
		public const uint CLIENT_ID_BYTES_FLAG = 1;
		public const uint DESTINATION_FLAG = 4;
		public const uint CLIENT_ID_FLAG = 2;
		public const uint HEADERS_FLAG = 8;
		public const uint BODY_FLAG = 1;
		public const uint MESSAGE_ID_BYTES_FLAG = 2;

		public object Body { get; set; }
		public object ClientId { get; set; }
		public string Destination { get; set; }
		public object Headers { get; set; }
		public string MessageId { get; set; }
		public double TimeStamp { get; set; }
		public double TimeToLive { get; set; }

		public virtual void ReadExternal(IDataInput input)
		{
			var flags = input.ReadFlags();
			for (int i = 0; i < flags.Count; i++)
			{
				int bits = 0;
				if (i == 0)
				{
					if ((flags[i] & BODY_FLAG) != 0)
					{
						Body = input.ReadObject();
					}
					if ((flags[i] & CLIENT_ID_FLAG) != 0)
					{
						ClientId = input.ReadObject();
					}
					if ((flags[i] & DESTINATION_FLAG) != 0)
					{
						Destination = input.ReadObject() as string;
					}
					if ((flags[i] & HEADERS_FLAG) != 0)
					{
						Headers = input.ReadObject();
					}
					if ((flags[i] & MESSAGE_ID_FLAG) != 0)
					{
						MessageId = input.ReadObject() as string;
					}
					if ((flags[i] & TIMESTAMP_FLAG) != 0)
					{
						TimeStamp = (double)input.ReadObject();
					}
					if ((flags[i] & TIME_TO_LIVE_FLAG) != 0)
					{
						TimeToLive = (double)input.ReadObject();
					}
					bits = 7;
				}
				else if (i == 1)
				{
					if ((flags[i] & CLIENT_ID_BYTES_FLAG) != 0)
					{
						ClientId = FromByteArray(input.ReadObject() as ByteArray);
					}
					if ((flags[i] & MESSAGE_ID_BYTES_FLAG) != 0)
					{
						MessageId = FromByteArray(input.ReadObject() as ByteArray);
					}
					bits = 2;
				}
				ReadRemaining(input, flags[i], bits);
			}
		}

		public virtual void WriteExternal(IDataOutput output)
		{
			throw new NotImplementedException();
		}

		protected string FromByteArray(ByteArray obj)
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

		protected void ReadRemaining(IDataInput input, int flag, int bits)
		{
			if ((flag >> bits) != 0)
			{
				for (int o = bits; o < 6; o++)
				{
					if ((flag >> o & 1) != 0)
					{
						input.ReadObject();
					}
				}
			}
		}
	}
}
