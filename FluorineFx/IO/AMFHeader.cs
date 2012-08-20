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

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFHeader
	{
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string CredentialsHeader = "Credentials";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// DSid stands for Data Set Identification. It represents a Guid used to identify the remote client.
        /// </summary>
        public const string AMFDSIdHeader = "DSId";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string DebugHeader = "amf_server_debug";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string ServiceBrowserHeader = "DescribeService";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string ClearedCredentials = "ClearedCredentials";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string CredentialsIdHeader = "CredentialsId";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string RequestPersistentHeader = "RequestPersistentHeader";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string UrlAppendHeader = "AppendToGatewayUrl";

		object	_content;
		bool	_mustUnderstand;
		string	_name;

		/// <summary>
		/// Initializes a new instance of the AMFHeader class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mustUnderstand"></param>
		/// <param name="content"></param>
		public AMFHeader(string name, bool mustUnderstand, object content)
		{
			this._name = name;
			this._mustUnderstand = mustUnderstand;
			this._content = content;
		}
        /// <summary>
        /// Gets the header name.
        /// </summary>
		public string Name
		{
			get{ return _name; }
		}
        /// <summary>
        /// Gets the header content.
        /// </summary>
		public object Content
		{
			get{ return _content; }
		}
        /// <summary>
        /// If a header is sent to the Flash Player with must understand set to true and the NetConnection instance's client object does not have a method to handle the header, then the Flash Player will invoke the onStatus handler on the NetConnection object.
        /// </summary>
		public bool MustUnderstand
		{
			get{ return _mustUnderstand; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public bool IsClearedCredentials
		{
			get
			{
				if( _content is string )
					return (string)_content == AMFHeader.ClearedCredentials;
				return false;
			}
		}
	}
}
