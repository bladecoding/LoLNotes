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
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.Event;

namespace FluorineFx.Net
{
    /// <summary>
    /// A NetStream object dispatches NetStreamAudioEventArgs objects when audio data is received.
    /// </summary>
    [CLSCompliant(false)]
    public class NetStreamAudioEventArgs : EventArgs
    {
        readonly AudioData _audioData;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetStreamAudioEventArgs"/> class.
        /// </summary>
        /// <param name="audioData">The audio data.</param>
        internal NetStreamAudioEventArgs(AudioData audioData)
        {
            _audioData = audioData;
        }

        /// <summary>
        /// Gets the audio data.
        /// </summary>
        /// <value>The audio data.</value>
        public AudioData AudioData
        {
            get { return _audioData; }
        } 
    }
}
