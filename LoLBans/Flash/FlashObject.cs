using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class FlashObject
    {
        public FlashObject Parent { get; set; }
        public List<FlashObject> Fields { get; set; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public FlashObject(string name)
            : this(name, null)
        {
        }
        public FlashObject(string name, string value)
            : this(null, name, value)
        {
        }
        public FlashObject(string type, string name, string value)
        {
            Type = type;
            Name = name;
            Value = value;
            Fields = new List<FlashObject>();
        }

        public bool HasFields
        {
            get { return Fields.Count > 0; }
        }

        public virtual FlashObject this[string key]
        {
            get
            {
                return Fields.FirstOrDefault(fo => fo.Name == key);
            }
            set
            {
                var index = Fields.FindIndex(fo => fo.Name == key);
                if (index == -1)
                    Fields.Add(value);
                else
                    Fields[index] = value;

            }
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is FlashObject)
            {
                return ((FlashObject)obj).Name == Name;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Helper method which sets all the properties in the class to their respected FlashObject field.
        /// Use InternalNameAttribute to specify a property which has a FlashObject counter-part.
        /// </summary>
        /// <param name="obj">Object to change properties</param>
        /// <param name="flash">Flash object to get fields from</param>
        public static void SetFields(object obj, FlashObject flash)
        {
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var intern = prop.GetCustomAttributes(typeof(InternalNameAttribute), false).FirstOrDefault() as InternalNameAttribute;
                if (intern == null)
                    continue;

                object value;
                var type = prop.PropertyType;

                if (type == typeof(string))
                {
                    value = flash[intern.Name].Value;
                } 
                else if (type == typeof(int))
                {
                    value = Parse.Int(flash[intern.Name].Value);
                }
                else if (type == typeof(bool))
                {
                    value = Parse.Bool(flash[intern.Name].Value);
                }
                else
                {
                    try
                    {
                        value = Activator.CreateInstance(type, flash[intern.Name]);
                    }
                    catch (Exception e)
                    {
                        throw new NotSupportedException(string.Format("Type {0} not supported by flash serializer ({1})", type.FullName, e.Message));
                    } 
                }

                prop.SetValue(obj, value, null); 
            }
        }
    }
}