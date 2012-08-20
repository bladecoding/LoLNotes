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
using FluorineFx.Collections;
using System.Web;
using System.Web.Caching;
using log4net;
#if !(NET_1_1)
using System.Collections.Generic;
#endif

namespace FluorineFx.Configuration
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    sealed class CacheMap
    {
        class CacheDescriptor
        {
            string _name;
            int _timeout;
            bool _slidingExpiration;

            public CacheDescriptor(string name, int timeout, bool slidingExpiration)
            {
                _name = name;
                _timeout = timeout;
                _slidingExpiration = slidingExpiration;
            }

            public string Name
            {
                get { return _name; }
            }

            public int Timeout
            {
                get { return _timeout; }
            }

            public bool SlidingExpiration
            {
                get { return _slidingExpiration; }
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CacheMap));
#if (NET_1_1)
        private Hashtable _cacheMap = new Hashtable();
#else
        private Dictionary<string, CacheDescriptor> _cacheMap = new Dictionary<string,CacheDescriptor>();
#endif
        public CacheMap()
        {
        }

        public void AddCacheDescriptor(string name, int timeout, bool slidingExpiration)
        {
            CacheDescriptor cacheDescriptor = new CacheDescriptor(name, timeout, slidingExpiration);
            lock (((ICollection)_cacheMap).SyncRoot)
            {
                _cacheMap[name] = cacheDescriptor;
            }
            if (log != null && log.IsDebugEnabled)
            {
                string msg = "Loading key: " + name + " to cache map, timeout: " + timeout + " sliding expiration: " + slidingExpiration;
                log.Debug(msg);
            }
        }

        public bool ContainsCacheDescriptor(string source)
        {
            if (source != null)
            {
                lock (((ICollection)_cacheMap).SyncRoot)
                {
                    return _cacheMap.ContainsKey(source);
                }
            }
            return false;
        }

        public int Count
        {
            get 
            {
                lock (((ICollection)_cacheMap).SyncRoot)
                {
                    return _cacheMap.Count;
                }
            }
        }

        public bool ContainsValue(string cacheKey)
        {
            return HttpRuntime.Cache.Get(cacheKey) != null;
        }

        public object Get(string cacheKey)
        {
            object value = HttpRuntime.Cache.Get(cacheKey);
            if (value != null)
            {
                if (log != null && log.IsDebugEnabled)
                {
                    string msg = "Cache hit, name: " + cacheKey;
                    log.Debug(msg);
                }
            }
            else
            {
                if (log != null && log.IsDebugEnabled)
                {
                    string msg = "Cache miss, name: " + cacheKey;
                    log.Debug(msg);
                }
            }
            return value;
        }

        public object Add(string source, string cacheKey, object value)
        {
            lock (((ICollection)_cacheMap).SyncRoot)
            {
                if (_cacheMap.ContainsKey(source))
                {
                    if (log != null && log.IsDebugEnabled)
                    {
                        string msg = "Add to ASP.NET cache name: " + source + " key: " + cacheKey;
                        log.Debug(msg);
                    }

                    CacheDescriptor cacheDescriptor = _cacheMap[source] as CacheDescriptor;
                    if (!cacheDescriptor.SlidingExpiration)
                        return HttpRuntime.Cache.Add(cacheKey, value, null, DateTime.Now.AddMinutes(cacheDescriptor.Timeout), TimeSpan.Zero, CacheItemPriority.Default, null);
                    else
                        return HttpRuntime.Cache.Add(cacheKey, value, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(cacheDescriptor.Timeout), CacheItemPriority.Default, null);
                }
            }
            if (log != null && log.IsDebugEnabled)
            {
                string msg = "Cannot add to ASP.NET cache the name: " + source + " key: " + cacheKey + ". Check your web.config file.";
                log.Debug(msg);
            }
            return null;
        }

        internal static string GenerateCacheKey(string source, IList arguments)
        {
            int argumentsHashCode = FluorineFx.Data.ListHashCodeProvider.GenerateHashCode(arguments);
            string key = source + "_" + argumentsHashCode.ToString();
            return key;
        }
    }
}
