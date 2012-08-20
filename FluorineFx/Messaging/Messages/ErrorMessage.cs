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
using System.Security;

namespace FluorineFx.Messaging.Messages
{
	/// <summary>
	/// The ErrorMessage class is used to report errors within the messaging system.
	/// An error message only occurs in response to a message sent within the system.
	/// </summary>
    [CLSCompliant(false)]
    public class ErrorMessage : AcknowledgeMessage
	{
        /// <summary>
        /// Client authentication fault code.
        /// </summary>
        public const string ClientAuthenticationError = "Client.Authentication";
        /// <summary>
        /// Client authorization fault code.
        /// </summary>
        public const string ClientAuthorizationError = "Client.Authorization";

		string _faultCode;
		string _faultString;
		string _faultDetail;
		object _rootCause;
		ASObject _extendedData;

		/// <summary>
		/// Initializes a new instance of the ErrorMessage class.
		/// </summary>
		public ErrorMessage()
		{
		}
		/// <summary>
		/// The fault code for the error. 
		/// This value typically follows the convention of "[outer_context].[inner_context].[issue]".
		/// 
		/// For example: "Channel.Connect.Failed", "Server.Call.Failed"
		/// </summary>
		public string faultCode
		{
			get{ return _faultCode; }
			set{ _faultCode = value; }
		}
		/// <summary>
		/// A simple description of the error.
		/// </summary>
		public string faultString
		{
			get{ return _faultString; }
			set{ _faultString = value; }
		}
		/// <summary>
		/// Detailed description of what caused the error. This is typically a stack trace from the remote destination
		/// </summary>
		public string faultDetail
		{
			get{ return _faultDetail; }
			set{ _faultDetail = value; }
		}
		/// <summary>
		/// Root cause for the error.
		/// </summary>
		public object rootCause
		{
			get{ return _rootCause; }
			set{ _rootCause = value; }
		}
        /// <summary>
        /// Extended data that the remote destination can choose to associate with this error for custom error processing on the client.
        /// </summary>
		public ASObject extendedData
		{
			get{ return _extendedData; }
			set{ _extendedData = value; }
		}

		internal static ErrorMessage GetErrorMessage(IMessage message, Exception exception)
		{
			MessageException me = null;
			if( exception is MessageException )
				me = exception as MessageException;
			else
				me = new MessageException(exception);
			ErrorMessage errorMessage = me.GetErrorMessage();
            if (message.clientId != null)
                errorMessage.clientId = message.clientId;
            else
                errorMessage.clientId = Guid.NewGuid().ToString("D");
			errorMessage.correlationId = message.messageId;
			errorMessage.destination = message.destination;
			if(exception is SecurityException)
                errorMessage.faultCode = ErrorMessage.ClientAuthenticationError;
			if(exception is UnauthorizedAccessException)
                errorMessage.faultCode = ErrorMessage.ClientAuthorizationError;
			return errorMessage;
		}

        protected override MessageBase CopyImpl(MessageBase clone)
        {
            // Instantiate the clone, if a derived type hasn't already.
            if (clone == null) clone = new ErrorMessage();
            // Allow base type(s) to copy their state into the new clone.
            base.CopyImpl(clone);
            // Copy our state into the clone.
            ((ErrorMessage)clone)._faultCode = _faultCode;
            ((ErrorMessage)clone)._faultString = _faultString;
            ((ErrorMessage)clone)._faultDetail = _faultDetail;
            ((ErrorMessage)clone)._rootCause = _rootCause;
            ((ErrorMessage)clone)._extendedData = _extendedData;
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current ErrorMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current ErrorMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            value += sep + "code =  " + faultCode;
            value += sep + "message =  " + faultString;
            value += sep + "details =  " + faultDetail;
            value += sep + "rootCause =  ";
            if (rootCause == null)
                value += "null";
            else
                value += rootCause.ToString();
            value += sep + "body =  " + BodyToString(body, indentLevel);
            value += sep + "extendedData =  " + BodyToString(extendedData, indentLevel);
            return value;
        }
	}
}
