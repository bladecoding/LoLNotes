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
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO.Mp4
{
    /// <summary>
    /// This reader is used to read the contents of an MP4 file.
    /// </summary>
    class Mp4Reader : ITagReader
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(Mp4Reader));
#endif
        /// <summary>
        /// Audio packet prefix
        /// </summary>
	    public static byte[] PREFIX_AUDIO_FRAME = new byte[]{(byte) 0xaf, (byte) 0x01};
	    /// <summary>
	    /// Audio config aac main
	    /// </summary>
	    public static byte[] AUDIO_CONFIG_FRAME_AAC_MAIN = new byte[]{(byte) 0x0a, (byte) 0x10};
	    /// <summary>
	    /// Audio config aac lc
	    /// </summary>
	    public static byte[] AUDIO_CONFIG_FRAME_AAC_LC = new byte[]{(byte) 0x12, (byte) 0x10};
        /// <summary>
        /// Audio config sbr
        /// </summary>
	    public static byte[] AUDIO_CONFIG_FRAME_SBR = new byte[]{(byte) 0x13, (byte) 0x90, (byte) 0x56, (byte) 0xe5, (byte) 0xa5, (byte) 0x48, (byte) 0x00};
        /// <summary>
        /// Video packet prefix for the decoder frame
        /// </summary>
	    public static byte[] PREFIX_VIDEO_CONFIG_FRAME = new byte[]{(byte) 0x17, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00};
        /// <summary>
        /// Video packet prefix for key frames
        /// </summary>
	    public static byte[] PREFIX_VIDEO_KEYFRAME = new byte[]{(byte) 0x17, (byte) 0x01, (byte) 0, (byte) 0, (byte) 0};
        /// <summary>
        /// Video packet prefix for standard frames (interframe)
        /// </summary>
	    public static byte[] PREFIX_VIDEO_FRAME = new byte[]{(byte) 0x27, (byte) 0x01, (byte) 0, (byte) 0, (byte) 0};

        object _syncLock = new object();
        //private FileInfo _file;
        private Stream _stream;
        private Mp4DataStream _inputStream;
        /// <summary>
        /// Container for metadata and any other tags that should be sent prior to media data.
        /// </summary>
        private LinkedList<ITag> _firstTags = new LinkedList<ITag>();
        /// <summary>
        /// Container for seek points in the video. These are the time stamps for the key frames.
        /// </summary>
        private LinkedList<int> _seekPoints;

        /// <summary>
        /// Mapping between file position and timestamp in ms.
        /// </summary>
        private Dictionary<int, long> _timePosMap;

        private Dictionary<int, long> _samplePosMap;


        /// <summary>
        /// Whether or not the clip contains a video track.
        /// </summary>
        private bool _hasVideo = false;
        /// <summary>
        /// Whether or not the clip contains an audio track.
        /// </summary>
        private bool _hasAudio = false;
        /// <summary>
        /// Default video codec.
        /// </summary>
        private String _videoCodecId = "avc1";
        /// <summary>
        /// Default audio codec.
        /// </summary>
        private String _audioCodecId = "mp4a";

        /// <summary>
        /// Decoder bytes / configs.
        /// </summary>
        private byte[] _audioDecoderBytes;
        private byte[] _videoDecoderBytes;

        /// <summary>
        /// Duration in milliseconds.
        /// </summary>
        private long _duration;
        /// <summary>
        /// Movie time scale.
        /// </summary>
        private int _timeScale;
        private int _width;
        private int _height;
        /// <summary>
        /// Audio sample rate kHz.
        /// </summary>
        private double _audioTimeScale;
        private int _audioChannels;
        /// <summary>
        /// Default to aac lc
        /// </summary>
        private int _audioCodecType = 1;

        private int _videoSampleCount;
        private double _fps;
        private double _videoTimeScale;
        private int _avcLevel;
        private int _avcProfile;
        private String _formattedDuration;
        private long _moovOffset;
        private long _mdatOffset;

        /// <summary>
        /// Samples to chunk mappings.
        /// </summary>
        private List<Mp4Atom.Record> _videoSamplesToChunks;
        private List<Mp4Atom.Record> _audioSamplesToChunks;
        /// <summary>
        /// Keyframe - sample numbers.
        /// </summary>
        private List<int> _syncSamples;
        /// <summary>
        /// Samples.
        /// </summary>
        private List<int> _videoSamples;
        private List<int> _audioSamples;
        /// <summary>
        /// Chunk offsets.
        /// </summary>
        private List<long> _videoChunkOffsets;
        private List<long> _audioChunkOffsets;

        /// <summary>
        /// Sample duration.
        /// </summary>
        private int _videoSampleDuration = 125;
        private int _audioSampleDuration = 1024;

        /// <summary>
        /// Keeps track of current frame / sample.
        /// </summary>
        private int _currentFrame = 0;

        private int _prevFrameSize = 0;

        private List<Mp4Frame> _frames = new List<Mp4Frame>();

        private long _audioCount;
        private long _videoCount;

        /// <summary>
        /// Creates MP4 reader from file input stream, sets up metadata generation flag.
        /// </summary>
        /// <param name="file"></param>
        public Mp4Reader(FileInfo file)
        {
            //_file = file;
            _stream = new FileStream(file.FullName, FileMode.Open);
            _inputStream = new Mp4DataStream(_stream);
            //Decode all the info that we want from the atoms
            DecodeHeader();
            //Analyze the samples/chunks and build the keyframe meta data
            AnalyzeFrames();
            //Add meta data
            _firstTags.AddLast(CreateFileMeta());
            //Create / add the pre-streaming (decoder config) tags
            CreatePreStreamingTags();
        }

        public Mp4Reader(Stream stream)
        {
            _stream = stream;
            _inputStream = new Mp4DataStream(_stream);
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        private long GetCurrentPosition()
        {
            return _inputStream.Offset;
        }

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
            get { return GetCurrentPosition(); }
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
                // The first atom will/should be the type
                Mp4Atom type = Mp4Atom.CreateAtom(_inputStream);
                // Expect ftyp
#if !SILVERLIGHT
                log.Debug(string.Format("Type {0}", type));
#endif
                //log.debug("Atom int types - free={} wide={}", MP4Atom.typeToInt("free"), MP4Atom.typeToInt("wide"));
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
                            #if !SILVERLIGHT
                            log.Debug(string.Format("Type {0}", moov));
#endif
                            //log.Debug("moov children: {}", moov.getChildren());
                            _moovOffset = _inputStream.Offset - moov.Size;

                            Mp4Atom mvhd = moov.Lookup(Mp4Atom.TypeToInt("mvhd"), 0);
                            if (mvhd != null)
                            {
#if !SILVERLIGHT
                                log.Debug("Movie header atom found");
#endif
                                //get the initial timescale
                                _timeScale = mvhd.TimeScale;
                                _duration = mvhd.Duration;
#if !SILVERLIGHT
                                log.Debug(string.Format("Time scale {0} Duration {1}", _timeScale, _duration));
#endif
                            }

                            /* nothing needed here yet
                            MP4Atom meta = moov.lookup(MP4Atom.typeToInt("meta"), 0);
                            if (meta != null) {
                                log.debug("Meta atom found");
                                log.debug("{}", ToStringBuilder.reflectionToString(meta));
                            }
                            */

                            //two tracks or bust
                            int i = 0;
                            while (i < 2)
                            {

                                Mp4Atom trak = moov.Lookup(Mp4Atom.TypeToInt("trak"), i);
                                if (trak != null)
                                {
#if !SILVERLIGHT
                                    log.Debug("Track atom found");
#endif
                                    //log.debug("trak children: {}", trak.getChildren());
                                    // trak: tkhd, edts, mdia
                                    Mp4Atom tkhd = trak.Lookup(Mp4Atom.TypeToInt("tkhd"), 0);
                                    if (tkhd != null)
                                    {
#if !SILVERLIGHT
                                        log.Debug("Track header atom found");
#endif
                                        //log.debug("tkhd children: {}", tkhd.getChildren());
                                        if (tkhd.Width > 0)
                                        {
                                            _width = tkhd.Width;
                                            _height = tkhd.Height;
#if !SILVERLIGHT
                                            log.Debug(string.Format("Width {0} x Height {1}", _width, _height));
#endif
                                        }
                                    }

                                    Mp4Atom edts = trak.Lookup(Mp4Atom.TypeToInt("edts"), 0);
                                    if (edts != null)
                                    {
#if !SILVERLIGHT
                                        log.Debug("Edit atom found");
#endif
                                        //log.debug("edts children: {}", edts.getChildren());
                                        //log.debug("Width {} x Height {}", edts.getWidth(), edts.getHeight());
                                    }

                                    Mp4Atom mdia = trak.Lookup(Mp4Atom.TypeToInt("mdia"), 0);
                                    if (mdia != null)
                                    {
#if !SILVERLIGHT
                                        log.Debug("Media atom found");
#endif
                                        // mdia: mdhd, hdlr, minf

                                        int scale = 0;
                                        //get the media header atom
                                        Mp4Atom mdhd = mdia.Lookup(Mp4Atom.TypeToInt("mdhd"), 0);
                                        if (mdhd != null)
                                        {
#if !SILVERLIGHT
                                            log.Debug("Media data header atom found");
#endif
                                            //this will be for either video or audio depending media info
                                            scale = mdhd.TimeScale;
#if !SILVERLIGHT
                                            log.Debug(string.Format("Time scale {0}", scale));
#endif
                                        }

                                        Mp4Atom hdlr = mdia.Lookup(Mp4Atom.TypeToInt("hdlr"), 0);
                                        if (hdlr != null)
                                        {
#if !SILVERLIGHT
                                            log.Debug("Handler ref atom found");
                                            // soun or vide
                                            log.Debug(string.Format("Handler type: {0}", Mp4Atom.IntToType(hdlr.HandlerType)));
#endif
                                            String hdlrType = Mp4Atom.IntToType(hdlr.HandlerType);
                                            if ("vide".Equals(hdlrType))
                                            {
                                                _hasVideo = true;
                                                if (scale > 0)
                                                {
                                                    _videoTimeScale = scale * 1.0;
#if !SILVERLIGHT
                                                    log.Debug(string.Format("Video time scale: {0}", _videoTimeScale));
#endif
                                                }
                                            }
                                            else if ("soun".Equals(hdlrType))
                                            {
                                                _hasAudio = true;
                                                if (scale > 0)
                                                {
                                                    _audioTimeScale = scale * 1.0;
#if !SILVERLIGHT
                                                    log.Debug(string.Format("Audio time scale: {0}", _audioTimeScale));
#endif
                                                }
                                            }
                                            i++;
                                        }

                                        Mp4Atom minf = mdia.Lookup(Mp4Atom.TypeToInt("minf"), 0);
                                        if (minf != null)
                                        {
#if !SILVERLIGHT
                                            log.Debug("Media info atom found");
#endif
                                            // minf: (audio) smhd, dinf, stbl / (video) vmhd,
                                            // dinf, stbl

                                            Mp4Atom smhd = minf.Lookup(Mp4Atom.TypeToInt("smhd"), 0);
                                            if (smhd != null)
                                            {
#if !SILVERLIGHT
                                                log.Debug("Sound header atom found");
#endif
                                                Mp4Atom dinf = minf.Lookup(Mp4Atom.TypeToInt("dinf"), 0);
                                                if (dinf != null)
                                                {
#if !SILVERLIGHT
                                                    log.Debug("Data info atom found");
#endif
                                                    // dinf: dref
                                                    //log.Debug("Sound dinf children: {}", dinf.getChildren());
                                                    Mp4Atom dref = dinf.Lookup(Mp4Atom.TypeToInt("dref"), 0);
                                                    if (dref != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Data reference atom found");
#endif
                                                    }

                                                }
                                                Mp4Atom stbl = minf.Lookup(Mp4Atom.TypeToInt("stbl"), 0);
                                                if (stbl != null)
                                                {
#if !SILVERLIGHT
                                                    log.Debug("Sample table atom found");
#endif
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
#if !SILVERLIGHT
                                                        log.Debug("Sample description atom found");
#endif
                                                        Mp4Atom mp4a = stsd.Children[0];
                                                        //could set the audio codec here
                                                        SetAudioCodecId(Mp4Atom.IntToType(mp4a.Type));
                                                        //log.debug("{}", ToStringBuilder.reflectionToString(mp4a));
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Sample size: {0}", mp4a.SampleSize));
#endif
                                                        int ats = mp4a.TimeScale;
                                                        //skip invalid audio time scale
                                                        if (ats > 0)
                                                        {
                                                            _audioTimeScale = ats * 1.0;
                                                        }
                                                        _audioChannels = mp4a.ChannelCount;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Sample rate (audio time scale): {0}", _audioTimeScale));
                                                        log.Debug(string.Format("Channels: {0}", _audioChannels));
#endif
                                                        //mp4a: esds
                                                        if (mp4a.Children.Count > 0)
                                                        {
#if !SILVERLIGHT
                                                            log.Debug("Elementary stream descriptor atom found");
#endif
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
#if !SILVERLIGHT
                                                        log.Debug("Sample to chunk atom found");
#endif
                                                        _audioSamplesToChunks = stsc.Records;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record count: {0}", _audioSamplesToChunks.Count));
#endif
                                                        Mp4Atom.Record rec = _audioSamplesToChunks[0];
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record data: Description index={0} Samples per chunk={1}", rec.SampleDescriptionIndex, rec.SamplesPerChunk));
#endif
                                                    }
                                                    //stsz - has Samples
                                                    Mp4Atom stsz = stbl.Lookup(Mp4Atom.TypeToInt("stsz"), 0);
                                                    if (stsz != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Sample size atom found");
#endif
                                                        _audioSamples = stsz.Samples;
                                                        //vector full of integers
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Sample size: {0}", stsz.SampleSize));
                                                        log.Debug(string.Format("Sample count: {0}", _audioSamples.Count));
#endif
                                                    }
                                                    //stco - has Chunks
                                                    Mp4Atom stco = stbl.Lookup(Mp4Atom.TypeToInt("stco"), 0);
                                                    if (stco != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Chunk offset atom found");
#endif
                                                        //vector full of integers
                                                        _audioChunkOffsets = stco.Chunks;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Chunk count: {0}", _audioChunkOffsets.Count));
#endif
                                                    }
                                                    //stts - has TimeSampleRecords
                                                    Mp4Atom stts = stbl.Lookup(Mp4Atom.TypeToInt("stts"), 0);
                                                    if (stts != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Time to sample atom found");
#endif
                                                        List<Mp4Atom.TimeSampleRecord> records = stts.TimeToSamplesRecords;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record count: {0}", records.Count));
#endif
                                                        Mp4Atom.TimeSampleRecord rec = records[0];
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record data: Consecutive samples={0} Duration={1}", rec.ConsecutiveSamples, rec.SampleDuration));
#endif
                                                        //if we have 1 record then all samples have the same duration
                                                        if (records.Count > 1)
                                                        {
                                                            //TODO: handle audio samples with varying durations
#if !SILVERLIGHT
                                                            log.Debug("Audio samples have differing durations, audio playback may fail");
#endif
                                                        }
                                                        _audioSampleDuration = rec.SampleDuration;
                                                    }
                                                }
                                            }
                                            Mp4Atom vmhd = minf.Lookup(Mp4Atom.TypeToInt("vmhd"), 0);
                                            if (vmhd != null)
                                            {
#if !SILVERLIGHT
                                                log.Debug("Video header atom found");
#endif
                                                Mp4Atom dinf = minf.Lookup(Mp4Atom.TypeToInt("dinf"), 0);
                                                if (dinf != null)
                                                {
#if !SILVERLIGHT
                                                    log.Debug("Data info atom found");
#endif
                                                    // dinf: dref
                                                    //log.debug("Video dinf children: {}", dinf.getChildren());
                                                    Mp4Atom dref = dinf.Lookup(Mp4Atom.TypeToInt("dref"), 0);
                                                    if (dref != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Data reference atom found");
#endif
                                                    }
                                                }
                                                Mp4Atom stbl = minf.Lookup(Mp4Atom.TypeToInt("stbl"), 0);
                                                if (stbl != null)
                                                {
#if !SILVERLIGHT
                                                    log.Debug("Sample table atom found");
#endif
                                                    // stbl: stsd, stts, stss, stsc, stsz, stco,
                                                    // stsh
                                                    //log.debug("Video stbl children: {}", stbl.getChildren());
                                                    // stsd - sample description
                                                    // stts - (decoding) time to sample
                                                    // stsc - sample to chunk
                                                    // stsz - sample size
                                                    // stco - chunk offset
                                                    // ctts - (composition) time to sample
                                                    // stss - sync sample
                                                    // sdtp - independent and disposable samples

                                                    //stsd - has codec child
                                                    Mp4Atom stsd = stbl.Lookup(Mp4Atom.TypeToInt("stsd"), 0);
                                                    if (stsd != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Sample description atom found");
#endif
                                                        //log.Debug("Sample description (video) stsd children: {}", stsd.getChildren());
                                                        Mp4Atom avc1 = stsd.Lookup(Mp4Atom.TypeToInt("avc1"), 0);
                                                        if (avc1 != null)
                                                        {
                                                            //log.debug("AVC1 children: {}", avc1.getChildren());
                                                            //set the video codec here - may be avc1 or mp4v
                                                            SetVideoCodecId(Mp4Atom.IntToType(avc1.Type));
                                                            //video decoder config
                                                            //TODO may need to be generic later
                                                            Mp4Atom codecChild = avc1.Lookup(Mp4Atom.TypeToInt("avcC"), 0);
                                                            if (codecChild != null)
                                                            {
                                                                _avcLevel = codecChild.AvcLevel;
#if !SILVERLIGHT
                                                                log.Debug(string.Format("AVC level: {0}", _avcLevel));
#endif
                                                                _avcProfile = codecChild.AvcProfile;
#if !SILVERLIGHT
                                                                log.Debug(string.Format("AVC Profile: {0}", _avcProfile));
                                                                log.Debug(string.Format("AVCC size: {0}", codecChild.Size));
#endif
                                                                _videoDecoderBytes = codecChild.VideoConfigBytes;
                                                                //log.Debug(string.Format("Video config bytes: {0}", ToStringBuilder.reflectionToString(videoDecoderBytes)));
                                                            }
                                                            else
                                                            {
                                                                //quicktime and ipods use a pixel aspect atom
                                                                //since we have no avcC check for this and avcC may
                                                                //be a child
                                                                Mp4Atom pasp = avc1.Lookup(Mp4Atom.TypeToInt("pasp"), 0);
                                                                if (pasp != null)
                                                                {
                                                                    //log.debug("PASP children: {}", pasp.getChildren());
                                                                    codecChild = pasp.Lookup(Mp4Atom.TypeToInt("avcC"), 0);
                                                                    if (codecChild != null)
                                                                    {
                                                                        _avcLevel = codecChild.AvcLevel;
#if !SILVERLIGHT
                                                                        log.Debug(string.Format("AVC level: {0}", _avcLevel));
#endif
                                                                        _avcProfile = codecChild.AvcProfile;
#if !SILVERLIGHT
                                                                        log.Debug(string.Format("AVC Profile: {0}", _avcProfile));
                                                                        log.Debug(string.Format("AVCC size: {0}", codecChild.Size));
#endif
                                                                        _videoDecoderBytes = codecChild.VideoConfigBytes;
                                                                        //log.debug("Video config bytes: {}", ToStringBuilder.reflectionToString(videoDecoderBytes));
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //look for mp4v
                                                            Mp4Atom mp4v = stsd.Lookup(Mp4Atom.TypeToInt("mp4v"), 0);
                                                            if (mp4v != null)
                                                            {
                                                                //log.debug("MP4V children: {}", mp4v.getChildren());
                                                                //set the video codec here - may be avc1 or mp4v
                                                                SetVideoCodecId(Mp4Atom.IntToType(mp4v.Type));
                                                                //look for esds 
                                                                Mp4Atom codecChild = mp4v.Lookup(Mp4Atom.TypeToInt("esds"), 0);
                                                                if (codecChild != null)
                                                                {
                                                                    //look for descriptors
                                                                    Mp4Descriptor descriptor = codecChild.EsdDescriptor;
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
                                                                                        _videoDecoderBytes = new byte[descr2.DSID.Length - 8];
                                                                                        Array.Copy(descr2.DSID, 8, _videoDecoderBytes, 0, _videoDecoderBytes.Length);
                                                                                        //log.debug("Video config bytes: {}", ToStringBuilder.reflectionToString(videoDecoderBytes));
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

                                                        }
                                                        //log.debug("{}", ToStringBuilder.reflectionToString(avc1));
                                                    }
                                                    //stsc - has Records
                                                    Mp4Atom stsc = stbl.Lookup(Mp4Atom.TypeToInt("stsc"), 0);
                                                    if (stsc != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Sample to chunk atom found");
#endif
                                                        _videoSamplesToChunks = stsc.Records;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record count: {0}", _videoSamplesToChunks.Count));
#endif
                                                        Mp4Atom.Record rec = _videoSamplesToChunks[0];
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record data: Description index={0} Samples per chunk={1}", rec.SampleDescriptionIndex, rec.SamplesPerChunk));
#endif
                                                    }
                                                    //stsz - has Samples
                                                    Mp4Atom stsz = stbl.Lookup(Mp4Atom.TypeToInt("stsz"), 0);
                                                    if (stsz != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Sample size atom found");
#endif
                                                        //vector full of integers							
                                                        _videoSamples = stsz.Samples;
                                                        //if sample size is 0 then the table must be checked due
                                                        //to variable sample sizes
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Sample size: {0}", stsz.SampleSize));
#endif
                                                        _videoSampleCount = _videoSamples.Count;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Sample count: {0}", _videoSampleCount));
#endif
                                                    }
                                                    //stco - has Chunks
                                                    Mp4Atom stco = stbl.Lookup(Mp4Atom.TypeToInt("stco"), 0);
                                                    if (stco != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Chunk offset atom found");
#endif
                                                        //vector full of integers
                                                        _videoChunkOffsets = stco.Chunks;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Chunk count: {0}", _videoChunkOffsets.Count));
#endif
                                                    }
                                                    //stss - has Sync - no sync means all samples are keyframes
                                                    Mp4Atom stss = stbl.Lookup(Mp4Atom.TypeToInt("stss"), 0);
                                                    if (stss != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Sync sample atom found");
#endif
                                                        //vector full of integers
                                                        _syncSamples = stss.SyncSamples;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Keyframes: {0}", _syncSamples.Count));
#endif
                                                    }
                                                    //stts - has TimeSampleRecords
                                                    Mp4Atom stts = stbl.Lookup(Mp4Atom.TypeToInt("stts"), 0);
                                                    if (stts != null)
                                                    {
#if !SILVERLIGHT
                                                        log.Debug("Time to sample atom found");
#endif
                                                        List<Mp4Atom.TimeSampleRecord> records = stts.TimeToSamplesRecords;
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record count: {0}", records.Count));
#endif
                                                        Mp4Atom.TimeSampleRecord rec = records[0];
#if !SILVERLIGHT
                                                        log.Debug(string.Format("Record data: Consecutive samples={0} Duration={1}", rec.ConsecutiveSamples, rec.SampleDuration));
#endif
                                                        //if we have 1 record then all samples have the same duration
                                                        if (records.Count > 1)
                                                        {
                                                            //TODO: handle video samples with varying durations
#if !SILVERLIGHT
                                                            log.Debug("Video samples have differing durations, video playback may fail");
#endif
                                                        }
                                                        _videoSampleDuration = rec.SampleDuration;
                                                    }
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            //calculate FPS
                            _fps = (_videoSampleCount * _timeScale) / (double)_duration;
#if !SILVERLIGHT
                            log.Debug(string.Format("FPS calc: ({0} * {1}) / {2}", _videoSampleCount, _timeScale, _duration));
                            log.Debug(string.Format("FPS: {0}", _fps));
#endif
                            //real duration
                            StringBuilder sb = new StringBuilder();
                            double videoTime = ((double)_duration / (double)_timeScale);
#if !SILVERLIGHT
                            log.Debug(string.Format("Video time: {0}", videoTime));
#endif
                            int minutes = (int)(videoTime / 60);
                            if (minutes > 0)
                            {
                                sb.Append(minutes);
                                sb.Append('.');
                            }
                            //formatter for seconds / millis
                            //NumberFormat df = DecimalFormat.getInstance();
                            //df.setMaximumFractionDigits(2);
                            //sb.append(df.format((videoTime % 60)));
                            sb.Append(videoTime % 60);
                            _formattedDuration = sb.ToString();
#if !SILVERLIGHT
                            log.Debug(string.Format("Time: {0}", _formattedDuration));
#endif
                            break;
                        case 1835295092: //mdat
                            topAtoms++;
                            long dataSize = 0L;
                            Mp4Atom mdat = atom;
                            dataSize = mdat.Size;
                            //log.debug("{}", ToStringBuilder.reflectionToString(mdat));
                            _mdatOffset = _inputStream.Offset - dataSize;
                            //log.Debug(string.Format("File size: {0} mdat size: {1}", _file.Length, dataSize));
#if !SILVERLIGHT
                            log.Debug(string.Format("mdat size: {0}", dataSize));
#endif
                            break;
                        case 1718773093: //free
                        case 2003395685: //wide
                            break;
                        default:
#if !SILVERLIGHT
                            log.Warn(string.Format("Unexpected atom: {}", Mp4Atom.IntToType(atom.Type)));
#endif
                            break;
                    }
                }

                //add the tag name (size) to the offsets
                _moovOffset += 8;
                _mdatOffset += 8;
#if !SILVERLIGHT
                log.Debug(string.Format("Offsets moov: {0} mdat: {1}", _moovOffset, _mdatOffset));
#endif

            }
            catch(Exception ex)
            {
#if !SILVERLIGHT
                log.Error("Exception decoding header / atoms", ex);
#endif
            }		            
        }

        public long Position
        {
            get
            {
                return GetCurrentPosition();
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

        /// <summary>
        /// Packages media data for return to providers.
        /// </summary>
        /// <returns></returns>
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
#if !SILVERLIGHT
                log.Debug(string.Format("Playback #{0} {1}", _currentFrame, frame));
#endif
                int sampleSize = frame.Size;

                int time = (int)Math.Round(frame.Time * 1000.0);
                //log.debug("Read tag - dst: {} base: {} time: {}", new Object[]{frameTs, baseTs, time});

                long samplePos = frame.Offset;
                //log.debug("Read tag - samplePos {}", samplePos);

                //determine frame type and packet body padding
                byte type = frame.Type;
                //assume video type
                int pad = 5;
                if (type == IOConstants.TYPE_AUDIO)
                {
                    pad = 2;
                }

                //create a byte buffer of the size of the sample
                byte[] data = new byte[sampleSize + pad];
                try
                {
                    //prefix is different for keyframes
                    if (type == IOConstants.TYPE_VIDEO)
                    {
                        if (frame.IsKeyFrame)
                        {
                            //log.debug("Writing keyframe prefix");
                            Array.Copy(PREFIX_VIDEO_KEYFRAME, data, PREFIX_VIDEO_KEYFRAME.Length);
                        }
                        else
                        {
                            //log.debug("Writing interframe prefix");
                            Array.Copy(PREFIX_VIDEO_FRAME, data, PREFIX_VIDEO_FRAME.Length);
                        }
                        _videoCount++;
                    }
                    else
                    {
                        //log.debug("Writing audio prefix");
                        Array.Copy(PREFIX_AUDIO_FRAME, data, PREFIX_AUDIO_FRAME.Length);
                        _audioCount++;
                    }
                    //do we need to add the mdat offset to the sample position?
                    _stream.Position = samplePos;
                    _stream.Read(data, pad, sampleSize);
                }
                catch (Exception ex)
                {
#if !SILVERLIGHT
                    log.Error("Error on channel position / read", ex);
#endif
                }

                //create the tag
                tag = new Tag(type, time, data.Length, data, _prevFrameSize);
                //log.debug("Read tag - type: {} body size: {}", (type == TYPE_AUDIO ? "Audio" : "Video"), tag.getBodySize());

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
            //_fs.Close();
            _inputStream.Close();
        }

        public bool HasVideo()
        {
            try
            {
                return _hasVideo;
            }
            finally
            {
                if (_frames != null)
                {
                    _frames.Clear();
                    _frames = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Performs frame analysis and generates metadata for use in seeking. All the frames are analyzed and sorted together based on time and offset.
        /// </summary>
        public void AnalyzeFrames()
        {
#if !SILVERLIGHT
            log.Debug("Analyzing frames");
#endif
            // Maps positions, samples, timestamps to one another
            _timePosMap = new Dictionary<int, long>();
            _samplePosMap = new Dictionary<int, long>();
            // tag == sample
            int sample = 1;
            long pos;
            for (int i = 0; i < _videoSamplesToChunks.Count; i++)
            {
                Mp4Atom.Record record = _videoSamplesToChunks[i];
                int firstChunk = record.FirstChunk;
                int lastChunk = _videoChunkOffsets.Count;
                if (i < _videoSamplesToChunks.Count - 1)
                {
                    Mp4Atom.Record nextRecord = _videoSamplesToChunks[i + 1];
                    lastChunk = nextRecord.FirstChunk - 1;
                }
                for (int chunk = firstChunk; chunk <= lastChunk; chunk++)
                {
                    int sampleCount = record.SamplesPerChunk;
                    pos = _videoChunkOffsets[chunk - 1];
                    while (sampleCount > 0)
                    {
                        //log.debug("Position: {}", pos);
                        _samplePosMap.Add(sample, pos);
                        //calculate ts
                        double ts = (_videoSampleDuration * (sample - 1)) / _videoTimeScale;
                        //check to see if the sample is a keyframe
                        bool keyframe = false;
                        //some files appear not to have sync samples
                        if (_syncSamples != null)
                        {
                            keyframe = _syncSamples.Contains(sample);
                            if (_seekPoints == null)
                            {
                                _seekPoints = new LinkedList<int>();
                            }
                            int keyframeTs = (int)Math.Round(ts * 1000.0);
                            _seekPoints.AddLast(keyframeTs);
                            _timePosMap.Add(keyframeTs, pos);
                        }
                        //size of the sample
                        int size = _videoSamples[sample - 1];
                        //create a frame
                        Mp4Frame frame = new Mp4Frame();
                        frame.IsKeyFrame = keyframe;
                        frame.Offset = pos;
                        frame.Size = size;
                        frame.Time = ts;
                        frame.Type = IOConstants.TYPE_VIDEO;
                        _frames.Add(frame);

                        //log.debug("Sample #{} {}", sample, frame);

                        //inc and dec stuff
                        pos += size;
                        sampleCount--;
                        sample++;
                    }
                }
            }

            //log.debug("Sample position map (video): {}", samplePosMap);

            //add the audio frames / samples / chunks		
            sample = 1;
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
#if !SILVERLIGHT
            log.Debug(string.Format("Frames count: {0}", _frames.Count));
            //log.debug("Frames: {}", frames);
#endif

            //release some memory, since we're done with the vectors
            _audioChunkOffsets.Clear();
            _audioChunkOffsets = null;
            _audioSamplesToChunks.Clear();
            _audioSamplesToChunks = null;

            _videoChunkOffsets.Clear();
            _videoChunkOffsets = null;
            _videoSamplesToChunks.Clear();
            _videoSamplesToChunks = null;

            _syncSamples.Clear();
            _syncSamples = null;            
        }

        /// <summary>
        /// Create tag for metadata event.
        /// 
        /// Info from http://www.kaourantin.net/2007/08/what-just-happened-to-video-on-web_20.html
        /// <para>
        /// duration - Obvious. But unlike for FLV files this field will always be present.
        /// videocodecid - For H.264 we report 'avc1'.
        /// audiocodecid - For AAC we report 'mp4a', for MP3 we report '.mp3'.
        /// avcprofile - 66, 77, 88, 100, 110, 122 or 144 which corresponds to the H.264 profiles.
        /// avclevel - A number between 10 and 51. Consult this list to find out more.
        /// aottype - Either 0, 1 or 2. This corresponds to AAC Main, AAC LC and SBR audio types.
        /// moovposition - The offset in bytes of the moov atom in a file.
        /// trackinfo - An array of objects containing various infomation about all the tracks in a file
        ///   ex.
        ///     trackinfo[0].length: 7081
        ///     trackinfo[0].timescale: 600
        ///     trackinfo[0].sampledescription.sampletype: avc1
        ///     trackinfo[0].language: und
        ///     trackinfo[1].length: 525312
        ///     trackinfo[1].timescale: 44100
        ///     trackinfo[1].sampledescription.sampletype: mp4a
        ///     trackinfo[1].language: und
        /// 
        /// chapters - As mentioned above information about chapters in audiobooks.
        /// seekpoints - As mentioned above times you can directly feed into NetStream.seek();
        /// videoframerate - The frame rate of the video if a monotone frame rate is used. Most videos will have a monotone frame rate.
        /// audiosamplerate - The original sampling rate of the audio track.
        /// audiochannels - The original number of channels of the audio track.
        /// tags - As mentioned above ID3 like tag information.
        /// </para>
        /// 
        /// <para>
        /// width: Display width in pixels.
        /// height: Display height in pixels.
        /// duration: Duration in seconds.
        /// avcprofile: AVC profile number such as 55, 77, 100 etc.
        /// avclevel: AVC IDC level number such as 10, 11, 20, 21 etc.
        /// aacaot: AAC audio object type; 0, 1 or 2 are supported.
        /// videoframerate: Frame rate of the video in this MP4.
        /// seekpoints: Array that lists the available keyframes in a file as time stamps in milliseconds. 
        ///     This is optional as the MP4 file might not contain this information. Generally speaking, 
        ///     most MP4 files will include this by default.
        /// videocodecid: Usually a string such as "avc1" or "VP6F."
        /// audiocodecid: Usually a string such as ".mp3" or "mp4a."
        /// progressivedownloadinfo: Object that provides information from the "pdin" atom. This is optional 
        ///     and many files will not have this field.
        /// trackinfo: Object that provides information on all the tracks in the MP4 file, including their sample description ID.
        /// tags: Array of key value pairs representing the information present in the "ilst" atom, which is 
        ///     the equivalent of ID3 tags for MP4 files. These tags are mostly used by iTunes. 
        /// </para>
        /// </summary>
        /// <returns>Metadata event tag.</returns>
        ITag CreateFileMeta()
        {
#if !SILVERLIGHT
            log.Debug("Creating onMetaData");
#endif
            // Create tag for onMetaData event
            ByteBuffer buf = ByteBuffer.Allocate(1024);
            buf.AutoExpand = true;
            AMFWriter output = new AMFWriter(buf);
            output.WriteString("onMetaData");

            Dictionary<string, object> props = new Dictionary<string, object>();
            // Duration property
            props.Add("duration", ((double)_duration / (double)_timeScale));
            props.Add("width", _width);
            props.Add("height", _height);

            // Video codec id
            props.Add("videocodecid", _videoCodecId);
            props.Add("avcprofile", _avcProfile);
            props.Add("avclevel", _avcLevel);
            props.Add("videoframerate", _fps);
            // Audio codec id - watch for mp3 instead of aac
            props.Add("audiocodecid", _audioCodecId);
            props.Add("aacaot", _audioCodecType);
            props.Add("audiosamplerate", _audioTimeScale);
            props.Add("audiochannels", _audioChannels);

            props.Add("moovposition", _moovOffset);
            //props.put("chapters", ""); //this is for f4b - books
            if (_seekPoints != null)
            {
                props.Add("seekpoints", _seekPoints);
            }
            //tags will only appear if there is an "ilst" atom in the file
            //props.put("tags", "");

            List<Dictionary<String, Object>> arr = new List<Dictionary<String, Object>>(2);
            if (_hasAudio)
            {
                Dictionary<String, Object> audioMap = new Dictionary<String, Object>(4);
                audioMap.Add("timescale", _audioTimeScale);
                audioMap.Add("language", "und");

                List<Dictionary<String, String>> desc = new List<Dictionary<String, String>>(1);
                audioMap.Add("sampledescription", desc);

                Dictionary<String, String> sampleMap = new Dictionary<String, String>(1);
                sampleMap.Add("sampletype", _audioCodecId);
                desc.Add(sampleMap);

                if (_audioSamples != null)
                {
                    audioMap.Add("length_property", _audioSampleDuration * _audioSamples.Count);
                    //release some memory, since we're done with the vectors
                    _audioSamples.Clear();
                    _audioSamples = null;
                }
                arr.Add(audioMap);
            }
            if (_hasVideo)
            {
                Dictionary<String, Object> videoMap = new Dictionary<String, Object>(3);
                videoMap.Add("timescale", _videoTimeScale);
                videoMap.Add("language", "und");

                List<Dictionary<String, String>> desc = new List<Dictionary<String, String>>(1);
                videoMap.Add("sampledescription", desc);

                Dictionary<String, String> sampleMap = new Dictionary<String, String>(1);
                sampleMap.Add("sampletype", _videoCodecId);
                desc.Add(sampleMap);

                if (_videoSamples != null)
                {
                    videoMap.Add("length_property", _videoSampleDuration * _videoSamples.Count);
                    //release some memory, since we're done with the vectors
                    _videoSamples.Clear();
                    _videoSamples = null;
                }
                arr.Add(videoMap);
            }
            props.Add("trackinfo", arr.ToArray());
            //set this based on existence of seekpoints
            props.Add("canSeekToEnd", (_seekPoints != null));

            output.WriteAssociativeArray(ObjectEncoding.AMF0, props);
            buf.Flip();

            //now that all the meta properties are done, update the duration
            _duration = (long)Math.Round(_duration * 1000d);

            ITag result = new Tag(IOConstants.TYPE_METADATA, 0, buf.Limit, buf.ToArray(), 0);
            return result;
        }

        /// <summary>
        /// Tag sequence
        /// MetaData, Video config, Audio config, remaining audio and video 
        /// 
        /// Packet prefixes:
        /// 17 00 00 00 00 = Video extra data (first video packet)
        /// 17 01 00 00 00 = Video keyframe
        /// 27 01 00 00 00 = Video interframe
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
#if !SILVERLIGHT
            log.Debug("Creating pre-streaming tags");
#endif
            ITag tag = null;
            //byte[] body = null;
            ByteBuffer body;

            if (_hasVideo)
            {
                //video tag #1
                body = ByteBuffer.Allocate(41);
                body.AutoExpand = true;
                body.Put(PREFIX_VIDEO_CONFIG_FRAME);
                if (_videoDecoderBytes != null)
                {
                    body.Put(_videoDecoderBytes);
                }
                tag = new Tag(IOConstants.TYPE_VIDEO, 0, (int)body.Length, body.ToArray(), 0);
                //add tag
                _firstTags.AddLast(tag);
            }

            if (_hasAudio)
            {
                //audio tag #1
                body = ByteBuffer.Allocate(7);
                body.AutoExpand = true;
                body.Put(new byte[] { (byte)0xaf, (byte)0 }); //prefix
                if (_audioDecoderBytes != null)
                {
                    body.Put(_audioDecoderBytes);
                }
                else
                {
                    //default to aac-lc when the esds doesn't contain descriptor bytes
                    //Array.Copy(AUDIO_CONFIG_FRAME_AAC_LC, 0, body, PREFIX_AUDIO_FRAME.Length, AUDIO_CONFIG_FRAME_AAC_LC.Length);
                    //body[PREFIX_AUDIO_FRAME.Length + AUDIO_CONFIG_FRAME_AAC_LC.Length] = 0x06; //suffix
                    body.Put(AUDIO_CONFIG_FRAME_AAC_LC);
                }
                body.Put((byte)0x06); //suffix
                tag = new Tag(IOConstants.TYPE_AUDIO, 0, (int)body.Length, body.ToArray(), tag.BodySize);
                //add tag
                _firstTags.AddLast(tag);
            }            
        }

        public void SetVideoCodecId(String videoCodecId)
        {
            this._videoCodecId = videoCodecId;
        }

        public void SetAudioCodecId(String audioCodecId)
        {
            this._audioCodecId = audioCodecId;
        }
    }
}
