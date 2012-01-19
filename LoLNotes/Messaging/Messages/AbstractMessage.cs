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
using System.Globalization;
using System.Text;
using FluorineFx.AMF3;
using LoLNotes.Util;

namespace LoLNotes.Messaging.Messages
{
	public class AbstractMessage : IExternalizable
	{
		public const byte HAS_NEXT_FLAG = 128;
		public const byte MESSAGE_ID_FLAG = 16;
		public const byte TIME_TO_LIVE_FLAG = 64;
		public const byte TIMESTAMP_FLAG = 32;
		public const byte CLIENT_ID_BYTES_FLAG = 1;
		public const byte DESTINATION_FLAG = 4;
		public const byte CLIENT_ID_FLAG = 2;
		public const byte HEADERS_FLAG = 8;
		public const byte BODY_FLAG = 1;
		public const byte MESSAGE_ID_BYTES_FLAG = 2;

		public object Body { get; set; }
		public string ClientId { get; set; }
		public ByteArray ClientIdBytes { get; set; }
		public string Destination { get; set; }
		public object Headers { get; set; }
		public string MessageId { get; set; }
		public ByteArray MessageIdBytes { get; set; }
		public Int64 TimeStamp { get; set; }
		public Int64 TimeToLive { get; set; }

		public virtual void ReadExternal(IDataInput input)
		{
			var flags = ReadFlags(input);
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
						ClientId = input.ReadObject() as string;
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
						TimeStamp = Convert.ToInt64(input.ReadObject());
					}
					if ((flags[i] & TIME_TO_LIVE_FLAG) != 0)
					{
						TimeToLive = Convert.ToInt64(input.ReadObject());
					}
					bits = 7;
				}
				else if (i == 1)
				{
					if ((flags[i] & CLIENT_ID_BYTES_FLAG) != 0)
					{
						ClientIdBytes = input.ReadObject() as ByteArray;
						ClientId = RtmpUtil.FromByteArray(ClientIdBytes);
					}
					if ((flags[i] & MESSAGE_ID_BYTES_FLAG) != 0)
					{
						MessageIdBytes = input.ReadObject() as ByteArray;
						MessageId = RtmpUtil.FromByteArray(MessageIdBytes);
					}
					bits = 2;
				}
				ReadRemaining(input, flags[i], bits);
			}
		}

		public virtual void WriteExternal(IDataOutput output)
		{
			if (ClientIdBytes == null)
				ClientIdBytes = RtmpUtil.ToByteArray(ClientId);
			if (MessageIdBytes == null)
				MessageIdBytes = RtmpUtil.ToByteArray(MessageId);

			int firstflags = 0;
			if (Body != null)
				firstflags |= BODY_FLAG;
			if (ClientId != null && ClientIdBytes == null)
				firstflags |= CLIENT_ID_FLAG;
			if (Destination != null)
				firstflags |= DESTINATION_FLAG;
			if (Headers != null)
				firstflags |= HEADERS_FLAG;
			if (MessageId != null && MessageIdBytes == null)
				firstflags |= MESSAGE_ID_FLAG;
			if (TimeStamp != 0)
				firstflags |= TIMESTAMP_FLAG;
			if (TimeToLive != 0)
				firstflags |= TIME_TO_LIVE_FLAG;

			int secondflags = 0;
			if (ClientIdBytes != null)
				secondflags |= CLIENT_ID_BYTES_FLAG;
			if (MessageIdBytes != null)
				secondflags |= MESSAGE_ID_BYTES_FLAG;

			if (secondflags != 0)
				firstflags |= HAS_NEXT_FLAG;

			output.WriteByte((byte)firstflags);
			if (secondflags != 0)
				output.WriteByte((byte)secondflags);

			if (Body != null)
				output.WriteObject(Body);
			if (ClientId != null && ClientIdBytes == null)
				output.WriteObject(ClientId);
			if (Destination != null)
				output.WriteObject(Destination);
			if (Headers != null)
				output.WriteObject(Headers);
			if (MessageId != null && MessageIdBytes == null)
				output.WriteObject(MessageId);
			if (TimeStamp != 0)
				output.WriteObject(TimeStamp);
			if (TimeToLive != 0)
				output.WriteObject(TimeToLive);

			if (ClientIdBytes != null)
				output.WriteObject(ClientIdBytes);
			if (MessageIdBytes != null)
				output.WriteObject(MessageIdBytes);
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

		public static List<byte> ReadFlags(IDataInput input)
		{
			var ret = new List<byte>();
			byte read;

			do
			{
				ret.Add(read = input.ReadUnsignedByte());
			}
			while ((read & HAS_NEXT_FLAG) != 0); //Highest bit reserved for declaring that there is another flag.

			return ret;
		}

		public override string ToString()
		{
			return string.Format("{0}({1})", !string.IsNullOrEmpty(Destination) ? Destination + " " : "" , GetType().Name);
		}
	}
}
