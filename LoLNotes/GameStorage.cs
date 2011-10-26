using System;
using System.Data;
using System.Data.Linq;
using System.Linq;
using LoLNotes.DB;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using Raven.Client.Document;

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
                    if (obj.TimeStamp <= match.Obj.TimeStamp)
                        return;
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
