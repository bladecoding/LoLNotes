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
using FluorineFx.IO;
using FluorineFx.IO.Mp3;

namespace FluorineFx.Messaging.Rtmp.IO.Mp3
{
    /// <summary>
    /// Streamable file service extension for MP3.
    /// </summary>
    class Mp3Service : BaseStreamableFileService, IMp3Service
    {
        #region IStreamableFileService Members

        public override string Prefix
        {
            get { return "mp3"; }
        }

        public override string Extension
        {
            get { return ".mp3"; }
        }

        public override IStreamableFile GetStreamableFile(FileInfo file)
        {
            return new Mp3File(file);
        }

        #endregion
    }
}
