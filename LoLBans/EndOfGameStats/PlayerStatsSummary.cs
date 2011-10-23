using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LoLBans
{
    [DebuggerDisplay("{SummonerName}")]
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

        [InternalName("botPlayer")]
        public bool BotPlayer { get; protected set; }

        [InternalName("elo")]
        public int Elo { get; protected set; }

        [InternalName("eloChange")]
        public int EloChange { get; protected set; }

        [InternalName("gameId")]
        public int GameId { get; protected set; }

        [InternalName("gameItems")]
        public GameItems Items { get; protected set; }

        [InternalName("inChat")]
        public bool InChat { get; protected set; }

        [InternalName("isMe")]
        public bool IsMe { get; protected set; }

        [InternalName("leaver")]
        public bool Leaver { get; protected set; }

        [InternalName("leaves")]
        public int Leaves { get; protected set; }

        [InternalName("level")]
        public int Level { get; protected set; }

        [InternalName("losses")]
        public int Losses { get; protected set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; protected set; }

        [InternalName("skinName")]
        public string SkinName { get; protected set; }

        [InternalName("spell1Id")]
        public int Spell1Id { get; protected set; }

        [InternalName("spell2Id")]
        public int Spell2Id { get; protected set; }

        [InternalName("statistics")]
        public PlayerStatList Statistics { get; protected set; }

        [InternalName("summonerName")]
        public string SummonerName { get; protected set; }

        [InternalName("teamId")]
        public int TeamId { get; protected set; }

        [InternalName("userId")]
        public int UserId { get; protected set; }

        [InternalName("wins")]
        public int Wins { get; protected set; }


    }
}
