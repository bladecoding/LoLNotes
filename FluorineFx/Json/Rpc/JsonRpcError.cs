//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Diagnostics;
using FluorineFx.Util;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Json.Rpc
{
    class JsonRpcError
    {
        public static JavaScriptObject FromException(Exception e)
        {
            return FromException(e, false);
        }

        public static JavaScriptObject FromException(Exception e, bool includeStackTrace)
        {
            ValidationUtils.ArgumentNotNull(e, "e");

            JavaScriptObject error = new JavaScriptObject();
            error.Add("name", "JSONRPCError");
            error.Add("message", e.GetBaseException().Message);

            if (includeStackTrace)
                error.Add("stackTrace", e.StackTrace);

            JavaScriptArray errors = new JavaScriptArray();

            do
            {
                errors.Add(ToLocalError(e));
                e = e.InnerException;
            }
            while (e != null);

            error.Add("errors", errors);

            return error;
        }

        public static JavaScriptObject FromException(ErrorMessage message, bool includeStackTrace)
        {
            ValidationUtils.ArgumentNotNull(message, "message");

            JavaScriptObject error = new JavaScriptObject();
            error.Add("name", "JSONRPCError");
            error.Add("message", message.faultString);
            error.Add("code", message.faultCode);

            if (includeStackTrace)
                error.Add("stackTrace", message.faultDetail);

            error.Add("errors", null);
            return error;
        }

        private static JavaScriptObject ToLocalError(Exception e)
        {
            Debug.Assert(e != null);

            JavaScriptObject error = new JavaScriptObject();
            error.Add("name", e.GetType().Name);
            error.Add("message", e.Message);
            return error;
        }

        private JsonRpcError()
        {
            throw new NotSupportedException();
        }
    }
}
