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
using System.IO;
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO
{
    /// <summary>
    /// A Tag represents the contents or payload of a FLV file
    /// </summary>
    class Tag : ITag
    {
        /// <summary>
        /// Tag data type
        /// </summary>
        private byte _dataType;
        /// <summary>
        /// Timestamp
        /// </summary>
        private int _timestamp;
        /// <summary>
        /// Tag body as byte buffer
        /// </summary>
        private byte[] _body;
        /// <summary>
        /// Tag body size
        /// </summary>
        private int _bodySize;
        /// <summary>
        /// Previous tag size
        /// </summary>
        private int _previuosTagSize;
        /// <summary>
        /// Bit flags
        /// </summary>
        private byte _flags;

        public byte Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public Tag()
        {
        }

        public Tag(byte dataType, int timestamp, int bodySize, byte[] body, int previousTagSize)
        {
            _dataType = dataType;
            _timestamp = timestamp;
            _bodySize = bodySize;
            _previuosTagSize = previousTagSize;
            if (body != null)
            {
                _body = new byte[bodySize];
                Buffer.BlockCopy(body, 0, _body, 0, bodySize);
            }
        }

        #region ITag Members

        public byte[] Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
                _bodySize = _body.Length;
            }
        }

        public int BodySize
        {
            get
            {
                return _bodySize;
            }
            set
            {
                _bodySize = value;
            }
        }

        public byte DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        public int Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = value;
            }
        }

        public ByteBuffer Data
        {
            get { return null; }
        }

        public int PreviousTagSize
        {
            get
            {
                return _previuosTagSize;
            }
            set
            {
                _previuosTagSize = value;
            }
        }

        #endregion
    }
}
