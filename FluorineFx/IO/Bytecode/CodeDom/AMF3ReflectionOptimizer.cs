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
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using log4net;

using FluorineFx.AMF3;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;

namespace FluorineFx.IO.Bytecode.CodeDom
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF3ReflectionOptimizer : AMF0ReflectionOptimizer
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(AMF3ReflectionOptimizer));

        ClassDefinition _classDefinition;

		public AMF3ReflectionOptimizer(Type type, ClassDefinition classDefinition, AMFReader reader) : base(type, reader)
		{
            _classDefinition = classDefinition;
		}

        protected override string GenerateCode(object instance)
        {
            Type type = instance.GetType();

            _layouter.Append(GetHeader());
            _layouter.AppendFormat(GetClassDefinition(), _mappedClass.FullName.Replace('.', '_').Replace("+", "__"), _mappedClass.FullName);

            _layouter.Begin();
            _layouter.Begin();
            _layouter.AppendFormat("{0} instance = new {0}();", _mappedClass.FullName);
            _layouter.Append("reader.AddAMF3ObjectReference(instance);");
            _layouter.Append("byte typeCode = 0;");
            for (int i = 0; i < _classDefinition.MemberCount; i++)
            {
                string key = _classDefinition.Members[i].Name;
                object value = _reader.ReadAMF3Data();
                _reader.SetMember(instance, key, value);

                _layouter.Append("typeCode = reader.ReadByte();");
                MemberInfo[] memberInfos = type.GetMember(key);
                if (memberInfos != null && memberInfos.Length > 0)
                    GeneratePropertySet(memberInfos[0]);
                else
                {
                    //Log this error (do not throw exception), otherwise our current AMF stream becomes unreliable
                    log.Warn(__Res.GetString(__Res.Optimizer_Warning));
                    string msg = __Res.GetString(__Res.Reflection_MemberNotFound, string.Format("{0}.{1}", _mappedClass.FullName, key));
                    log.Warn(msg);
                    _layouter.AppendFormat("//{0}", msg);
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                }
            }

            _layouter.Append("return instance;");
            _layouter.End();
            _layouter.Append("}");
            _layouter.End();
            _layouter.Append("}"); // Close class
            _layouter.Append("}"); // Close namespace
            return _layouter.ToString();
        }

        private void GeneratePropertySet(MemberInfo memberInfo)
        {
            Type memberType = null;
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                memberType = propertyInfo.PropertyType;
            }
            if (memberInfo is FieldInfo)
            {
                FieldInfo fieldInfo = memberInfo as FieldInfo;
                memberType = fieldInfo.FieldType;
            }
            _layouter.AppendFormat("//Setting member {0}", memberInfo.Name);

            //The primitive types are: Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Double, Single
            if (memberType.IsPrimitive || memberType == typeof(decimal))
            {
                switch (Type.GetTypeCode(memberType))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.Int16:
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        _layouter.Append("if( typeCode == AMF3TypeCode.Number )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = ({1})reader.ReadDouble();", memberInfo.Name, memberType.FullName);
                        _layouter.End();
                        _layouter.Append("else if( typeCode == AMF3TypeCode.Integer )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = ({1})reader.ReadAMF3Int();", memberInfo.Name, memberType.FullName);
                        _layouter.End();
                        _layouter.Append("else");
                        _layouter.Begin();
                        if (DoTypeCheck())
                            GenerateThrowUnexpectedAMFException(memberInfo);
                        else
                            _layouter.Append("reader.ReadAMF3Data(typeCode);");
                        _layouter.End();
                        break;
                    case TypeCode.Boolean:
                        _layouter.Append("if( typeCode == AMF3TypeCode.BooleanFalse )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = false;", memberInfo.Name);
                        _layouter.End();
                        _layouter.Append("else if( typeCode == AMF3TypeCode.BooleanTrue )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = true;", memberInfo.Name);
                        _layouter.End();
                        _layouter.Append("else");
                        _layouter.Begin();
                        if (DoTypeCheck())
                            GenerateThrowUnexpectedAMFException(memberInfo);
                        else
                            _layouter.Append("reader.ReadAMF3Data(typeCode);");
                        _layouter.End();
                        break;
                    case TypeCode.Char:
                        _layouter.Append("if( typeCode == AMF3TypeCode.String )");
                        _layouter.Append("{");
                        _layouter.Begin();
                        _layouter.AppendFormat("string str{0} = reader.ReadAMF3String();", memberInfo.Name);
                        _layouter.AppendFormat("if( str{0} != null && str{0} != string.Empty )", memberInfo.Name);
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = str{0}[0];", memberInfo.Name);
                        _layouter.End();
                        _layouter.End();
                        _layouter.Append("}");
                        _layouter.Append("else");
                        _layouter.Begin();
                        if (DoTypeCheck())
                            GenerateThrowUnexpectedAMFException(memberInfo);
                        else
                            _layouter.Append("reader.ReadAMF3Data(typeCode);");
                        _layouter.End();
                        break;
                }
                return;
            }
            if (memberType.IsEnum)
            {
                _layouter.Append("if( typeCode == AMF3TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = ({1})Enum.Parse(typeof({1}), reader.ReadAMF3String(), true);", memberInfo.Name, memberType.FullName);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Integer )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = ({1})Enum.ToObject(typeof({1}), reader.ReadAMF3Int());", memberInfo.Name, memberType.FullName);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                _layouter.End();
                return;
            }
            if (memberType == typeof(DateTime))
            {
                _layouter.Append("if( typeCode == AMF3TypeCode.DateTime )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadAMF3Date();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                _layouter.End();
                return;
            }
            if (memberType == typeof(Guid))
            {
                _layouter.Append("if( typeCode == AMF3TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = new Guid(reader.ReadAMF3String());", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                _layouter.End();
                return;
            }
            if (memberType.IsValueType)
            {
                //structs are not handled
                throw new FluorineException("Struct value types are not supported");
            }
            if (memberType == typeof(string))
            {
                _layouter.Append("if( typeCode == AMF3TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadAMF3String();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Null )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Undefined )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                _layouter.End();
                return;
            }
            if (memberType == typeof(XmlDocument))
            {
                _layouter.Append("if( typeCode == AMF3TypeCode.Xml )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadAMF3XmlDocument();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Xml2 )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadAMF3XmlDocument();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Null )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF3TypeCode.Undefined )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadAMF3Data(typeCode);");
                _layouter.End();
                return;
            }

            _layouter.AppendFormat("instance.{0} = ({1})TypeHelper.ChangeType(reader.ReadAMF3Data(typeCode), typeof({1}));", memberInfo.Name, TypeHelper.GetCSharpName(memberType));
        }
	}
}
