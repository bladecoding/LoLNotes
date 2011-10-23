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


            FlashObject.SetFields(this, body);
        }

        [InternalName("maxNumPlayers")]
        public int MaxPlayers
        {
            get; protected set;
        }

        [InternalName("name")]
        public string Name
        {
            get; protected set;
        }

        [InternalName("mapId")]
        public int MapId
        {
            get; protected set;
        }

        [InternalName("id")]
        public int Id
        {
            get; protected set;
        }
        [InternalName("gameMode")]
        public string GameMode
        {
            get; protected set;
        }
        [InternalName("gameState")]
        public string GameState
        {
            get; protected set;
        }
        [InternalName("gameType")]
        public string GameType
        {
            get; protected set;
        }
        [InternalName("creationTime")]
        public string CreationTime
        {
            get; protected set;
        }
        [InternalName("teamOne")]
        public TeamParticipants TeamOne
        {
            get;
            protected set;
        }
        [InternalName("teamTwo")]
        public TeamParticipants TeamTwo
        {
            get;
            protected set;
        }
    }
}