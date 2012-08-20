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

using FluorineFx.Exceptions;

namespace FluorineFx.IO.Readers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMFUnknownTagReader : IAMFReader
	{
		public AMFUnknownTagReader()
		{
		}

		#region IAMFReader Members

		public object ReadData(AMFReader reader)
		{
			throw new UnexpectedAMF();
		}

		#endregion
	}

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class MovieclipMarker : IAMFReader
    {
        public MovieclipMarker()
		{
		}

		#region IAMFReader Members

		public object ReadData(AMFReader reader)
		{
			throw new UnexpectedAMF();
		}

		#endregion
    }

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class UnsupportedMarker : IAMFReader
    {
        public UnsupportedMarker()
		{
		}

		#region IAMFReader Members

		public object ReadData(AMFReader reader)
		{
			throw new UnexpectedAMF();
		}

		#endregion
    }
}
