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

namespace FluorineFx.Scheduling
{
    /// <summary>
    /// Interface that must be implemented by classes that can be scheduled for periodic execution.
    /// </summary>
    public interface IScheduledJob
    {
        /// <summary>
        /// Get or sets the name of this job.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Get or set the data map that is associated with the job.
        /// </summary>
        Hashtable JobDataMap { get; set; }
        /// <summary>
        /// Called each time the job is triggered by the scheduling service.
        /// </summary>
        /// <param name="context">The execution context.</param>
        void Execute(ScheduledJobContext context);
    }
}
