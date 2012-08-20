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
using System.Web;

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class HttpApplicationStateWrapper : IApplicationState
	{
		public HttpApplicationStateWrapper()
		{
		}

		#region IApplicationState Members

		public object this[int index]
		{
			get
			{
				return HttpContext.Current.Application[index];
			}
		}

		public object this[string name]
		{
			get
			{
				return HttpContext.Current.Application[name];
			}
			set
			{
				HttpContext.Current.Application[name] = value;
			}
		}

		public void Remove(string key)
		{
			HttpContext.Current.Application.Remove(key);
		}

		public void RemoveAt(int index)
		{
			HttpContext.Current.Application.RemoveAt(index);
		}

		public void Add(string name, object value)
		{
			HttpContext.Current.Application.Add(name, value);
		}

        /// <summary>
        /// Locks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void Lock()
        {
            HttpContext.Current.Application.Lock();
        }
        /// <summary>
        /// Unlocks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void UnLock()
        {
            HttpContext.Current.Application.UnLock();
        }

		#endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return HttpContext.Current.Application.GetEnumerator();
        }

        #endregion
    }
}
