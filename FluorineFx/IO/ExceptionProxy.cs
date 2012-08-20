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
using System.Collections;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx.AMF3;
using FluorineFx.Configuration;

namespace FluorineFx.IO
{
    class ExceptionProxy : ObjectProxy
    {
        public override ClassDefinition GetClassDefinition(object instance)
        {
            Type type = instance.GetType();
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] propertyInfos = type.GetProperties(bindingAttr);
#if !(NET_1_1)
            List<ClassMember> classMemberList = new List<ClassMember>();
#else
            ArrayList classMemberList = new ArrayList();
#endif
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                if (propertyInfo.Name != "HelpLink" && propertyInfo.Name != "TargetSite")
                {
                    ClassMember classMember = new ClassMember(propertyInfo.Name, bindingAttr, propertyInfo.MemberType, null);
                    classMemberList.Add(classMember);
                }
            }
            string customClassName = type.FullName;
            customClassName = FluorineConfiguration.Instance.GetCustomClass(customClassName);
#if !(NET_1_1)
            ClassMember[] classMembers = classMemberList.ToArray();
#else
			ClassMember[] classMembers = classMemberList.ToArray(typeof(ClassMember)) as ClassMember[];
#endif
			ClassDefinition classDefinition = new ClassDefinition(customClassName, classMembers, GetIsExternalizable(instance), GetIsDynamic(instance));
            return classDefinition;
        }
    }
}
