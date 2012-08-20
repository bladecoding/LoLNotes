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
using System.IO;
#if !SILVERLIGHT
using log4net;
#endif

namespace FluorineFx.IO.FLV
{
    class FlvHeader
    {
        /// <summary>
        /// FLV version
        /// </summary>
        private byte _version = 0x00; //version 1

        public byte Version
        {
            get { return _version; }
            set { _version = value; }
        }
        /// <summary>
        /// Reserved flag, one
        /// </summary>
        private byte _flagReserved01 = 0x00;

        public byte FlagReserved01
        {
            get { return _flagReserved01; }
            set { _flagReserved01 = value; }
        }
        /// <summary>
        /// Audio flag
        /// </summary>
        private bool _flagAudio;

        public bool FlagAudio
        {
            get { return _flagAudio; }
            set { _flagAudio = value; }
        }
        /// <summary>
        /// Reserved flag, two
        /// </summary>
        private byte _flagReserved02 = 0x00;

        public byte FlagReserved02
        {
            get { return _flagReserved02; }
            set { _flagReserved02 = value; }
        }
        /// <summary>
        /// Video flag
        /// </summary>
        private bool _flagVideo;

        public bool FlagVideo
        {
            get { return _flagVideo; }
            set { _flagVideo = value; }
        }
        /// <summary>
        /// DATA OFFSET reserved for data up to 4,294,967,295
        /// </summary>
        private int _dataOffset = 0x00;

        public int DataOffset
        {
            get { return _dataOffset; }
            set { _dataOffset = value; }
        }

        /// <summary>
        /// Sets the type flags on whether this data is audio or video.
        /// </summary>
        /// <param name="typeFlags">Type flags determining data types (audio or video).</param>
        internal void SetTypeFlags(byte typeFlags)
        {
            _flagVideo = (((byte)(((typeFlags << 0x7) >> 0x7) & 0x01)) > 0x00);
            _flagAudio = (((byte)(((typeFlags << 0x5) >> 0x7) & 0x01)) > 0x00);
        }

        public override string ToString()
        {
            string ret = "";
            ret += "version: " + this.Version + " \n";
            ret += "type flags video: " + this.FlagVideo + " \n";
            ret += "type flags audio: " + this.FlagAudio + " \n";
            ret += "data offset: " + this.DataOffset + "\n";
            return ret;
        }
    }
}
