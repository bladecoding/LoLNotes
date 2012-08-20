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
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
#if (NET_1_1)
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
#else
using System.IO.Compression;
#endif
using FluorineFx.Util;
using FluorineFx.SWX.Writers;
using FluorineFx.HttpCompress;

namespace FluorineFx.SWX
{
    class SwxAssembler
    {
        /// <summary>
        /// Header - FCS (uncompressed), version Flash 6
        /// </summary>
        static byte[] SwfHeader = new byte[] {0x57, 0x53, //WS
            0x06, //Flash version
            0x00, 0x00, 0x00, 0x00,//File length
            0x30, 0x0A, 0x00, 0xA0, 0x00, 0x01, 0x01, 0x00, 0x43, 0x02, 
            0xFF, 0xFF, 0xFF};

        static byte UncompressedSwf = 0x46;
        static byte CompressedSwf = 0x43;
        // Action bytecodes
        static byte[] ActionPush = new byte[] { 0x96, 0x00, 0x00 };
        static byte[] ActionShowFrame = new byte[] { 0x40, 0x00 };
        static byte[] ActionEndSwf = new byte[] { 0x00, 0x00 };
        static byte ActionSetVariable = 0x1D;
        static byte[] ActionDoAction = new byte[] { 0x3F, 0x03 };//Long tag header, Length of tag
        static byte ActionInitArray = 0x42;
        static byte ActionInitObject = 0x43;
        public static byte ActionPushData = 0x96;
        // Data type codes
        public static byte DataTypeString = 0x00;
        public static byte DataTypeFloat = 0x01;
        public static byte DataTypeNull = 0x02;
        public static byte DataTypeBoolean = 0x05;
        public static byte DataTypeDouble = 0x06;
        public static byte DataTypeInteger = 0x07;
        public static byte DataTypeConstantPool1 = 0x08;
        // Misc
        public static byte NullTerminator = 0x00;
        // Allow domain (*)
        static byte[] AllowDomain = new byte[]
        {
            0x96, 0x09, 0x00, 0x00, 0x5F, 0x70, 0x61, 0x72, 0x65, 0x6E, 0x74, 0x00, 0x1C, 0x96, 0x06, 0x00, 0x00, 0x5F, 0x75, 0x72, 0x6C, 0x00, 0x4E, 0x96, 0x0D, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x00, 0x53, 0x79, 0x73, 0x74, 
            0x65, 0x6D, 0x00, 0x1C, 0x96, 0x0A, 0x00, 0x00, 0x73, 0x65, 0x63, 0x75, 0x72, 0x69, 0x74, 0x79, 0x00, 0x4E, 0x96, 0x0D, 0x00, 0x00, 0x61, 0x6C, 0x6C, 0x6F, 0x77, 0x44, 0x6F, 0x6D, 0x61, 0x69, 0x6E, 0x00, 0x52, 0x17
        };
        static byte[] SystemAllowDomain = new byte[]
        {
            0x07, 0x01, 0x00, 0x00, 0x00, 0x00, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x00, 0x1C, 0x96, 0x0A, 0x00, 0x00, 0x73, 0x65, 0x63, 0x75, 0x72, 0x69, 0x74, 0x79, 0x00, 0x4E, 0x96, 0x0D, 0x00, 0x00, 0x61, 0x6C, 0x6C, 0x6F, 0x77, 0x44, 0x6F, 0x6D, 0x61, 0x69, 0x6E, 0x00, 0x52, 0x17
        };

        static byte[] DebugStart = new byte[]
        {
            0x88, 0x3C, 0x00, 0x07, 0x00, 0x72, 0x65, 0x73, 0x75, 0x6C, 0x74, 0x00, 0x6C, 0x63, 0x00, 0x4C, 0x6F, 0x63, 0x61, 0x6C, 0x43, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x00, 0x5F, 0x73, 0x77, 0x78, 0x44, 0x65, 0x62, 0x75, 0x67, 0x67, 0x65, 0x72, 0x00, 0x63, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74, 0x00, 0x64, 0x65, 0x62, 0x75, 0x67, 0x00, 0x73, 0x65, 0x6E, 0x64, 0x00
        };

        static byte[] DebugEnd = new byte[]
        {
            0x96, 0x0D, 0x00, 0x08, 0x01, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x02, 0x40, 0x3C, 0x96, 0x09, 0x00, 0x08, 0x03, 0x07, 0x01, 0x00, 0x00, 0x00, 0x08, 0x01, 0x1C, 0x96, 0x02, 0x00, 0x08, 0x04, 0x52, 0x17, 0x96, 0x02, 0x00, 0x08, 0x00, 0x1C, 0x96, 0x05, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x42, 0x96, 0x0B, 0x00, 0x08, 0x05, 0x08, 0x03, 0x07, 0x03, 0x00, 0x00, 0x00, 0x08, 0x01, 0x1C, 0x96, 0x02, 0x00, 0x08, 0x06, 0x52, 0x17
        };


