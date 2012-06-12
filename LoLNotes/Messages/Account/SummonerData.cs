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
using LoLNotes.Flash;

namespace LoLNotes.Messages.Account
{
	[Message(".Summoner")]
	public class SummonerData : BaseObject, ICloneable
	{
		public SummonerData()
			: base(null)
		{
		}
		public SummonerData(ASObject obj)
			: base(obj)
		{
			BaseObject.SetFields(this, obj);
		}

		[InternalName("name")]
		public string Username { get; set; }
		[InternalName("acctId")]
		public Int64 AccountId { get; set; }
		[InternalName("sumId")]
		public Int64 SummonerId { get; set; }

		public object Clone()
		{
			return new SummonerData
			{
				Username = Username,
				AccountId = AccountId,
				SummonerId = SummonerId
			};
		}
	}
}