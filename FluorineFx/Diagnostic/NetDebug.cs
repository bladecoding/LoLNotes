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

namespace FluorineFx
{
	/// <summary>
	/// NetDebug remote trace.<br/>
	/// Controls the information that appears in the NetConnection Debugger.
	/// </summary>
	public class NetDebug
	{
		private static ArrayList _traceStack;

		/// <summary>
		/// Initializes a new instance of the NetDebug class.
		/// </summary>
		private NetDebug()
		{
		}
		/// <summary>
		/// Initializes a new instance of the NetDebug class.
		/// </summary>
		static NetDebug()
		{
			_traceStack = ArrayList.Synchronized( new ArrayList() );
		}

		/// <summary>
		/// Displays a trace message in the NetConnection Debugger.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public static void Trace(string message)
		{
#if DEBUG
			if( message != null )
				_traceStack.Add(message);
#endif
		}
		/// <summary>
		/// Displays an object in the NetConnection Debugger.
		/// </summary>
		/// <param name="obj">Object to display.</param>
		public static void Trace(object obj)
		{
#if DEBUG
			if( obj != null )
				_traceStack.Add(obj);
#endif
		}

		internal static ArrayList GetTraceStack()
		{
			return _traceStack;
		}
		/// <summary>
		/// Clear messages collected to display.
		/// </summary>
		public static void Clear()
		{
			_traceStack.Clear();
		}
	}
}
