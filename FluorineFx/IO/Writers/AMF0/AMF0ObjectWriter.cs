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
using System.Collections.Generic;
using System.ComponentModel;

namespace FluorineFx.IO.Writers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF0ObjectWriter : IAMFWriter
	{
		public AMF0ObjectWriter()
		{
		}
		#region IAMFWriter Members

		public bool IsPrimitive{ get{return false;} }

		public void WriteData(AMFWriter writer, object data)
		{
			if( data is IList )
			{
				IList list = data as IList;
				object[] array = new object[list.Count];
				list.CopyTo(array, 0);
				writer.WriteArray(ObjectEncoding.AMF0, array);
				return;
			}
#if !(SILVERLIGHT)
            IListSource listSource = data as IListSource;
            if (listSource != null)
            {
                IList list = listSource.GetList();
                object[] array = new object[list.Count];
                list.CopyTo(array, 0);
                writer.WriteArray(ObjectEncoding.AMF0, array);
                return;
            }
#endif
			if(data is IDictionary)
			{
				writer.WriteAssociativeArray(ObjectEncoding.AMF0, data as IDictionary);
				return;
			}
			if(data is Exception)
			{
				writer.WriteASO(ObjectEncoding.AMF0, new ExceptionASO(data as Exception) );
				return;
			}
            if (data is IEnumerable)
            {
                List<object> tmp = new List<object>();
                foreach (object element in (data as IEnumerable))
                {
                    tmp.Add(element);
                }
                writer.WriteArray(ObjectEncoding.AMF0, tmp.ToArray());
                return;
            }
			writer.WriteObject(ObjectEncoding.AMF0, data);
		}

		#endregion
	}
}
