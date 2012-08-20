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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

/**
	This software module was originally developed by Apple Computer, Inc. in the 
	course of development of MPEG-4. This software module is an implementation of 
	a part of one or more MPEG-4 tools as specified by MPEG-4. ISO/IEC gives users 
	of MPEG-4 free license to this software module or modifications thereof for 
	use in hardware or software products claiming conformance to MPEG-4. Those 
	intending to use this software module in hardware or software products are 
	advised that its use may infringe existing patents. The original developer of 
	this software module and his/her company, the subsequent editors and their 
	companies, and ISO/IEC have no liability for use of this software module or 
	modifications thereof in an implementation. Copyright is not released for non 
	MPEG-4 conforming products. Apple Computer, Inc. retains full right to use the 
	code for its own purpose, assign or donate the code to a third party and to
	inhibit third parties from using the code for non MPEG-4 conforming products.
	This copyright notice must be included in all copies or	derivative works. 
	Copyright (c) 1999.
*/

namespace FluorineFx.IO.Mp4
{
    /// <summary>
    /// The Mp4Atom object represents the smallest information block of the MP4 file. It could contain other atoms as children.
    /// </summary>
    class Mp4Atom
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(Mp4Atom));
#endif
        
        public readonly static int MP4AudioSampleEntryAtomType = Mp4Atom.TypeToInt("mp4a");
        
        public readonly static int MP4ChunkLargeOffsetAtomType = Mp4Atom.TypeToInt("co64");
        
        public readonly static int MP4ChunkOffsetAtomType = Mp4Atom.TypeToInt("stco");
        
        public readonly static int MP4DataInformationAtomType = Mp4Atom.TypeToInt("dinf");
        
        public readonly static int MP4ESDAtomType = Mp4Atom.TypeToInt("esds");
        
        public readonly static int MP4ExtendedAtomType = Mp4Atom.TypeToInt("uuid");
        
        public readonly static int MP4HandlerAtomType = Mp4Atom.TypeToInt("hdlr");
        
        public readonly static int MP4MediaAtomType = Mp4Atom.TypeToInt("mdia");
        
        public readonly static int MP4MediaHeaderAtomType = Mp4Atom.TypeToInt("mdhd");
        
        public readonly static int MP4MediaInformationAtomType = Mp4Atom.TypeToInt("minf");
        
        public readonly static int MP4MovieAtomType = Mp4Atom.TypeToInt("moov");
        
        public readonly static int MP4MovieHeaderAtomType = Mp4Atom.TypeToInt("mvhd");
        
        public readonly static int MP4SampleDescriptionAtomType = Mp4Atom.TypeToInt("stsd");
        
        public readonly static int MP4SampleSizeAtomType = Mp4Atom.TypeToInt("stsz");
        
        public readonly static int MP4CompactSampleSizeAtomType = Mp4Atom.TypeToInt("stz2");
        
        public readonly static int MP4SampleTableAtomType = Mp4Atom.TypeToInt("stbl");
        
        public readonly static int MP4SampleToChunkAtomType = Mp4Atom.TypeToInt("stsc");
        
        public readonly static int MP4SoundMediaHeaderAtomType = Mp4Atom.TypeToInt("smhd");
        
        public readonly static int MP4TrackAtomType = Mp4Atom.TypeToInt("trak");
        
        public readonly static int MP4TrackHeaderAtomType = Mp4Atom.TypeToInt("tkhd");
        
        public readonly static int MP4VideoMediaHeaderAtomType = Mp4Atom.TypeToInt("vmhd");
        
        public readonly static int MP4VisualSampleEntryAtomType = Mp4Atom.TypeToInt("mp4v");
        // the type of the avc1 / H.263 
        public readonly static int MP4VideoSampleEntryAtomType = Mp4Atom.TypeToInt("avc1");
        // contains key frames
        public readonly static int MP4SyncSampleAtomType = Mp4Atom.TypeToInt("stss");
        public readonly static int MP4TimeToSampleAtomType = Mp4Atom.TypeToInt("stts");
        // contains avc properties
        public readonly static int MP4AVCAtomType = Mp4Atom.TypeToInt("avcC");
        // movie data, this ones is not actually parsed
        public readonly static int MP4MovieDataType = Mp4Atom.TypeToInt("mdat");
        // pixel aspect ratio
        public readonly static int MP4PixelAspectAtomType = Mp4Atom.TypeToInt("pasp");

        /** The size of the atom. */
        protected long _size;
        /** The type of the atom. */
        protected int _type;
        /** The user's extended type of the atom. */
        protected String _uuid;
        /** The amount of bytes that bytesRead from the mpeg stream. */
        protected long _bytesRead;
        /** The children of this atom. */
        protected List<Mp4Atom> _children = new List<Mp4Atom>(3);

        public Mp4Atom(long size, int type, String uuid, long bytesRead)
        {
            _size = size;
            _type = type;
            _uuid = uuid;
            _bytesRead = bytesRead;
        }
        /// <summary>
        /// Constructs an Atom object from the data in the bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The constructed atom.</returns>
        public static Mp4Atom CreateAtom(Mp4DataStream bitstream)
        {
            String uuid = null;
            long size = bitstream.ReadBytes(4);
            if (size == 0)
            {
                throw new IOException("Invalid size");
            }
            int type = (int)bitstream.ReadBytes(4);
            long bytesRead = 8;
            if (type == MP4ExtendedAtomType)
            {
                uuid = bitstream.ReadString(16);
                bytesRead += 16;
            }
            // large size
            if (size == 1)
            {
                size = bitstream.ReadBytes(8);
                bytesRead += 8;
            }
            Mp4Atom atom = new Mp4Atom(size, type, uuid, bytesRead);
            if ((type == MP4MediaAtomType) || (type == MP4DataInformationAtomType) || (type == MP4MovieAtomType)
                || (type == MP4MediaInformationAtomType) || (type == MP4SampleTableAtomType) || (type == MP4TrackAtomType))
            {
                bytesRead = atom.create_composite_atom(bitstream);
            }
            else if (type == MP4AudioSampleEntryAtomType)
            {
                bytesRead = atom.create_audio_sample_entry_atom(bitstream);
            }
            else if (type == MP4ChunkLargeOffsetAtomType)
            {
                bytesRead = atom.create_chunk_large_offset_atom(bitstream);
            }
            else if (type == MP4ChunkOffsetAtomType)
            {
                bytesRead = atom.create_chunk_offset_atom(bitstream);
            }
            else if (type == MP4HandlerAtomType)
            {
                bytesRead = atom.create_handler_atom(bitstream);
            }
            else if (type == MP4MediaHeaderAtomType)
            {
                bytesRead = atom.create_media_header_atom(bitstream);
            }
            else if (type == MP4MovieHeaderAtomType)
            {
                bytesRead = atom.create_movie_header_atom(bitstream);
            }
            else if (type == MP4SampleDescriptionAtomType)
            {
                bytesRead = atom.create_sample_description_atom(bitstream);
            }
            else if (type == MP4SampleSizeAtomType)
            {
                bytesRead = atom.create_sample_size_atom(bitstream);
            }
            else if (type == MP4CompactSampleSizeAtomType)
            {
                bytesRead = atom.create_compact_sample_size_atom(bitstream);
            }
            else if (type == MP4SampleToChunkAtomType)
            {
                bytesRead = atom.create_sample_to_chunk_atom(bitstream);
            }
            else if (type == MP4SyncSampleAtomType)
            {
                bytesRead = atom.create_sync_sample_atom(bitstream);
            }
            else if (type == MP4TimeToSampleAtomType)
            {
                bytesRead = atom.create_time_to_sample_atom(bitstream);
            }
            else if (type == MP4SoundMediaHeaderAtomType)
            {
                bytesRead = atom.create_sound_media_header_atom(bitstream);
            }
            else if (type == MP4TrackHeaderAtomType)
            {
                bytesRead = atom.create_track_header_atom(bitstream);
            }
            else if (type == MP4VideoMediaHeaderAtomType)
            {
                bytesRead = atom.create_video_media_header_atom(bitstream);
            }
            else if (type == MP4VisualSampleEntryAtomType)
            {
                bytesRead = atom.create_visual_sample_entry_atom(bitstream);
            }
            else if (type == MP4VideoSampleEntryAtomType)
            {
                bytesRead = atom.create_video_sample_entry_atom(bitstream);
            }
            else if (type == MP4ESDAtomType)
            {
                bytesRead = atom.create_esd_atom(bitstream);
            }
            else if (type == MP4AVCAtomType)
            {
                bytesRead = atom.create_avc_config_atom(bitstream);
            }
            else if (type == MP4PixelAspectAtomType)
            {
                bytesRead = atom.create_pasp_atom(bitstream);
            }
#if !SILVERLIGHT
            log.Debug(string.Format("Atom: type = {0} size = {1}", IntToType(type), size));
#endif
            bitstream.SkipBytes(size - bytesRead);
            return atom;
        }

        protected int version = 0;
        protected int flags = 0;

        /// <summary>
        /// Loads the version of the full atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_full_atom(Mp4DataStream bitstream)
        {
            long value = bitstream.ReadBytes(4);
            version = (int)value >> 24;
            flags = (int)value & 0xffffff;
            _bytesRead += 4;
            return _bytesRead;
        }
        /// <summary>
        /// Loads the composite atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_composite_atom(Mp4DataStream bitstream)
        {
            while (_bytesRead < _size)
            {
                Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
                _children.Add(child);
                _bytesRead += child.Size;
            }
            return _bytesRead;
        }
        /// <summary>
        /// Looks up a child atom with the specified type, skips the <code>number</code> children with the same type before finding a result.
        /// </summary>
        /// <param name="type">The type of the atom.</param>
        /// <param name="number">The number of atoms to skip.</param>
        /// <returns>The atom if found othwerwise null.</returns>
        public Mp4Atom Lookup(long type, int number)
        {
            int position = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                Mp4Atom atom = _children[i];
                if (atom.Type == type)
                {
                    if (position >= number)
                    {
                        return atom;
                    }
                    position++;
                }
            }
            return null;
        }

        private int channelCount = 0;

        public int ChannelCount
        {
            get { return channelCount; }
        }
        /// <summary>
        /// Loads AudioSampleEntry atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_audio_sample_entry_atom(Mp4DataStream bitstream)
        {
            //qtff page 117
#if !SILVERLIGHT
            log.Debug("Audio sample entry");
#endif
            bitstream.SkipBytes(6);
            int dataReferenceIndex = (int)bitstream.ReadBytes(2);
            bitstream.SkipBytes(8);
            channelCount = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Channels: {0}", channelCount));
#endif
            sampleSize = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Sample size (bits): {0}", sampleSize));
#endif
            bitstream.SkipBytes(4);
            timeScale = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Time scale: {0}", timeScale));
#endif
            bitstream.SkipBytes(2);
            _bytesRead += 28;
            Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
            _children.Add(child);
            _bytesRead += child.Size;
            return _bytesRead;
        }

        protected int entryCount;

        /// <summary>
        /// The decoding time to sample table.
        /// </summary>
        protected List<long> chunks = new List<long>();

        public List<long> Chunks
        {
            get { return chunks; }
        }

        /// <summary>
        /// Loads ChunkLargeOffset atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_chunk_large_offset_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
            _bytesRead += 8;
            for (int i = 0; i < entryCount; i++)
            {
                long chunkOffset = bitstream.ReadBytes(8);
                chunks.Add(chunkOffset);
                _bytesRead += 8;
            }
            return _bytesRead;
        }
        /// <summary>
        /// Loads ChunkOffset atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_chunk_offset_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
            _bytesRead += 4;
            for (int i = 0; i < entryCount; i++)
            {
                long chunkOffset = bitstream.ReadBytes(4);
                chunks.Add(chunkOffset);
                _bytesRead += 4;
            }
            return _bytesRead;
        }

        protected int handlerType;

        public int HandlerType
        {
            get { return handlerType; }
        }

        /// <summary>
        /// Loads Handler atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_handler_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            int qt_componentType = (int)bitstream.ReadBytes(4);
            handlerType = (int)bitstream.ReadBytes(4);
            int qt_componentManufacturer = (int)bitstream.ReadBytes(4);
            int qt_componentFlags = (int)bitstream.ReadBytes(4);
            int qt_componentFlagsMask = (int)bitstream.ReadBytes(4);
            _bytesRead += 20;
            int length = (int)(_size - _bytesRead - 1);
            String trackName = bitstream.ReadString(length);
