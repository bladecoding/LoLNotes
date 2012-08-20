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
using System.Runtime.Serialization;

namespace FluorineFx.Exceptions
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
	class ProtocolException : FluorineException
	{
		/// <summary>
		/// Initializes a new instance of the ProtocolException class.
		/// </summary>
		public ProtocolException()
		{
		}
		/// <summary>
		/// Initializes a new instance of the ProtocolException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>			
		public ProtocolException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the ProtocolException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="inner">The exception that is the cause of the current exception. If the innerException parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception.</param>	
		/// <remarks>An exception that is thrown as a direct result of a previous exception should include a reference to the previous exception in the InnerException property. The InnerException property returns the same value that is passed into the constructor, or a null reference (Nothing in Visual Basic) if the InnerException property does not supply the inner exception value to the constructor.</remarks>			
		public ProtocolException(string message, Exception inner) : base(message, inner)																 
		{																 
		}
#if !SILVERLIGHT
		/// <summary>
		/// Initializes a new instance of the ProtocolException class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		public ProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
