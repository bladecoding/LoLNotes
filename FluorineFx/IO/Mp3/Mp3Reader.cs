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
#if !NET_1_1
using System.Collections.Generic;
#endif
using System.IO;
using log4net;
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO.Mp3
{
    class Mp3Reader : ITagReader, IKeyFrameDataAnalyzer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Mp3Reader));
        object _syncLock = new object();

        private FileInfo _file;
        private FileStream _fileStream;
        /// <summary>
        /// Last read tag object
        /// </summary>
        private ITag _tag;
        /// <summary>
        /// Previous tag size
        /// </summary>
        private int _prevSize;
        /// <summary>
        /// Current time
        /// </summary>
        private double _currentTime;
        /// <summary>
        /// Frame metadata
        /// </summary>
        private KeyFrameMeta _frameMeta;
        /// <summary>
        /// Mapping between file position and timestamp in ms.
        /// (Long, Long)
        /// </summary>
#if !NET_1_1
        private Dictionary<long, double> _posTimeMap;
#else
        private Hashtable _posTimeMap;
#endif
        private int _dataRate;
        /// <summary>
        /// Whether first frame is read
        /// </summary>
        private bool _firstFrame;
        /// <summary>
        /// File metadata
        /// </summary>
        private ITag _fileMeta;
        /// <summary>
        /// File duration
        /// </summary>
        private long _duration;

        public Mp3Reader(FileInfo file)
        {
            _file = file;
            _fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
            // Analyze keyframes data
            AnalyzeKeyFrames();
            _firstFrame = true;
            // Process ID3v2 header if present
            ProcessID3v2Header();
            // Create file metadata object
            _fileMeta = CreateFileMeta();
            // MP3 header is length of 32 bits, that is, 4 bytes
            // Read further if there's still data
            if (_fileStream.Length - _fileStream.Position > 4)
            {
                // Look to next frame
                SearchNextFrame();
                // Save position
                long pos = _fileStream.Position;
                // Read header...
                // Data in MP3 file goes header-data-header-data...header-data
                Mp3Header header = ReadHeader();
                // Set position
                _fileStream.Position = pos;
                // Check header
                if (header != null)
                {
                    CheckValidHeader(header);
                }
                else
                    throw new NotSupportedException("No initial header found.");
            }
        }

        #region ITagReader Members

        public IStreamableFile File
        {
            get { return null; }
        }

        public int Offset
        {
            get { return 0; }
        }

        public long BytesRead
        {
            get { return _fileStream.Position ; }
        }

        public long Duration
        {
            get { return _duration; }
        }

        public void DecodeHeader()
        {
        }

        public long Position
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                if (value == long.MaxValue)
                {
                    // Seek at EOF
                    _fileStream.Position = _fileStream.Length;
                    _currentTime = _duration;
                    return;
                }
                _fileStream.Position = value;
                // Advance to next frame
                SearchNextFrame();
                // Make sure we can resolve file positions to timestamps
                AnalyzeKeyFrames();
                if (_posTimeMap.ContainsKey(_fileStream.Position))
                    _currentTime = (double)_posTimeMap[_fileStream.Position];
                else
                {
                    // Unknown frame position - this should never happen
                    _currentTime = 0;
                }
            }
        }

        public bool HasMoreTags()
        {
            Mp3Header header = null;
            while (header == null && (_fileStream.Length - _fileStream.Position) > 4)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    _fileStream.Read(buffer, 0, 4);
                    header = new Mp3Header(buffer);
                }
                catch (IOException ex)
                {
                    log.Error("MP3Reader HasMoreTags", ex);
                    break;
                }
                catch (Exception)
                {
                    SearchNextFrame();
                }
            }

            if (header == null)
                return false;

            if (header.FrameSize == 0)
            {
                // TODO find better solution how to deal with broken files...
                // See APPSERVER-62 for details
                return false;
            }

            if (_fileStream.Position + header.FrameSize - 4 > _fileStream.Length)
            {
                // Last frame is incomplete
                _fileStream.Position = _fileStream.Length;
                return false;
            }

            _fileStream.Position = _fileStream.Position - 4;
            return true;
        }

        public ITag ReadTag()
        {
            lock (_syncLock)
            {
                if (_firstFrame)
                {
                    // Return file metadata as first tag.
                    _firstFrame = false;
                    return _fileMeta;
                }

                Mp3Header header = ReadHeader();
                if (header == null)
                    return null;

                int frameSize = header.FrameSize;
                if (frameSize == 0)
                {
                    // TODO find better solution how to deal with broken files...
                    // See APPSERVER-62 for details
                    return null;
                }

                if (_fileStream.Position + frameSize - 4 > _fileStream.Length)
                {
                    // Last frame is incomplete
                    _fileStream.Position = _fileStream.Length;
                    return null;
                }

                _tag = new Tag(IOConstants.TYPE_AUDIO, (int)_currentTime, frameSize + 1, null, _prevSize);
                _prevSize = frameSize + 1;
                _currentTime += header.FrameDuration;
                byte[] buffer = new byte[_tag.BodySize];
                //ByteBuffer body = ByteBuffer.Allocate(_tag.BodySize);
                byte tagType = (byte)((IOConstants.FLAG_FORMAT_MP3 << 4) | (IOConstants.FLAG_SIZE_16_BIT << 1));
                switch (header.SampleRate)
                {
                    case 44100:
                        tagType |= (byte)(IOConstants.FLAG_RATE_44_KHZ << 2);
                        break;
                    case 22050:
                        tagType |= (byte)(IOConstants.FLAG_RATE_22_KHZ << 2);
                        break;
                    case 11025:
                        tagType |= (byte)(IOConstants.FLAG_RATE_11_KHZ << 2);
                        break;
                    default:
                        tagType |= (byte)(IOConstants.FLAG_RATE_5_5_KHZ << 2);
                        break;
                }
                tagType |= (header.IsStereo ? IOConstants.FLAG_TYPE_STEREO : IOConstants.FLAG_TYPE_MONO);
                /*
                body.Put(tagType);
                body.PutInt(header.Data);
                byte[] buffer = new byte[frameSize - 4];
                _fileStream.Read(buffer, 0, buffer.Length);
                body.Put(buffer);
                body.Flip();
                _tag.Body = body.ToArray();
                */
                buffer[0] = tagType;
                Array.Copy(header.Data, 0, buffer, 1, 4);
                _fileStream.Read(buffer, 5, frameSize - 4);
                _tag.Body = buffer;
                return _tag;
            }
        }

        public void Close()
        {
            if (_posTimeMap != null)
                _posTimeMap.Clear();
            _fileStream.Close();
        }

        /// <summary>
        /// An MP3 stream never has video.
        /// </summary>
        /// <returns></returns>
        public bool HasVideo()
        {
            return false;
        }

        #endregion

        #region IKeyFrameDataAnalyzer Members

        public KeyFrameMeta AnalyzeKeyFrames()
        {
            lock (_syncLock)
            {
                if (_frameMeta != null)
                    return _frameMeta;

#if !NET_1_1
                List<long> positionList = new List<long>();
                List<double> timestampList = new List<double>();
#else
                ArrayList positionList = new ArrayList();
		        ArrayList timestampList = new ArrayList();
#endif
                _dataRate = 0;
                long rate = 0;
                int count = 0;
                long origPos = _fileStream.Position;
                double time = 0;
                _fileStream.Position = 0;
                ProcessID3v2Header();
                SearchNextFrame();
                while (this.HasMoreTags())
                {
                    Mp3Header header = ReadHeader();
                    if (header == null)
                    {
                        // No more tags
                        break;
                    }

                    if (header.FrameSize == 0)
                    {
                        // TODO find better solution how to deal with broken files...
                        // See APPSERVER-62 for details
                        break;
                    }

                    long pos = _fileStream.Position - 4;
                    if (pos + header.FrameSize > _fileStream.Length)
                    {
                        // Last frame is incomplete
                        break;
                    }

                    positionList.Add(pos);
                    timestampList.Add(time);
                    rate += header.BitRate / 1000;
                    time += header.FrameDuration;
                    _fileStream.Position = pos + header.FrameSize;
                    count++;
                }
                // restore the pos
                _fileStream.Position = origPos;

                _duration = (long)time;
                _dataRate = (int)(rate / count);
#if !NET_1_1
                _posTimeMap = new Dictionary<long, double>();
#else
                _posTimeMap = new Hashtable();
#endif
                _frameMeta = new KeyFrameMeta();
                _frameMeta.Duration = _duration;
                _frameMeta.Positions = new long[positionList.Count];
                _frameMeta.Timestamps = new int[timestampList.Count];
                _frameMeta.AudioOnly = true;
                for (int i = 0; i < _frameMeta.Positions.Length; i++)
                {
                    _frameMeta.Positions[i] = (int)positionList[i];
                    _frameMeta.Timestamps[i] = (int)timestampList[i];
                    _posTimeMap.Add(positionList[i], timestampList[i]);
                }
                return _frameMeta;
            }
        }

        #endregion

        /// <summary>
        /// Check if the file can be played back with Flash. Supported sample rates are 44KHz, 22KHz, 11KHz and 5.5KHz
        /// </summary>
        /// <param name="header"></param>
        private void CheckValidHeader(Mp3Header header)
        {
            switch (header.SampleRate)
            {
                case 44100:
                case 22050:
                case 11025:
                case 5513:
                    // Supported sample rate
                    break;
                default:
                    throw new NotSupportedException("Unsupported sample rate: " + header.SampleRate);
            }
        }

        /// <summary>
        /// Create file metadata object
        /// </summary>
        /// <returns></returns>
        private ITag CreateFileMeta()
        {
            // Create tag for onMetaData event
            ByteBuffer buf = ByteBuffer.Allocate(1024);
            buf.AutoExpand = true;
            AMFWriter output = new AMFWriter(buf);
            output.WriteString("onMetaData");
            Hashtable props = new Hashtable();
            // Duration property
            props.Add("duration", _frameMeta.Timestamps[_frameMeta.Timestamps.Length - 1] / 1000.0);
            props.Add("audiocodecid", IOConstants.FLAG_FORMAT_MP3);
            if (_dataRate > 0)
            {
                props.Add("audiodatarate", _dataRate);
            }
            props.Add("canSeekToEnd", true);
            output.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            buf.Flip();

            ITag result = new Tag(IOConstants.TYPE_METADATA, 0, buf.Limit, buf.ToArray(), _prevSize);
            return result;
        }

        /// <summary>
        /// Search for next frame sync word. Sync word identifies valid frame.
        /// </summary>
        public void SearchNextFrame() 
        {
		    while(_fileStream.Length - _fileStream.Position > 1) 
            {
			    int ch = _fileStream.ReadByte() & 0xff;
			    if (ch != 0xff) 
				    continue;

			    if ((_fileStream.ReadByte() & 0xe0) == 0xe0) 
                {
				    // Found it
				    _fileStream.Position =  _fileStream.Position - 2;
				    return;
                }
            }
        }

        private Mp3Header ReadHeader()
        {
            Mp3Header header = null;
            while (header == null && (_fileStream.Length - _fileStream.Position) > 4)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    _fileStream.Read(buffer, 0, 4);
                    header = new Mp3Header(buffer);
                }
                catch (IOException ex)
                {
                    log.Error("MP3Reader ReadTag", ex);
                    break;
                }
                catch (Exception)
                {
                    SearchNextFrame();
                }
            }
            return header;
        }

        private void ProcessID3v2Header()
        {
            if (_fileStream.Length - _fileStream.Position <= 10)
                // We need at least 10 bytes ID3v2 header + data
                return;

            long start = _fileStream.Position;
            byte a, b, c;
            a = (byte)_fileStream.ReadByte();
            b = (byte)_fileStream.ReadByte();
            c = (byte)_fileStream.ReadByte();
            if (a != 'I' || b != 'D' || c != '3')
            {
                // No ID3v2 header
                _fileStream.Position = start;
                return;
            }

            // Skip version and flags
            _fileStream.Seek(3, SeekOrigin.Current);
            int size = (_fileStream.ReadByte() & 0x7f) << 21 | (_fileStream.ReadByte() & 0x7f) << 14 | (_fileStream.ReadByte() & 0x7f) << 7 | (_fileStream.ReadByte() & 0x7f);
            // Skip ID3v2 header for now
            _fileStream.Seek(size, SeekOrigin.Current);
        }
    }
}
