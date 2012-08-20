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

using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Service;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public class RtmpChannel
	{
        /// <summary>
        /// RTMP connection used to transfer packets.
        /// </summary>
		RtmpConnection	_connection;
        /// <summary>
        /// Channel id.
        /// </summary>
		int				_channelId;

        internal RtmpChannel(RtmpConnection connection, int channelId)
		{
			_connection = connection;
			_channelId = channelId;
		}
        /// <summary>
        /// Gets the channel id.
        /// </summary>
		public int ChannelId
		{
			get{ return _channelId; }
		}
        /// <summary>
        /// Get the connection.
        /// </summary>
        public RtmpConnection Connection
        {
            get { return _connection; }
        }
        /// <summary>
        /// Closes channel with this id on RTMP connection.
        /// </summary>
		public void Close()
		{
			_connection.CloseChannel(_channelId);
		}
        /// <summary>
        /// Writes packet from event data to the RTMP connection.
        /// </summary>
        /// <param name="message">Event data.</param>
        public void Write(IRtmpEvent message)
		{
            IClientStream stream = null;
            if( _connection is IStreamCapableConnection )
                stream = (_connection as IStreamCapableConnection).GetStreamByChannelId(_channelId);
			if(_channelId > 3 && stream == null) 
			{
				//Stream doesn't exist any longer, discarding message
				return;
			}
			int streamId = (stream == null) ? 0 : stream.StreamId;
			Write(message, streamId);
		}
        /// <summary>
        /// Writes packet from event data to the RTMP connection using the specified stream id.
        /// </summary>
        /// <param name="message">Event data.</param>
        /// <param name="streamId">Stream id.</param>
        private void Write(IRtmpEvent message, int streamId) 
		{
			RtmpHeader header = new RtmpHeader();
			RtmpPacket packet = new RtmpPacket(header, message);

			header.ChannelId = _channelId;
			header.Timer = message.Timestamp;
			header.StreamId = streamId;
			header.DataType = message.DataType;
            if( message.Header != null )
			    header.IsTimerRelative =  message.Header.IsTimerRelative;
			_connection.Write(packet);
		}
        /// <summary>
        /// Sends status notification.
        /// </summary>
        /// <param name="status">Status object.</param>
        public void SendStatus(StatusASO status)
        {
            bool andReturn = !status.code.Equals(StatusASO.NS_DATA_START);
            Invoke invoke;
            if (andReturn)
            {
                PendingCall call = new PendingCall(null, "onStatus", new object[] { status });
                invoke = new Invoke();
                invoke.InvokeId = 1;
                invoke.ServiceCall = call;
            }
            else
            {
                Call call = new Call(null, "onStatus", new object[] { status });
                invoke = (Invoke)new Notify();
                invoke.InvokeId = 1;
                invoke.ServiceCall = call;
            }
            // We send directly to the corresponding stream as for
            // some status codes, no stream has been created and thus
            // "getStreamByChannelId" will fail.
            Write(invoke, _connection.GetStreamIdForChannel(_channelId));
        }
        /// <summary>
        /// Returns a string that represents the current RtmpChannel object.
        /// </summary>
        /// <returns>A string that represents the current RtmpChannel object.</returns>
        public override string ToString()
		{
            return "RTMP Channel " + _channelId;
		}

	}
}
