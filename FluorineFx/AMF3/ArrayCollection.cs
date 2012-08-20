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
using System.ComponentModel;
using System.Reflection;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx;
using FluorineFx.Util;

namespace FluorineFx.AMF3
{
	/// <summary>
	/// Provides a type converter to convert ArrayCollection objects to and from various other representations.
	/// </summary>
#if !SILVERLIGHT
    public class ArrayCollectionConverter : ArrayConverter
#else
    public class ArrayCollectionConverter : TypeConverter
#endif
	{
		/// <summary>
		/// Overloaded. Returns whether this converter can convert the object to the specified type.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="destinationType">A Type that represents the type you want to convert to.</param>
		/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if( destinationType == typeof(ArrayCollection) )
				return true;
			if( destinationType.IsArray )
				return true;
#if !SILVERLIGHT
			if( destinationType == typeof(ArrayList) )
				return true;
#endif
			if( destinationType == typeof(IList) )
				return true;
			Type typeIList = destinationType.GetInterface("System.Collections.IList", false);
			if(typeIList != null)
				return true;
			//generic interface
			Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1", false);
			if (typeGenericICollection != null)
				return true;
#if !SILVERLIGHT
			return base.CanConvertTo(context, destinationType);
#else
            return base.CanConvertTo(destinationType);
#endif
		}
		/// <summary>
		/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="culture">A CultureInfo object. If a null reference (Nothing in Visual Basic) is passed, the current culture is assumed.</param>
		/// <param name="value">The Object to convert.</param>
		/// <param name="destinationType">The Type to convert the value parameter to.</param>
		/// <returns>An Object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
            ArrayCollection ac = value as ArrayCollection;
            ValidationUtils.ArgumentNotNull(ac, "value");
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if( destinationType == typeof(ArrayCollection) )
				return value;
			if( destinationType.IsArray )
			{
				return ac.ToArray();
			}
#if !SILVERLIGHT
            if( destinationType == typeof(ArrayList) )
			{
                if (ac.List is ArrayList)
                    return ac.List;
                return ArrayList.Adapter(ac.List);
			}
#endif
			if( destinationType == typeof(IList) )
			{
				return ac.List;
			}
			//generic interface
			Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1", false);
			if (typeGenericICollection != null)
			{
				object obj = TypeHelper.CreateInstance(destinationType);
                MethodInfo miAddCollection = typeGenericICollection.GetMethod("Add");
				if (miAddCollection != null)
				{
					ParameterInfo[] parameters = miAddCollection.GetParameters();
					if(parameters != null && parameters.Length == 1)
					{
						Type parameterType = parameters[0].ParameterType;
						IList list = (IList) value;
						for (int i = 0; i < list.Count; i++)
						{
							miAddCollection.Invoke(obj, new object[] { TypeHelper.ChangeType(list[i], parameterType) });
						}
						return obj;
					}
				}
			}
			Type typeIList = destinationType.GetInterface("System.Collections.IList", false);
			if(typeIList != null)
			{
				object obj = TypeHelper.CreateInstance(destinationType);
				IList list = obj as IList;
				for(int i = 0; i < ac.List.Count; i++)
				{
					list.Add( ac.List[i] );
				}
				return obj;
			}
#if !SILVERLIGHT
            return base.ConvertTo (context, culture, value, destinationType);
#else
            return base.ConvertTo(value, destinationType);
#endif
		}
	}

	/// <summary>
	/// Flex ArrayCollection class. The ArrayCollection class is a wrapper class that exposes an Array as a collection.
	/// </summary>
	[TypeConverter(typeof(ArrayCollectionConverter))]
    [CLSCompliant(false)]
    public class ArrayCollection : IExternalizable, IList
	{
		private IList _list;

		/// <summary>
		/// Initializes a new instance of the ArrayCollection class.
		/// </summary>
		public ArrayCollection()
		{
#if (NET_1_1)
            _list = new ArrayList();
#else
            _list = new List<object>();
#endif
		}
		/// <summary>
        /// Creates an ArrayCollection wrapper for a specific IList.
		/// </summary>
        /// <param name="list">The IList to wrap.</param>
		public ArrayCollection(IList list)
		{
			_list = list;
		}
        /// <summary>
        /// Gets the number of elements contained in the ArrayCollection.
        /// </summary>
		public int Count
		{
			get{ return _list == null ? 0 : _list.Count; }
		}
        /// <summary>
        /// Gets the underlying IList.
        /// </summary>
		public IList List
		{
			get{ return _list ; }
		}
        /// <summary>
        /// Copies the elements of the ArrayCollection to a new array.
        /// </summary>
        /// <returns></returns>
		public object[] ToArray()
		{
			if( _list != null )
			{
#if !SILVERLIGHT
                if( _list is ArrayList )
					return ((ArrayList)_list).ToArray();
#endif
#if !(NET_1_1)
				if( _list is List<object> )
                    return ((List<object>)_list).ToArray();
#endif
                object[] objArray = new object[_list.Count];
				for(int i = 0; i < _list.Count; i++ )
					objArray[i] = _list[i];
				return objArray;
			}
			return null;
		}

		#region IExternalizable Members
        /// <summary>
        /// Decode the ArrayCollection from a data stream.
        /// </summary>
        /// <param name="input">IDataInput interface.</param>
		public void ReadExternal(IDataInput input)
		{
			_list = input.ReadObject() as IList;
		}
        /// <summary>
        /// Encode the ArrayCollection for a data stream.
        /// </summary>
        /// <param name="output">IDataOutput interface.</param>
		public void WriteExternal(IDataOutput output)
		{
			output.WriteObject(ToArray());
		}

		#endregion

		#region IList Members
        /// <summary>
        /// Gets a value indicating whether the ArrayCollection is read-only.
        /// </summary>
		public bool IsReadOnly
		{
			get
			{
				return _list.IsReadOnly;
			}
		}
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
		public object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}
        /// <summary>
        /// Removes the ArrayCollection item at the specified index. 
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}
        /// <summary>
        /// Inserts an item to the ArrayCollection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the ArrayCollection.</param>
		public void Insert(int index, object value)
		{
			_list.Insert(index, value);
		}
        /// <summary>
        /// Removes the first occurrence of a specific object from the ArrayCollection.
        /// </summary>
        /// <param name="value">The Object to remove from the ArrayCollection.</param>
		public void Remove(object value)
		{
			_list.Remove(value);
		}
        /// <summary>
        /// Determines whether the ArrayCollection contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the ArrayCollection.</param>
        /// <returns>true if the Object is found in the ArrayCollection; otherwise, false.</returns>
		public bool Contains(object value)
		{
			return _list.Contains(value);
		}
        /// <summary>
        /// Removes all items from the ArrayCollection.
        /// </summary>
		public void Clear()
		{
			_list.Clear();
		}
        /// <summary>
        /// Determines the index of a specific item in the ArrayCollection. 
        /// </summary>
        /// <param name="value">The Object to locate in the ArrayCollection.</param>
        /// <returns>The index of value if found in the ArrayCollection; otherwise, -1.</returns>
		public int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}
        /// <summary>
        /// Adds an item to the ArrayCollection.
        /// </summary>
        /// <param name="value">The Object to add to the ArrayCollection.</param>
        /// <returns>The position into which the new element was inserted.</returns>
		public int Add(object value)
		{
			return _list.Add(value);
		}
        /// <summary>
        /// Gets a value indicating whether the ArrayCollection has a fixed size. 
        /// </summary>
		public bool IsFixedSize
		{
			get
			{
				return _list.IsFixedSize;
			}
		}

		#endregion

		#region ICollection Members
        /// <summary>
        /// Gets a value indicating whether access to the ArrayCollection is synchronized (thread safe).
        /// </summary>
		public bool IsSynchronized
		{
			get
			{
				return _list.IsSynchronized;
			}
		}
        /// <summary>
        /// Copies the elements of the ArrayCollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ArrayCollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}
        /// <summary>
        /// Gets an object that can be used to synchronize access to the ArrayCollection.
        /// </summary>
		public object SyncRoot
		{
			get
			{
				return _list.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through an ArrayCollection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
		public IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion
	}
}
