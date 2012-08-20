using System;
using System.IO;

#if (NET_1_1)
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
#else
using System.IO.Compression;
#endif

namespace FluorineFx.HttpCompress
{
#if (NET_1_1)
	/// <summary>
	/// A filter to support HTTP compression using the Deflate algorithm.
	/// </summary>
	class DeflateFilter : CompressingFilter 
	{

		/// <summary>
		/// compression stream member
		/// has to be a member as we can only have one instance of the
		/// actual filter class
		/// </summary>
		private DeflaterOutputStream m_stream = null;
		private Deflater	_deflater = null;
		/// <summary>
		/// Basic constructor that uses the Normal compression level
		/// </summary>
		/// <param name="baseStream">The stream to wrap up with the deflate algorithm</param>
		public DeflateFilter(Stream baseStream) : this(baseStream, CompressionLevels.Normal) { }

		/// <summary>
		/// Full constructor that allows you to set the wrapped stream and the level of compression
		/// </summary>
		/// <param name="baseStream">The stream to wrap up with the deflate algorithm</param>
		/// <param name="compressionLevel">The level of compression to use</param>
		public DeflateFilter(Stream baseStream, CompressionLevels compressionLevel) : base(baseStream, compressionLevel) 
		{
			_deflater = GetDeflater();
			m_stream = new DeflaterOutputStream(baseStream, _deflater);
		}

		private Deflater GetDeflater() 
		{
			switch(CompressionLevel) 
			{
				case CompressionLevels.High:
					return new Deflater(Deflater.BEST_COMPRESSION, true);
				case CompressionLevels.Low:
					return new Deflater(Deflater.BEST_SPEED, true);
				case CompressionLevels.Normal:
				default:
					return new Deflater(Deflater.DEFAULT_COMPRESSION, true);
			}
		}

		/// <summary>
		/// Write out bytes to the underlying stream after compressing them using deflate
		/// </summary>
		/// <param name="buffer">The array of bytes to write</param>
		/// <param name="offset">The offset into the supplied buffer to start</param>
		/// <param name="count">The number of bytes to write</param>
		public override void Write(byte[] buffer, int offset, int count) 
		{
			if(!HasWrittenHeaders) WriteHeaders();
			m_stream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Return the Http name for this encoding.  Here, deflate.
		/// </summary>
		public override string ContentEncoding 
		{
			get { return "deflate"; }
		}

		/// <summary>
		/// Closes this Filter and calls the base class implementation.
		/// </summary>
		public override void Close() 
		{
			m_stream.Close();
		}

		/// <summary>
		/// Flushes that the filter out to underlying storage
		/// </summary>
		public override void Flush() 
		{
			m_stream.Flush();
		}

		public override long TotalIn
		{
			get
			{
				if( _deflater != null )
					return _deflater.TotalIn;
				else
					return base.TotalIn;
			}
		}

		public override long TotalOut
		{
			get
			{ 
				if( _deflater != null )
					return _deflater.TotalOut;
				else
					return base.TotalOut;				
			}
		}
	}
#else
    /// <summary>
    /// A filter to support HTTP compression using the Deflate algorithm.
    /// </summary>
    class DeflateFilter : CompressingFilter
    {
        /// <summary>
        /// compression stream member
        /// has to be a member as we can only have one instance of the
        /// actual filter class
        /// </summary>
        private DeflateStream m_stream = null;

        /// <summary>
        /// Basic constructor that uses the Normal compression level
        /// </summary>
        /// <param name="baseStream">The stream to wrap up with the deflate algorithm</param>
        public DeflateFilter(Stream baseStream) : this(baseStream, CompressionLevels.Normal) { }

        /// <summary>
        /// Full constructor that allows you to set the wrapped stream and the level of compression
        /// </summary>
        /// <param name="baseStream">The stream to wrap up with the deflate algorithm</param>
        /// <param name="compressionLevel">The level of compression to use</param>
        public DeflateFilter(Stream baseStream, CompressionLevels compressionLevel)
            : base(baseStream, compressionLevel)
        {
            m_stream = new DeflateStream(baseStream, CompressionMode.Compress);
        }

        /// <summary>
        /// Write out bytes to the underlying stream after compressing them using deflate
        /// </summary>
        /// <param name="buffer">The array of bytes to write</param>
        /// <param name="offset">The offset into the supplied buffer to start</param>
        /// <param name="count">The number of bytes to write</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!HasWrittenHeaders) WriteHeaders();
            m_stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Return the Http name for this encoding.  Here, deflate.
        /// </summary>
        public override string ContentEncoding
        {
            get { return "deflate"; }
        }

        /// <summary>
        /// Closes this Filter and calls the base class implementation.
        /// </summary>
        public override void Close()
        {
            m_stream.Close();
        }

        /// <summary>
        /// Flushes that the filter out to underlying storage
        /// </summary>
        public override void Flush()
        {
            m_stream.Flush();
        }
    }
#endif
}
