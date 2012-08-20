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
using System.Web;
using System.Collections.Specialized;
using System.Security;
// Import log4net classes.
using log4net;

using FluorineFx;
using FluorineFx.Invocation;
using FluorineFx.Configuration;
using FluorineFx.HttpCompress;

namespace FluorineFx.SWX
{
    class SwxHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SwxHandler));

        public void Handle(HttpApplication httpApplication)
        {
            object result = null;
            string url = null;
            bool debug = false;
            CompressionLevels compressionLevel = CompressionLevels.None;
			if( FluorineConfiguration.Instance.HttpCompressSettings.HandleRequest == HandleRequest.Swx || 
				FluorineConfiguration.Instance.HttpCompressSettings.HandleRequest == HandleRequest.All )
				compressionLevel = FluorineConfiguration.Instance.HttpCompressSettings.CompressionLevel;
            bool allowDomain = FluorineConfiguration.Instance.SwxSettings.AllowDomain;
            try
            {
                NameValueCollection parameters;
                if (httpApplication.Request.RequestType == "GET")
                    parameters = httpApplication.Request.QueryString;
                else
                    parameters = httpApplication.Request.Form;

                string serviceClass = parameters["serviceClass"];
                string operation = parameters["method"];
                string args = parameters["args"];
                url = parameters["url"];
                debug = parameters["debug"] != null ? FluorineFx.Util.Convert.ToBoolean(parameters["debug"]) : false;

				if (url != null)
				{
					url = HttpUtility.UrlDecode(url);
					// Firefox/Flash (at least, and tested only on a Mac), sends 
					// file:/// (three slashses) in the URI and that fails the validation
					// so replacing that with two slashes instead.
					url = url.Replace("///", "//");

					try
					{
						UriBuilder uriBuilder = new UriBuilder(url);
					}
					catch (UriFormatException)
					{
						if (log.IsWarnEnabled)
						{
							string msg = __Res.GetString(__Res.Swx_InvalidCrossDomainUrl, url);
							log.Warn(msg);
						}
						url = null;
					}
				}
				else
				{
					if (allowDomain && log.IsWarnEnabled)
					{
						string msg = "No referring URL received from Flash. Cross-domain will not be supported on this call regardless of allowDomain setting";
						log.Warn(msg);
					}
				}
                // If the user did not pass an args array, treat it as
                // an empty args array. (Although this may be an error
                // on the client side, it may also be the user calling
                // a method that doesn't take arguments and we shouldn't
                // force the user to create an args parameter with an empty
                // array.)
                if (args == "undefined" || args == string.Empty)
                    args = "[]";
                // Massage special characters back
                args = args.Replace("\\t", "\t");
                args = args.Replace("\\n", "\n");
                args = args.Replace("\\'", "'");

                FluorineFx.Json.JavaScriptArray argsArray = FluorineFx.Json.JavaScriptConvert.DeserializeObject(args) as FluorineFx.Json.JavaScriptArray;
                object[] arguments = argsArray.ToArray();

                object instance = ObjectFactory.CreateInstance(serviceClass);
                MethodInfo mi = MethodHandler.GetMethod(instance.GetType(), operation, arguments);
                ParameterInfo[] parameterInfos = mi.GetParameters();
                TypeHelper.NarrowValues(arguments, parameterInfos);
                InvocationHandler invocationHandler = new InvocationHandler(mi);
                result = invocationHandler.Invoke(instance, arguments);
            }
            catch (TargetInvocationException exception)
            {
                Hashtable resultObj = new Hashtable();
                resultObj["error"] = true;
                resultObj["code"] = "SERVER.PROCESSING";
                resultObj["message"] = exception.InnerException.Message;
                result = resultObj;
            }
            catch (Exception exception)
            {
                Hashtable resultObj = new Hashtable();
                resultObj["error"] = true;
                resultObj["code"] = "SERVER.PROCESSING";
                resultObj["message"] = exception.Message;
                result = resultObj;
            }

            SwxAssembler assembler = new SwxAssembler();
            byte[] buffer = assembler.WriteSwf(result, debug, compressionLevel, url, allowDomain);
            httpApplication.Response.Clear();
            httpApplication.Response.ClearHeaders();
            httpApplication.Response.Buffer = true;
            httpApplication.Response.ContentType = "application/swf";
            httpApplication.Response.AppendHeader("Content-Length", buffer.Length.ToString());
            httpApplication.Response.AppendHeader("Content-Disposition", "attachment; filename=data.swf");
            if (buffer.Length > 0)
                httpApplication.Response.OutputStream.Write(buffer, 0, buffer.Length);
            try
            {
                httpApplication.Response.Flush();
            }
            catch (SecurityException)
            {
            }
        }
    }
}
