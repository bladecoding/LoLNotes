using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Db4objects.Db4o;
using LoLNotes.DB;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using LoLNotes.GameStats.PlayerStats;
using NotMissing.Logging;

namespace LoLNotes.GameStats
{
    public class GameRecorder
    {
        /// <summary>
        /// This lock is to prevents 2 objects from being added at the same time.
        /// </summary>
        readonly object DatabaseLock = new object();
        readonly IObjectContainer Database;
        readonly IFlashProcessor Flash;
        readonly GameStatsReader StatsReader;
        readonly GameLobbyReader LobbyReader;

        public GameRecorder(IObjectContainer db, IFlashProcessor flash)
        {
            Database = db;
            Flash = flash;

            StatsReader = new GameStatsReader(Flash);
            LobbyReader = new GameLobbyReader(Flash);

            StatsReader.ObjectRead += StatsReader_ObjectRead;
            LobbyReader.ObjectRead += LobbyReader_ObjectRead;
        }

        void LobbyReader_ObjectRead(GameDTO obj)
        {
            Task.Factory.StartNew(LobbyReaderTask, obj);
        }
        void StatsReader_ObjectRead(EndOfGameStats game)
        {
            Task.Factory.StartNew(StatsReaderTask, game);
        }

        void LobbyReaderTask(object obj)
        {
            var lobby = (GameDTO)obj;

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
            StaticLogger.Info(string.Format("GameDTO committed in {0}ms", sw.ElapsedMilliseconds));
        }

        void StatsReaderTask(object obj)
        {
            var game = (EndOfGameStats)obj;

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
            StaticLogger.Info(string.Format("EndOfGameStats committed in {0}ms", sw.ElapsedMilliseconds));
        }
    }
}
