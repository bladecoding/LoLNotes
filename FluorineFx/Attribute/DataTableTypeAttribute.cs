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
using System.Reflection;
using FluorineFx.Invocation;

namespace FluorineFx
{
    /// <summary>
    /// 	<para>The DataTableTypeAttribute specifies the type of data in a DataTable.</para>
    /// 	<para>Rows will be serialized as a collection of strongly typed ASObjects (columns
    ///     as properties).</para>
    /// </summary>
    /// <example>
    /// 	<code lang="CS">
    /// In this sample the Flex client will receive an Arraycollection of PhoneVO objects.
    /// [DataTableType("FlexRemoteObjectSample.PhoneVO")]
    /// public DataTable GetDataTable()
    /// {
    ///     DataSet dataSet = new DataSet("mydataset");
    ///     DataTable dataTable = dataSet.Tables.Add("phones");
    ///     dataTable.Columns.Add( "number", typeof(string) );
    ///     dataTable.Rows.Add( new object[] {"123456"} );
    ///     dataTable.Rows.Add( new object[] {"456789"} );
    ///     return dataTable;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DataTableTypeAttribute : System.Attribute, IInvocationCallback
	{
		string	_remoteClass;
		string	_tableName;
		string	_propertyName;

        /// <summary>
        /// Initializes a new instance of the DataTableTypeAttribute class.
        /// </summary>
        /// <param name="remoteClass">The ActionScript3 class name.</param>
		public DataTableTypeAttribute(string remoteClass)
		{
			_remoteClass = remoteClass;
		}
        /// <summary>
        /// Initializes a new instance of the DataTableTypeAttribute class. See also <see cref="FluorineFx.DataSetTypeAttribute"/> 
        /// </summary>
        /// <param name="tableName">Refers to DataTable from a DataSet.</param>
        /// <param name="remoteClass">The ActionScript3 class name.</param>
		public DataTableTypeAttribute(string tableName, string remoteClass)
		{
			_remoteClass = remoteClass;
			_tableName = tableName;
		}
        /// <summary>
        /// Initializes a new instance of the DataTableTypeAttribute class. See also <see cref="FluorineFx.DataSetTypeAttribute"/> 
        /// </summary>
        /// <param name="tableName">Refers to DataTable from a DataSet.</param>
        /// <param name="propertyName">The parent object's property name.</param>
        /// <param name="remoteClass">The ActionScript3 class name.</param>
        public DataTableTypeAttribute(string tableName, string propertyName, string remoteClass)
		{
			_remoteClass = remoteClass;
			_tableName = tableName;
			_propertyName = propertyName;
		}
        /// <summary>Gets the ActionScript 3 class name.</summary>
		public string RemoteClass{ get{ return _remoteClass; } }

		#region IInvocationCallback Members
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="invocationManager"></param>
        /// <param name="memberInfo"></param>
        /// <param name="obj"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        public void OnInvoked(IInvocationManager invocationManager, MemberInfo memberInfo, object obj, object[] arguments, object result)
		{
			if( result is DataSet )
			{
				if( _tableName != null )
				{
					DataSet dataSet = result as DataSet;
					if( dataSet.Tables.Contains(_tableName) )
					{
						DataTable dataTable = dataSet.Tables[_tableName];
						if( _propertyName != null )
							dataTable.ExtendedProperties.Add("alias", _propertyName);
						ArrayList list = ConvertDataTable(dataTable);
						invocationManager.Properties[dataTable] = list;
					}
				}
			}
			if( result is DataTable )
			{
				DataTable dataTable = result as DataTable;
				ArrayList list = ConvertDataTable(dataTable);
				invocationManager.Result = list;
			}
		}

		#endregion

		private ArrayList ConvertDataTable(DataTable dataTable)
		{
			ArrayList result = new ArrayList(dataTable.Rows.Count);
			for(int i = 0; i < dataTable.Rows.Count; i++)
			{
				DataRow dataRow = dataTable.Rows[i];
				ASObject aso = new ASObject(_remoteClass);
				for(int j = 0; j < dataTable.Columns.Count; j++)
				{
					DataColumn column = dataTable.Columns[j];
					/*
					object value = null;
					if( !dataRow.IsNull(column) )
						value = dataRow[column];
					*/
					aso.Add(column.ColumnName, dataRow[column]);
				}
				result.Add(aso);
			}
			return result;
		}
	}
}
