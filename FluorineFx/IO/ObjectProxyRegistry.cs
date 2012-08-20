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
using System.Reflection;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.AMF3;
using FluorineFx.Configuration;
using FluorineFx.Util;
using FluorineFx.Exceptions;

namespace FluorineFx.IO
{
    sealed class ObjectProxyRegistry
    {
        static object _objLock = new object();
        /// <summary>
        /// Volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed.
        /// </summary>
        static volatile ObjectProxyRegistry _instance;
        static IObjectProxy _defaultObjectProxy;

#if !(NET_1_1)
        Dictionary<Type, IObjectProxy> _registeredProxies;
#else
        Hashtable	_registeredProxies;
#endif

        private ObjectProxyRegistry()
		{
		}

        static public ObjectProxyRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_objLock)
                    {
                        if (_instance == null)
                        {
#if WCF
                            _defaultObjectProxy = new WcfProxy();
#else
                            _defaultObjectProxy = new ObjectProxy();
#endif
                            ObjectProxyRegistry instance = new ObjectProxyRegistry();
                            instance.Init();
                            _instance = instance;
                        }
                    }
                }
                return _instance;
            }
        }

        private void Init()
        {
#if !(NET_1_1)
            _registeredProxies = new Dictionary<Type, IObjectProxy>();
#else
            _registeredProxies = new Hashtable();
#endif

            _registeredProxies.Add(typeof(ASObject), new ASObjectProxy());
            _registeredProxies.Add(typeof(IExternalizable), new ExternalizableProxy());
            _registeredProxies.Add(typeof(Exception), new ExceptionProxy());
            //_registeredProxies.Add(typeof(System.Data.Objects.DataClasses.EntityObject), new EntityObjectProxy());
#if !(NET_1_1) && !(NET_2_0) && !(SILVERLIGHT)
			_registeredProxies.Add(typeof(System.Data.Objects.DataClasses.StructuralObject), new EntityObjectProxy());
#endif
        }

        public IObjectProxy GetObjectProxy(Type type)
        {
            if( type.GetInterface(typeof(IExternalizable).FullName, true) != null )
                return _registeredProxies[typeof(IExternalizable)] as IObjectProxy;
            if (type.GetInterface("INHibernateProxy", false) != null)
            {
                //TODO
                //Quick fix for INHibernateProxy
                type = type.BaseType;
            }
            if (_registeredProxies.ContainsKey(type))
                return _registeredProxies[type] as IObjectProxy;
            foreach(DictionaryEntry entry in (IDictionary)_registeredProxies)
            {
                if (type.IsSubclassOf(entry.Key as Type))
                    return entry.Value as IObjectProxy;
            }
            return _defaultObjectProxy;
        }
    }
}
