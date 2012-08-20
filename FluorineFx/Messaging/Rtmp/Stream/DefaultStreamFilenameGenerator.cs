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
using System.Text;
using System.Collections;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Rtmp.Persistence;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// Default filename generator for streams. The files will be stored in a
    /// directory "streams" in the application folder.
    /// </summary>
    class DefaultStreamFilenameGenerator : IStreamFilenameGenerator
    {
        /// <summary>
        /// Generate stream directory based on relative scope path. The base directory is
        /// <code>streams</code>, e.g. a scope <code>/application/one/two</code> will
        /// generate a directory <code>/streams/one/two</code> inside the application.
        /// </summary>
        /// <param name="scope">Scope.</param>
        /// <returns>Directory based on relative scope path.</returns>
        private string GetStreamDirectory(IScope scope) 
        {
            return PersistenceUtils.GetPath(scope, "streams");
	    }

        #region IStreamFilenameGenerator Members

        public void Start(ConfigurationSection configuration)
        {
        }

        public void Shutdown()
        {
        }

        public string GenerateFilename(IScope scope, string name, GenerationType type)
        {
            return GenerateFilename(scope, name, null, type);
        }

        public string GenerateFilename(IScope scope, string name, string extension, GenerationType type)
        {
            string result = GetStreamDirectory(scope) + name;
            if(extension != null && !string.Empty.Equals(extension))
            {
                result += extension;
            }
            return result;
        }
        /// <summary>
        /// The default filenames are relative to the scope path, so always return <code>false</code>.
        /// </summary>
        public bool ResolvesToAbsolutePath
        {
            get { return false; }
        }

        #endregion
    }
}
