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

namespace FluorineFx.AMF3
{
	/// <summary>
	/// The IExternalizable interface provides control over serialization of a class as it is encoded into a data stream.
	/// </summary>
    [CLSCompliant(false)]
    public interface IExternalizable
	{
		/// <summary>
		/// A class implements this method to decode itself from a data stream by calling the methods of the IDataInput interface. 
		/// </summary>
		/// <param name="input">Input data stream.</param>
		void ReadExternal(IDataInput input);
		/// <summary>
		/// A class implements this method to encode itself for a data stream by calling the methods of the IDataOutput interface.
		/// </summary>
		/// <param name="output">Output data stream.</param>
		void WriteExternal(IDataOutput output);
	}
}
