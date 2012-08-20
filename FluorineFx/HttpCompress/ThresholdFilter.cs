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
using System.Web;

namespace FluorineFx.HttpCompress
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ThresholdFilter : HttpOutputFilter
	{
		MemoryStream	_stream;
		Stream			_baseStream;
		int				_threshold;

		public ThresholdFilter(Stream compressStream, Stream baseStream, int threshold) : base(compressStream)
		{
			_baseStream = baseStream;
			_stream = new MemoryStream();
			_threshold = threshold;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override void Close()
		{
			byte[] buffer = _stream.ToArray();
			if( _threshold <= 0 || _stream.Length > _threshold )
			{
				BaseStream.Write(buffer, 0, buffer.Length);
				BaseStream.Flush();
				BaseStream.Close();
			}
			else
			{
				_baseStream.Write(buffer, 0, buffer.Length);
				_baseStream.Flush();
				_baseStream.Close();
			}

			_stream.Close();
		}

		public CompressingFilter CompressingFilter
		{
			get{ return BaseStream as CompressingFilter; }
		}
	}
}
