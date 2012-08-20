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

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public enum HeaderType : byte
	{
        /// <summary>
        /// New header marker.
        /// </summary>
		HeaderNew = 0,
        /// <summary>
        /// Same source marker.
        /// </summary>
		HeaderSameSource = 1,
        /// <summary>
        /// Timer change marker.
        /// </summary>
		HeaderTimerChange = 2,
        /// <summary>
        /// Continuation marker.
        /// </summary>
		HeaderContinue = 3
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public enum RtmpState
	{
		Connect = 0,
		Handshake = 1,
		Connected = 2,
		Error = 3,
		Disconnected = 4
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public enum RtmpMode
	{
		Server = 0,
		Client = 1
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	enum ScopeLevel
	{
		Global = 0,
		Application = 1,
		Room = 2
	}
}
