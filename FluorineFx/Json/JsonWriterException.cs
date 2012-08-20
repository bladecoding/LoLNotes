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

namespace FluorineFx.Json
{
	/// <summary>
	/// The exception thrown when an error occurs while reading Json text.
	/// </summary>
	public class JsonWriterException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonWriterException"/> class.
		/// </summary>
		public JsonWriterException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonWriterException"/> class
		/// with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public JsonWriterException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonWriterException"/> class
		/// with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public JsonWriterException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
