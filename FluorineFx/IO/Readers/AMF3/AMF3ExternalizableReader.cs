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
using FluorineFx.AMF3;
using FluorineFx.IO.Bytecode;
using FluorineFx.Exceptions;

namespace FluorineFx.IO.Readers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    class AMF3ExternalizableReader : IReflectionOptimizer
	{

        public AMF3ExternalizableReader()
		{
		}

        #region IReflectionOptimizer Members

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        public object ReadData(AMFReader reader, ClassDefinition classDefinition)
        {
            object instance = ObjectFactory.CreateInstance(classDefinition.ClassName);
            if (instance == null)
            {
                string msg = __Res.GetString(__Res.Type_InitError, classDefinition.ClassName);
                throw new FluorineException(msg);
            }
            reader.AddAMF3ObjectReference(instance);
            if (instance is IExternalizable)
            {
                IExternalizable externalizable = instance as IExternalizable;
                DataInput dataInput = new DataInput(reader);
                externalizable.ReadExternal(dataInput);
                return instance;
            }
            else
            {
                string msg = __Res.GetString(__Res.Externalizable_CastFail, instance.GetType().FullName);
                throw new FluorineException(msg);
            }
        }

        #endregion
    }
}
