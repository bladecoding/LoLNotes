using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("Id: {GameUniqueId}")]
    public class ObfuscatedParticipant : Participant
    {
        public ObfuscatedParticipant(FlashObject thebase)
            : base(thebase)
        {
            FlashObject.SetFields(this, thebase);
        }

        [InternalName("gameUniqueId")]
        public int GameUniqueId
        {
            get;
            protected set;
        }
    }
}