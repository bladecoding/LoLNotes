using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class BotParticipant : GameParticipant
    {
        public BotParticipant(FlashObject thebase)
            : base(thebase)
        {
            FlashObject.SetFields(this, thebase);
        }

        [InternalName("botSkillLevel")]
        public int BotSkillLevel
        {
            get;
            protected set;
        }
        [InternalName("botSkillLevelName")]
        public string BotSkillLevelName
        {
            get;
            protected set;
        }
        [InternalName("teamId")]
        public string TeamId
        {
            get;
            protected set;
        }
    }
}