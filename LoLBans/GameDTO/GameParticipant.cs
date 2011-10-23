using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class GameParticipant : Participant
    {
        public GameParticipant(FlashObject thebase)
            : base(thebase)
        {
            FlashObject.SetFields(this, thebase);
        }
        [InternalName("isMe")]
        public bool IsMe
        {
            get;
            protected set;
        }
        [InternalName("isGameOwner")]
        public bool IsGameOwner
        {
            get;
            protected set;
        }
        [InternalName("pickTurn")]
        public int PickTurn
        {
            get;
            protected set;
        }
        [InternalName("summonerInternalName")]
        public string InternalName
        {
            get;
            protected set;
        }
        [InternalName("summonerName")]
        public string Name
        {
            get;
            protected set;
        }
    }
}