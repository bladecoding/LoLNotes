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

namespace FluorineFx.Messaging.Messages
{
	/// <summary>
	/// A message that represents an infrastructure command passed between client and server. 
	/// Subscribe/unsubscribe operations result in CommandMessage transmissions, as do polling operations.
	/// </summary>
    [CLSCompliant(false)]
    public class CommandMessage : AsyncMessage
	{
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public const string AuthenticationMessageRefType = "flex.messaging.messages.AuthenticationMessage";
		/// <summary>
		/// This operation is used to test connectivity over the current channel to the remote endpoint.
		/// </summary>
		public const int ClientPingOperation = 5;
		/// <summary>
		/// This operation is used to subscribe to a remote destination.
		/// </summary>
		public const int SubscribeOperation = 0;
		/// <summary>
		/// This operation is used to unsubscribe from a remote destination.
		/// </summary>
		public const int UnsubscribeOperation = 1;
		/// <summary>
		/// This is the default operation for new CommandMessage instances. 
		/// </summary>
		public const int UnknownOperation = 10000;
		/// <summary>
		/// This operation is used to poll a remote destination for pending, undelivered messages.
		/// </summary>
		public const int PollOperation = 2;
		/// <summary>
		/// This operation is used by a remote destination to sync missed or cached messages back to a client as a result of a client issued poll command.
		/// </summary>
		public const int ClientSyncOperation = 4;
		/// <summary>
		/// This operation is used to request a list of failover endpoint URIs for the remote destination based on cluster membership.
		/// </summary>
		public const int ClusterRequestOperation = 7;
		/// <summary>
		/// This operation is used to send credentials to the endpoint so that the user can be 
		/// logged in over the current channel. The credentials need to be Base64 encoded and 
		/// stored in the body of the message.
		/// </summary>
		public const int LoginOperation = 8;
		/// <summary>
		/// This operation is used to log the user out of the current channel, and will 
		/// invalidate the server session if the channel is HTTP based. 
		/// </summary>
		public const int LogoutOperation = 9;
		/// <summary>
		/// This operation is used to indicate that the client's session with a remote destination has timed out.
		/// </summary>
		public const int SessionInvalidateOperation = 10;
        /// <summary>
        /// This operation is used to indicate that a channel has disconnected.
        /// </summary>
        public const int DisconnectOperation = 12;

		/// <summary>
		/// The name for the selector header in subscribe messages.
		/// </summary>
		public static string SelectorHeader = "DSSelector";
		/// <summary>
		/// The name for the header used internaly on the server to indicate that an 
		///	unsubscribe message is due to a client session being invalidated. 
		/// </summary>
		public static string SessionInvalidatedHeader = "DSSessionInvalidated";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public static string FluorineMessageClientTimeoutHeader = "FluorineMessageClientTimeout";
        /// <summary>
        /// Poll wait must be suppressed.
        /// </summary>
        internal static string FluorineSuppressPollWaitHeader = "FluorineSuppressPollWait";


		int _operation;


		/// <summary>
		/// Initializes a new instance of the CommandMessage class.
		/// The message id is set to a universally unique 
		/// value, and the timestamp for the message is set to the current system timestamp. 
		/// The operation is set to a default value of UnknownOperation.
		/// </summary>
		public CommandMessage()
		{
			_operation = CommandMessage.UnknownOperation;
		}
		/// <summary>
		/// Initializes a new instance of the CommandMessage class. The message id is set to a 
		/// universally unique value, and the timestamp for the message is set 
		/// to the current system timestamp.
		/// </summary>
        /// <param name="operation">Operation for the new CommandMessage instance.</param>
		public CommandMessage(int operation)
		{
			_operation = operation;
		}
		/// <summary>
		/// Gets or sets the operation for this CommandMessage.
		/// </summary>
		public int operation
		{
			get{ return _operation; }
			set{ _operation = value; }
		}

        protected override MessageBase CopyImpl(MessageBase clone)
        {
            // Instantiate the clone, if a derived type hasn't already.
            if (clone == null) clone = new CommandMessage();
            // Allow base type(s) to copy their state into the new clone.
            base.CopyImpl(clone);
            // Copy our state into the clone.
            //((CommandMessage)clone)._messageRefType = _messageRefType;
            ((CommandMessage)clone)._operation = _operation;
            return clone;
        }

        static string[] OperationNames = { "subscribe", "unsubscribe", "poll", "unused3", "client_sync", "client_ping", "unused6", "cluster_request", "login", "logout", "subscription_invalidate", "multi_subscribe", "disconnect", "trigger_connect" };

        /// <summary>
        /// Converts operation code to string.
        /// </summary>
        /// <param name="operation">The operation code.</param>
        /// <returns>A string representing the operation code.</returns>
        public static string OperationToString(int operation)
        {
            if (operation < 0 || operation >= OperationNames.Length)
                return "invalid operation " + operation;
            return OperationNames[operation];
        }

        /// <summary>
        /// Returns a string that represents the current CommandMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current CommandMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = sep + "operation = " + OperationToString(operation);
            if (operation == SubscribeOperation)
                value += sep + "selector = " + GetHeader(SelectorHeader);
            if (operation != LoginOperation)
            {
                value += base.ToStringFields(indentLevel);
            }
            else
            {
                value += sep + "clientId =  " + clientId;
                value += sep + "destination =  " + destination;
                value += sep + "messageId =  " + messageId;
                value += sep + "timestamp =  " + timestamp;
                value += sep + "timeToLive =  " + timeToLive;
                value += sep + "***not printing credentials***";
            }
            return value;
        }
	}
}
