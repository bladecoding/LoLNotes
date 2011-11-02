using LoLNotes.Flash;

namespace LoLNotes.Messages
{
    public class MessageObject : BaseObject
    {
        public MessageObject(FlashObject obj)
            : base(obj)
        {
            FlashObject.SetFields(this, obj);
        }

        [InternalName("timestamp")]
        public long TimeStamp { get; set; }
        [InternalName("destination")]
        public string Destination { get; set; }
    }
}
