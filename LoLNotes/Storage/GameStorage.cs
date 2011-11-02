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

            var config = Database.Ext().Configure();
            var types = new[] 
            { 
                typeof(PlayerEntry), 
                typeof(StatsEntry),
                typeof(DbWrap<PlayerEntry>),
                typeof(DbWrap<GameDTO>),
                typeof(DbWrap<EndOfGameStats>),
            };
            foreach (var type in types)
            {
                config.ObjectClass(type).CascadeOnUpdate(true);
                config.ObjectClass(type).CascadeOnActivate(true);
                config.ObjectClass(type).CascadeOnDelete(true);
            }

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
        public DbWrap<GameDTO> RecordLobby(GameDTO lobby)
        {
            var match = Database.Query<DbWrap<GameDTO>>().FirstOrDefault(m => m.Obj.Id == lobby.Id);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                if (lobby.TimeStamp <= match.Obj.TimeStamp)
                    return match;
                match.Obj = lobby;
            }
            else
            {
                match = new DbWrap<GameDTO>(lobby);
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
        public DbWrap<PlayerEntry> RecordPlayer(PlayerEntry entry, bool ignoretimestamp)
        {
            var match = Database.Query<DbWrap<PlayerEntry>>().FirstOrDefault(m => m.Obj.Id == entry.Id);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                if (!ignoretimestamp && entry.TimeStamp <= match.Obj.TimeStamp)
                    return match;
                match.Obj = entry;
            }
            else
            {
                match = new DbWrap<PlayerEntry>(entry);
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
        public DbWrap<EndOfGameStats> RecordGame(EndOfGameStats game)
        {

            var match = Database.Query<DbWrap<EndOfGameStats>>().FirstOrDefault(m => m.Obj.GameId == game.GameId);
            if (match != null)
            {
                //If the object read is older then don't bother adding it.
                //However it may have new player information so don't return.
                if (game.TimeStamp <= match.Obj.TimeStamp)
                    return match;
                match.Obj = game;
            }
            else
            {
                match = new DbWrap<EndOfGameStats>(game);
            }
            Database.Store(match);

            var statslist = game.TeamPlayerStats.Union(game.OtherTeamPlayerStats).ToList();
            for (int i = 0; i < statslist.Count; i++)
            {
                var search = new PlayerEntry(game, statslist[i]);
                var entry = RecordPlayer(search, false);

                //RecordPlayer returned a PlayerEntry that we did not pass.
                //That means it returned a PlayerEntry that it found in the DB.
                //So lets update that PlayerEntry's stats.
                if (entry.Obj != search)
                {
                    //Checking that stats age is done inside UpdateStats
                    //otherwise you would be searching for gamemode/gametype twice.
                    entry.Obj.UpdateStats(game, statslist[i]);
                    Database.Store(entry);
                }
            }

            return match; ;
        }
    }
}
