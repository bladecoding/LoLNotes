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
using FluorineFx.IO.FLV;

namespace FluorineFx.Messaging.Rtmp.IO.Flv
{
    /// <summary>
    /// Sets up the service and hands out FLV objects to its callers
    /// </summary>
    class FlvService : BaseStreamableFileService, IFlvService
    {
        private AMFWriter _amfWriter;
        private AMFReader _amfReader;

        /// <summary>
        /// Generate FLV metadata?
        /// </summary>
        private bool _generateMetadata;

        public bool GenerateMetadata
        {
            get { return _generateMetadata; }
            set { _generateMetadata = value; }
        }

        public override string Prefix
        {
            get { return "flv"; }
        }

        public override string Extension
        {
            get { return ".flv"; }
        }

        public override IStreamableFile GetStreamableFile(FileInfo file)
        {
            return new FluorineFx.IO.FLV.Flv(file, _generateMetadata);
        }

        #region IFlvService Members

        public AMFWriter Serializer
        {
            get
            {
                return _amfWriter;
            }
            set
            {
                _amfWriter = value;
            }
        }

        #endregion

        public AMFReader Deserializer
        {
            get { return _amfReader; }
            set { _amfReader = value; }
        }

    }
}
