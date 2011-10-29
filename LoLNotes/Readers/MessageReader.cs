using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoLNotes.Flash;
using LoLNotes.GameLobby;

namespace LoLNotes.Readers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Type must have a constructor with (FlashObject)</typeparam>
    public class MessageReader<T> : IObjectReader<T> where T : MessageObject
    {
        public string Name { get; protected set; }
        public IFlashProcessor Connection { get; protected set; }

        public event ObjectReadD<T> ObjectRead;

        public MessageReader(string messagename)
            : this(messagename, null)
        {
        }

        public MessageReader(string messagename, IFlashProcessor conn)
        {
            Name = messagename;
            Connection = conn;

            if (conn != null)
                Connection.ProcessObject += ProcessObject;
        }

        /// <summary>
        /// Gets the T object from the flash object
        /// </summary>
        /// <param name="flashobj">Flash object to get data from</param>
        /// <returns>T object from flash object if successful. Null if not successful.</returns>
        public virtual T GetObject(FlashObject flashobj)
        {
            if (flashobj == null)
                return default(T);

            var body = flashobj["body"];
            if (body == null)
                return default(T);

            if (!body.Value.Contains(Name))
                return default(T);

            return (T)Activator.CreateInstance(typeof(T), flashobj);
        }

        protected virtual void ProcessObject(FlashObject flashobj)
        {
            if (ObjectRead == null)
                return;

            var obj = GetObject(flashobj);
            if (obj != null)
                ObjectRead(obj);
        }
    }
}
