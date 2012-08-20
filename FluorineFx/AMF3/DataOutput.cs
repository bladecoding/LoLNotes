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

using FluorineFx.IO;

namespace FluorineFx.AMF3
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DataOutput : IDataOutput
	{
		private AMFWriter _amfWriter;
        private ObjectEncoding _objectEncoding;

		public DataOutput(AMFWriter amfWriter)
		{
			_amfWriter = amfWriter;
            _objectEncoding = ObjectEncoding.AMF3;
		}

        public ObjectEncoding ObjectEncoding
        {
            get { return _objectEncoding; }
            set { _objectEncoding = value; }
        }

		#region IDataOutput Members

		/// <summary>
		/// Writes a Boolean value.
		/// </summary>
		/// <param name="value"></param>
		public void WriteBoolean(bool value)
		{
			_amfWriter.WriteBoolean(value);
		}
		/// <summary>
		/// Writes a byte.
		/// </summary>
		/// <param name="value"></param>
		public void WriteByte(byte value)
		{
			_amfWriter.WriteByte(value);
		}
		/// <summary>
		/// Writes a sequence of length bytes from the specified byte array, bytes, starting offset(zero-based index) bytes into the byte stream.
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void WriteBytes(byte[] bytes, int offset, int length)
		{
			for(int i = offset; i < offset + length; i++)
				_amfWriter.WriteByte( bytes[i] );
		}
		/// <summary>
		/// Writes an IEEE 754 double-precision (64-bit) floating point number.
		/// </summary>
		/// <param name="value"></param>
		public void WriteDouble(double value)
		{
			_amfWriter.WriteDouble(value);
		}
		/// <summary>
		/// Writes an IEEE 754 single-precision (32-bit) floating point number.
		/// </summary>
		/// <param name="value"></param>
		public void WriteFloat(float value)
		{
			_amfWriter.WriteFloat(value);
		}
		/// <summary>
		/// Writes a 32-bit signed integer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt(int value)
		{
			_amfWriter.WriteInt32(value);
		}
		/// <summary>
		/// Writes an object to the byte stream or byte array in AMF serialized format.
		/// </summary>
		/// <param name="value"></param>
		public void WriteObject(object value)
		{
            if( _objectEncoding == ObjectEncoding.AMF0 )
                _amfWriter.WriteData(ObjectEncoding.AMF0, value);
            if (_objectEncoding == ObjectEncoding.AMF3)
                _amfWriter.WriteAMF3Data(value);
		}
		/// <summary>
		/// Writes a 16-bit integer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteShort(short value)
		{
			_amfWriter.WriteShort(value);
		}
		/// <summary>
		/// Writes a 32-bit unsigned integer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUnsignedInt(uint value)
		{
			_amfWriter.WriteInt32((int)value);
		}
		/// <summary>
		/// Writes a UTF-8 string to the byte stream. 
		/// The length of the UTF-8 string in bytes is written first, as a 16-bit integer, followed by 
		/// the bytes representing the characters of the string.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUTF(string value)
		{
			_amfWriter.WriteUTF(value);
		}
		/// <summary>
		/// Writes a UTF-8 string. Similar to writeUTF, but does not prefix the string with a 16-bit length word.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUTFBytes(string value)
		{
			_amfWriter.WriteUTFBytes(value);
		}

		#endregion
	}
}
