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

namespace FluorineFx.Reflection.Lightweight
{
    /// <summary>
    /// Base class for invocation emitters.
    /// </summary>
    abstract class InvocationEmitter : AbstractEmitter
    {
        protected InvocationEmitter(Type targetType, BindingFlags flags, MemberTypes memberTypes, Type[] parameterTypes, MemberInfo memberInfo)
            : base(targetType, flags, memberTypes, parameterTypes, memberInfo)
        {
        }

        protected byte CreateLocalsForByRefParams(byte paramArrayIndex, MethodBase invocationInfo)
        {
            byte numberOfByRefParams = 0;
            ParameterInfo[] parameters = invocationInfo.GetParameters();
            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                Type paramType = ParameterTypes[i];
                if (paramType.IsByRef)
                {
                    Type type = paramType.GetElementType();
                    Emit.DeclareLocal(type);
                    if (!parameters[i].IsOut) // no initialization necessary is 'out' parameter
                    {
                        Emit.ldarg(paramArrayIndex)
                            .ldc_i4(i)
                            .ldelem_ref
                            .CastFromObject(type)
                            .stloc(numberOfByRefParams)
                            .end();
                    }
                    numberOfByRefParams++;
                }
            }
            return numberOfByRefParams;
        }

        protected void PushParamsOrLocalsToStack(int paramArrayIndex)
        {
            byte currentByRefParam = 0;
            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                Type paramType = ParameterTypes[i];
                if (paramType.IsByRef)
                {
                    Emit.ldloca_s(currentByRefParam++);
                }
                else
                {
                    Emit.ldarg(paramArrayIndex)
                        .ldc_i4(i)
                        .ldelem_ref
                        .CastFromObject(paramType);
                }
            }
        }

        protected void AssignByRefParamsToArray(int paramArrayIndex)
        {
            byte currentByRefParam = 0;
            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                Type paramType = ParameterTypes[i];
                if (paramType.IsByRef)
                {
                    Emit.ldarg(paramArrayIndex)
                        .ldc_i4(i)
                        .ldloc(currentByRefParam++)
                        .boxIfValueType(paramType.GetElementType())
                        .stelem_ref
                        .end();
                }
            }
        }
    }
}
