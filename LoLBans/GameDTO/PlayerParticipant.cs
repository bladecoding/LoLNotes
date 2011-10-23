using System;
using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class PlayerParticipant : GameParticipant
    {
        public PlayerParticipant(FlashObject thebase)
            : base(thebase)
        {
        }

        public int ProfileIconId
        {
            get { return Parse.Int(Base["ProfileIconId"].Value); }
        }
        public int Id
        {
            get { return Parse.Int(Base["summonerId"].Value); }
        }
    }
}