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
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Output stream that consists of audio, video and data channels.
    /// </summary>
    [CLSCompliant(false)]
    public class OutputStream
    {
        /// <summary>
        /// Video channel.
        /// </summary>
        private RtmpChannel _video;
        /// <summary>
        /// Audio channel.
        /// </summary>
        private RtmpChannel _audio;
        /// <summary>
        /// Data channel.
        /// </summary>
        private RtmpChannel _data;

        /// <summary>
        /// Creates output stream from channels.
        /// </summary>
        /// <param name="video">Video channel.</param>
        /// <param name="audio">Audio channel.</param>
        /// <param name="data">Data channel.</param>
        internal OutputStream(RtmpChannel video, RtmpChannel audio, RtmpChannel data)
        {
            _video = video;
            _audio = audio;
            _data = data;
        }
        /// <summary>
        /// Closes audio, video and data channels.
        /// </summary>
        public void Close()
        {
            _video.Close();
            _audio.Close();
            _data.Close();
        }
        /// <summary>
        /// Gets the audio channel.
        /// </summary>
        public RtmpChannel Audio
        {
            get{ return _audio; }
        }
        /// <summary>
        /// Gets the data channel.
        /// </summary>
        public RtmpChannel Data
        {
            get{ return _data; }
        }
        /// <summary>
        /// Gets the video channel.
        /// </summary>
        public RtmpChannel Video
        {
            get { return _video; }
        }
    }
}