#if !SILVERLIGHT
            log.Debug(string.Format("Track name: {0}", trackName));
#endif
            _bytesRead += length;
            return _bytesRead;
        }

        protected DateTime creationTime;
        protected DateTime modificationTime;
        protected int timeScale;
        protected long duration;

        /// <summary>
        /// Loads MediaHeader atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_media_header_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            if (version == 1)
            {
                creationTime = createDate(bitstream.ReadBytes(8));
                modificationTime = createDate(bitstream.ReadBytes(8));
                timeScale = (int)bitstream.ReadBytes(4);
                duration = bitstream.ReadBytes(8);
                _bytesRead += 28;
            }
            else
            {
                creationTime = createDate(bitstream.ReadBytes(4));
                modificationTime = createDate(bitstream.ReadBytes(4));
                timeScale = (int)bitstream.ReadBytes(4);
                duration = bitstream.ReadBytes(4);
                _bytesRead += 16;
            }
            int packedLanguage = (int)bitstream.ReadBytes(2);
            int qt_quality = (int)bitstream.ReadBytes(2);
            _bytesRead += 4;
            return _bytesRead;
        }

        public long Duration
        {
            get { return duration; }
        }

        public int TimeScale
        {
            get { return timeScale; }
        }

        /// <summary>
        /// Loads MovieHeader atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_movie_header_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            if (version == 1)
            {
                creationTime = createDate(bitstream.ReadBytes(8));
                modificationTime = createDate(bitstream.ReadBytes(8));
                timeScale = (int)bitstream.ReadBytes(4);
                duration = bitstream.ReadBytes(8);
                _bytesRead += 28;
            }
            else
            {
                creationTime = createDate(bitstream.ReadBytes(4));
                modificationTime = createDate(bitstream.ReadBytes(4));
                timeScale = (int)bitstream.ReadBytes(4);
                duration = bitstream.ReadBytes(4);
                _bytesRead += 16;
            }
            int qt_preferredRate = (int)bitstream.ReadBytes(4);
            int qt_preferredVolume = (int)bitstream.ReadBytes(2);
            bitstream.SkipBytes(10);
            long qt_matrixA = bitstream.ReadBytes(4);
            long qt_matrixB = bitstream.ReadBytes(4);
            long qt_matrixU = bitstream.ReadBytes(4);
            long qt_matrixC = bitstream.ReadBytes(4);
            long qt_matrixD = bitstream.ReadBytes(4);
            long qt_matrixV = bitstream.ReadBytes(4);
            long qt_matrixX = bitstream.ReadBytes(4);
            long qt_matrixY = bitstream.ReadBytes(4);
            long qt_matrixW = bitstream.ReadBytes(4);
            long qt_previewTime = bitstream.ReadBytes(4);
            long qt_previewDuration = bitstream.ReadBytes(4);
            long qt_posterTime = bitstream.ReadBytes(4);
            long qt_selectionTime = bitstream.ReadBytes(4);
            long qt_selectionDuration = bitstream.ReadBytes(4);
            long qt_currentTime = bitstream.ReadBytes(4);
            long nextTrackID = bitstream.ReadBytes(4);
            _bytesRead += 80;
            return _bytesRead;
        }
        /// <summary>
        /// Loads SampleDescription atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_sample_description_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
