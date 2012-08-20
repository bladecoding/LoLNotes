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

namespace FluorineFx.IO.Readers
{
    class AMF3IntVectorReader : IAMFReader
    {
        public AMF3IntVectorReader()
		{
		}

        #region IAMFReader Members

        public object ReadData(AMFReader reader)
        {
            return reader.ReadAMF3IntVector();
        }

        #endregion
    }

    class AMF3UIntVectorReader : IAMFReader
    {
        public AMF3UIntVectorReader()
        {
        }

        #region IAMFReader Members

        public object ReadData(AMFReader reader)
        {
            return reader.ReadAMF3UIntVector();
        }

        #endregion
    }

    class AMF3DoubleVectorReader : IAMFReader
    {
        public AMF3DoubleVectorReader()
        {
        }

        #region IAMFReader Members

        public object ReadData(AMFReader reader)
        {
            return reader.ReadAMF3DoubleVector();
        }

        #endregion
    }

    class AMF3ObjectVectorReader : IAMFReader
    {
        public AMF3ObjectVectorReader()
        {
        }

        #region IAMFReader Members

        public object ReadData(AMFReader reader)
        {
            return reader.ReadAMF3ObjectVector();
        }

        #endregion
    }
}
