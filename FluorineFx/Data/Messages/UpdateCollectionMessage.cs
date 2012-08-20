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

namespace FluorineFx.Data.Messages
{
	/// <summary>
	/// This message is used to establish consistency between the remote sequence and the 
	/// corresponding local collection. It contains all insert and delete operations that 
	/// were performed on a collection. 
	/// Clients send this message when a local collection is updated using the collection API 
	/// (IListView.removeItemAt(), etc) or the Single Managed Object API (DataService.createItem()). 
	/// The remote destination sends this message when the remote sequence is updated 
	/// and items are moved or removed and inserted. 
	/// This body property contains a list of UpdateCollectionRange objects that indicate just 
	/// how the collection was modified. 
	/// Applying the update collection ranges inorder will establish a consistent ordering of the items within the specified collection. 
	/// </summary>
    [CLSCompliant(false)]
    public class UpdateCollectionMessage : DataMessage
	{
		/// <summary>
		///  Indicates this update collection message was client generated.
		/// </summary>
		public const int ClientUpdate = 0;
		/// <summary>
		/// Indicates this update collection message was client generated and the 
		/// remote destination determined that it should be reverted on that client.
		/// </summary>
		public const int ServerOverride = 2;
		/// <summary>
		/// Indicates this update collection message was remotely generated and is 
		/// based on the current state of the remote sequence.
		/// </summary>
		public const int ServerUpdate = 1;

		object[] _collectionId;
		bool _replace;
		int _updateMode;

        /// <summary>
        /// Initializes a new instance of the UpdateCollectionMessage class.
        /// </summary>
		public UpdateCollectionMessage()
		{
			_operation = DataMessage.UpdateCollectionOperation;
		}

		/// <summary>
		/// The unique identifier for the collection that was updated. 
		/// For a collection filled with the DataService.fill() method this contains 
		/// and Array of the parameters specified.
		/// </summary>
		public object[] collectionId
		{
			get{ return _collectionId; }
			set{ _collectionId = value; }
		}
		/// <summary>
		/// Indicates if the entire collection should be replaced by the contents of this message.
		/// When the number of changes to a remote sequence have reached a tipping point a 
		/// replace message is generated as an optimization. 
		/// When true the body property contains an Array of item identities that should 
		/// replace any existing items. 
		/// </summary>
		public bool replace
		{
			get{ return _replace; }
			set{ _replace = value; }
		}
		/// <summary>
		/// Indicates the state of this update. The remote destination sends update collection messages 
		/// to clients with one of three update modes: 
		/// ServerUpdate - client applies this update collection unconditionally 
		/// ClientUpdate - the committing client does not need to process this update collection. 
		/// ServerOverride - the remote destination modified the update collection message sent by the client and the committing client must revert this update collection. 
		/// </summary>
		public int updateMode
		{
			get{ return _updateMode; }
			set{ _updateMode = value; }
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="updateType"></param>
        /// <param name="position"></param>
        /// <param name="identity"></param>
		public void AddItemIdentityChange(int updateType, int position, object identity)
		{
			UpdateCollectionRange range = new UpdateCollectionRange();
			range.position = position;
			range.updateType = updateType;
			range.identities = new object[1];
			range.identities[0] = identity;

			object[] bodyArray = this.body as object[];
			if(bodyArray == null)
			{
				bodyArray = new object[1];
				bodyArray[0] = range;
			} 
			else
			{
				object[] newbody = new object[ bodyArray.Length + 1 ];
				bodyArray.CopyTo(newbody, 0);
				bodyArray[ bodyArray.Length-1 ] = range;
			}
			this.body = bodyArray;
		}

        /// <summary>
        /// Returns a string that represents the current UpdateCollectionMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current UpdateCollectionMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            value += sep + "collectionId = " + BodyToString(collectionId, indentLevel + 1);
            value += sep + "replace = " + replace;
            value += sep + "updateMode = " + UpdateModeToString(updateMode);
            return value;
        }

        /// <summary>
        /// Converts updateMode code to string.
        /// </summary>
        /// <param name="operation">The updateMode code.</param>
        /// <returns>A string representing the updateMode code.</returns>
        public static string UpdateModeToString(int updateMode)
        {
            if (updateMode < 0 || updateMode >= UpdateModes.Length)
                return "invalid mode " + updateMode;
            return UpdateModes[updateMode];
        }

        static string[] UpdateModes = { "client_update", "server_update", "server_override" };
	}
}
