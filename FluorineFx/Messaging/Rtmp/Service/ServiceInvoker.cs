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
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using log4net;
using log4net.Config;
using FluorineFx.Invocation;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Exceptions;

namespace FluorineFx.Messaging.Rtmp.Service
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ServiceInvoker : IServiceInvoker
	{
		public static string SERVICE_NAME = "serviceInvoker";
        static ILog log = LogManager.GetLogger(typeof(ServiceInvoker));
        /// <summary>
        /// Service resolvers.
        /// </summary>
		private ICollection<IServiceResolver> _serviceResolvers;

		public ServiceInvoker()
		{
		}

        //TODO
        public void SetServiceResolvers(ICollection<IServiceResolver> resolvers) 
		{
			_serviceResolvers = resolvers;
		}
		/// <summary>
		/// Lookup a handler for the passed service name in the given scope.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="serviceName"></param>
		/// <returns></returns>
		private object GetServiceHandler(IScope scope, string serviceName) 
		{
			// Get application scope handler first
			object service = scope.Handler;
			if(serviceName == null || serviceName == string.Empty) 
			{
				// No service requested, return application scope handler
                if (log.IsDebugEnabled)
                {
                    if( service != null )
                        log.Debug(__Res.GetString(__Res.ServiceInvoker_Resolve, "[scope handler]", service.GetType().FullName, scope.Name));
                }
                if (log.IsErrorEnabled)
                {
                    if (service == null)
                        log.Error(__Res.GetString(__Res.ServiceInvoker_ResolveFail, "[scope handler]", scope.Name));
                }
                return service;
			}
			// Search service resolver that knows about service name
			if( _serviceResolvers != null )
			{
				foreach(IServiceResolver resolver in _serviceResolvers) 
				{
					service = resolver.ResolveService(scope, serviceName);
                    if (service != null)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(__Res.GetString(__Res.ServiceInvoker_Resolve, serviceName, service.GetType().FullName, scope.Name));
                        return service;
                    }
				}
			}
			// Requested service does not exist.
            if (log.IsErrorEnabled)
                log.Error(__Res.GetString(__Res.ServiceInvoker_ResolveFail, serviceName, scope.Name));
			return null;
		}

		#region IServiceInvoker Members

		public bool Invoke(IServiceCall call, IScope scope)
		{
			string serviceName = call.ServiceName;
			object service = GetServiceHandler(scope, serviceName);

			if (service == null) 
			{
				call.Exception = new ServiceNotFoundException(serviceName);
				call.Status = Call.STATUS_SERVICE_NOT_FOUND;
				return false;
			} 
			return Invoke(call, service);
		}

		public bool Invoke(IServiceCall call, object service)
		{
            string serviceMethod = call.ServiceMethodName;
            object[] arguments = call.Arguments;

            // First, search for method with exact parameter type match
            MethodInfo mi = MethodHandler.GetMethod(service.GetType(), serviceMethod, arguments, true, false, false);
            if (mi == null)
            {
                // Second, search for method with type conversions
                // This second call will trace 'suitable method' errors too
                mi = MethodHandler.GetMethod(service.GetType(), serviceMethod, arguments, false, false, true);
                if (mi == null)
                {
                    string msg = __Res.GetString(__Res.Invocation_NoSuitableMethod, serviceMethod);
                    call.Status = Call.STATUS_METHOD_NOT_FOUND;
                    call.Exception = new FluorineException(msg);
                    return false;
                }
            }

            try
            {
                ParameterInfo[] parameterInfos = mi.GetParameters();
                object[] parameters = new object[parameterInfos.Length];
                arguments.CopyTo(parameters, 0);
                TypeHelper.NarrowValues(parameters, parameterInfos);

                object result = null;
                if (mi.ReturnType == typeof(void))
                {
                    InvocationHandler invocationHandler = new InvocationHandler(mi);
                    invocationHandler.Invoke(service, parameters);
                    call.Status = Call.STATUS_SUCCESS_VOID;
                }
                else
                {
                    InvocationHandler invocationHandler = new InvocationHandler(mi);
                    result = invocationHandler.Invoke(service, parameters);
                    call.Status = result == null ? Call.STATUS_SUCCESS_NULL : Call.STATUS_SUCCESS_RESULT;
                }
                if (call is IPendingServiceCall)
                    (call as IPendingServiceCall).Result = result;
            }
            catch (SecurityException exception)
            {
                call.Exception = exception;
                call.Status = Call.STATUS_ACCESS_DENIED;
                if (log.IsDebugEnabled)
                    log.Debug(exception.Message);
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                call.Exception = exception;
                call.Status = Call.STATUS_ACCESS_DENIED;
                if (log.IsDebugEnabled)
                    log.Debug(exception.Message);
                return false;
            }
            catch (TargetInvocationException exception)
            {
                call.Exception = exception.InnerException;
                call.Status = Call.STATUS_INVOCATION_EXCEPTION;
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Invocation_Failed, mi.Name, exception.InnerException.Message));
                return false;
            }
            catch (Exception exception)
            {
                call.Exception = exception;
                call.Status = Call.STATUS_GENERAL_EXCEPTION;
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Invocation_Failed, mi.Name, exception.Message));
                return false;
            }
            return true;
		}

		#endregion
	}
}
