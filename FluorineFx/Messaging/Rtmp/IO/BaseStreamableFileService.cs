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

namespace FluorineFx.Messaging.Rtmp.IO
{
    /// <summary>
    /// Base class for streamable file services.
    /// </summary>
    abstract class BaseStreamableFileService : IStreamableFileService
    {
        #region IStreamableFileService Members

        public abstract string Prefix{ get; }

        public abstract string Extension{ get; }

        public string PrepareFilename(string name)
        {
            string prefix = this.Prefix + ':';
            if (name.StartsWith(prefix))
            {
                name = name.Substring(prefix.Length);
                if (name.LastIndexOf('.') != name.Length - 4)
                {
                    name = name + this.Extension.Split(new char[] { ',' })[0];
                }
            }
            return name;
        }

        public bool CanHandle(FileInfo file)
        {
            bool valid = false;
            if (file.Exists)
            {
                string[] extensions = this.Extension.Split(new char[] { ',' });
                foreach (string extension in extensions)
                {
                    if (extension == file.Extension)
                    {
                        valid = true;
                        break;
                    }
                }
            }
            return valid;
        }

        public abstract IStreamableFile GetStreamableFile(FileInfo file);

        #endregion
    }
}
