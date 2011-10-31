using LoLNotes.Flash;
using LoLNotes.Messages.Translators;

namespace LoLNotes.Messages.Readers
{
    /// <summary>
    /// Reads objects from a IFlashProcessor
    /// </summary>
    public class MessageReader : IObjectReader
    {
        public event ObjectReadD ObjectRead;
        IFlashProcessor Flash;

        public MessageReader(IFlashProcessor flash)
        {
            Flash = flash;
            Flash.ProcessObject += Flash_ProcessObject;
        }

        void Flash_ProcessObject(FlashObject flashobj)
        {
            if (ObjectRead == null)
                return;

            var obj = MessageTranslator.Instance.GetObject(flashobj);
            if (obj != null)
                ObjectRead(obj);
        }
    }
}
