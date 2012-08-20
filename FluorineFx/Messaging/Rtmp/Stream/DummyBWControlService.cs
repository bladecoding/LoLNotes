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
using FluorineFx.Configuration;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// A dummy bandwidth control service (bandwidth controller) that always has token available.
    /// </summary>
    class DummyBWControlService : IBWControlService
    {
        private ITokenBucket _dummyBucket = new DummyTokenBukcet();
        //Map(IBWControllable, IBWControlContext)
        private Hashtable _contextMap = new Hashtable();

        #region IService Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IBWControlService Members

        public IBWControlContext RegisterBWControllable(IBWControllable bc)
        {
            lock (_contextMap.SyncRoot)
            {
                if (!_contextMap.Contains(bc))
                {
                    DummyBWContext context = new DummyBWContext(bc);
                    _contextMap.Add(bc, context);
                }
                return _contextMap[bc] as IBWControlContext;
            }
        }

        public void UnregisterBWControllable(IBWControlContext context)
        {
            lock (_contextMap.SyncRoot)
            {
                _contextMap.Remove(context.GetBWControllable());
            }
        }

        public IBWControlContext LookupContext(IBWControllable bc)
        {
            lock (_contextMap.SyncRoot)
            {
                return _contextMap[bc] as IBWControlContext;
            }
        }

        public void UpdateBWConfigure(IBWControlContext context)
        {
            // do nothing
        }

        public void ResetBuckets(IBWControlContext context)
        {
            // do nothing
        }

        public ITokenBucket GetAudioBucket(IBWControlContext context)
        {
            return _dummyBucket;
        }

        public ITokenBucket GetVideoBucket(IBWControlContext context)
        {
            return _dummyBucket;
        }

        public ITokenBucket GetDataBucket(IBWControlContext context)
        {
            return _dummyBucket;
        }

        #endregion
    }

	class DummyTokenBukcet : ITokenBucket 
    {
        #region ITokenBucket Members

        public bool AcquireToken(long tokenCount, long wait)
        {
            return true;
        }

        public bool AcquireTokenNonblocking(long tokenCount, ITokenBucketCallback callback)
        {
            return true;
        }

        public long AcquireTokenBestEffort(long upperLimitCount)
        {
            return upperLimitCount;
        }

        public long Capacity
        {
            get { return 0; }
        }

        public double Speed
        {
            get { return 0; }
        }

        public void Reset()
        {            
        }

        #endregion
    }

	class DummyBWContext : IBWControlContext 
    {
        private IBWControllable _controllable;

        public DummyBWContext(IBWControllable controllable)
        {
            _controllable = controllable;
        }

        #region IBWControlContext Members

        public IBWControllable GetBWControllable()
        {
            return _controllable;
        }

        #endregion
    }
}
