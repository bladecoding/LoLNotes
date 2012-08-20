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

namespace FluorineFx.Messaging.Rtmp.Stream.Codec
{
    class VideoCodec
    {
        /// <summary>
        /// FLV frame key marker constant.
        /// </summary>
        public const byte FLV_FRAME_KEY = 0x10;

        public static VideoCodec Jpeg = new VideoCodec(0x01);
        public static VideoCodec H263 = new VideoCodec(0x02);
        public static VideoCodec ScreenVideo = new VideoCodec(0x03);
        public static VideoCodec VP6 = new VideoCodec(0x04);
        public static VideoCodec VP6a = new VideoCodec(0x05);
        public static VideoCodec ScreenVideo2 = new VideoCodec(0x06);
        public static VideoCodec AVC = new VideoCodec(0x07);
            
        readonly byte _id;

        /// <summary>
        /// Gets the numeric id for this codec, that happens to correspond to the numeric identifier that FLV will use for this codec.
        /// </summary>
        /// <value>The codec id.</value>
        public byte Id
        {
            get { return _id; }
        }

        private VideoCodec(byte id)
        {
            _id = id;
        }
    }
}
