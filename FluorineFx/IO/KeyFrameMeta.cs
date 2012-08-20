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

namespace FluorineFx.IO
{
    /// <summary>
    /// Keyframe metadata.
    /// </summary>
    public class KeyFrameMeta
    {
        long _duration;
        /// <summary>
        /// Gets or sets duration in milliseconds.
        /// </summary>
        public long Duration
        {
          get { return _duration; }
          set { _duration = value; }
        }
        bool _audioOnly;
        /// <summary>
        /// Gets or sets whether only audio frames are present.
        /// </summary>
        public bool AudioOnly
        {
            get { return _audioOnly; }
            set { _audioOnly = value; }
        }
        int[] _timestamps;
        /// <summary>
        /// Gets or sets keyframe timestamps.
        /// </summary>
        public int[] Timestamps
        {
            get { return _timestamps; }
            set { _timestamps = value; }
        }
        long[] _positions;
        /// <summary>
        /// Gets or sets keyframe positions.
        /// </summary>
        public long[] Positions
        {
            get { return _positions; }
            set { _positions = value; }
        }
    }
}
