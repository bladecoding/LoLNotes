/*
	Fluorine Projector SWF2Exe open source library based on Flash Remoting
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
using System.Threading;

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class RtmpApplicationState : IApplicationState
	{
        private static RtmpApplicationState _instance;
		private static SortedList _applicationState;
        protected static object _objLock = new object();

        private RtmpApplicationState()
		{
		}

        static public RtmpApplicationState Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_objLock)
                    {
                        if (_instance == null)
                            _instance = new RtmpApplicationState();
                    }
                }
                return _instance;
            }
        }

        private SortedList GetApplicationStateData()
		{
            if (_applicationState == null)
            {
                lock (_objLock)
                {
                    if (_applicationState == null)
                        _applicationState = new SortedList();
                }
            }
            return _applicationState;
		}


        #region IApplicationState Members

        public object this[int index]
        {
            get 
            {
                lock (_objLock)
                {
                    return GetApplicationStateData().GetByIndex(index);
                }
            }
        }

        public object this[string name]
        {
            get
            {
                lock (_objLock)
                {
                    return GetApplicationStateData()[name];
                }
            }
            set
            {
                lock (_objLock)
                {
                    GetApplicationStateData()[name] = value;
                }
            }
        }

        public void Remove(string key)
        {
            lock (_objLock)
            {
                GetApplicationStateData().Remove(key);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_objLock)
            {
                GetApplicationStateData().RemoveAt(index);
            }
        }

        public void Add(string name, object value)
        {
            lock (_objLock)
            {
                GetApplicationStateData().Add(name, value);
            }
        }

        /// <summary>
        /// Locks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void Lock()
        {
            Monitor.Enter(_objLock);
        }
        /// <summary>
        /// Unlocks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void UnLock()
        {
            Monitor.Exit(_objLock);
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return GetApplicationStateData().GetEnumerator();
        }

        #endregion
    }
}