#if !SILVERLIGHT
            log.Debug(string.Format("stsd entry count: {0}", entryCount));
#endif
            _bytesRead += 4;
            for (int i = 0; i < entryCount; i++)
            {
                Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
                _children.Add(child);
                _bytesRead += child.Size;
            }
            return _bytesRead;
        }

        protected int sampleSize;
        protected int sampleCount;

        /// <summary>
        /// The decoding time to sample table
        /// </summary>
        protected List<int> samples = new List<int>();

        public List<int> Samples
        {
            get { return samples; }
        }

        public int SampleSize
        {
            get { return sampleSize; }
        }

        /// <summary>
        /// Loads MP4SampleSizeAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_sample_size_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            sampleSize = (int)bitstream.ReadBytes(4);
            sampleCount = (int)bitstream.ReadBytes(4);
            _bytesRead += 8;
            if (sampleSize == 0)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    int size = (int)bitstream.ReadBytes(4);
                    samples.Add(size);
                    _bytesRead += 4;
                }
            }
            return _bytesRead;
        }

        protected int fieldSize;

        /// <summary>
        /// Loads CompactSampleSize atom from the input stream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_compact_sample_size_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            bitstream.SkipBytes(3);
            sampleSize = 0;
            fieldSize = (int)bitstream.ReadBytes(1);
            sampleCount = (int)bitstream.ReadBytes(4);
            _bytesRead += 8;
            for (int i = 0; i < sampleCount; i++)
            {
                int size = 0;
                switch (fieldSize)
                {
                    case 4:
                        size = (int)bitstream.ReadBytes(1);
                        // TODO check the following code
                        samples.Add(size & 0x0f);
                        size = (size >> 4) & 0x0f;
                        i++;
                        _bytesRead += 1;
                        break;
                    case 8:
                        size = (int)bitstream.ReadBytes(1);
                        _bytesRead += 1;
                        break;
                    case 16:
                        size = (int)bitstream.ReadBytes(2);
                        _bytesRead += 2;
                        break;
                }
                if (i < sampleCount)
                {
                    samples.Add(size);
                }
            }
            return _bytesRead;
        }

        public class Record
        {
            private int firstChunk;
            private int samplesPerChunk;
            private int sampleDescriptionIndex;

            public Record(int firstChunk, int samplesPerChunk, int sampleDescriptionIndex)
            {
                this.firstChunk = firstChunk;
                this.samplesPerChunk = samplesPerChunk;
                this.sampleDescriptionIndex = sampleDescriptionIndex;
            }

            public int FirstChunk
            {
                get { return firstChunk; }
            }
            public int SamplesPerChunk
            {
                get { return samplesPerChunk; }
            }
            public int SampleDescriptionIndex
            {
                get { return sampleDescriptionIndex; }
            }
        }

        /// <summary>
        /// The decoding time to sample table
        /// </summary>
        protected List<Record> records = new List<Record>();

        public List<Record> Records
        {
            get { return records; }
        }

        /// <summary>
        /// Loads MP4SampleToChunkAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_sample_to_chunk_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
            _bytesRead += 4;
            for (int i = 0; i < entryCount; i++)
            {
                int firstChunk = (int)bitstream.ReadBytes(4);
                int samplesPerChunk = (int)bitstream.ReadBytes(4);
                int sampleDescriptionIndex = (int)bitstream.ReadBytes(4);
                records.Add(new Record(firstChunk, samplesPerChunk, sampleDescriptionIndex));
                _bytesRead += 12;
            }
            return _bytesRead;
        }

        protected List<int> syncSamples = new List<int>();

        public List<int> SyncSamples
        {
            get { return syncSamples; }
        }

        /// <summary>
        /// Loads MP4SyncSampleAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_sync_sample_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("Sync sample atom contains keyframe info");
