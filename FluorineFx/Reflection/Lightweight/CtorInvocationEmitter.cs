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
using System.Reflection;
using System.Reflection.Emit;
using FluorineFx.Util;

namespace FluorineFx.Reflection.Lightweight
{
    /// <summary>
    /// Constructor emitter.
    /// </summary>
    class CtorInvocationEmitter : InvocationEmitter
    {
        public CtorInvocationEmitter(ConstructorInfo ctorInfo, BindingFlags bindingFlags)
            : this(ctorInfo.DeclaringType, bindingFlags, ReflectionUtils.ToTypeArray(ctorInfo.GetParameters()), ctorInfo) 
        {
        }

        public CtorInvocationEmitter(Type targetType, BindingFlags bindingFlags, Type[] paramTypes)
            : this(targetType, bindingFlags, paramTypes, null) 
        { 
        }

        private CtorInvocationEmitter(Type targetType, BindingFlags flags, Type[] parameterTypes, ConstructorInfo ctorInfo)
            : base(targetType, flags, MemberTypes.Constructor, parameterTypes, ctorInfo)
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod("ctor", TargetType, typeof(object), new Type[] { typeof(object) });
        }

        protected internal override Delegate CreateDelegate()
        {
            if (ReflectionUtils.IsTargetTypeStruct(TargetType) && ReflectionUtils.IsEmptyTypeList(ParameterTypes))
            {
                // No-arg struct needs special initialization
                Emit.DeclareLocal(TargetType);      // TargetType tmp
                Emit.ldloca_s(0)                    // &tmp
                    .initobj(TargetType)            // init_obj(&tmp)
                    .ldloc_0.end();                 // load tmp
            }
            else if (TargetType.IsArray)
            {
                Emit.ldarg_0                                  // load args[] (method arguments)
                    .ldc_i4_0                                 // load 0
                    .ldelem_ref                               // load args[0] (length)
                    .unbox_any(typeof(int))                   // unbox stack
                    .newarr(TargetType.GetElementType());     // new T[args[0]]
            }
            else
            {
                ConstructorInfo ctorInfo = TargetType.GetConstructor(Flags, null, ParameterTypes, null);
                byte startUsableLocalIndex = 0;
                if (ReflectionUtils.HasRefParam(ParameterTypes))
                {
                    startUsableLocalIndex = CreateLocalsForByRefParams(0, ctorInfo); // create by_ref_locals from argument array
                    Emit.DeclareLocal(TargetType);                    // TargetType tmp;
                }

                PushParamsOrLocalsToStack(0);               // push arguments and by_ref_locals
                Emit.newobj(ctorInfo);                      // ctor (<stack>)

                if (ReflectionUtils.HasRefParam(ParameterTypes))
                {
                    Emit.stloc(startUsableLocalIndex);      // tmp = <stack>;
                    AssignByRefParamsToArray(0);            // store by_ref_locals back to argument array
                    Emit.ldloc(startUsableLocalIndex);      // tmp
                }
            }
            Emit.boxIfValueType(TargetType)
                .ret();                                // return (box)<stack>;
            return Method.CreateDelegate(typeof(ConstructorInvoker));            
        }
    }
}
