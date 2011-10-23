using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans
{
    public class GameDTOReader
    {
        protected LoLConnection connection;

        public delegate void OnGameDTOD(GameDTO game);
        public event OnGameDTOD OnGameDTO;

        public GameDTOReader(LoLConnection conn)
        {
            connection = conn;
            connection.ProcessObject += connection_ProcessObject;
        }

        void connection_ProcessObject(FlashObject obj)
        {
            if (OnGameDTO == null)
                return;

            var body = obj["body"];
            if (body == null)
                return;

            if (!body.Value.Contains("GameDTO"))
                return;

            OnGameDTO(new GameDTO(body));
        }
    }
}
