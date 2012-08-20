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

namespace FluorineFx.Scheduling
{
    /// <summary>
    /// Instructs Scheduler what to do with a trigger and job.
    /// </summary>
    public enum SchedulerInstruction
    {
        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that the <see cref="Trigger" />
        /// has no further instructions.
        /// </summary>
        NoInstruction,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that the <see cref="Trigger" />
        /// wants the <see cref="JobDetail" /> to re-Execute
        /// immediately. If not in a 'RECOVERING' or 'FAILED_OVER' situation, the
        /// execution context will be re-used (giving the <see cref="IJob" /> the
        /// abilitiy to 'see' anything placed in the context by its last execution).
        /// </summary>      
        ReExecuteJob,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that the <see cref="Trigger" />
        /// should be put in the <see cref="TriggerState.Complete" /> state.
        /// </summary>
        SetTriggerComplete,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that the <see cref="Trigger" />
        /// wants itself deleted.
        /// </summary>
        DeleteTrigger,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that all <see cref="Trigger" />
        /// s referencing the same <see cref="JobDetail" /> as
        /// this one should be put in the <see cref="TriggerState.Complete" /> state.
        /// </summary>
        SetAllJobTriggersComplete,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that all <see cref="Trigger" />
        /// s referencing the same <see cref="JobDetail" /> as
        /// this one should be put in the <see cref="TriggerState.Error" /> state.
        /// </summary>
        SetAllJobTriggersError,

        /// <summary>
        /// Instructs the <see cref="ISchedulingService" /> that the <see cref="Trigger" />
        /// should be put in the <see cref="TriggerState.Error" /> state.
        /// </summary>
        SetTriggerError
    }
}
