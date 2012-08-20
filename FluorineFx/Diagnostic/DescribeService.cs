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
using System.Reflection;
using FluorineFx.IO;

namespace FluorineFx.Diagnostic
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DescribeService
	{
		AMFBody _amfBody;

		public DescribeService(AMFBody amfBody)
		{
			_amfBody = amfBody;
		}

		public Hashtable GetDescription()
		{
			Hashtable hashtable = new Hashtable();
			if( _amfBody != null )
			{
				hashtable["version"] = "1.0";
				hashtable["address"] = _amfBody.TypeName;
				Type type = TypeHelper.Locate(_amfBody.TypeName);
				hashtable["description"] = "Service description not found.";
				ArrayList functions = new ArrayList();
				if( type != null )
				{
					object[] attributes = type.GetCustomAttributes( typeof(System.ComponentModel.DescriptionAttribute), false );
					if( attributes != null && attributes.Length > 0 )
					{
						System.ComponentModel.DescriptionAttribute descriptionAttribute = attributes[0] as System.ComponentModel.DescriptionAttribute;
						hashtable["description"] = descriptionAttribute.Description;
					}

					foreach(MethodInfo methodInfo in type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
					{
						if( TypeHelper.SkipMethod(methodInfo) )
							continue;

						string description = TypeHelper.GetDescription(methodInfo);

						Hashtable functionHashtable = new Hashtable();
						functionHashtable["name"] = methodInfo.Name;
						functionHashtable["version"] = "1.0";
						functionHashtable["description"] = description;
						if( methodInfo.ReturnType.Name == "Void") 
							functionHashtable["returns"] = "undefined";
						else
							functionHashtable["returns"] = methodInfo.ReturnType.Name;

						ArrayList parameters = new ArrayList();
						functionHashtable["arguments"] = parameters;

						if( methodInfo.GetParameters() != null && methodInfo.GetParameters().Length > 0 )
						{
							foreach(ParameterInfo parameterInfo in methodInfo.GetParameters())
							{
								Hashtable parameter = new Hashtable();
								parameter["name"] = parameterInfo.Name;
								parameter["required"] = true;
								if( parameterInfo.ParameterType.IsArray )
								{
									parameter["type"] = "Array";
								}
								else
								{
									parameter["type"] = parameterInfo.ParameterType.Name;
								}
								parameters.Add( parameter );
							}
						}

						functions.Add( functionHashtable );
					}

				}
				hashtable["functions"] = functions;
			}
			return hashtable;
		}
	}
}
