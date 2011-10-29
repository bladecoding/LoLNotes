using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLNotes.GameStats.PlayerStats
{
    public class PlayerEntry
    {
        public PlayerEntry()
        {
        }
        public PlayerEntry(EndOfGameStats game, PlayerStatsSummary stats)
        {
            Stats = stats;
            GameType = game.GameType;
            TimeStamp = game.TimeStamp;
        }

        public int Id { get { return Stats != null ? Stats.UserId : 0; } }
        public string GameType { get; set; }
        public long TimeStamp { get; set; }
        public PlayerStatsSummary Stats { get; set; }
    }
}
