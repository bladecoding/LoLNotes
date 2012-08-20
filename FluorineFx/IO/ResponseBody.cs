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
using System.IO;

using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Configuration;

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public class ResponseBody : AMFBody
	{
		AMFBody	_requestBody;

		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		internal ResponseBody()
		{
		}
		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		public ResponseBody(AMFBody	requestBody)
		{
			_requestBody = requestBody;
		}
		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="content"></param>
		public ResponseBody(AMFBody	requestBody, object content)
		{
			_requestBody = requestBody;
			_target = requestBody.Response + AMFBody.OnResult;
			_content = content;
			_response = "null";
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public AMFBody RequestBody
		{
			get{ return _requestBody; }
			set{ _requestBody = value; }
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public override IList GetParameterList()
		{
			if( _requestBody == null )
				return null;

			return _requestBody.GetParameterList ();
		}
	}
}
