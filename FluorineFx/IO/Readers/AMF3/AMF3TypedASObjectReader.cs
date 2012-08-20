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

namespace FluorineFx.IO.Readers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	/// <remarks>
	/// This reader is used only in case that a requested type is not found and the gateway choose to represent 
	/// typed objects with ActionScript typed objects.
	/// </remarks>
    class AMF3TypedASObjectReader : IReflectionOptimizer
	{
		string _typeIdentifier;

		public AMF3TypedASObjectReader(string typeIdentifier)
		{
			_typeIdentifier = typeIdentifier;
		}

        #region IReflectionOptimizer Members

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        public object ReadData(AMFReader reader, ClassDefinition classDefinition)
        {
            ASObject aso = new ASObject(_typeIdentifier);
            reader.AddAMF3ObjectReference(aso);
            string key = reader.ReadAMF3String();
            aso.TypeName = _typeIdentifier;
            while (key != string.Empty)
            {
                object value = reader.ReadAMF3Data();
                aso.Add(key, value);
                key = reader.ReadAMF3String();
            }
            return aso;
        }

        #endregion
    }
}
