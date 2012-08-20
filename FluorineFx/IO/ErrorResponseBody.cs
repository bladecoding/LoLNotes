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

using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ErrorResponseBody : ResponseBody
	{
	    /// <summary>
		/// Initializes a new instance of the ErrorResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="error"></param>
		public ErrorResponseBody(AMFBody requestBody, string error):base(requestBody)
		{
			IgnoreResults = requestBody.IgnoreResults;
			Target = requestBody.Response + AMFBody.OnStatus;
			Response = null;
			Content = new ExceptionASO(error);
		}
		/// <summary>
		/// Initializes a new instance of the ErrorResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="exception"></param>
		public ErrorResponseBody(AMFBody requestBody, Exception exception):base(requestBody)
		{
			if( requestBody.IsEmptyTarget )
			{
				object content = requestBody.Content;
				if( content is IList )
					content = (content as IList)[0];
				IMessage message = content as IMessage;
				//Check for Flex2 messages and handle
				if( message != null )
				{
					ErrorMessage errorMessage = ErrorMessage.GetErrorMessage(message, exception);
					Content = errorMessage;
				}
			}
            if( Content == null )
                Content = new ExceptionASO(exception);
			IgnoreResults = requestBody.IgnoreResults;
            Target = requestBody.Response + AMFBody.OnStatus;
			Response = null;
		}
		/// <summary>
		/// Initializes a new instance of the ErrorResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="message"></param>
		/// <param name="exception"></param>
		public ErrorResponseBody(AMFBody requestBody, IMessage message, Exception exception):base(requestBody)
		{
			ErrorMessage errorMessage = ErrorMessage.GetErrorMessage(message, exception);
			Content = errorMessage;
            Target = requestBody.Response + AMFBody.OnStatus;
			IgnoreResults = requestBody.IgnoreResults;
			Response = "";
		}
		/// <summary>
		/// Initializes a new instance of the ErrorResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="message"></param>
		/// <param name="errorMessage"></param>
		public ErrorResponseBody(AMFBody requestBody, IMessage message, ErrorMessage errorMessage):base(requestBody)
		{
			Content = errorMessage;
            Target = requestBody.Response + AMFBody.OnStatus;
			IgnoreResults = requestBody.IgnoreResults;
			Response = "";
		}

	}
}
