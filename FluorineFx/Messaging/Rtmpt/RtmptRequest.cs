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
using FluorineFx.Util;
using FluorineFx.Messaging.Rtmp;

namespace FluorineFx.Messaging.Rtmpt
{
    class RtmptRequest
    {
        string _url;
        string _protocol;
        string _httpMethod;
        Hashtable _headers;
        ByteBuffer _data;
        RtmpConnection _connection;

        public RtmptRequest(RtmpConnection connection, string url, string protocol, string httpMethod, Hashtable headers)
        {
            _connection = connection;
            _url = url;
            _protocol = protocol;
            _httpMethod = httpMethod;
            _headers = headers;
        }

        public string Url { get { return _url; } }
        public string Protocol { get { return _protocol; } }
        public string HttpMethod { get { return _httpMethod; } }
        public Hashtable Headers { get { return _headers; } }

        public int ContentLength
        {
            get
            {
                if (_headers.Contains("Content-Length"))
                {
                    int contentLength = System.Convert.ToInt32(_headers["Content-Length"]);
                    return contentLength;
                }
                return 0;
            }
        }

        public int HttpVersion
        {
            get { return _protocol == "HTTP/1.1" ? 1 : 0; }
        }

        public ByteBuffer Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public RtmpConnection Connection
        {
            get { return _connection; }
        }
    }
}
