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
using System.Text;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class RtmpHeader
	{
        /// <summary>
        /// Channel.
        /// </summary>
		int _channelId = 0;
        /// <summary>
        /// Timer.
        /// </summary>
		int _timer = 0;
        /// <summary>
        /// Header size
        /// </summary>
		int _size = 0;
        /// <summary>
        /// Type of data.
        /// </summary>
		byte _headerDataType = 0;
        /// <summary>
        /// Stream id.
        /// </summary>
		int _streamId = 0;
        /// <summary>
        /// Whether timer value is relative.
        /// </summary>
		bool _timerRelative = true;

        internal RtmpHeader()
		{
		}
        /// <summary>
        /// Gets or set the channel id.
        /// </summary>
		public int ChannelId
		{
			get{ return _channelId; }
			set{ _channelId = value; }
		}
        /// <summary>
        /// Gets or set the data type.
        /// </summary>
		public byte DataType
		{
			get{ return _headerDataType; }
			set{ _headerDataType = value; }
		}
        /// <summary>
        /// Gets or sets the header size.
        /// </summary>
		public int Size
		{
			get{ return _size; }
			set{ _size = value; }
		}
        /// <summary>
        /// Gets or sets the stream id.
        /// </summary>
		public int StreamId
		{
			get{ return _streamId; }
			set{ _streamId = value; }
		}
        /// <summary>
        /// Gets or sets the timer.
        /// </summary>
		public int Timer
		{
			get{ return _timer; }
			set{ _timer = value; }
		}
        /// <summary>
        /// Gets or sets the timer relative flag.
        /// </summary>
		public bool IsTimerRelative
		{
			get{ return _timerRelative; }
			set{ _timerRelative = value; }
		}
        /// <summary>
        /// Gets the header lenght.
        /// </summary>
        /// <param name="headerType"></param>
        /// <returns></returns>
		public static int GetHeaderLength(HeaderType headerType) 
		{
			switch (headerType) 
			{
				case HeaderType.HeaderNew:
					return 12;
				case HeaderType.HeaderSameSource:
					return 8;
				case HeaderType.HeaderTimerChange:
					return 4;
				case HeaderType.HeaderContinue:
					return 1;
				default:
					return -1;
			}
		}
        /// <summary>
        /// Returns a string that represents the current RtmpHeader object.
        /// </summary>
        /// <returns>A string that represents the current RtmpHeader object.</returns>
        public override string ToString()
        {
            return ToString(1);
        }
        /// <summary>
        /// Returns a string that represents the current RtmpHeader object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the header members.</param>
        /// <returns>A string that represents the current RtmpHeader object.</returns>
        public string ToString(int indentLevel)
        {
            return ToStringHeader(indentLevel) + ToStringFields(indentLevel + 1);
        }
        /// <summary>
        /// Returns a header string that represents the current RtmpHeader object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the header members.</param>
        /// <returns>A header string that represents the current RtmpHeader object.</returns>
        private string ToStringHeader(int indentLevel)
        {
            string value = "RTMP Header";
            return value;
        }
        /// <summary>
        /// Returns a string that represents the current RtmpHeader object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the header members.</param>
        /// <returns>A string that represents the current RtmpHeader object fields.</returns>
        internal string ToStringFields(int indentLevel)
        {
            String sep = MessageBase.GetFieldSeparator(indentLevel);
            String value = sep + "channelId = " + _channelId;
            value += sep + "timer = " + _timer + " (" + (_timerRelative ? "relative" : "absolute") + ")";
            value += sep + "size = " + _size;
            value += sep + "streamId = " + _streamId;
            value += sep + "dateType = " + _headerDataType + " (" + HeaderTypeToString(_headerDataType) + ")";
            return value;
        }

        static string[] HeaderTypeNames = { "unknown", "chunk_size", "unknown2", "bytes_read", "ping", "server_bw", "client_bw", "unknown7", "audio", "video", "unknown10", "unknown11", "unknown12", "unknown13", "unknown14", "flex_stream", "flex_shared_object", "flex_message", "notify", "shared_object", "invoke" };

        private static string HeaderTypeToString(int type)
        {
            if (type < 0 || type >= HeaderTypeNames.Length)
                return "invalid header type";
            return HeaderTypeNames[type];
        }

	}
}
