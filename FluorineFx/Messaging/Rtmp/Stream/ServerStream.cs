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
using System.Collections.Generic;
using System.IO;
using log4net;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Messaging;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
using FluorineFx.Messaging.Rtmp.Stream.Consumer;
using FluorineFx.Scheduling;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Implementation for server side stream.
    /// </summary>
    /// <example>
    /// 	<code lang="CS" title="Server stream">
    /// 		<![CDATA[
    /// public class WebRadioApplication : ApplicationAdapter
    ///     {
    ///         public override bool AppStart(IScope application)
    ///         {
    ///             IServerStream stream = StreamUtils.CreateServerStream(this.Scope, "webradio");
    ///             SimplePlayItem playItem = new SimplePlayItem();
    ///             playItem.Name = "track1.mp3";            
    ///             stream.AddItem(playItem);
    ///             playItem = new SimplePlayItem();
    ///             playItem.Name = "track2.mp3";
    ///             stream.AddItem(playItem);
    ///             stream.IsRewind = true;
    ///             stream.Start();
    ///             return base.AppStart(application);
    ///         }
    ///     }]]>
    /// 	</code>
    /// </example>
    [CLSCompliant(false)]
    public class ServerStream : AbstractStream, IServerStream, IFilter, IPushableConsumer, IPipeConnectionListener
    {
        static private ILog log = LogManager.GetLogger(typeof(ServerStream));

        private static long WAIT_THRESHOLD = 0;
        /// <summary>
        /// Stream published name
        /// </summary>
        protected String _publishedName;
        /// <summary>
        /// Actual playlist controller
        /// </summary>
        protected IPlaylistController _controller;
        /// <summary>
        /// Default playlist controller
        /// </summary>
        protected IPlaylistController _defaultController;
        /// <summary>
        /// Rewind flag state
        /// </summary>
        private bool _isRewind;
        /// <summary>
        /// Random flag state
        /// </summary>
        private bool _isRandom;
        /// <summary>
        /// Repeat flag state
        /// </summary>
        private bool _isRepeat;
        /// <summary>
        /// List of items in this playlist
        /// </summary>
        protected ArrayList _items;
        /// <summary>
        /// Current item index
        /// </summary>
        private int _currentItemIndex;
        /// <summary>
        /// Current item
        /// </summary>
        protected IPlayItem _currentItem;
        /// <summary>
        /// Message input
        /// </summary>
        private IMessageInput _msgIn;
        /// <summary>
        /// Message output
        /// </summary>
        private IMessageOutput _msgOut;
        /// <summary>
        /// Pipe for recording
        /// </summary>
        private IPipe _recordPipe;
        /// <summary>
        /// The filename we are recording to.
        /// </summary>
        protected string _recordingFilename;
        /// <summary>
        /// VOD start timestamp
        /// </summary>
        private long _vodStartTS;
        /// <summary>
        /// Server start timestamp
        /// </summary>
        private long _serverStartTS;
        /// <summary>
        /// Next msg's timestamp
        /// </summary>
        private long _nextTS;
        /// <summary>
        /// Next RTMP message
        /// </summary>
        private RtmpMessage _nextRTMPMessage;
        /// <summary>
        /// Listeners to get notified about received packets
        /// </summary>
        private CopyOnWriteArraySet _listeners = new CopyOnWriteArraySet();

        /// <summary>
        /// Scheduling service
        /// </summary>
        private ISchedulingService _schedulingService;
        /// <summary>
        /// Live broadcasting scheduled job name
        /// </summary>
        private string _liveJobName;
        /// <summary>
        /// VOD scheduled job name
        /// </summary>
        private string _vodJobName;

        internal ServerStream()
        {
            _defaultController = new SimplePlaylistController();
            _items = new ArrayList();
            _state = State.UNINIT;
        }

        #region IServerStream Members

        /// <summary>
        /// Toggles the paused state.
        /// </summary>
        public void Pause()
        {
            if (_state == State.PLAYING)
            {
                _state = State.PAUSED;
            }
            else if (_state == State.PAUSED)
            {
                _state = State.PLAYING;
                _vodStartTS = 0;
                _serverStartTS = System.Environment.TickCount;
                ScheduleNextMessage();
            }
        }
        /// <summary>
        /// Seek to a given position in the stream.
        /// </summary>
        /// <param name="position">New playback position in milliseconds.</param>
        public void Seek(int position)
        {
            if (_state != State.PLAYING && _state != State.PAUSED)
                // Can't seek when stopped/closed
                return;
            SendVODSeekCM(_msgIn, position);
        }

        #endregion

        #region IPlaylist Members

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">Playlist item.</param>
        public void AddItem(IPlayItem item)
        {
            lock (this.SyncRoot)
            {
                _items.Add(item);
            }
        }
        /// <summary>
        /// Adds an item to specific index.
        /// </summary>
        /// <param name="item">Playlist item.</param>
        /// <param name="index">Index in list.</param>
        public void AddItem(IPlayItem item, int index)
        {
            lock (this.SyncRoot)
            {
                _items.Insert(index, item);
            }
        }
        /// <summary>
        /// Removes an item from list.
        /// </summary>
        /// <param name="index">Index in list.</param>
        public void RemoveItem(int index)
        {
            lock (this.SyncRoot)
            {
                if (index < 0 || index >= _items.Count)
                    return;
                _items.RemoveAt(index);
            }
        }
        /// <summary>
        /// Remove all items.
        /// </summary>
        public void RemoveAllItems()
        {
            lock (this.SyncRoot)
            {
                _items.Clear();
            }
        }
        /// <summary>
        /// Gets the number of items in list.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return _items.Count;
                }
            }
        }
        /// <summary>
        /// Gets the currently playing item index.
        /// </summary>
        public int CurrentItemIndex
        {
            get { return _currentItemIndex; }
        }
        /// <summary>
        /// Gets the currently playing item.
        /// </summary>
        public IPlayItem CurrentItem
        {
            get { return _currentItem; }
        }
        /// <summary>
        /// Returns the item at the specified index.
        /// </summary>
        /// <param name="index">Item index.</param>
        /// <returns>Item at the specified index in list.</returns>
        public IPlayItem GetItem(int index)
        {
            lock (this.SyncRoot)
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
        }
        /// <summary>
        /// Gets whether the playlist has more items after the currently playing one.
        /// </summary>
        public bool HasMoreItems
        {
            get
            {
                lock (this.SyncRoot)
                {
                    int nextItem = _currentItemIndex + 1;
                    if (nextItem >= _items.Count && !_isRepeat)
                        return false;
                    else
                        return true;
                }
            }
        }
        /// <summary>
        /// Go for the previous played item.
        /// </summary>
        public void PreviousItem()
        {
            lock (this.SyncRoot)
            {
                Stop();
                MoveToPrevious();
                if (_currentItemIndex == -1)
                    return;
                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                Play(item);
            }
        }
        /// <summary>
        /// Go for next item decided by controller logic.
        /// </summary>
        public void NextItem()
        {
            lock (this.SyncRoot)
            {
                Stop();
                MoveToNext();
                if (_currentItemIndex == -1)
                    return;
                IPlayItem item = _items[_currentItemIndex] as IPlayItem;
                Play(item);
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
                Play(item);
            }
        }
        /// <summary>
        /// Gets or sets whether items are randomly played.
        /// </summary>
        public bool IsRandom
        {
            get { return _isRandom; }
            set { _isRandom = value; }
        }
        /// <summary>
        /// Gets or sets whether rewind the list.
        /// </summary>
        public bool IsRewind
        {
            get { return _isRewind; }
            set { _isRewind = value; }
        }
        /// <summary>
        /// Gets or sets whether repeat playing an item.
        /// </summary>
        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { _isRepeat = value; }
        }
        /// <summary>
        /// Sets list controller.
        /// </summary>
        /// <param name="controller">Playlist controller.</param>
        public void SetPlaylistController(IPlaylistController controller)
        {
            _controller = controller;
        }

        #endregion

        #region IBroadcastStream Members

        /// <summary>
        /// Saves the broadcast stream as a file. 
        /// </summary>
        /// <param name="name">The path of the file relative to the scope.</param>
        /// <param name="isAppend">Whether to append to the end of file.</param>
        public void SaveAs(string name, bool isAppend)
        {
            try
            {
                IScope scope = this.Scope;
                IStreamFilenameGenerator generator = ScopeUtils.GetScopeService(scope, typeof(IStreamFilenameGenerator)) as IStreamFilenameGenerator;
                string filename = generator.GenerateFilename(scope, name, ".flv", GenerationType.RECORD);
                // Get file for that filename
                FileInfo file;
                if (generator.ResolvesToAbsolutePath)
                    file = new FileInfo(filename);
                else
                    file = scope.Context.GetResource(filename).File;

                if (!isAppend)
                {
                    if (file.Exists)
                    {
                        // Per livedoc of FCS/FMS:
                        // When "live" or "record" is used,
                        // any previously recorded stream with the same stream URI is deleted.
                        file.Delete();
                    }
                }
                else
                {
                    if (!file.Exists)
                    {
                        // Per livedoc of FCS/FMS:
                        // If a recorded stream at the same URI does not already exist,
                        // "append" creates the stream as though "record" was passed.
                        isAppend = false;
                    }
                }

                if (!file.Exists)
                {
                    // Make sure the destination directory exists
                    string directory = Path.GetDirectoryName(file.FullName);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                if (!file.Exists)
                {
                    using (FileStream fs = file.Create()) { }
                }

                FileConsumer fc = new FileConsumer(scope, file);
#if !(NET_1_1)
                Dictionary<string, object> parameterMap = new Dictionary<string, object>();
#else
            Hashtable parameterMap = new Hashtable();
#endif
                if (isAppend)
                {
                    parameterMap.Add("mode", "append");
                }
                else
                {
                    parameterMap.Add("mode", "record");
                }
                if (null == _recordPipe)
                {
                    _recordPipe = new InMemoryPushPushPipe();
                }
                _recordPipe.Subscribe(fc, parameterMap);
                _recordingFilename = filename;
            }
            catch (IOException ex)
            {
                log.Error("Save as exception", ex);
            }
        }
        /// <summary>
        /// Gets the filename the stream is being saved as.
        /// </summary>
        /// <value>The filename relative to the scope or null if the stream is not being saved.</value>
        public string SaveFilename
        {
            get { return _recordingFilename; }
        }
        /// <summary>
        /// Gets or sets stream publish name. Publish name is the value of the first parameter
        /// had been passed to <c>NetStream.publish</c> on client side in SWF.
        /// </summary>
        public string PublishedName
        {
            get
            {
                return _publishedName;
            }
            set
            {
                _publishedName = value;
            }
        }
        /// <summary>
        /// Gets the provider corresponding to this stream.
        /// </summary>
        public IProvider Provider
        {
            get { return this; }
        }

        #endregion

        #region IMessageComponent Members

        /// <summary>
        /// Handles out-of-band control message.
        /// </summary>
        /// <param name="source">Message component source.</param>
        /// <param name="pipe">Connection pipe.</param>
        /// <param name="oobCtrlMsg">Out-of-band control message</param>
        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
        }

        #endregion

        #region IPushableConsumer Members

        /// <summary>
        /// Pushes message through pipe.
        /// </summary>
        /// <param name="pipe">Pipe.</param>
        /// <param name="message">Message.</param>
        public void PushMessage(IPipe pipe, IMessage message)
        {
            PushMessage(message);
        }

        private void PushMessage(IMessage message)
        {
            _msgOut.PushMessage(message);
            _recordPipe.PushMessage(message);

            // Notify listeners about received packet
            if (message is RtmpMessage)
            {
                IRtmpEvent rtmpEvent = ((RtmpMessage)message).body;
                if (rtmpEvent is IStreamPacket)
                {
                    foreach (IStreamListener listener in GetStreamListeners())
                    {
                        try
                        {
                            listener.PacketReceived(this, (IStreamPacket)rtmpEvent);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error while notifying listener " + listener, ex);
                        }
                    }
                }
            }
        }
        #endregion

        #region IPipeConnectionListener Members

        /// <summary>
        /// Pipe connection event handler. There are two types of pipe connection events so far,
        /// provider push connection event and provider disconnection event.
        /// </summary>
        /// <param name="evt"></param>
        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.PROVIDER_CONNECT_PUSH:
                    if (evt.Provider == this && (evt.ParameterMap == null || !evt.ParameterMap.ContainsKey("record")))
                    {
                        _msgOut = (IMessageOutput)evt.Source;
                    }
                    break;
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    if (_msgOut == evt.Source)
                        _msgOut = null;
                    break;
                default:
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Add a listener to be notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddStreamListener(IStreamListener listener)
        {
            _listeners.Add(listener);
        }
        /// <summary>
        /// Returns registered stream listeners.
        /// </summary>
        /// <returns>Collection of stream listeners.</returns>
        public ICollection GetStreamListeners()
        {
            return _listeners;
        }
        /// <summary>
        /// Remove a listener from being notified about received packets.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveStreamListener(IStreamListener listener)
        {
            _listeners.Remove(listener);
        }

        /// <summary>
        /// Move to the next item updating the currentItemIndex.
        /// </summary>
        protected void MoveToNext()
        {
            if (_currentItemIndex >= _items.Count)
                _currentItemIndex = _items.Count - 1;
            if (_controller != null)
                _currentItemIndex = _controller.NextItem(this, _currentItemIndex);
            else
                _currentItemIndex = _defaultController.NextItem(this, _currentItemIndex);
        }

        /// <summary>
        /// Move to the previous item updating the currentItemIndex.
        /// </summary>
        protected void MoveToPrevious()
        {
            if (_currentItemIndex >= _items.Count)
                _currentItemIndex = _items.Count - 1;
            if (_controller != null)
                _currentItemIndex = _controller.PreviousItem(this, _currentItemIndex);
            else
                _currentItemIndex = _defaultController.PreviousItem(this, _currentItemIndex);
        }

        /// <summary>
        /// Play next item on item end
        /// </summary>
        protected void OnItemEnd()
        {
            NextItem();
        }

        internal class PlayItemScheduledJob : ScheduledJobBase
        {
            ServerStream _serverStream;

            public PlayItemScheduledJob(ServerStream serverStream)
            {
                _serverStream = serverStream;
            }

            public override void Execute(ScheduledJobContext context)
            {
                lock (_serverStream.SyncRoot)
                {
                    if (_serverStream._liveJobName == null)
                        return;
                    _serverStream._liveJobName = null;
                    _serverStream.OnItemEnd();
                }
            }
        }

        /// <summary>
        /// Play a specific IPlayItem.
        /// The strategy for now is VOD first, Live second.
        /// </summary>
        /// <param name="item">Item to play</param>
        protected void Play(IPlayItem item)
        {
            // Return if already playing
            if (_state != State.STOPPED)
                return;
            // Assume this is not live stream
            bool isLive = false;
            IProviderService providerService = ScopeUtils.GetScopeService(this.Scope, typeof(IProviderService)) as IProviderService;
            _msgIn = providerService.GetVODProviderInput(this.Scope, item.Name);
            if (_msgIn == null)
            {
                _msgIn = providerService.GetLiveProviderInput(this.Scope, item.Name, true);
                isLive = true;
            }
            if (_msgIn == null)
            {
                log.Warn("Can't get both VOD and Live input from providerService");
                return;
            }
            _state = State.PLAYING;
            _currentItem = item;
            SendResetMessage();
            _msgIn.Subscribe(this, null);
            if (isLive)
            {
                if (item.Length >= 0)
                {
                    PlayItemScheduledJob job = new PlayItemScheduledJob(this);
                    _liveJobName = _schedulingService.AddScheduledOnceJob(item.Length, job);
                }
            }
            else
            {
                long start = item.Start;
                if (start < 0)
                    start = 0;
                SendVODInitCM(_msgIn, (int)start);
                StartBroadcastVOD();
            }
        }

        private void SendResetMessage()
        {
            try
            {
                PushMessage(new ResetMessage());
            }
            catch (IOException ex)
            {
                log.Error("Error while sending reset message.", ex);
            }
        }

        /// <summary>
        /// Send VOD init control message
        /// </summary>
        /// <param name="msgIn"></param>
        /// <param name="start"></param>
        private void SendVODInitCM(IMessageInput msgIn, int start)
        {
            OOBControlMessage oobCtrlMsg = new OOBControlMessage();
            oobCtrlMsg.Target = typeof(IPassive).Name;
            oobCtrlMsg.ServiceName = "init";
            oobCtrlMsg.ServiceParameterMap.Add("startTS", start);
            _msgIn.SendOOBControlMessage(this, oobCtrlMsg);
        }
        
        /// <summary>
        /// Send VOD seek control message
        /// </summary>
        /// <param name="msgIn">Message input</param>
        /// <param name="position">New timestamp to play from</param>
        private void SendVODSeekCM(IMessageInput msgIn, int position)
        {
            OOBControlMessage oobCtrlMsg = new OOBControlMessage();
            oobCtrlMsg.Target = typeof(ISeekableProvider).Name;
            oobCtrlMsg.ServiceName = "seek";
            oobCtrlMsg.ServiceParameterMap.Add("position", position);
            msgIn.SendOOBControlMessage(this, oobCtrlMsg);

            lock (this.SyncRoot)
            {
                // Reset properties
                _vodStartTS = 0;
                _serverStartTS = System.Environment.TickCount;
                if (_nextRTMPMessage != null)
                {
                    try
                    {
                        PushMessage(_nextRTMPMessage);
                    }
                    catch (IOException ex)
                    {
                        log.Error("Error while sending message.", ex);
                    }
                    _nextRTMPMessage = null;
                }
                ResetMessage reset = new ResetMessage();
                try
                {
                    PushMessage(reset);
                }
                catch (IOException ex)
                {
                    log.Error("Error while sending message.", ex);
                }
                ScheduleNextMessage();
            }
        }

        private void StartBroadcastVOD()
        {
            _nextRTMPMessage = null;
            _vodStartTS = 0;
            _serverStartTS = System.Environment.TickCount;
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                if (_recordingFilename != null)
                    handler.StreamRecordStart(this);
                else
                    handler.StreamPublishStart(this);
            }
            NotifyBroadcastStart();
            ScheduleNextMessage();
        }

        /// <summary>
        /// Notifies handler on stream broadcast start
        /// </summary>
        protected void NotifyBroadcastStart()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamBroadcastStart(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamBroadcastStart", ex);
                }
            }
        }

        /// <summary>
        /// Notifies handler on stream broadcast stop
        /// </summary>
        protected void NotifyBroadcastClose()
        {
            IStreamAwareScopeHandler handler = GetStreamAwareHandler();
            if (handler != null)
            {
                try
                {
                    handler.StreamBroadcastClose(this);
                }
                catch (Exception ex)
                {
                    log.Error("Error notify streamBroadcastStop", ex);
                }
            }
        }

        internal class VODScheduledJob : ScheduledJobBase
        {
            ServerStream _serverStream;

            public VODScheduledJob(ServerStream serverStream)
            {
                _serverStream = serverStream;
            }

            public override void Execute(ScheduledJobContext context)
            {
                lock (_serverStream.SyncRoot)
                {
					if (_serverStream._vodJobName == null)
						return;
					_serverStream._vodJobName = null;
					if (!_serverStream.DoPushMessage()) {
						return;
					}
					if (_serverStream._state == State.PLAYING) {
						_serverStream.ScheduleNextMessage();
					} else {
						// Stream is paused, don't load more messages
						_serverStream._nextRTMPMessage = null;
					}
                }
            }
        }

        /// <summary>
        /// Pull the next message from IMessageInput and schedule it for push according to the timestamp.
        /// </summary>
        protected void ScheduleNextMessage()
        {
            bool first = _nextRTMPMessage == null;
            long delta;

            while (true)
            {
                _nextRTMPMessage = GetNextRTMPMessage();
                if (_nextRTMPMessage == null)
                {
                    OnItemEnd();
                    return;
                }

                IRtmpEvent rtmpEvent = _nextRTMPMessage.body;
                // filter all non-AV messages
                if (!(rtmpEvent is VideoData)
                        && !(rtmpEvent is AudioData))
                {
                    continue;
                }
                rtmpEvent = _nextRTMPMessage.body;
                _nextTS = rtmpEvent.Timestamp;
                if (first)
                {
                    _vodStartTS = _nextTS;
                    first = false;
                }

                delta = _nextTS - _vodStartTS - (System.Environment.TickCount - _serverStartTS);
                if (delta < WAIT_THRESHOLD)
                {
                    if (!DoPushMessage())
                    {
                        return;
                    }
                    if (_state != State.PLAYING)
                    {
                        // Stream is paused, don't load more messages
                        _nextRTMPMessage = null;
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
            VODScheduledJob job = new VODScheduledJob(this);
            _vodJobName = _schedulingService.AddScheduledOnceJob(delta, job);
        }

        private bool DoPushMessage()
        {
            bool sent = false;
            long start = _currentItem.Start;
            if (start < 0)
                start = 0;
            if (_currentItem.Length >= 0 && _nextTS - start > _currentItem.Length)
            {
                OnItemEnd();
                return sent;
            }
            if (_nextRTMPMessage != null)
            {
                sent = true;
                try
                {
                    PushMessage(_nextRTMPMessage);
                }
                catch (IOException ex)
                {
                    log.Error("Error while sending message", ex);
                }
            }
            return sent;
        }

        /// <summary>
        /// Next RTMP message
        /// </summary>
        /// <returns></returns>
        private RtmpMessage GetNextRTMPMessage()
        {
            IMessage message;
            do
            {
                // Pull message from message input object...
                try
                {
                    message = _msgIn.PullMessage();
                }
                catch (IOException ex)
                {
                    log.Error("Error while pulling message", ex);
                    message = null;
                }
                // If message is null then return null
                if (message == null)
                    return null;
            } while (!(message is RtmpMessage));
            // Cast and return
            return message as RtmpMessage;
        }

        /// <summary>
        /// Starts the server-side stream.
        /// </summary>
        public override void Start()
        {
            if (_state != State.UNINIT)
                throw new NotSupportedException("State " + _state + " not valid to start");
            if (_items.Count == 0)
                throw new NotSupportedException("At least one item should be specified to start");
            if (_publishedName == null)
                throw new NotSupportedException("A published name is needed to start");
            // publish this server-side stream

            IProviderService providerService = this.Scope.GetService(typeof(IProviderService)) as IProviderService;
            providerService.RegisterBroadcastStream(this.Scope, _publishedName, this);
            _recordPipe = new InMemoryPushPushPipe();
#if !(NET_1_1)
            Dictionary<string, object> recordParamMap = new Dictionary<string, object>();
#else
            Hashtable recordParamMap = new Hashtable();
#endif
            recordParamMap.Add("record", null);
            _recordPipe.Subscribe((IProvider)this, recordParamMap);
            _recordingFilename = null;
            _schedulingService = this.Scope.GetService(typeof(ISchedulingService)) as ISchedulingService;
            _state = State.STOPPED;
            _currentItemIndex = -1;
            NextItem();
        }
        /// <summary>
        /// Stops the server-side stream.
        /// </summary>
        public override void Stop()
        {
            lock (this.SyncRoot)
            {
                if (_state != State.PLAYING && _state != State.PAUSED)
                    return;
                if (_liveJobName != null)
                {
                    _schedulingService.RemoveScheduledJob(_liveJobName);
                    _liveJobName = null;
                }
                if (_vodJobName != null)
                {
                    _schedulingService.RemoveScheduledJob(_vodJobName);
                    _vodJobName = null;
                }
                if (_msgIn != null)
                {
                    _msgIn.Unsubscribe(this);
                    _msgIn = null;
                }
                if (_nextRTMPMessage != null)
                {
                    _nextRTMPMessage = null;
                }
                _state = State.STOPPED;
            }
        }
        /// <summary>
        /// Closes the server-side stream.
        /// </summary>
        public override void Close()
        {
            lock (this.SyncRoot)
            {
                if (_state == State.PLAYING || _state == State.PAUSED)
                {
                    Stop();
                }
                if (_msgOut != null)
                {
                    _msgOut.Unsubscribe(this);
                }
                _recordPipe.Unsubscribe((IProvider)this);
                NotifyBroadcastClose();
                _state = State.CLOSED;
            }
        }
    }
}
