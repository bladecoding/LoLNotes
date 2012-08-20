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
using FluorineFx.Reflection.Lightweight;
using log4net;

using FluorineFx.AMF3;
using FluorineFx.Configuration;
using FluorineFx.Exceptions;


namespace FluorineFx.IO.Bytecode.Lightweight
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    class AMF3ReflectionOptimizer : IReflectionOptimizer
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(AMF3ReflectionOptimizer));

        CreateInstanceInvoker _createInstanceMethod;
        ReadDataInvoker _readDataMethod;
        ClassDefinition _classDefinition;
#if !(MONO) && !(NET_2_0) && !(NET_3_5) && !(SILVERLIGHT)
        PermissionSet _ps;
#endif

        public AMF3ReflectionOptimizer(Type type, ClassDefinition classDefinition, AMFReader reader, object instance)
		{
            _classDefinition = classDefinition;
            _createInstanceMethod = CreateCreateInstanceMethod(type);
#if !(MONO) && !(NET_2_0) && !(NET_3_5) && !(SILVERLIGHT)
            _ps = new PermissionSet(PermissionState.None);
            _ps.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
#endif
            _readDataMethod = CreateReadDataMethod(type, reader, instance);
        }

        private CreateInstanceInvoker CreateCreateInstanceMethod(System.Type type)
        {
            DynamicMethod method = new DynamicMethod(string.Empty, typeof(object), null, type, true);
            ILGenerator il = method.GetILGenerator();

            ConstructorInfo constructor = type.GetConstructor(EmitHelper.AnyVisibilityInstance, null, CallingConventions.HasThis, System.Type.EmptyTypes, null);
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);
            return (CreateInstanceInvoker)method.CreateDelegate(typeof(CreateInstanceInvoker));
        }

        protected virtual ReadDataInvoker CreateReadDataMethod(Type type, AMFReader reader, object instance)
        {
#if !(MONO) && !(NET_2_0) && !(NET_3_5) && !(SILVERLIGHT)
            bool canSkipChecks = _ps.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#else
            bool canSkipChecks = SecurityManager.IsGranted(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
#endif
            DynamicMethod method = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(AMFReader), typeof(ClassDefinition) }, this.GetType(), canSkipChecks);
            ILGenerator il = method.GetILGenerator();

            LocalBuilder instanceLocal = il.DeclareLocal(type);//[0] instance
            LocalBuilder typeCodeLocal = il.DeclareLocal(typeof(byte));//[1] uint8 typeCode
            LocalBuilder keyLocal = il.DeclareLocal(typeof(string));//[2] string key
            LocalBuilder objTmp = il.DeclareLocal(typeof(object));//[3] temp object store
            LocalBuilder intTmp1 = il.DeclareLocal(typeof(int));//[4] temp int store, length
            LocalBuilder intTmp2 = il.DeclareLocal(typeof(int));//[5] temp int store, index
            LocalBuilder objTmp2 = il.DeclareLocal(typeof(object));//[6] temp object store
            LocalBuilder typeTmp = il.DeclareLocal(typeof(Type));//[7] temp Type store

            EmitHelper emit = new EmitHelper(il);
            ConstructorInfo typeConstructor = type.GetConstructor(EmitHelper.AnyVisibilityInstance, null, CallingConventions.HasThis, System.Type.EmptyTypes, null);
            MethodInfo miAddReference = typeof(AMFReader).GetMethod("AddAMF3ObjectReference");
            MethodInfo miReadByte = typeof(AMFReader).GetMethod("ReadByte");
            emit
                //object instance = new object();
                .newobj(typeConstructor) //Create the new instance and push the object reference onto the evaluation stack
                .stloc_0 //Pop from the top of the evaluation stack and store it in a the local variable list at index 0
                //reader.AddReference(instance);
                .ldarg_0 //Push the argument indexed at 1 onto the evaluation stack 'reader'
                .ldloc_0 //Loads the local variable at index 0 onto the evaluation stack 'instance'
                .callvirt(miAddReference) //Arguments are popped from the stack, the method call is performed, return value is pushed onto the stack
                //typeCode = 0;
                .ldc_i4_0 //Push the integer value of 0 onto the evaluation stack as an int32
                .stloc_1 //Pop and store it in a the local variable list at index 1
                //string key = null;
                .ldnull //Push a null reference onto the evaluation stack
                .stloc_2 //Pop and store it in a the local variable list at index 2 'key'
                .end()
            ;

            for (int i = 0; i < _classDefinition.MemberCount; i++)
            {
                string key = _classDefinition.Members[i].Name;
                byte typeCode = reader.ReadByte();
                object value = reader.ReadAMF3Data(typeCode);
                reader.SetMember(instance, key, value);

                emit
                    //.ldarg_1
                    //.callvirt(typeof(ClassDefinition).GetMethod("get_Members"))
                    //.ldc_i4(i)
                    //.conv_ovf_i
                    //.ldelem_ref
                    //.stloc_2
                    .ldarg_0
                    .callvirt(miReadByte)
                    .stloc_1
                    .end()
                ;
                
                MemberInfo[] memberInfos = type.GetMember(key);
                if (memberInfos != null && memberInfos.Length > 0)
                    GeneratePropertySet(emit, typeCode, memberInfos[0]);
                else
                {
                    //Log this error (do not throw exception), otherwise our current AMF stream becomes unreliable
                    log.Warn(__Res.GetString(__Res.Optimizer_Warning));
                    string msg = __Res.GetString(__Res.Reflection_MemberNotFound, string.Format("{0}.{1}", type.FullName, key));
                    log.Warn(msg);
                    //reader.ReadAMF3Data(typeCode);
                    emit
                        .ldarg_0 //Push 'reader'
                        .ldloc_1 //Push 'typeCode'
                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                        .pop
                        .end()
                    ;
                }
            }
            Label labelExit = emit.DefineLabel();
            emit
                .MarkLabel(labelExit)
                //return instance;
                .ldloc_0 //Load the local variable at index 0 onto the evaluation stack
                .ret() //Return
            ;

            return (ReadDataInvoker)method.CreateDelegate(typeof(ReadDataInvoker));
        }

        protected bool DoTypeCheck()
        {
            return FluorineConfiguration.Instance.OptimizerSettings.TypeCheck;
        }

        private void GeneratePropertySet(EmitHelper emit, int typeCode, MemberInfo memberInfo)
        {
            Type memberType = null;
            if (memberInfo.MemberType ==  MemberTypes.Property)
            {
                PropertyInfo propertyInfo = memberInfo.DeclaringType.GetProperty(memberInfo.Name);
                memberType = propertyInfo.PropertyType;
            }
            if (memberInfo is FieldInfo)
            {
                FieldInfo fieldInfo = memberInfo.DeclaringType.GetField(memberInfo.Name);
                memberType = fieldInfo.FieldType;
            }
            if (memberType == null)
                throw new ArgumentNullException(memberInfo.Name);

            //The primitive types are: Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Double, Single
            //We handle here Decimal types too
            if (memberType.IsPrimitive || memberType == typeof(decimal))
            {
                TypeCode primitiveTypeCode = Type.GetTypeCode(memberType);
                switch (primitiveTypeCode)
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
                        {
                            #region Primitive numeric
                            Label labelCheckInt = emit.ILGenerator.DefineLabel();
                            Label labelNotNumber = emit.ILGenerator.DefineLabel();
                            Label labelExit = emit.ILGenerator.DefineLabel();

                            //if( typeCode == AMF0TypeCode.Number )
                            emit
                                .ldloc_1 //Push 'typeCode'
                                .ldc_i4(AMF3TypeCode.Number)
                                .ceq
                                .brfalse_s(labelCheckInt)
                                //instance.{0} = ({1})reader.ReadDouble();
                                .ldloc_0 //Push 'instance'
                                .ldarg_0 //Push 'reader'
                                .callvirt(typeof(AMFReader).GetMethod("ReadDouble"))
                                .GeneratePrimitiveCast(primitiveTypeCode)
                                .GenerateSetMember(memberInfo)
                                .br_s(labelExit)
                                .MarkLabel(labelCheckInt)
                                .ldloc_1 //Push 'typeCode'
                                .ldc_i4(AMF3TypeCode.Integer)
                                .ceq
                                .brfalse_s(labelNotNumber)
                                //instance.{0} = ({1})reader.ReadAMF3Int();
                                .ldloc_0 //Push 'instance'
                                .ldarg_0 //Push 'reader'
                                .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Int"))
                                .GeneratePrimitiveCast(primitiveTypeCode)
                                .GenerateSetMember(memberInfo)
                                .br_s(labelExit)
                                .MarkLabel(labelNotNumber)
                                .end()
                            ;
                            if (DoTypeCheck())
                            {
                                emit
                                    .GenerateThrowUnexpectedAMFException(memberInfo)
                                    .MarkLabel(labelExit)
                                    //.nop
                                    .end()
                                ;
                            }
                            else
                            {
                                emit
                                    .ldarg_0 //Push 'reader'
                                    .ldloc_1 //Push 'typeCode'
                                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                                    .pop
                                    .MarkLabel(labelExit)
                                    .end()
                                ;
                            }
                            #endregion Primitive numeric
                        }
                        break;
                    case TypeCode.Boolean:
                        {
                            #region Primitive boolean
                            Label labelCheckBooleanTrue = emit.ILGenerator.DefineLabel();
                            Label labelNotBoolean = emit.ILGenerator.DefineLabel();
                            Label labelExit = emit.ILGenerator.DefineLabel();

                            //if( typeCode == AMF3TypeCode.BooleanFalse )
                            emit
                                .ldloc_1 //Push 'typeCode'
                                .ldc_i4(AMF3TypeCode.BooleanFalse)
                                .ceq
                                .brfalse_s(labelCheckBooleanTrue)
                                //instance.{0} = false;
                                .ldloc_0 //Push 'instance'
                                .ldc_i4_0
                                .GenerateSetMember(memberInfo)
                                .br_s(labelExit)
                                .MarkLabel(labelCheckBooleanTrue)
                                .ldloc_1 //Push 'typeCode'
                                .ldc_i4(AMF3TypeCode.BooleanTrue)
                                .ceq
                                .brfalse_s(labelNotBoolean)
                                //instance.{0} = true;
                                .ldloc_0 //Push 'instance'
                                .ldc_i4_1
                                .GenerateSetMember(memberInfo)
                                .br_s(labelExit)
                                .MarkLabel(labelNotBoolean)
                                .end()
                            ;
                            if (DoTypeCheck())
                            {
                                emit
                                    .GenerateThrowUnexpectedAMFException(memberInfo)
                                    .MarkLabel(labelExit)
                                    //.nop
                                    .end()
                                ;
                            }
                            else
                            {
                                emit
                                    .ldarg_0 //Push 'reader'
                                    .ldloc_1 //Push 'typeCode'
                                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                                    .pop
                                    .MarkLabel(labelExit)
                                    .end()
                                ;
                            }
                            #endregion Primitive boolean
                        }
                        break;
                    case TypeCode.Char:
                        {
                            #region Primitive Char
                            {
                                Label labelNotString = emit.ILGenerator.DefineLabel();
                                Label labelExit = emit.ILGenerator.DefineLabel();
                                //if( typeCode == AMF3TypeCode.String )
                                emit
                                    .ldloc_1 //Push 'typeCode'
                                    .ldc_i4(AMF3TypeCode.String)
                                    .ceq
                                    .brfalse_s(labelNotString)
                                    //instance.member = reader.ReadAMF3String()[0];
                                    .ldarg_0 //Push 'reader'
                                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3String"))
                                    .stloc_2
                                    .ldloc_2 //Push 'key'
                                    .brfalse_s(labelNotString) // Branch if 'key' is null
                                    .ldloc_2 //Push strTmp
                                    .ldsfld(typeof(string).GetField("Empty"))
                                    .call(typeof(string).GetMethod("op_Inequality", new Type[] { typeof(string), typeof(string) }))
                                    .brfalse_s(labelNotString)
                                    .ldloc_0 //Push 'instance'
                                    .ldloc_2 //Push 'key'
                                    .ldc_i4_0 //Push char index 0
                                    .callvirt(typeof(string).GetMethod("get_Chars", new Type[] { typeof(Int32) }))
                                    .GenerateSetMember(memberInfo)
                                    .br_s(labelExit)
                                    .MarkLabel(labelNotString)
                                    .end()
                                ;
                                if (DoTypeCheck())
                                {
                                    emit
                                        .GenerateThrowUnexpectedAMFException(memberInfo)
                                        .MarkLabel(labelExit)
                                        //.nop
                                        .end()
                                    ;
                                }
                                else
                                {
                                    emit
                                        .ldarg_0 //Push 'reader'
                                        .ldloc_1 //Push 'typeCode'
                                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                                        .pop
                                        .MarkLabel(labelExit)
                                        .end()
                                    ;
                                }
                            }
                            #endregion Primitive Char
                        }
                        break;
                }
                return;
            }
            if (memberType.IsEnum)
            {
                #region Enum
                Label labelNotInteger = emit.ILGenerator.DefineLabel();
                Label labelNotString = emit.ILGenerator.DefineLabel();
                Label labelExit = emit.ILGenerator.DefineLabel();
                //if (typeCode == AMF3TypeCode.String || typeCode == AMF3TypeCode.Integer)
                emit
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.Integer)
                    .ceq
                    .brfalse_s(labelNotInteger)
                    //instance.{0} = ({1})Enum.ToObject(typeof({1}), reader.ReadAMF3Int());
                    .ldloc_0 //Push 'instance'
                    .ldtoken(memberType)
                    .call(typeof(Type).GetMethod("GetTypeFromHandle"))
                    .ldarg_0 //Push 'reader'
                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Int"))
                    .call(typeof(Enum).GetMethod("ToObject", new Type[] { typeof(Type), typeof(Int32) }))
                    .unbox_any(memberType)
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelNotInteger)
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.String)
                    .ceq
                    .brfalse_s(labelNotString)
                    //instance.{0} = ({1})Enum.Parse(typeof({1}), reader.ReadAMF3String(), true);
                    .ldloc_0 //Push 'instance'
                    .ldtoken(memberType)
                    .call(typeof(Type).GetMethod("GetTypeFromHandle"))
                    .ldarg_0 //Push 'reader'
                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3String"))
                    .ldc_i4_1
                    .call(typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }))
                    .unbox_any(memberType)
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelNotString)
                    .end()
                ;
                if (DoTypeCheck())
                {
                    emit
                        .GenerateThrowUnexpectedAMFException(memberInfo)
                        .MarkLabel(labelExit)
                        //.nop
                        .end()
                    ;
                }
                else
                {
                    emit
                        .ldarg_0 //Push 'reader'
                        .ldloc_1 //Push 'typeCode'
                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                        .pop
                        .MarkLabel(labelExit)
                        .end()
                    ;
                }
                return;
                #endregion Enum
            }
            if (memberType == typeof(DateTime))
            {
                #region DateTime
                Label labelNotDate = emit.ILGenerator.DefineLabel();
                Label labelExit = emit.ILGenerator.DefineLabel();
                //if( typeCode == AMF3TypeCode.DateTime )
                emit
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.DateTime)
                    .ceq
                    .brfalse_s(labelNotDate)
                    .ldloc_0 //Push 'instance'
                    .ldarg_0 //Push 'reader'
                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Date"))
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelNotDate)
                    .end()
                ;
                if (DoTypeCheck())
                {
                    emit
                        .GenerateThrowUnexpectedAMFException(memberInfo)
                        .MarkLabel(labelExit)
                        //.nop
                        .end()
                    ;
                }
                else
                {
                    emit
                        .ldarg_0 //Push 'reader'
                        .ldloc_1 //Push 'typeCode'
                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                        .pop
                        .MarkLabel(labelExit)
                        .end()
                    ;
                }
                return;
                #endregion DateTime
            }
            if (memberType == typeof(Guid))
            {
                #region Guid
                Label labelNotString = emit.ILGenerator.DefineLabel();
                Label labelExit = emit.ILGenerator.DefineLabel();
                emit
                    //if( typeCode == AMF3TypeCode.String )
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.String)
                    .ceq
                    .brfalse_s(labelNotString)
                    .ldloc_0 //Push 'instance'
                    .ldarg_0 //Push 'reader'
                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3String"))
                    .newobj(typeof(Guid).GetConstructor(EmitHelper.AnyVisibilityInstance, null, CallingConventions.HasThis, new Type[] { typeof(string) }, null))
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelNotString)
                    .end()
                ;
                if (DoTypeCheck())
                {
                    emit
                        .GenerateThrowUnexpectedAMFException(memberInfo)
                        .MarkLabel(labelExit)
                        //.nop
                        .end()
                    ;
                }
                else
                {
                    emit
                        .ldarg_0 //Push 'reader'
                        .ldloc_1 //Push 'typeCode'
                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                        .pop
                        .MarkLabel(labelExit)
                        .end()
                    ;
                }
                return;
                #endregion Guid
            }
            if (memberType.IsValueType)
            {
                //structs are not handled
                throw new FluorineException("Struct value types are not supported");
            }
            if (memberType == typeof(string))
            {
                #region String

                Label labelCheckNull = emit.ILGenerator.DefineLabel();
                Label labelCheckUndefined = emit.ILGenerator.DefineLabel();
                Label labelNotString = emit.ILGenerator.DefineLabel();
                Label labelExit = emit.ILGenerator.DefineLabel();

                emit
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.String)
                    .ceq
                    .brfalse_s(labelCheckNull)
                    //instance.{0} = reader.ReadAMF3String();
                    .ldloc_0 //Push 'instance'
                    .ldarg_0 //Push 'reader'
                    .callvirt(typeof(AMFReader).GetMethod("ReadAMF3String"))
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelCheckNull)
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.Null)
                    .ceq
                    .brfalse_s(labelCheckUndefined)
                    .ldloc_0 //Push 'instance'
                    .ldc_i4_0
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelCheckUndefined)
                    .ldloc_1 //Push 'typeCode'
                    .ldc_i4(AMF3TypeCode.Undefined)
                    .ceq
                    .brfalse_s(labelNotString)
                    .ldloc_0 //Push 'instance'
                    .ldc_i4_0
                    .GenerateSetMember(memberInfo)
                    .br_s(labelExit)
                    .MarkLabel(labelNotString)
                    .end()
                ;
                if (DoTypeCheck())
                {
                    emit
                        .GenerateThrowUnexpectedAMFException(memberInfo)
                        .MarkLabel(labelExit)
                        //.nop
                        .end()
                    ;
                }
                else
                {
                    emit
                        .ldarg_0 //Push 'reader'
                        .ldloc_1 //Push 'typeCode'
                        .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                        .pop
                        .MarkLabel(labelExit)
                        .end()
                    ;
                }
                return;

                #endregion String
            }
            //instance.member = (type)TypeHelper.ChangeType(reader.ReadAMF3Data(typeCode), typeof(member));
            emit
                .ldloc_0 //Push 'instance'
                .ldarg_0 //Push 'reader'
                .ldloc_1 //Push 'typeCode'
                .callvirt(typeof(AMFReader).GetMethod("ReadAMF3Data", new Type[] { typeof(byte) }))
                .ldtoken(memberType)
                .call(typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }))
                .call(typeof(TypeHelper).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }))
                .CastFromObject(memberType)
                .GenerateSetMember(memberInfo)
                .end()
            ;
        }

        #region IReflectionOptimizer Members

        public object CreateInstance()
        {
            return _createInstanceMethod();
        }

        public object ReadData(AMFReader reader, ClassDefinition classDefinition)
        {
            return _readDataMethod(reader, classDefinition);
        }

        #endregion
    }
}
