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

namespace FluorineFx
{
	/// <summary>
	/// Indicates that the result of a service method is pageable recordset.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class PageSizeAttribute : System.Attribute
	{
		int _pageSize;
		int _offset;
		int _limit;

		/// <summary>
		/// Initializes a new instance of the PageSizeAttribute class.
		/// </summary>
		/// <param name="pageSize">The number of records requested each time.</param>
		public PageSizeAttribute(int pageSize)
		{
			_pageSize = pageSize;
			_offset = 0;
			_limit = 25;
		}
		/// <summary>
		/// Initializes a new instance of the PageSizeAttribute class.
		/// </summary>
		/// <param name="pageSize">The number of records requested each time.</param>
		/// <param name="offset">The offset of the first row to return.</param>
		/// <param name="limit">The maximum number of rows to return.</param>
		public PageSizeAttribute(int pageSize, int offset, int limit)
		{
			_pageSize = pageSize;
			_offset = offset;
			_limit = limit;
		}
		/// <summary>
		/// Gets the page size (number of records requested each time).
		/// </summary>
		public int PageSize
		{
			get{ return _pageSize; }
		}
		/// <summary>
		/// Gets the offset of the first row to return.
		/// </summary>
		public int Offset
		{
			get{ return _offset; }
		}
		/// <summary>
		/// Gets the maximum number of rows to return.
		/// </summary>
		public int Limit
		{
			get{ return _limit; }
		}
	}
}
