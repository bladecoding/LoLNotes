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

namespace FluorineFx.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class ListComparer : IComparer
	{
		public ListComparer()
		{
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			IList list1 = x as IList;
			IList list2 = y as IList;
			if( list1 != null && list2 != null )
			{
				if( list1.Count != list2.Count )
					return -1;
				for(int i = 0; i < list1.Count; i++)
				{
					bool equal = ( list1[i] != null ? list1[i].Equals(list2[i]) : list2[i] == null );
					if( !equal )
						return -1;
				}
			}
			return 0;
		}

		#endregion
	}
}
