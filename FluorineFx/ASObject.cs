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
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FluorineFx.Util;
using FluorineFx.Exceptions;

namespace FluorineFx
{
	/// <summary>
	/// The ASObject class represents a Flash object.
	/// </summary>
    [TypeConverter(typeof(ASObjectConverter))]
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ASObject : Dictionary<string, Object>
    {
        private string _typeName;

        /// <summary>
        /// Initializes a new instance of the ASObject class.
        /// </summary>
        public ASObject()
        {
        }
        /// <summary>
        /// Initializes a new instance of the ASObject class.
        /// </summary>
        /// <param name="typeName">Typed object type name.</param>
        public ASObject(string typeName)
        {
            _typeName = typeName;
        }
        /// <summary>
        /// Initializes a new instance of the ASObject class by copying the elements from the specified dictionary to the new ASObject object.
        /// </summary>
        /// <param name="dictionary">The IDictionary object to copy to a new ASObject object.</param>
        public ASObject(IDictionary<string, object> dictionary)
            : base(dictionary)
        {
        }
#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of an ASObject object during deserialization.
        /// </summary>
        /// <param name="info">The information needed to serialize an object.</param>
        /// <param name="context">The source or destination for the serialization stream.</param>
        public ASObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        /// <summary>
        /// Gets or sets the type name for a typed object.
        /// </summary>
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }
        /// <summary>
        /// Gets the Boolean value indicating whether the ASObject is typed.
        /// </summary>
        public bool IsTypedObject
        {
            get { return !string.IsNullOrEmpty(_typeName); }
        }

        /// <summary>
        /// Returns a string that represents the current ASObject object.
        /// </summary>
        /// <returns>A string that represents the current ASObject object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if( IsTypedObject )
                sb.Append(TypeName);
            sb.Append("{");
            int i = 0;
            foreach (KeyValuePair<string, object> entry in this)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(entry.Key);
                i++;
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Provides a type converter to convert ASObject objects to and from various other representations.
    /// </summary>
    public class ASObjectConverter : TypeConverter
    {
        /// <summary>
        /// Overloaded. Returns whether this converter can convert the object to the specified type.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
        /// <param name="destinationType">A Type that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsValueType || destinationType.IsEnum)
                return false;
            if (!ReflectionUtils.IsInstantiatableType(destinationType))
                return false;
            return true;
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
            ASObject aso = value as ASObject;
            if (!ReflectionUtils.IsInstantiatableType(destinationType))
                return null;

            object instance = TypeHelper.CreateInstance(destinationType);
            if (instance != null)
            {
                foreach (string memberName in aso.Keys)
                {
                    object val = aso[memberName];
                    //MemberInfo mi = ReflectionUtils.GetMember(destinationType, key, MemberTypes.Field | MemberTypes.Property);
                    //if (mi != null)
                    //    ReflectionUtils.SetMemberValue(mi, result, aso[key]);

                    PropertyInfo propertyInfo = null;
                    try
                    {
                        propertyInfo = destinationType.GetProperty(memberName);
                    }
                    catch (AmbiguousMatchException)
                    {
                        //To resolve the ambiguity, include BindingFlags.DeclaredOnly to restrict the search to members that are not inherited.
                        propertyInfo = destinationType.GetProperty(memberName, BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    }
                    if (propertyInfo != null)
                    {
                        try
                        {
                            val = TypeHelper.ChangeType(val, propertyInfo.PropertyType);
                            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null)
                            {
                                if (propertyInfo.GetIndexParameters() == null || propertyInfo.GetIndexParameters().Length == 0)
                                    propertyInfo.SetValue(instance, val, null);
                                else
                                {
                                    string msg = __Res.GetString(__Res.Reflection_PropertyIndexFail, string.Format("{0}.{1}", destinationType.FullName, memberName));
                                    throw new FluorineException(msg);
                                }
                            }
                            else
                            {
                                //string msg = __Res.GetString(__Res.Reflection_PropertyReadOnly, string.Format("{0}.{1}", type.FullName, memberName));
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = __Res.GetString(__Res.Reflection_PropertySetFail, string.Format("{0}.{1}", destinationType.FullName, memberName), ex.Message);
                            throw new FluorineException(msg);
                        }
                    }
                    else
                    {
                        FieldInfo fi = destinationType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                        try
                        {
                            if (fi != null)
                            {
                                val = TypeHelper.ChangeType(val, fi.FieldType);
                                fi.SetValue(instance, val);
                            }
                            else
                            {
                                //string msg = __Res.GetString(__Res.Reflection_MemberNotFound, string.Format("{0}.{1}", destinationType.FullName, memberName));
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = __Res.GetString(__Res.Reflection_FieldSetFail, string.Format("{0}.{1}", destinationType.FullName, memberName), ex.Message);
                            throw new FluorineException(msg);
                        }
                    }
                }
            }
            return instance;
        }
    }
}
