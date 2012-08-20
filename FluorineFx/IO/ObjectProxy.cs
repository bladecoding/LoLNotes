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
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Text;
using log4net;
#endif
using FluorineFx.AMF3;
using FluorineFx.Configuration;
using FluorineFx.Util;
using FluorineFx.Exceptions;
using FluorineFx.Invocation;

namespace FluorineFx.IO
{
    class ObjectProxy : IObjectProxy
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(ObjectProxy));
#endif

        #region IObjectProxy Members

        public bool GetIsExternalizable(object instance)
        {
            return instance is IExternalizable;
        }

        public bool GetIsDynamic(object instance)
        {
            return instance is ASObject;
        }

        public virtual ClassDefinition GetClassDefinition(object instance)
        {
            ValidationUtils.ArgumentNotNull(instance, "instance");
            Type type = instance.GetType();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Creating class definition for typed {0}", type.FullName);
            sb.Append("{");

            List<string> memberNames = new List<string>();
            List<ClassMember> classMemberList = new List<ClassMember>();
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                string name = propertyInfo.Name;
                if (propertyInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    continue;
                if (propertyInfo.GetGetMethod() == null || propertyInfo.GetGetMethod().GetParameters().Length > 0)
                {
                    //The gateway will not be able to access this property
                    string msg = __Res.GetString(__Res.Reflection_PropertyIndexFail, string.Format("{0}.{1}", type.FullName, propertyInfo.Name));
#if !SILVERLIGHT
                    if (log.IsWarnEnabled)
                        log.Warn(msg);
#endif
                    continue;
                }
                if (memberNames.Contains(name))
                    continue;
                memberNames.Add(name);
                BindingFlags bf = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                try
                {
                    PropertyInfo propertyInfoTmp = type.GetProperty(name);
                    Unreferenced.Parameter(propertyInfoTmp);
                }
                catch (AmbiguousMatchException)
                {
                    bf = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;
                }
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                ClassMember classMember = new ClassMember(name, bf, propertyInfo.MemberType, attributes);
                classMemberList.Add(classMember);

                if (i != 0)
                    sb.Append(", ");
                sb.Append(name);
            }
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
#if !SILVERLIGHT
                if (fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0)
                    continue;
#endif
                if (fieldInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    continue;
                string name = fieldInfo.Name;
                object[] attributes = fieldInfo.GetCustomAttributes(false);
                ClassMember classMember = new ClassMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, fieldInfo.MemberType, attributes);
                classMemberList.Add(classMember);

                if (i != 0 && propertyInfos.Length > 0)
                    sb.Append(", ");
                sb.Append(name);
            }
            ClassMember[] classMembers = classMemberList.ToArray();
            string customClassName = type.FullName;
            customClassName = FluorineConfiguration.Instance.GetCustomClass(customClassName);
            ClassDefinition classDefinition = new ClassDefinition(customClassName, classMembers, GetIsExternalizable(instance), GetIsDynamic(instance));
            return classDefinition;
        }

        public virtual object GetValue(object instance, ClassMember member)
        {
            ValidationUtils.ArgumentNotNull(instance, "instance");
            Type type = instance.GetType();
            if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = type.GetProperty(member.Name, member.BindingFlags);
                object value = propertyInfo.GetValue(instance, null);
                value = HandleAttributes(instance, propertyInfo, value, member.CustomAttributes);
                return value;
            }
            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = type.GetField(member.Name, member.BindingFlags);
                object value = fieldInfo.GetValue(instance);
                value = HandleAttributes(instance, fieldInfo, value, member.CustomAttributes);
                return value;
            }
            string msg = __Res.GetString(__Res.Reflection_MemberNotFound, string.Format("{0}.{1}", type.FullName, member.Name));
            throw new FluorineException(msg);
        }

        protected object HandleAttributes(object instance, MemberInfo memberInfo, object result, object[] attributes)
        {
            if (attributes != null && attributes.Length > 0)
            {
                InvocationManager invocationManager = new InvocationManager();
                invocationManager.Result = result;
                for (int i = 0; i < attributes.Length; i++)
                {
                    Attribute attribute = attributes[i] as Attribute;
                    if (attribute is IInvocationCallback)
                    {
                        IInvocationCallback invocationCallback = attribute as IInvocationCallback;
                        invocationCallback.OnInvoked(invocationManager, memberInfo, instance, null, result);
                    }
                }
                for (int i = 0; i < attributes.Length; i++)
                {
                    Attribute attribute = attributes[i] as Attribute;
                    if (attribute is IInvocationResultHandler)
                    {
                        IInvocationResultHandler invocationResultHandler = attribute as IInvocationResultHandler;
                        invocationResultHandler.HandleResult(invocationManager, memberInfo, instance, null, result);
                    }
                }
                return invocationManager.Result;
            }
            return result;
        }

        public virtual void SetValue(object instance, ClassMember member, object value)
        {
            ValidationUtils.ArgumentNotNull(instance, "instance");
            Type type = instance.GetType();
            if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = type.GetProperty(member.Name, member.BindingFlags);
                propertyInfo.SetValue(instance, value, null);
            }
            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = type.GetField(member.Name, member.BindingFlags);
                fieldInfo.SetValue(instance, value);
            }
            string msg = __Res.GetString(__Res.Reflection_MemberNotFound, string.Format("{0}.{1}", type.FullName, member.Name));
            throw new FluorineException(msg);
        }

        #endregion

    }
}
