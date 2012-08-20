//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Web;
using System.Collections;
using FluorineFx.Util;
using FluorineFx.Json.Services;
using FluorineFx.Messaging;
using FluorineFx.Context;
using FluorineFx.Configuration;
using log4net;

namespace FluorineFx.Json.Rpc
{
    sealed class JsonRpcProxyGenerator : JsonRpcServiceFeature
    {
        private DateTime _lastModifiedTime  = DateTime.MinValue;
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonRpcProxyGenerator));
        private Hashtable _generators;

        public JsonRpcProxyGenerator(MessageBroker messageBroker)
            : base(messageBroker)
        {
            _generators = new Hashtable();
            _generators.Add("default", new DefaultJsonRpcProxyGenerator());
            try
            {
                JsonRpcGeneratorCollection generatorSettings = FluorineConfiguration.Instance.FluorineSettings.JSonSettings.JsonRpcGenerators;
                for (int i = 0; i < generatorSettings.Count; i++)
                {
                    JsonRpcGenerator generatorSetting = generatorSettings[i];
                    IJsonRpcProxyGenerator generator = ObjectFactory.CreateInstance(generatorSetting.Type) as IJsonRpcProxyGenerator;
                    if (generator != null)
                    {
                        _generators.Add(generatorSetting.Name, generator);
                    }
                    else
                    {
                        if (log.IsErrorEnabled)
                            log.Error(string.Format("JsonRpcGenerator {0} was not found", generatorSetting.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Creating JsonRpcProxyGenerator failed", ex);
            }
        }

        protected override void ProcessRequest()
        {
            Destination destination = null;
            string source = this.Request.QueryString["source"];
            if (!StringUtils.IsNullOrEmpty(source))
            {
                string destinationId = this.MessageBroker.GetDestinationId(source);
                destination = this.MessageBroker.GetDestination(destinationId);
            }
            else
            {
                string destinationId = this.Request.QueryString["destination"];
                if (!StringUtils.IsNullOrEmpty(destinationId))
                {
                    destination = this.MessageBroker.GetDestination(destinationId);
                    source = destination.Source;
                }
            }
            FactoryInstance factoryInstance = destination.GetFactoryInstance();
            factoryInstance.Source = source;
            Type type = factoryInstance.GetInstanceClass();
            if (type != null)
            {
                ServiceClass serviceClass = JsonRpcServiceReflector.FromType(type);
                UpdateLastModifiedTime(type);
                if (!Modified())
                {
                    this.Response.StatusCode = 304;
                    return;
                }

                if (_lastModifiedTime != DateTime.MinValue)
                {
                    this.Response.Cache.SetCacheability(HttpCacheability.Public);
                    this.Response.Cache.SetLastModified(_lastModifiedTime);
                }

                Response.ContentType = "text/javascript";

                string clientFileName = serviceClass.Name + "Proxy.js";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + clientFileName);
                //WriteProxy(serviceClass, new IndentedTextWriter(Response.Output));

                string generatorName = "default";
                if (!StringUtils.IsNullOrEmpty(this.Request.QueryString["generator"]))
                    generatorName = this.Request.QueryString["generator"];
                if (_generators.Contains(generatorName))
                {
                    IJsonRpcProxyGenerator generator = _generators[generatorName] as IJsonRpcProxyGenerator;
                    generator.WriteProxy(serviceClass, new IndentedTextWriter(Response.Output), this.Request);
                }
                else
                {
                    if (log.IsErrorEnabled)
                        log.Error(string.Format("JsonRpcGenerator {0} was not found", generatorName));
                }
            }       
        }

        private bool Modified()
        {
            if (_lastModifiedTime == DateTime.MinValue)
                return true;

            string modifiedSinceHeader = StringUtils.MaskNullString(Request.Headers["If-Modified-Since"]);

            //
            // Apparently, Netscape added a non-standard extension to the
            // If-Modified-Since header in HTTP/1.0 where extra parameters
            // can be sent using a semi-colon as the delimiter. One such 
            // parameter is the original content length, which was meant 
            // to improve the accuracy of If-Modified-Since in case a 
            // document is updated twice in the same second. Here's an
            // example: 
            //
            // If-Modified-Since: Thu, 11 May 2006 07:59:51 GMT; length=3419
            //
            // HTTP/1.1 solved the same problem in a better way via the ETag 
            // header and If-None-Match. However, it looks like that some
            // proxies still use this technique, so the following checks for
            // a semi-colon in the header value and clips it to everything
            // before it.
            //
            // This is a fix for bug #7462:
            // http://developer.berlios.de/bugs/?func=detailbug&bug_id=7462&group_id=4638
            //

            int paramsIndex = modifiedSinceHeader.IndexOf(';');
            if (paramsIndex >= 0)
                modifiedSinceHeader = modifiedSinceHeader.Substring(0, paramsIndex);

            if (modifiedSinceHeader.Length == 0)
                return true;

            DateTime modifiedSinceTime;

            try
            {
                modifiedSinceTime = DateTimeUtils.ParseInternetDate(modifiedSinceHeader);
            }
            catch (FormatException)
            {
                //
                // Accorinding to the HTTP specification, if the passed 
                // If-Modified-Since date is invalid, the response is 
                // exactly the same as for a normal GET. See section
                // 14.25 of RFC 2616 for more information:
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.25
                //

                return true;
            }

            DateTime time = _lastModifiedTime;
            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            if (time > modifiedSinceTime)
                return true;

            return false;
        }

        private DateTime UpdateLastModifiedTime(Type type)
        {
            if (_lastModifiedTime == DateTime.MinValue)
            {
                _lastModifiedTime = DateTime.Now;
                // The last modified time is determined by taking the
                // last modified time of the physical file (for example,
                // a DLL) representing the type's assembly.
                try
                {
                    Uri codeBase = new Uri(type.Assembly.CodeBase);

                    if (codeBase != null && codeBase.IsFile)
                    {
                        string path = codeBase.LocalPath;

                        if (File.Exists(path))
                        {
                            try
                            {
                                _lastModifiedTime = File.GetLastWriteTime(path);
                            }
                            catch (UnauthorizedAccessException) { /* ignored */ }
                            catch (IOException) { /* ignored */ }
                        }
                    }
                }
                catch (SecurityException)
                {
                    //
                    // This clause ignores security exceptions that may
                    // be caused by an application that is partially
                    // trusted and therefore would not be allowed to
                    // disover the service assembly code base as well
                    // as the physical file's modification time.
                    //
                }
            }
            return _lastModifiedTime;
        }
    }
}
