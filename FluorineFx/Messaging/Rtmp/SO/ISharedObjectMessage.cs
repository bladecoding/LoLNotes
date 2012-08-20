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
using System.Collections;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Collections.Generic;

namespace FluorineFx.Messaging.Rtmp.SO
{
	/// <summary>
    /// Shared object message.
	/// </summary>
	interface ISharedObjectMessage : IRtmpEvent
	{
		/// <summary>
		/// Gets the name of the shared object this message belongs to.
		/// </summary>
		string Name{ get; }
		/// <summary>
		/// Returns the version to modify.
		/// </summary>
		int Version{ get; }
		/// <summary>
        /// Gets a value indicating whether the message affects a persistent shared object.
		/// </summary>
		bool IsPersistent{ get; }
        /// <summary>
        /// Returns a set of ISharedObjectEvent objects containing informations what to change.
        /// </summary>
        IQueue<ISharedObjectEvent> Events { get; }
        /// <summary>
        /// Add a shared object event.
        /// </summary>
        /// <param name="type">Event type.</param>
        /// <param name="key">Handler key.</param>
        /// <param name="value">Event value.</param>
        void AddEvent(SharedObjectEventType type, string key, object value);
        /// <summary>
        /// Add a shared object event.
        /// </summary>
        /// <param name="sharedObjectEvent">Shared object event.</param>
		void AddEvent(ISharedObjectEvent sharedObjectEvent);
        /// <summary>
        /// Clear shared object.
        /// </summary>
		void Clear();
        /// <summary>
        /// Gets a value indicating whether the shared object is empty.
        /// </summary>
		bool IsEmpty{ get; }
	}
}
