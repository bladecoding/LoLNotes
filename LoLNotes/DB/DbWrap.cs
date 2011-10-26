namespace LoLNotes.DB
{
    //Added this as I don't know another way of updating a ravendb entry without copying everything.
    //This way you can just update property to update the entry.
    public class DbWrap<T>
    {
        public T Obj { get; set; }
        public DbWrap(T obj)
        {
            Obj = obj;
        }
    }
}
