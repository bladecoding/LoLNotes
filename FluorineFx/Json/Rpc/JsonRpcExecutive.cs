//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Collections;
using System.IO;
using System.Web;
using log4net;
using FluorineFx.Json.Services;
using FluorineFx.Util;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Api;
using FluorineFx.Context;

namespace FluorineFx.Json.Rpc
{
    sealed class JsonRpcExecutive : JsonRpcServiceFeature
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonRpcExecutive));

        public JsonRpcExecutive(MessageBroker messageBroker)
            : base(messageBroker)
        {
        }

        protected override void ProcessRequest()
        {
            if (!StringUtils.CaselessEquals(Request.RequestType, "POST"))
            {
                throw new JsonRpcException(string.Format("HTTP {0} is not supported for RPC execution. Use HTTP POST only.", Request.RequestType));
            }

            // Sets the "Cache-Control" header value to "no-cache".
            // NOTE: It does not send the common HTTP 1.0 request directive
            // "Pragma" with the value "no-cache".
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            // Response will be plain text, though it would have been nice to 
            // be more specific, like text/json.
            Response.ContentType = "text/plain";

            // Dispatch
            using (TextReader reader = GetRequestReader())
                ProcessRequest(reader, Response.Output);
        }

        private TextReader GetRequestReader()
        {
            if (StringUtils.CaselessEquals(Request.ContentType, "application/x-www-form-urlencoded"))
            {
                string request = Request.Form.Count == 1 ? Request.Form[0] : Request.Form["JSON-RPC"];
                return new StringReader(request);
            }
            else
            {
                return new StreamReader(Request.InputStream, Request.ContentEncoding);
            }
        }

        private void ProcessRequest(TextReader input, TextWriter output)
        {
            ValidationUtils.ArgumentNotNull(input, "input");
            ValidationUtils.ArgumentNotNull(output, "output");

            IDictionary response;

            try
            {
                IDictionary request = JavaScriptConvert.DeserializeObject(input) as IDictionary;
                response = Invoke(request);
            }
            catch (MissingMethodException e)
            {
                response = CreateResponse(null, null, FromException(e));
            }
            catch (JsonReaderException e)
            {
                response = CreateResponse(null, null, FromException(e));
            }
            catch (JsonWriterException e)
            {
                response = CreateResponse(null, null, FromException(e));
            }

            string result = JavaScriptConvert.SerializeObject(response);
            output.Write(result);
        }

        private IDictionary Invoke(IDictionary request)
        {
            ValidationUtils.ArgumentNotNull(request, "request");
            object error = null;
            object result = null;

            ISession session = this.MessageBroker.SessionManager.GetHttpSession(HttpContext.Current);
            FluorineContext.Current.SetSession(session);
            //Context initialized, notify listeners.
            if (session != null && session.IsNew)
                session.NotifyCreated();

            // Get the ID of the request.
            object id = request["id"];
            string credentials = request["credentials"] as string;
            if (!StringUtils.IsNullOrEmpty(credentials))
            {
                try
                {
                    CommandMessage commandMessage = new CommandMessage(CommandMessage.LoginOperation);
                    commandMessage.body = credentials;
                    IMessage message = this.MessageBroker.RouteMessage(commandMessage);
                    if (message is ErrorMessage)
                    {
                        error = FromException(message as ErrorMessage);
                        return CreateResponse(id, result, error);
                    }
                }
                catch (Exception ex)
                {
                    error = FromException(ex);
                    return CreateResponse(id, result, error);
                }
            }

            // If the ID is not there or was not set then this is a notification
            // request from the client that does not expect any response. Right
            // now, we don't support this.
            bool isNotification = JavaScriptConvert.IsNull(id);

            if (isNotification)
                throw new NotSupportedException("Notification are not yet supported.");

            log.Debug(string.Format("Received request with the ID {0}.", id.ToString()));

            // Get the method name and arguments.
            string methodName = StringUtils.MaskNullString((string)request["method"]);

            if (methodName.Length == 0)
                throw new JsonRpcException("No method name supplied for this request.");

            if (methodName == "clearCredentials")
            {
                try
                {
                    CommandMessage commandMessage = new CommandMessage(CommandMessage.LogoutOperation);
                    IMessage message = this.MessageBroker.RouteMessage(commandMessage);
                    if (message is ErrorMessage)
                    {
                        error = FromException(message as ErrorMessage);
                        return CreateResponse(id, result, error);
                    }
                    else
                        return CreateResponse(id, message.body, null);
                }
                catch (Exception ex)
                {
                    error = FromException(ex);
                    return CreateResponse(id, result, error);
                }
            }

            //Info("Invoking method {1} on service {0}.", ServiceName, methodName);

            // Invoke the method on the service and handle errors.
            try
            {
                RemotingMessage message = new RemotingMessage();
                message.destination = this.Request.QueryString["destination"] as string;
                message.source = this.Request.QueryString["source"] as string;
                message.operation = methodName;
                object argsObject = request["params"];
                object[] args = (argsObject as JavaScriptArray).ToArray();
                message.body = args;
                IMessage response = this.MessageBroker.RouteMessage(message);
                if (response is ErrorMessage)
                    error = FromException(response as ErrorMessage);
                else
                    result = response.body;

                /*
                Method method = serviceClass.GetMethodByName(methodName);

                object[] args;
                string[] names = null;

                object argsObject = request["params"];
                IDictionary argByName = argsObject as IDictionary;

                if (argByName != null)
                {
                    names = new string[argByName.Count];
                    argByName.Keys.CopyTo(names, 0);
                    args = new object[argByName.Count];
                    argByName.Values.CopyTo(args, 0);
                }
                else
                {
                    args = (argsObject as JavaScriptArray).ToArray();
                }

                result = method.Invoke(instance, names, args);
                 */
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw;
            }

            // Setup and return the response object.
            return CreateResponse(id, result, error);
        }

        private object FromException(Exception ex)
        {
            return JsonRpcError.FromException(ex, false);
        }

        private object FromException(ErrorMessage message)
        {
            return JsonRpcError.FromException(message, false);
        }

        private static IDictionary CreateResponse(object id, object result, object error)
        {
            JavaScriptObject response = new JavaScriptObject();
            response["id"] = id;
            if (error != null)
                response["error"] = error;
            else
                response["result"] = result;
            return response;
        }
    }
}
