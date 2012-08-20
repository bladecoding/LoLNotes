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

namespace FluorineFx.Json
{
	/// <summary>
	/// Specifies the type of Json token.
	/// </summary>
	public enum JsonToken
	{
		/// <summary>
		/// This is returned by the <see cref="JsonReader"/> if a <see cref="JsonReader.Read"/> method has not been called. 
		/// </summary>
		None,
		/// <summary>
		/// An object start token.
		/// </summary>
		StartObject,
		/// <summary>
		/// An array start token.
		/// </summary>
		StartArray,
		/// <summary>
		/// An object property name.
		/// </summary>
		PropertyName,
		/// <summary>
		/// A comment.
		/// </summary>
		Comment,
		/// <summary>
		/// An interger.
		/// </summary>
		Integer,
		/// <summary>
		/// A float.
		/// </summary>
		Float,
		/// <summary>
		/// A string.
		/// </summary>
		String,
		/// <summary>
		/// A boolean.
		/// </summary>
		Boolean,
		/// <summary>
		/// A null token.
		/// </summary>
		Null,
		/// <summary>
		/// An undefined token.
		/// </summary>
		Undefined,
		/// <summary>
		/// An object end token.
		/// </summary>
		EndObject,
		/// <summary>
		/// An array end token.
		/// </summary>
		EndArray,
		/// <summary>
		/// A JavaScript object constructor.
		/// </summary>
		Constructor,
		/// <summary>
		/// A Date.
		/// </summary>
		Date
	}
}
