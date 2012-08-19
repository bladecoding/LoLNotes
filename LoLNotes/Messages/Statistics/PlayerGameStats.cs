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

namespace LoLNotes.Messages.Statistics
{
	public class PlayerGameStats : BaseObject
	{
		public PlayerGameStats(ASObject obj)
			: base(obj)
		{
			BaseObject.SetFields(this, obj);
		}

		[InternalName("skinName")]
		public string SkinName { get; set; }

		[InternalName("skinIndex")]
		public Int32 SkinIndex { get; set; }

		[InternalName("fellowPlayers")]
		public FellowPlayerInfoList FellowPlayers { get; set; }

		[InternalName("gameType")]
		public string GameType { get; set; }

		[InternalName("experienceEarned")]
		public Int32 ExperienceEarned { get; set; }

		[InternalName("difficulty")]
		public object Difficulty { get; set; }

		[InternalName("gameMapId")]
		public Int32 GameMapId { get; set; }

		[InternalName("leaver")]
		public bool Leaver { get; set; }

		[InternalName("spell1")]
		public Int32 Spell1 { get; set; }

		[InternalName("gameTypeEnum")]
		public string GameTypeEnum { get; set; }

		[InternalName("teamId")]
		public Int64 TeamId { get; set; }

		[InternalName("summonerId")]
		public Int64 SummonerId { get; set; }

		[InternalName("statistics")]
		public RawStatList Statistics { get; set; }

		[InternalName("spell2")]
		public Int32 Spell2 { get; set; }

		[InternalName("afk")]
		public bool Afk { get; set; }

		[InternalName("boostXpEarned")]
		public Int32 BoostXpEarned { get; set; }

		[InternalName("adjustedRating")]
		public Int32 AdjustedRating { get; set; }

		[InternalName("premadeSize")]
		public Int32 PremadeSize { get; set; }

		[InternalName("boostIpEarned")]
		public Int32 BoostIpEarned { get; set; }

		[InternalName("gameId")]
		public Int64 GameId { get; set; }

		[InternalName("timeInQueue")]
		public Int32 TimeInQueue { get; set; }

		[InternalName("ipEarned")]
		public Int32 IpEarned { get; set; }

		[InternalName("eloChange")]
		public Int32 EloChange { get; set; }

		[InternalName("gameMode")]
		public string GameMode { get; set; }

		[InternalName("KCoefficient")]
		public Int32 KCoefficient { get; set; }

		[InternalName("teamRating")]
		public Int32 TeamRating { get; set; }

		[InternalName("subType")]
		public string SubType { get; set; }

		[InternalName("queueType")]
		public string QueueType { get; set; }

		[InternalName("premadeTeam")]
		public Int32 PremadeTeam { get; set; }

		[InternalName("predictedWinPct")]
		public Double PredictedWinPct { get; set; }

		[InternalName("rating")]
		public Int32 Rating { get; set; }

		[InternalName("championId")]
		public Int32 ChampionId { get; set; }
	}
}
