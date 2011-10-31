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
    public class GameRecorder
    {
        /// <summary>
        /// This lock is to prevents 2 objects from being added at the same time.
        /// </summary>
        readonly object DatabaseLock = new object();
        readonly IObjectContainer Database;
        readonly IFlashProcessor Flash;
        readonly MessageReader Reader;

        public GameRecorder(IObjectContainer db, IFlashProcessor flash)
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
            Task.Factory.StartNew(() => RecordLobby(lobby));
        }
        void StatsRead(EndOfGameStats game)
        {
            Task.Factory.StartNew(() => RecordGame(game));
        }

        public void RecordLobby(GameDTO lobby)
        {
            var sw = Stopwatch.StartNew();
            lock (DatabaseLock)
            {
                var match = Database.Query<DbWrap<GameDTO>>().FirstOrDefault(m => m.Obj.Id == lobby.Id);
                if (match != null)
                {
                    //If the object read is older then don't bother adding it.
                    if (lobby.TimeStamp <= match.Obj.TimeStamp)
                        return;
                    match.Obj = lobby;
                }
                else
                {
                    Database.Store(new DbWrap<GameDTO>(lobby));
                }
                Database.Commit();
            }
            sw.Stop();
            StaticLogger.Trace(string.Format("GameDTO committed in {0}ms", sw.ElapsedMilliseconds));
        }

        public void RecordGame(EndOfGameStats game)
        {
            var sw = Stopwatch.StartNew();
            lock (DatabaseLock)
            {
                var match = Database.Query<DbWrap<EndOfGameStats>>().FirstOrDefault(m => m.Obj.GameId == game.GameId);
                if (match != null)
                {
                    //If the object read is older then don't bother adding it.
                    //However it may have new player information so don't return.
                    if (game.TimeStamp > match.Obj.TimeStamp)
                        match.Obj = game;
                }
                else
                {
                    match = new DbWrap<EndOfGameStats>(game);
                    Database.Store(match);
                }

                var statslist = game.TeamPlayerStats.Union(game.OtherTeamPlayerStats).ToList();
                for (int i = 0; i < statslist.Count; i++)
                {
                    var entry = Database.Query<DbWrap<PlayerEntry>>().
                        FirstOrDefault(p => p.Obj.Id == statslist[i].UserId);

                    if (entry != null)
                    {
                        //Checking that stats age is done inside UpdateStats
                        //otherwise you would be searching for gamemode/gametype twice.
                        entry.Obj.UpdateStats(game, statslist[i]);
                    }
                    else
                    {
                        entry = new DbWrap<PlayerEntry>(new PlayerEntry(game, statslist[i]));
                        Database.Store(entry);
                    }
                    
                }

                Database.Commit();
            }
            sw.Stop();
            StaticLogger.Trace(string.Format("EndOfGameStats committed in {0}ms", sw.ElapsedMilliseconds));
        }
    }
}
