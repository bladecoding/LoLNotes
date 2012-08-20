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
using System.Globalization;
using System.Runtime.Serialization;

namespace FluorineFx.Scheduling
{
    public class JobExecutionException : SchedulerException
    {
        private bool refire = false;
        private bool unscheduleTrigg = false;
        private bool unscheduleAllTriggs = false;

        /// <summary>
        /// Gets or sets a value indicating whether to unschedule firing trigger.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if firing trigger should be unscheduled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool UnscheduleFiringTrigger
        {
            set { unscheduleTrigg = value; }
            get { return unscheduleTrigg; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to unschedule all triggers.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if all triggers should be unscheduled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool UnscheduleAllTriggers
        {
            set { unscheduleAllTriggs = value; }
            get { return unscheduleAllTriggs; }
        }


        /// <summary>
        /// Create a JobExcecutionException, with the 're-fire immediately' flag set
        /// to <see langword="false" />.
        /// </summary>
        public JobExecutionException()
        {
        }

        /// <summary>
        /// Create a JobExcecutionException, with the given cause.
        /// </summary>
        /// <param name="cause">The cause.</param>
        public JobExecutionException(Exception cause)
            : base(cause)
        {
        }

        /// <summary>
        /// Create a JobExcecutionException, with the given message.
        /// </summary>
        public JobExecutionException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionException"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="cause">The original cause.</param>
        public JobExecutionException(string msg, Exception cause)
            : base(msg, cause)
        {
        }

        /// <summary>
        /// Create a JobExcecutionException with the 're-fire immediately' flag set
        /// to the given value.
        /// </summary>
        public JobExecutionException(bool refireImmediately)
        {
            refire = refireImmediately;
        }

        /// <summary>
        /// Create a JobExcecutionException with the given underlying exception, and
        /// the 're-fire immediately' flag set to the given value.
        /// </summary>
        public JobExecutionException(Exception cause, bool refireImmediately)
            : base(cause)
        {
            refire = refireImmediately;
        }

        /// <summary>
        /// Create a JobExcecutionException with the given message, and underlying
        /// exception, and the 're-fire immediately' flag set to the given value.
        /// </summary>
        public JobExecutionException(string msg, Exception cause, bool refireImmediately)
            : base(msg, cause)
        {
            refire = refireImmediately;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        public JobExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to refire immediately.
        /// </summary>
        /// <value><c>true</c> if to refire immediately; otherwise, <c>false</c>.</value>
        public virtual bool RefireImmediately
        {
            get { return refire; }
            set { refire = value; }
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
            return
                string.Format(CultureInfo.InvariantCulture,
                    "Parameters: refire = {0}, unscheduleFiringTrigger = {1}, unscheduleAllTriggers = {2} \n {3}",
                    RefireImmediately, UnscheduleFiringTrigger, UnscheduleAllTriggers, base.ToString());
        }
    }
}
