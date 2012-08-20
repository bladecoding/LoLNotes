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

using FluorineFx.Messaging.Messages;

namespace FluorineFx.Data.Messages
{
	/// <summary>
	/// ErrorMessage that will be sent when a data conflict occurs.
	/// </summary>
	class DataErrorMessage : ErrorMessage
	{
		public DataMessage cause;
		public object serverObject;
		public IList propertyNames;

		public DataErrorMessage()
		{
		}

		public DataErrorMessage(DataSyncException dataSyncException)
		{
			//cause = dataSyncException.ConflictCause;
			serverObject = dataSyncException.ServerObject;
			propertyNames = dataSyncException.PropertyNames;
		}

        /// <summary>
        /// Returns a string that represents the current DataErrorMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current DataErrorMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            value += sep + "cause = " + (cause != null ? cause.ToString(indentLevel) : null);
            value += sep + "serverObject = " + serverObject;
            value += sep + "propertyNames = " + BodyToString(propertyNames, indentLevel);
            return value;
        }
	}
}
