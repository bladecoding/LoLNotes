using System;

namespace LoLBans
{
    public class InternalNameAttribute : Attribute
    {
        public string Name { get; set; }
        public InternalNameAttribute(string name)
        {
            Name = name;
        }
    }
}