#endif
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
#if !SILVERLIGHT
            log.Debug(string.Format("Sync entries: {0}", entryCount));
#endif
            _bytesRead += 4;
            for (int i = 0; i < entryCount; i++)
            {
                int sample = (int)bitstream.ReadBytes(4);
                //log.trace("Sync entry: {}", sample);
                syncSamples.Add(sample);
                _bytesRead += 4;
            }
            return _bytesRead;
        }

        public class TimeSampleRecord
        {
            private int consecutiveSamples;
            private int sampleDuration;

            public TimeSampleRecord(int consecutiveSamples, int sampleDuration)
            {
                this.consecutiveSamples = consecutiveSamples;
                this.sampleDuration = sampleDuration;
            }

            public int ConsecutiveSamples
            {
                get { return consecutiveSamples; }
            }
            public int SampleDuration
            {
                get { return sampleDuration; }
            }
        }

        protected List<TimeSampleRecord> timeToSamplesRecords = new List<TimeSampleRecord>();

        public List<TimeSampleRecord> TimeToSamplesRecords
        {
            get { return timeToSamplesRecords; }
        }

        /// <summary>
        /// Loads MP4TimeToSampleAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_time_to_sample_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("Time to sample atom");
#endif
            create_full_atom(bitstream);
            entryCount = (int)bitstream.ReadBytes(4);
