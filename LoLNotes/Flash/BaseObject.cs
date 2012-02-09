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
using System.Collections;
using System.Linq;
using System.Reflection;
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Messages;
using LoLNotes.Util;
using NotMissing;
using NotMissing.Logging;

namespace LoLNotes.Flash
{
	public class BaseObject
	{
		protected readonly ASObject Base;
		public BaseObject(ASObject obj)
		{
			Base = obj;
		}

		/// <summary>
		/// Helper method which sets all the properties in the class to their respected FlashObject field.
		/// Use InternalNameAttribute to specify a property which has a FlashObject counter-part.
		/// SetFields does not travel the hierarchy. So Derived types must make their own separate call to SetFields.
		/// </summary>
		/// <param name="obj">Object to change properties</param>
		/// <param name="flash">Flash object to get fields from</param>
		public static void SetFields<T>(T obj, ASObject flash)
		{
			if (flash == null)
				return;

			foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				var intern = prop.GetAttribute<InternalNameAttribute>();
				if (intern == null)
					continue;

				var type = prop.PropertyType;
				object value;

				if (!flash.TryGetValue(intern.Name, out value))
				{
					StaticLogger.Warning(string.Format("{0} missing ASObject property {1}", typeof(T).FullName, intern.Name));
					continue;
				}

				try
				{
					if (type == typeof(string))
					{
						value = Convert.ToString(flash[intern.Name]);
					}
					else if (type == typeof(Int32))
					{
						value = Convert.ToInt32(flash[intern.Name]);
					}
					else if (type == typeof(Int64))
					{
						value = Convert.ToInt64(flash[intern.Name]);
					}
					else if (type == typeof(double))
					{
						value = Convert.ToInt64(flash[intern.Name]);
					}
					else if (type == typeof(bool))
					{
						value = Convert.ToBoolean(flash[intern.Name]);
					}
					else if (type == typeof(DateTime))
					{
						value = Convert.ToDateTime(flash[intern.Name]);
					}
					else if (type == typeof(ASObject))
					{
						value = flash[intern.Name] as ASObject;
					}
					else if (type == typeof(ArrayCollection))
					{
						value = flash[intern.Name] as ArrayCollection;
					}
					else if (type == typeof(object))
					{
						value = flash[intern.Name];
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
					StaticLogger.Error(new Exception(string.Format("Error parsing {0}#{1}", typeof(T).FullName, prop.Name), e));
				}
			}
		}

		public static object GetObject(object obj)
		{
			if (obj == null)
				return null;

			var type = obj.GetType();
			if (type == typeof(BaseObject))
			{
				var msgattr = type.GetAttribute<MessageAttribute>();
				if (msgattr.FullName == null)
					throw new NotSupportedException(string.Format("Serialization for type {0} not supported", type.FullName));

				var ret = new ASObject(msgattr.FullName);
				foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					var intern = prop.GetAttribute<InternalNameAttribute>();
					if (intern == null)
						continue;

					ret[intern.Name] = prop.GetValue(obj, null);
				}
				return ret;
			}
			if (type == typeof(BaseList<>))
			{
				return new ArrayCollection((IList)obj);
			}
			return obj;
		}
	}
}
