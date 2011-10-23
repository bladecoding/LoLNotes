using System;

namespace LoLBans
{
    public class GameDTO
    {
        protected readonly FlashObject Base;

        public GameDTO(FlashObject body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Base = body;
        }

        public int MaxPlayers
        {
            get { return Parse.Int(Base["maxNumPlayers"].Value); }
        }

        public string Name
        {
            get { return Base["name"].Value; }
        }

        public int MapId
        {
            get { return Parse.Int(Base["mapId"].Value); }
        }

        public int Id
        {
            get { return Parse.Int(Base["id"].Value); }
        }

        public string GameMode
        {
            get { return Base["gameMode"].Value; }
        }

        public string GameState
        {
            get { return Base["gameState"].Value; }
        }

        public string GameType
        {
            get { return Base["gameType"].Value; }
        }

        public string CreationTime
        {
            get { return Base["creationTime"].Value; }
        }

        public Team TeamOne
        {
            get
            {
                return GetTeam("teamOne");
            }
        }
        public Team TeamTwo
        {
            get
            {
                return GetTeam("teamTwo");
            }
        }

        Team GetTeam(string name)
        {
            var ret = new Team();

            var obj = Base[name];
            if (obj == null)
                return ret;

            var array = obj["list"]["source"];

            foreach (var field in array.Fields)
            {
                if (field.Value.Contains("PlayerParticipant"))
                {
                    ret.Add(new PlayerParticipant(field));
                }
                else if (field.Value.Contains("ObfuscatedParticipant"))
                {
                    ret.Add(new ObfuscatedParticipant(field));
                }
                else if (field.Value.Contains("BotParticipant"))
                {
                    ret.Add(new BotParticipant(field));
                }
                else
                {
                    throw new NotSupportedException("Unexcepted type in team array " + field.Value);
                }
            }

            return ret;
        }

    }
}