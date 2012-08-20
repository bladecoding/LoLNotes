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
using log4net;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Util;


namespace FluorineFx.Messaging.Rtmp.Stream.Codec
{
    class ScreenVideo : IVideoStreamCodec
    {
        /// <summary>
        /// ScreenVideo video codec constant.
        /// </summary>
        static string CodecName = "ScreenVideo";

        /// <summary>
        /// FLV codec screen marker constant.
        /// </summary>
	    public const byte FLV_CODEC_SCREEN = 0x03;


        /// <summary>
        /// Block data.
        /// </summary>
        private byte[] _blockData;
        /// <summary>
        /// Block size.
        /// </summary>
        private int[] _blockSize;
        /// <summary>
        /// Video width.
        /// </summary>
        private int _width;
        /// <summary>
        /// Video height.
        /// </summary>
        private int _height;
        /// <summary>
        /// Width info.
        /// </summary>
        private int _widthInfo;
        /// <summary>
        /// Height info.
        /// </summary>
        private int _heightInfo;
        /// <summary>
        /// Block width.
        /// </summary>
        private int _blockWidth;
        /// <summary>
        /// Block height.
        /// </summary>
        private int _blockHeight;
        /// <summary>
        /// Number of blocks.
        /// </summary>
        private int _blockCount;
        /// <summary>
        /// Block data size.
        /// </summary>
        private int _blockDataSize;
        /// <summary>
        /// Total block data size.
        /// </summary>
        private int _totalBlockDataSize;

        public ScreenVideo()
        {
            Reset();
        }

        #region IVideoStreamCodec Members

        public string Name
        {
            get { return ScreenVideo.CodecName; }
        }

        public void Reset()
        {
            _blockData = null;
            _blockSize = null;
            _width = 0;
            _height = 0;
            _widthInfo = 0;
            _heightInfo = 0;
            _blockWidth = 0;
            _blockHeight = 0;
            _blockCount = 0;
            _blockDataSize = 0;
            _totalBlockDataSize = 0;
        }

        public bool CanDropFrames
        {
            get { return false; }
        }

        public bool CanHandleData(ByteBuffer data)
        {
            if (data.Limit == 0)
                return false;// Empty buffer
            byte first = data.Get();
            bool result = ((first & 0x0f) == VideoCodec.ScreenVideo.Id);
            data.Rewind();
            return result;
        }

        public bool AddData(ByteBuffer data)
        {
            if (data.Limit == 0)
                return false;// Empty buffer
            if (!CanHandleData(data))
                return false;
            data.Get();
            UpdateSize(data);
            int idx = 0;
            int pos = 0;
            byte[] tmpData = new byte[_blockDataSize];

            int countBlocks = _blockCount;
            while (data.Remaining > 0 && countBlocks > 0)
            {
                short size = data.GetShort();
                countBlocks--;
                if (size == 0)
                {
                    // Block has not been modified
                    idx += 1;
                    pos += _blockDataSize;
                    continue;
                }
                // Store new block data
                _blockSize[idx] = size;
                data.Read(tmpData, 0, size);
                Array.Copy(tmpData, 0, _blockData, pos, size);
                idx += 1;
                pos += _blockDataSize;
            }
            data.Rewind();
            return true;            
        }

        public ByteBuffer GetKeyframe()
        {
            ByteBuffer result = ByteBuffer.Allocate(1024);
            result.AutoExpand = true;
            // Header
            result.Put((byte)(VideoCodec.FLV_FRAME_KEY | VideoCodec.ScreenVideo.Id));
            // Frame size
            result.PutShort((short)_widthInfo);
            result.PutShort((short)_heightInfo);
            // Get compressed blocks
            byte[] tmpData = new byte[_blockDataSize];
            int pos = 0;
            for (int idx = 0; idx < _blockCount; idx++)
            {
                int size = _blockSize[idx];
                if (size == 0)
                {
                    // This should not happen: no data for this block
                    return null;
                }
                result.PutShort((short)size);
                Array.Copy(_blockData, pos, tmpData, 0, size);
                result.Put(tmpData, 0, size);
                pos += _blockDataSize;
            }
            result.Rewind();
            return result;
        }

        public ByteBuffer GetDecoderConfiguration()
        {
            return null;
        }

        #endregion

        /// <summary>
        /// This uses the same algorithm as "compressBound" from zlib.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int GetMaxCompressedSize(int size)
        {
            return size + (size >> 12) + (size >> 14) + 11;
        }

        /// <summary>
        /// Update total block size.
        /// </summary>
        /// <param name="data"></param>
        private void UpdateSize(ByteBuffer data)
        {
            _widthInfo = data.GetShort();
            _heightInfo = data.GetShort();
            // extract width and height of the frame
            _width = _widthInfo & 0xfff;
            _height = _heightInfo & 0xfff;
            // calculate size of blocks
            _blockWidth = _widthInfo & 0xf000;
            _blockWidth = (_blockWidth >> 12) + 1;
            _blockWidth <<= 4;

            _blockHeight = _heightInfo & 0xf000;
            _blockHeight = (_blockHeight >> 12) + 1;
            _blockHeight <<= 4;

            int xblocks = _width / _blockWidth;
            if ((_width % _blockWidth) != 0)
            {
                // partial block
                xblocks += 1;
            }

            int yblocks = _height / _blockHeight;
            if ((_height % _blockHeight) != 0)
            {
                // partial block
                yblocks += 1;
            }

            _blockCount = xblocks * yblocks;

            int blockSize = GetMaxCompressedSize(_blockWidth * _blockHeight * 3);
            int totalBlockSize = blockSize * _blockCount;
            if (_totalBlockDataSize != totalBlockSize)
            {
                //log.Debug("Allocating memory for {} compressed blocks.", this.blockCount);
                _blockDataSize = blockSize;
                _totalBlockDataSize = totalBlockSize;
                _blockData = new byte[blockSize * _blockCount];
                _blockSize = new int[blockSize * _blockCount];
                // Reset the sizes to zero
                for (int idx = 0; idx < _blockCount; idx++)
                {
                    _blockSize[idx] = 0;
                }
            }
        }
    }
}
