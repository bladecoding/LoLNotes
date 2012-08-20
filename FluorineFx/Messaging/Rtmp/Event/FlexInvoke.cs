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
using System.IO;

using FluorineFx.Util;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Event;

namespace FluorineFx.Messaging.Rtmp.Event
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public sealed class FlexInvoke : Invoke
	{
        /// <summary>
        /// Method name
        /// </summary>
		//string _cmd;
        /// <summary>
        /// Response
        /// </summary>
		object _cmdData;
		//object[] _parameters;
		//object	_response;

        public FlexInvoke():base()
		{
			_dataType = Constants.TypeFlexInvoke;
            //SetResponseSuccess();
		}

        internal FlexInvoke(int invokeId, object cmdData)
            : this()
		{
			//_cmd = cmd;
			this.InvokeId = invokeId;
			_cmdData = cmdData;
			//_parameters = parameters;
		}

        internal FlexInvoke(ByteBuffer data)
            : base(data)
		{
            _dataType = Constants.TypeFlexInvoke;
		}
        /*
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
		public object[] Parameters
		{ 
			get{ return _parameters; } 
			set{ _parameters = value; } 
		}
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
		public string Cmd
		{ 
			get{ return _cmd; } 
			set{ _cmd = value; }
		}
        /// <summary>
        /// Gets or sets the response object.
        /// </summary>
		public object Response
		{
			get{ return _response; } 
			set{ _response = value; } 
		}
		/// <summary>
		/// Sets success response.
		/// </summary>
		public void SetResponseSuccess()
		{
			_cmd = "_result";
		}
        /// <summary>
        /// Sets failure response.
        /// </summary>
		public void SetResponseFailure()
		{
			_cmd = "_error";
		}
        */
        /// <summary>
		/// Gets or sets the command data.
		/// </summary>
		public object CmdData
		{ 
			get{ return _cmdData; } 
		}
        /// <summary>
        /// Returns a string that represents the current object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the members.</param>
        /// <returns>A string that represents the current object fields.</returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            //value += sep + "cmd = " + BodyToString(_cmd, indentLevel + 1);
            value += sep + "cmdData = " + _cmdData;
            //value += sep + "parameters = " + BodyToString(_parameters, indentLevel + 1);
            //value += sep + "response = " + BodyToString(_response, indentLevel + 1);
            return value;
        }
	}
}
