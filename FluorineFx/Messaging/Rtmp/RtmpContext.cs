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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx.Util;
using FluorineFx.Threading;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	enum DecoderState
	{
		/// <summary>
		/// Decoding finished successfully state.
		/// </summary>
		Ok = 0,
		/// <summary>
		/// Deconding continues state.
		/// </summary>
		Continue = 1,
		/// <summary>
		/// Decoder is buffering state constant.
		/// </summary>
		Buffer = 2
	}
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public sealed class RtmpContext : IDisposable
	{
		long _decoderBufferAmount = 0;
		DecoderState _decoderState = DecoderState.Ok;
		ObjectEncoding _objectEncoding;

        /// <summary>
        /// State bit field.
        /// 1 UseLegacyCollection
        /// 2 UseLegacyThrowable
        /// 4 RtmpMode
        /// </summary>
        byte __fields;

		//RtmpMode _mode;
		RtmpState _state;

		int _lastReadChannel = 0x00;
		int _lastWriteChannel = 0x00;

        Dictionary<int, RtmpHeader> _readHeaders = new Dictionary<int, RtmpHeader>();
        Dictionary<int, RtmpHeader> _writeHeaders = new Dictionary<int, RtmpHeader>();
        Dictionary<int, RtmpPacket> _readPackets = new Dictionary<int, RtmpPacket>();
        Dictionary<int, RtmpPacket> _writePackets = new Dictionary<int, RtmpPacket>();

        const int DefaultChunkSize = 128;
		int _readChunkSize = DefaultChunkSize;
		int _writeChunkSize = DefaultChunkSize;

        /// <summary>
        /// Handshake as sent to the client
        /// </summary>
        byte[] _handshake;

        private FastReaderWriterLock _readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the RtmpContext class.
        /// </summary>
        /// <param name="mode"></param>
        public RtmpContext(RtmpMode mode)
		{
			//_mode = mode;
            _readerWriterLock = new FastReaderWriterLock();
            SetMode(mode);
			_objectEncoding = ObjectEncoding.AMF0;
		}
        /// <summary>
        /// Gets or sets object encoding.
        /// </summary>
		public ObjectEncoding ObjectEncoding
		{
			get{ return _objectEncoding; }
			set{ _objectEncoding = value; }
		}
        /// <summary>
        /// Gets or sets whether legacy collection serialization is used for AMF3.
        /// </summary>
        public bool UseLegacyCollection
        {
            get { return (__fields & 1) == 1; }
            set { __fields = (value) ? (byte)(__fields | 1) : (byte)(__fields & ~1); }
        }
        /// <summary>
        /// Gets or sets whether legacy exception serialization is used for AMF3.
        /// </summary>
        public bool UseLegacyThrowable
        {
            get { return (__fields & 2) == 2; }
            set { __fields = (value) ? (byte)(__fields | 2) : (byte)(__fields & ~2); }
        }

        /// <summary>
        /// Gets current RTMP mode.
        /// </summary>
        public RtmpMode Mode
        {
            get { return (RtmpMode)(__fields & 4); }
        }

        internal void SetMode(RtmpMode value)
        {
            __fields = value == RtmpMode.Client ? (byte)(__fields | 4) : (byte)(__fields & ~4);
        }

        /*
        /// <summary>
        /// Gets current RTMP mode.
        /// </summary>
        public RtmpMode Mode
        {
            get { return _mode; }
        }

        internal void SetMode(RtmpMode value)
        {
            _mode = value;
        }
        */

        internal FastReaderWriterLock ReaderWriterLock
        {
            get { return _readerWriterLock; }
        }

		/// <summary>
		/// Current state of protocol.
		/// </summary>
		public RtmpState State
		{
			get{ return _state; }
            set
            {
                try
                {
                    ReaderWriterLock.AcquireWriterLock();
                    _state = value;
                    if (_state == RtmpState.Disconnected)
                    {
                        // Free temporary packets
                        FreePackets(_readPackets);
                        FreePackets(_writePackets);
                    }
                }
                finally
                {
                    ReaderWriterLock.ReleaseWriterLock();
                }
            }
		}


        private void FreePackets(Dictionary<int, RtmpPacket> packets)
        {
            foreach (RtmpPacket packet in packets.Values)
            {
                if (packet != null && packet.Data != null)
                {
                    packet.Data = null;
                }
            }
            packets.Clear();
        }

        internal void SetLastReadHeader(int channelId, RtmpHeader header)
        {
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                _lastReadChannel = channelId;
                _readHeaders[channelId] = header;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Returns the last read header for channel.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Last read header.</returns>
		public RtmpHeader GetLastReadHeader(int channelId) 
		{
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_readHeaders.ContainsKey(channelId))
                    return _readHeaders[channelId] as RtmpHeader;
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }
            return null;
		}

        internal void SetLastWriteHeader(int channelId, RtmpHeader header)
        {
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                _lastWriteChannel = channelId;
                _writeHeaders[channelId] = header;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Returns the last written header for channel.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Last written header.</returns>
        public RtmpHeader GetLastWriteHeader(int channelId)
        {
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_writeHeaders.ContainsKey(channelId))
                    return _writeHeaders[channelId] as RtmpHeader;
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }
            return null;
        }

        internal void SetLastReadPacket(int channelId, RtmpPacket packet)
        {
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                RtmpPacket prevPacket = null;
                if (_readPackets.ContainsKey(channelId))
                    prevPacket = _readPackets[channelId] as RtmpPacket;
                if (prevPacket != null && prevPacket.Data != null)
                {
                    prevPacket.Data = null;
                }
                _readPackets[channelId] = packet;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Returns the last read packet for channel.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Last read packet for that channel.</returns>
        public RtmpPacket GetLastReadPacket(int channelId)
        {
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_readPackets.ContainsKey(channelId))
                    return _readPackets[channelId] as RtmpPacket;
                return null;
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }
        }

        internal void SetLastWritePacket(int channelId, RtmpPacket packet)
        {
            try
            {
                ReaderWriterLock.AcquireWriterLock();
                RtmpPacket prevPacket = null;
                if (_writePackets.ContainsKey(channelId))
                    prevPacket = _writePackets[channelId] as RtmpPacket;
                if (prevPacket != null && prevPacket.Data != null)
                {
                    prevPacket.Data = null;
                }
                _writePackets[channelId] = packet;
            }
            finally
            {
                ReaderWriterLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Returns the last written packet.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Last written packet.</returns>
        public RtmpPacket GetLastWritePacket(int channelId)
        {
            try
            {
                ReaderWriterLock.AcquireReaderLock();
                if (_writePackets.ContainsKey(channelId))
                    return _writePackets[channelId] as RtmpPacket;
            }
            finally
            {
                ReaderWriterLock.ReleaseReaderLock();
            }
            return null;
        }
        /// <summary>
        /// Returns channel being read last.
        /// </summary>
        /// <returns>Last read channel.</returns>
		public int GetLastReadChannel() 
		{
			return _lastReadChannel;
		}
        /// <summary>
        /// Returns channel being written last.
        /// </summary>
        /// <returns>Last write channel.</returns>
		public int GetLastWriteChannel() 
		{
			return _lastWriteChannel;
		}
        /// <summary>
        /// Returns read chunk size. Data is being read chunk-by-chunk.
        /// </summary>
        /// <returns>Read chunk size.</returns>
		public int GetReadChunkSize() 
		{
			return _readChunkSize;
		}

		internal void SetReadChunkSize(int readChunkSize) 
		{
			_readChunkSize = readChunkSize;
		}
        /// <summary>
        /// Returns write chunk size. Data is being written chunk-by-chunk.
        /// </summary>
        /// <returns>Write chunk size.</returns>
		public int GetWriteChunkSize() 
		{
			return _writeChunkSize;
		}

        internal void SetWriteChunkSize(int writeChunkSize) 
		{
			_writeChunkSize = writeChunkSize;
		}
		/// <summary>
		/// Returns current buffer amount.
		/// </summary>
		/// <returns></returns>
		public long GetDecoderBufferAmount() 
		{
			return _decoderBufferAmount;
		}
		/// <summary>
		/// Specifies buffer decoding amount.
		/// </summary>
		/// <param name="amount"></param>
		public void SetBufferDecoding(long amount) 
		{
			_decoderState = DecoderState.Buffer;
			_decoderBufferAmount = amount;
		}
		/// <summary>
		/// Set decoding state as "needed to be continued"
		/// </summary>
		public void ContinueDecoding() 
		{
			_decoderState = DecoderState.Continue;
		}
		/// <summary>
		/// Checks whether remaining buffer size is greater or equal than buffer amount and so if it makes sense to start decoding.
		/// </summary>
		/// <param name="remaining"></param>
		/// <returns></returns>
		public bool CanStartDecoding(long remaining) 
		{
			if(remaining >= _decoderBufferAmount) 
				return true;
			else 
				return false;
		}
		/// <summary>
		/// Starts decoding. Sets state to "ready" and clears buffer amount.
		/// </summary>
		public void StartDecoding() 
		{
			_decoderState = DecoderState.Ok;
			_decoderBufferAmount = 0;
		}
		/// <summary>
		/// Checks whether decoding is complete.
		/// </summary>
		public bool HasDecodedObject
		{
			get{ return _decoderState == DecoderState.Ok; }
		}
		/// <summary>
		/// Checks whether decoding process can be continued.
		/// </summary>
		public bool CanContinueDecoding
		{
			get{ return _decoderState != DecoderState.Buffer; }
		}

        /// <summary>
        /// Store the handshake sent to the client.
        /// </summary>
        /// <param name="data">Handshake data.</param>
        public void SetHandshake(byte[] data)
        {
            _handshake = data;
        }
        /// <summary>
        /// Validate handshake.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool ValidateHandshakeReply(ByteBuffer data, int start, int length)
        {
            try
            {
                if (_handshake == null )
                    return false;
                for (int i = start, j = 0; i < length; i++, j++)
                {
                    if (data[i] != _handshake[9 + j])
                        return false;
                }
                return true;
            }
            finally
            {
                _handshake = null;
            }
        } 

        #region IDisposable Members

        /// <summary>
        /// Disposes the resources used by the RtmpContext.
        /// </summary>
        public void Dispose()
        {
            _handshake = null;
        }

        #endregion
    }
}
