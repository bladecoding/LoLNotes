using System;

namespace LoLNotes.Messages
{
    /// <summary>
    /// Defines a message object for the MessageTranslator
    /// </summary>
    public class MessageAttribute : Attribute
    {
        public string Name { get; set; }
        public MessageAttribute(string name)
        {
            Name = name;
        }
    }
}