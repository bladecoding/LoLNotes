/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

using FluorineFx;
using FluorineFx.AMF3;

namespace FluorineFx.Json
{
    /// <summary>
    /// Provides a type converter to convert JavaScriptArray objects to and from various other representations.
    /// </summary>
    public class JavaScriptArrayConverter : ArrayConverter
    {
        /// <summary>
        /// Overloaded. Returns whether this converter can convert the object to the specified type.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
        /// <param name="destinationType">A Type that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (destinationType.IsArray)
                return true;
            if (destinationType == typeof(ArrayList))
                return true;
            if (destinationType == typeof(IList))
                return true;
            Type typeIList = destinationType.GetInterface("System.Collections.IList");
            if (typeIList != null)
                return true;
            //generic interface
            Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1");
            if (typeGenericICollection != null)
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        /// <summary>
        /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
        /// <param name="culture">A CultureInfo object. If a null reference (Nothing in Visual Basic) is passed, the current culture is assumed.</param>
        /// <param name="value">The Object to convert.</param>
        /// <param name="destinationType">The Type to convert the value parameter to.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (destinationType.IsArray)
            {
                return (value as JavaScriptArray).ToArray();
            }
            if (destinationType == typeof(ArrayList))
            {
                return value as ArrayList;
            }
            if (destinationType == typeof(IList))
            {
                return value as IList;
            }
            //generic interface
            Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1");
            if (typeGenericICollection != null)
            {
                object obj = TypeHelper.CreateInstance(destinationType);
                MethodInfo miAddCollection = destinationType.GetMethod("Add");
                if (miAddCollection != null)
                {
                    ParameterInfo[] parameters = miAddCollection.GetParameters();
                    if (parameters != null && parameters.Length == 1)
                    {
                        Type parameterType = parameters[0].ParameterType;
                        IList list = (IList)value;
                        for (int i = 0; i < list.Count; i++)
                        {
                            miAddCollection.Invoke(obj, new object[] { TypeHelper.ChangeType(list[i], parameterType) });
                        }
                        return obj;
                    }
                }
            }
            Type typeIList = destinationType.GetInterface("System.Collections.IList");
            if (typeIList != null)
            {
                object obj = TypeHelper.CreateInstance(destinationType);
                IList list = obj as IList;
                for (int i = 0; i < (value as IList).Count; i++)
                {
                    list.Add((value as IList)[i]);
                }
                return obj;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Represents a JavaScript array.
    /// </summary>
    [TypeConverter(typeof(JavaScriptArrayConverter))]
    public class JavaScriptArray : ArrayList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptArray"/> class.
        /// </summary>
        public JavaScriptArray()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptArray"/> class that
        /// contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new array.</param>
        public JavaScriptArray(ICollection collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptArray"/> class that
        /// is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new array can initially store.</param>
        public JavaScriptArray(int capacity)
            : base(capacity)
        {
        }
    }
}