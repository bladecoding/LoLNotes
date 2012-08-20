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
using System.Text;

namespace FluorineFx.Util
{
	internal abstract class MiscellaneousUtils
	{
        protected MiscellaneousUtils() { }

		public static string GetDescription(object o)
		{
			ValidationUtils.ArgumentNotNull(o, "o");

			ICustomAttributeProvider attributeProvider = o as ICustomAttributeProvider;

			// object passed in isn't an attribute provider
			// if value is an enum value, get value field member, otherwise get values type
			if (attributeProvider == null)
			{
				Type valueType = o.GetType();

				if (valueType.IsEnum)
					attributeProvider = valueType.GetField(o.ToString());
				else
					attributeProvider = valueType;
			}

			DescriptionAttribute descriptionAttribute = ReflectionUtils.GetAttribute(typeof(DescriptionAttribute), attributeProvider) as DescriptionAttribute;

			if (descriptionAttribute != null)
				return descriptionAttribute.Description;
			else
				throw new Exception(string.Format("No DescriptionAttribute on '{0}'.", o.GetType()));
		}

		public static string[] GetDescriptions(IList values)
		{
			ValidationUtils.ArgumentNotNull(values, "values");

			string[] descriptions = new string[values.Count];

			for (int i = 0; i < values.Count; i++)
			{
				descriptions[i] = GetDescription(values[i]);
			}

			return descriptions;
		}

		public static string ToString(object value)
		{
			return (value != null) ? value.ToString() : "{null}";
		}
	}
}
