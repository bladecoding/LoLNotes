using System.Linq;
using Db4objects.Db4o;
using LoLNotes.DB;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using LoLNotes.GameStats.PlayerStats;

namespace LoLNotes.GameStats
{
    public class GameRecorder
    {
        /// <summary>
        /// This lock is to prevent 2 objects from being added at the same time.
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

            StatsReader = new GameStatsReader(flash);
            LobbyReader = new GameLobbyReader(flash);

            StatsReader.ObjectRead += StatsReader_ObjectRead;
            LobbyReader.ObjectRead += LobbyReader_ObjectRead;
        }

        void LobbyReader_ObjectRead(GameDTO obj)
        {
            lock (DatabaseLock)
            {
                var match = Database.Query<DbWrap<GameDTO>>().FirstOrDefault(m => m.Obj.Id == obj.Id);
                if (match != null)
                {
                    //If the object read is older than don't bother adding it.
                    if (obj.TimeStamp <= match.Obj.TimeStamp)
                        return;
                    match.Obj = obj;
                }
                else
                {
                    Database.Store(new DbWrap<GameDTO>(obj));
                }
                Database.Commit();
            }
        }

        void StatsReader_ObjectRead(EndOfGameStats obj)
        {
            lock (DatabaseLock)
            {
                var match = Database.Query<DbWrap<EndOfGameStats>>().FirstOrDefault(m => m.Obj.GameId == obj.GameId && m.Obj.GameType == obj.GameType);
                if (match != null)
                {
                    //If the object read is older than don't bother adding it.
                    if (obj.TimeStamp > match.Obj.TimeStamp)
                        match.Obj = obj;
                }
                else
                {
                    Database.Store(new DbWrap<EndOfGameStats>(obj));
                }
                Database.Commit();
            }
        }
    }
}
