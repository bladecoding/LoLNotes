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
using System.Collections.Generic;
using System.IO;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO.FLV
{
    /// <summary>
    /// A Reader is used to read the contents of a FLV file.
    /// </summary>
    class FlvReader : ITagReader, IKeyFrameDataAnalyzer
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(FlvReader));
#endif
        object _syncLock = new object();

        private FileInfo _file;
        private AMFReader _reader;
        /// <summary>
        /// Set to true to generate metadata automatically before the first tag.
        /// </summary>
        private bool _generateMetadata;
        /// <summary>
        /// Keyframe metadata.
        /// </summary>
        private KeyFrameMeta _keyframeMeta;
        /// <summary>
        /// Position of first video tag.
        /// </summary>
        private long _firstVideoTag = -1;
        /// <summary>
        /// Position of first audio tag.
        /// </summary>
        private long _firstAudioTag = -1;
        /// <summary>
        /// Current tag.
        /// </summary>
        private int _tagPosition;
        /// <summary>
        /// Duration in milliseconds.
        /// </summary>
        private long _duration;
        /// <summary>
        /// Mapping between file position and timestamp in ms.
        /// (Long, Long)
        /// </summary>
        private Dictionary<long, long> _posTimeMap;
        /// <summary>
        /// Mapping between file position and tag number.
        /// (Long, Integer)
        /// </summary>
        private Dictionary<long, int> _posTagMap;

        /// <summary>
        /// The header of this FLV file.
        /// </summary>
        private FlvHeader _header;

        public FlvReader()
        {
        }

        public FlvReader(FileInfo file)
            : this(file, false)
        {

        }

        public FlvReader(FileInfo file, bool generateMetadata)
        {
            _file = file;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
            //_reader = new AMFReader(file.Open(FileMode.Open));
            _reader = new AMFReader(fs);
            _generateMetadata = generateMetadata;

            if (GetRemainingBytes() >= 9)
                DecodeHeader();
            _keyframeMeta = AnalyzeKeyFrames();
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        /// <summary>
        /// Get the remaining bytes that could be read from a file or ByteBuffer.
        /// </summary>
        /// <returns></returns>
        private long GetRemainingBytes()
        {
            return _reader.BaseStream.Length - _reader.BaseStream.Position;
        }
        /// <summary>
        /// Get the total readable bytes in a file or ByteBuffer.
        /// </summary>
        /// <returns></returns>
        private long GetTotalBytes()
        {
            return _file.Length;
        }
        /// <summary>
        /// Get the current position in a file or ByteBuffer.
        /// </summary>
        /// <returns></returns>
        private long GetCurrentPosition()
        {
            return _reader.BaseStream.Position;
        }
        /// <summary>
        /// Modifies current position.
        /// </summary>
        /// <param name="pos"></param>
        private void SetCurrentPosition(long pos)
        {
            if (pos == long.MaxValue)
            {
                pos = _file.Length;
            }
            _reader.BaseStream.Position = pos;
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
            get { return GetCurrentPosition(); }
        }

        public long Duration
        {
            get { return _duration; }
        }

        public void DecodeHeader()
        {
		    // SIGNATURE, lets just skip
		    _header = new FlvHeader();
            byte[] signature = _reader.ReadBytes(3);//FLV
            _header.Version = _reader.ReadByte();
            _header.SetTypeFlags(_reader.ReadByte());
            _header.DataOffset = _reader.ReadInt32();
#if !SILVERLIGHT
		    if (log.IsDebugEnabled) 
            {
			    log.Debug("Flv header: " + _header.ToString());
		    }
#endif
        }

        /// <summary>
        /// Gets or sets the current position.
        /// The caller must ensure the pos is a valid one
        /// </summary>
        public long Position
        {
            get
            {
                return GetCurrentPosition();
            }
            set
            {
                SetCurrentPosition(value);
                if (value == long.MaxValue)
                {
                    _tagPosition = _posTagMap.Count + 1;
                    return;
                }
                // Make sure we have informations about the keyframes.
                AnalyzeKeyFrames();
                // Update the current tag number
                if (_posTagMap.ContainsKey(value))
                {
                    _tagPosition = (int)_posTagMap[value];
                }
            }
        }

        public bool HasMoreTags()
        {
            return GetRemainingBytes() > 4;
        }

        public ITag ReadTag()
        {
            lock (this.SyncRoot)
            {
                long oldPos = GetCurrentPosition();
                ITag tag = ReadTagHeader();

                if (_tagPosition == 0 && tag.DataType != IOConstants.TYPE_METADATA && _generateMetadata)
                {
                    // Generate initial metadata automatically
                    SetCurrentPosition(oldPos);
                    KeyFrameMeta meta = AnalyzeKeyFrames();
                    _tagPosition++;
                    if (meta != null)
                    {
                        return CreateFileMeta();
                    }
                }
		        // This assists in 'properly' handling damaged FLV files		
		        long newPosition = GetCurrentPosition() + tag.BodySize;
                if (newPosition <= GetTotalBytes())
                {
                    byte[] buffer = _reader.ReadBytes(tag.BodySize);
                    tag.Body = buffer;
                    _tagPosition++;
                }
                return tag;
            }
        }

        public void Close()
        {
            _reader.Close();
        }

        public bool HasVideo()
        {
            KeyFrameMeta meta = AnalyzeKeyFrames();
            if (meta == null)
                return false;
            return (!meta.AudioOnly && meta.Positions.Length > 0);
        }

        #endregion

        #region IKeyFrameDataAnalyzer Members

        /// <summary>
        /// Key frames analysis may be used as a utility method so synchronize it.
        /// </summary>
        /// <returns></returns>
        public KeyFrameMeta AnalyzeKeyFrames()
        {
            lock (this.SyncRoot)
            {
                if (_keyframeMeta != null)
                    return _keyframeMeta;

                // Lists of video positions and timestamps
                List<long> positionList = new List<long>();
                List<int> timestampList = new List<int>();
                // Lists of audio positions and timestamps
                List<long> audioPositionList = new List<long>();
                List<int> audioTimestampList = new List<int>();
                long origPos = GetCurrentPosition();
                // point to the first tag
                SetCurrentPosition(9);

                // Maps positions to tags
                _posTagMap = new Dictionary<long,int>();
                int idx = 0;
                bool audioOnly = true;
                while (this.HasMoreTags())
                {
                    long pos = GetCurrentPosition();
                    _posTagMap.Add(pos, idx++);
                    // Read tag header and duration
                    ITag tmpTag = this.ReadTagHeader();
                    _duration = tmpTag.Timestamp;
                    if (tmpTag.DataType == IOConstants.TYPE_VIDEO)
                    {
                        if (audioOnly)
                        {
                            audioOnly = false;
                            audioPositionList.Clear();
                            audioTimestampList.Clear();
                        }
                        if (_firstVideoTag == -1)
                        {
                            _firstVideoTag = pos;
                        }

                        // Grab Frame type
                        byte frametype = _reader.ReadByte();
                        if (((frametype & IOConstants.MASK_VIDEO_FRAMETYPE) >> 4) == IOConstants.FLAG_FRAMETYPE_KEYFRAME)
                        {
                            positionList.Add(pos);
                            timestampList.Add(tmpTag.Timestamp);
                        }
                    }
                    else if (tmpTag.DataType == IOConstants.TYPE_AUDIO)
                    {
                        if (_firstAudioTag == -1)
                        {
                            _firstAudioTag = pos;
                        }
                        if (audioOnly)
                        {
                            audioPositionList.Add(pos);
                            audioTimestampList.Add(tmpTag.Timestamp);
                        }
                    }
                    // This properly handles damaged FLV files - as far as duration/size is concerned
                    long newPosition = pos + tmpTag.BodySize + 15;
                    if (newPosition >= GetTotalBytes())
                    {
#if !SILVERLIGHT
                        log.Info("New position exceeds limit");
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Keyframe analysis");
                            log.Debug(" data type=" + tmpTag.DataType + " bodysize=" + tmpTag.BodySize);
                            log.Debug(" remaining=" + GetRemainingBytes() + " limit=" + GetTotalBytes() + " new pos=" + newPosition + " pos=" + pos);
                        }
#endif
                        break;
                    }
                    else
                    {
                        SetCurrentPosition(newPosition);
                    }
                }
                // restore the pos
                SetCurrentPosition(origPos);

                _keyframeMeta = new KeyFrameMeta();
                _keyframeMeta.Duration = _duration;
                _posTimeMap = new Dictionary<long,long>();
                if (audioOnly)
                {
                    // The flv only contains audio tags, use their lists to support pause and seeking
                    positionList = audioPositionList;
                    timestampList = audioTimestampList;
                }
                _keyframeMeta.AudioOnly = audioOnly;
                _keyframeMeta.Positions = new long[positionList.Count];
                _keyframeMeta.Timestamps = new int[timestampList.Count];
                for (int i = 0; i < _keyframeMeta.Positions.Length; i++)
                {
                    _keyframeMeta.Positions[i] = (long)positionList[i];
                    _keyframeMeta.Timestamps[i] = (int)timestampList[i];
                    _posTimeMap.Add((long)positionList[i], (long)((int)timestampList[i]));
                }
                return _keyframeMeta;
            }
        }

        #endregion

        public int VideoCodecId
        {
            get
            {
		        KeyFrameMeta meta = AnalyzeKeyFrames();
	            if (meta == null)
        	        return -1;
		        long old = GetCurrentPosition();
		        SetCurrentPosition( _firstVideoTag );
		        ReadTagHeader();
                byte frametype = _reader.ReadByte();
		        SetCurrentPosition(old);
		        return frametype & IOConstants.MASK_VIDEO_CODEC;
            }
        }

        public int AudioCodecId
        {
            get
            {
                KeyFrameMeta meta = AnalyzeKeyFrames();
                if (meta == null)
                    return -1;

                long old = GetCurrentPosition();
                SetCurrentPosition(_firstAudioTag);
                ReadTagHeader();
                byte frametype = _reader.ReadByte();
                SetCurrentPosition(old);
                return frametype & IOConstants.MASK_SOUND_FORMAT;
            }
        }

        /// <summary>
        /// Create tag for metadata event.
        /// </summary>
        /// <returns></returns>
        private ITag CreateFileMeta()
        {
            // Create tag for onMetaData event
            ByteBuffer buf = ByteBuffer.Allocate(1024);
            buf.AutoExpand = true;
            AMFWriter output = new AMFWriter(buf);

            // Duration property
            output.WriteString("onMetaData");
            Dictionary<string, object> props = new Dictionary<string,object>();
            props.Add("duration", _duration / 1000.0);
            if (_firstVideoTag != -1)
            {
                long old = GetCurrentPosition();
                SetCurrentPosition(_firstVideoTag);
                ReadTagHeader();
                byte frametype = _reader.ReadByte();
                // Video codec id
                props.Add("videocodecid", frametype & IOConstants.MASK_VIDEO_CODEC);
                SetCurrentPosition(old);
            }
            if (_firstAudioTag != -1)
            {
                long old = GetCurrentPosition();
                SetCurrentPosition(_firstAudioTag);
                ReadTagHeader();
                byte frametype = _reader.ReadByte();
                // Audio codec id
                props.Add("audiocodecid", (frametype & IOConstants.MASK_SOUND_FORMAT) >> 4);
                SetCurrentPosition(old);
            }
            props.Add("canSeekToEnd", true);
            output.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            buf.Flip();

            ITag result = new Tag(IOConstants.TYPE_METADATA, 0, buf.Limit, buf.ToArray(), 0);
            return result;
        }

        public ByteBuffer GetFileData()
        {
            // TODO as of now, return null will disable cache
            // we need to redesign the cache architecture so that
            // the cache is layed underneath FLVReader not above it,
            // thus both tag cache and file cache are feasible.
            return null;
        }

        /// <summary>
        /// Read only header part of a tag.
        /// </summary>
        /// <returns></returns>
        private ITag ReadTagHeader()
        {
            // PREVIOUS TAG SIZE
            int previousTagSize = _reader.ReadInt32();
            // START OF FLV TAG
            byte dataType = _reader.ReadByte();
            int bodySize = _reader.ReadUInt24();
            int timestamp = _reader.ReadUInt24();
            timestamp |= _reader.ReadByte() << 24;
            int streamId = _reader.ReadUInt24();
            return new Tag(dataType, timestamp, bodySize, null, previousTagSize);
        }
    }
}
