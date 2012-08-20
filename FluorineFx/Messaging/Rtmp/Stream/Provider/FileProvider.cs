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
using log4net;
using FluorineFx.IO;
using FluorineFx.IO.FLV;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.IO;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Stream.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream.Provider
{
    /// <summary>
    /// Pullable provider for files.
    /// </summary>
    class FileProvider : IPassive, ISeekableProvider, IPullableProvider, IPipeConnectionListener, IStreamTypeAwareProvider 
    {
        private static ILog log = LogManager.GetLogger(typeof(FileProvider));
        object _syncLock = new object();

        /// <summary>
        /// Provider scope.
        /// </summary>
        private IScope _scope;
        /// <summary>
        /// Source file.
        /// </summary>
        private FileInfo _file;
        /// <summary>
        /// Consumer pipe.
        /// </summary>
        private IPipe _pipe;
        /// <summary>
        /// Tag reader.
        /// </summary>
        private ITagReader _reader;
        /// <summary>
        /// Keyframe metadata.
        /// </summary>
        private KeyFrameMeta _keyFrameMeta;
        /// <summary>
        /// Position at start.
        /// </summary>
        private int _start;

        /// <summary>
        /// Initializes a new instance of the FileProvider class for specific file and scope.
        /// </summary>
        /// <param name="scope">Scope.</param>
        /// <param name="file">File.</param>
        public FileProvider(IScope scope, FileInfo file)
        {
            _scope = scope;
            _file = file;
        }
        /// <summary>
        /// Gets or sets the start position.
        /// </summary>
        public int Start
        {
            get { return _start; }
            set { _start = value; }
        }

        #region ISeekableProvider Members

        public int Seek(int ts)
        {
            lock (_syncLock)
            {
		        if (_keyFrameMeta == null) 
                {
                    if (!(_reader is IKeyFrameDataAnalyzer)) 
                    {
				        // Seeking not supported
				        return ts;
			        }
                    _keyFrameMeta = (_reader as IKeyFrameDataAnalyzer).AnalyzeKeyFrames();
		        }

                if (_keyFrameMeta.Positions.Length == 0)
                {
			        // no video keyframe metainfo, it's an audio-only FLV
			        // we skip the seek for now.
			        // TODO add audio-seek capability
			        return ts;
		        }
                if (ts >= _keyFrameMeta.Duration)
                {
			        // Seek at or beyond EOF
			        _reader.Position = long.MaxValue;
			        return (int) _keyFrameMeta.Duration;
		        }
		        int frame = 0;
		        for (int i = 0; i < _keyFrameMeta.Positions.Length; i++) 
                {
			        if (_keyFrameMeta.Timestamps[i] > ts) {
				        break;
			        }
			        frame = i;
		        }
		        _reader.Position = _keyFrameMeta.Positions[frame];
		        return _keyFrameMeta.Timestamps[frame];
            }
        }

        #endregion

        #region IMessageComponent Members

        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
            if(typeof(IPassive).Name.Equals(oobCtrlMsg.Target))
            {
                if (oobCtrlMsg.ServiceName.Equals("init"))
                {
                    _start = System.Convert.ToInt32(oobCtrlMsg.ServiceParameterMap["startTS"]);
                }
            }
            else if (typeof(ISeekableProvider).Name.Equals(oobCtrlMsg.Target))
            {
                if (oobCtrlMsg.ServiceName.Equals("seek"))
                {
                    int position = System.Convert.ToInt32(oobCtrlMsg.ServiceParameterMap["position"]);
                    int seekPos = Seek(position);
                    // Return position we seeked to
                    oobCtrlMsg.Result = seekPos;
                }
            }
            else if (typeof(IStreamTypeAwareProvider).Name.Equals(oobCtrlMsg.Target))
            {
                if (oobCtrlMsg.ServiceName.Equals("hasVideo"))
                {
                    oobCtrlMsg.Result = this.HasVideo();
                }
            }
        }

        #endregion

        #region IPullableProvider Members

        public IMessage PullMessage(IPipe pipe)
        {
            lock (_syncLock)
            {
                if (_pipe != pipe)
                    return null;
                if (_reader == null)
                    Init();
                if (!_reader.HasMoreTags())
                {
                    // TODO send OOBCM to notify EOF
                    // Do not unsubscribe as this kills VOD seek while in buffer
                    // this.pipe.unsubscribe(this);
                    return null;
                }
                ITag tag = _reader.ReadTag();
                IRtmpEvent msg = null;
                int timestamp = tag.Timestamp;
                switch (tag.DataType)
                {
                    case Constants.TypeAudioData:
                        msg = new AudioData(tag.Body);
                        break;
                    case Constants.TypeVideoData:
                        msg = new VideoData(tag.Body);
                        break;
                    case Constants.TypeInvoke:
                        msg = new Invoke(tag.Body);
                        break;
                    case Constants.TypeNotify:
                        msg = new Notify(tag.Body);
                        break;
                    case Constants.TypeFlexStreamEnd:
                        msg = new FlexStreamSend(tag.Body);
                        break;
                    default:
                        log.Warn("Unexpected type " + tag.DataType);
                        msg = new Unknown(tag.DataType, tag.Body);
                        break;
                }
                msg.Timestamp = timestamp;
                RtmpMessage rtmpMsg = new RtmpMessage();
                rtmpMsg.body = msg;
                return rtmpMsg;
            }
        }

        public IMessage PullMessage(IPipe pipe, long wait)
        {
            return PullMessage(pipe);
        }

        #endregion

        #region IPipeConnectionListener Members

        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
		    switch (evt.Type) {
			    case PipeConnectionEvent.PROVIDER_CONNECT_PULL:
				    if (_pipe == null)
					    _pipe = evt.Source as IPipe;
				    break;
			    case PipeConnectionEvent.PROVIDER_DISCONNECT:
				    if (_pipe == evt.Source)
                    {
					    _pipe = null;
					    Uninit();
				    }
				    break;
			    case PipeConnectionEvent.CONSUMER_DISCONNECT:
                    if (_pipe == evt.Source)
                        Uninit();
                    break;
			    default:
				    break;
		    }
        }

        #endregion

        #region IStreamTypeAwareProvider Members

        public bool HasVideo()
        {
            return (_reader != null && _reader.HasVideo());
        }

        #endregion

        /// <summary>
        /// Initializes file provider. Creates streamable file factory and service, seeks to start position.
        /// </summary>
        private void Init()
        {
            IStreamableFileFactory factory = (IStreamableFileFactory)ScopeUtils.GetScopeService(_scope, typeof(IStreamableFileFactory));
            IStreamableFileService service = factory.GetService(_file);
            if (service == null)
            {
                log.Error("No service found for " + _file.FullName);
                return;
            }
            IStreamableFile streamFile = service.GetStreamableFile(_file);
            _reader = streamFile.GetReader();
            if (_start > 0)
                Seek(_start);
        }

        /// <summary>
        /// Reset.
        /// </summary>
        private void Uninit() 
        {
            lock (_syncLock)
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }
            }
	    }
    }
}
