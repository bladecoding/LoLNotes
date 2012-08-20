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
using System.Text;
using System.Security;
using System.Security.Permissions;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Exceptions;

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public static class MethodHandler
	{
#if !SILVERLIGHT
        private static readonly ILog Log = LogManager.GetLogger(typeof(MethodHandler));
#endif

	    /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
		public static MethodInfo GetMethod(Type type, string methodName, IList arguments)
		{
			return GetMethod(type, methodName, arguments, false);
		}

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <param name="exactMatch"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(Type type, string methodName, IList arguments, bool exactMatch)
        {
            return GetMethod(type, methodName, arguments, exactMatch, true);
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <param name="exactMatch"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(Type type, string methodName, IList arguments, bool exactMatch, bool throwException)
        {
            return GetMethod(type, methodName, arguments, exactMatch, throwException, true);
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <param name="exactMatch"></param>
        /// <param name="throwException"></param>
        /// <param name="traceError"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(Type type, string methodName, IList arguments, bool exactMatch, bool throwException, bool traceError)
		{
			MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
            List<MethodInfo> suitableMethodInfos = new List<MethodInfo>();
            for (int i = 0; i < methodInfos.Length; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                if (methodInfo.Name == methodName)
                {
                    if ((methodInfo.GetParameters().Length == 0 && arguments == null)
                        || (arguments != null && methodInfo.GetParameters().Length == arguments.Count))
                        suitableMethodInfos.Add(methodInfo);
                }
            }
            if (suitableMethodInfos.Count > 0)
            {
                //Overloaded methods may suffer performance penalties because of type conversion checking
                for (int i = suitableMethodInfos.Count-1; i >= 0; i--)
                {
                    MethodInfo methodInfo = suitableMethodInfos[i];
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    bool match = true;
                    //Matching method name and parameters number
                    for (int j = 0; j < parameterInfos.Length; j++)
                    {
                        ParameterInfo parameterInfo = parameterInfos[j];
                        if (!exactMatch)
                        {
                            if (!TypeHelper.IsAssignable(arguments[j], parameterInfo.ParameterType))
                            {
                                match = false;
                                break;
                            }
                        }
                        else
                        {
                            if (arguments[j] == null || arguments[j].GetType() != parameterInfo.ParameterType)
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                    if (!match)
                        suitableMethodInfos.Remove(methodInfo);
                }
            }
			if( suitableMethodInfos.Count == 0 )
			{
                string msg = __Res.GetString(__Res.Invocation_NoSuitableMethod, methodName);
                if (traceError)
                {
#if !SILVERLIGHT
                    if (Log.IsErrorEnabled)
                    {
                        //Trace the issue
                        Log.Error(msg);
                        Log.Error("Displaying verbose logging information");
                        try
                        {
                            new FileIOPermission(PermissionState.Unrestricted);
                            Log.Error(string.Format("Reflected type was '{0}' location of the loaded file {1}", type.AssemblyQualifiedName, type.Assembly.Location));
                        }
                        catch (SecurityException)
                        {
                            Log.Error(string.Format("Reflected type was '{0}' location of the loaded file cannot be determined", type.AssemblyQualifiedName));
                        }
                        List<MethodInfo> suitableMethodInfosTmp = new List<MethodInfo>();
                        for (int i = 0; i < methodInfos.Length; i++)
                        {
                            MethodInfo methodInfo = methodInfos[i];
                            if (methodInfo.Name == methodName)
                            {
                                if ((methodInfo.GetParameters().Length == 0 && arguments == null)
                                    || (arguments != null && methodInfo.GetParameters().Length == arguments.Count))
                                {
                                    suitableMethodInfosTmp.Add(methodInfo);
                                }
                            }
                        }
                        for (int i = suitableMethodInfosTmp.Count - 1; i >= 0; i--)
                        {
                            MethodInfo methodInfo = suitableMethodInfosTmp[i];
                            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                            StringBuilder signature = new StringBuilder();
                            for (int j = 0; j < parameterInfos.Length; j++)
                            {
                                if (signature.Length != 0)
                                    signature.Append(", ");
                                signature.Append(parameterInfos[j].Name);
                                signature.Append("(");
                                signature.Append(parameterInfos[j].ParameterType.Name);
                                signature.Append(")");
                            }
                            Log.Error(string.Format("Checking {0}({1})", methodInfo.Name, signature));
                            bool match = true;
                            //Matching method name and parameters number
                            for (int j = 0; j < parameterInfos.Length; j++)
                            {
                                ParameterInfo parameterInfo = parameterInfos[j];
                                object arg = arguments[j];
                                if (!exactMatch)
                                {
                                    if (!TypeHelper.IsAssignable(arg, parameterInfo.ParameterType))
                                    {
                                        if (arg != null)
                                            Log.Error(string.Format("{0}({1}) did not match value \"{2}\" ({3})", parameterInfo.Name, parameterInfo.ParameterType.Name, arg, arg.GetType().Name));
                                        else
                                            Log.Error(string.Format("{0}({1}) did not match null)", parameterInfo.Name, parameterInfo.ParameterType.Name));
                                        match = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (arg == null || arg.GetType() != parameterInfo.ParameterType)
                                    {
                                        if (arg != null)
                                            Log.Error(string.Format("{0}({1}) did not match value \"{2}\" ({3})", parameterInfo.Name, parameterInfo.ParameterType.Name, arg, arg.GetType().Name));
                                        else
                                            Log.Error(string.Format("{0}({1}) did not match null)", parameterInfo.Name, parameterInfo.ParameterType.Name));
                                        match = false;
                                        break;
                                    }
                                }
                            }
                            if (!match)
                                suitableMethodInfosTmp.Remove(methodInfo);
                        }
                        /*
                        for (int j = 0; arguments != null && j < arguments.Count; j++)
                        {
                            object arg = arguments[j];
                            string trace;
                            if (arg != null)
                                trace = __Res.GetString(__Res.Invocation_ParameterType, j, arg.GetType().FullName);
                            else
                                trace = __Res.GetString(__Res.Invocation_ParameterType, j, "null");
                            log.Error(trace);
                        }
                        */
                    }
#endif
                }
				if( throwException )
					throw new FluorineException(msg);
			}
			if( suitableMethodInfos.Count > 1 )
			{
                string msg = __Res.GetString(__Res.Invocation_Ambiguity, methodName);
				if( throwException )
					throw new FluorineException(msg);
			}
            if (suitableMethodInfos.Count > 0)
                return suitableMethodInfos[0];
            return null;
		}
	}
}
