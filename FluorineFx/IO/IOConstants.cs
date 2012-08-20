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

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class IOConstants
	{
		/// <summary>
		/// Video data
		/// </summary>
		public static byte TYPE_VIDEO = 0x09;
		/// <summary>
		/// Audio data
		/// </summary>
		public static byte TYPE_AUDIO = 0x08;
		/// <summary>
		/// Metadata
		/// </summary>
		public static byte TYPE_METADATA = 0x12;
		/// <summary>
		/// Mask sound type
		/// </summary>
		public static byte MASK_SOUND_TYPE = 0x01;
		/// <summary>
		/// Mono mode
		/// </summary>
		public static byte FLAG_TYPE_MONO = 0x00;
		/// <summary>
		/// Stereo mode
		/// </summary>
		public static byte FLAG_TYPE_STEREO = 0x01;
		/// <summary>
		/// Mask sound size
		/// </summary>
		public static byte MASK_SOUND_SIZE = 0x02;
		/// <summary>
		/// 8 bit flag size
		/// </summary>
		public static byte FLAG_SIZE_8_BIT = 0x00;
		/// <summary>
		/// 16 bit flag size
		/// </summary>
		public static byte FLAG_SIZE_16_BIT = 0x01;
		/// <summary>
		/// Mask sound rate
		/// </summary>
		public static byte MASK_SOUND_RATE = 0x0C;
		/// <summary>
		/// 5.5 KHz rate flag
		/// </summary>
		public static byte FLAG_RATE_5_5_KHZ = 0x00;
		/// <summary>
		/// 11 KHz rate flag
		/// </summary>
		public static byte FLAG_RATE_11_KHZ = 0x01;
		/// <summary>
		/// 22 KHz rate flag
		/// </summary>
		public static byte FLAG_RATE_22_KHZ = 0x02;
		/// <summary>
		/// 44 KHz rate flag
		/// </summary>
		public static byte FLAG_RATE_44_KHZ = 0x03;
		/// <summary>
		/// Mask sound format (unsigned)
		/// </summary>
		public static sbyte MASK_SOUND_FORMAT = 0xF0 - 0xFF; // unsigned 
		/// <summary>
		/// Raw data format flag
		/// </summary>
		public static byte FLAG_FORMAT_RAW = 0x00;
		/// <summary>
		/// ADPCM format flag
		/// </summary>
		public static byte FLAG_FORMAT_ADPCM = 0x01;
		/// <summary>
		/// MP3 format flag
		/// </summary>
		public static byte FLAG_FORMAT_MP3 = 0x02;
		/// <summary>
		/// 8 KHz NellyMoser audio format flag
		/// </summary>
		public static byte FLAG_FORMAT_NELLYMOSER_8_KHZ = 0x05;
		/// <summary>
		/// NellyMoser-encoded audio format flag
		/// </summary>
		public static byte FLAG_FORMAT_NELLYMOSER = 0x06;
		/// <summary>
		/// Mask video codec
		/// </summary>
		public static byte MASK_VIDEO_CODEC = 0x0F;
		/// <summary>
		/// H263 codec flag
		/// </summary>
		public static byte FLAG_CODEC_H263 = 0x02;
		/// <summary>
		/// Screen codec flag
		/// </summary>
		public static byte FLAG_CODEC_SCREEN = 0x03;
		/// <summary>
		/// On2 VP6 codec flag
		/// </summary>
		public static byte FLAG_CODEC_VP6 = 0x04;
		/// <summary>
		/// Video frametype flag
		/// </summary>
		public static sbyte MASK_VIDEO_FRAMETYPE = 0xF0 - 0xFF; // unsigned 
		/// <summary>
		/// Keyframe type flag
		/// </summary>
		public static byte FLAG_FRAMETYPE_KEYFRAME = 0x01;
		/// <summary>
		/// Interframe flag. Interframes are created from keyframes rather than independent image
		/// </summary>
		public static byte FLAG_FRAMETYPE_INTERFRAME = 0x02;
		/// <summary>
		/// Disposable frame type flag
		/// </summary>
		public static byte FLAG_FRAMETYPE_DISPOSABLE = 0x03;
	}
}
