using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class GameParticipant : Participant
    {
        public GameParticipant(FlashObject thebase)
            : base(thebase)
        {
        }

        public bool IsMe
        {
            get { return Parse.Bool(Base["isMe"].Value); }
        }
        public bool IsGameOwner
        {
            get { return Parse.Bool(Base["isGameOwner"].Value); }
        }
        public int PickTurn
        {
            get { return Parse.Int(Base["pickTurn"].Value); }
        }
        public string InternalName
        {
            get { return Base["summonerInternalName"].Value; }
        }
        public string Name
        {
            get { return Base["summonerName"].Value; }
        }
    }
}