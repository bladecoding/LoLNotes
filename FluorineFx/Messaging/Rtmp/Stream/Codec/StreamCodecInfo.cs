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
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Rtmp.Stream.Codec
{
    class StreamCodecInfo : IStreamCodecInfo
    {
        /// <summary>
        /// Audio support flag.
        /// </summary>
        private bool _audio;
        /// <summary>
        /// Video support flag.
        /// </summary>
        private bool _video;
        /// <summary>
        /// Video codec.
        /// </summary>
        private IVideoStreamCodec _videoCodec;
        /// <summary>
        /// Audio codec.
        /// </summary>
        private IAudioStreamCodec _audioCodec;

        #region IStreamCodecInfo Members

        public bool HasAudio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        public bool HasVideo
        {
            get { return _video; }
            set { _video = value; }
        }

        public string AudioCodecName
        {
            get 
            {
                return null;
            }
        }

        public string VideoCodecName
        {
            get
            {
                if (_videoCodec == null)
                    return null;
                return _videoCodec.Name;
            }
        }

        public IVideoStreamCodec VideoCodec
        {
            get { return _videoCodec; }
            set { _videoCodec = value; }
        }

        /// <summary>
        /// Gets the audio codec used by stream codec.
        /// </summary>
        /// <value>The audio codec.</value>
        public IAudioStreamCodec AudioCodec 
        {
            get { return _audioCodec; }
            set { _audioCodec = value; }
        }

        #endregion
    }
}
