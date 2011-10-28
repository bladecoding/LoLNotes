using System.Linq;
using LoLNotes.DB;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using LoLNotes.GameStats.PlayerStats;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace LoLNotes.GameStats
{
    public class GameRecorder
    {
        readonly DocumentStore Store;
        readonly IFlashProcessor Flash;
        readonly GameStatsReader StatsReader;
        readonly GameLobbyReader LobbyReader;

        public GameRecorder(DocumentStore store, IFlashProcessor flash)
        {
            Store = store;
            Flash = flash;

            StatsReader = new GameStatsReader(flash);
            LobbyReader = new GameLobbyReader(flash);

            StatsReader.ObjectRead += StatsReader_ObjectRead;
            LobbyReader.ObjectRead += LobbyReader_ObjectRead;

            if (store.DatabaseCommands.GetIndex("PlayerStatsIndex") == null)
            {
                store.DatabaseCommands.PutIndex("PlayerStatsIndex", new IndexDefinitionBuilder<EndOfGameStats>
                {
                    Map = endstats => from stat in (from end in endstats select end).First().OtherTeamPlayerStats select stat
                });
            }
        }

        void LobbyReader_ObjectRead(GameDTO obj)
        {
            using (var sess = Store.OpenSession())
            {
                var match = sess.Query<DbWrap<GameDTO>>().FirstOrDefault(m => m.Obj.Id == obj.Id);
                if (match != null)
                {
                    //If the object read is older than don't bother adding it.
                    if (obj.TimeStamp <= match.Obj.TimeStamp)
                        return;
                    match.Obj = obj;
                }
                else
                {
                    sess.Store(new DbWrap<GameDTO>(obj));
                }
                sess.SaveChanges();
            }
        }

        void StatsReader_ObjectRead(EndOfGameStats obj)
        {
            using (var sess = Store.OpenSession())
            {
                var match = sess.Query<DbWrap<EndOfGameStats>>().FirstOrDefault(m => m.Obj.GameId == obj.GameId);
                if (match != null)
                {
                    //If the object read is older than don't bother adding it.
                    if (obj.TimeStamp > match.Obj.TimeStamp)
                        match.Obj = obj;
                }
                else
                {
                    sess.Store(new DbWrap<EndOfGameStats>(obj));
                }

                sess.SaveChanges();
            }
        }
    }
}
