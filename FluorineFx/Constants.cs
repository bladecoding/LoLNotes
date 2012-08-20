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

namespace FluorineFx
{
	/// <summary>
	/// Object encoding (AMF version).
	/// </summary>
	public enum ObjectEncoding
	{
        /// <summary>
        /// AMF0 serialization.
        /// </summary>
        AMF0 = 0,
		/// <summary>
		/// AMF3 serialization.
		/// </summary>
		AMF3 = 3
	}

    /// <summary>
    /// AMF0 data types.
    /// </summary>
	public class AMF0TypeCode
	{
        /// <summary>
        /// AMF Number data type.
        /// </summary>
		public const byte Number = 0;
        /// <summary>
        /// AMF Boolean data type.
        /// </summary>
        public const byte Boolean = 1;
        /// <summary>
        /// AMF String data type.
        /// </summary>
        public const byte String = 2;
        /// <summary>
        /// AMF ASObject data type.
        /// </summary>
        public const byte ASObject = 3;
        /// <summary>
        /// AMF Null data type.
        /// </summary>
        public const byte Null = 5;
        /// <summary>
        /// AMF Undefined data type.
        /// </summary>
        public const byte Undefined = 6;
        /// <summary>
        /// AMF Reference data type.
        /// </summary>
        public const byte Reference = 7;
        /// <summary>
        /// AMF AssociativeArray data type.
        /// </summary>
        public const byte AssociativeArray = 8;
        /// <summary>
        /// AMF EndOfObject data type.
        /// </summary>
        public const byte EndOfObject = 9;
        /// <summary>
        /// AMF Array data type.
        /// </summary>
        public const byte Array = 10;
        /// <summary>
        /// AMF DateTime data type.
        /// </summary>
        public const byte DateTime = 11;
        /// <summary>
        /// AMF LongString data type.
        /// </summary>
        public const byte LongString = 12;
        /// <summary>
        /// AMF Xml data type.
        /// </summary>
        public const byte Xml = 15;
        /// <summary>
        /// AMF CustomClass(TypedObject) data type.
        /// </summary>
        public const byte CustomClass = 16;
        /// <summary>
        /// AMF3 data.
        /// </summary>
        public const byte AMF3Tag = 17;
	}

    /// <summary>
    /// AMF3 data types.
    /// </summary>
    public class AMF3TypeCode
	{
        /// <summary>
        /// AMF Undefined data type.
        /// </summary>
        public const byte Undefined = 0;
        /// <summary>
        /// AMF Null data type.
        /// </summary>
        public const byte Null = 1;
        /// <summary>
        /// AMF Boolean false data type.
        /// </summary>
        public const byte BooleanFalse = 2;
        /// <summary>
        /// AMF Boolean true data type.
        /// </summary>
        public const byte BooleanTrue = 3;
        /// <summary>
        /// AMF Integer data type.
        /// </summary>
        public const byte Integer = 4;
        /// <summary>
        /// AMF Number data type.
        /// </summary>
        public const byte Number = 5;
        /// <summary>
        /// AMF String data type.
        /// </summary>
        public const byte String = 6;
        /// <summary>
        /// AMF DateTime data type.
        /// </summary>
        public const byte DateTime = 8;
        /// <summary>
        /// AMF Array data type.
        /// </summary>
        public const byte Array = 9;
        /// <summary>
        /// AMF Object data type.
        /// </summary>
        public const byte Object = 10;
        /// <summary>
        /// AMF Xml data type.
        /// </summary>
        public const byte Xml = 11;
        /// <summary>
        /// AMF Xml data type.
        /// </summary>
        public const byte Xml2 = 7;
        /// <summary>
        /// AMF ByteArray data type.
        /// </summary>
        public const byte ByteArray = 12;
        /// <summary>
        /// AMF Integer Vector data type.
        /// </summary>
        public const byte IntVector = 13;
        /// <summary>
        /// AMF Uint Vector data type.
        /// </summary>
        public const byte UIntVector = 14;
        /// <summary>
        /// AMF Number Vector data type.
        /// </summary>
        public const byte NumberVector = 15;
        /// <summary>
        /// AMF Object Vector data type.
        /// </summary>
        public const byte ObjectVector = 16;
	}

    internal sealed class ContentType
    {
        public const string RTMPT = "application/x-fcs";
        public const string AMF = "application/x-amf";
        public const string XForm = "application/x-www-form-urlencoded";
    }

    internal class Internal
    {
        public static readonly object[] EmptyIList = new object[0];
    }
}
