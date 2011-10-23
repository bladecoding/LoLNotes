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


    [DebuggerDisplay("{Name}")]
    public class BotParticipant : GameParticipant
    {
        public BotParticipant(FlashObject thebase)
            : base(thebase)
        {
        }

        public int BotSkillLevel
        {
            get { return Parse.Int(Base["botSkillLevel"].Value); }
        }
        public string BotSkillLevelName
        {
            get { return Base["botSkillLevelName"].Value; }
        }
        public string TeamId
        {
            get { return Base["teamId"].Value; }
        }
    }
}