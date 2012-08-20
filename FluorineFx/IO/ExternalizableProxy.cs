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
    class ExternalizableProxy : IObjectProxy
    {
        #region IObjectProxy Members

        public bool GetIsExternalizable(object instance)
        {
            return true;
        }

        public bool GetIsDynamic(object instance)
        {
            return false;
        }

        public ClassDefinition GetClassDefinition(object instance)
        {
            Type type = instance.GetType();
            string customClassName = type.FullName;
            customClassName = FluorineConfiguration.Instance.GetCustomClass(customClassName);
            ClassDefinition classDefinition = new ClassDefinition(customClassName, ClassDefinition.EmptyClassMembers, true, false);
            return classDefinition;
        }

        public object GetValue(object instance, ClassMember member)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object instance, ClassMember member, object value)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
