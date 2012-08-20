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

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class MessageOutput : AMFMessage
	{
		/// <summary>
		/// Initializes a new instance of the MessageOutput class.
		/// </summary>
		public MessageOutput():this(0)
		{
		}
		/// <summary>
		/// Initializes a new instance of the MessageOutput class.
		/// </summary>
		/// <param name="version"></param>
		public MessageOutput(ushort version):base(version)
		{
		}

        [CLSCompliant(false)]
        public bool ContainsResponse(AMFBody requestBody)
		{
			return GetResponse(requestBody) != null;
		}

        [CLSCompliant(false)]
        public ResponseBody GetResponse(AMFBody requestBody)
		{
			for(int i = 0; i < _bodies.Count; i++)
			{
				ResponseBody responseBody = _bodies[i] as ResponseBody;
				if( responseBody.RequestBody == requestBody )
					return responseBody;
			}
			return null;
		}
	}
}
