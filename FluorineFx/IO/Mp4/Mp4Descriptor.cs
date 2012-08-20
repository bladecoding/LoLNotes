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
using System.Collections;
using System.Collections.Generic;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

/**
 This software module was originally developed by Apple Computer, Inc.
 in the course of development of MPEG-4. 
 This software module is an implementation of a part of one or 
 more MPEG-4 tools as specified by MPEG-4. 
 ISO/IEC gives users of MPEG-4 free license to this
 software module or modifications thereof for use in hardware 
 or software products claiming conformance to MPEG-4.
 Those intending to use this software module in hardware or software
 products are advised that its use may infringe existing patents.
 The original developer of this software module and his/her company,
 the subsequent editors and their companies, and ISO/IEC have no
 liability for use of this software module or modifications thereof
 in an implementation.
 Copyright is not released for non MPEG-4 conforming
 products. Apple Computer, Inc. retains full right to use the code for its own
 purpose, assign or donate the code to a third party and to
 inhibit third parties from using the code for non 
 MPEG-4 conforming products.
 This copyright notice must be included in all copies or
 derivative works. Copyright (c) 1999, 2000.
 */

namespace FluorineFx.IO.Mp4
{
    class Mp4Descriptor
    {
        public const int MP4ES_DescriptorTag = 3;
        public const int MP4DecoderConfigDescriptorTag = 4;
        public const int MP4DecSpecificInfoDescriptorTag = 5;

        protected int _type;
        protected int _size;
        protected int _bytesRead;

        protected List<Mp4Descriptor> _children = new List<Mp4Descriptor>();

        public Mp4Descriptor(int type, int size)
        {
            _bytesRead = 0;
            _type = type;
            _size = size;
        }

        public static Mp4Descriptor CreateDescriptor(Mp4DataStream bitstream)
        {
            int tag = (int)bitstream.ReadBytes(1);
            int bytesRead = 1;
            int size = 0;
            int b = 0;
            do
            {
                b = (int)bitstream.ReadBytes(1);
                size <<= 7;
                size |= b & 0x7f;
                bytesRead++;
            } while ((b & 0x80) == 0x80);
            Mp4Descriptor descriptor = new Mp4Descriptor(tag, size);
            switch (tag)
            {
                case MP4ES_DescriptorTag:
                    descriptor.CreateESDescriptor(bitstream);
                    break;
                case MP4DecoderConfigDescriptorTag:
                    descriptor.CreateDecoderConfigDescriptor(bitstream);
                    break;
                case MP4DecSpecificInfoDescriptorTag:
                    descriptor.CreateDecSpecificInfoDescriptor(bitstream);
                    break;
                default:
                    break;
            }
            bitstream.SkipBytes(descriptor._size - descriptor._bytesRead);
            descriptor._bytesRead = bytesRead + descriptor._size;
            return descriptor;
        }

        /// <summary>
        /// Loads the MP4ES_Descriptor from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        public void CreateESDescriptor(Mp4DataStream bitstream)
        {
            int ES_ID = (int)bitstream.ReadBytes(2);
            int flags = (int)bitstream.ReadBytes(1);
            bool streamDependenceFlag = (flags & (1 << 7)) != 0;
            bool urlFlag = (flags & (1 << 6)) != 0;
            bool ocrFlag = (flags & (1 << 5)) != 0;
            _bytesRead += 3;
            if (streamDependenceFlag)
            {
                bitstream.SkipBytes(2);
                _bytesRead += 2;
            }
            if (urlFlag)
            {
                int str_size = (int)bitstream.ReadBytes(1);
                bitstream.ReadString(str_size);
                _bytesRead += str_size + 1;
            }
            if (ocrFlag)
            {
                bitstream.SkipBytes(2);
                _bytesRead += 2;
            }
            while (_bytesRead < _size)
            {
                Mp4Descriptor descriptor = CreateDescriptor(bitstream);
                _children.Add(descriptor);
                _bytesRead += descriptor.BytesRead;
            }
        }
        /// <summary>
        /// Loads the MP4DecoderConfigDescriptor from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        public void CreateDecoderConfigDescriptor(Mp4DataStream bitstream)
        {
            int objectTypeIndication = (int)bitstream.ReadBytes(1);
            int value = (int)bitstream.ReadBytes(1);
            bool upstream = (value & (1 << 1)) > 0;
            byte streamType = (byte)(value >> 2);
            value = (int)bitstream.ReadBytes(2);
            int bufferSizeDB = value << 8;
            value = (int)bitstream.ReadBytes(1);
            bufferSizeDB |= value & 0xff;
            int maxBitRate = (int)bitstream.ReadBytes(4);
            int minBitRate = (int)bitstream.ReadBytes(4);
            _bytesRead += 13;
            if (_bytesRead < _size)
            {
                Mp4Descriptor descriptor = CreateDescriptor(bitstream);
                _children.Add(descriptor);
                _bytesRead += descriptor.BytesRead;
            }
        }

        protected int decSpecificDataSize;

        protected long decSpecificDataOffset;

        private byte[] dsid;

        public byte[] DSID
        {
            get { return dsid; }
        }

        public void CreateDecSpecificInfoDescriptor(Mp4DataStream bitstream)
        {
            decSpecificDataOffset = bitstream.Offset;
            dsid = new byte[_size];
            for (int b = 0; b < _size; b++)
            {
                dsid[b] = (byte)bitstream.ReadBytes(1);
                _bytesRead++;
            }
            decSpecificDataSize = _size - _bytesRead;
        }

        public long DecSpecificDataOffset
        {
            get { return decSpecificDataOffset; }
        }

        public int DecSpecificDataSize
        {
            get { return decSpecificDataSize; }
        }

        /// <summary>
        /// Looks up a child descriptor with the specified <code>type</code>, skips the <code>number</code> children with the same type before retrieving.
        /// </summary>
        /// <param name="type">The type of the descriptor.</param>
        /// <param name="number">The number of child descriptors to skip.</param>
        /// <returns>The descriptor if found, otherwise null.</returns>
        public Mp4Descriptor Lookup(int type, int number)
        {
            int position = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                Mp4Descriptor descriptor = _children[i];
                if (descriptor.Type == type)
                {
                    if (position >= number)
                    {
                        return descriptor;
                    }
                    position++;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the type of the descriptor.
        /// </summary>
        public int Type
        {
            get { return _type; }
        }

        public int BytesRead
        {
            get { return _bytesRead; }
        }

        public List<Mp4Descriptor> Children
        {
            get { return _children; }
        }
    }
}
