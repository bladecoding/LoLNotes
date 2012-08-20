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

namespace FluorineFx.Messaging.Api.Event
{
	/// <summary>
    /// Provides an Observer pattern, that is it has a list of objects that listen to events.
	/// </summary>
	public interface IEventObservable
	{
        /// <summary>
        /// Add event listener to this observable.
        /// </summary>
        /// <param name="listener">Event listener.</param>
		void AddEventListener(IEventListener listener);
        /// <summary>
        /// Remove event listener from this observable.
        /// </summary>
        /// <param name="listener">Event listener.</param>
		void RemoveEventListener(IEventListener listener);
        /// <summary>
        /// Get the event listeners collection.
        /// </summary>
        /// <returns>Collection of event listeners.</returns>
		ICollection GetEventListeners();
	}
}
