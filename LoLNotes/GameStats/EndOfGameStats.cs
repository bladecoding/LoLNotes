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
using LoLNotes.Flash;
using LoLNotes.GameStats.PlayerStats;

namespace LoLNotes.GameStats
{
    public class EndOfGameStats
    {
        protected readonly FlashObject Base;

        public EndOfGameStats(FlashObject body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Base = body;

            FlashObject.SetFields(this, Base);
        }

        [InternalName("basePoints")]
        public int BasePoints
        {
            get;
            protected set;
        }
        [InternalName("boostIpEarned")]
        public int BoostIpEarned
        {
            get;
            protected set;
        }
        [InternalName("boostXpEarned")]
        public int BoostXpEarned
        {
            get;
            protected set;
        }
        [InternalName("completionBonusPoints")]
        public int CompletionBonusPoints
        {
            get;
            protected set;
        }
        [InternalName("difficulty")]
        public string Difficulty
        {
            get;
            protected set;
        }
        [InternalName("elo")]
        public int Elo
        {
            get;
            protected set;
        }
        [InternalName("eloChange")]
        public int EloChange
        {
            get;
            protected set;
        }
        [InternalName("experienceEarned")]
        public int ExperienceEarned
        {
            get;
            protected set;
        }
        [InternalName("experienceTotal")]
        public int ExperienceTotal
        {
            get;
            protected set;
        }
        [InternalName("expPointsToNextLevel")]
        public int ExpPointsToNextLevel
        {
            get;
            protected set;
        }
        [InternalName("firstWinBonus")]
        public int FirstWinBonus
        {
            get;
            protected set;
        }
        [InternalName("gameId")]
        public int GameId
        {
            get;
            protected set;
        }
        [InternalName("gameLength")]
        public int GameLength
        {
            get;
            protected set;
        }
        [InternalName("gameMode")]
        public string GameMode
        {
            get;
            protected set;
        }
        [InternalName("gameType")]
        public string GameType
        {
            get;
            protected set;
        }
        [InternalName("imbalancedTeamsNoPoints")]
        public bool ImbalancedTeamsNoPoints
        {
            get;
            protected set;
        }
        [InternalName("ipEarned")]
        public int IpEarned
        {
            get;
            protected set;
        }
        [InternalName("ipTotal")]
        public int IpTotal
        {
            get;
            protected set;
        }
        [InternalName("leveledUp")]
        public bool LeveledUp
        {
            get;
            protected set;
        }
        [InternalName("locationBoostIpEarned")]
        public int LocationBoostIpEarned
        {
            get;
            protected set;
        }
        [InternalName("locationBoostXpEarned")]
        public int LocationBoostXpEarned
        {
            get;
            protected set;
        }
        [InternalName("loyaltyBoostIpEarned")]
        public int LoyaltyBoostIpEarned
        {
            get;
            protected set;
        }
        [InternalName("loyaltyBoostXpEarned")]
        public int LoyaltyBoostXpEarned
        {
            get;
            protected set;
        }

        //TODO newSpells, not hugely important but not sure what the arraycollection contains
        [InternalName("odinBonusIp")]
        public int OdinBonusIp
        {
            get;
            protected set;
        }

        [InternalName("otherTeamPlayerParticipantStats")]
        public PlayerStatsSummaryList OtherTeamPlayerStats
        {
            get;
            protected set;
        }

        //TODO pointsPenalties, not sure whats in the arraycollection
        [InternalName("practiceMinutesLeftToday")]
        public int PracticeMinutesLeftToday
        {
            get;
            protected set;
        }
        [InternalName("practiceMinutesPlayedToday")]
        public int PracticeMinutesPlayedToday
        {
            get;
            protected set;
        }
        [InternalName("practiceMsecsUntilReset")]
        public int PracticeMsecsUntilReset
        {
            get;
            protected set;
        }
        [InternalName("queueBonusEarned")]
        public int QueueBonusEarned
        {
            get;
            protected set;
        }
        [InternalName("queueType")]
        public string QueueType
        {
            get;
            protected set;
        }
        [InternalName("ranked")]
        public bool Ranked
        {
            get;
            protected set;
        }
        [InternalName("skinIndex")]
        public int SkinIndex
        {
            get;
            protected set;
        }
        [InternalName("skinName")]
        public string SkinName
        {
            get;
            protected set;
        }
        [InternalName("talentPointsGained")]
        public int TalentPointsGained
        {
            get;
            protected set;
        }

        [InternalName("teamPlayerParticipantStats")]
        public PlayerStatsSummaryList TeamPlayerStats
        {
            get;
            protected set;
        }

        /// <summary>
        /// Always 0?
        /// </summary>
        [InternalName("timeUntilNextFirstWinBonus")]
        public int TimeUntilNextFirstWinBonus
        {
            get;
            protected set;
        }
        [InternalName("userId")]
        public int UserId
        {
            get;
            protected set;
        }
    }
}
