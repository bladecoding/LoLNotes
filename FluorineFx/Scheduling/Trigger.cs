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
using FluorineFx.Util;

#if NET_1_1
using FluorineFx.Util.Nullables;
#else
using NullableDateTime = System.Nullable<System.DateTime>;
#endif


namespace FluorineFx.Scheduling
{
    public class Trigger : IComparable
    {
        /// <summary>
        /// Used to indicate the 'repeat count' of the trigger is indefinite. Or in
        /// other words, the trigger should repeat continually until the trigger's
        /// ending timestamp.
        /// </summary>
        public const int RepeatIndefinitely = -1;
        public const int DefaultPriority = 5;

        private string _name;
        private string _jobName;
        private NullableDateTime _endTimeUtc;
        private DateTime _startTimeUtc;
        private long _repeatInterval;

        private NullableDateTime _nextFireTimeUtc = null;
        private NullableDateTime _previousFireTimeUtc = null;
        private int _repeatCount = 0;
        private int _timesTriggered = 0;
        private bool _complete = false;
        private int _priority = DefaultPriority;

        /// <summary>
		/// Create a <see cref="Trigger" /> that will occur immediately, and not repeat.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobName">The job name.</param>
        public Trigger(string name, string jobName)
            : this(name, jobName, DateTime.UtcNow, null, 0, 0)
        {
        }
        /// <summary>
        /// Create a <see cref="Trigger" /> that will occur immediately, and
        /// repeat at the the given interval the given number of times.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobName">The job name.</param>
        /// <param name="repeatCount">The number of times for the <see cref="Trigger" /> to repeat
        /// firing, use <see cref="RepeatIndefinitely "/> for unlimited times.</param></param>
        /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
        public Trigger(string name, string jobName, int repeatCount, long repeatInterval)
            : this(name, jobName, DateTime.UtcNow, null, repeatCount, repeatInterval)
        {
        }
        /// <summary>
        /// Create a <see cref="Trigger" /> that will occur at the given time, and not repeat.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobName">The job name.</param>
        /// <param name="startTimeUtc">A UTC <see cref="DateTime" /> set to the time for the <see cref="Trigger" /> to fire.</param>
        public Trigger(string name, string jobName, DateTime startTimeUtc)
            : this(name, jobName, startTimeUtc, null, 0, 0)
        {
        }
        /// <summary>
        /// Create a <see cref="Trigger" /> that will occur at the given time,
        /// and repeat at the the given interval the given number of times, or until
        /// the given end time.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobName">The job name.</param>
        /// <param name="startTimeUtc">A UTC <see cref="DateTime" /> set to the time for the <see cref="Trigger" /> to fire.</param>
        /// <param name="endTimeUtc">A UTC <see cref="DateTime" /> set to the time for the <see cref="Trigger" /> to quit repeat firing.</param>
        /// <param name="repeatCount">The number of times for the <see cref="Trigger" /> to repeat
        /// firing, use <see cref="RepeatIndefinitely "/> for unlimited times.</param></param>
        /// <param name="repeatInterval">The number of milliseconds to pause between the repeat firing.</param>
        public Trigger(string name, string jobName, DateTime startTimeUtc, NullableDateTime endTimeUtc, int repeatCount, long repeatInterval)
        {
            _name = name;
            _jobName = jobName;
            this.StartTimeUtc = startTimeUtc;
            this.EndTimeUtc = endTimeUtc;
            this.RepeatInterval = repeatInterval;
            this.RepeatCount = repeatCount;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string JobName
        {
            get { return _jobName; }
            set { _jobName = value; }
        }

        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private int _misfireInstruction = FluorineFx.Scheduling.MisfireInstruction.InstructionNotSet;

        /// <summary>
        /// Get or set the instruction the <see cref="IScheduler" /> should be given for
        /// handling misfire situations for this <see cref="Trigger" />- the
        /// concrete <see cref="Trigger" /> type that you are using will have
        /// defined a set of additional MISFIRE_INSTRUCTION_XXX
        /// constants that may be passed to this method.
        /// <p>
        /// If not explicitly set, the default value is <see cref="MisfireInstruction.InstructionNotSet" />.
        /// </p>
        /// </summary>
        /// <seealso cref="Quartz.MisfireInstruction.InstructionNotSet" />
        /// <seealso cref="UpdateAfterMisfire" />
        /// <seealso cref="SimpleTrigger" />
        /// <seealso cref="CronTrigger" />
        public virtual int MisfireInstruction
        {
            get { return _misfireInstruction; }
            set
            {
                _misfireInstruction = value;
            }
        }

        public DateTime StartTimeUtc
        {
            get { return _startTimeUtc; }
            set
            {
                if (EndTimeUtc.HasValue && EndTimeUtc.Value < value)
                    throw new ArgumentException("End time cannot be before start time");

                if (HasMillisecondPrecision)
                {
                    // round off millisecond...	
                    _startTimeUtc = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
                }
                else
                {
                    _startTimeUtc = value;
                }
                _startTimeUtc = DateTimeUtils.AssumeUniversalTime(_startTimeUtc);
            }
        }

        public NullableDateTime EndTimeUtc
        {
            get { return _endTimeUtc; }
            set
            {
                _endTimeUtc = value;
                DateTime sTime = StartTimeUtc;
                if (value.HasValue && (sTime > value.Value))
                    throw new ArgumentException("End time cannot be before start time");
                _endTimeUtc = DateTimeUtils.AssumeUniversalTime(value);
            }
        }

        public bool HasMillisecondPrecision
        {
            get { return true; }
        }
        /// <summary>
        /// Get or set thhe number of times the <see cref="Trigger" /> should
        /// repeat, after which it will be automatically deleted.
        /// </summary>
        /// <seealso cref="RepeatIndefinitely" />
        public int RepeatCount
        {
            get { return _repeatCount; }
            set
            {
                if (value < 0 && value != RepeatIndefinitely)
                    throw new ArgumentException("Repeat count must be >= 0, use the constant RepeatIndefinitely for infinite.");
                _repeatCount = value;
            }
        }
        /// <summary>
        /// Get or set the the time interval (in milliseconds) at which the Trigger should repeat.
        /// </summary>
        public long RepeatInterval
        {
            get { return _repeatInterval; }

            set
            {
                if (value < 0)
                    throw new ArgumentException("Repeat interval must be >= 0");
                _repeatInterval = value;
            }
        }

        /// <summary>
        /// Get or set the number of times the <see cref="Trigger" /> has already fired.
        /// </summary>
        public virtual int TimesTriggered
        {
            get { return _timesTriggered; }
            set { _timesTriggered = value; }
        }

        /// <summary> 
        /// Returns the final UTC time at which the <see cref="Trigger" /> will
        /// fire, if repeatCount is RepeatIndefinitely, null will be returned.
        /// <p>
        /// Note that the return time may be in the past.
        /// </p>
        /// </summary>
        public NullableDateTime FinalFireTimeUtc
        {
            get
            {
                if (_repeatCount == 0)
                    return StartTimeUtc;

                if (_repeatCount == RepeatIndefinitely && !EndTimeUtc.HasValue)
                    return null;

                if (_repeatCount == RepeatIndefinitely && !EndTimeUtc.HasValue)
                {
                    return null;
                }
                else if (_repeatCount == RepeatIndefinitely)
                {
                    return GetFireTimeBefore(EndTimeUtc);
                }

                DateTime lastTrigger = StartTimeUtc.AddMilliseconds(_repeatCount * _repeatInterval);

                if (!EndTimeUtc.HasValue || lastTrigger < EndTimeUtc.Value)
                {
                    return lastTrigger;
                }
                else
                {
                    return GetFireTimeBefore(EndTimeUtc);
                }
            }
        }

        /// <summary>
        /// Called when the scheduler has decided to 'fire'
        /// the trigger (Execute the associated <see cref="IScheduledJob" />), in order to
        /// give the <see cref="Trigger" /> a chance to update itself for its next
        /// triggering (if any).
        /// </summary>
        /// <seealso cref="JobExecutionException" />
        public void Triggered()
        {
            _timesTriggered++;
            _previousFireTimeUtc = _nextFireTimeUtc;
            _nextFireTimeUtc = GetFireTimeAfter(_nextFireTimeUtc);
        }
        /// <summary>
        /// Called by the scheduler at the time a <see cref="Trigger" /> is first
        /// added to the scheduler, in order to have the <see cref="Trigger" />
        /// compute its first fire time.
        /// </summary>
        /// <returns></returns>
        public NullableDateTime ComputeFirstFireTimeUtc()
        {
            _nextFireTimeUtc = StartTimeUtc;
            return _nextFireTimeUtc;
        }

        public NullableDateTime GetNextFireTimeUtc()
        {
            return _nextFireTimeUtc;
        }

        public NullableDateTime GetPreviousFireTimeUtc()
        {
            return _previousFireTimeUtc;
        }

        /// <summary> 
        /// Returns the next UTC time at which the <see cref="Trigger" /> will
        /// fire, after the given UTC time. If the trigger will not fire after the given
        /// time, <see langword="null" /> will be returned.
        /// </summary>
        public NullableDateTime GetFireTimeAfter(NullableDateTime afterTimeUtc)
        {
            afterTimeUtc = DateTimeUtils.AssumeUniversalTime(afterTimeUtc);

            if (_complete)
                return null;

            if ((_timesTriggered > _repeatCount) && (_repeatCount != RepeatIndefinitely))
                return null;

            if (!afterTimeUtc.HasValue)
                afterTimeUtc = DateTime.UtcNow;

            if (_repeatCount == 0 && afterTimeUtc.Value.CompareTo(StartTimeUtc) >= 0)
                return null;

            DateTime startMillis = StartTimeUtc;
            DateTime afterMillis = afterTimeUtc.Value;
            DateTime endMillis = !EndTimeUtc.HasValue ? DateTime.MaxValue : EndTimeUtc.Value;

            if (endMillis <= afterMillis)
                return null;

            if (afterMillis < startMillis)
                return startMillis;

            long numberOfTimesExecuted = ((long)(afterMillis - startMillis).TotalMilliseconds / _repeatInterval) + 1;

            if ((numberOfTimesExecuted > _repeatCount) && (_repeatCount != RepeatIndefinitely))
                return null;

            DateTime time = startMillis.AddMilliseconds(numberOfTimesExecuted * _repeatInterval);
            if (endMillis <= time)
                return null;

            return time;
        }

        /// <summary>
        /// Returns the last UTC time at which the <see cref="Trigger" /> will
        /// fire, before the given time. If the trigger will not fire before the
        /// given time, <see langword="null" /> will be returned.
        /// </summary>
        public virtual NullableDateTime GetFireTimeBefore(NullableDateTime endUtc)
        {
            endUtc = DateTimeUtils.AssumeUniversalTime(endUtc);
            if (endUtc.Value < StartTimeUtc)
                return null;

            int numFires = ComputeNumTimesFiredBetween(StartTimeUtc, endUtc);
            return StartTimeUtc.AddMilliseconds(numFires * _repeatInterval);
        }

        /// <summary>
        /// Computes the number of times fired between the two UTC date times.
        /// </summary>
        /// <param name="startTimeUtc">The UTC start date and time.</param>
        /// <param name="endTimeUtc">The UTC end date and time.</param>
        /// <returns></returns>
        public virtual int ComputeNumTimesFiredBetween(NullableDateTime startTimeUtc, NullableDateTime endTimeUtc)
        {
            startTimeUtc = DateTimeUtils.AssumeUniversalTime(startTimeUtc);
            endTimeUtc = DateTimeUtils.AssumeUniversalTime(endTimeUtc);

            long time = (long)(endTimeUtc.Value - startTimeUtc.Value).TotalMilliseconds;
            return (int)(time / _repeatInterval);
        }

        /// <summary> 
        /// Determines whether or not the <see cref="Trigger" /> will occur again.
        /// </summary>
        public bool GetMayFireAgain()
        {
            return GetNextFireTimeUtc().HasValue;
        }

        public virtual SchedulerInstruction ExecutionComplete(ScheduledJobContext context, JobExecutionException result)
        {
            if (result != null && result.RefireImmediately)
                return SchedulerInstruction.ReExecuteJob;
            if (result != null && result.UnscheduleFiringTrigger)
                return SchedulerInstruction.SetTriggerComplete;
            if (result != null && result.UnscheduleAllTriggers)
                return SchedulerInstruction.SetAllJobTriggersComplete;
            if (result != null && !result.RefireImmediately)
                return SchedulerInstruction.NoInstruction;
            if (!GetMayFireAgain())
                return SchedulerInstruction.DeleteTrigger;
            return SchedulerInstruction.NoInstruction;
        }

        /// <summary>
        /// Updates the <see cref="SimpleTrigger" />'s state based on the
        /// MisfireInstruction value that was selected when the <see cref="SimpleTrigger" />
        /// was created.
        /// </summary>
        /// <remarks>
        /// If MisfireSmartPolicyEnabled is set to true,
        /// then the following scheme will be used: <br />
        /// <ul>
        /// <li>If the Repeat Count is 0, then the instruction will
        /// be interpreted as <see cref="MisfireInstruction.SimpleTrigger.FireNow" />.</li>
        /// <li>If the Repeat Count is <see cref="RepeatIndefinitely" />, then
        /// the instruction will be interpreted as <see cref="MisfireInstruction.SimpleTrigger.RescheduleNowWithRemainingRepeatCount" />.
        /// <b>WARNING:</b> using MisfirePolicy.SimpleTrigger.RescheduleNowWithRemainingRepeatCount 
        /// with a trigger that has a non-null end-time may cause the trigger to 
        /// never fire again if the end-time arrived during the misfire time span. 
        /// </li>
        /// <li>If the Repeat Count is > 0, then the instruction
        /// will be interpreted as <see cref="MisfireInstruction.SimpleTrigger.RescheduleNowWithExistingRepeatCount" />.
        /// </li>
        /// </ul>
        /// </remarks>
        public void UpdateAfterMisfire()
        {
            int instr = MisfireInstruction;
            if (instr == FluorineFx.Scheduling.MisfireInstruction.SmartPolicy)
            {
                if (RepeatCount == 0)
                {
                    instr = FluorineFx.Scheduling.MisfireInstruction.FireNow;
                }
                else if (RepeatCount == RepeatIndefinitely)
                {
                    instr = FluorineFx.Scheduling.MisfireInstruction.RescheduleNextWithRemainingCount;

                }
                else
                {
                    instr = FluorineFx.Scheduling.MisfireInstruction.RescheduleNowWithExistingRepeatCount;
                }
            }
            else if (instr == FluorineFx.Scheduling.MisfireInstruction.FireNow && RepeatCount != 0)
            {
                instr = FluorineFx.Scheduling.MisfireInstruction.RescheduleNowWithRemainingRepeatCount;
            }

            if (instr == FluorineFx.Scheduling.MisfireInstruction.FireNow)
            {
                SetNextFireTime(DateTime.UtcNow);
            }
            else if (instr == FluorineFx.Scheduling.MisfireInstruction.RescheduleNextWithExistingCount)
            {
                NullableDateTime newFireTime = GetFireTimeAfter(DateTime.UtcNow);
                SetNextFireTime(newFireTime);
            }
            else if (instr == FluorineFx.Scheduling.MisfireInstruction.RescheduleNextWithRemainingCount)
            {
                NullableDateTime newFireTime = GetFireTimeAfter(DateTime.UtcNow);
                if (newFireTime.HasValue)
                {
                    int timesMissed = ComputeNumTimesFiredBetween(_nextFireTimeUtc, newFireTime);
                    TimesTriggered = TimesTriggered + timesMissed;
                }

                SetNextFireTime(newFireTime);
            }
            else if (instr == FluorineFx.Scheduling.MisfireInstruction.RescheduleNowWithExistingRepeatCount)
            {
                DateTime newFireTime = DateTime.UtcNow;
                if (_repeatCount != 0 && _repeatCount != RepeatIndefinitely)
                {
                    RepeatCount = RepeatCount - TimesTriggered;
                    TimesTriggered = 0;
                }

                if (EndTimeUtc.HasValue && EndTimeUtc.Value < newFireTime)
                {
                    SetNextFireTime(null); // We are past the end time
                }
                else
                {
                    StartTimeUtc = newFireTime;
                    SetNextFireTime(newFireTime);
                }
            }
            else if (instr == FluorineFx.Scheduling.MisfireInstruction.RescheduleNowWithRemainingRepeatCount)
            {
                DateTime newFireTime = DateTime.UtcNow;
                int timesMissed = ComputeNumTimesFiredBetween(_nextFireTimeUtc, newFireTime);

                if (_repeatCount != 0 && _repeatCount != RepeatIndefinitely)
                {
                    int remainingCount = RepeatCount - (TimesTriggered + timesMissed);
                    if (remainingCount <= 0)
                    {
                        remainingCount = 0;
                    }
                    RepeatCount = remainingCount;
                    TimesTriggered = 0;
                }


                if (EndTimeUtc.HasValue && EndTimeUtc.Value < newFireTime)
                {
                    SetNextFireTime(null); // We are past the end time
                }
                else
                {
                    StartTimeUtc = newFireTime;
                    SetNextFireTime(newFireTime);
                }
            }
        }

        /// <summary>
        /// Set the next UTC time at which the <see cref="SimpleTrigger" /> should fire.
        /// <strong>This method should not be invoked by client code.</strong>
        /// </summary>
        public void SetNextFireTime(NullableDateTime fireTimeUtc)
        {
            _nextFireTimeUtc = DateTimeUtils.AssumeUniversalTime(fireTimeUtc);
        }

        /// <summary>
        /// Set the previous UTC time at which the <see cref="SimpleTrigger" /> fired.
        /// <strong>This method should not be invoked by client code.</strong>
        /// </summary>
        public virtual void SetPreviousFireTime(NullableDateTime fireTimeUtc)
        {
            _previousFireTimeUtc = DateTimeUtils.AssumeUniversalTime(fireTimeUtc);
        }


        #region IComparable Members

        /// <summary>
        /// Compare the next fire time of this <see cref="Trigger" /> to that of another.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Trigger other = (Trigger)obj;

            NullableDateTime myTime = GetNextFireTimeUtc();
            NullableDateTime otherTime = other.GetNextFireTimeUtc();

            if (!myTime.HasValue && !otherTime.HasValue)
                return 0;

            if (!myTime.HasValue)
                return 1;

            if (!otherTime.HasValue)
                return -1;

            if ((myTime.Value < otherTime.Value))
                return -1;

            if ((myTime.Value > otherTime.Value))
                return 1;

            return 0;
        }

        #endregion
    }
}
