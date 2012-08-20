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
using System.Text;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO.Mp4
{
    /// <summary>
    /// Wrapper class for input streams containing MPEG4 data.
    /// Original idea based on code from MediaFrame (http://www.mediaframe.org)
    /// </summary>
    class Mp4DataStream
    {
        /// <summary>
        /// The input stream
        /// </summary>
        private Stream _stream;
        /// <summary>
        /// The current offset (position) in the stream.
        /// </summary>
        private long _offset = 0;

        public Mp4DataStream(Stream stream)
        {
            _stream = stream;
        }

        public long ReadBytes(int n)
        {
            int c = -1;
            long result = 0;
            while ((n-- > 0) && ((c = _stream.ReadByte()) != -1))
            {
                result <<= 8;
                result += c & 0xff;
                _offset++;
            }
            if (c == -1)
                throw new EndOfStreamException();
            return result;
        }

        public String ReadString(int n)
        {
            int c = -1;
            StringBuilder sb = new StringBuilder();
            while ((n-- > 0) && ((c = (char)_stream.ReadByte()) != -1))
            {
                sb.Append((char)c);
                _offset++;
            }
            if (c == -1)
                throw new EndOfStreamException();
            return sb.ToString();
        }

        public void SkipBytes(long n)
        {
            _offset += n;
            _stream.Seek(n, SeekOrigin.Current);
        }

        public long Offset
        {
            get { return _offset; }
        }

        public void Close()
        {
            _stream.Close();
        }
    }
}
