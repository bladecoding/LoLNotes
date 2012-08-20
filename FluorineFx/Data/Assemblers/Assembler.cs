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

namespace FluorineFx.Data.Assemblers
{
	/// <summary>
	/// Assembler base class.
	/// </summary>
	public class Assembler : IAssembler
	{
		/// <summary>
		/// Add this item onto the set of items in this filled collection if it is not already in the collection.
		/// </summary>
		public const int AppendToFill  = 2;
		/// <summary>
		/// Do not execute the fill method for this newly updated or created item. 
		/// </summary>
		public const int DoNotExecuteFill = 0;
		/// <summary>
		/// Reexecute the fill method for this newly updated or created item. 
		/// </summary>
		public const int ExecuteFill = 1;
		/// <summary>
		/// Remove this item from the set of items in this filled collection if it is in the collection.
		/// </summary>
		public const int RemoveFromFill = 3;

		/// <summary>
		/// Initializes a new instance of the Assembler class.
		/// </summary>
		public Assembler()
		{
		}

		#region IAssembler Members

		/// <summary>
		/// Called when a client adds an item to a filled collection.
		/// </summary>
		/// <param name="fillParameters">The list of parameters which identify the fill that the client changed.</param>
		/// <param name="position">The index where a new item was added.</param>
		/// <param name="identity">The identity of the item added at the specified position.</param>
		public virtual void AddItemToFill(IList fillParameters, int position, Hashtable identity)
		{
		}
		/// <summary>
		/// This method can be used to help control how fill methods are refreshed.
		/// </summary>
		/// <param name="fillParameters">Client-side parameters to a fill method that created a managed collection still managed by one or more clients.</param>
		/// <returns>true if the fill identified by the fill parameters should be auto-refreshed or false if auto-refresh is off for this fill.</returns>
		public virtual bool AutoRefreshFill(IList fillParameters)
		{
			return true;
		}
		/// <summary>
		/// Retrieve the number of items for a given query with the supplied parameters.
		/// </summary>
		/// <param name="countParameters">A list of parameters to the count method provided by the client invocation.</param>
		/// <returns>The number of items in the collection specified by the countParameters.</returns>
		public virtual int Count(IList countParameters)
		{
			throw new NotSupportedException("Assembler does not implement Count method");
		}
		/// <summary>
		/// Creates the item. Often, you fill in the identity properties unless those values were supplied by the client. 
		/// </summary>
		/// <param name="item">The initial instance of the item to create.</param>
		public virtual void CreateItem(object item)
		{
			throw new NotSupportedException("Assembler does not implement CreateItem method");
		}
		/// <summary>
		/// This is called when the client application removes an item managed by the destination corresponding to this assembler. 
		/// </summary>
		/// <param name="previousVersion">The original version of the item on the client which the client intends to remove.</param>
		public virtual void DeleteItem(object previousVersion)
		{
			throw new NotSupportedException("Assembler does not implement DeleteItem method");
		}
		/// <summary>
		/// This method is called by for any fill methods called by the client which are not configured in the configuration file explicitly using the fill-method tag.
		/// </summary>
		/// <param name="list">The list of fill parameters provided to the DataService.fill method on the client. Note that the first parameter - the ArrayCollection is not included in this list.</param>
		/// <returns>A collection containing a list of items to be managed by the client. This collection should contain instances which all have valid identity properties and should not contain more than one instance with the same identity.</returns>
		public virtual IList Fill(IList list)
		{
			throw new NotSupportedException("Assembler does not implement Fill method");
		}
		/// <summary>
		/// Retrieves an item with the specified identity.
		/// </summary>
		/// <param name="identity">A Hashtable which contains key/value pairs for each identity property.</param>
		/// <returns>The item corresponding to this identity property or null if there is no item for this identity.</returns>
        public virtual object GetItem(IDictionary identity)
		{
			throw new NotSupportedException("Assembler does not implement GetItem method");
		}
		/// <summary>
		/// Given a list of identities, returns the list of items.
		/// </summary>
		/// <param name="identityList">A list of Hashtable objects specifying the list of items.</param>
		/// <returns>The list of items corresponding to the list of identities specified. If an item is not found, a null value should be placed into the list to indicate that.</returns>
		public virtual IList GetItems(IList identityList)
		{
			ArrayList result = new ArrayList(identityList.Count);
			for(int i = 0; i < identityList.Count; i++)
                result.Add(GetItem(identityList[i] as IDictionary));
			return result;
		}
		/// <summary>
		/// If your fill methods are auto-refreshed, this method is called for each item that changes (either created or updated as indicated by the isCreate parameter).
		/// </summary>
		/// <param name="fillParameters">The parameters which identify a fill method that is still actively being managed by one or more clients connected to this server.</param>
		/// <param name="item">The item which is being created or updated in a recently committed transaction.</param>
		/// <param name="isCreate">true if this item was just created operation, false if it was just updated.</param>
		/// <returns>Assembler.DoNotExecuteFill - do nothing, Assembler.ExecuteFill - re-run the fill method to get the new list, Assembler.AppendToFill - just add it to the existing list, Assembler.RemoveFromFill - remove it from the sequence</returns>
		public virtual int RefreshFill(IList fillParameters, object item, bool isCreate)
		{
			return Assembler.ExecuteFill;
		}
		/// <summary>
		/// Called when a client removes an item from a filled collection.
		/// </summary>
		/// <param name="fillParameters">The list of parameters which identify the fill that the client changed.</param>
		/// <param name="position">The index where a new item was removed.</param>
		/// <param name="identity">The identity of the item removed at the specified position.</param>
		public virtual void RemoveItemFromFill(IList fillParameters, int position, Hashtable identity)
		{
		}
		/// <summary>
		/// Updates the item. The newVersion is always going to be present and contains the new version of the item.
		/// The previousVersion contains any state required for maintaining the integrity of this instance.
		/// </summary>
		/// <param name="newVersion">The new version of the item with which to perform the update.</param>
		/// <param name="previousVersion">The original version of the item before these changes were made (used for conflict detection).</param>
		/// <param name="changes">The list of changed property names.</param>
		public virtual void UpdateItem(object newVersion, object previousVersion, IList changes)
		{
			throw new NotSupportedException("Assembler does not implement UpdateItem method");
		}

		#endregion
	}
}
