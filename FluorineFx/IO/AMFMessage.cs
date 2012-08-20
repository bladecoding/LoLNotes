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
using FluorineFx.Exceptions;

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class AMFMessage
	{
        /// <summary>
        /// AMF packet version.
        /// </summary>
		protected ushort _version = 0;
#if !(NET_1_1)
        /// <summary>
        /// AMF packet body values.
        /// </summary>
        protected List<AMFBody> _bodies;
        /// <summary>
        /// AMF packet headers.
        /// </summary>
        protected List<AMFHeader> _headers;
#else
		protected ArrayList _bodies;
		protected ArrayList _headers;
#endif

        /// <summary>
		/// Initializes a new instance of the AMFMessage class.
		/// </summary>
		public AMFMessage() : this(0)
		{
		}
		/// <summary>
		/// Initializes a new instance of the AMFMessage class.
		/// </summary>
		/// <param name="version"></param>
		public AMFMessage(ushort version)
		{
			this._version = version;
#if !(NET_1_1)
            _headers = new List<AMFHeader>(1);
            _bodies = new List<AMFBody>(1);
#else
			_headers = new ArrayList(1);
			_bodies = new ArrayList(1);
#endif
		}
        /// <summary>
        /// Gets the AMF packet version.
        /// </summary>
		public ushort Version
		{
			get{ return _version; }
		}
        /// <summary>
        /// Adds a body to the AMF packet.
        /// </summary>
        /// <param name="body">The body object to add.</param>
		public void AddBody(AMFBody body)
		{
			this._bodies.Add(body);
		}
        /// <summary>
        /// Adds a header to the AMF packet.
        /// </summary>
        /// <param name="header">The header object to add.</param>
		public void AddHeader(AMFHeader header)
		{
			this._headers.Add(header);
		}
        /// <summary>
        /// Gets the body count.
        /// </summary>
		public int BodyCount
		{
			get{ return _bodies.Count; }
		}

#if !(NET_1_1)
        /// <summary>
        /// Gets a readonly collection of AMF bodies.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<AMFBody> Bodies
        {
            get { return _bodies.AsReadOnly(); }
        }
#else
        public ArrayList Bodies
		{
			get{ return ArrayList.ReadOnly(_bodies); }
		}
#endif
		/// <summary>
		/// Gets the header count.
		/// </summary>
		public int HeaderCount
		{
			get{ return _headers.Count; }
		}
        /// <summary>
        /// Gets a single AMF body object by index.
        /// </summary>
        /// <param name="index">The numerical index of the body.</param>
        /// <returns>The body referenced by index.</returns>
		public AMFBody GetBodyAt(int index)
		{
			return _bodies[index] as AMFBody;
		}
        /// <summary>
        /// Gets a single AMF header object by index.
        /// </summary>
        /// <param name="index">The numerical index of the header.</param>
        /// <returns>The header referenced by index.</returns>
		public AMFHeader GetHeaderAt(int index)
		{
			return _headers[index] as AMFHeader;
		}
        /// <summary>
        /// Gets the value of a single AMF header object by name.
        /// </summary>
        /// <param name="header">The name of the header.</param>
        /// <returns>The header referenced by name.</returns>
		public AMFHeader GetHeader(string header)
		{
			for(int i = 0; _headers != null && i < _headers.Count; i++)
			{
				AMFHeader amfHeader = _headers[i] as AMFHeader;
				if( amfHeader.Name == header )
					return amfHeader;
			}
			return null;
		}
        /// <summary>
        /// Removes the named header from teh AMF packet.
        /// </summary>
        /// <param name="header">The name of the header.</param>
        public void RemoveHeader(string header)
        {
            for (int i = 0; _headers != null && i < _headers.Count; i++)
            {
                AMFHeader amfHeader = _headers[i] as AMFHeader;
                if (amfHeader.Name == header)
                {
                    _headers.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Gets the AMF version/encoding used for this AMF packet.
        /// </summary>
		public ObjectEncoding ObjectEncoding
		{
			get
			{
                if (_version == 0 || _version == 1)
                    return ObjectEncoding.AMF0;
                if (_version == 3)
                    return ObjectEncoding.AMF3;
                throw new UnexpectedAMF();
            }
		}
	}
}
