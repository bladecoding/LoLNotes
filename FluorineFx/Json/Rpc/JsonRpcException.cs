//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using SerializationInfo = System.Runtime.Serialization.SerializationInfo;
using StreamingContext = System.Runtime.Serialization.StreamingContext;
using FluorineFx.Util;

namespace FluorineFx.Json.Rpc
{
    [Serializable]
    class JsonRpcException : ApplicationException
    {
        private const string _defaultMessage = "The JSON-RPC request could not be completed due to an error.";

        public JsonRpcException() : this((string)null) { }

        public JsonRpcException(Exception innerException)
            :
            base(_defaultMessage, innerException) { }

        public JsonRpcException(string message)
            :
            base(StringUtils.MaskNullString(message, _defaultMessage)) { }

        public JsonRpcException(string message, Exception innerException)
            :
            base(StringUtils.MaskNullString(message, _defaultMessage), innerException) { }

        protected JsonRpcException(SerializationInfo info, StreamingContext context)
            :
            base(info, context) { }
    }
}
