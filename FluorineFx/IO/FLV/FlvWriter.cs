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
    /// A Writer is used to write the contents of a FLV file
    /// </summary>
    class FlvWriter : ITagWriter
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(FlvWriter));
#endif
        object _syncLock = new object();

        private AMFWriter _writer;

        /// <summary>
        /// Number of bytes written
        /// </summary>
        private long _bytesWritten;
        /// <summary>
        /// Size of tag containing onMetaData.
        /// </summary>
        private int _fileMetaSize = 0;
        /// <summary>
        /// Id of the video codec used.
        /// </summary>
        private int _videoCodecId = -1;
        /// <summary>
        /// Id of the audio codec used.
        /// </summary>
        private int _audioCodecId = -1;
        /// <summary>
        /// Are we appending to an existing file?
        /// </summary>
        private bool _append;
        /// <summary>
        /// Duration of the file.
        /// </summary>
        private int _duration;
        /// <summary>
        /// Position of the meta data tag in our file.
        /// </summary>
        private long _metaPosition;

        public FlvWriter(Stream stream, bool append)
        {
            _writer = new AMFWriter(stream);
            _append = append;
            if (_append)
            {
                if (stream.Length > 9 + 15)
                {
                    try
                    {
                        //Skip header
                        stream.Position = 9;
                        byte[] tagBuffer = new byte[15];
                        //previousTagSize
                        stream.Read(tagBuffer, 0, 4);
                        //start of flv tag
                        byte dataType = (byte)stream.ReadByte();
                        if (dataType == IOConstants.TYPE_METADATA)
                        {
                            _metaPosition = stream.Position - 1;
                            //body size
                            stream.Read(tagBuffer, 5, 3);
                            int bodySize = tagBuffer[5] << 16 | tagBuffer[6] << 8 | tagBuffer[7];
                            //timestamp
                            stream.Read(tagBuffer, 8, 4);
                            //streamid
                            stream.Read(tagBuffer, 12, 3);
                            byte[] buffer = new byte[bodySize];
                            stream.Read(buffer, 0, buffer.Length);
                            MemoryStream ms = new MemoryStream(buffer);
                            AMFReader input = new AMFReader(ms);
                            string onMetaData = input.ReadData() as string;//input.ReadString();
                            IDictionary properties = input.ReadData() as IDictionary;
                            if (properties.Contains("duration"))
                                _duration = System.Convert.ToInt32(properties["duration"]);
                            else
                            {
#if !SILVERLIGHT
                                log.Warn("Could not read Flv duration from metadata");
#endif
                            }
                        }
                        else
                        {
#if !SILVERLIGHT
                            log.Warn("Could not read Flv duration");
#endif
                        }
                    }
                    catch (Exception ex)
                    {
#if !SILVERLIGHT
                        log.Warn("Error reading Flv duration", ex);
#endif
                    }
                }
                stream.Seek(0, SeekOrigin.End);//Appending
            }
        }

        #region ITagWriter Members

        public IStreamableFile File
        {
            get { return null; }
        }

        public long Position
        {
            get { return _writer.BaseStream.Position; }
        }

        public long BytesWritten
        {
            get { return _bytesWritten; }
        }

        public void WriteHeader()
        {
		    _writer.WriteByte((byte)0x46);
		    _writer.WriteByte((byte)0x4C);
		    _writer.WriteByte((byte)0x56);
		    // Write version
		    _writer.WriteByte((byte) 0x01);

		    // 0x05 for audio and video
		    _writer.WriteByte((byte)0x05);
		    // Data Offset
		    _writer.WriteInt32(0x09);
		    // First lastTagSize
		    // Always zero
		    _writer.WriteInt32(0);
        }

        public bool WriteTag(ITag tag)
        {
            long start = _writer.BaseStream.Position;
		    if (!_append && _bytesWritten == 0 && tag.DataType != IOConstants.TYPE_METADATA) 
            {
			    // Write intermediate onMetaData tag, will be replaced later
			    WriteMetadataTag(0, -1, -1);
		    }
            // Data Type
            _writer.WriteByte(tag.DataType);
		    // Body Size
            _writer.WriteUInt24(tag.BodySize);
		    // Timestamp
            _writer.WriteUInt24(tag.Timestamp);
            byte extended = (byte)((tag.Timestamp & 0xFF000000) >> 24);
		    // Reserved
            _writer.WriteByte(extended);
            _writer.WriteUInt24(0x00);

            if (tag.BodySize != 0)//???
            {
                byte[] bodyBuffer = tag.Body;
                _writer.WriteBytes(bodyBuffer);

                if (_audioCodecId == -1 && tag.DataType == IOConstants.TYPE_AUDIO)
                {
                    byte id = bodyBuffer[0];
                    _audioCodecId = (id & IOConstants.MASK_SOUND_FORMAT) >> 4;
                }
                else if (_videoCodecId == -1 && tag.DataType == IOConstants.TYPE_VIDEO)
                {
                    byte id = bodyBuffer[0];
                    _videoCodecId = id & IOConstants.MASK_VIDEO_CODEC;
                }
            }
		    _duration = Math.Max(_duration, tag.Timestamp);
		    // Add the tag size
    		_writer.WriteInt32(tag.BodySize + 11);
		    _bytesWritten += (_writer.BaseStream.Position - start);
		    return true;
        }

        public bool WriteTag(byte type, ByteBuffer data)
        {
            return false;
        }

        public bool WriteStream(byte[] buffer)
        {
            return false;
        }

        public void Close()
        {
            try
            {
                if (_metaPosition > 0)
                {
                    long oldPos = _writer.BaseStream.Position;
                    try
                    {
                        _writer.BaseStream.Position = _metaPosition;
                        WriteMetadataTag(_duration * 0.001, _videoCodecId, _audioCodecId);
                    }
                    finally
                    {
                        _writer.BaseStream.Position = oldPos;
                    }
                }
                _writer.Close();
            }
            catch (IOException ex)
            {
#if !SILVERLIGHT
                log.Error("FlvWriter close", ex);
#endif
            }
        }

        #endregion

        /// <summary>
        /// Write "onMetaData" tag to the file.
        /// </summary>
        /// <param name="duration">Duration to write in milliseconds.</param>
        /// <param name="videoCodecId">Id of the video codec used while recording.</param>
        /// <param name="audioCodecId">Id of the audio codec used while recording.</param>
        private void WriteMetadataTag(double duration, object videoCodecId, object audioCodecId)
        {
            _metaPosition = _writer.BaseStream.Position;
            MemoryStream ms = new MemoryStream();
            AMFWriter output = new AMFWriter(ms);
            output.WriteString("onMetaData");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("duration", _duration);
            if (videoCodecId != null)
            {
                props.Add("videocodecid", videoCodecId);
            }
            if (audioCodecId != null)
            {
                props.Add("audiocodecid", audioCodecId);
            }
            props.Add("canSeekToEnd", true);
            output.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            byte[] buffer = ms.ToArray();
            if (_fileMetaSize == 0)
            {
                _fileMetaSize = buffer.Length;
            }
            ITag onMetaData = new Tag(IOConstants.TYPE_METADATA, 0, buffer.Length, buffer, 0);
            WriteTag(onMetaData);
        }
    }
}
