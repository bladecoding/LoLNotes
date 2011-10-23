using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{GameUniqueId}")]
    public class ObfuscatedParticipant : Participant
    {
        public ObfuscatedParticipant(FlashObject thebase)
            : base(thebase)
        {
        }

        public int GameUniqueId
        {
            get { return Parse.Int(Base["gameUniqueId"].Value); }
        }
    }
}