        //static byte[] ResultVariableDebug = new byte[] { 0x96, 0x02, 0x00, 0x08, 0x00 };
        //static byte[] ResultVariable = new byte[] { 0x96, 0x08, 0x00, 0x00, 0x72, 0x65, 0x73, 0x75, 0x6C, 0x74, 0x00 };


        private static Hashtable SWXWriterTable;

        static SwxAssembler()
		{
            SWXWriterTable = new Hashtable();
            SWXDoubleWriter swxDoubleWriter = new SWXDoubleWriter();
            SWXWriterTable.Add(typeof(System.SByte), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Byte), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Int16), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.UInt16), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Int32), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.UInt32), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Int64), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.UInt64), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Single), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Double), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Decimal), swxDoubleWriter);
            SWXWriterTable.Add(typeof(System.Boolean), new SWXBooleanWriter());
            SWXWriterTable.Add(typeof(System.String), new SWXStringWriter());
            SWXWriterTable.Add(typeof(System.Array), new SWXArrayWriter());
        }

        ByteBuffer _swf;

        public SwxAssembler()
        {
            _swf = ByteBuffer.Allocate(50);
        }

        internal void PushNull()
        {
            _swf.Put(SwxAssembler.DataTypeNull);
        }

        internal void PushBoolean(bool value)
        {
            _swf.Put(SwxAssembler.DataTypeBoolean);
            _swf.Put(value ? (byte)1 : (byte)0);
        }

        internal void PushDouble(double value)
        {
            //64-bit IEEE double-precision little-endian double value
            _swf.Put(SwxAssembler.DataTypeDouble);
            byte[] bytes = BitConverter.GetBytes(value);
            //_swf.Put(value);
            _swf.Put(bytes, 4, 4);
            _swf.Put(bytes, 0, 4);
        }

        internal void PushInteger(int value)
        {
            _swf.Put(SwxAssembler.DataTypeInteger);
            byte[] bytes = BitConverter.GetBytes(value);
            _swf.Put(bytes);
        }

        internal void PushString(string value)
        {
            //Null terminated String
            _swf.Put(SwxAssembler.DataTypeString);
            UTF8Encoding utf8Encoding = new UTF8Encoding();
            byte[] bytes = utf8Encoding.GetBytes(value);
            _swf.Put(bytes);
            _swf.Put(SwxAssembler.NullTerminator);
        }

        private void EndPush()
        {
            if (_swf.Bookmark != -1)
            {
                _swf.Put((int)_swf.Bookmark, (UInt16)(_swf.Position - _swf.Bookmark - 2));
                _swf.ClearBookmark();
            }
        }

        private void StartPush()
        {
            if (_swf.Bookmark == -1)
            {
                _swf.Put(SwxAssembler.ActionPushData);
                _swf.Mark();//start marking for length check
                _swf.Skip(2);//Skip ActionRecord length
            }
        }

        internal void PushArray(Array value)
        {
            for (int i = value.Length - 1; i >= 0; i--)
            {
                object element = value.GetValue(i);
                DataToBytecode(element);
            }
            //Push array length
            StartPush();
            PushInteger(value.Length);
            EndPush();
            _swf.Put(SwxAssembler.ActionInitArray);
        }

        internal void PushAssociativeArray(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                DataToBytecode(entry.Key);
                DataToBytecode(entry.Value);
            }
            StartPush();
            PushInteger(dictionary.Count);
            EndPush();
            _swf.Put(SwxAssembler.ActionInitObject);
        }

        internal void PushObject(object obj)
        {
            Type type = obj.GetType();
            string customClass = type.FullName;

            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ArrayList properties = new ArrayList(propertyInfos);
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                PropertyInfo propertyInfo = properties[i] as PropertyInfo;
                if (propertyInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0)
                    properties.RemoveAt(i);
                if (propertyInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    properties.RemoveAt(i);
            }
            foreach (PropertyInfo propertyInfo in properties)
            {
                DataToBytecode(propertyInfo.Name);
                object value = propertyInfo.GetValue(obj, null);
                DataToBytecode(value);
            }

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            ArrayList fields = new ArrayList(fieldInfos);
            for (int i = fields.Count - 1; i >= 0; i--)
            {
                FieldInfo fieldInfo = fields[i] as FieldInfo;
                if (fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0)
                    fields.RemoveAt(i);
                if (fieldInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    fields.RemoveAt(i);
            }
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo fieldInfo = fields[i] as FieldInfo;
                DataToBytecode(fieldInfo.Name);
                object value = fieldInfo.GetValue(obj);
                DataToBytecode(value);
            }
            StartPush();
            PushInteger(properties.Count + fields.Count);
            EndPush();
            _swf.Put(SwxAssembler.ActionInitObject);
        }

        internal byte[] WriteSwf(object data, bool debug, CompressionLevels compressionLevel, string url, bool allowDomain)
        {
            // Create the SWF
            byte headerType = compressionLevel != CompressionLevels.None ? SwxAssembler.CompressedSwf : SwxAssembler.UncompressedSwf;
            _swf.Put(headerType);
            _swf.Put(SwxAssembler.SwfHeader);

            //DoAction
            _swf.Put(SwxAssembler.ActionDoAction);
            int doActionBlockSizeIndex = (int)_swf.Position;
            _swf.Skip(4);
            int doActionBlockStartIndex = (int)_swf.Position;

            if (debug)
                _swf.Put(SwxAssembler.DebugStart);

            _swf.Put(SwxAssembler.ActionPushData);
            _swf.Mark();//start marking for length check
            _swf.Skip(2);//Skip ActionRecord length

            // Add the 'result' variable name -- either
            // using the constant table if in debug mode
            // or as a regular string otherwise
            if (debug)
            {
                _swf.Put(SwxAssembler.DataTypeConstantPool1);
                _swf.Put((byte)0);
            }
            else
            {
                PushString("result");
            }
            DataToBytecode(data);
            //Put ActionRecord length
            EndPush();
            
            _swf.Put(SwxAssembler.ActionSetVariable);
            if (allowDomain)
            {
                GenerateAllowDomainBytecode(url);
            }
            if (debug)
                _swf.Put(SwxAssembler.DebugEnd);

            //Fix DoAction size
            long doActionBlockEndIndex = _swf.Position;
            UInt32 doActionBlockSizeInBytes = (UInt32)(doActionBlockEndIndex - doActionBlockStartIndex);
            _swf.Put(doActionBlockSizeIndex, doActionBlockSizeInBytes);
            
            //Swf End
            _swf.Put(SwxAssembler.ActionShowFrame);
            _swf.Put(SwxAssembler.ActionEndSwf);
            
            //Fix Swf size
            UInt32 swfSizeInBytes = (UInt32)_swf.Length;
            _swf.Put(4, swfSizeInBytes);

            _swf.Flip();
            byte[] buffer = _swf.ToArray();

            if (compressionLevel != CompressionLevels.None)
            {
                MemoryStream msCompressed = new MemoryStream();
#if (NET_1_1)
                ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream deflaterOutputStream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(msCompressed, new ICSharpCode.SharpZipLib.Zip.Compression.Deflater((int)compressionLevel, false));
                deflaterOutputStream.Write(buffer, 8, buffer.Length - 8);
                deflaterOutputStream.Close();
#else
                DeflateStream deflateStream = new DeflateStream(msCompressed, CompressionMode.Compress, false);
                deflateStream.Write(buffer, 8, buffer.Length - 8);
                deflateStream.Close();
#endif
                byte[] msBuffer = msCompressed.ToArray();
                byte[] compressedBuffer = new byte[msBuffer.Length + 8];
                Buffer.BlockCopy(buffer, 0, compressedBuffer, 0, 8);
                Buffer.BlockCopy(msBuffer, 0, compressedBuffer, 8, msBuffer.Length);
                buffer = compressedBuffer;
            }
            //ByteBuffer dumpBuffer = ByteBuffer.Wrap(buffer);
            //dumpBuffer.Dump("test.swf");
            return buffer;
        }

        internal void DataToBytecode(object data)
        {
            if (_swf.Bookmark != -1)
            {
                //ActionRecord length is UINT16 (max 65535)
                int byteCodeLength = (int)(_swf.Position - _swf.Bookmark);
                if (byteCodeLength >= 65520) // For testing use >= 2)
                {
                    EndPush();
                    _swf.Put(SwxAssembler.ActionPushData);
                    _swf.Mark();//start marking for length check
                    _swf.Skip(2);//Skip ActionRecord length
                }
            }
            else
            {
                _swf.Put(SwxAssembler.ActionPushData);
                _swf.Mark();//start marking for length check
                _swf.Skip(2);//Skip ActionRecord length
            }

            if (data == null)
            {
                PushNull();
                return;
            }
            Type type = data.GetType();
            ISWXWriter swxWriter = SWXWriterTable[type] as ISWXWriter;
            //Second try with basetype (enums and arrays for example)
            if (swxWriter == null)
                swxWriter = SWXWriterTable[type.BaseType] as ISWXWriter;

            if (swxWriter == null)
            {
                lock (SWXWriterTable)
                {
                    if (!SWXWriterTable.Contains(type))
                    {
                        swxWriter = new SWXObjectWriter();
                        SWXWriterTable.Add(type, swxWriter);
                    }
                    else
                        swxWriter = SWXWriterTable[type] as ISWXWriter;
                }
            }
            swxWriter.WriteData(this, data);
        }

        private void GenerateAllowDomainBytecode(string url)
        {
            if (url != null)
            {
                _swf.Put(SwxAssembler.ActionPushData);
                int sizeIndex = (int)_swf.Position;
                _swf.Skip(2);//Skip ActionRecord length
                PushString(url);
                _swf.Put(sizeIndex, (UInt16)((_swf.Position - sizeIndex - 2) + 13));
                _swf.Put(SwxAssembler.SystemAllowDomain);
            }
        }
    }
}
