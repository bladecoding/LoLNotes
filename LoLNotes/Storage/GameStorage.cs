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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using LoLNotes.Flash;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using NotMissing.Logging;

namespace LoLNotes.Storage
{
    public class GameStorage
    {
        /// <summary>
        /// This lock is to prevents 2 objects from being added at the same time.
        /// </summary>
        public readonly object DatabaseLock = new object();
        readonly IObjectContainer Database;
        readonly IFlashProcessor Flash;
        readonly MessageReader Reader;

        public GameStorage(IObjectContainer db, IFlashProcessor flash)
        {
            Database = db;
            Flash = flash;

            Reader = new MessageReader(Flash);
            Reader.ObjectRead += Reader_ObjectRead;
        }

        void Reader_ObjectRead(object obj)
        {
            if (obj is GameDTO)
                LobbyRead((GameDTO)obj);
            else if (obj is EndOfGameStats)
                StatsRead((EndOfGameStats)obj);
        }

        void LobbyRead(GameDTO lobby)
        {
            Task.Factory.StartNew(() => CommitLobby(lobby));
        }
        void StatsRead(EndOfGameStats game)
        {
            Task.Factory.StartNew(() => CommitGame(game));
        }

        /// <summary>
        /// Records/Commits the lobby to the database. Locking the database lock.
        /// </summary>
        /// <param name="lobby"></param>
        public void CommitLobby(GameDTO lobby)
        {
            Stopwatch sw;
            lock (DatabaseLock)
            {
                sw = Stopwatch.StartNew();

                RecordLobby(lobby);
                Database.Commit();
            }
            sw.Stop();
            StaticLogger.Trace(string.Format("GameDTO committed in {0}ms", sw.ElapsedMilliseconds));
        }
        /// <summary>
        /// Checks the database for the lobby and if it does not exist then it adds it.
        /// Does not lock the database or commit
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns>The GameDTO from the database</returns>
        public GameDTO RecordLobby(GameDTO lobby)
        {
            if (lobby == null)
                throw new ArgumentNullException("lobby");

            var match = Database.Query<GameDTO>().FirstOrDefault(m => m.Id == lobby.Id);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                if (lobby.TimeStamp <= match.TimeStamp)
                    return match;

                match.CreationTime = lobby.CreationTime;
                match.Destination = lobby.Destination;
                match.GameMode = lobby.GameMode;
                match.GameState = lobby.GameState;
                match.GameType = lobby.GameType;
                match.MapId = lobby.MapId;
                match.MaxPlayers = lobby.MaxPlayers;
                match.Name = lobby.Name;
                match.TimeStamp = lobby.TimeStamp;
            }
            else
            {
                match = lobby;
            }
            Database.Store(match);
            return match;
        }

        /// <summary>
        /// Records/Commits the player to the database. Locking the database lock.
        /// </summary>
        /// <param name="entry">Player to record</param>
        /// <param name="ignoretimestamp">Whether or not to ignore the timestamp when updating</param>
        /// <returns>The PlayerEntry from the database</returns>
        public void CommitPlayer(PlayerEntry entry, bool ignoretimestamp)
        {
            Stopwatch sw;
            lock (DatabaseLock)
            {
                sw = Stopwatch.StartNew();

                RecordPlayer(entry, ignoretimestamp);
                Database.Commit();
            }
            sw.Stop();
            StaticLogger.Trace(string.Format("PlayerEntry committed in {0}ms", sw.ElapsedMilliseconds));
        }

        /// <summary>
        /// Checks the database for the player and if it does not exist then it adds it.
        /// Does not lock the database or commit
        /// </summary>
        /// <param name="entry">Player to record</param>
        /// <param name="ignoretimestamp">Whether or not to ignore the timestamp when updating</param>
        /// <returns>The PlayerEntry from the database</returns>
        public PlayerEntry RecordPlayer(PlayerEntry entry, bool ignoretimestamp)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var match = Database.Query<PlayerEntry>().FirstOrDefault(m => m.Id == entry.Id);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                if (!ignoretimestamp && entry.TimeStamp <= match.TimeStamp)
                    return match;

