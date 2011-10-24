namespace LoLBans
{
    public delegate void ProcessObjectD(FlashObject obj);
    public delegate void ProcessLineD(string line);

    public interface IFlashProcessor
    {
        event ProcessObjectD ProcessObject;
        event ProcessLineD ProcessLine;
    }
}