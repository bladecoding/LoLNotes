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
using FluorineFx.Context;
using System.Security;
using System.Security.Permissions;

namespace FluorineFx
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class PagingContext
    {
        const string FluorinePagingContextKey = "__@fluorinepagingcontext";

        int _offset;

        /// <summary>
        /// The offset of the first row to return.
        /// </summary>
        public int Offset
        {
            get { return _offset; }
        }

        int _limit;

        /// <summary>
        /// The maximum number of rows to return.
        /// </summary>
        public int Limit
        {
            get { return _limit; }
        }

        internal PagingContext(int offset, int limit)
        {
            _offset = offset;
            _limit = limit;
        }

        /// <summary>
        /// Gets the PagingContext object for the current request.
        /// </summary>
        static public PagingContext Current
        {
            get
            {
                return FluorineWebSafeCallContext.GetData(FluorinePagingContextKey) as PagingContext;
            }
        }

        internal static void SetPagingContext(PagingContext current)
        {
            FluorineWebSafeCallContext.SetData(FluorinePagingContextKey, current);
        }
    }
}
