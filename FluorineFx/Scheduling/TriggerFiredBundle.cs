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
using FluorineFx.Util;

#if NET_1_1
using FluorineFx.Util.Nullables;
#else
using NullableDateTime = System.Nullable<System.DateTime>;
#endif

namespace FluorineFx.Scheduling
{
    /// <summary>
    /// A simple class used for returning execution-time data from the JobStore to the <see cref="SchedulerThread" />.
    /// </summary>
    [Serializable]
    class TriggerFiredBundle
    {
        private readonly IScheduledJob _job;
        private readonly Trigger _trigger;
        private readonly bool _jobIsRecovering;
        private NullableDateTime _fireTimeUtc;
        private NullableDateTime _scheduledFireTimeUtc;
        private NullableDateTime _prevFireTimeUtc;
        private NullableDateTime _nextFireTimeUtc;

        /// <summary>
        /// Gets the job detail.
        /// </summary>
        /// <value>The job detail.</value>
        public virtual IScheduledJob Job
        {
            get { return _job; }
        }

        /// <summary>
        /// Gets the trigger.
        /// </summary>
        /// <value>The trigger.</value>
        public virtual Trigger Trigger
        {
            get { return _trigger; }
        }


        /// <summary>
        /// Gets a value indicating whether this <see cref="TriggerFiredBundle"/> is recovering.
        /// </summary>
        /// <value><c>true</c> if recovering; otherwise, <c>false</c>.</value>
        public virtual bool Recovering
        {
            get { return _jobIsRecovering; }
        }

        /// <returns> 
        /// Returns the UTC fire time.
        /// </returns>
        public virtual NullableDateTime FireTimeUtc
        {
            get { return _fireTimeUtc; }
        }

        /// <summary>
        /// Gets the next UTC fire time.
        /// </summary>
        /// <value>The next fire time.</value>
        /// <returns> Returns the nextFireTimeUtc.</returns>
        public virtual NullableDateTime NextFireTimeUtc
        {
            get { return _nextFireTimeUtc; }
        }

        /// <summary>
        /// Gets the previous UTC fire time.
        /// </summary>
        /// <value>The previous fire time.</value>
        /// <returns> Returns the previous fire time. </returns>
        public virtual NullableDateTime PrevFireTimeUtc
        {
            get { return _prevFireTimeUtc; }
        }

        /// <returns> 
        /// Returns the scheduled UTC fire time.
        /// </returns>
        public virtual NullableDateTime ScheduledFireTimeUtc
        {
            get { return _scheduledFireTimeUtc; }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFiredBundle"/> class.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="jobIsRecovering">if set to <c>true</c> [job is recovering].</param>
        /// <param name="fireTimeUtc">The fire time.</param>
        /// <param name="scheduledFireTimeUtc">The scheduled fire time.</param>
        /// <param name="prevFireTimeUtc">The previous fire time.</param>
        /// <param name="nextFireTimeUtc">The next fire time.</param>
        public TriggerFiredBundle(IScheduledJob job, Trigger trigger, bool jobIsRecovering,
                                  NullableDateTime fireTimeUtc,
                                  NullableDateTime scheduledFireTimeUtc,
                                  NullableDateTime prevFireTimeUtc,
                                  NullableDateTime nextFireTimeUtc)
        {
            _job = job;
            _trigger = trigger;
            _jobIsRecovering = jobIsRecovering;
            _fireTimeUtc = DateTimeUtils.AssumeUniversalTime(fireTimeUtc);
            _scheduledFireTimeUtc = DateTimeUtils.AssumeUniversalTime(scheduledFireTimeUtc);
            _prevFireTimeUtc = DateTimeUtils.AssumeUniversalTime(prevFireTimeUtc);
            _nextFireTimeUtc = DateTimeUtils.AssumeUniversalTime(nextFireTimeUtc);
        }
    }
}
