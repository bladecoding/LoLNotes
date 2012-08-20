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
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Util;

namespace FluorineFx.Messaging.Rtmp.Event
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class Unknown : BaseEvent
	{
		protected ByteBuffer	_data;

		public Unknown(ByteBuffer data):base(EventType.SYSTEM)
		{
			_dataType = Constants.TypeUnknown;
			_data = data;
		}

        public Unknown(byte dataType, ByteBuffer data)
            : base(EventType.SYSTEM)
        {
            _dataType = dataType;
            _data = data;
        }

        public Unknown(byte dataType, byte[] data)
            : this(dataType, ByteBuffer.Wrap(data))
        {
        }

		public ByteBuffer Data
		{
			get{ return _data; }
		}
	}
}
