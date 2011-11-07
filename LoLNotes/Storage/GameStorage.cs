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
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using NotMissing;
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

        public delegate void PlayerUpdateHandler(PlayerEntry player);
        /// <summary>
        /// Called when the recorder gets a new/updated player.
        /// </summary>
        public event PlayerUpdateHandler PlayerUpdate;

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
                if (lobby.TimeStamp > match.TimeStamp)
                {
                    Database.Delete(match);
                    Database.Store(match = lobby);
                }
            }
            else
            {
                match = lobby;
                Database.Store(match);
            }


            foreach (PlayerParticipant plr in lobby.TeamOne.Union(match.TeamTwo).Where(p => p is PlayerParticipant).ToList())
            {
                RecordPlayer(new PlayerEntry(lobby, plr), false, true);
            }

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
        /// <param name="dontupdateexisting">If true RecordPlayer will not update an existing record. Instead it will return if a record is found.</param>
        /// <returns>Returns true if the player was recorded, otherwise false</returns>
        public bool RecordPlayer(PlayerEntry entry, bool ignoretimestamp, bool dontupdateexisting = false)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var match = Database.Query<PlayerEntry>().FirstOrDefault(m => m.Id == entry.Id);
            if (match != null)
            {
                if (dontupdateexisting)
                   return false;

                if (!ignoretimestamp)
                {
                    //If the object read is older then don't bother adding it.
                    if (entry.GameTimeStamp != 0 && entry.GameTimeStamp <= match.GameTimeStamp)
                        return false;
                    if (entry.LobbyTimeStamp != 0 && entry.LobbyTimeStamp <= match.LobbyTimeStamp)
                        return false;
                }

                Database.Delete(match);
                Database.Store(match = entry);
            }
            else
            {
                match = entry;
                Database.Store(match);
            }
            OnPlayerUpdate(match);
            return true;
        }

        public PlayerEntry GetPlayer(int id)
        {
            lock (DatabaseLock)
            {
                var ret = Database.Query<PlayerEntry>().
                    Where(e => e.Id == id).
                    FirstOrDefault();
                return ret != null ? ret.CloneT() : null;
            }
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
                    Database.Delete(match);
                    Database.Store(match = game);
                }
            }
            else
            {
                Database.Store(match = game);
            }

            var statslist = game.TeamPlayerStats.Union(game.OtherTeamPlayerStats).ToList();
            for (int i = 0; i < statslist.Count; i++)
            {
                var entry = GetPlayer(statslist[i].UserId);
                //RecordPlayer returned a PlayerEntry that we did not pass.
                //That means it returned a PlayerEntry that it found in the DB.
                //So lets update that PlayerEntry's stats.
                if (entry != null)
                {
                    //Checking that stats age is done inside UpdateStats
                    //otherwise you would be searching for gamemode/gametype twice.
                    if (entry.UpdateStats(game, statslist[i]))
                        RecordPlayer(entry, true);
                }
                else
                {
                    entry = new PlayerEntry(game, statslist[i]);
                    RecordPlayer(entry, false);
                }
            }

            return match;
        }

        protected virtual void OnPlayerUpdate(PlayerEntry player)
        {
            if (PlayerUpdate != null)
                PlayerUpdate(player);
        }
    }
}
