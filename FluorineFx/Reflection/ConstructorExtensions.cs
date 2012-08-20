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
using FluorineFx.Reflection.Lightweight;
using FluorineFx.Util;

namespace FluorineFx.Reflection
{
    /// <summary>
    /// Type instantiation helper.
    /// </summary>
    static class ConstructorExtensions
    {
        /// <summary>
        /// Invokes a constructor whose parameter types are inferred from <paramref name="parameters" /> 
        /// on the given <paramref name="type"/> with <paramref name="parameters" /> being the arguments.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="parameters">An array of arguments that match in number, order, and type the parameters of the constructor to invoke.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(Type type, params object[] parameters )
        {
            return DelegateForCreateInstance( type, ReflectionUtils.ToTypeArray(parameters))( parameters );
        }

        /// <summary>
        /// Creates a delegate which can invoke the constructor whose parameter types are <paramref name="parameterTypes" /> on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="parameterTypes">An array of argument types that match in number, order, and type the parameters of the constructor to invoke.</param>
        /// <returns>A delegate for constructor invocation.</returns>
        public static ConstructorInvoker DelegateForCreateInstance(Type type, params Type[] parameterTypes )
        {
            return (ConstructorInvoker)new CtorInvocationEmitter(type, BindingFlags.CreateInstance|BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static, parameterTypes).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can invoke the constructor whose parameter types are inferred from <paramref name="parameters" /> on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="parameters">An array of arguments that match in number, order, and type the parameters of the constructor to invoke.</param>
        /// <returns>A delegate for constructor invocation.</returns>
        public static ConstructorInvoker DelegateForCreateInstance(Type type, params object[] parameters)
        {
            return DelegateForCreateInstance(type, ReflectionUtils.ToTypeArray(parameters));
        }
    }
}
