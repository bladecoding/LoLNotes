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
using FluorineFx.Exceptions;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// The ServiceException class is used to report exceptions within the messaging system.
    /// </summary>
    public class ServiceException : MessageException
    {
		/// <summary>
        /// Initializes a new instance of the ServiceException class.
		/// </summary>
        public ServiceException()
            : base()
        {
        }
		/// <summary>
        /// Initializes a new instance of the ServiceException class.
		/// </summary>
		/// <param name="inner">Reference to the inner exception that is the cause of this exception.</param>
        public ServiceException(Exception inner)
            : base(inner.Message, inner)
		{
		}
        /// <summary>
        /// Initializes a new instance of the ServiceException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>			
        public ServiceException(string message)
            : base(message)
        {
        }
    }
}
