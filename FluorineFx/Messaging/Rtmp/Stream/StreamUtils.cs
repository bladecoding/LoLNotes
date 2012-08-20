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
using log4net;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Stream helper methods.
    /// </summary>
    [CLSCompliant(false)]
    public class StreamUtils
    {
        static private ILog log = LogManager.GetLogger(typeof(StreamUtils));

        private StreamUtils()
        {
        }

        /// <summary>
        /// Creates server stream.
        /// </summary>
        /// <param name="scope">Scope of stream.</param>
        /// <param name="name">Name of stream.</param>
        /// <returns></returns>
        public static IServerStream CreateServerStream(IScope scope, string name)
        {
            log.Debug("Creating server stream: " + name + " Scope: " + scope);
            ServerStream stream = new ServerStream();
            stream.Scope = scope;
            stream.Name = name;
            stream.PublishedName = name;
            return stream;
        }
    }
}
