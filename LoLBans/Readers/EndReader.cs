using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans.Readers
{
    public class EndReader : IObjectReader<EndOfGameStats>
    {
        protected IFlashProcessor connection;

        public event ObjectReadD<EndOfGameStats> ObjectRead;

        public EndReader(IFlashProcessor conn)
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

            if (!body.Value.Contains("EndOfGameStats"))
                return;

            ObjectRead(new EndOfGameStats(body));
        }
    }
}
