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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using System.ComponentModel;
using FluorineFx.AMF3;
using FluorineFx.Util;

namespace FluorineFx.IO.Writers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF3ObjectWriter : IAMFWriter
	{
		public AMF3ObjectWriter()
		{
		}
		#region IAMFWriter Members

		public bool IsPrimitive{ get{return false;} }

		public void WriteData(AMFWriter writer, object data)
		{
            if (data is ArrayCollection)
            {
                writer.WriteByte(AMF3TypeCode.Object);
                writer.WriteAMF3Object(data);
                return;
            }
            //TODO: configure somehow Vector support
            /*
            if (CollectionUtils.IsGenericListType(data.GetType()))
            {
                Type itemType = ReflectionUtils.GetListItemType(data.GetType());
                switch (Type.GetTypeCode(itemType))
                {
                    case TypeCode.Int32:
                        writer.WriteByte(AMF3TypeCode.IntVector);
                        writer.WriteAMF3IntVector(data as IList<int>);
                        return;
                    case TypeCode.UInt32:
                        writer.WriteByte(AMF3TypeCode.UIntVector);
                        writer.WriteAMF3UIntVector(data as IList<uint>);
                        return;
                    case TypeCode.Double:
                        writer.WriteByte(AMF3TypeCode.NumberVector);
                        writer.WriteAMF3DoubleVector(data as IList<double>);
                        return;
                    case TypeCode.String:
                        writer.WriteByte(AMF3TypeCode.ObjectVector);
                        writer.WriteAMF3ObjectVector(data as IList<string>);
                        return;
                    case TypeCode.Boolean:
                        writer.WriteByte(AMF3TypeCode.ObjectVector);
                        writer.WriteAMF3ObjectVector(data as IList<bool>);
                        return;
                    default:
                        writer.WriteByte(AMF3TypeCode.ObjectVector);
                        writer.WriteAMF3ObjectVector(data as IList);
                        return;
                }
            }
            */
            IList list = data as IList;
            if (list != null )
			{
				//http://livedocs.macromedia.com/flex/2/docs/wwhelp/wwhimpl/common/html/wwhelp.htm?context=LiveDocs_Parts&file=00001104.html#270405
				//http://livedocs.macromedia.com/flex/2/docs/wwhelp/wwhimpl/common/html/wwhelp.htm?context=LiveDocs_Parts&file=00001105.html#268711
				if( writer.UseLegacyCollection )
				{
					writer.WriteByte(AMF3TypeCode.Array);
                    writer.WriteAMF3Array(list);
				}
				else
				{
					writer.WriteByte(AMF3TypeCode.Object);
                    object value = new ArrayCollection(list);
					writer.WriteAMF3Object(value);
				}
				return;
			}
#if !(SILVERLIGHT)
            IListSource listSource = data as IListSource;
            if (listSource != null)
            {
                if (writer.UseLegacyCollection)
                {
                    writer.WriteByte(AMF3TypeCode.Array);
                    writer.WriteAMF3Array(listSource.GetList());
                }
                else
                {
                    writer.WriteByte(AMF3TypeCode.Object);
                    object value = new ArrayCollection(listSource.GetList());
                    writer.WriteAMF3Object(value);
                }
                return;
            }
#endif
            IDictionary dictionary = data as IDictionary;
            if (dictionary != null)
			{
				//writer.WriteByte(AMF3TypeCode.Object);
				//writer.WriteAMF3Object(data);
                writer.WriteByte(AMF3TypeCode.Array);
                writer.WriteAMF3AssociativeArray(dictionary);
				return;
			}
			if(data is Exception)
			{
				writer.WriteByte(AMF3TypeCode.Object);
                if (writer.UseLegacyThrowable)
				    writer.WriteAMF3Object(new ExceptionASO(data as Exception) );
                else
                    writer.WriteAMF3Object(data);
				return;
			}
			if( data is IExternalizable )
			{
				writer.WriteByte(AMF3TypeCode.Object);
				writer.WriteAMF3Object(data);
				return;
			}
            if (data is IEnumerable)
			{
                IEnumerator enumerator = (data as IEnumerable).GetEnumerator();
#if !(NET_1_1)
                List<object> tmp = new List<object>();
#else
                ArrayList tmp = new ArrayList();
#endif
                foreach (object element in (data as IEnumerable))
                {
                    tmp.Add(element);
                }
                if (writer.UseLegacyCollection)
                {
                    writer.WriteByte(AMF3TypeCode.Array);
                    writer.WriteAMF3Array(tmp);
                }
                else
                {
                    writer.WriteByte(AMF3TypeCode.Object);
                    object value = new ArrayCollection(tmp);
                    writer.WriteAMF3Object(value);
                }
                return;
			}
			writer.WriteByte(AMF3TypeCode.Object);
			writer.WriteAMF3Object(data);
		}

		#endregion
	}
}
