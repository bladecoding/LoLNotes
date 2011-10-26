using System;
using System.Data;
using System.Data.Linq;
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
            
        }

        void StatsReader_ObjectRead(EndOfGameStats obj)
        {
            using (var sess = Store.OpenSession())
            {
                sess.Store(obj);
            }
        }
    }
}
