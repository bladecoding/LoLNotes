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
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

using log4net;

using FluorineFx.Exceptions;
using FluorineFx.Configuration;
using FluorineFx.Util;

namespace FluorineFx.IO.Bytecode.CodeDom
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF0ReflectionOptimizer
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AMF0ReflectionOptimizer));

		private CompilerParameters _cp = new CompilerParameters();
        protected Type _mappedClass;
        protected AMFReader _reader;
		protected Layouter _layouter;

		public AMF0ReflectionOptimizer(Type type, AMFReader reader)
		{
			_mappedClass = type;
			_reader = reader;
			_layouter = new Layouter();
		}

		public IReflectionOptimizer Generate(object instance)
		{
			try
			{
				InitCompiler();
				return Build(GenerateCode(instance));
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void InitCompiler()
		{
			_cp.GenerateInMemory = true;
			if( FluorineConfiguration.Instance.OptimizerSettings.Debug )
			{
				_cp.GenerateInMemory = false;
				_cp.IncludeDebugInformation = true;
			}
			_cp.TreatWarningsAsErrors = false;

#if ! DEBUG
				_cp.CompilerOptions = "/optimize";
#endif

			AddAssembly(Assembly.GetExecutingAssembly().Location);

			Assembly classAssembly = _mappedClass.Assembly;
			AddAssembly(classAssembly.Location);

			foreach (AssemblyName referencedName in classAssembly.GetReferencedAssemblies())
			{
				Assembly referencedAssembly = Assembly.Load(referencedName);
				AddAssembly(referencedAssembly.Location);
			}
            foreach (AssemblyName referencedName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                Assembly referencedAssembly = Assembly.Load(referencedName);
                AddAssembly(referencedAssembly.Location);
            }
        }

		/// <summary>
		/// Add an assembly to the list of ReferencedAssemblies
		/// required to build the class
		/// </summary>
		/// <param name="name"></param>
		private void AddAssembly(string name)
		{
			if (name.StartsWith("System.")) return;

			if (!_cp.ReferencedAssemblies.Contains(name))
			{
				_cp.ReferencedAssemblies.Add(name);
			}
		}

		
		private IReflectionOptimizer Build(string code)
		{
			CodeDomProvider provider = new CSharpCodeProvider();
			CompilerResults res;
			if( FluorineConfiguration.Instance.OptimizerSettings.Debug )
			{
				string file = Path.Combine(Path.GetTempPath(), _mappedClass.FullName.Replace('.', '_').Replace("+", "__") ) + ".cs";
				StreamWriter sw = File.CreateText( file);
				sw.Write(code);
				sw.Close();
                log.Debug(__Res.GetString(__Res.Optimizer_FileLocation, _mappedClass.FullName, file));

				_cp.TempFiles = new TempFileCollection(Path.GetTempPath());
				_cp.TempFiles.KeepFiles = true;

#if !(NET_1_1)
                res = provider.CompileAssemblyFromFile( _cp, file );
#else
				ICodeCompiler compiler = provider.CreateCompiler();
				res = compiler.CompileAssemblyFromFile( _cp, file );
#endif

			}
			else
            {
#if !(NET_1_1)
                res = provider.CompileAssemblyFromSource( _cp, new string[] {code});
#else
				ICodeCompiler compiler = provider.CreateCompiler();
				res = compiler.CompileAssemblyFromSource( _cp, code );
#endif
			}

			if (res.Errors.HasErrors)
			{
				foreach (CompilerError e in res.Errors)
				{
					log.Error(__Res.GetString(__Res.Compiler_Error, e.Line, e.Column, e.ErrorText));
				}
				throw new InvalidOperationException(res.Errors[0].ErrorText);
			}

			Assembly assembly = res.CompiledAssembly;
			System.Type[] types = assembly.GetTypes();
			IReflectionOptimizer optimizer = (IReflectionOptimizer)assembly.CreateInstance(types[0].FullName, false, BindingFlags.CreateInstance, null, null, null, null);
			return optimizer;
		}

        protected virtual string GetHeader()
        {
            return
            "using System;\n" +
            "using System.Reflection;\n" +
            "using FluorineFx;\n" +
            "using FluorineFx.AMF3;\n" +
            "using FluorineFx.IO;\n" +
            "using FluorineFx.Exceptions;\n" +
            "using FluorineFx.Configuration;\n" +
            "using FluorineFx.IO.Bytecode;\n" +
            "using log4net;\n" +
            "namespace FluorineFx.Bytecode.CodeDom {\n";
        }

        protected virtual string GetClassDefinition()
        {
            return
            @"public class {0} : IReflectionOptimizer {{
					
	public {0}() {{
	}}

	public object CreateInstance() {{
		return new {1}();
	}}

	public object ReadData(AMFReader reader, ClassDefinition classDefinition) {{
	";
        }

		protected virtual string GenerateCode(object instance)
		{
            _layouter.Append(GetHeader());
			_layouter.AppendFormat(GetClassDefinition(), _mappedClass.FullName.Replace('.', '_').Replace("+", "__"), _mappedClass.FullName);

			_layouter.Begin();
			_layouter.Begin();
			_layouter.AppendFormat("{0} instance = new {0}();", _mappedClass.FullName);
			_layouter.Append("reader.AddReference(instance);");

			Type type = instance.GetType();
			string key = _reader.ReadString();
			_layouter.Append("byte typeCode = 0;");
			_layouter.Append("string key = null;");
			for (byte typeCode = _reader.ReadByte(); typeCode != AMF0TypeCode.EndOfObject; typeCode = _reader.ReadByte())
			{
				_layouter.Append("key = reader.ReadString();");
				_layouter.Append("typeCode = reader.ReadByte();");

				object value = _reader.ReadData(typeCode);
                _reader.SetMember(instance, key, value);
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
                    _layouter.Append("reader.ReadData(typeCode);");
                }
				key = _reader.ReadString();
			}
			_layouter.Append("key = reader.ReadString();");
			_layouter.Append("typeCode = reader.ReadByte();");
			_layouter.Append("if( typeCode != AMF0TypeCode.EndOfObject ) throw new UnexpectedAMF();");

			_layouter.Append("return instance;");
			_layouter.End();
			_layouter.Append("}");
			_layouter.End();
			_layouter.Append("}"); // Close class
			_layouter.Append("}"); // Close namespace
			return _layouter.ToString();
		}

        protected bool DoTypeCheck()
        {
            return FluorineConfiguration.Instance.OptimizerSettings.TypeCheck;
        }

        private void GeneratePropertySet(MemberInfo memberInfo)
		{
			Type memberType = null;
			if( memberInfo is PropertyInfo )
			{
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				memberType = propertyInfo.PropertyType;
			}
			if( memberInfo is FieldInfo )
			{
				FieldInfo fieldInfo = memberInfo as FieldInfo;
				memberType = fieldInfo.FieldType;
			}
			_layouter.AppendFormat("//Setting member {0}", memberInfo.Name);

			//The primitive types are: Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Double, Single
			if( memberType.IsPrimitive || memberType == typeof(decimal) )
			{
				switch(Type.GetTypeCode(memberType))
				{							
					case TypeCode.Byte:
					case TypeCode.Decimal:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Double:
                        _layouter.Append("if( typeCode == AMF0TypeCode.Number )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = ({1})reader.ReadDouble();", memberInfo.Name, memberType.FullName);
                        _layouter.End();
                        _layouter.Append("else");
                        _layouter.Begin();
                        if (DoTypeCheck())
                            GenerateThrowUnexpectedAMFException(memberInfo);
                        else
                            _layouter.Append("reader.ReadData(typeCode);");
                        _layouter.End();
						break;
					case TypeCode.Boolean:
                        _layouter.Append("if( typeCode == AMF0TypeCode.Boolean )");
                        _layouter.Begin();
                        _layouter.AppendFormat("instance.{0} = reader.ReadBoolean();", memberInfo.Name);
                        _layouter.End();
                        _layouter.Append("else");
                        _layouter.Begin();
                        if (DoTypeCheck())
                            GenerateThrowUnexpectedAMFException(memberInfo);
                        else
                            _layouter.Append("reader.ReadData(typeCode);");
                        _layouter.End();
						break;
					case TypeCode.Char:
                        _layouter.Append("if( typeCode == AMF0TypeCode.String )");
                        _layouter.Append("{");
                        _layouter.Begin();
                        _layouter.AppendFormat("string str{0} = reader.ReadString();", memberInfo.Name);
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
                            _layouter.Append("reader.ReadData(typeCode);");
                        _layouter.End();
						break;
				}
				return;
			}
			if( memberType.IsEnum )
			{
                _layouter.Append("if( typeCode == AMF0TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = ({1})Enum.Parse(typeof({1}), reader.ReadString(), true);", memberInfo.Name, memberType.FullName);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF0TypeCode.Number )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = ({1})Enum.ToObject(typeof({1}), Convert.ToInt32(reader.ReadDouble()));", memberInfo.Name, memberType.FullName);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadData(typeCode);");
                _layouter.End();
				return;
			}
			if( memberType == typeof(DateTime) )
			{
                _layouter.Append("if( typeCode == AMF0TypeCode.DateTime )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadDateTime();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadData(typeCode);");
                _layouter.End();
				return;
			}
			if (memberType == typeof(Guid))
			{
                _layouter.Append("if( typeCode == AMF0TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = new Guid(reader.ReadString());", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadData(typeCode);");
                _layouter.End();
				return;
			}
			if( memberType.IsValueType )
			{
				//structs are not handled
				throw new FluorineException("Struct value types are not supported");
			}
			if( memberType == typeof(string) )
			{
                _layouter.Append("if( typeCode == AMF0TypeCode.String )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadString();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF0TypeCode.LongString )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = reader.ReadLongString();", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF0TypeCode.Null )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF0TypeCode.Undefined )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadData(typeCode);");
                _layouter.End();
				return;
			}
			if( memberType == typeof(XmlDocument) )
			{
                _layouter.Append("if( typeCode == AMF0TypeCode.Xml )");
                _layouter.Append("{");
                _layouter.Begin();
                _layouter.AppendFormat("string xml{0} = reader.ReadLongString();", memberInfo.Name);
                _layouter.AppendFormat("System.Xml.XmlDocument xmlDocument{0} = new System.Xml.XmlDocument();", memberInfo.Name);
                _layouter.AppendFormat("xmlDocument{0}.LoadXml(xml{0});", memberInfo.Name);
                _layouter.AppendFormat("instance.{0} = xmlDocument{0};", memberInfo.Name);
                _layouter.End();
                _layouter.Append("}");
                _layouter.Append("else if( typeCode == AMF0TypeCode.Null )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else if( typeCode == AMF0TypeCode.Undefined )");
                _layouter.Begin();
                _layouter.AppendFormat("instance.{0} = null;", memberInfo.Name);
                _layouter.End();
                _layouter.Append("else");
                _layouter.Begin();
                if (DoTypeCheck())
                    GenerateThrowUnexpectedAMFException(memberInfo);
                else
                    _layouter.Append("reader.ReadData(typeCode);");
                _layouter.End();
				return;
			}
            _layouter.AppendFormat("instance.{0} = ({1})TypeHelper.ChangeType(reader.ReadData(typeCode), typeof({1}));", memberInfo.Name, TypeHelper.GetCSharpName(memberType));
		}

		protected void GenerateElseThrowUnexpectedAMFException(MemberInfo memberInfo)
		{
			_layouter.Append("else");
			_layouter.Begin();
			_layouter.AppendFormat("throw new UnexpectedAMF(\"Unexpected data for member {0}\");", memberInfo.Name);
			_layouter.End();
		}

        protected void GenerateThrowUnexpectedAMFException(MemberInfo memberInfo)
        {
            _layouter.AppendFormat("throw new UnexpectedAMF(\"Unexpected data for member {0}\");", memberInfo.Name);
        }

        protected void GenerateThrowUnexpectedAMFException(string message)
        {
            _layouter.AppendFormat("throw new UnexpectedAMF(\"{0}\");", message);
        }

	}
}
