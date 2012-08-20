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
using System.Collections.Generic;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.IO.FLV;

namespace FluorineFx.IO.Mp4
{
    /// <summary>
    /// Implements the MP4 api
    /// </summary>
    class Mp4 : IMp4
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(Mp4));
#endif

        private FileInfo _file;
        private MetaService _metaService;
        private MetaData _metaData;

        public Mp4(FileInfo file)
        {
            _file = file;
	    }

        #region IMp4 Members

        public bool HasMetaData
        {
            get { return _metaData != null; }
        }

        public MetaData MetaData
        {
            get
            {
                _metaService.Input = new FileStream(_file.FullName, FileMode.Open);
                return null;
            }
            set
            {
                string tmpFileName = Path.GetTempFileName();
                FileInfo tmpFile = new FileInfo(tmpFileName);
                if (_metaService == null)
                    _metaService = new MetaService(tmpFile);
                _metaService.Input = new FileStream(_file.FullName, FileMode.Open);
                _metaService.Output = new FileStream(tmpFile.FullName, FileMode.Create);
                _metaService.Write(value);
                _metaData = value;
                _file.Delete();
                try
                {
                    tmpFile.MoveTo(_file.FullName);
                }
                catch
                {
                    // Probably renaming across filesystems? Move manually.
                    FileStream fis = new FileStream(tmpFile.FullName, FileMode.Open);
                    FileStream fos = new FileStream(_file.FullName, FileMode.Create);
                    byte[] buf = new byte[16384];
                    int i = 0;
                    while ((i = fis.Read(buf, 0, buf.Length)) != -1)
                    {
                        fos.Write(buf, 0, i);
                    }
                    fis.Close();
                    fos.Close();
                    tmpFile.Delete();
                }
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

        public bool HasKeyFrameData
        {
            get { return false; }
        }

        public Dictionary<string, object> KeyFrameData
        {
            get
            {
                return null;
            }
            set
            {
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
            Mp4Reader reader = null;
            string fileName = _file.Name;
            if (_file.Exists)
            {
#if !SILVERLIGHT
                if (log.IsDebugEnabled)
                    log.Debug("File size: " + _file.Length);
#endif
                reader = new Mp4Reader(_file);
            }
            else
            {
#if !SILVERLIGHT
                log.Info("Creating new file: " + fileName);
#endif
                using (FileStream fs = _file.Create()) { }
                _file.Refresh();
                reader = new Mp4Reader(_file);
            }
            return reader;            
        }

        public ITagWriter GetWriter()
        {
            return null;
        }

        public ITagWriter GetAppendWriter()
        {
            return null;
        }

        #endregion
    }
}
