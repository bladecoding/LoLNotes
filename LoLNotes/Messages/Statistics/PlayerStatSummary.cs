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
using LoLNotes.Flash;

namespace LoLNotes.Messages.Statistics
{
	public class PlayerStatSummary : BaseObject
	{
		public PlayerStatSummary(ASObject obj)
			: base(obj)
		{
			BaseObject.SetFields(this, obj);
		}

		[InternalName("maxRating")]
		public Int32 MaxRating { get; set; }

		[InternalName("playerStatSummaryTypeString")]
		public string PlayerStatSummaryTypeString { get; set; }

		[InternalName("aggregatedStats")]
		public ASObject AggregatedStats { get; set; }

		[InternalName("leaves")]
		public Int32 Leaves { get; set; }

		[InternalName("playerStatSummaryType")]
		public string PlayerStatSummaryType { get; set; }

		[InternalName("userId")]
		public Int32 UserId { get; set; }

		[InternalName("losses")]
		public Int32 Losses { get; set; }

		[InternalName("rating")]
		public Int32 Rating { get; set; }

		[InternalName("wins")]
		public Int32 Wins { get; set; }

	}
}
