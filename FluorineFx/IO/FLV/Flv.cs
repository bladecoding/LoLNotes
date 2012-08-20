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
using System.Collections.Generic;
using System.IO;
//using System.Web;
//using System.Web.Caching;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;

namespace FluorineFx.IO.FLV
{
    class Flv : IFlv
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(Flv));
#endif
        private FileInfo _file;
        private bool _generateMetadata;
        private MetaService _metaService;
        private MetaData _metaData;

        public Flv(FileInfo file)
            : this(file, false)
        {
	    }

        public Flv(FileInfo file, bool generateMetadata)
        {
            _file = file;
            _generateMetadata = generateMetadata;
            int count = 0;

            if (!_generateMetadata)
            {
                try
                {
                    FlvReader reader = new FlvReader(_file);
                    ITag tag = null;
                    while (reader.HasMoreTags() && (++count < 5))
                    {
                        tag = reader.ReadTag();
                        if (tag.DataType == IOConstants.TYPE_METADATA)
                        {
                            if (_metaService == null) _metaService = new MetaService(_file);
                            _metaData = _metaService.ReadMetaData(tag.Body);
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
#if !SILVERLIGHT
                    log.Error("An error occured looking for metadata:", ex);
#endif
                }
            }
        }

        #region IFlv Members

        public bool HasMetaData
        {
            get { return _metaData != null; }
        }

        public bool HasKeyFrameData
        {
            get { return false; }
        }

        public MetaData MetaData
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public MetaService MetaService
        {
            get
            {
                return _metaService;
            }
            set
            {
                _metaService = value;
            }
        }

        public Dictionary<string, object> KeyFrameData
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RefreshHeaders()
        {
        }

        public void FlushHeaders()
        {
        }

        public ITagReader ReaderFromNearestKeyFrame(int seekPoint)
        {
            return null;
        }

        public ITagWriter WriterFromNearestKeyFrame(int seekPoint)
        {
            return null;
        }

        #endregion

        #region IStreamableFile Members

        public ITagReader GetReader()
        {
            FlvReader reader = null;
            string fileName = _file.Name;

            if (_file.Exists)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug("File size: " + _file.Length);
#endif
                reader = new FlvReader(_file, _generateMetadata);
            }
            else
            {
#if !SILVERLIGHT
                log.Info("Creating new file: " + fileName);
#endif
                using (FileStream fs = _file.Create()){}
                _file.Refresh();
                reader = new FlvReader(_file, _generateMetadata);
            }
            return reader;            
        }

        public ITagWriter GetWriter()
        {
            if (_file.Exists)
                _file.Delete();
            FileStream stream = _file.Create();
            ITagWriter writer = new FlvWriter(stream, false);
            writer.WriteHeader();
            return writer;
        }

        public ITagWriter GetAppendWriter()
        {
            // If the file doesnt exist, we cant append to it, so return a writer
            if (!_file.Exists)
            {
#if !SILVERLIGHT
                log.Info("File does not exist, calling writer. This will create a new file.");
#endif
                return GetWriter();
            }
            //FileStream stream = _file.Open(FileMode.Append);
            FileStream stream = new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 65536);
            ITagWriter writer = new FlvWriter(stream, true);
            return writer;
        }

        #endregion
    }
}
