using System.Data.Linq;
using LoLNotes.DB;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using LoLNotes.Readers;
using LoLNotes.Util;

namespace LoLNotes.GameStats
{
    public class GameRecorder
    {
        readonly LoLNotesDataContext Context;
        readonly IFlashProcessor Flash;
        readonly GameStatsReader StatsReader;
        readonly GameLobbyReader LobbyReader;

        public GameRecorder(LoLNotesDataContext context, IFlashProcessor flash)
        {
            Context = context;
            Flash = flash;

            StatsReader = new GameStatsReader(flash);
            LobbyReader = new GameLobbyReader(flash);

            StatsReader.ObjectRead += StatsReader_ObjectRead;
            LobbyReader.ObjectRead += LobbyReader_ObjectRead;

        }

        void LobbyReader_ObjectRead(GameDTO obj)
        {
            throw new System.NotImplementedException();
        }

        void StatsReader_ObjectRead(EndOfGameStats obj)
        {
            var raw = fastJSON.JSON.Instance.ToJSON(obj, false);
            var game = new Games
            {
                GameId = obj.GameId,
                GameLength = obj.GameLength,
                RawData = Compression.CompressString(raw)
            };
            Context.Games.InsertOnSubmit(game);
            Context.SubmitChanges(ConflictMode.ContinueOnConflict);
        }
    }
}
