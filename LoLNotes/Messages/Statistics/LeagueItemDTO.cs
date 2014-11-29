﻿/*
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

namespace LoLNotes.Messages.Summoner
{
	[Message(".LeagueItemDTO")]
	public class LeagueItemDTO : MessageObject
	{
		public LeagueItemDTO(ASObject obj)
			: base(obj)
		{
			BaseObject.SetFields(this, obj);
		}

		[InternalName("previousDayLeaguePosition")]
		public Int32 PreviousDayLeaguePosition { get; set; }

		[InternalName("timeLastDecayMessageShown")]
		public Double TimeLastDecayMessageShown { get; set; }

		[InternalName("hotStreak")]
		public Boolean HotStreak { get; set; }

		[InternalName("leagueName")]
		public String LeagueName { get; set; }

		[InternalName("miniSeries")]
		public object MiniSeries { get; set; }

		[InternalName("tier")]
		public String Tier { get; set; }

		[InternalName("freshBlood")]
		public Boolean FreshBlood { get; set; }

		[InternalName("lastPlayed")]
		public Double LastPlayed { get; set; }

		[InternalName("playerOrTeamId")]
		public String PlayerOrTeamId { get; set; }

		[InternalName("leaguePoints")]
		public Int32 LeaguePoints { get; set; }

		[InternalName("inactive")]
		public Boolean Inactive { get; set; }

		[InternalName("rank")]
		public String Rank { get; set; }

		[InternalName("veteran")]
		public Boolean Veteran { get; set; }

		[InternalName("queueType")]
		public String QueueType { get; set; }

		[InternalName("losses")]
		public Int32 Losses { get; set; }

		[InternalName("timeUntilDecay")]
		public Double TimeUntilDecay { get; set; }

		[InternalName("playerOrTeamName")]
		public String PlayerOrTeamName { get; set; }

		[InternalName("wins")]
		public Int32 Wins { get; set; }
	}
}
