namespace LoLBans
{
    public class Participant
    {
        protected readonly FlashObject Base;
        public Participant(FlashObject thebase)
        {
            Base = thebase;
        }

        public int PickMode
        {
            get { return Parse.Int(Base["pickMode"].Value); }
        }
    }
}