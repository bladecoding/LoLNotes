using System;
using System.Linq;
using System.Text;

namespace LoLBans.EndOfGameStats
{
    public class PlayerParticipantStatsSummary
    {
        protected readonly FlashObject Base;

        public PlayerParticipantStatsSummary(FlashObject body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Base = body;

            FlashObject.SetFields(this, body);
        }
        [InternalName("_profileIconId")]
        public int ProfileIconId
        {
            get; protected set;
        }
        [InternalName("_summonerName")]
        public string SummonerName
        {
            get; protected set;
        }
        [InternalName("botPlayer")]
        public bool BotPlayer
        {
            get; protected set;
        }
        [InternalName("elo")]
        public int Elo
        {
            get; protected set;
        }
        [InternalName("eloChange")]
        public int EloChange
        {
            get; protected set;
        }
        [InternalName("gameId")]
        public int GameId
        {
            get; protected set;
        }
        [InternalName("gameItems")]
        public GameItems Items { get; protected set; }
    }
}
