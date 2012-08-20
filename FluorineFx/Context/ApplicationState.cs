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
using System.Web.Caching;
using FluorineFx.Collections;

namespace FluorineFx.Context
{
    class ApplicationState : IApplicationState
    {
        IResource _resource;

        public ApplicationState()
        {
            _resource = new FileSystemResource("~/Web.config");
            if (_resource == null || !_resource.Exists)
                _resource = new FileSystemResource("~/web.config");
        }

        #region IApplicationState Members

        public object this[string name]
        {
            get
            {
                return HttpRuntime.Cache[name];
            }
            set
            {
                Add(name, value);
            }
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public void Add(string name, object value)
        {
            //Cache can be cleared by touching Web.config
            if (_resource != null && _resource.Exists)
            {
                CacheDependency cacheDependency = new CacheDependency(_resource.File.FullName);
                HttpRuntime.Cache.Insert(name, value, cacheDependency, DateTime.Now.AddYears(1), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
                return;
            }
        }

        /// <summary>
        /// Locks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void Lock()
        {
            //System.Web.Caching.Cache is thread-safe
        }
        /// <summary>
        /// Unlocks access to an IApplicationState variable to facilitate access synchronization.
        /// </summary>
        public void UnLock()
        {
            //System.Web.Caching.Cache is thread-safe
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return HttpRuntime.Cache.GetEnumerator();
        }

        #endregion
    }
}
