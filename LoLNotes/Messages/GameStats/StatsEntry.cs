using LoLNotes.Messages.GameStats.PlayerStats;

namespace LoLNotes.Messages.GameStats
{
    public class StatsEntry
    {
        public StatsEntry()
        {
        }
        public StatsEntry(EndOfGameStats game, PlayerStatsSummary stats)
        {
            GameMode = game.GameMode;
            GameType = game.GameType;
            Summary = stats;
        }
        public string GameMode { get; set; }
        public string GameType { get; set; }
        public PlayerStatsSummary Summary { get; set; }
    }
}