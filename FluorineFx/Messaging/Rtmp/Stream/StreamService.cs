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
using log4net;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Stream.Support;
using FluorineFx.Messaging.Api;
using FluorineFx.Context;
using FluorineFx.Configuration;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    class StreamService : IStreamService
    {
        private static ILog log = LogManager.GetLogger(typeof(StreamService));

        /// <summary>
        /// Returns current stream id.
        /// </summary>
        /// <returns></returns>
        private int GetCurrentStreamId()
        {
            return (int)FluorineContext.Current.Connection.GetAttribute(FluorineContext.FluorineStreamIdKey);
        }

        /// <summary>
        /// Returns broadcast scope object for given scope and child scope name.
        /// </summary>
        /// <param name="scope">Scope object.</param>
        /// <param name="name">Child scope name.</param>
        /// <returns>Broadcast scope.</returns>
        public IBroadcastScope GetBroadcastScope(IScope scope, string name) 
        {
		    IBasicScope basicScope = scope.GetBasicScope(Constants.BroadcastScopeType, name);
            return basicScope as IBroadcastScope;
		}
    
        #region IStreamService Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        public int createStream()
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return -1;
            return (connection as IStreamCapableConnection).ReserveStreamId();
        }

        public void closeStream()
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return;
		
		    IClientStream stream = (connection as IStreamCapableConnection).GetStreamById(GetCurrentStreamId());
		    if (stream != null) 
            {
			    if (stream is IClientBroadcastStream) 
                {
				    IClientBroadcastStream bs = stream as IClientBroadcastStream;
				    IBroadcastScope bsScope = GetBroadcastScope(connection.Scope, bs.PublishedName);
				    if (bsScope != null && connection is BaseConnection) 
                    {
                        (connection as BaseConnection).UnregisterBasicScope(bsScope);
				    }
			    }
			    stream.Close();
		    }
            (connection as IStreamCapableConnection).DeleteStreamById(GetCurrentStreamId());       
        }

        public void deleteStream(int streamId)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection)) 
			    return;
            IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
            deleteStream(streamConnection, streamId);
        }

        public void deleteStream(IStreamCapableConnection connection, int streamId)
        {
		    IClientStream stream = connection.GetStreamById(streamId);
		    if (stream != null) 
            {
			    if (stream is IClientBroadcastStream) 
                {
				    IClientBroadcastStream bs = stream as IClientBroadcastStream;
				    IBroadcastScope bsScope = GetBroadcastScope(connection.Scope, bs.PublishedName);
				    if (bsScope != null && connection is BaseConnection) 
                    {
                        (connection as BaseConnection).UnregisterBasicScope(bsScope);
				    }
			    }
			    stream.Close();
		    }
		    connection.UnreserveStreamId(streamId);
        }

        public void releaseStream(string streamName)
        {
        }

        public void play(bool dontStop)
        {
		    if (!dontStop) 
            {
		        IConnection connection = FluorineContext.Current.Connection;
		        if (!(connection is IStreamCapableConnection)) 
			        return;
                IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
			    int streamId = GetCurrentStreamId();
			    IClientStream stream = streamConnection.GetStreamById(streamId);
			    if (stream != null) 
				    stream.Stop();
		    }
        }

        public void play(string name)
        {
            play(name, -2000, -1000, true);
        }

        public void play(string name, double start)
        {
            play(name, start, -1000, true);
        }

        public void play(string name, double start, double length)
        {
            play(name, start, length, true);
        }

        public void play(string name, double start, double length, bool flushPlaylist)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection)) 
			    return;
            IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    IScope scope = connection.Scope;
		    int streamId = GetCurrentStreamId();
		    if (name == null || string.Empty.Equals(name))
            {
			    SendNSFailed(streamConnection as RtmpConnection, "The stream name may not be empty.", name, streamId);
			    return;
		    }
		    IStreamSecurityService security = ScopeUtils.GetScopeService(scope, typeof(IStreamSecurityService)) as IStreamSecurityService;
		    if (security != null) 
            {
			    IEnumerator handlers = security.GetStreamPlaybackSecurity();
			    while(handlers.MoveNext())
                {
                    IStreamPlaybackSecurity handler = handlers.Current as IStreamPlaybackSecurity;
				    if(!handler.IsPlaybackAllowed(scope, name, (long)start, (long)length, flushPlaylist)) 
                    {
                        SendNSFailed(streamConnection as RtmpConnection, "You are not allowed to play the stream.", name, streamId);
					    return;
				    }
			    }
		    }
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    bool created = false;
		    if (stream == null) 
            {
			    stream = streamConnection.NewPlaylistSubscriberStream(streamId);
			    stream.Start();
			    created = true;
		    }
		    if (!(stream is ISubscriberStream))
			    return;
		    ISubscriberStream subscriberStream = stream as ISubscriberStream;
		    SimplePlayItem item = new SimplePlayItem();
		    item.Name = name;
		    item.Start = (long)start;
		    item.Length = (long)length;
		    if (subscriberStream is IPlaylistSubscriberStream) 
            {
			    IPlaylistSubscriberStream playlistStream = subscriberStream as IPlaylistSubscriberStream;
			    if (flushPlaylist)
				    playlistStream.RemoveAllItems();
			    playlistStream.AddItem(item);
		    } 
            else if (subscriberStream is ISingleItemSubscriberStream) 
            {
                ISingleItemSubscriberStream singleStream = subscriberStream as ISingleItemSubscriberStream;
			    singleStream.PlayItem = item;
		    } 
            else 
            {
			    // not supported by this stream service
			    return;
		    }
		    try 
            {
			    subscriberStream.Play();
		    } catch(System.IO.IOException ex) 
            {
			    if (created) 
                {
				    stream.Close();
				    streamConnection.DeleteStreamById(streamId);
			    }
                SendNSFailed(streamConnection as RtmpConnection, ex.Message, name, streamId);
		    }
        }

        public void publish(string name)
        {
            publish(name, Constants.ClientStreamModeLive);
        }

        public void publish(string name, string mode)
        {
	        IConnection connection = FluorineContext.Current.Connection;
	        if (!(connection is IStreamCapableConnection)) 
		        return;
            IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    IScope scope = connection.Scope;
		    int streamId = GetCurrentStreamId();
		    if (name == null || string.Empty.Equals(name)) 
            {
                SendNSFailed(streamConnection as RtmpConnection, "The stream name may not be empty.", name, streamId);
			    return;
		    }

		    IStreamSecurityService security = ScopeUtils.GetScopeService(scope, typeof(IStreamSecurityService)) as IStreamSecurityService;
		    if (security != null) 
            {
			    IEnumerator handlers = security.GetStreamPublishSecurity();
			    while(handlers.MoveNext())
                {
                    IStreamPublishSecurity handler = handlers.Current as IStreamPublishSecurity;
				    if (!handler.IsPublishAllowed(scope, name, mode)) 
                    {
                        SendNSFailed(streamConnection as RtmpConnection, "You are not allowed to publish the stream.", name, streamId);
					    return;
				    }
			    }
		    }
		    IBroadcastScope bsScope = GetBroadcastScope(scope, name);
		    if (bsScope != null && bsScope.GetProviders().Count > 0) 
            {
			    // Another stream with that name is already published.
			    StatusASO badName = new StatusASO(StatusASO.NS_PUBLISH_BADNAME);
			    badName.clientid = streamId;
			    badName.details = name;
			    badName.level = "error";
			    // FIXME: there should be a direct way to send the status
                RtmpChannel channel = (streamConnection as RtmpConnection).GetChannel((byte)(4 + ((streamId - 1) * 5)));
			    channel.SendStatus(badName);
			    return;
		    }
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    if (stream != null && !(stream is IClientBroadcastStream))
			    return;
		    bool created = false;
		    if (stream == null) 
            {
			    stream = streamConnection.NewBroadcastStream(streamId);
			    created = true;
		    }
		    IClientBroadcastStream bs = stream as IClientBroadcastStream;
		    try 
            {
			    bs.PublishedName = name;
			    IScopeContext context = connection.Scope.Context;
			    //IProviderService providerService = (IProviderService)context.getBean(IProviderService.BEAN_NAME);
                IProviderService providerService = ScopeUtils.GetScopeService(connection.Scope, typeof(IProviderService)) as IProviderService;
                // TODO handle registration failure
			    if (providerService.RegisterBroadcastStream(connection.Scope, name, bs)) 
                {
				    bsScope = GetBroadcastScope(connection.Scope, name);
                    bsScope.SetAttribute(Constants.BroadcastScopeStreamAttribute, bs);
				    if (connection is BaseConnection) 
                    {
                        (connection as BaseConnection).RegisterBasicScope(bsScope);
				    }
			    }
			    if (Constants.ClientStreamModeRecord.Equals(mode)) 
                {
				    bs.Start();
				    bs.SaveAs(name, false);
                }
                else if (Constants.ClientStreamModeAppend.Equals(mode)) 
                {
				    bs.Start();
				    bs.SaveAs(name, true);
			    }
                else if (Constants.ClientStreamModeLive.Equals(mode)) 
                {
				    bs.Start();
			    }
			    bs.StartPublishing();
            } 
            catch (System.IO.IOException ex)
            {
                StatusASO accessDenied = new StatusASO(StatusASO.NS_RECORD_NOACCESS);
			    accessDenied.clientid = streamId;
			    accessDenied.description = "The file could not be created/written to." + ex.Message;
			    accessDenied.details = name;
			    accessDenied.level = "error";
			    // FIXME: there should be a direct way to send the status
                RtmpChannel channel = (streamConnection as RtmpConnection).GetChannel((byte)(4 + ((streamId - 1) * 5)));
			    channel.SendStatus(accessDenied);
			    bs.Close();
			    if (created)
				    streamConnection.DeleteStreamById(streamId);
		     } 
            catch (Exception ex)
            {
			    log.Warn("Publish caught exception", ex);
		    }
        }

        public void publish(bool dontStop)
        {
            if (!dontStop)
            {
                IConnection connection = FluorineContext.Current.Connection;
                if (!(connection is IStreamCapableConnection))
                    return;
                IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
                int streamId = GetCurrentStreamId();
                IClientStream stream = streamConnection.GetStreamById(streamId);
                if (!(stream is IBroadcastStream))
                    return;
                IBroadcastStream bs = stream as IBroadcastStream;
                if (bs.PublishedName == null)
                    return;
                IBroadcastScope bsScope = GetBroadcastScope(connection.Scope, bs.PublishedName);
                if (bsScope != null)
                {
                    bsScope.Unsubscribe(bs.Provider);
                    if (connection is BaseConnection)
                        (connection as BaseConnection).UnregisterBasicScope(bsScope);
                }
                bs.Close();
                streamConnection.DeleteStreamById(streamId);
            }
        }

        public void seek(double position)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return;		
		    IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    int streamId = GetCurrentStreamId();
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    if (stream == null || !(stream is ISubscriberStream))
			    return;
            ISubscriberStream subscriberStream = stream as ISubscriberStream;
		    try 
            {
			    subscriberStream.Seek((int)position);
		    } 
            catch (NotSupportedException ex) 
            {
                StatusASO seekFailed = new StatusASO(StatusASO.NS_SEEK_FAILED);
			    seekFailed.clientid = streamId;
			    seekFailed.description = "The stream doesn't support seeking.";
			    seekFailed.level = "error";
                seekFailed.details = ex.Message;
			    // FIXME: there should be a direct way to send the status
                RtmpChannel channel = (streamConnection as RtmpConnection).GetChannel((byte)(4 + ((streamId - 1) * 5)));
			    channel.SendStatus(seekFailed);
		    }
        }

        public void pause(bool pausePlayback, double position)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return;		
		    IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    int streamId = GetCurrentStreamId();
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    if (stream == null || !(stream is ISubscriberStream)) 
			    return;
            ISubscriberStream subscriberStream = stream as ISubscriberStream;
		    if (pausePlayback) 
			    subscriberStream.Pause((int)position);
		    else
			    subscriberStream.Resume((int)position);
        }

        public void receiveVideo(bool receive)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return;		
		    IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    int streamId = GetCurrentStreamId();
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    if (stream == null || !(stream is ISubscriberStream))
			    return;
            ISubscriberStream subscriberStream = stream as ISubscriberStream;
		    subscriberStream.ReceiveVideo(receive);
        }

        public void receiveAudio(bool receive)
        {
		    IConnection connection = FluorineContext.Current.Connection;
		    if (!(connection is IStreamCapableConnection))
			    return;		
		    IStreamCapableConnection streamConnection = connection as IStreamCapableConnection;
		    int streamId = GetCurrentStreamId();
		    IClientStream stream = streamConnection.GetStreamById(streamId);
		    if (stream == null || !(stream is ISubscriberStream))
			    return;
            ISubscriberStream subscriberStream = stream as ISubscriberStream;
		    subscriberStream.ReceiveAudio(receive);
        }

        #endregion

        private void SendNSFailed(RtmpConnection connection, string description, string name, int streamId)
        {
            StatusASO failed = new StatusASO(StatusASO.NS_FAILED);
            failed.clientid = streamId;
            failed.description = description;
            failed.details = name;
            failed.level = "error";
            // FIXME: there should be a direct way to send the status
            RtmpChannel channel = connection.GetChannel((byte)(4 + ((streamId - 1) * 5)));
            channel.SendStatus(failed);
        }
    }
}
