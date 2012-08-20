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
#if !NET_1_1
using System.Collections.Generic;
#endif
using log4net;
using FluorineFx.Collections;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Rtmp.IO.Flv;
using FluorineFx.Messaging.Rtmp.IO.Mp3;
using FluorineFx.Messaging.Rtmp.IO.Mp4;

namespace FluorineFx.Messaging.Rtmp.IO
{
    /// <summary>
    /// Creates streamable file services.
    /// </summary>
    class StreamableFileFactory : IStreamableFileFactory
    {
        private static ILog log = LogManager.GetLogger(typeof(StreamableFileFactory));

        public StreamableFileFactory()
        {
            _services.Add(new FlvService());
            _services.Add(new Mp3Service());
            _services.Add(new Mp4Service());
        }

        /// <summary>
        /// Set of IStreamableFileService instances.
        /// </summary>
#if !NET_1_1
        List<IStreamableFileService> _services = new List<IStreamableFileService>();
#else
        ArrayList _services = new ArrayList();
#endif

        /*
        public void SetServices(Set services)
        {
            _services = services;
        }
        */

        #region IStreamableFileFactory Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        public IStreamableFileService GetService(FileInfo file)
        {
		    log.Info("Get service for file: " + file.Name);
		    // Return first service that can handle the passed file
		    foreach(IStreamableFileService service in _services)
            {
			    if (service.CanHandle(file)) 
                {
                    log.Info("Found service for file: " + file.Name);
				    return service;
			    }
		    }
		    return null;
        }

#if !NET_1_1
        public ICollection<IStreamableFileService> GetServices()
#else
        public ICollection GetServices()
#endif
        {
            //log.Info("StreamableFileFactory get services.");
            return _services;
        }

        #endregion
    }
}
