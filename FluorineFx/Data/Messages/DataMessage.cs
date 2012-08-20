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
	/// DataMessage transports an operation that occured on a managed object or collection. This class of message is transmitted between clients subscribed to a remote destination.
	/// The message describes all of the relevant details of the operation (used to replicate updates and detect conflicts).
	/// </summary>
    [CLSCompliant(false)]
    public class DataMessage : AsyncMessage
	{
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
		public const string PageSizeHeader = "pageSize";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string PageIndexHeader = "pageIndex";
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string SequenceIdHeader = "sequenceId";

		/// <summary>
		/// Indicates a create operation.
		/// </summary>
		public const int CreateOperation = 0;
		/// <summary>
		/// This operation requests that the remote destination create a sequence using 
		/// the remote destination's adapter.
		/// </summary>
		public const int FillOperation = 1;
		/// <summary>
		/// This operation requests that the remote destination get a specific managed object based on its unique ID.
		/// </summary>
		public const int GetOperation = 2;
		/// <summary>
		/// This operation indicates an update to data object has been performed.
		/// </summary>
		public const int UpdateOperation = 3;
		/// <summary>
		/// This operation indicates that the specified item should be removed.
		/// </summary>
		public const int DeleteOperation = 4;
		/// <summary>
		/// This operation represents a set of batched operations to be performed as a single unit.
		/// </summary>
		public const int BatchedOperation = 5;
		/// <summary>
		/// This operation represents a set of operations to be performed as a single unit but 
		/// which may contain multiple batched, create, update or delete operations that involve 
		/// more than one destination, that is, more than one remote adapter.
		/// </summary>
		public const int MultiBatchOperation = 6;
		/// <summary>
		/// This operation is similar to the MultiBatchOperation with the addition that the server 
		/// should encapsulate the multiple batches of messages within a transacation.
		/// </summary>
		public const int TransactedOperation = 7;
		/// <summary>
		/// This operation is used to retrieve a page of sequenced content that is delivered across 
		/// several messages instead of in a single message.
		/// </summary>
		public const int PageOperation = 8;
        /// <summary>
        /// This operation requests that a configured &lt;count-method&gt; be invoked on a remote destination.
        /// </summary>
        public const int CountOperation = 9;
        /// <summary>
		/// This operation requests an item with the specified identity from the remote destination.
		/// </summary>
		public const int GetOrCreateOperation = 10;
		/// <summary>
		/// This operation requests a create of the specified item from a remote destination.
		/// </summary>
		public const int CreateAndSequenceOperation = 11;
		/// <summary>
		/// This operation requests a sequence id for a set of fill parameters.
		/// </summary>
		public const int GetSequenceIdOperation = 12;
		/// <summary>
		/// This operation requests the remote destination add a new association between the specified instances.
		/// </summary>
		public const int AssociationAddOperation = 13;
		/// <summary>
		/// This operation requests the remote destination remove an association between the specified instances.
		/// </summary>
		public const int AssociationRemoveOperation = 14;
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int RefreshFillOperation = 16;
		/// <summary>
		/// This operation is sent when a local or remote sequence has been modified by insert(s) or delete(s).
		/// </summary>
		public const int UpdateCollectionOperation = 17;
		/// <summary>
		/// This operation indicates that the client is no longer interested in receiving notificaion of operations performed on the specified collection.
		/// </summary>
		public const int ReleaseCollectionOperation = 18;
		/// <summary>
		/// This operation indicates that the client is no longer interested in receiving notification of operations performed on the specified item.
		/// </summary>
		public const int ReleaseItemOperation = 19;
		/// <summary>
		/// This operation indicates a request for a page of items specified by identities.
		/// </summary>
		public const int PageItemsOperation = 20;

		/// <summary>
		/// This constant is used to access the list of changed property names.
		/// </summary>
		public const int UpdateBodyChanges = 0;
		/// <summary>
		/// This constant is used to access the previous value of the changed item.
		/// </summary>
		public const int UpdateBodyPrev = 1;
		/// <summary>
		/// This constant is used to access the new value of a changed item.
		/// </summary>
		public const int UpdateBodyNew = 2;


        /// <summary>
        /// Operation/command of this DataMessage.
        /// </summary>
        protected int _operation;
        /// <summary>
        /// Identity hash which defines the unique identity of the item affected by this DataMessage.
        /// </summary>
		Hashtable	_identity;

        /// <summary>
        /// Initializes a new instance of the DataMessage class.
        /// </summary>
        public DataMessage()
		{
			_timestamp = System.Environment.TickCount;
		}
		/// <summary>
		/// Gets or sets the operation/command of this DataMessage.
		/// Operations indicate how the remote destination should process this message. 
		/// </summary>
		public int operation
		{
			get{ return _operation; }
			set{ _operation = value; }
		}
		/// <summary>
		/// Gets or sets the identity hash which defines the unique identity of the item affected by this DataMessage 
		/// (relevant for create/update/delete but not fill operations). 
		/// </summary>
		public Hashtable identity
		{
			get{ return _identity; }
			set{ _identity = value; }
		}

        static string[] OperationNames = { "create", "fill", "get", "update", "delete", "batched", "multi_batch", "transacted", "page", "count", "get_or_create", "create_and_sequence", "get_sequence_id", "association_add", "association_remove", "fillids", "refresh_fill", "update_collection", "release_collection", "release_item", "page_items" };

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
        /// Returns a string that represents the current DataMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current DataMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            value += sep + "operation = " + OperationToString(operation);
            value += sep + "id = " + BodyToString(identity, indentLevel);
            return value;
        }
	}
}
