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

using System.Collections.Generic;
using System.Drawing;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.GameStats.PlayerStats;

namespace LoLNotes.Storage
{
    public class PlayerEntry
    {
        public PlayerEntry()
        {
            StatsList = new List<StatsEntry>();
        }
        public PlayerEntry(GameDTO game, PlayerParticipant plr)
            : this()
        {
            TimeStamp = game.TimeStamp;
            Name = plr.Name;
            Id = plr.Id;
        }
        public PlayerEntry(EndOfGameStats game, PlayerStatsSummary stats)
            : this()
        {
            TimeStamp = game.TimeStamp;
            Name = stats.SummonerName;
            Id = stats.UserId;
            UpdateStats(game, stats);
        }

        /// <summary>
        /// Update the players stats if they are new stats.
        /// </summary>
        /// <param name="game">the game used for gamemode/gametype/timestamp</param>
        /// <param name="stats">StatsSummary to update with</param>
        /// <returns>True if stats were updated otherwise false</returns>
        public bool UpdateStats(EndOfGameStats game, PlayerStatsSummary stats)
        {
            for (int i = 0; i < StatsList.Count; i++)
            {
                if (StatsList[i].GameType == game.GameType &&
                    StatsList[i].GameMode == game.GameMode)
                {
                    //We found the stats, if they aren't new though return.
                    if (TimeStamp > game.TimeStamp)
                        return false;
                    StatsList[i].Summary = stats;
                    TimeStamp = game.TimeStamp;
                    return true;
                }
            }
            StatsList.Add(new StatsEntry(game, stats));
            return true;
        }

        public string Note { get; set; }
        public Color NoteColor { get; set; }
        public string Name { get; set; }
        public string InternalName { get; set; }
        public int Id { get; set; }
        public long TimeStamp { get; set; }
        public List<StatsEntry> StatsList { get; set; }
    }
}
