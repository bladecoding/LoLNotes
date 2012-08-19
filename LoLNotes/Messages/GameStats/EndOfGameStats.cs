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
using LoLNotes.Messages.GameStats.PlayerStats;
using NotMissing;

namespace LoLNotes.Messages.GameStats
{
	[Message(".EndOfGameStats")]
    public class EndOfGameStats : MessageObject, ICloneable
    {
        public EndOfGameStats()
            : base(null)
        {
        }
        public EndOfGameStats(ASObject obj)
            : base(obj)
        {
            BaseObject.SetFields(this, obj);
        }

        [InternalName("basePoints")]
        public int BasePoints
        {
            get; set;
        }
        [InternalName("boostIpEarned")]
        public int BoostIpEarned
        {
            get; set;
        }
        [InternalName("boostXpEarned")]
        public int BoostXpEarned
        {
            get; set;
        }
        [InternalName("completionBonusPoints")]
        public int CompletionBonusPoints
        {
            get; set;
        }
        [InternalName("difficulty")]
        public string Difficulty
        {
            get; set;
        }
        [InternalName("elo")]
        public int Elo
        {
            get; set;
        }
        [InternalName("eloChange")]
        public int EloChange
        {
            get; set;
        }
        [InternalName("experienceEarned")]
        public int ExperienceEarned
        {
            get; set;
        }
        [InternalName("experienceTotal")]
        public int ExperienceTotal
        {
            get; set;
        }
        public int ExpPointsToNextLevel
        {
            get; set;
        }
        [InternalName("firstWinBonus")]
        public int FirstWinBonus
        {
            get; set;
        }
        [InternalName("gameId")]
        public Int64 GameId
        {
            get; set;
        }
        [InternalName("gameLength")]
        public int GameLength
        {
            get; set;
        }
        [InternalName("gameMode")]
        public string GameMode
        {
            get; set;
        }
        [InternalName("gameType")]
        public string GameType
        {
            get; set;
        }
        [InternalName("imbalancedTeamsNoPoints")]
        public bool ImbalancedTeamsNoPoints
        {
            get; set;
        }
        [InternalName("ipEarned")]
        public int IpEarned
        {
            get; set;
        }
        [InternalName("ipTotal")]
        public int IpTotal
        {
            get; set;
        }
        [InternalName("leveledUp")]
        public bool LeveledUp
        {
            get; set;
        }
        public int LocationBoostIpEarned
        {
            get; set;
        }
        public int LocationBoostXpEarned
        {
            get; set;
        }
        [InternalName("loyaltyBoostIpEarned")]
        public int LoyaltyBoostIpEarned
        {
            get; set;
        }
        [InternalName("loyaltyBoostXpEarned")]
        public int LoyaltyBoostXpEarned
        {
            get; set;
        }

        //TODO newSpells, not hugely important but not sure what the arraycollection contains

        [InternalName("odinBonusIp")]
        public int OdinBonusIp
        {
            get; set;
        }

        [InternalName("otherTeamPlayerParticipantStats")]
        public PlayerStatsSummaryList OtherTeamPlayerStats
        {
            get; set;
        }

        //TODO pointsPenalties, not sure whats in the arraycollection
        [InternalName("practiceMinutesLeftToday")]
        public int PracticeMinutesLeftToday
        {
            get; set;
        }
        [InternalName("practiceMsecsUntilReset")]
        public int PracticeMsecsUntilReset
        {
            get; set;
        }
        [InternalName("queueBonusEarned")]
        public int QueueBonusEarned
        {
            get; set;
        }
        [InternalName("queueType")]
        public string QueueType
        {
            get; set;
        }
        [InternalName("ranked")]
        public bool Ranked
        {
            get; set;
        }
        [InternalName("skinIndex")]
        public int SkinIndex
        {
            get; set;
        }
        [InternalName("talentPointsGained")]
        public int TalentPointsGained
        {
            get; set;
        }

        [InternalName("teamPlayerParticipantStats")]
        public PlayerStatsSummaryList TeamPlayerStats
        {
            get; set;
        }

        /// <summary>
        /// Always 0?
        /// </summary>
        [InternalName("timeUntilNextFirstWinBonus")]
        public int TimeUntilNextFirstWinBonus
        {
            get; set;
        }
        [InternalName("userId")]
        public Int64 UserId
        {
            get; set;
        }

        public object Clone()
        {
            return new EndOfGameStats
            {
                BasePoints = BasePoints,
                BoostIpEarned = BoostIpEarned,
                BoostXpEarned = BoostXpEarned,
                CompletionBonusPoints = CompletionBonusPoints,
                Difficulty = Difficulty,
                Elo = Elo,
                EloChange = EloChange,
                ExperienceEarned = ExperienceEarned,
                ExperienceTotal = ExperienceTotal,
                ExpPointsToNextLevel = ExpPointsToNextLevel,
                FirstWinBonus = FirstWinBonus,
                GameId = GameId,
                GameLength = GameLength,
                GameMode = GameMode,
                GameType = GameType,
                ImbalancedTeamsNoPoints = ImbalancedTeamsNoPoints,
                IpEarned = IpEarned,
                IpTotal = IpTotal,
                LeveledUp = LeveledUp,
                LocationBoostIpEarned = LocationBoostIpEarned,
                LocationBoostXpEarned = LocationBoostXpEarned,
                LoyaltyBoostIpEarned = LoyaltyBoostIpEarned,
                LoyaltyBoostXpEarned = LoyaltyBoostXpEarned,
                OdinBonusIp = OdinBonusIp,
                OtherTeamPlayerStats =  new PlayerStatsSummaryList(OtherTeamPlayerStats.Clone()),
                PracticeMinutesLeftToday = PracticeMinutesLeftToday,
                PracticeMsecsUntilReset = PracticeMsecsUntilReset,
                QueueBonusEarned = QueueBonusEarned,
                QueueType = QueueType,
                Ranked = Ranked,
                SkinIndex = SkinIndex,
                TalentPointsGained = TalentPointsGained,
                TeamPlayerStats = new PlayerStatsSummaryList(TeamPlayerStats.Clone()),
                TimeUntilNextFirstWinBonus = TimeUntilNextFirstWinBonus,
                UserId = UserId,
                TimeStamp = TimeStamp,
            };
        }
    }
}
