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
using System.IO;
using System.Text;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.Messaging.Rtmpt;

namespace FluorineFx.Messaging.Rtmp
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    sealed class RtmptProtocolEncoder
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmptProtocolEncoder));
#endif

        public static ByteBuffer HandleBadRequest(string message, RtmptRequest request)
        {
            ByteBuffer buffer = ByteBuffer.Allocate(100);
            StreamWriter sw = new StreamWriter(buffer);
            if (request.HttpVersion == 1)
            {
                sw.Write("HTTP/1.1 400 " + message + "\r\n");
                sw.Write("Cache-Control: no-cache\r\n");
            }
            else
            {
                sw.Write("HTTP/1.0 400 " + message + "\r\n");
                sw.Write("Pragma: no-cache\r\n");
            }
            sw.Write("Content-Type: text/plain\r\n");
            sw.Write("Content-Length: " + message.Length.ToString() + "\r\n");
            sw.Write("Connection: Keep-Alive\r\n");
            sw.Write("\r\n");
            sw.Write(message);
            sw.Flush();
            return buffer;
        }

        public static ByteBuffer ReturnMessage(string message, RtmptRequest request)
        {
            ByteBuffer buffer = ByteBuffer.Allocate(100);
            StreamWriter sw = new StreamWriter(buffer);
            if (request.HttpVersion == 1)
            {
                sw.Write("HTTP/1.1 200 OK\r\n");
                sw.Write("Cache-Control: no-cache\r\n");
            }
            else
            {
                sw.Write("HTTP/1.0 200 OK\r\n");
                sw.Write("Pragma: no-cache\r\n");
            }
            sw.Write("Content-Length: " + message.Length.ToString() + "\r\n");
            sw.Write(string.Format("Content-Type: {0}\r\n", ContentType.RTMPT));
            sw.Write("Connection: Keep-Alive\r\n");
            sw.Write("\r\n");
            sw.Write(message);
            sw.Flush();
            return buffer;
        }

        public static ByteBuffer ReturnMessage(byte message, RtmptRequest request)
        {
            ByteBuffer buffer = ByteBuffer.Allocate(100);
            StreamWriter sw = new StreamWriter(buffer);
            if (request.HttpVersion == 1)
            {
                sw.Write("HTTP/1.1 200 OK\r\n");
                sw.Write("Cache-Control: no-cache\r\n");
            }
            else
            {
                sw.Write("HTTP/1.0 200 OK\r\n");
                sw.Write("Pragma: no-cache\r\n");
            }
            sw.Write("Content-Length: 1\r\n");
            sw.Write("Connection: Keep-Alive\r\n");
            sw.Write(string.Format("Content-Type: {0}\r\n", ContentType.RTMPT));
            sw.Write("\r\n");
            sw.Write((char)message);
            sw.Flush();
            return buffer;
        }

        public static ByteBuffer ReturnMessage(byte pollingDelay, ByteBuffer data, RtmptRequest request)
        {
            ByteBuffer buffer = ByteBuffer.Allocate((int)data.Length + 30);
            StreamWriter sw = new StreamWriter(buffer);
            int contentLength = data.Limit + 1;
            if (request.HttpVersion == 1)
            {
                sw.Write("HTTP/1.1 200 OK\r\n");
                sw.Write("Cache-Control: no-cache\r\n");
            }
            else
            {
                sw.Write("HTTP/1.0 200 OK\r\n");
                sw.Write("Pragma: no-cache\r\n");
            }
            sw.Write(string.Format("Content-Length: {0}\r\n", contentLength));
            sw.Write("Connection: Keep-Alive\r\n");
            sw.Write(string.Format("Content-Type: {0}\r\n", ContentType.RTMPT));
            sw.Write("\r\n");
            sw.Write((char)pollingDelay);
            sw.Flush();
            BinaryWriter bw = new BinaryWriter(buffer);
            byte[] buf = data.ToArray();
            bw.Write(buf);
            bw.Flush();
            return buffer;
        }
    }
}