#if !SILVERLIGHT
            log.Debug(string.Format("Time to sample entries: {0}", entryCount));
#endif
            _bytesRead += 4;
            for (int i = 0; i < entryCount; i++)
            {
                int sampleCount = (int)bitstream.ReadBytes(4);
                int sampleDuration = (int)bitstream.ReadBytes(4);
                //log.trace("Sync entry: {}", sample);
                timeToSamplesRecords.Add(new TimeSampleRecord(sampleCount, sampleDuration));
                _bytesRead += 8;
            }
            return _bytesRead;
        }

        protected int balance;

        /// <summary>
        /// Loads MP4SoundMediaHeaderAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_sound_media_header_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            balance = (int)bitstream.ReadBytes(2);
            bitstream.SkipBytes(2);
            _bytesRead += 4;
            return _bytesRead;
        }

        protected long trackId;
        protected int qt_trackWidth;
        protected int qt_trackHeight;

        /// <summary>
        /// Loads MP4TrackHeaderAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_track_header_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
#if !SILVERLIGHT
            log.Debug(string.Format("Version: {0}", version));
#endif
            if (version == 1)
            {
                creationTime = createDate(bitstream.ReadBytes(8));
                modificationTime = createDate(bitstream.ReadBytes(8));
                trackId = bitstream.ReadBytes(4);
                bitstream.SkipBytes(4);
                duration = bitstream.ReadBytes(8);
                _bytesRead += 32;
            }
            else
            {
                creationTime = createDate(bitstream.ReadBytes(4));
                modificationTime = createDate(bitstream.ReadBytes(4));
                trackId = bitstream.ReadBytes(4);
                bitstream.SkipBytes(4);
                duration = bitstream.ReadBytes(4);
                _bytesRead += 20;
            }
            bitstream.SkipBytes(8); //reserved by Apple
            int qt_layer = (int)bitstream.ReadBytes(2);
            int qt_alternateGroup = (int)bitstream.ReadBytes(2);
            int qt_volume = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Volume: {0}", qt_volume));
