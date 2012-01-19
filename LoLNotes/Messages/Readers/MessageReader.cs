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
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Flash;
using LoLNotes.Messages.Translators;
using LoLNotes.Messaging;

namespace LoLNotes.Messages.Readers
{
	/// <summary>
	/// Reads objects from a IFlashProcessor
	/// </summary>
	public class MessageReader : IObjectReader
	{
		public event ObjectReadD ObjectRead;
		IMessageProcessor Flash;

		public MessageReader(IMessageProcessor flash)
		{
			Flash = flash;
			Flash.ProcessObject += Flash_ProcessObject;
		}

		void Flash_ProcessObject(object sender, object flashobj, Int64 timestamp)
		{
			if (ObjectRead == null)
				return;

			object obj = null;	
			if (flashobj is ASObject)
			{
				obj = MessageTranslator.Instance.GetObject((ASObject)flashobj);
			}
			else if (flashobj is ArrayCollection)
			{
				obj = MessageTranslator.Instance.GetArray((ArrayCollection)flashobj);
			}
			if (obj != null)
			{
				if (obj is MessageObject)
					((MessageObject)obj).TimeStamp = timestamp;
				ObjectRead(obj);
			}

		}
	}
}
