namespace LoLNotes.Flash
{
    public class MessageObject : BaseObject
    {
        public MessageObject(FlashObject obj)
            : base(obj)
        {
            FlashObject.SetFields(this, obj);
        }

        [InternalName("timestamp")]
        public long TimeStamp { get; protected set; }
        [InternalName("destination")]
        public string Destination { get; protected set; }
    }
}
