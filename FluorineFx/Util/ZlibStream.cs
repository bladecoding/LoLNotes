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
using System.IO.Compression;

namespace FluorineFx.Util
{
    class ZlibStream : DeflateStream
    {
        private bool _hasRead = false;
        /// <summary>
        /// Largest prime smaller than 65536.
        /// </summary>
        const int ModAdler = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        const int NMAX = 5552;

        uint _checksum;
        readonly CompressionMode _mode;
        private readonly bool _leaveOpen;
        Stream _stream;

        /// <summary>  
        /// Initializes a new instance of the System.IO.Compression.ZlibStream class using the specified stream and System.IO.Compression.CompressionMode value.
        /// </summary>  
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="mode">One of the System.IO.Compression.CompressionMode values that indicates the action to take.</param>
        public ZlibStream(Stream stream, CompressionMode mode)
            : this(stream, mode, false)
        {
        }

        /// <summary>  
        /// Initializes a new instance of the System.IO.Compression.ZlibStream class using the specified stream and System.IO.Compression.CompressionMode value. 
        /// </summary>  
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="mode">One of the System.IO.Compression.CompressionMode values that indicates the action to take.</param>
        /// <param name="leaveOpen">true to leave the stream open; otherwise, false.</param>  
        public ZlibStream(Stream stream, CompressionMode mode, bool leaveOpen) : base(stream, mode, true)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
            _mode = mode;
            if (mode == CompressionMode.Compress)
            {
                //A zlib stream has the following structure:
                //   0   1
                // +---+---+
                // |CMF|FLG|   (more-->)
                // +---+---+
                //
                // (if FLG.FDICT set)
                //
                //   0   1   2   3
                // +---+---+---+---+
                // |     DICTID    |   (more-->)
                // +---+---+---+---+
                //
                // +=====================+---+---+---+---+
                // |...compressed data...|    ADLER32    |
                // +=====================+---+---+---+---+

                //CMF (Compression Method and flags)
                //bits 0 to 3  CM     Compression method
                //bits 4 to 7  CINFO  Compression info
                //CM = 8 denotes the "deflate" compression method with a window size up to 32K.
                //CINFO (Compression info) For CM = 8, CINFO is the base-2 logarithm of the LZ77 window size, minus eight (CINFO=7 indicates a 32K window size).
                //  FLG (FLaGs)
                //     This flag byte is divided as follows:
                //
                //        bits 0 to 4  FCHECK  (check bits for CMF and FLG)
                //        bit  5       FDICT   (preset dictionary)
                //        bits 6 to 7  FLEVEL  (compression level)

                //deflate implementation, those bytes are 0x58 and 0x85
                //we use a window size of 8K and the value of FLEVEL should be 2 (default algorithm)
                //some comments indicating this is interpreted as max window size, so you might also try setting this to 7, corresponding to a window size of 32K.
                //byte[] header = { 0x78, 0xda };
                byte[] header = { 0x58, 0x85 };
                //byte[] header = { 0x78, 0x9c };
                _stream.Write(header, 0, header.Length);
                _checksum = 1;
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            //The zlib format is specified by RFC 1950. Zlib also uses deflate, plus 2 or 6 header bytes, and a 4 byte checksum at the end. 
            //The first 2 bytes indicate the compression method and flags. If the dictionary flag is set, then 4 additional bytes will follow.
            //Preset dictionaries aren't very common and we don't support them.
            if (_hasRead == false)
            {
                // Chop off the first two bytes
                int h1 = _stream.ReadByte();
                int h2 = _stream.ReadByte();
                _hasRead = true;
            }
            return base.Read(array, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            IAsyncResult result = base.BeginWrite(array, offset, count, asyncCallback, asyncState);
            _checksum = Adler32(_checksum, array, offset, count);
            return result;
        }

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            _checksum = Adler32(_checksum, array, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _mode == CompressionMode.Compress && _stream != null)
            {
                /*
                _stream.WriteByte((byte)(_checksum & 0xff));
                _stream.WriteByte((byte)((_checksum >> 8) & 0xff));
                _stream.WriteByte((byte)((_checksum >> 16) & 0xff));
                _stream.WriteByte((byte)((_checksum >> 24) & 0xff));
                */
                _stream.Write(BitConverter.GetBytes(_checksum), 0, 4);
                if (!_leaveOpen) _stream.Close();
                _stream = null;
            }
        }

        private static uint Adler32(uint adler, byte[] buffer, int offset, int length)
        {
            if (buffer == null)
                return 1;

            int s1 = (int)(adler & 0xffff);
            int s2 = (int)((adler >> 16) & 0xffff);

            while (length > 0)
            {
                int k = length < NMAX ? length : NMAX;
                length -= k;
                while (k >= 16)
                {
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    s1 += buffer[offset++]; s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buffer[offset++];
                        s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= ModAdler;
                s2 %= ModAdler;
            }
            return (uint)((s2 << 16) | s1);
        }
    }
}
