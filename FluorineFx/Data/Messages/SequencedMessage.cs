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

using FluorineFx.Messaging.Messages;

namespace FluorineFx.Data.Messages
{
	/// <summary>
	/// Clients receive this message in response to DataService.fill() request. 
	/// The body of the message is an Array of items that were returned from 
	/// the remote destination based on the fill parameters.
	/// </summary>
    [CLSCompliant(false)]
	public class SequencedMessage : AcknowledgeMessage
	{	
		int _sequenceId;
		int _sequenceSize;
		DataMessage _dataMessage;
		object[] _sequenceProxies;

        /// <summary>
        /// Initializes a new instance of the SequencedMessage class.
        /// </summary>
        public SequencedMessage()
		{
		}

		/// <summary>
		/// Provides access to the sequence id for this message. 
		/// The sequence id is a unique identifier for a sequence within a remote destination. 
		/// This value is only unique for the endpoint and destination contacted. 
		/// </summary>
		public int sequenceId
		{
			get{ return _sequenceId; }
			set{ _sequenceId = value; }
		}
		/// <summary>
		/// Provides access to the sequence size for this message.
		/// The sequence size indicates how many items reside in the remote sequence. 
		/// </summary>
		public int sequenceSize
		{
			get{ return _sequenceSize; }
			set{ _sequenceSize = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public DataMessage dataMessage
		{
			get{ return _dataMessage; }
			set{ _dataMessage = value; }
		}
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public object[] sequenceProxies
		{
			get{ return _sequenceProxies; }
			set{ _sequenceProxies = value; }
		}

        /// <summary>
        /// Returns a string that represents the current SequencedMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current SequencedMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = sep + "sequenceId = " + sequenceId + sep + "sequenceSize = " + sequenceSize;
            if (sequenceProxies == null || sequenceProxies.Length == 0)
            {
                value += sep + "(no sequence proxies)";
            }
            else
            {
                string sep2 = GetFieldSeparator(indentLevel + 1);
                value += sep + sequenceProxies.Length + " sequenceProxies ";
                for (int i = 0; i < sequenceProxies.Length; i++)
                    value += sep2 + "[" + i + "] = " + BodyToString(sequenceProxies[i], indentLevel + 1);
            }
            if (dataMessage != null)
                value += sep + "dataMessage = " + dataMessage.ToString(indentLevel + 1);
            value += base.ToStringFields(indentLevel);
            return value;
        }
	}
}
