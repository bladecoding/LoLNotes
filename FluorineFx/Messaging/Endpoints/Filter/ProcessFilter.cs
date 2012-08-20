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
using System.Data;
using System.Collections;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using System.Reflection;
using log4net;
using FluorineFx.IO;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;
using FluorineFx.Diagnostic;
using FluorineFx.Security;
using FluorineFx.Invocation;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ProcessFilter : AbstractFilter
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(ProcessFilter));
		EndpointBase _endpoint;

		/// <summary>
		/// Initializes a new instance of the ProcessFilter class.
		/// </summary>
		public ProcessFilter(EndpointBase endpoint)
		{
			_endpoint = endpoint;
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			MessageOutput messageOutput = context.MessageOutput;
			for(int i = 0; i < context.AMFMessage.BodyCount; i++)
			{
				AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
				ResponseBody responseBody = null;
				//Check for Flex2 messages and skip
				if( amfBody.IsEmptyTarget )
					continue;

				if( amfBody.IsDebug )
					continue;
				if( amfBody.IsDescribeService )
				{
					responseBody = new ResponseBody();
					responseBody.IgnoreResults = amfBody.IgnoreResults;
                    responseBody.Target = amfBody.Response + AMFBody.OnResult;
					responseBody.Response = null;
					DescribeService describeService = new DescribeService( amfBody );
					responseBody.Content = describeService.GetDescription();
					messageOutput.AddBody(responseBody);
					continue;
				}

				//Check if response exists.
				responseBody = messageOutput.GetResponse(amfBody);
				if( responseBody != null )
				{
					continue;
				}

				try
				{
				    MessageBroker messageBroker = _endpoint.GetMessageBroker();
                    RemotingService remotingService = messageBroker.GetService(RemotingService.RemotingServiceId) as RemotingService;
                    if (remotingService == null)
                    {
                        string serviceNotFound = __Res.GetString(__Res.Service_NotFound, RemotingService.RemotingServiceId);
                        responseBody = new ErrorResponseBody(amfBody, new FluorineException(serviceNotFound));
                        messageOutput.AddBody(responseBody);
                        if (log.IsErrorEnabled)
                            log.Error(serviceNotFound);
                        continue;
                    }
                    Destination destination = null;
                    if (destination == null)
                        destination = remotingService.GetDestinationWithSource(amfBody.TypeName);
                    if (destination == null)
                        destination = remotingService.DefaultDestination;
                    //At this moment we got a destination with the exact source or we have a default destination with the "*" source.
                    if (destination == null)
                    {
                        string destinationNotFound = __Res.GetString(__Res.Destination_NotFound, amfBody.TypeName);
                        responseBody = new ErrorResponseBody(amfBody, new FluorineException(destinationNotFound));
                        messageOutput.AddBody(responseBody);
                        if (log.IsErrorEnabled)
                            log.Error(destinationNotFound);
                        continue;
                    }

                    try
                    {
                        remotingService.CheckSecurity(destination);
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        responseBody = new ErrorResponseBody(amfBody, exception);
                        if (log.IsDebugEnabled)
                            log.Debug(exception.Message);
                        continue;
                    }

                    //Cache check
                    string source = amfBody.TypeName + "." + amfBody.Method;
                    IList parameterList = amfBody.GetParameterList();
                    string key = FluorineFx.Configuration.CacheMap.GenerateCacheKey(source, parameterList);
                    if (FluorineConfiguration.Instance.CacheMap.ContainsValue(key))
                    {
                        object result = FluorineFx.Configuration.FluorineConfiguration.Instance.CacheMap.Get(key);
                        if (result != null)
                        {
                            if (log != null && log.IsDebugEnabled)
                                log.Debug(__Res.GetString(__Res.Cache_HitKey, source, key));
                            responseBody = new ResponseBody(amfBody, result);
                            messageOutput.AddBody(responseBody);
                            continue;
                        }
                    }

				    FactoryInstance factoryInstance = destination.GetFactoryInstance();
				    factoryInstance.Source = amfBody.TypeName;
				    if( FluorineContext.Current.ActivationMode != null )//query string can override the activation mode
					    factoryInstance.Scope = FluorineContext.Current.ActivationMode;
				    object instance = factoryInstance.Lookup();

				    if( instance != null )
                    {
                        try
                        {
                            bool isAccessible = TypeHelper.GetTypeIsAccessible(instance.GetType());
                            if (!isAccessible)
                            {
                                string msg = __Res.GetString(__Res.Type_InitError, amfBody.TypeName);
                                if (log.IsErrorEnabled)
                                    log.Error(msg);
                                responseBody = new ErrorResponseBody(amfBody, new FluorineException(msg));
                                messageOutput.AddBody(responseBody);
                                continue;
                            }

                            MethodInfo mi = null;
                            if (!amfBody.IsRecordsetDelivery)
                            {
                                mi = MethodHandler.GetMethod(instance.GetType(), amfBody.Method, amfBody.GetParameterList());
                            }
                            else
                            {
                                //will receive recordsetid only (ignore)
                                mi = instance.GetType().GetMethod(amfBody.Method);
                            }
                            if (mi != null)
                            {
                                object[] roleAttributes = mi.GetCustomAttributes(typeof(RoleAttribute), true);
                                if (roleAttributes != null && roleAttributes.Length == 1)
                                {
                                    RoleAttribute roleAttribute = roleAttributes[0] as RoleAttribute;
                                    string[] roles = roleAttribute.Roles.Split(',');

                                    bool authorized = messageBroker.LoginManager.DoAuthorization(roles);
                                    if (!authorized)
                                        throw new UnauthorizedAccessException(__Res.GetString(__Res.Security_AccessNotAllowed));
                                }

                                #region Invocation handling
                                PageSizeAttribute pageSizeAttribute = null;
                                MethodInfo miCounter = null;
                                object[] pageSizeAttributes = mi.GetCustomAttributes(typeof(PageSizeAttribute), true);
                                if (pageSizeAttributes != null && pageSizeAttributes.Length == 1)
                                {
                                    pageSizeAttribute = pageSizeAttributes[0] as PageSizeAttribute;
                                    miCounter = instance.GetType().GetMethod(amfBody.Method + "Count");
                                    if (miCounter != null && miCounter.ReturnType != typeof(System.Int32))
                                        miCounter = null; //check signature
                                }
                                ParameterInfo[] parameterInfos = mi.GetParameters();
                                //Try to handle missing/optional parameters.
                                object[] args = new object[parameterInfos.Length];
                                if (!amfBody.IsRecordsetDelivery)
                                {
                                    if (args.Length != parameterList.Count)
                                    {
                                        string msg = __Res.GetString(__Res.Arg_Mismatch, parameterList.Count, mi.Name, args.Length);
                                        if (log != null && log.IsErrorEnabled)
                                            log.Error(msg);
                                        responseBody = new ErrorResponseBody(amfBody, new ArgumentException(msg));
                                        messageOutput.AddBody(responseBody);
                                        continue;
                                    }
                                    parameterList.CopyTo(args, 0);
                                    if (pageSizeAttribute != null)
                                    {
                                        PagingContext pagingContext = new PagingContext(pageSizeAttribute.Offset, pageSizeAttribute.Limit);
                                        PagingContext.SetPagingContext(pagingContext);
                                    }
                                }
                                else
                                {
                                    if (amfBody.Target.EndsWith(".release"))
                                    {
                                        responseBody = new ResponseBody(amfBody, null);
                                        messageOutput.AddBody(responseBody);
                                        continue;
                                    }
                                    string recordsetId = parameterList[0] as string;
                                    string recordetDeliveryParameters = amfBody.GetRecordsetArgs();
                                    byte[] buffer = System.Convert.FromBase64String(recordetDeliveryParameters);
                                    recordetDeliveryParameters = System.Text.Encoding.UTF8.GetString(buffer);
                                    if (recordetDeliveryParameters != null && recordetDeliveryParameters != string.Empty)
                                    {
                                        string[] stringParameters = recordetDeliveryParameters.Split(new char[] { ',' });
                                        for (int j = 0; j < stringParameters.Length; j++)
                                        {
                                            if (stringParameters[j] == string.Empty)
                                                args[j] = null;
                                            else
                                                args[j] = stringParameters[j];
                                        }
                                        //TypeHelper.NarrowValues(argsStore, parameterInfos);
                                    }
                                    PagingContext pagingContext = new PagingContext(System.Convert.ToInt32(parameterList[1]), System.Convert.ToInt32(parameterList[2]));
                                    PagingContext.SetPagingContext(pagingContext);
                                }

                                TypeHelper.NarrowValues(args, parameterInfos);

                                try
                                {
                                    InvocationHandler invocationHandler = new InvocationHandler(mi);
                                    object result = invocationHandler.Invoke(instance, args);

                                    if (FluorineConfiguration.Instance.CacheMap != null && FluorineConfiguration.Instance.CacheMap.ContainsCacheDescriptor(source))
                                    {
                                        //The result should be cached
                                        CacheableObject cacheableObject = new CacheableObject(source, key, result);
                                        FluorineConfiguration.Instance.CacheMap.Add(cacheableObject.Source, cacheableObject.CacheKey, cacheableObject);
                                        result = cacheableObject;
                                    }
                                    responseBody = new ResponseBody(amfBody, result);

                                    if (pageSizeAttribute != null)
                                    {
                                        int totalCount = 0;
                                        string recordsetId = null;

                                        IList list = amfBody.GetParameterList();
                                        string recordetDeliveryParameters = null;
                                        if (!amfBody.IsRecordsetDelivery)
                                        {
                                            //fist call paging
                                            object[] argsStore = new object[list.Count];
                                            list.CopyTo(argsStore, 0);
                                            recordsetId = System.Guid.NewGuid().ToString();
                                            if (miCounter != null)
                                            {
                                                //object[] counterArgs = new object[0];
                                                totalCount = (int)miCounter.Invoke(instance, args);
                                            }
                                            string[] stringParameters = new string[argsStore.Length];
                                            for (int j = 0; j < argsStore.Length; j++)
                                            {
                                                if (argsStore[j] != null)
                                                    stringParameters[j] = argsStore[j].ToString();
                                                else
                                                    stringParameters[j] = string.Empty;
                                            }
                                            recordetDeliveryParameters = string.Join(",", stringParameters);
                                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(recordetDeliveryParameters);
                                            recordetDeliveryParameters = System.Convert.ToBase64String(buffer);
                                        }
                                        else
                                        {
                                            recordsetId = amfBody.GetParameterList()[0] as string;
                                        }
                                        if (result is DataTable)
                                        {
                                            DataTable dataTable = result as DataTable;
                                            dataTable.ExtendedProperties["TotalCount"] = totalCount;
                                            dataTable.ExtendedProperties["Service"] = recordetDeliveryParameters + "/" + amfBody.Target;
                                            dataTable.ExtendedProperties["RecordsetId"] = recordsetId;
                                            if (amfBody.IsRecordsetDelivery)
                                            {
                                                dataTable.ExtendedProperties["Cursor"] = Convert.ToInt32(list[list.Count - 2]);
                                                dataTable.ExtendedProperties["DynamicPage"] = true;
                                            }
                                        }
                                    }
                                }
                                catch (UnauthorizedAccessException exception)
                                {
                                    responseBody = new ErrorResponseBody(amfBody, exception);
                                    if (log.IsDebugEnabled)
                                        log.Debug(exception.Message);
                                }
                                catch (Exception exception)
                                {
                                    if (exception is TargetInvocationException && exception.InnerException != null)
                                        responseBody = new ErrorResponseBody(amfBody, exception.InnerException);
                                    else
                                        responseBody = new ErrorResponseBody(amfBody, exception);
                                    if (log.IsDebugEnabled)
                                        log.Debug(__Res.GetString(__Res.Invocation_Failed, mi.Name, exception.Message));
                                }
                                #endregion Invocation handling
                            }
                            else
                                responseBody = new ErrorResponseBody(amfBody, new MissingMethodException(amfBody.TypeName, amfBody.Method));
                        }
                        finally
                        {
                            factoryInstance.OnOperationComplete(instance);
                        }
                    }
                    else
                        responseBody = new ErrorResponseBody(amfBody, new TypeInitializationException(amfBody.TypeName, null));
                }
			    catch(Exception exception)
			    {
					if( log != null && log.IsErrorEnabled )
                        log.Error(exception.Message, exception);
				    responseBody = new ErrorResponseBody(amfBody, exception);
			    }
				messageOutput.AddBody(responseBody);
			}
		}
		#endregion

	}
}
