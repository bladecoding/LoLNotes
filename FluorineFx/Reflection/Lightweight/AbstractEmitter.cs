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

namespace FluorineFx.Reflection.Lightweight
{
    /// <summary>
    /// Base class for reflection emitters.
    /// </summary>
    abstract class AbstractEmitter
    {
        private DynamicMethod _method;
        private EmitHelper _emit;
        private readonly Type _targetType;
        private readonly BindingFlags _bindingFlags;
        private readonly MemberTypes _memberTypes;
        private readonly Type[] _parameterTypes;
        private readonly MemberInfo _memberInfo;

        protected AbstractEmitter(Type targetType, BindingFlags bindingFlags, MemberTypes memberTypes, Type[] parameterTypes, MemberInfo memberInfo)
        {
            _targetType = targetType;
            _bindingFlags = bindingFlags;
            _memberTypes = memberTypes;
            _parameterTypes = parameterTypes;
            _memberInfo = memberInfo;
        }

        public MemberInfo MemberInfo
        {
            get { return _memberInfo; }
        }

        public MemberTypes MemberTypes
        {
            get { return _memberTypes; }
        }

        public BindingFlags Flags
        {
            get { return _bindingFlags; }
        }

        public EmitHelper Emit
        {
            get { return _emit; }
        }

        public Type[] ParameterTypes
        {
            get { return _parameterTypes; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }

        public DynamicMethod Method
        {
            get { return _method; }
        }

        internal Delegate GetDelegate()
        {
            _method = CreateDynamicMethod();
            _emit = new EmitHelper(Method.GetILGenerator());
            Delegate @delegate = CreateDelegate();
            return @delegate;
        }

        protected internal abstract DynamicMethod CreateDynamicMethod();


        protected internal static DynamicMethod CreateDynamicMethod(string name, Type targetType, Type returnType, Type[] paramTypes)
        {
            return new DynamicMethod(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, paramTypes,
                targetType.IsArray ? targetType.GetElementType() : targetType, true);
        }

        protected internal abstract Delegate CreateDelegate();
    }
}
