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
using FluorineFx.Util;
#if !(NET_1_1)
using System.Collections.Generic;
#endif

namespace FluorineFx.Json
{
    /// <summary>
    /// Provides a type converter to convert JavaScriptArray objects to and from various other representations.
    /// </summary>
    public class JavaScriptObjectConverter : TypeConverter
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
                return false;
            if (destinationType.IsEnum)
                return false;
            if (destinationType.IsPrimitive)
                return false;
            if (destinationType.IsValueType)
                return false;
			return true;
            //return base.CanConvertTo(context, destinationType);
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
			JavaScriptObject javaScriptObject = value as JavaScriptObject;

            if (destinationType == typeof(object) || destinationType == typeof(JavaScriptObject))
                return value;
			if (destinationType == typeof(Hashtable))
				return new Hashtable(javaScriptObject);
			if (destinationType == typeof(ASObject))
				return new ASObject(javaScriptObject);

			if (ReflectionUtils.ImplementsInterface(destinationType, "System.Collections.Generic.IDictionary`2"))
				return TypeHelper.ChangeType(javaScriptObject, destinationType);
			if (ReflectionUtils.ImplementsInterface(destinationType, "System.Collections.IDictionary"))
				return TypeHelper.ChangeType(javaScriptObject, destinationType);

			object newObject = Activator.CreateInstance(destinationType);
#if !(NET_1_1)
            foreach (KeyValuePair<string, object> entry in value as JavaScriptObject)
#else
			foreach(DictionaryEntry entry in value as JavaScriptObject)
#endif
			{
				string memberName = entry.Key as string;
				MemberInfo memberInfo = ReflectionUtils.GetMember(destinationType, memberName, MemberTypes.Field | MemberTypes.Property);
                if (memberInfo != null)
                {
                    object memberValue = TypeHelper.ChangeType(entry.Value, ReflectionUtils.GetMemberUnderlyingType(memberInfo));
                    ReflectionUtils.SetMemberValue(memberInfo, newObject, memberValue);
                }
			}

			return newObject;
        }
    }

    /// <summary>
    /// Represents a JavaScript object.
    /// </summary>
    [TypeConverter(typeof(JavaScriptObjectConverter))]
#if !(NET_1_1)
    public class JavaScriptObject : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptObject"/> class.
        /// </summary>
        public JavaScriptObject()
            : base(EqualityComparer<string>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptObject"/> class that
        /// contains values copied from the specified <see cref="JavaScriptObject"/>.
        /// </summary>
        /// <param name="javaScriptObject">The <see cref="JavaScriptObject"/> whose elements are copied to the new object.</param>
        public JavaScriptObject(JavaScriptObject javaScriptObject)
            : base(javaScriptObject, EqualityComparer<string>.Default)
        {
        }
    }
#else
    public class JavaScriptObject : Hashtable //Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptObject"/> class.
        /// </summary>
        public JavaScriptObject() //: base(EqualityComparer<string>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptObject"/> class that
        /// contains values copied from the specified <see cref="JavaScriptObject"/>.
        /// </summary>
        /// <param name="javaScriptObject">The <see cref="JavaScriptObject"/> whose elements are copied to the new object.</param>
        public JavaScriptObject(JavaScriptObject javaScriptObject)
            : base(javaScriptObject)
        //: base(javaScriptObject, EqualityComparer<string>.Default)
        {
        }
    }
#endif
}
