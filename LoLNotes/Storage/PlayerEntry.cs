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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Collections;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.GameStats.PlayerStats;
using System.Linq;
using NotMissing;

namespace LoLNotes.Storage
{
    [DebuggerDisplay("{Name}")]
    public class PlayerEntry : ICloneable
    {
        public PlayerEntry()
        {
            StatsList = new ArrayList4<StatsEntry>();
        }
        public PlayerEntry(GameDTO game, PlayerParticipant plr)
            : this()
        {
            LobbyTimeStamp = game.TimeStamp;
            Name = plr.Name;
            Id = plr.Id;
        }
        public PlayerEntry(EndOfGameStats game, PlayerStatsSummary stats)
            : this()
        {
            GameTimeStamp = game.TimeStamp;
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
            if (StatsList == null)
                StatsList = new ArrayList4<StatsEntry>();
            for (int i = 0; i < StatsList.Count; i++)
            {
                if (StatsList[i].GameType == game.GameType &&
                    StatsList[i].GameMode == game.GameMode)
                {
                    //We found the stats, if they aren't new though return.
                    if (StatsList[i].TimeStamp > game.TimeStamp)
                        return false;
                    StatsList.Activate(ActivationPurpose.Write);
                    StatsList[i].Summary = stats;
                    StatsList[i].TimeStamp = game.TimeStamp;
                    GameTimeStamp = game.TimeStamp;
                    return true;
                }
            }
            GameTimeStamp = game.TimeStamp;
            StatsList.Activate(ActivationPurpose.Write);
            StatsList.Add(new StatsEntry(game, stats));
            return true;
        }

        public string Note { get; set; }
        public Color NoteColor { get; set; }
        public string Name { get; set; }
        public string InternalName { get; set; }
        public int Id { get; set; }
        /// <summary>
        /// TimeStamp of the last lobby update
        /// </summary>
        public long LobbyTimeStamp { get; set; }
        /// <summary>
        /// TimeStamp of the last end of game stats update
        /// </summary>
        public long GameTimeStamp { get; set; }
        public ArrayList4<StatsEntry> StatsList { get; set; }

        public object Clone()
        {
            return new PlayerEntry
            {
                Note = Note,
                NoteColor = NoteColor,
                Name = Name,
                InternalName = InternalName,
                Id = Id,
                GameTimeStamp = GameTimeStamp,
                LobbyTimeStamp = LobbyTimeStamp,
                StatsList = new ArrayList4<StatsEntry>(StatsList.Clone().ToList()),
            };
        }
    }
}
