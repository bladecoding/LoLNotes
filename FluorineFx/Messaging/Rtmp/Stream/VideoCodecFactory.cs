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
using FluorineFx.Messaging.Rtmp.Stream.Codec;
using FluorineFx.Util;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Factory for video codecs. Creates and returns video codecs.
    /// </summary>
    class VideoCodecFactory
    {
        private static ILog log = LogManager.GetLogger(typeof(VideoCodecFactory));
        /// <summary>
        /// List of available codecs.
        /// </summary>
        private IList _codecs = new ArrayList();

        public IList Codecs
        {
            set { _codecs = value; }
        }
        /// <summary>
        /// Create and return new video codec applicable for byte buffer data
        /// </summary>
        /// <param name="data">Byte buffer data.</param>
        /// <returns>Video codec.</returns>
        public IVideoStreamCodec GetVideoCodec(ByteBuffer data)
        {
            IVideoStreamCodec result = null;
            //get the codec identifying byte
            int codecId = data.Get() & 0x0f;
            switch (codecId)
            {
                case 2: //sorenson 
                    result = new SorensonVideo();
                    break;
                case 3: //screen video
                    result = new ScreenVideo();
                    break;
                case 7: //avc/h.264 video
                    result = new AVCVideo();
                    break;
            }
            data.Rewind();
            if (result == null)
            {
                IVideoStreamCodec codec;
                foreach (IVideoStreamCodec storedCodec in _codecs)
                {
                    // XXX: this is a bit of a hack to create new instances of the
                    // configured video codec for each stream
                    try
                    {
                        codec = Activator.CreateInstance(storedCodec.GetType()) as IVideoStreamCodec;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Could not create video codec instance.", ex);
                        continue;
                    }

                    log.Info("Trying codec " + codec);
                    if (codec.CanHandleData(data))
                    {
                        result = codec;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
