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
using System.Data.SqlTypes;
using System.Collections;

using log4net;
using FluorineFx.Exceptions;

namespace FluorineFx.IO.Writers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF3SqlTypesWriter : IAMFWriter
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(AMF3SqlTypesWriter));

		public AMF3SqlTypesWriter()
		{
		}

		#region IAMFWriter Members

		public bool IsPrimitive{ get{ return true; } }

		public void WriteData(AMFWriter writer, object data)
		{
			if( data is INullable )
			{
				if( (data as INullable).IsNull )
				{
					writer.WriteAMF3Null();
					return;
				}
			}
			if( data is SqlByte )
			{
				writer.WriteAMF3Data(((SqlByte)data).Value );
				return;
			}
			if( data is SqlInt16 )
			{
				writer.WriteAMF3Data(((SqlInt16)data).Value );
				return;
			}
			if( data is SqlInt32 )
			{
				writer.WriteAMF3Data(((SqlInt32)data).Value );
				return;
			}
			if( data is SqlInt64 )
			{
				writer.WriteAMF3Data(((SqlInt64)data).Value );
				return;
			}
			if( data is SqlSingle )
			{
				writer.WriteAMF3Data(((SqlSingle)data).Value );
				return;
			}
			if( data is SqlDouble )
			{
				writer.WriteAMF3Data(((SqlDouble)data ).Value);
				return;
			}
			if( data is SqlDecimal )
			{
				writer.WriteAMF3Data(((SqlDecimal)data).Value );
				return;
			}
			if( data is SqlMoney )
			{
				writer.WriteAMF3Data(((SqlMoney)data).Value );
				return;
			}
			if( data is SqlDateTime )
			{
				writer.WriteAMF3Data(((SqlDateTime)data).Value);
				return;
			}
			if( data is SqlString )
			{
				writer.WriteAMF3String(((SqlString)data).Value);
				return;
			}
			if( data is SqlGuid )
			{
				writer.WriteAMF3Data(((SqlGuid)data).Value.ToString("D"));
				return;
			}
			if( data is SqlBoolean )
			{
				writer.WriteAMF3Bool(((SqlBoolean)data).Value);
				return;
			}
			string msg = string.Format("Could not find serializer for type {0}", data.GetType().FullName);
			if (_log.IsErrorEnabled)
				_log.Error(msg);
			throw new FluorineException(msg);
		}

		#endregion
	}
}
