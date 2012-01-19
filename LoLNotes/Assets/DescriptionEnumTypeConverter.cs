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
using System.ComponentModel;

namespace LoLNotes.Assets
{
    public class DescriptionEnumTypeConverter<T> : EnumConverter
        where T : struct
    {
        private readonly Dictionary<T, string> s_toString =
            new Dictionary<T, string>();

        private readonly Dictionary<string, T> s_toValue =
            new Dictionary<string, T>();

        private bool s_isInitialized;

        static DescriptionEnumTypeConverter()
        {
            System.Diagnostics.Debug.Assert(typeof(T).IsEnum, "The custom enum class must be used with an enum type.");
        }

        public DescriptionEnumTypeConverter()
            : base(typeof(T))
        {
            if (!s_isInitialized)
            {
                Initialize();
                s_isInitialized = true;
            }
        }

        protected void Initialize()
        {
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                string description = GetDescription(item);
                s_toString[item] = description;
                s_toValue[description] = item;
            }
        }

        private static string GetDescription(T optionValue)
        {
            var optionDescription = optionValue.ToString();
            var optionInfo = typeof(T).GetField(optionDescription);
            if (Attribute.IsDefined(optionInfo, typeof(DescriptionAttribute)))
            {
                var attribute =
                    (DescriptionAttribute)Attribute.
                                              GetCustomAttribute(optionInfo, typeof(DescriptionAttribute));
                return attribute.Description;
            }
            return optionDescription;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         System.Globalization.CultureInfo culture,
                                         object value, Type destinationType)
        {
            var optionValue = (T)value;

            if (destinationType == typeof(string) &&
                s_toString.ContainsKey(optionValue))
            {
                return s_toString[optionValue];
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                                           System.Globalization.CultureInfo culture, object value)
        {
            var stringValue = value as string;

            if (!string.IsNullOrEmpty(stringValue) && s_toValue.ContainsKey(stringValue))
            {
                return s_toValue[stringValue];
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
