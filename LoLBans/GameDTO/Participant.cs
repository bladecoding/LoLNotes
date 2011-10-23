namespace LoLBans
{
    public class Participant
    {
        protected readonly FlashObject Base;
        public Participant(FlashObject thebase)
        {
            Base = thebase;
            FlashObject.SetFields(this, thebase);
        }
        [InternalName("pickMode")]
        public int PickMode
        {
            get;
            protected set;
        }
    }
}