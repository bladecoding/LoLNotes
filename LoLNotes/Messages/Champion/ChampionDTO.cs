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
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Flash;

namespace LoLNotes.Messages.Champion
{
	[Message(".ChampionDTO")]
	public class ChampionDTO : BaseObject
	{
		public ChampionDTO(ASObject obj)
			: base(obj)
		{
			BaseObject.SetFields(this, obj);
		}

		[InternalName("purchased")]
		public Int64 Purchased { get; set; }

		[InternalName("championSkins")]
		public ArrayCollection ChampionSkins { get; set; }

		[InternalName("rankedPlayEnabled")]
		public bool RankedPlayEnabled { get; set; }

		[InternalName("purchaseDate")]
		public Int64 PurchaseDate { get; set; }

		[InternalName("winCountRemaining")]
		public Int32 WinCountRemaining { get; set; }

		[InternalName("botEnabled")]
		public bool BotEnabled { get; set; }

		[InternalName("active")]
		public bool Active { get; set; }

		[InternalName("endDate")]
		public Int64 EndDate { get; set; }

		[InternalName("freeToPlay")]
		public bool FreeToPlay { get; set; }

		[InternalName("championId")]
		public Int32 ChampionId { get; set; }

		[InternalName("freeToPlayReward")]
		public bool FreeToPlayReward { get; set; }

		[InternalName("owned")]
		public bool Owned { get; set; }

	}
}
