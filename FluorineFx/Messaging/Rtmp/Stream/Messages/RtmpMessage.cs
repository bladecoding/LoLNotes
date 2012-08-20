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
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;

namespace FluorineFx.Messaging.Rtmp.Stream.Messages
{
    /// <summary>
    /// RTMP message
    /// </summary>
    [CLSCompliant(false)]
    public class RtmpMessage : AsyncMessage
    {
        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        /// <remarks>The body is the data that is delivered to the remote destination.</remarks>
        public new IRtmpEvent body
        {
            get { return _body as IRtmpEvent; }
            set { _body = value; }
        }

        protected override MessageBase CopyImpl(MessageBase clone)
        {
            // Instantiate the clone, if a derived type hasn't already.
            if (clone == null) clone = new RtmpMessage();
            return base.CopyImpl(clone);
        }
    }
}
