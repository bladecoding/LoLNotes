using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans.Readers
{
    public class GameDTOReader : IObjectReader<GameDTO>
    {
        protected IFlashProcessor connection;


        public event ObjectReadD<GameDTO> ObjectRead;

        public GameDTOReader(IFlashProcessor conn)
        {
            connection = conn;
            connection.ProcessObject += connection_ProcessObject;
        }

        void connection_ProcessObject(FlashObject obj)
        {
            if (ObjectRead == null)
                return;

            var body = obj["body"];
            if (body == null)
                return;

            if (!body.Value.Contains("GameDTO"))
                return;

            ObjectRead(new GameDTO(body));
        }
    }
}
