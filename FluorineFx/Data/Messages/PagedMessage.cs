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
	/// This messsage provides information about a partial sequence result.
	/// When paging is enabled for a destination and DataService.fill() or a page request 
	/// is made the remote destination will return this message as a response.
	/// The body property is an Array containing the items for the requested page with a 
	/// length of the configured page size.
	/// </summary>
    [CLSCompliant(false)]
    public class PagedMessage : SequencedMessage
	{
		int _pageCount;
		int _pageIndex;

        /// <summary>
        /// Initializes a new instance of the PagedMessage class.
        /// </summary>
        public PagedMessage()
		{
		}

		/// <summary>
		/// Provides access to the number of total pages in a sequence based on the current page size.
		/// </summary>
		public int pageCount
		{
			get{ return _pageCount; }
			set{ _pageCount = value; }
		}
		/// <summary>
		/// Provides access to the index of the current page in a sequence.
		/// </summary>
		public int pageIndex
		{
			get{ return _pageIndex; }
			set{ _pageIndex = value; }
		}

        /// <summary>
        /// Returns a string that represents the current PagedMessage object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>
        /// A string that represents the current PagedMessage object fields.
        /// </returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = sep + "pageCount = " + pageCount;
            value += sep + "pageIndex = " + pageIndex;
            value += base.ToStringFields(indentLevel);
            return value;
        }
	}
}
