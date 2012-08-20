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
using FluorineFx.Messaging.Config;
using FluorineFx.Exceptions;

namespace FluorineFx.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class Identity : Hashtable
	{
		object _item;

		private Identity()
		{
		}

		public Identity(object item)
		{
			_item = item;
		}

		[Transient]
		public object Item
		{
			get{ return _item; }
		}

        public Identity(IDictionary map)
		{
			foreach(DictionaryEntry entry in map)
			{
				this.Add(entry.Key, entry.Value);
			}
		}

		public static Identity GetIdentity(object item, DataDestination destination)
		{
            IdentityConfiguration[] keys = destination.GetIdentityKeys();
			Identity identity = new Identity(item);
			foreach(IdentityConfiguration ic in keys)
			{
                string key = ic.Property;
				PropertyInfo pi = item.GetType().GetProperty(key);
				if( pi != null )
				{
					try
					{
						identity[key] = pi.GetValue( item, new object[0] );
					}
					catch(Exception ex)
					{
						throw new FluorineException(__Res.GetString(__Res.Identity_Failed, key), ex);
					}
				}
				else
				{				
					try
					{
						FieldInfo fi = item.GetType().GetField(key, BindingFlags.Public | BindingFlags.Instance);
						if( fi != null )
						{
							identity[key] = fi.GetValue( item );
						}
					}
					catch(Exception ex)
					{
						throw new FluorineException(__Res.GetString(__Res.Identity_Failed, key), ex);
					}
				}
			}
			return identity;
		}

		public override bool Equals(object obj)
		{
			if(obj is Identity)
			{
				Identity identity = obj as Identity;
				if( this.Count != identity.Count )
					return false;
				foreach(DictionaryEntry entry in this)
				{
					if( !identity.ContainsKey(entry.Key) )
						return false;
					object value = identity[entry.Key];
					bool equal = ( entry.Value != null ? entry.Value.Equals(value) : value == null );
					if( !equal )
						return false;
				}
				return true;
			}
			return base.Equals (obj);
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			foreach(DictionaryEntry entry in this)
			{
				if(entry.Value != null)
					hashCode ^= entry.Value.GetHashCode();
				else
					hashCode ^= 0;
			}
			return hashCode;
			//return base.GetHashCode ();
		}

	}
}