#endif
            bitstream.SkipBytes(2); //reserved by Apple
            long qt_matrixA = bitstream.ReadBytes(4);
            long qt_matrixB = bitstream.ReadBytes(4);
            long qt_matrixU = bitstream.ReadBytes(4);
            long qt_matrixC = bitstream.ReadBytes(4);
            long qt_matrixD = bitstream.ReadBytes(4);
            long qt_matrixV = bitstream.ReadBytes(4);
            long qt_matrixX = bitstream.ReadBytes(4);
            long qt_matrixY = bitstream.ReadBytes(4);
            long qt_matrixW = bitstream.ReadBytes(4);
            qt_trackWidth = (int)bitstream.ReadBytes(4);
            width = (qt_trackWidth >> 16);
            qt_trackHeight = (int)bitstream.ReadBytes(4);
            height = (qt_trackHeight >> 16);
            _bytesRead += 60;
            return _bytesRead;
        }

        protected int graphicsMode;
        protected int opColorRed;
        protected int opColorGreen;
        protected int opColorBlue;

        /// <summary>
        /// Loads MP4VideoMediaHeaderAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_video_media_header_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            if ((_size - _bytesRead) == 8)
            {
                graphicsMode = (int)bitstream.ReadBytes(2);
                opColorRed = (int)bitstream.ReadBytes(2);
                opColorGreen = (int)bitstream.ReadBytes(2);
                opColorBlue = (int)bitstream.ReadBytes(2);
                _bytesRead += 8;
            }
            return _bytesRead;
        }

        protected int width;
        protected int height;

        /// <summary>
        /// Loads MP4VisualSampleEntryAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_visual_sample_entry_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("Visual entry atom contains wxh");
