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
using FluorineFx.Util;
using FluorineFx.IO;
using FluorineFx.IO.FLV;

namespace FluorineFx.IO.Mp4
{
    /// <summary>
    /// Represents MP4 file
    /// </summary>
    [CLSCompliant(false)]
    public interface IMp4 : IStreamableFile
    {
        /// <summary>
        /// Returns a boolean stating whether the mp4 has metadata.
        /// </summary>
        /// <value><code>true</code> if file has injected metadata, <code>false</code> otherwise.</value>
        bool HasMetaData { get; }
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        MetaData MetaData { get; set; }
        /// <summary>
        /// Gets or sets the MetaService.
        /// </summary>
        MetaService MetaService { get; set; }
        /// <summary>
        /// Returns a boolean stating whether a mp4 has keyframedata.
        /// </summary>
        bool HasKeyFrameData { get; }
        /// <summary>
        /// Gets or sets the keyframe data.
        /// </summary>
        Dictionary<string, object> KeyFrameData { get; set; }
        /// <summary>
        /// Refreshes the headers. Usually used after data is added to the mp4 file.
        /// </summary>
        void RefreshHeaders();
        /// <summary>
        /// Flushes the headers.
        /// </summary>
        void FlushHeaders();
        /// <summary>
        /// Returns a reader closest to the nearest keyframe.
        /// </summary>
        /// <param name="seekPoint">Point in file we are seeking around.</param>
        /// <returns>Tag reader closest to the specified point.</returns>
        ITagReader ReaderFromNearestKeyFrame(int seekPoint);
        /// <summary>
        /// Returns a writer based on the nearest key frame.
        /// </summary>
        /// <param name="seekPoint">Point in file we are seeking around.</param>
        /// <returns>Tag writer closest to the specified point.</returns>
        ITagWriter WriterFromNearestKeyFrame(int seekPoint);
    }
}
