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
using System.Reflection;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.AMF3;
using FluorineFx.Configuration;
using FluorineFx.Util;
using FluorineFx.Exceptions;

namespace FluorineFx.IO
{
#if WCF
    class WcfProxy : ObjectProxy
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(WcfProxy));
#endif

        static bool IsDataContract(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(System.Runtime.Serialization.DataContractAttribute), false);
            return attributes.Length == 1;
        }

        #region IObjectProxy Members

        public override ClassDefinition GetClassDefinition(object instance)
        {
            ClassDefinition classDefinition = null;
            Type type = instance.GetType();
            //Verify [DataContract] or [Serializable] on type
            bool serializable = IsDataContract(type) || type.IsSerializable;
            if (!serializable && type.Assembly != typeof(AMFWriter).Assembly)
                throw new FluorineException(string.Format("Type {0} was not marked as a data contract.", type.FullName));
            List<string> memberNames = new List<string>();
            List<ClassMember> classMemberList = new List<ClassMember>();
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i] as PropertyInfo;
                string name = propertyInfo.Name;
                if (propertyInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    continue;
                if (propertyInfo.GetGetMethod() == null || propertyInfo.GetGetMethod().GetParameters().Length > 0)
                {
                    //The gateway will not be able to access this property
                    string msg = __Res.GetString(__Res.Reflection_PropertyIndexFail, string.Format("{0}.{1}", type.FullName, propertyInfo.Name));
                    if (log.IsWarnEnabled)
                        log.Warn(msg);
                    continue;
                }
                object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    System.Runtime.Serialization.DataMemberAttribute attribute = (System.Runtime.Serialization.DataMemberAttribute)customAttributes[0];
                    if (attribute.Name != null && attribute.Name.Length != 0)
                        name = attribute.Name;
                }
                else
                {
                    if (!type.IsSerializable && type.Assembly != typeof(AMFWriter).Assembly)
                        continue;
                }
                if (memberNames.Contains(name))
                    continue;
                memberNames.Add(name);
                BindingFlags bf = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                try
                {
                    PropertyInfo propertyInfoTmp = type.GetProperty(name);
                }
                catch (AmbiguousMatchException)
                {
                    bf = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance;
                }
                object[] attributes = propertyInfo.GetCustomAttributes(false);
                ClassMember classMember = new ClassMember(name, bf, propertyInfo.MemberType, attributes);
                classMemberList.Add(classMember);
            }
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i] as FieldInfo;
                if (fieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0)
                    continue;
                if (fieldInfo.GetCustomAttributes(typeof(TransientAttribute), true).Length > 0)
                    continue;
                string name = fieldInfo.Name;
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    System.Runtime.Serialization.DataMemberAttribute attribute = (System.Runtime.Serialization.DataMemberAttribute)customAttributes[0];
                    if (attribute.Name != null && attribute.Name.Length != 0)
                        name = attribute.Name;
                }
                else
                {
                    if (!type.IsSerializable && type.Assembly != typeof(AMFWriter).Assembly)
                        continue;
                }
                object[] attributes = fieldInfo.GetCustomAttributes(false);
                ClassMember classMember = new ClassMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, fieldInfo.MemberType, attributes);
                classMemberList.Add(classMember);
            }
            ClassMember[] classMembers = classMemberList.ToArray();
            string customClassName = type.FullName;
            customClassName = FluorineConfiguration.Instance.GetCustomClass(customClassName);
            classDefinition = new ClassDefinition(customClassName, classMembers, GetIsExternalizable(instance), GetIsDynamic(instance));
            return classDefinition;
        }

        #endregion
    }
#endif
}
