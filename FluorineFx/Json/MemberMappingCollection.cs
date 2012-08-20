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
	internal class MemberMappingCollection : CollectionBase
	{
		private Hashtable _dict;

		public MemberMappingCollection()
		{
			_dict = new Hashtable();
		}

		public int Add( MemberMapping memberMapping )  
		{
			if (Contains(memberMapping.MappingName))
			{
				// don't overwrite existing mapping with ignored mapping
				if (memberMapping.Ignored)
					return -1;

				MemberMapping existingMemberMapping = this[memberMapping.MappingName];

				if (!existingMemberMapping.Ignored)
				{
					throw new JsonSerializationException(
						string.Format(
						"A member with the name '{0}' already exists on {1}. Use the JsonPropertyAttribute to specify another name.",
						memberMapping.MappingName, memberMapping.Member.DeclaringType));
				}
				else
				{
					// remove ignored mapping so it can be replaced in collection
					Remove(existingMemberMapping);
				}
			}
			_dict[memberMapping.MappingName] = memberMapping;
			return List.Add(memberMapping);
		}

		public int IndexOf( MemberMapping value )  
		{
			return List.IndexOf(value) ;
		}

		public void Remove( MemberMapping value )  
		{
			_dict.Remove(value.MappingName);
			List.Remove(value);
		}

		public bool Contains( MemberMapping value )  
		{
			return List.Contains(value);
		}

		public bool Contains( string key )  
		{
			return _dict.Contains(key);
		}

		public MemberMapping this[ int index ]  
		{
			get  
			{
				return (MemberMapping)List[index];
			}
			set  
			{
				List[index] = value;
			}
		}

		public MemberMapping this[ string key ]  
		{
			get  
			{
				return (MemberMapping)_dict[key];
			}
			set  
			{
				_dict[key] = value;
			}
		}
	}
}