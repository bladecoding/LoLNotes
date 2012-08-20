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
using FluorineFx.Messaging.Api.Event;

namespace FluorineFx.Messaging.Rtmp.Event
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public interface IRtmpEvent : IEvent
	{
        /// <summary>
        /// Gets the data type.
        /// </summary>
		byte DataType{ get; }
        /// <summary>
        /// Gets or sets the RTMP packet header.
        /// </summary>
		RtmpHeader Header{ get; set; }
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
		int Timestamp{ get; set; }
        /// <summary>
        /// Gets or sets the extended timestamp.
        /// </summary>
        int ExtendedTimestamp { get; set; }
        /// <summary>
        /// Returns a string that represents the current event object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the event members.</param>
        /// <returns>A string that represents the current event object.</returns>
        string ToString(int indentLevel);
	}
}
