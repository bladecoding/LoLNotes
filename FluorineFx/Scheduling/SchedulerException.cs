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
using System.Globalization;
using System.Runtime.Serialization;

namespace FluorineFx.Scheduling
{
    public class SchedulerException : ApplicationException
    {
        public const int ErrorBadConfiguration = 50;
        public const int ErrorClientError = 100;
        public const int ErrorJobExecutionThrewException = 800;
        public const int ErrorTriggerThrewException = 850;
        public const int ErrorUnspecified = 0;
        public const int ErrorUnsupportedFunctionInThisConfiguration = 210;


        private readonly Exception cause;

        private int errorCode = ErrorUnspecified;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        public SchedulerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public SchedulerException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        public SchedulerException(SerializationInfo info, StreamingContext context)
            : base(info.GetString("Message"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="errorCode">The error code.</param>
        public SchedulerException(string msg, int errorCode)
            : base(msg)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="cause">The cause.</param>
        public SchedulerException(Exception cause)
            : base(cause.ToString(), cause)
        {
            this.cause = cause;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="cause">The cause.</param>
        public SchedulerException(string msg, Exception cause)
            : base(msg, cause)
        {
            this.cause = cause;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="cause">The cause.</param>
        /// <param name="errorCode">The error code.</param>
        public SchedulerException(string msg, Exception cause, int errorCode)
            : base(msg, cause)
        {
            this.cause = cause;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Return the exception that is the underlying cause of this exception.
        /// This may be used to find more detail about the cause of the error.
        /// </summary>
        /// <returns> The underlying exception, or <see langword="null" /> if there is not
        /// one.
        /// </returns>
        public virtual Exception UnderlyingException
        {
            get { return cause; }
        }

        /// <summary>
        /// Get the error code associated with this exception.
        /// This may be used to find more detail about the cause of the error.
        /// </summary>
        /// <returns> 
        /// One of the ERR_XXX constants defined in this class.
        /// </returns>
        public int ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }

        /// <summary>
        /// Determine if the specified error code is in the <see cref="ErrorClientError" />
        /// category of errors.
        /// </summary>
        public virtual bool ClientError
        {
            get { return (errorCode >= ErrorClientError && errorCode <= ErrorClientError + 99); }
        }

        /// <summary>
        /// Determine if the specified error code is in the <see cref="ErrorClientError" />
        /// category of errors.
        /// </summary>
        public virtual bool ConfigurationError
        {
            get { return (errorCode >= ErrorBadConfiguration && errorCode <= ErrorBadConfiguration + 49); }
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/></PermissionSet>
        public override string ToString()
        {
            if (cause == null)
            {
                return base.ToString();
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} [See nested exception: {1}]", base.ToString(), cause);
            }
        }
    }
}
