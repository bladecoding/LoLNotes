//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using log4net;
using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Json.Services;

namespace FluorineFx.Json.Rpc
{
    internal sealed class JsonRpcServiceReflector
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonRpcServiceReflector));
        private static readonly CopyOnWriteDictionary _classByTypeCache = new CopyOnWriteDictionary();

        public static ServiceClass FromType(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, "type");

            ServiceClass serviceClass = (ServiceClass)_classByTypeCache[type];
            if (serviceClass == null)
            {
                serviceClass = BuildFromType(type);
                _classByTypeCache[type] = serviceClass;
            }
            return serviceClass;
        }

        private static ServiceClass BuildFromType(Type type)
        {
            bool isAccessible = TypeHelper.GetTypeIsAccessible(type);
            if (isAccessible)
            {
                ServiceClass serviceClass = new ServiceClass(type);
                return serviceClass;
            }
            else
            {
                string msg = __Res.GetString(__Res.Type_InitError, type.FullName);
                if (log.IsErrorEnabled)
                    log.Error(msg);
                throw new TypeLoadException(msg);
            }
        }

        private JsonRpcServiceReflector()
        {
            throw new NotSupportedException();
        }
    }
}