                match.InternalName = entry.InternalName;
                match.Name = entry.Name;
                match.Note = entry.Note;
                match.NoteColor = entry.NoteColor;
                match.StatsList = entry.StatsList;
                match.TimeStamp = entry.TimeStamp;
            }
            else
            {
                match = entry;
            }

            Database.Store(match);
            return match;
        }

        public void CommitGame(EndOfGameStats game)
        {
            Stopwatch sw;
            lock (DatabaseLock)
            {
                sw = Stopwatch.StartNew();

                RecordGame(game);
                Database.Commit();
            }
            sw.Stop();
            StaticLogger.Trace(string.Format("EndOfGameStats committed in {0}ms", sw.ElapsedMilliseconds));
        }
        public EndOfGameStats RecordGame(EndOfGameStats game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            var match = Database.Query<EndOfGameStats>().FirstOrDefault(m => m.GameId == game.GameId);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                //However it may have new player information so don't return.
                if (game.TimeStamp > match.TimeStamp)
                {
                    match.BasePoints = game.BasePoints;
                    match.BoostIpEarned = game.BoostIpEarned;
                    match.BoostXpEarned = game.BoostXpEarned;
                    match.CompletionBonusPoints = game.CompletionBonusPoints;
                    match.Destination = game.Destination;
                    match.Difficulty = game.Difficulty;
                    match.Elo = game.Elo;
                    match.EloChange = game.EloChange;
                    match.ExpPointsToNextLevel = game.ExpPointsToNextLevel;
                    match.ExperienceEarned = game.ExperienceEarned;
                    match.ExperienceTotal = game.ExperienceTotal;
                    match.FirstWinBonus = game.FirstWinBonus;
                    match.GameLength = game.GameLength;
                    match.GameMode = game.GameMode;
                    match.GameType = game.GameType;
                    match.ImbalancedTeamsNoPoints = game.ImbalancedTeamsNoPoints;
                    match.IpEarned = game.IpEarned;
                    match.IpTotal = game.IpTotal;
                    match.LeveledUp = game.LeveledUp;
                    match.LocationBoostIpEarned = game.LocationBoostIpEarned;
                    match.LocationBoostXpEarned = game.LocationBoostXpEarned;
                    match.LoyaltyBoostIpEarned = game.LoyaltyBoostIpEarned;
                    match.LoyaltyBoostXpEarned = game.LoyaltyBoostXpEarned;
                    match.OdinBonusIp = game.OdinBonusIp;
                    match.OtherTeamPlayerStats = game.OtherTeamPlayerStats;
                    match.PracticeMinutesLeftToday = game.PracticeMinutesLeftToday;
                    match.PracticeMinutesPlayedToday = game.PracticeMinutesPlayedToday;
                    match.PracticeMsecsUntilReset = game.PracticeMsecsUntilReset;
                    match.QueueBonusEarned = game.QueueBonusEarned;
                    match.QueueType = game.QueueType;
                    match.Ranked = game.Ranked;
                    match.SkinIndex = game.SkinIndex;
                    match.SkinName = game.SkinName;
                    match.TalentPointsGained = game.TalentPointsGained;
                    match.TeamPlayerStats = game.TeamPlayerStats;
                    match.TimeStamp = game.TimeStamp;
                    match.TimeUntilNextFirstWinBonus = game.TimeUntilNextFirstWinBonus;
                    match.UserId = game.UserId; 

                    Database.Store(match);
                }
            }
            else
            {
                Database.Store(match = game);
            }

            var statslist = game.TeamPlayerStats.Union(game.OtherTeamPlayerStats).ToList();
            for (int i = 0; i < statslist.Count; i++)
            {
                var search = new PlayerEntry(game, statslist[i]);
                var entry = RecordPlayer(search, false);

                //RecordPlayer returned a PlayerEntry that we did not pass.
                //That means it returned a PlayerEntry that it found in the DB.
                //So lets update that PlayerEntry's stats.
                if (entry != search)
                {
                    //Checking that stats age is done inside UpdateStats
                    //otherwise you would be searching for gamemode/gametype twice.
                    entry.UpdateStats(game, statslist[i]);
                    Database.Store(entry);
                }
            }

            return match;
        }
    }
}
