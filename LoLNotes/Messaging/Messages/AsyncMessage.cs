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
using FluorineFx.AMF3;
using LoLNotes.Util;

namespace LoLNotes.Messaging.Messages
{
	public class AsyncMessage : AbstractMessage
	{
		const byte CORRELATION_ID_FLAG = 1;
		const byte CORRELATION_ID_BYTES_FLAG = 2;

		public string CorrelationId { get; set; }
		public ByteArray CorrelationIdBytes { get; set; }


		public override void ReadExternal(IDataInput input)
		{
			base.ReadExternal(input);
			var flags = ReadFlags(input);
			for (int i = 0; i < flags.Count; i++)
			{
				int bits = 0;
				if (i == 0)
				{
					if ((flags[i] & CORRELATION_ID_FLAG) != 0)
					{
						CorrelationId = input.ReadObject() as string;
					}
					if ((flags[i] & CORRELATION_ID_BYTES_FLAG) != 0)
					{
						CorrelationId = RtmpUtil.FromByteArray(input.ReadObject() as ByteArray);
					}
					bits = 2;
				}
				ReadRemaining(input, flags[i], bits);
			}
		}

		public override void WriteExternal(IDataOutput output)
		{
			base.WriteExternal(output);

			if (CorrelationIdBytes == null)
				CorrelationIdBytes = RtmpUtil.ToByteArray(CorrelationId);

			int flag = 0;
			if (CorrelationId != null && CorrelationIdBytes == null)
				flag |= CORRELATION_ID_FLAG;
			if (CorrelationIdBytes != null)
				flag |= CORRELATION_ID_BYTES_FLAG;

			output.WriteByte((byte)flag);

			if (CorrelationId != null && CorrelationIdBytes == null)
				output.WriteObject(CorrelationId);
			if (CorrelationIdBytes != null)
				output.WriteObject(CorrelationIdBytes);
		}
	}
}
