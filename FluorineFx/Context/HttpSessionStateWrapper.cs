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
using System.Web.SessionState;

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class HttpSessionStateWrapper : ISessionState
	{
		HttpSessionState	_httpSessionState;

		private HttpSessionStateWrapper(HttpSessionState httpSessionState)
		{
			_httpSessionState = httpSessionState;
		}

        internal static HttpSessionStateWrapper CreateSessionWrapper(HttpSessionState httpSessionState)
        {
            HttpSessionStateWrapper httpSessionStateWrapper = new HttpSessionStateWrapper(httpSessionState);
            //if (httpSessionState.IsNewSession)
            //    httpSessionStateWrapper.NotifyCreated();
            return httpSessionStateWrapper;
        }
        
        public void CopyTo(Array array, int index)
        {
            _httpSessionState.CopyTo(array, index);
        }

        public int Count
        {
            get { return _httpSessionState.Count; }
        }

        public bool IsSynchronized
        {
            get { return _httpSessionState.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _httpSessionState.SyncRoot; }
        }

        public IEnumerator GetEnumerator()
        {
            return _httpSessionState.GetEnumerator();
        }

        public void Add(string name, object value)
        {
            _httpSessionState.Add(name, value);
        }

        public void Clear()
        {
            _httpSessionState.Clear();
        }

        public void Remove(string name)
        {
            _httpSessionState.Remove(name);
        }

        public void RemoveAll()
        {
            _httpSessionState.RemoveAll();
        }

        public void RemoveAt(int index)
        {
            _httpSessionState.RemoveAt(index);
        }

        public string SessionID
        {
            get { return _httpSessionState.SessionID; }
        }

        public object this[string name]
        {
            get
            {
                return _httpSessionState[name];
            }
            set
            {
                _httpSessionState[name] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return _httpSessionState[index];
            }
            set
            {
                _httpSessionState[index] = value;
            }
        }
    }
}
