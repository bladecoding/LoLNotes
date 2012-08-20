using System;
using System.Data;
using System.Reflection;
using FluorineFx.Invocation;
using FluorineFx.Util;
using FluorineFx.Json.Rpc;
using FluorineFx.Json.Services;

namespace FluorineFx.Json
{
    /// <summary>
    /// Indicates whether an operation (method) can be invoked using Json RPC.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class JsonRpcMethodAttribute : Attribute, IInvocationResultHandler
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the JsonRpcMethodAttribute class.
        /// </summary>
        public JsonRpcMethodAttribute() { }
        /// <summary>
        /// Initializes a new instance of the JsonRpcMethodAttribute class.
        /// </summary>
        /// <param name="name"></param>
        public JsonRpcMethodAttribute(string name)
        {
            _name = name;
        }
        /// <summary>
        /// Get or sets the attribute name.
        /// </summary>
        public string Name
        {
            get { return StringUtils.MaskNullString(_name); }
            set { _name = value; }
        }


        #region IInvocationResultHandler Members

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="invocationManager"></param>
        /// <param name="memberInfo"></param>
        /// <param name="obj"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        public void HandleResult(IInvocationManager invocationManager, MemberInfo memberInfo, object obj, object[] arguments, object result)
        {
            if (invocationManager.Result is DataSet)
            {
                DataSet dataSet = result as DataSet;
                invocationManager.Result = TypeHelper.ConvertDataSetToASO(dataSet, false);
            }
            if (invocationManager.Result is DataTable)
            {
                DataTable dataTable = result as DataTable;
                invocationManager.Result = TypeHelper.ConvertDataTableToASO(dataTable, false);
            }
        }

        #endregion
    }
}
