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
using log4net;
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.IO.FLV;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Messaging;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Rtmp.Stream;
using FluorineFx.Messaging.Rtmp.Stream.Messages;
using FluorineFx.Messaging.Rtmp.IO;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Stream.Consumer
{
    class FileConsumer : IPushableConsumer, IPipeConnectionListener
    {
        private static ILog log = LogManager.GetLogger(typeof(FileConsumer));

        object _syncLock = new object();

        /// <summary>
        /// Scope
        /// </summary>
        private IScope _scope;
        /// <summary>
        /// File
        /// </summary>
        private FileInfo _file;
        /// <summary>
        /// Tag writer
        /// </summary>
        private ITagWriter _writer;
        /// <summary>
        /// Operation mode
        /// </summary>
        private string _mode;
        /// <summary>
        /// Offset
        /// </summary>
        private int _offset;
        /// <summary>
        /// Last write timestamp
        /// </summary>
        private int _lastTimestamp;
        /// <summary>
        /// Start timestamp
        /// </summary>
        private int _startTimestamp;

        /// <summary>
        /// Initializes a new instance of the FileConsumer class.
        /// </summary>
        /// <param name="scope">Scope of consumer.</param>
        /// <param name="file">File.</param>
        public FileConsumer(IScope scope, FileInfo file)
        {
            _scope = scope;
            _file = file;
            _offset = 0;
            _lastTimestamp = 0;
            _startTimestamp = -1;
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        #region IPushableConsumer Members

        /// <summary>
        /// Push message through pipe.
        /// Synchronize this method to avoid FLV corruption from abrupt disconnection.
        /// </summary>
        /// <param name="pipe">Pipe.</param>
        /// <param name="message">Message to push.</param>
        public void PushMessage(IPipe pipe, IMessage message)
        {
            lock (this.SyncRoot)
            {
                if (message is ResetMessage)
                {
                    _startTimestamp = -1;
                    _offset += _lastTimestamp;
                    return;
                }
                else if (message is StatusMessage)
                {
                    return;
                }
                if (!(message is RtmpMessage))
                    return;

                if (_writer == null)
                {
                    Init();
                }
                FluorineFx.Messaging.Rtmp.Stream.Messages.RtmpMessage rtmpMsg = message as FluorineFx.Messaging.Rtmp.Stream.Messages.RtmpMessage;
                IRtmpEvent msg = rtmpMsg.body;
                if (_startTimestamp == -1)
                {
                    _startTimestamp = msg.Timestamp;
                }
                int timestamp = msg.Timestamp - _startTimestamp;
                if (timestamp < 0)
                {
                    log.Warn("Skipping message with negative timestamp.");
                    return;
                }
                _lastTimestamp = timestamp;

                ITag tag = new Tag();

                tag.DataType = (byte)msg.DataType;
                tag.Timestamp = timestamp + _offset;
                if (msg is IStreamData)
                {
                    ByteBuffer data = (msg as IStreamData).Data;
                    tag.Body = data.ToArray();
                }

                try
                {
                    _writer.WriteTag(tag);
                }
                catch (IOException ex)
                {
                    log.Error("Error writing tag", ex);
                }
            }
        }

        #endregion

        #region IMessageComponent Members

        /// <summary>
        /// Out-of-band control message handler.
        /// </summary>
        /// <param name="source">Source of message.</param>
        /// <param name="pipe">Pipe that is used to transmit OOB message.</param>
        /// <param name="oobCtrlMsg">OOB control message.</param>
        public void OnOOBControlMessage(IMessageComponent source, IPipe pipe, OOBControlMessage oobCtrlMsg)
        {
        }

        #endregion

        #region IPipeConnectionListener Members

        /// <summary>
        /// Pipe connection event handler.
        /// </summary>
        /// <param name="evt">Pipe connection event.</param>
        public void OnPipeConnectionEvent(PipeConnectionEvent evt)
        {
            switch (evt.Type)
            {
                case PipeConnectionEvent.CONSUMER_CONNECT_PUSH:
                    if (evt.Consumer != this)
                    {
                        break;
                    }
                    IDictionary paramMap = evt.ParameterMap as IDictionary;
                    if (paramMap != null)
                    {
                        _mode = paramMap["mode"] as string;
                    }
                    break;
                case PipeConnectionEvent.CONSUMER_DISCONNECT:
                    if (evt.Consumer == this)
                    {
                    }
                    break;
                case PipeConnectionEvent.PROVIDER_DISCONNECT:
                    // we only support one provider at a time
                    // so do releasing when provider disconnects
                    Uninit();
                    break;
                default:
                    break;
            }
        }

        #endregion

        private void Init()
        {
            IStreamableFileFactory factory = ScopeUtils.GetScopeService(_scope, typeof(IStreamableFileFactory)) as IStreamableFileFactory;
            string directory = Path.GetDirectoryName(_file.FullName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (!_file.Exists)
            {
                // Maybe the (previously existing) file has been deleted
                using( FileStream fs = _file.Create()){}
            }
			/*
            else if (_file.IsReadOnly)
            {
                throw new IOException("The file is read-only");
            }
			*/
            IStreamableFileService service = factory.GetService(_file);
            IStreamableFile flv = service.GetStreamableFile(_file);
            if (_mode == null || _mode.Equals(Constants.ClientStreamModeRecord))
            {
                _writer = flv.GetWriter();
            }
            else if (_mode.Equals(Constants.ClientStreamModeAppend))
            {
                _writer = flv.GetAppendWriter();
            }
            else
            {
                throw new NotSupportedException("Illegal mode type: " + _mode);
            }
        }

        /// <summary>
        /// Reset
        /// </summary>
        private void Uninit()
        {
            lock (this.SyncRoot)
            {
                if (_writer != null)
                {
                    _writer.Close();
                    _writer = null;
                }
            }
        }
    }
}
