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

namespace FluorineFx.Json
{
	public class JsonConverterCollection : CollectionBase
	{
		public JsonConverterCollection()
		{
		}
 
		public JsonConverterCollection(JsonConverterCollection collection)
		{
			this.InnerList.AddRange(collection);
		}
 
		public JsonConverter this[int index]
		{
			get { return (JsonConverter)List[index]; }
			set { List[index] = value; }
		}
 
		public virtual void Add(JsonConverter converter)
		{
			List.Add(converter);
		}
 
		public virtual void Remove(JsonConverter converter)
		{
			List.Remove(converter);
		}
 
		public bool Contains(JsonConverter converter)
		{
			return List.Contains(converter);
		}
 
		public int IndexOf(JsonConverter converter)
		{
			return List.IndexOf(converter);
		}
 
		protected override void OnValidate(object value)
		{
			base.OnValidate(value);
			if (!(value is JsonConverter))
			{
				throw new ArgumentException("JsonConverterCollection only supports JsonConverter objects.");
			}
		}
	}
}
