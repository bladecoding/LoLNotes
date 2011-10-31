using System;
using System.Collections.Generic;
using System.Linq;
using LoLNotes.Flash;

namespace LoLNotes.Messages.Translators
{
    /// <summary>
    /// Translate a FlashObject to its respected object
    /// </summary>
    public class MessageTranslator : IObjectTranslator
    {
        public Dictionary<string, Type> Types { get; protected set; }

        /// <summary>
        /// MessageTranslator for this assembly
        /// </summary>
        public static MessageTranslator Instance { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types">Types to search through</param>
        public MessageTranslator(params Type[] types)
        {
            foreach (var type in types)
            {
                var attr = type.GetCustomAttributes(typeof(MessageAttribute), false).FirstOrDefault() as MessageAttribute;
                if (attr == null)
                    continue;

                Types.Add(attr.Name, type);
            }
        }

        static MessageTranslator()
        {
            Instance = new MessageTranslator(typeof(MessageTranslator).Assembly.GetTypes());
        }   

        /// <summary>
        /// Gets the T object from the flash object
        /// </summary>
        /// <param name="flashobj">Flash object to get data from</param>
        /// <returns>T object from flash object if successful. Null if not successful.</returns>
        public virtual object GetObject(FlashObject flashobj)
        {
            if (flashobj == null)
                return null;

            var body = flashobj["body"];
            if (body == null)
                return null;

            var type = Types.Where(kv => body.Value.Contains(kv.Key)).FirstOrDefault();

            return type.Value != null ? Activator.CreateInstance(type.Value, flashobj) : null;
        }
    }
}