#endif
            bitstream.SkipBytes(24);
            width = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Width: {0}", width));
#endif
            height = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Height: {0}", height));
#endif
            bitstream.SkipBytes(50);
            _bytesRead += 78;
            Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
            _children.Add(child);
            _bytesRead += child.Size;
            return _bytesRead;
        }
        /// <summary>
        /// Loads MP4VideoSampleEntryAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_video_sample_entry_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("Video entry atom contains wxh");
#endif
            bitstream.SkipBytes(6);
            int dataReferenceIndex = (int)bitstream.ReadBytes(2);
            bitstream.SkipBytes(2);
            bitstream.SkipBytes(2);
            bitstream.SkipBytes(12);
            width = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Width: {0}", width));
#endif
            height = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Height: {0}", height));
#endif
            int horizontalRez = (int)bitstream.ReadBytes(4) >> 16;
#if !SILVERLIGHT
            log.Debug(string.Format("H Resolution: {0}", horizontalRez));
#endif
            int verticalRez = (int)bitstream.ReadBytes(4) >> 16;
#if !SILVERLIGHT
            log.Debug(string.Format("V Resolution: {0}", verticalRez));
#endif
            bitstream.SkipBytes(4);
            int frameCount = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Frame to sample count: {0}", frameCount));
#endif
            int stringLen = (int)bitstream.ReadBytes(1);
#if !SILVERLIGHT
            log.Debug(string.Format("String length (cpname): {0}", stringLen));
#endif
            String compressorName = bitstream.ReadString(31);
#if !SILVERLIGHT
            log.Debug(string.Format("Compressor name: {0}", compressorName.Trim()));
#endif
            int depth = (int)bitstream.ReadBytes(2);
#if !SILVERLIGHT
            log.Debug(string.Format("Depth: {0}", depth));
#endif
            bitstream.SkipBytes(2);
            _bytesRead += 78;
#if !SILVERLIGHT
            log.Debug(string.Format("Bytes read: {0}", _bytesRead));
#endif
            Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
            _children.Add(child);
            _bytesRead += child.Size;
            return _bytesRead;
        }

        public int Height
        {
            get { return height; }
        }

        public int Width
        {
            get { return width; }
        }

        protected int avcLevel;
        protected int avcProfile;

        public int AvcLevel
        {
            get { return avcLevel; }
        }

        public int AvcProfile
        {
            get { return avcProfile; }
        }

        private byte[] videoConfigBytes;

        public byte[] VideoConfigBytes
        {
            get { return videoConfigBytes; }
        }

        /// <summary>
        /// Loads AVCC atom from the input bitstream.
        /// <para>
        /// 8+ bytes ISO/IEC 14496-10 or 3GPP AVC decode config box
        ///  = long unsigned offset + long ASCII text string 'avcC'
        /// -> 1 byte version = 8-bit hex version  (current = 1)
        /// -> 1 byte H.264 profile = 8-bit unsigned stream profile
        /// -> 1 byte H.264 compatible profiles = 8-bit hex flags
        /// -> 1 byte H.264 level = 8-bit unsigned stream level
        /// -> 1 1/2 nibble reserved = 6-bit unsigned value set to 63
        /// -> 1/2 nibble NAL length = 2-bit length byte size type
        ///  - 1 byte = 0 ; 2 bytes = 1 ; 4 bytes = 3
        /// -> 1 byte number of SPS = 8-bit unsigned total
        /// -> 2+ bytes SPS length = short unsigned length
        /// -> + SPS NAL unit = hexdump
        /// -> 1 byte number of PPS = 8-bit unsigned total
        /// -> 2+ bytes PPS length = short unsigned length
        /// -> + PPS NAL unit = hexdump 
        /// </para>
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_avc_config_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("AVC config");
            log.Debug(string.Format("Offset: {0}", bitstream.Offset));
