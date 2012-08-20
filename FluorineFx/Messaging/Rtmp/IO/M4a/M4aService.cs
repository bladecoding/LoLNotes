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
using FluorineFx.IO.M4a;

namespace FluorineFx.Messaging.Rtmp.IO.M4a
{
    class M4aService : BaseStreamableFileService, IM4aService
    {
        private AMFWriter _amfWriter;

        public override string Prefix
        {
            get { return "f4a"; }
        }

        /// <summary>
        /// Gets the file extensions handled by this service.
        /// </summary>
        /// <value>The extensions.</value>
        public override string Extension
        {
            get { return ".f4a,.m4a,.aac"; }
        }

        public override IStreamableFile GetStreamableFile(FileInfo file)
        {
            return new FluorineFx.IO.M4a.M4a(file);
        }

        #region IM4aService Members

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
    }
}
