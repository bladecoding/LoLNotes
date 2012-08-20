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
using System.Web;
using System.IO;
using System.Security;
using System.Security.Permissions;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Endpoints
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMFContext
	{
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineAMFContextKey = "__@fluorineamfcontext";

		AMFMessage		_amfMessage;
		MessageOutput	_messageOutput;
		Stream			_inputStream;
		Stream			_outputStream;

		/// <summary>
		/// Initializes a new instance of the AMFContext class.
		/// </summary>
		public AMFContext(Stream inputStream, Stream outputStream)
		{
			_inputStream = inputStream;
			_outputStream = outputStream;
		}


		public AMFMessage AMFMessage
		{
			get{ return _amfMessage; }
			set{ _amfMessage = value; }
		}

		public MessageOutput MessageOutput
		{
			get{ return _messageOutput; }
			set{ _messageOutput = value; }
		}

		public Stream InputStream
		{
			get{ return _inputStream; }
		}

		public Stream OutputStream
		{
			get{ return _outputStream; }
		}

        /// <summary>
        /// Gets the FluorineContext object for the current HTTP request.
        /// </summary>
        static public AMFContext Current
        {
            get
            {
                AMFContext context = FluorineWebSafeCallContext.GetData(AMFContext.FluorineAMFContextKey) as AMFContext;
                return context;
            }
            set
            {
                FluorineWebSafeCallContext.SetData(AMFContext.FluorineAMFContextKey, value);
            }
        }        
	}
}
