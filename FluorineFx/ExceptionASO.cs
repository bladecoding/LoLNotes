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
using FluorineFx.Configuration;

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class ExceptionASO : ASObject
	{
		/// <summary>
		/// Initializes a new instance of the ExceptionASO class.
		/// </summary>
        /// <param name="exception">The exception that is the cause of the current exception.</param>
		public ExceptionASO(Exception exception)
		{
			// When Flash Remoting MX receives a Status event, Flash passes an error object that contains 
			// information about the error to the status event handler. 
			// The error object has the following format
			//
            // code			Currently, always "Server.Processing".
            // level		Currently, always "error".
			// description	A string that describes the error.
			// details		A stack trace that indicates the processing state at the time of the exception. 
			// type			The error class name.
			// rootcause	A nested error object that contains additional information on the cause of the error. Provided only if a Java servletException is thrown.

            Add("code", "Server.Processing");
			Add("level", "error");
			Add("description", exception.Message);
            if (FluorineConfiguration.Instance.FluorineSettings.CustomErrors.StackTrace)
                Add("details", exception.StackTrace);
            else
                Add("details", null);
			Add("type", exception.GetType().FullName);
			if(exception.InnerException != null)
				Add("rootcause", new ExceptionASO(exception.InnerException));
		}

        public ExceptionASO(string error)
        {
            Add("code", "Server.Processing");
            Add("level", "error");
            Add("description", error);
        }
	}
}
