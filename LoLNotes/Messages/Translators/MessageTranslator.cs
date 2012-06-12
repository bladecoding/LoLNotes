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
using System.Collections.Generic;
using System.Linq;
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Flash;
using NotMissing.Logging;

namespace LoLNotes.Messages.Translators
{
	/// <summary>
	/// Translate a FlashObject to its respected object
	/// </summary>
	public class MessageTranslator : IObjectTranslator
	{
		protected Dictionary<string, Type> Types { get; set; }

		/// <summary>
		/// MessageTranslator for this assembly
		/// </summary>
		public static MessageTranslator Instance { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="types">Types to search through</param>
		public MessageTranslator(params Type[] types)
		{
			Types = new Dictionary<string, Type>();
			foreach (var type in types)
			{
				var attr = type.GetCustomAttributes(typeof(MessageAttribute), false).FirstOrDefault() as MessageAttribute;
				if (attr == null)
					continue;

				Types.Add(attr.Name, type);
			}
		}

		static MessageTranslator()
		{
			Instance = new MessageTranslator(typeof(MessageTranslator).Assembly.GetTypes());
		}

		/// <summary>
		/// Gets an object from the flash object
		/// </summary>
		/// <param name="flashobj">Flash object to get data from</param>
		/// <returns>T object from flash object if successful. Null if not successful.</returns>
		public virtual object GetObject(ASObject flashobj)
		{
			if (flashobj == null)
				return null;

			var type = Types.FirstOrDefault(kv => flashobj.TypeName.EndsWith(kv.Key));
			return type.Value != null ? Activator.CreateInstance(type.Value, flashobj) : null;
		}

		public virtual object GetArray(ArrayCollection array)
		{
			if (array.Count < 1)
				return array;

			if (!(array[0] is ASObject))
				return array;

			var first = GetObject((ASObject)array[0]);
			if (first == null)
				return null;

			var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(first.GetType()));
			foreach (var item in array)
			{
				if (item is ASObject)
				{
					var obj = GetObject((ASObject)item);
					if (obj == null)
						continue;
					list.Add(obj);
				}
			}

			return list;
		}

		public virtual T GetObject<T>(ASObject flashobj) where T : class
		{
			return GetObject(flashobj) as T;
		}
	}
}
