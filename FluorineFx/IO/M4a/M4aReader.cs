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
using System.Text;
using System.IO;
using log4net;
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.IO.Mp4;

namespace FluorineFx.IO.M4a
{
    /// <summary>
    /// A reader used to read the contents of a M4A file.
    /// </summary>
    class M4aReader : ITagReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(M4aReader));

        object _syncLock = new object();
        private FileInfo _file;
        private FileStream _fs;
        private Mp4DataStream _inputStream;

        private String _audioCodecId = "mp4a";

        /// <summary>
        /// Decoder bytes / configs
        /// </summary>
        private byte[] _audioDecoderBytes;
        /// <summary>
        /// Duration in milliseconds.
        /// </summary>
        private long _duration;
        private int _timeScale;
        /// <summary>
        /// Audio sample rate kHz
        /// </summary>
        private double _audioTimeScale;
        private int _audioChannels;
        /// <summary>
        /// Default to aac lc
        /// </summary>
        private int _audioCodecType = 1;
        private String _formattedDuration;
        private long _moovOffset;
        private long _mdatOffset;
        /// <summary>
        /// Samples to chunk mappings
        /// </summary>
        private List<Mp4Atom.Record> _audioSamplesToChunks;
        /// <summary>
        /// Samples
        /// </summary>
        private List<int> _audioSamples;
        /// <summary>
        /// Chunk offsets
        /// </summary>
        private List<long> _audioChunkOffsets;
        /// <summary>
        /// Sample duration
        /// </summary>
        private int _audioSampleDuration = 1024;
        /// <summary>
        /// Keep track of current sample
        /// </summary>
        private int _currentFrame = 1;

        private int _prevFrameSize = 0;

        private List<Mp4Frame> _frames = new List<Mp4Frame>();

        /// <summary>
        /// Container for metadata and any other tags that should be sent prior to media data.
        /// </summary>
        private LinkedList<ITag> _firstTags = new LinkedList<ITag>();

        public M4aReader(FileInfo file)
        {
            _file = file;
            _fs = new FileStream(_file.FullName, FileMode.Open);
            _inputStream = new Mp4DataStream(_fs);
            //Decode all the info that we want from the atoms
            DecodeHeader();
            //Analyze the samples/chunks and build the keyframe meta data
            AnalyzeFrames();
            //Add meta data
            _firstTags.AddLast(CreateFileMeta());
            //Create / add the pre-streaming (decoder config) tags
            CreatePreStreamingTags();

        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        #region ITagReader Members

        public IStreamableFile File
        {
            get { return null; }
        }

        public int Offset
        {
            get { return (int)_inputStream.Offset; }
        }

        public long BytesRead
        {
            get { return _inputStream.Offset; }
        }

        public long Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// This handles the moov atom being at the beginning or end of the file, so the mdat may also be before or after the moov atom.
        /// </summary>
        public void DecodeHeader()
        {
            try
            {
                // the first atom will/should be the type
                Mp4Atom type = Mp4Atom.CreateAtom(_inputStream);
                // expect ftyp
                log.Debug(string.Format("Type {0}", type));
                // keep a running count of the number of atoms found at the "top" levels
                int topAtoms = 0;
                // we want a moov and an mdat, anything else throw the invalid file type error
                while (topAtoms < 2)
                {
                    Mp4Atom atom = Mp4Atom.CreateAtom(_inputStream);
                    switch (atom.Type)
                    {
                        case 1836019574: //moov
                            topAtoms++;
                            Mp4Atom moov = atom;
                            // expect moov
                            log.Debug(string.Format("Type {0}", moov));
                            //log.debug("moov children: {}", moov.getChildren());
                            _moovOffset = _inputStream.Offset - moov.Size;

                            Mp4Atom mvhd = moov.Lookup(Mp4Atom.TypeToInt("mvhd"), 0);
                            if (mvhd != null)
                            {
                                log.Debug("Movie header atom found");
                                //get the initial timescale
                                _timeScale = mvhd.TimeScale;
                                _duration = mvhd.Duration;
                                log.Debug(string.Format("Time scale {0} Duration {1}", _timeScale, _duration));
                            }

                            /* nothing needed here yet
                            MP4Atom meta = moov.lookup(MP4Atom.typeToInt("meta"), 0);
                            if (meta != null) {
                                log.debug("Meta atom found");
                                log.debug("{}", ToStringBuilder.reflectionToString(meta));
                            }
                            */

                            Mp4Atom trak = moov.Lookup(Mp4Atom.TypeToInt("trak"), 0);
                            if (trak != null)
                            {
                                log.Debug("Track atom found");
                                //log.debug("trak children: {}", trak.getChildren());
                                // trak: tkhd, edts, mdia

                                Mp4Atom edts = trak.Lookup(Mp4Atom.TypeToInt("edts"), 0);
                                if (edts != null)
                                {
                                    log.Debug("Edit atom found");
                                    //log.debug("edts children: {}", edts.getChildren());
                                }

                                Mp4Atom mdia = trak.Lookup(Mp4Atom.TypeToInt("mdia"), 0);
                                if (mdia != null)
                                {
                                    log.Debug("Media atom found");
                                    // mdia: mdhd, hdlr, minf

                                    int scale = 0;
                                    //get the media header atom
                                    Mp4Atom mdhd = mdia.Lookup(Mp4Atom.TypeToInt("mdhd"), 0);
                                    if (mdhd != null)
                                    {
                                        log.Debug("Media data header atom found");
                                        //this will be for either video or audio depending media info
                                        scale = mdhd.TimeScale;
                                        log.Debug(string.Format("Time scale {0}", scale));
                                    }

                                    Mp4Atom hdlr = mdia.Lookup(Mp4Atom.TypeToInt("hdlr"), 0);
                                    if (hdlr != null)
                                    {
                                        log.Debug("Handler ref atom found");
                                        // soun or vide
                                        log.Debug(string.Format("Handler type: {0}", Mp4Atom.IntToType(hdlr.HandlerType)));
                                        String hdlrType = Mp4Atom.IntToType(hdlr.HandlerType);
                                        if ("soun".Equals(hdlrType))
                                        {
                                            if (scale > 0)
                                            {
                                                _audioTimeScale = scale * 1.0;
                                                log.Debug(string.Format("Audio time scale: {0}", _audioTimeScale));
                                            }
                                        }
                                    }

                                    Mp4Atom minf = mdia.Lookup(Mp4Atom.TypeToInt("minf"), 0);
                                    if (minf != null)
                                    {
                                        log.Debug("Media info atom found");
                                        // minf: (audio) smhd, dinf, stbl / (video) vmhd,
                                        // dinf, stbl
                                        Mp4Atom smhd = minf.Lookup(Mp4Atom.TypeToInt("smhd"), 0);
                                        if (smhd != null)
                                        {
                                            log.Debug("Sound header atom found");
                                            Mp4Atom dinf = minf.Lookup(Mp4Atom.TypeToInt("dinf"), 0);
                                            if (dinf != null)
                                            {
                                                log.Debug("Data info atom found");
                                                // dinf: dref
                                                //log.Debug("Sound dinf children: {}", dinf.getChildren());
                                                Mp4Atom dref = dinf.Lookup(Mp4Atom.TypeToInt("dref"), 0);
                                                if (dref != null)
                                                {
                                                    log.Debug("Data reference atom found");
                                                }

                                            }
                                            Mp4Atom stbl = minf.Lookup(Mp4Atom.TypeToInt("stbl"), 0);
                                            if (stbl != null)
                                            {
                                                log.Debug("Sample table atom found");
                                                // stbl: stsd, stts, stss, stsc, stsz, stco,
                                                // stsh
                                                //log.debug("Sound stbl children: {}", stbl.getChildren());
                                                // stsd - sample description
                                                // stts - time to sample
                                                // stsc - sample to chunk
                                                // stsz - sample size
                                                // stco - chunk offset

                                                //stsd - has codec child
                                                Mp4Atom stsd = stbl.Lookup(Mp4Atom.TypeToInt("stsd"), 0);
                                                if (stsd != null)
                                                {
                                                    //stsd: mp4a
                                                    log.Debug("Sample description atom found");
                                                    Mp4Atom mp4a = stsd.Children[0];
                                                    //could set the audio codec here
                                                    SetAudioCodecId(Mp4Atom.IntToType(mp4a.Type));
                                                    //log.debug("{}", ToStringBuilder.reflectionToString(mp4a));
                                                    log.Debug(string.Format("Sample size: {0}", mp4a.SampleSize));
                                                    int ats = mp4a.TimeScale;
                                                    //skip invalid audio time scale
                                                    if (ats > 0)
                                                    {
                                                        _audioTimeScale = ats * 1.0;
                                                    }
                                                    _audioChannels = mp4a.ChannelCount;
                                                    log.Debug(string.Format("Sample rate (audio time scale): {0}", _audioTimeScale));
                                                    log.Debug(string.Format("Channels: {0}", _audioChannels));
                                                    //mp4a: esds
                                                    if (mp4a.Children.Count > 0)
                                                    {
                                                        log.Debug("Elementary stream descriptor atom found");
                                                        Mp4Atom esds = mp4a.Children[0];
                                                        //log.debug("{}", ToStringBuilder.reflectionToString(esds));
                                                        Mp4Descriptor descriptor = esds.EsdDescriptor;
                                                        //log.debug("{}", ToStringBuilder.reflectionToString(descriptor));
                                                        if (descriptor != null)
                                                        {
                                                            List<Mp4Descriptor> children = descriptor.Children;
                                                            for (int e = 0; e < children.Count; e++)
                                                            {
                                                                Mp4Descriptor descr = children[e];
                                                                //log.debug("{}", ToStringBuilder.reflectionToString(descr));
                                                                if (descr.Children.Count > 0)
                                                                {
                                                                    List<Mp4Descriptor> children2 = descr.Children;
                                                                    for (int e2 = 0; e2 < children2.Count; e2++)
                                                                    {
                                                                        Mp4Descriptor descr2 = children2[e2];
                                                                        //log.debug("{}", ToStringBuilder.reflectionToString(descr2));
                                                                        if (descr2.Type == Mp4Descriptor.MP4DecSpecificInfoDescriptorTag)
                                                                        {
                                                                            //we only want the MP4DecSpecificInfoDescriptorTag
                                                                            _audioDecoderBytes = descr2.DSID;
                                                                            //compare the bytes to get the aacaot/aottype 
                                                                            //match first byte
                                                                            switch (_audioDecoderBytes[0])
                                                                            {
                                                                                case 0x12:
                                                                                default:
                                                                                    //AAC LC - 12 10
                                                                                    _audioCodecType = 1;
                                                                                    break;
                                                                                case 0x0a:
                                                                                    //AAC Main - 0A 10
                                                                                    _audioCodecType = 0;
                                                                                    break;
                                                                                case 0x11:
                                                                                case 0x13:
                                                                                    //AAC LC SBR - 11 90 & 13 xx
                                                                                    _audioCodecType = 2;
                                                                                    break;
                                                                            }
                                                                            //we want to break out of top level for loop
                                                                            e = 99;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                //stsc - has Records
                                                Mp4Atom stsc = stbl.Lookup(Mp4Atom.TypeToInt("stsc"), 0);
                                                if (stsc != null)
                                                {
                                                    log.Debug("Sample to chunk atom found");
                                                    _audioSamplesToChunks = stsc.Records;
                                                    log.Debug(string.Format("Record count: {0}", _audioSamplesToChunks.Count));
                                                    Mp4Atom.Record rec = _audioSamplesToChunks[0];
                                                    log.Debug(string.Format("Record data: Description index={0} Samples per chunk={1}", rec.SampleDescriptionIndex, rec.SamplesPerChunk));
                                                }
                                                //stsz - has Samples
                                                Mp4Atom stsz = stbl.Lookup(Mp4Atom.TypeToInt("stsz"), 0);
                                                if (stsz != null)
                                                {
                                                    log.Debug("Sample size atom found");
                                                    _audioSamples = stsz.Samples;
                                                    //vector full of integers										
                                                    log.Debug(string.Format("Sample size: {0}", stsz.SampleSize));
                                                    log.Debug(string.Format("Sample count: {0}", _audioSamples.Count));
                                                }
                                                //stco - has Chunks
                                                Mp4Atom stco = stbl.Lookup(Mp4Atom.TypeToInt("stco"), 0);
                                                if (stco != null)
                                                {
                                                    log.Debug("Chunk offset atom found");
                                                    //vector full of integers
                                                    _audioChunkOffsets = stco.Chunks;
                                                    log.Debug(string.Format("Chunk count: {0}", _audioChunkOffsets.Count));
                                                }
                                                //stts - has TimeSampleRecords
                                                Mp4Atom stts = stbl.Lookup(Mp4Atom.TypeToInt("stts"), 0);
                                                if (stts != null)
                                                {
                                                    log.Debug("Time to sample atom found");
                                                    List<Mp4Atom.TimeSampleRecord> records = stts.TimeToSamplesRecords;
                                                    log.Debug(string.Format("Record count: {0}", records.Count));
                                                    Mp4Atom.TimeSampleRecord rec = records[0];
                                                    log.Debug(string.Format("Record data: Consecutive samples={0} Duration={1}", rec.ConsecutiveSamples, rec.SampleDuration));
                                                    //if we have 1 record then all samples have the same duration
                                                    if (records.Count > 1)
                                                    {
                                                        //TODO: handle audio samples with varying durations
                                                        log.Debug("Audio samples have differing durations, audio playback may fail");
                                                    }
                                                    _audioSampleDuration = rec.SampleDuration;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //real duration
                            StringBuilder sb = new StringBuilder();
                            double clipTime = ((double)_duration / (double)_timeScale);
                            log.Debug(string.Format("Clip time: {0}", clipTime));
                            int minutes = (int)(clipTime / 60);
                            if (minutes > 0)
                            {
                                sb.Append(minutes);
                                sb.Append('.');
                            }
                            //formatter for seconds / millis
                            //NumberFormat df = DecimalFormat.getInstance();
                            //df.setMaximumFractionDigits(2);
                            //sb.append(df.format((clipTime % 60)));
                            sb.Append(clipTime % 60);
                            _formattedDuration = sb.ToString();
                            log.Debug(string.Format("Time: {0}", _formattedDuration));
                            break;
                        case 1835295092: //mdat
                            topAtoms++;
                            long dataSize = 0L;
                            Mp4Atom mdat = atom;
                            dataSize = mdat.Size;
                            //log.debug("{}", ToStringBuilder.reflectionToString(mdat));
                            _mdatOffset = _inputStream.Offset - dataSize;
                            log.Debug(string.Format("File size: {0} mdat size: {1}", _file.Length, dataSize));
                            break;
                        case 1718773093: //free
                        case 2003395685: //wide
                            break;
                        default:
                            log.Warn(string.Format("Unexpected atom: {}", Mp4Atom.IntToType(atom.Type)));
                            break;
                    }
                }
                //add the tag name (size) to the offsets
                _moovOffset += 8;
                _mdatOffset += 8;
                log.Debug(string.Format("Offsets moov: {0} mdat: {1}", _moovOffset, _mdatOffset));
            }
            catch (Exception ex)
            {
                log.Error("Exception decoding header / atoms", ex);
            }
        }

        public long Position
        {
            get
            {
                return _inputStream.Offset;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool HasMoreTags()
        {
            return _currentFrame < _frames.Count;
        }

        public ITag ReadTag()
        {
            lock (this.SyncRoot)
            {
                ITag tag = null;
                //empty-out the pre-streaming tags first
                if (_firstTags.Count > 0)
                {
                    //log.debug("Returning pre-tag");
                    // Return first tags before media data
                    tag = _firstTags.First.Value;
                    _firstTags.RemoveFirst();
                    return tag;
                }
                //log.debug("Read tag - sample {} prevFrameSize {} audio: {} video: {}", new Object[]{currentSample, prevFrameSize, audioCount, videoCount});

                //get the current frame
                Mp4Frame frame = _frames[_currentFrame];
                log.Debug(string.Format("Playback #{0} {1}", _currentFrame, frame));

                int sampleSize = frame.Size;

                int time = (int)Math.Round(frame.Time * 1000.0);
                //log.debug("Read tag - dst: {} base: {} time: {}", new Object[]{frameTs, baseTs, time});

                long samplePos = frame.Offset;
                //log.debug("Read tag - samplePos {}", samplePos);

                //determine frame type and packet body padding
                byte type = frame.Type;

                //create a byte buffer of the size of the sample
                byte[] data = new byte[sampleSize + 2];
                try
                {
                    Array.Copy(Mp4Reader.PREFIX_AUDIO_FRAME, data, Mp4Reader.PREFIX_AUDIO_FRAME.Length);
                    //do we need to add the mdat offset to the sample position?
                    _fs.Position = samplePos;
                    _fs.Read(data, Mp4Reader.PREFIX_AUDIO_FRAME.Length, sampleSize);
                }
                catch (Exception ex)
                {
                    log.Error("Error on channel position / read", ex);
                }

                //create the tag
                tag = new Tag(type, time, data.Length, data, _prevFrameSize);
                //increment the frame number
                _currentFrame++;
                //set the frame / tag size
                _prevFrameSize = tag.BodySize;
                //log.debug("Tag: {}", tag);
                return tag;
            }
        }

        public void Close()
        {
            _inputStream.Close();
        }

        public bool HasVideo()
        {
            return false;
        }

        #endregion

        public void SetAudioCodecId(String audioCodecId)
        {
            this._audioCodecId = audioCodecId;
        }

        /// <summary>
        /// Tag sequence
        /// MetaData, Audio config, remaining audio
        /// 
        /// Packet prefixes:
        /// af 00 ...   06 = Audio extra data (first audio packet)
        /// af 01          = Audio frame
        /// 
        /// Audio extra data(s):
        /// af 00                = Prefix
        /// 11 90 4f 14          = AAC Main   = aottype 0
        /// 12 10                = AAC LC     = aottype 1
        /// 13 90 56 e5 a5 48 00 = HE-AAC SBR = aottype 2
        /// 06                   = Suffix
        /// 
        /// Still not absolutely certain about this order or the bytes - need to verify later
        /// </summary>
        private void CreatePreStreamingTags()
        {
            log.Debug("Creating pre-streaming tags");
            ByteBuffer body = ByteBuffer.Allocate(41);
            body.AutoExpand = true;
            body.Put(new byte[] { (byte)0xaf, (byte)0 }); //prefix
            if (_audioDecoderBytes != null)
            {
                body.Put(_audioDecoderBytes);
            }
            else
            {
                //default to aac-lc when the esds doesnt contain descripter bytes
                body.Put(Mp4Reader.AUDIO_CONFIG_FRAME_AAC_LC);
            }
            body.Put((byte)0x06); //suffix
            ITag tag = new Tag(IOConstants.TYPE_AUDIO, 0, (int)body.Length, body.ToArray(), _prevFrameSize);
            //add tag
            _firstTags.AddLast(tag);
        }

        /// <summary>
        /// Create tag for metadata event.
        /// </summary>
        /// <returns></returns>
        ITag CreateFileMeta()
        {
            log.Debug("Creating onMetaData");
            // Create tag for onMetaData event
            ByteBuffer buf = ByteBuffer.Allocate(1024);
            buf.AutoExpand = true;
            AMFWriter output = new AMFWriter(buf);
            output.WriteString("onMetaData");

            Hashtable props = new Hashtable();
            // Duration property
            props.Add("duration", ((double)_duration / (double)_timeScale));
            // Audio codec id - watch for mp3 instead of aac
            props.Add("audiocodecid", _audioCodecId);
            props.Add("aacaot", _audioCodecType);
            props.Add("audiosamplerate", _audioTimeScale);
            props.Add("audiochannels", _audioChannels);

            props.Add("moovposition", _moovOffset);
            //tags will only appear if there is an "ilst" atom in the file
            //props.put("tags", "");

            props.Add("canSeekToEnd", false);
            output.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            buf.Flip();

            //now that all the meta properties are done, update the duration
            _duration = (long)Math.Round(_duration * 1000d);

            ITag result = new Tag(IOConstants.TYPE_METADATA, 0, buf.Limit, buf.ToArray(), 0);
            return result;
        }

        /// <summary>
        /// Performs frame analysis and generates metadata for use in seeking. All the frames are analyzed and sorted together based on time and offset.
        /// </summary>
        public void AnalyzeFrames()
        {
            log.Debug("Analyzing frames");
            // tag == sample
            int sample = 1;
            long pos = 0;

            //add the audio frames / samples / chunks
            for (int i = 0; i < _audioSamplesToChunks.Count; i++)
            {
                Mp4Atom.Record record = _audioSamplesToChunks[i];
                int firstChunk = record.FirstChunk;
                int lastChunk = _audioChunkOffsets.Count;
                if (i < _audioSamplesToChunks.Count - 1)
                {
                    Mp4Atom.Record nextRecord = _audioSamplesToChunks[i + 1];
                    lastChunk = nextRecord.FirstChunk - 1;
                }
                for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
                {
                    int sampleCount = record.SamplesPerChunk;
                    pos = _audioChunkOffsets[chunk - 1];
                    while (sampleCount > 0)
                    {
                        //calculate ts
                        double ts = (_audioSampleDuration * (sample - 1)) / _audioTimeScale;
                        //sample size
                        int size = _audioSamples[sample - 1];
                        //create a frame
                        Mp4Frame frame = new Mp4Frame();
                        frame.Offset = pos;
                        frame.Size = size;
                        frame.Time = ts;
                        frame.Type = IOConstants.TYPE_AUDIO;
                        _frames.Add(frame);
                        //log.debug("Sample #{} {}", sample, frame);
                        //inc and dec stuff
                        pos += size;
                        sampleCount--;
                        sample++;
                    }
                }
            }
            //sort the frames
            _frames.Sort();

            log.Debug(string.Format("Frames count: {0}", _frames.Count));
        }
    }
}
