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
using System.Threading;
using log4net;
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Statistics;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.Messaging;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Codec;
using FluorineFx.Messaging.Rtmp.Stream.Consumer;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
using FluorineFx.Messaging.Messages;
using FluorineFx.Scheduling;
using FluorineFx.Threading;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Stream of playlist subsciber.
    /// </summary>
    class PlaylistSubscriberStream : AbstractClientStream, IPlaylistSubscriberStream, IPlaylistSubscriberStreamStatistics
    {
        private static ILog log = LogManager.GetLogger(typeof(PlaylistSubscriberStream));

        /// <summary>
        /// Playlist controller
        /// </summary>
	    private IPlaylistController _controller;
        /// <summary>
        /// Default playlist controller
        /// </summary>
	    private IPlaylistController _defaultController;
        /// <summary>
        /// Playlist items
        /// </summary>
	    private IList _items;
        /// <summary>
        /// Current item index
        /// </summary>
	    private int _currentItemIndex;
        /// <summary>
        /// Plays items back
        /// </summary>
	    private PlayEngine _engine;
        /// <summary>
        /// Service that controls bandwidth
        /// </summary>
	    private IBWControlService _bwController;
        /// <summary>
        /// Operating context for bandwidth controller
        /// </summary>
	    private IBWControlContext _bwContext;
        /// <summary>
        /// Rewind mode state
        /// </summary>
	    private bool _isRewind;
        /// <summary>
        /// Random mode state
        /// </summary>
	    private bool _isRandom;
        /// <summary>
        /// Repeat mode state
        /// </summary>
	    private bool _isRepeat;
        // <summary>
        // Executor that will be used to schedule stream playback to keep the client buffer filled.
        // </summary>
	    //private volatile ScheduledThreadPoolExecutor executor;
        /// <summary>
        /// Interval in ms to check for buffer underruns in VOD streams.
        /// </summary>
        private int _bufferCheckInterval = 0;

        public int BufferCheckInterval
        {
            get { return _bufferCheckInterval; }
            set { _bufferCheckInterval = value; }
        }
        /// <summary>
        /// Number of pending messages at which a <code>NetStream.Play.InsufficientBW</code> message is generated for VOD streams.
        /// </summary>
        private int _underrunTrigger = 10;

        public int UnderrunTrigger
        {
            get { return _underrunTrigger; }
            set { _underrunTrigger = value; }
        }
        /// <summary>
        /// Number of bytes sent.
        /// </summary>
	    private long _bytesSent = 0;

        public PlaylistSubscriberStream()
        {
            _defaultController = new SimplePlaylistController();
            _items = new ArrayList();
            //_engine = new PlayEngine(this);
            _currentItemIndex = 0;
            _creationTime = System.Environment.TickCount;
        }


        /// <summary>
        /// Creates a play engine based on current services (scheduling service, consumer service, and provider service).
        /// </summary>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="consumerService">The consumer service.</param>
        /// <param name="providerService">The provider service.</param>
        /// <returns>Play engine.</returns>
        PlayEngine CreateEngine(ISchedulingService schedulingService, IConsumerService consumerService, IProviderService providerService)
        {
            _engine = new PlayEngine.Builder(this, schedulingService, consumerService, providerService).Build();
            return _engine;
        }

        public override void Start()
        {
            // Ensure the play engine exists
            if (_engine == null)
            {
                IScope scope = this.Scope;
                if (scope != null)
                {
                    ISchedulingService schedulingService = scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
                    IConsumerService consumerService = scope.GetService(typeof(IConsumerService)) as IConsumerService;
                    IProviderService providerService = scope.GetService(typeof(IProviderService)) as IProviderService;
                    _engine = new PlayEngine.Builder(this, schedulingService, consumerService, providerService).Build();
                }
                else
                {
                    if( log.IsErrorEnabled )
                        log.Error("Scope was null on start");
                }
            }

            // Create bw control service and register myself
            // Bandwidth control service should not be bound to a specific scope because it's designed to control
            // the bandwidth system-wide.
            _bwController = this.Scope.GetService(typeof(IBWControlService)) as IBWControlService;
            _bwContext = _bwController.RegisterBWControllable(this);
            //Set bandwidth members on the engine
            _engine.SetBandwidthController(_bwController, _bwContext);
            //Set buffer check interval
            _engine.BufferCheckInterval = _bufferCheckInterval;
            //Set underrun trigger
            _engine.UnderrunTrigger = _underrunTrigger;
            // Start playback engine
            _engine.Start();
            // Notify subscribers on start
            NotifySubscriberStart();
        }

        public override void Stop()
        {
            try
            {
                if (this.State != State.STOPPED)
                    _engine.Stop();
            }
            catch (IllegalStateException)
            {
                log.Debug("Stop caught an IllegalStateException");
            }
        }

        public override void Close()
        {
            _engine.Close();
            // unregister from bandwidth controller
            _bwController.UnregisterBWControllable(_bwContext);
            NotifySubscriberClose();
        }

        public override IBandwidthConfigure BandwidthConfiguration
        {
            get
            {
                return base.BandwidthConfiguration;
            }
            set
            {
                base.BandwidthConfiguration = value;
                _engine.UpdateBandwithConfigure();
            }
        }

        #region IPlaylistSubscriberStream Members

        public IPlaylistSubscriberStreamStatistics Statistics
        {
            get { return this; }
        }

        #endregion

        #region ISubscriberStream Members

        public void Play()
        {
		    lock(this.SyncRoot) 
            {
                // Return if playlist is empty
                if (_items.Count == 0)
				    return;
                // Move to next if current item is set to -1
                if (_currentItemIndex == -1)
				    MoveToNext();
                // Get playlist item
                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                // Check how many is yet to play...
                int count = _items.Count;
                // If there's some more items on list then play current item
                while(count-- > 0) 
                {
				    try 
                    {
					    _engine.Play(item);
					    break;
				    } 
                    catch (StreamNotFoundException) 
                    {
					    // go for next item
					    MoveToNext();
					    if (_currentItemIndex == -1) 
                        {
						    // we reached the end
						    break;
					    }
					    item = _items[_currentItemIndex] as IPlayItem;
				    }
                    catch (IllegalStateException) 
                    {
					    // a stream is already playing
					    break;
				    }
			    }
            }
        }

        public void Pause(int position)
        {
            try
            {
                _engine.Pause(position);
            }
            catch (NotSupportedException)
            {
                log.Debug("Pause caught an NotSupportedException");
            }
            
        }

        public void Resume(int position)
        {
            try
            {
                _engine.Resume(position);
            }
            catch (NotSupportedException)
            {
                log.Debug("Resume caught an NotSupportedException");
            }
        }

        public void Seek(int position)
        {
            try
            {
                _engine.Seek(position);
            }
            catch (NotSupportedException)
            {
                log.Debug("Seek caught an NotSupportedException");
            }
        }

        public bool IsPaused
        {
            get { return _engine.IsPaused; }
        }

        public void ReceiveVideo(bool receive)
        {
            bool receiveVideo = _engine.ReceiveVideo(receive);
            if (!receiveVideo && receive)
            {
                //Video has been re-enabled
                SeekToCurrentPlayback();
            }
        }

        public void ReceiveAudio(bool receive)
        {
            //Check if engine currently receives audio, returns previous value
            bool receiveAudio = _engine.ReceiveAudio(receive);
            if (receiveAudio && !receive)
            {
                //Send a black audio packet to reset the player
                _engine.SendBlankAudio = true;
            }
            else if (!receiveAudio && receive)
            {
                //Do a seek	
                SeekToCurrentPlayback();
            }		
        }

        #endregion

        #region IPlaylist Members

        public void AddItem(IPlayItem item)
        {
            lock(this.SyncRoot)
            {
			    _items.Add(item);
		    }
        }

        public void AddItem(IPlayItem item, int index)
        {
            lock (this.SyncRoot)
            {
                _items.Insert(index, item);
            }
        }

        public void RemoveItem(int index)
        {
            lock (this.SyncRoot)
            {
                if (index < 0 || index >= _items.Count)
                    return;
                int initialSize = _items.Count;
                _items.RemoveAt(index);
                if (_currentItemIndex == index)
                {
                    // set the next item.
                    if (index == initialSize - 1)
                    {
                        _currentItemIndex = index - 1;
                    }
                }
            }
        }

        public void RemoveAllItems()
        {
            lock (this.SyncRoot)
            {
                // we try to stop the engine first
                Stop();
                _items.Clear();
            }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public int CurrentItemIndex
        {
            get
            {
                return _currentItemIndex;
            }
        }

        public IPlayItem CurrentItem
        {
            get { return GetItem(CurrentItemIndex); }
        }

        public IPlayItem GetItem(int index)
        {
            try
            {
                return _items[index] as IPlayItem;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public bool HasMoreItems
        {
            get 
            {
                lock (this.SyncRoot)
                {
                    int nextItem = _currentItemIndex + 1;
                    if (nextItem >= _items.Count && !IsRepeat)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        public int ItemSize
        {
            get{ return _items.Count; }
        }

        public void PreviousItem()
        {
            lock (this.SyncRoot)
            {
                Stop();
                MoveToPrevious();
                if (_currentItemIndex == -1)
                {
                    return;
                }
                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                int count = _items.Count;
                while (count-- > 0)
                {
                    try
                    {
                        _engine.Play(item);
                        break;
                    }
                    catch (IOException ex)
                    {
                        log.Error("Error while starting to play item, moving to next.", ex);
                        // go for next item
                        MoveToPrevious();
                        if (_currentItemIndex == -1)
                        {
                            // we reached the end.
                            break;
                        }
                        item = _items[_currentItemIndex] as IPlayItem;
                    }
                    catch (StreamNotFoundException)
                    {
                        // go for next item
                        MoveToPrevious();
                        if (_currentItemIndex == -1)
                        {
                            // we reached the end.
                            break;
                        }
                        item = _items[_currentItemIndex] as IPlayItem;
                    }
                    catch (NotSupportedException)
                    {
                        // a stream is already playing
                        break;
                    }
                }
            }
        }

        public void NextItem()
        {
            lock (this.SyncRoot)
            {
                MoveToNext();
                if (_currentItemIndex == -1)
                    return;

                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                int count = _items.Count;
                while (count-- > 0)
                {
                    try
                    {
                        _engine.Play(item, false);
                        break;
                    }
                    catch (IOException ex)
                    {
                        log.Error("Error while starting to play item, moving to next.", ex);
                        // go for next item
                        MoveToNext();
                        if (_currentItemIndex == -1)
                        {
                            // we reached the end.
                            break;
                        }
                        item = _items[_currentItemIndex] as IPlayItem;
                    }
                    catch (StreamNotFoundException)
                    {
                        // go for next item
                        MoveToNext();
                        if (_currentItemIndex == -1)
                        {
                            // we reaches the end.
                            break;
                        }
                        item = _items[_currentItemIndex] as IPlayItem;
                    }
                    catch (NotSupportedException)
                    {
                        // a stream is already playing
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set the current item for playing.
        /// </summary>
        /// <param name="index">Position in list</param>
        public void SetItem(int index)
        {
            lock (this.SyncRoot)
            {
                if (index < 0 || index >= _items.Count)
                    return;

                Stop();
                _currentItemIndex = index;
                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                try
                {
                    _engine.Play(item);
                }
                catch (IOException ex)
                {
                    log.Error("SetItem caught a IOException", ex);
                }
                catch (StreamNotFoundException)
                {
                    // let the engine retain the STOPPED state
                    // and wait for control from outside
                    log.Debug("SetItem caught a StreamNotFoundException");
                }
                catch (NotSupportedException ex)
                {
                    log.Error("Illegal state exception on playlist item setup", ex);
                }
            }
        }

        public bool IsRandom
        {
            get { return _isRandom; }
            set { _isRandom = value; }
        }

        public bool IsRewind
        {
            get { return _isRewind; }
            set { _isRewind = value; }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { _isRepeat = value; }
        }


        public void SetPlaylistController(IPlaylistController controller)
        {
            _controller = controller;
        }

        #endregion

        #region IPlaylistSubscriberStreamStatistics Members

        public long BytesSent
        {
            get { return _bytesSent; }
        }

        public double EstimatedBufferFill
        {
            get
            {
                IRtmpEvent msg = _engine.LastMessage;
                if (msg == null)
                {
                    // Nothing has been sent yet
                    return 0.0;
                }
                // Buffer size as requested by the client
                long buffer = this.ClientBufferDuration;
                if (buffer == 0)
                    return 100.0;
                // Duration the stream is playing
                long delta = System.Environment.TickCount - _engine.PlaybackStart;
                // Expected amount of data present in client buffer
                long buffered = msg.Timestamp - delta;
                return (buffered * 100.0) / buffer;
            }
        }

        #endregion

        #region IStreamStatistics Members

        public int CurrentTimestamp
        {
            get 
            { 
    	        IRtmpEvent msg = _engine.LastMessage;
    	        if (msg == null)
    		        return 0;
    	        return msg.Timestamp;
            }
        }

        #endregion

        #region IStatisticsBase Members

        #endregion

        /// <summary>
        /// Seek to current position to restart playback with audio and/or video.
        /// </summary>
        private void SeekToCurrentPlayback()
        {
            if (_engine.IsPullMode)
            {
                try
                {
                    // TODO: figure out if this is the correct position to seek to
                    long delta = System.Environment.TickCount - _engine.PlaybackStart;
                    _engine.Seek((int)delta);
                }
                catch (NotSupportedException)
                {
                    // Ignore error, should not happen for pullMode engines
                }
            }
        }
        /// <summary>
        /// Move the current item to the next in list.
        /// </summary>
        private void MoveToNext()
        {
            if (_controller != null)
                _currentItemIndex = _controller.NextItem(this, _currentItemIndex);
            else
                _currentItemIndex = _defaultController.NextItem(this, _currentItemIndex);
        }
        /// <summary>
        /// Move the current item to the previous in list.
        /// </summary>
        private void MoveToPrevious()
        {
            if (_controller != null)
                _currentItemIndex = _controller.PreviousItem(this, _currentItemIndex);
            else
                _currentItemIndex = _defaultController.PreviousItem(this, _currentItemIndex);
        }
        /// <summary>
        /// Notified by the play engine when the current item reaches the end.
        /// </summary>
        internal void OnItemEnd()
        {
            NextItem();
        }


        /// <summary>
        /// Notifies subscribers on start.
        /// </summary>
        private void NotifySubscriberStart()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamSubscriberStart(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamSubscriberStart", ex);
                }
            }
        }
        /// <summary>
        /// Notifies subscribers on stop.
        /// </summary>
        private void NotifySubscriberClose()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamSubscriberClose(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamSubscriberClose", ex);
                }
            }
        }
        /// <summary>
        /// Notifies subscribers on item playback.
        /// </summary>
        /// <param name="item">Item being played.</param>
        /// <param name="isLive">Is it a live broadcasting?</param>
        internal void NotifyItemPlay(IPlayItem item, bool isLive)
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamPlaylistItemPlay(this, item, isLive);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamPlaylistItemPlay", ex);
                }
            }
        }
        /// <summary>
        /// Notifies subscribers on item stop
        /// </summary>
        /// <param name="item">Item that just has been stopped.</param>
        internal void NotifyItemStop(IPlayItem item)
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamPlaylistItemStop(this, item);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamPlaylistItemStop", ex);
                }
            }
        }
        /// <summary>
        /// Notifies subscribers on pause.
        /// </summary>
        /// <param name="item">Item that just has been paused.</param>
        /// <param name="position">Playback head position.</param>
        internal void NotifyItemPause(IPlayItem item, int position)
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamPlaylistVODItemPause(this, item, position);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamPlaylistVODItemPause", ex);
                }
            }
        }
        /// <summary>
        /// Notifies subscribers on resume
        /// </summary>
        /// <param name="item">Item that just has been resumed.</param>
        /// <param name="position">Playback head position.</param>
        internal void NotifyItemResume(IPlayItem item, int position)
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamPlaylistVODItemResume(this, item, position);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamPlaylistVODItemResume", ex);
                }
            }
        }
        /// <summary>
        /// Notify on item seek
        /// </summary>
        /// <param name="item">Playlist item.</param>
        /// <param name="position">Seek position.</param>
        internal void NotifyItemSeek(IPlayItem item, int position)
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamPlaylistVODItemSeek(this, item, position);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamPlaylistVODItemSeek", ex);
                }
            }
        }

        /// <summary>
        /// Notified by RTMPHandler when a message has been sent. Glue for old code base.
        /// </summary>
        /// <param name="message">Message that has been written.</param>
        public void Written(object message)
        {
            if (!_engine.IsPullMode)
            {
                // Not needed for live streams
                return;
            }
            /*
            try
            {
                _engine.PullAndPush();
            }
            catch (Exception ex)
            {
                log.Error("Error while pulling message.", ex);
            }
            */
            ThreadPoolEx.Global.QueueUserWorkItem(new WaitCallback(WrittenAsync), null);
        }

        private void WrittenAsync(object unused)
        {
            try
            {
                _engine.PullAndPush();
            }
            catch (Exception ex)
            {
                log.Error("Error while pulling message.", ex);
            }
        }
    }
}
