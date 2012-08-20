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
using FluorineFx.Util;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
using FluorineFx.Messaging.Rtmp.IO;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    class StreamTracker
    {
        /// <summary>
        /// Last audio flag.
        /// </summary>
        private int _lastAudio;
        /// <summary>
        /// Last video flag.
        /// </summary>
        private int _lastVideo;
        /// <summary>
        /// Last notification flag.
        /// </summary>
        private int _lastNotify;
        /// <summary>
        /// Relative flag.
        /// </summary>
        private bool _relative;
        /// <summary>
        /// First video flag.
        /// </summary>
        private bool _firstVideo;
        /// <summary>
        /// First audio flag.
        /// </summary>
        private bool _firstAudio;
        /// <summary>
        /// First notification flag.
        /// </summary>
        private bool _firstNotify;

        public StreamTracker()
        {
            Reset();
        }

        /// <summary>
        /// Reset state.
        /// </summary>
        public void Reset()
        {
            _lastAudio = 0;
            _lastVideo = 0;
            _lastNotify = 0;
            _firstVideo = true;
            _firstAudio = true;
            _firstNotify = true;
        }

        public bool IsRelative
        {
            get { return _relative; }
        }

        /// <summary>
        /// RTMP event handler.
        /// </summary>
        /// <param name="evt">RTMP event</param>
        /// <returns>Timeframe since last notification (or audio or video packet sending).</returns>
        public int Add(IRtmpEvent evt)
        {
            _relative = true;
            int timestamp = evt.Timestamp;
            int tsOut = 0;

            switch (evt.DataType)
            {

                case Constants.TypeAudioData:
                    if (_firstAudio)
                    {
                        tsOut = evt.Timestamp;
                        _relative = false;
                        _firstAudio = false;
                    }
                    else
                    {
                        tsOut = timestamp - _lastAudio;
                        _lastAudio = timestamp;
                    }
                    break;

                case Constants.TypeVideoData:
                    if (_firstVideo)
                    {
                        tsOut = evt.Timestamp;
                        _relative = false;
                        _firstVideo = false;
                    }
                    else
                    {
                        tsOut = timestamp - _lastVideo;
                        _lastVideo = timestamp;
                    }
                    break;

                case Constants.TypeNotify:
                case Constants.TypeInvoke:
                    if (_firstNotify)
                    {
                        tsOut = evt.Timestamp;
                        _relative = false;
                        _firstNotify = false;
                    }
                    else
                    {
                        tsOut = timestamp - _lastNotify;
                        _lastNotify = timestamp;
                    }
                    break;

                default:
                    // ignore other types
                    break;
            }
            return tsOut;
        }
    }
}
