//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Diagnostics;
using System.Reflection;

namespace FluorineFx.Json.Services
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class Parameter
    {
        private string _name;
        private Type _parameterType;
        private int _position;
        private bool _isParamArray;

        public static Parameter FromParameterInfo(ParameterInfo parameterInfo)
        {
            Parameter parameter = new Parameter();
            parameter._name = parameterInfo.Name;
            parameter._position = parameterInfo.Position;
            parameter._parameterType = parameterInfo.ParameterType;
            parameter._isParamArray = parameterInfo.IsDefined(typeof(ParamArrayAttribute), true);
            return parameter;
        }

        internal Parameter()
        {
        }
        
        public string Name
        {
            get { return _name; }
        }

        public Type ParameterType
        {
            get { return _parameterType; }
        }

        public int Position
        {
            get { return _position; }
        }

        public bool IsParamArray
        {
            get { return _isParamArray; }
        }
    }
}

