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


namespace FluorineFx.Messaging.Api.SO
{
	/// <summary>
    /// Notifications about shared object updates.
	/// </summary>
    [CLSCompliant(false)]
	public interface ISharedObjectListener
	{
        /// <summary>
        /// Called when a client connects to a shared object.
        /// </summary>
        /// <param name="so">The shared object.</param>
		void OnSharedObjectConnect(ISharedObject so);
        /// <summary>
        /// Called when a client disconnects from a shared object.
        /// </summary>
        /// <param name="so">The shared object.</param>
        void OnSharedObjectDisconnect(ISharedObject so);
        /// <summary>
        /// Called when a shared object attribute is updated.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="key">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
		void OnSharedObjectUpdate(ISharedObject so, string key, object value);
        /// <summary>
        /// Called when multiple attributes of a shared object are updated.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="values">The new attributes of the shared object.</param>
		void OnSharedObjectUpdate(ISharedObject so, IAttributeStore values);
#if !(NET_1_1)
        /// <summary>
        /// Called when multiple attributes of a shared object are updated.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="values">The new attributes of the shared object.</param>
        void OnSharedObjectUpdate(ISharedObject so, IDictionary<string, object> values);
#else
        /// <summary>
        /// Called when multiple attributes of a shared object are updated.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="values">The new attributes of the shared object.</param>
        void OnSharedObjectUpdate(ISharedObject so, IDictionary values);
#endif
        /// <summary>
        /// Called when an attribute is deleted from the shared object.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="key">The name of the attribute to delete.</param>
		void OnSharedObjectDelete(ISharedObject so, string key);
        /// <summary>
        /// Called when all attributes of a shared object are removed.
        /// </summary>
        /// <param name="so">The shared object.</param>
        void OnSharedObjectClear(ISharedObject so);
        /// <summary>
        /// Called when a shared object method call is sent.
        /// </summary>
        /// <param name="so">The shared object.</param>
        /// <param name="method">The method name to call.</param>
        /// <param name="parameters">The arguments.</param>
		void OnSharedObjectSend(ISharedObject so, string method, IList parameters);
	}
}
