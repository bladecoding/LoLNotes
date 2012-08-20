//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using FluorineFx.Util;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Json.Services
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class Method
    {
        private string _name;
        private Type _resultType;
        private Parameter[] _parameters;
        private string[] _parameterNames;
        private Parameter[] _sortedParameters;
        private string _description;

        public static Method FromMethodInfo(MethodInfo methodInfo)
        {
            Method method = new Method();
            method._name = methodInfo.Name;
            method._resultType = methodInfo.ReturnType;
            method._description = null;

            method._parameters = new Parameter[methodInfo.GetParameters().Length];
            method._parameterNames = new string[methodInfo.GetParameters().Length];
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                Parameter parameter = Parameter.FromParameterInfo(parameterInfo);
                int position = parameter.Position;
                method._parameters[position] = parameter;
                method._parameterNames[position] = parameter.Name;
            }
            // Keep a sorted list of parameters and their names so we can do fast look ups using binary search.
            method._sortedParameters = (Parameter[])method._parameters.Clone();
            InvariantStringArray.Sort(method._parameterNames, method._sortedParameters);

            return method;
        }

        internal Method()
        {
        }

        public string Name
        {
            get { return _name; }
        }

        public Parameter[] GetParameters()
        {
            return _parameters;
        }

        public Type ResultType
        {
            get { return _resultType; }
        }
        
        public string Description
        {
            get { return _description; }
        }

        /*
        public object Invoke(object service, string[] names, object[] args)
        {
            if (names != null)
                args = MapArguments(names, args);
            
            args = TransposeVariableArguments(args);
            return _handler.Invoke(service, args);
        }
        */

        /// <summary>
        /// Determines if the method accepts variable number of arguments or
        /// not. A method is designated as accepting variable arguments by
        /// annotating the last parameter of the method with the JsonRpcParams
        /// attribute.
        /// </summary>

        public bool HasParamArray
        {
            get
            {
                return _parameters.Length > 0 && 
                    _parameters[_parameters.Length - 1].IsParamArray;
            }
        }

        private object[] MapArguments(string[] names, object[] args)
        {
            Debug.Assert(names != null);
            Debug.Assert(args != null);
            Debug.Assert(names.Length == args.Length);
            
            object[] mapped = new object[_parameters.Length];

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                
                if (name == null || name.Length == 0)
                    continue;
                
                object arg = args[i];
                
                if (arg == null)
                    continue;
                
                int position = -1;
                
                if (name.Length <= 2)
                {
                    char ch1;
                    char ch2;

                    if (name.Length == 2)
                    {
                        ch1 = name[0];
                        ch2 = name[1];
                    }
                    else
                    {
                        ch1 = '0';
                        ch2 = name[0];
                    }

                    if (ch1 >= '0' && ch1 <= '9' &&
                        ch2 >= '0' && ch2 <= '9')
                    {
                        position = int.Parse(name, NumberStyles.Number, CultureInfo.InvariantCulture);
                    
                        if (position < _parameters.Length)
                            mapped[position] = arg;
                    }
                }
                
                if (position < 0)
                {
                    int order = InvariantStringArray.BinarySearch(_parameterNames, name);
                    if (order >= 0)
                        position = _sortedParameters[order].Position;
                }
                
                if (position >= 0)
                    mapped[position] = arg;
            }

            return mapped;
        }

        /// <summary>
        /// Takes an array of arguments that are designated for a method and
        /// transposes them if the target method supports variable arguments (in
        /// other words, the last parameter is annotated with the JsonRpcParams
        /// attribute). If the method does not support variable arguments then
        /// the input array is returned verbatim. 
        /// </summary>

        // TODO: Allow args to be null to represent empty arguments.
        // TODO: Allow parameter conversions

        public object[] TransposeVariableArguments(object[] args)
        {
            //
            // If the method does not have take variable arguments then just
            // return the arguments array verbatim.
            //

            if (!HasParamArray)
                return args;

            int parameterCount = _parameters.Length;

            object[] varArgs = null;
            
            //
            // The variable argument may already be setup correctly as an
            // array. If so then the formal and actual parameter count will
            // match here.
            //
            
            if (args.Length == parameterCount)
            {
                object lastArg = args[args.Length - 1];

                if (lastArg != null)
                {
                    //
                    // Is the last argument already set up as an object 
                    // array ready to be received as the variable arguments?
                    //
                    
                    varArgs = lastArg as object[];
                    
                    if (varArgs == null)
                    {
                        //
                        // Is the last argument an array of some sort? If so 
                        // then we convert it into an array of objects since 
                        // that is what we support right now for variable 
                        // arguments.
                        //
                        // TODO: Allow variable arguments to be more specific type, such as array of integers.
                        // TODO: Don't make a copy if one doesn't have to be made. 
                        // For example if the types are compatible on the receiving end.
                        //
                        
                        Array lastArrayArg = lastArg as Array;
                        
                        if (lastArrayArg != null && lastArrayArg.GetType().GetArrayRank() == 1)
                        {
                            varArgs = new object[lastArrayArg.Length];
                            Array.Copy(lastArrayArg, varArgs, varArgs.Length);
                        }
                    }
                }
            }

            //
            // Copy out the extra arguments into a new array that represents
            // the variable parts.
            //

            if (varArgs == null)
            {
                varArgs = new object[(args.Length - parameterCount) + 1];
                Array.Copy(args, parameterCount - 1, varArgs, 0, varArgs.Length);
            }

            //
            // Setup a new array of arguments that has a copy of the fixed
            // arguments followed by the variable arguments array setup above.
            //

            object[] transposedArgs = new object[parameterCount];
            Array.Copy(args, transposedArgs, parameterCount - 1);
            transposedArgs[transposedArgs.Length - 1] = varArgs;
            return transposedArgs;
        }

        private static bool TypesMatch(Type expected, Type actual)
        {
            Debug.Assert(expected != null);
            Debug.Assert(actual != null);
            
            //
            // If the expected type is sealed then use a quick check by
            // comparing types for equality. Otherwise, use the slow
            // approach to determine type compatibility be their
            // relationship.
            //
            
            return expected.IsSealed ? 
                expected.Equals(actual) : 
                expected.IsAssignableFrom(actual);
        }
    }
}

