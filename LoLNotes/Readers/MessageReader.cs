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

        public MessageReader(string messagename, IFlashProcessor conn)
        {
            Name = messagename;
            Connection = conn;
            Connection.ProcessObject += connection_ProcessObject;
        }

        void connection_ProcessObject(FlashObject obj)
        {
            if (ObjectRead == null)
                return;

            var body = obj["body"];
            if (body == null)
                return;

            if (!body.Value.Contains(Name))
                return;

            ObjectRead((T)Activator.CreateInstance(typeof(T), obj));
        }
    }
}
