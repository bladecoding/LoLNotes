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

namespace FluorineFx.Messaging.Messages
{
	/// <summary>
    /// Represents a Flex message object.
	/// </summary>
#if SILVERLIGHT
    public interface IMessage
#else
    public interface IMessage //: ICloneable
#endif
	{
        /// <summary>
        /// Gets or sets the client identity indicating which client sent the message.
        /// </summary>
		object clientId { get; set; }
        /// <summary>
        /// Gets or sets the message destination.
        /// </summary>
		string destination { get; set; }
        /// <summary>
        /// Gets or sets the identity of the message.
        /// </summary>
        /// <remarks>This field is unique and can be used to correlate a response to the original request message.</remarks>
		string messageId { get; set; }
        /// <summary>
        /// Gets or sets the time stamp for the message.
        /// </summary>
        /// <remarks>The time stamp is the date and time that the message was sent.</remarks>
		long timestamp { get; set; }
        /// <summary>
        /// Gets or sets the validity for the message.
        /// </summary>
        /// <remarks>Time to live is the number of milliseconds that this message remains valid starting from the specified timestamp value.</remarks>
		long timeToLive { get; set; }
        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        /// <remarks>The body is the data that is delivered to the remote destination.</remarks>
		object body { get; set; }
        /// <summary>
        /// Gets or sets the headers of the message.
        /// </summary>
        /// <remarks>
        /// The headers of a message are an associative array where the key is the header name and the value is the header value.
        /// This property provides access to the specialized meta information for the specific message instance. 
        /// Flex core header names begin with a 'DS' prefix. Custom header names should start with a unique prefix to avoid name collisions.
        /// </remarks>
#if !(NET_1_1)
        Dictionary<string, object> headers { get; set; }
#else
        Hashtable headers { get; set; }
#endif
        /// <summary>
        /// Retrieves the specified header value.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>The value associated with the specified header name.</returns>
		object GetHeader(string name);
        /// <summary>
        /// Sets a header value.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Value associated with the header name.</param>
		void SetHeader(string name, object value);
        /// <summary>
        /// Retrieves whether for the specified header name an associated value exists.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>The associated value with the header name.</returns>
		bool HeaderExists(string name);
        /// <summary>
        /// Gets the Flex client id specified in the message headers ("DSId").
        /// </summary>
        /// <returns>The Flex client id.</returns>
        string GetFlexClientId();
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        IMessage Copy();
	}
}
