using LoLNotes.Flash;

namespace LoLNotes.Messages.Translators
{
    public interface IObjectTranslator
    {
        object GetObject(FlashObject flashobj);
    }
}