#endif
            //store the decoder config bytes
            videoConfigBytes = new byte[(int)_size];
            for (int b = 0; b < videoConfigBytes.Length; b++)
            {
                videoConfigBytes[b] = (byte)bitstream.ReadBytes(1);
                switch (b)
                {
                    //0 / version
                    case 1: //profile
                        avcProfile = videoConfigBytes[b];
#if !SILVERLIGHT
                        log.Debug(string.Format("AVC profile: {0}", avcProfile));
#endif
                        break;
                    case 2: //compatible profile
                        int avcCompatProfile = videoConfigBytes[b];
#if !SILVERLIGHT
                        log.Debug(string.Format("AVC compatible profile: {0}", avcCompatProfile));
#endif
                        break;
                    case 3: //avc level
                        avcLevel = videoConfigBytes[b];
#if !SILVERLIGHT
                        log.Debug(string.Format("AVC level: {0}", avcLevel));
#endif
                        break;
                    case 4: //NAL length
                        break;
                    case 5: //SPS number
                        int numberSPS = videoConfigBytes[b];
#if !SILVERLIGHT
                        log.Debug(string.Format("Number of SPS: {0}", numberSPS));
#endif
                        break;
                    default:
                        break;
                }
                _bytesRead++;
            }
            return _bytesRead;
        }
        /// <summary>
        /// Creates the PASP atom or Pixel Aspect Ratio. It is created by Quicktime
        /// when exporting an MP4 file. The atom is required for ipod's and acts as
        /// a container for the avcC atom in these cases.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_pasp_atom(Mp4DataStream bitstream)
        {
#if !SILVERLIGHT
            log.Debug("Pixel aspect ratio");
#endif
            int hSpacing = (int)bitstream.ReadBytes(4);
            int vSpacing = (int)bitstream.ReadBytes(4);
#if !SILVERLIGHT
            log.Debug(string.Format("hSpacing: {0} vSpacing: {1}", hSpacing, vSpacing));
#endif
            _bytesRead += 8;
            Mp4Atom child = Mp4Atom.CreateAtom(bitstream);
            _children.Add(child);
            _bytesRead += child.Size;
            return _bytesRead;
        }

        protected Mp4Descriptor esd_descriptor;

        /// <summary>
        /// Gets the ESD descriptor.
        /// </summary>
        public Mp4Descriptor EsdDescriptor
        {
            get { return esd_descriptor; }
        }

        /// <summary>
        /// Loads M4ESDAtom atom from the input bitstream.
        /// </summary>
        /// <param name="bitstream">The input bitstream.</param>
        /// <returns>The number of bytes loaded.</returns>
        public long create_esd_atom(Mp4DataStream bitstream)
        {
            create_full_atom(bitstream);
            esd_descriptor = Mp4Descriptor.CreateDescriptor(bitstream);
            _bytesRead += esd_descriptor.BytesRead;
            return _bytesRead;
        }

        /// <summary>
        /// Converts the time in seconds since midnight 1 Jan 1904.
        /// </summary>
        /// <param name="movieTime">The time in milliseconds since midnight 1 Jan 1904.</param>
        /// <returns>The DateTime object.</returns>
        public static DateTime createDate(long movieTime)
        {
            return new DateTime(movieTime * 1000 - 2082850791998L);
        }

        public static int TypeToInt(String type)
        {
            int result = (type[0] << 24) + (type[1] << 16) + (type[2] << 8) + type[3];
            return result;
        }

        public static String IntToType(int type)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)((type >> 24) & 0xff));
            sb.Append((char)((type >> 16) & 0xff));
            sb.Append((char)((type >> 8) & 0xff));
            sb.Append((char)(type & 0xff));
            return sb.ToString();
        }

        /// <summary>
        /// Gets children from this atom.
        /// </summary>
        public List<Mp4Atom> Children
        {
            get { return _children; }
        }
        /// <summary>
        /// Gets the size of this atom.
        /// </summary>
        public long Size
        {
            get { return _size; }
        }
        /// <summary>
        /// Gets the type of this atom.
        /// </summary>
        public int Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return IntToType(this.Type);
        }

    }
}
