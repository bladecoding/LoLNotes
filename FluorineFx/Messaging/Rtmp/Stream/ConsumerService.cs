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
using FluorineFx.Configuration;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Consumer;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Basic consumer service implementation. Used to get pushed messages at consumer endpoint.
    /// </summary>
    class ConsumerService : IConsumerService
    {
        #region IConsumerService Members

        public IMessageOutput GetConsumerOutput(IClientStream stream)
        {
		    IStreamCapableConnection streamConnection = stream.Connection;
            if (streamConnection == null || !(streamConnection is RtmpConnection)) 
			    return null;
            RtmpConnection connection = streamConnection as RtmpConnection;
		    // TODO Better manage channels.
		    // now we use OutputStream as a channel wrapper.
            OutputStream outputStream = connection.CreateOutputStream(stream.StreamId);
		    IPipe pipe = new InMemoryPushPushPipe();
            pipe.Subscribe(new ConnectionConsumer(connection, outputStream.Video.ChannelId, outputStream.Audio.ChannelId, outputStream.Data.ChannelId), null);
		    return pipe;
        }

        #endregion

        #region IService Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}
