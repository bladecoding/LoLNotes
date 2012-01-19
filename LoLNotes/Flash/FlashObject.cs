/*
copyright (C) 2011-2012 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LoLNotes.Util;
using NotMissing.Logging;

namespace LoLNotes.Flash
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
        /// SetFields does not travel the hierarchy. So Derived types must make their own separate call to SetFields.
        /// </summary>
        /// <param name="obj">Object to change properties</param>
        /// <param name="flash">Flash object to get fields from</param>
        public static void SetFields<T>(T obj, FlashObject flash)
        {
            if (flash == null)
                return;

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var intern = prop.GetCustomAttributes(typeof(InternalNameAttribute), false).FirstOrDefault() as InternalNameAttribute;
                if (intern == null)
                    continue;

                object value;
                var type = prop.PropertyType;

                try
                {
                    if (type == typeof(string))
                    {
                        value = flash[intern.Name].Value;
                    }
                    else if (type == typeof(int))
                    {
                        value = Parse.Int(flash[intern.Name].Value);
                    }
                    else if (type == typeof(long))
                    {
                        value = Parse.Long(flash[intern.Name].Value);
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
                            throw new NotSupportedException(string.Format("Type {0} not supported by flash serializer", type.FullName), e);

                        }
                    }
                    prop.SetValue(obj, value, null);
                }
                catch (Exception e)
                {
                    StaticLogger.Error(string.Format("Error parsing {0}#{1}", typeof(T).FullName, prop.Name));
                    StaticLogger.Error(e);
                }
            }
        }
    }
}
