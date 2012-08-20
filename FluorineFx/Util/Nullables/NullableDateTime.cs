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

//NHibernate.NET

using System;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace FluorineFx.Util.Nullables
{
    public class NullableDateTimeConverter : TypeConverter
    {
        public NullableDateTimeConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else if (sourceType == typeof(DateTime))
                return true;
            else if (sourceType == typeof(DBNull))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            else if (destinationType == typeof(DateTime))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return NullableDateTime.Default;
            }
            if (value is DateTime)
            {
                return new NullableDateTime((DateTime)value);
            }
            if (value is DBNull)
            {
                return NullableDateTime.Default;
            }
            if (value is string)
            {
                string stringValue = ((string)value).Trim();

                if (stringValue == string.Empty)
                    return NullableDateTime.Default;

                //get underlying types converter
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(DateTime));

                DateTime newValue = (DateTime)converter.ConvertFromString(context, culture, stringValue);

                return new NullableDateTime(newValue);
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                         Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is NullableDateTime)
            {
                NullableDateTime nullable = (NullableDateTime)value;

                Type[] constructorArgTypes = new Type[1] { typeof(DateTime) };
                ConstructorInfo constructor = typeof(NullableDateTime).GetConstructor(constructorArgTypes);

                if (constructor != null)
                {
                    object[] constructorArgValues = new object[1] { nullable.Value };
                    return new InstanceDescriptor(constructor, constructorArgValues);
                }
            }
            else if (destinationType == typeof(DateTime))
            {
                NullableDateTime ndt = (NullableDateTime)value;

                if (ndt.HasValue)
                    return ndt.Value;
                else
                    return DBNull.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return new NullableDateTime((DateTime)propertyValues["Value"]);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
                                                                   Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(NullableDateTime), attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    /// <summary>
    /// An <see cref="INullableType"/> that wraps a <see cref="DateTime"/> value.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <a href="http://msdn.microsoft.com/netframework/programming/bcl/faq/DateAndTimeFAQ.aspx">DateTime FAQ</a>
    /// on MSDN. 
    /// </remarks>
    [TypeConverter(typeof(NullableDateTimeConverter)), Serializable()]
    public struct NullableDateTime : INullableType, IFormattable, IComparable
    {
        public static readonly NullableDateTime Default = new NullableDateTime();

        private DateTime _value;
        private bool hasValue;

        #region Constructors

        public NullableDateTime(DateTime value)
        {
            _value = value;
            hasValue = true;
        }

        #endregion

        #region INullable Members

        object INullableType.Value
        {
            get { return Value; }
        }

        public bool HasValue
        {
            get { return hasValue; }
        }

        #endregion

        public DateTime Value
        {
            get
            {
                if (hasValue)
                    return _value;
                else
                    throw new InvalidOperationException("Nullable type must have a value.");
            }
        }

        #region Casts

        public static explicit operator DateTime(NullableDateTime nullable)
        {
            if (!nullable.HasValue)
                throw new NullReferenceException();

            return nullable.Value;
        }

        public static implicit operator NullableDateTime(DateTime value)
        {
            return new NullableDateTime(value);
        }

        public static implicit operator NullableDateTime(DBNull value)
        {
            return Default;
        }

        #endregion

        public override string ToString()
        {
            if (HasValue)
                return Value.ToString();
            else
                return string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj is DBNull && !HasValue)
                return true;
            else if (obj is NullableDateTime)
                return Equals((NullableDateTime)obj);
            else
                return false;
            //if this is reached, it is either some other type, or DBnull is compared with this and we have a Value.
        }

        public bool Equals(NullableDateTime x)
        {
            return Equals(this, x);
        }

        public static bool Equals(NullableDateTime x, NullableDateTime y)
        {
            if (x.HasValue != y.HasValue) //one is null
                return false;
            else if (x.HasValue) //therefor y also HasValue
                return x.Value == y.Value;
            else //both are null
                return true;
        }

        public static bool operator ==(NullableDateTime x, NullableDateTime y)
        {
            return x.Equals(y);
        }

        public static bool operator ==(NullableDateTime x, object y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NullableDateTime x, NullableDateTime y)
        {
            return !x.Equals(y);
        }

        public static bool operator !=(NullableDateTime x, object y)
        {
            return !x.Equals(y);
        }

        public override int GetHashCode()
        {
            if (HasValue)
                return Value.GetHashCode();
            else
                return 0; //GetHashCode() doesn't garantee uniqueness, and neither do we.
        }

        //TODO: Operators for DateTime (?)

        #region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (HasValue)
                return Value.ToString(format, formatProvider);
            else
                return string.Empty;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is NullableDateTime) //chack and unbox
            {
                NullableDateTime value = (NullableDateTime)obj;

                if (value.HasValue == this.HasValue) //both null or not null
                {
                    if (this.HasValue) //this has a value, so they both do
                        return Value.CompareTo(value.Value);
                    else
                        return 0; //both null, so they are equal;
                }
                else //one is null
                {
                    if (HasValue) //he have a value, so we are greater.
                        return 1;
                    else
                        return -1;
                }
            }
            else if (obj is DateTime)
            {
                DateTime value = (DateTime)obj;

                if (HasValue) //not null, so compare the real values.
                    return Value.CompareTo(value);
                else
                    return -1; //this is null, so less that the real value;
            }

            throw new ArgumentException("NullableDateTime can only compare to another NullableDateTime or a System.DateTime");
        }

        #endregion

        #region Parse Members

        public static NullableDateTime Parse(string s)
        {
            if ((s == null) || (s.Trim().Length == 0))
            {
                return new NullableDateTime();
            }
            else
            {
                try
                {
                    return new NullableDateTime(DateTime.Parse(s));
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing '" + s + "' to NullableDateTime.", ex);
                }
            }
        }

        // TODO: implement the rest of the Parse overloads found in DateTime

        #endregion
    }
}
