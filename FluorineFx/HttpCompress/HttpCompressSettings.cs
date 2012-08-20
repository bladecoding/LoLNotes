using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;

using FluorineFx.Configuration;

namespace FluorineFx.HttpCompress
{
	/// <summary>
	/// The request types handled.
	/// </summary>
	public enum HandleRequest
	{
		/// <summary>Compress both aspx and amf requests</summary>
		[XmlEnum(Name = "all")]
		All,
		/// <summary>Compress only AMF requests</summary>
		[XmlEnum(Name = "amf")]
		Amf,
		/// <summary>Compress only SWX requests</summary>
		[XmlEnum(Name = "swx")]
		Swx,
		/// <summary>Nothing is compressed</summary>
		[XmlEnum(Name = "none")]
		None=-1
	}

	/// <summary>
	/// The available compression algorithms to use with the HttpCompressionModule.
	/// </summary>
	public enum Algorithms 
	{
		/// <summary>Use the Deflate algorithm</summary>
		[XmlEnum(Name = "deflate")]
		Deflate,
		/// <summary>Use the GZip algorithm</summary>
		[XmlEnum(Name = "gzip")]
		GZip,
		/// <summary>Use the default algorithm (picked by client)</summary>
		[XmlEnum(Name = "default")]
		Default=-1
	}

	/// <summary>
	/// The level of compression to use with deflate.
	/// </summary>
	public enum CompressionLevels 
	{
		/// <summary>Use the default compression level</summary>
		[XmlEnum(Name = "default")]
		Default = -1,
		/// <summary>The highest level of compression.  Also the slowest.</summary>
		[XmlEnum(Name = "highest")]
		Highest = 9,
		/// <summary>A higher level of compression.</summary>
		[XmlEnum(Name = "higher")]
		Higher = 8,
		/// <summary>A high level of compression.</summary>
		[XmlEnum(Name = "high")]
		High = 7,
		/// <summary>More compression.</summary>
		[XmlEnum(Name = "more")]
		More = 6,
		/// <summary>Normal compression.</summary>
		[XmlEnum(Name = "normal")]
		Normal = 5,
		/// <summary>Less than normal compression.</summary>
		[XmlEnum(Name = "less")]
		Less = 4,
		/// <summary>A low level of compression.</summary>
		[XmlEnum(Name = "low")]
		Low = 3,
		/// <summary>A lower level of compression.</summary>
		[XmlEnum(Name = "lower")]
		Lower = 2,
		/// <summary>The lowest level of compression that still performs compression.</summary>
		[XmlEnum(Name = "lowest")]
		Lowest = 1,
		/// <summary>No compression.</summary>
		[XmlEnum(Name = "none")]
		None = 0
	}
}
