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
using System.Threading;
using System.Globalization;
using FluorineFx.Configuration;
using FluorineFx.Collections;
using FluorineFx.Threading;
using log4net;

#if NET_1_1
using FluorineFx.Util.Nullables;
#else
using NullableDateTime = System.Nullable<System.DateTime>;
#endif

namespace FluorineFx.Scheduling
{
    /// <summary>
    /// Possible internal trigger states.
    /// </summary>
    internal enum InternalTriggerState
    {
        /// <summary>
        /// Waiting 
        /// </summary>
        Waiting,
        /// <summary>
        /// Acquired
        /// </summary>
        Acquired,
        /// <summary>
        /// Executing
        /// </summary>
        Executing,
        /// <summary>
        /// Complete
        /// </summary>
        Complete,
        /// <summary>
        /// Paused
        /// </summary>
        Paused,
        /// <summary>
        /// Blocked
        /// </summary>
        Blocked,
        /// <summary>
        /// Paused and Blocked
        /// </summary>
        PausedAndBlocked,
        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    class SchedulingService : ISchedulingService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SchedulingService));

        private Thread _thread;
        ThreadPoolEx _threadPool;
        private readonly object _pauseLock = new object();
        private bool _signaled;
        private bool _paused;
        private bool _halted;
        // When the scheduler finds there is no current trigger to fire, how long
        // it should wait until checking again...
        private const long DefaultIdleWaitTime = 30 * 1000;
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private long _idleWaitTime = DefaultIdleWaitTime;
        private int _idleWaitVariablness = 7 * 1000;
        private long _misfireThreshold = 5000L;

        public SchedulingService()
        {
            _paused = false;
            _halted = false;
            _threadPool = new ThreadPoolEx(ThreadPoolEx.DefaultIdleTimeout, 10);
            _thread = new Thread(new ThreadStart(Run));
            _thread.Start();
        }

        /// <summary>
        /// Gets the randomized idle wait time.
        /// </summary>
        /// <value>The randomized idle wait time.</value>
        private long GetRandomizedIdleWaitTime()
        {
            return _idleWaitTime - _random.Next(_idleWaitVariablness);
        }

        public virtual long MisfireThreshold
        {
            get { return _misfireThreshold; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Misfirethreashold must be larger than 0");
                _misfireThreshold = value;
            }
        }

        /// <summary>
        /// Signals the main processing loop to pause at the next possible point.
        /// </summary>
        internal virtual void TogglePause(bool pause)
        {
            lock (_pauseLock)
            {
                _paused = pause;

                if (_paused)
                {
                    SignalSchedulingChange();
                }
                else
                {
                    Monitor.Pulse(_pauseLock);
                }
            }
        }

        /// <summary>
        /// Signals the main processing loop to pause at the next possible point.
        /// </summary>
        internal virtual void Halt()
        {
            lock (_pauseLock)
            {
                _halted = true;

                if (_paused)
                {
                    Monitor.Pulse(_pauseLock);
                }
                else
                {
                    SignalSchedulingChange();
                }
            }
        }

        /// <summary>
        /// Signals the main processing loop that a change in scheduling has been
        /// made - in order to interrupt any sleeping that may be occuring while
        /// waiting for the fire time to arrive.
        /// </summary>
        internal virtual void SignalSchedulingChange()
        {
            _signaled = true;
        }

        /// <summary>
        /// The main processing loop.
        /// </summary>
        public void Run()
        {
            bool lastAcquireFailed = false;

            while (!_halted)
            {
                try
                {
                    // check if we're supposed to pause...
                    lock (_pauseLock)
                    {
                        while (_paused && !_halted)
                        {
                            try
                            {
                                // wait until togglePause(false) is called...
                                Monitor.Wait(_pauseLock, 100);
                            }
                            catch (ThreadInterruptedException)
                            {
                            }
                        }
                        if (_halted)
                        {
                            break;
                        }
                    }

                    int availTreadCount = _threadPool.AvailableThreads;
                    DateTime now;
                    int spinInterval;
                    int numPauses;
                    if (availTreadCount > 0)
                    {
                        Trigger trigger = null;

                        now = DateTime.UtcNow;

                        _signaled = false;
                        try
                        {
                            trigger = AcquireNextTrigger(now.AddMilliseconds(_idleWaitTime));
                            lastAcquireFailed = false;
                        }
                        catch (Exception e)
                        {
                            if (!lastAcquireFailed)
                            {
                                log.Error("SchedulerThreadLoop: RuntimeException " + e.Message, e);
                            }
                            lastAcquireFailed = true;
                        }

                        if (trigger != null)
                        {
                            now = DateTime.UtcNow;
                            DateTime triggerTime = trigger.GetNextFireTimeUtc().Value;
                            long timeUntilTrigger = (long)(triggerTime - now).TotalMilliseconds;
                            spinInterval = 10;

                            // this looping may seem a bit silly, but it's the
                            // current work-around
                            // for a dead-lock that can occur if the Thread.sleep()
                            // is replaced with
                            // a obj.wait() that gets notified when the signal is
                            // set...
                            // so to be able to detect the signal change without
                            // sleeping the entire
                            // timeUntilTrigger, we spin here... don't worry
                            // though, this spinning
                            // doesn't even register 0.2% cpu usage on a pentium 4.
                            numPauses = (int)(timeUntilTrigger / spinInterval);
                            while (numPauses >= 0 && !_signaled)
                            {
                                try
                                {
                                    Thread.Sleep(spinInterval);
                                }
                                catch (ThreadInterruptedException)
                                {
                                }

                                now = DateTime.UtcNow;
                                timeUntilTrigger = (long)(triggerTime - now).TotalMilliseconds;
                                numPauses = (int)(timeUntilTrigger / spinInterval);
                            }
                            if (_signaled)
                            {
                                try
                                {
                                    ReleaseAcquiredTrigger(trigger);
                                }
                                catch (Exception ex)
                                {
                                    log.Error("ReleaseAcquiredTrigger: RuntimeException " + ex.Message, ex);
                                }
                                _signaled = false;
                                continue;
                            }

                            // set trigger to 'executing'
                            TriggerFiredBundle bundle = null;

                            lock (_pauseLock)
                            {
                                if (!_halted)
                                {
                                    try
                                    {
                                        bundle = TriggerFired(trigger);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(string.Format(CultureInfo.InvariantCulture, "RuntimeException while firing trigger {0}", trigger.Name), ex);
                                    }
                                }

                                // it's possible to get 'null' if the trigger was paused,
                                // blocked, or other similar occurances that prevent it being
                                // fired at this time...  or if the scheduler was shutdown (halted)
                                if (bundle == null)
                                {
                                    try
                                    {
                                        ReleaseAcquiredTrigger(trigger);
                                    }
                                    catch (SchedulerException)
                                    {
                                    }
                                    continue;
                                }

                                _threadPool.QueueUserWorkItem(new WaitCallback(ProcessJob), bundle);
                            }

                            continue;
                        }
                    }
                    else
                    {
                        // if(availTreadCount > 0)
                        continue; // should never happen, if threadPool.blockForAvailableThreads() follows contract
                    }

                    // this looping may seem a bit silly, but it's the current
                    // work-around
                    // for a dead-lock that can occur if the Thread.sleep() is replaced
                    // with
                    // a obj.wait() that gets notified when the signal is set...
                    // so to be able to detect the signal change without sleeping the
                    // entier
                    // getRandomizedIdleWaitTime(), we spin here... don't worry though,
                    // the
                    // CPU usage of this spinning can't even be measured on a pentium
                    // 4.
                    now = DateTime.UtcNow;
                    DateTime waitTime = now.AddMilliseconds(GetRandomizedIdleWaitTime());
                    long timeUntilContinue = (long)(waitTime - now).TotalMilliseconds;
                    spinInterval = 10;
                    numPauses = (int)(timeUntilContinue / spinInterval);

                    while (numPauses > 0 && !_signaled)
                    {
                        try
                        {
                            Thread.Sleep(10);
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                        now = DateTime.UtcNow;
                        timeUntilContinue = (long)(waitTime - now).TotalMilliseconds;
                        numPauses = (int)(timeUntilContinue / spinInterval);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    log.Error("Runtime error occured in main trigger firing loop.", ex);
                }
            }
        }

        public void ProcessJob(object state)
        {
            TriggerFiredBundle bundle = state as TriggerFiredBundle;
            Trigger trigger = bundle.Trigger;
            IScheduledJob job = bundle.Job;
            do
            {
                JobExecutionException jobExecutionException = null;
                ScheduledJobContext scheduledJobContext = new ScheduledJobContext();
                try
                {
                    job.Execute(scheduledJobContext);
                }
                catch (JobExecutionException jee)
                {
                    jobExecutionException = jee;
                    log.Info(string.Format(CultureInfo.InvariantCulture, "Job {0} threw a JobExecutionException: ", job.Name), jee);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format(CultureInfo.InvariantCulture, "Job {0} threw an unhandled Exception: ", job.Name), ex);
                    SchedulerException se = new SchedulerException("Job threw an unhandled exception.", ex);
                    se.ErrorCode = SchedulerException.ErrorJobExecutionThrewException;
                    jobExecutionException = new JobExecutionException(se, false);
                    jobExecutionException.ErrorCode = JobExecutionException.ErrorJobExecutionThrewException;
                }

                SchedulerInstruction instCode = SchedulerInstruction.NoInstruction;
                try
                {
                    instCode = trigger.ExecutionComplete(scheduledJobContext, jobExecutionException);
                }
                catch(Exception)
                {
                    // If this happens, there's a bug in the trigger...
                }
                // update job/trigger or re-Execute job
                if (instCode == SchedulerInstruction.ReExecuteJob)
                {
                    if (log.IsDebugEnabled)
                        log.Debug("Rescheduling trigger to reexecute");
                    continue;
                }
                TriggeredJobComplete(trigger, job, instCode);
                break;
            } while (true);
            NotifySchedulerThread();
        }
            
    

        private readonly IDictionary _jobsDictionary = new Hashtable(100);
        private readonly IDictionary _triggersDictionary = new Hashtable(100);
        private readonly ArrayList _triggers = new ArrayList(100);
        private readonly object _jobLock = new object();
        private readonly object _triggerLock = new object();
        private readonly TreeSet _pausedTriggers = new TreeSet();
        private readonly TreeSet _pausedJobs = new TreeSet();
        private readonly TreeSet _blockedJobs = new TreeSet();
        private readonly TreeSet _timeTriggers = new TreeSet(new TriggerComparator());


        public void StoreJob(IScheduledJob job, bool replaceExisting)
        {
            JobWrapper jobWrapper = new JobWrapper(job);
            bool replace = false;
            lock (_jobLock)
            {
                if (_jobsDictionary[jobWrapper.Name] != null)
                {
                    if (!replaceExisting)
                    {
                        throw new ArgumentException("job");
                    }
                    replace = true;
                }
                if (!replace)
                {
                    _jobsDictionary[jobWrapper.Name] = jobWrapper;
                }
                else
                {
                    // update job detail
                    JobWrapper orig = _jobsDictionary[jobWrapper.Name] as JobWrapper;
                    orig.Job = jobWrapper.Job;
                }
            }
        }

        public void StoreJobAndTrigger(IScheduledJob job, Trigger trigger)
        {
            StoreJob(job, false);
            StoreTrigger(trigger, false);
        }

        public void StoreTrigger(Trigger trigger, bool replaceExisting)
        {
            TriggerWrapper tw = new TriggerWrapper(trigger);
            lock(_triggerLock)
            {
                if (_triggersDictionary.Contains(tw.Name))
                {
                    if (!replaceExisting)
                    {
                        throw new NotSupportedException("Object already exists " + trigger.Name);
                    }
                    // don't delete orphaned job, this trigger has the job anyways
                    RemoveTrigger(trigger.Name, false);
                }

                if (RetrieveJob(trigger.JobName) == null)
                {
                    throw new ApplicationException("The job (" + trigger.JobName + ") referenced by the trigger does not exist.");
                }

                // add to triggers array
                _triggers.Add(tw);

                _triggersDictionary[tw.Name] = tw;

                lock (_pausedTriggers)
                {
                    if (_pausedTriggers.Contains(trigger.Name) || _pausedJobs.Contains(trigger.JobName))
                    {
                        tw.State = InternalTriggerState.Paused;
                        if (_blockedJobs.Contains(trigger.JobName))
                        {
                            tw.State = InternalTriggerState.PausedAndBlocked;
                        }
                    }
                    else if (_blockedJobs.Contains(trigger.JobName))
                    {
                        tw.State = InternalTriggerState.Blocked;
                    }
                    else
                    {
                        _timeTriggers.Add(tw);
                    }
                }
            }
        }

        public IScheduledJob RetrieveJob(string jobName)
        {
            lock (_jobsDictionary)
            {
                JobWrapper jw = _jobsDictionary[jobName] as JobWrapper;
                return (jw != null) ? jw.Job : null;
            }
        }

        public Trigger RetrieveTrigger(string triggerName)
        {
            lock (_triggersDictionary)
            {
                TriggerWrapper tw = _triggersDictionary[triggerName] as TriggerWrapper;
                return (tw != null) ? tw.Trigger : null;
            }
        }

        public bool RemoveTrigger(string triggerName)
        {
            return RemoveTrigger(triggerName, true);
        }

        public bool RemoveTrigger(string triggerName, bool deleteOrphanedJob)
        {
            bool found;
            lock (_triggerLock)
            {
                // remove from triggers
                object tempObject;
                tempObject = _triggersDictionary[triggerName];
                _triggersDictionary.Remove(triggerName);
                found = (tempObject == null) ? false : true;
                if (found)
                {
                    TriggerWrapper tw = null;
                    // remove from triggers array
                    for (int i = 0; i < _triggers.Count; ++i)
                    {
                        tw = _triggers[i] as TriggerWrapper;
                        if (triggerName.Equals(tw.Name))
                        {
                            _triggers.RemoveAt(i);
                            break;
                        }
                    }
                    _timeTriggers.Remove(tw);

                    JobWrapper jw = _jobsDictionary[tw.Trigger.JobName] as JobWrapper;
                    Trigger[] triggers = GetTriggersForJob(tw.Trigger.JobName);
                    if ((triggers == null || triggers.Length == 0) && deleteOrphanedJob)
                    {
                        RemoveJob(tw.Trigger.JobName);
                    }
                }
            }
            return found;
        }

        public bool RemoveJob(string jobName)
        {
            bool found = false;
            Trigger[] triggers = GetTriggersForJob(jobName);
            for (int i = 0; i < triggers.Length; i++)
            {
                Trigger trigger = triggers[i];
                RemoveTrigger(trigger.Name);
                found = true;
            }
            lock (_jobLock)
            {
                object tempObject = _jobsDictionary[jobName];
                _jobsDictionary.Remove(jobName);
                found = (tempObject != null) | found;
            }
            return found;
        }

        /// <summary>
        /// Get all of the Triggers that are associated to the given Job.
        /// <p>
        /// If there are no matches, a zero-length array should be returned.
        /// </p>
        /// </summary>
        public virtual Trigger[] GetTriggersForJob(string jobName)
        {
            ArrayList result = new ArrayList();
            lock (_triggerLock)
            {
                for (int i = 0; i < _triggers.Count; i++)
                {
                    TriggerWrapper tw = _triggers[i] as TriggerWrapper;
                    if (tw.JobName.Equals(jobName))
                    {
                        result.Add(tw.Trigger);
                    }
                }
            }
            return (Trigger[])result.ToArray(typeof(Trigger));
        }

        public Trigger AcquireNextTrigger(DateTime noLaterThan)
        {
            TriggerWrapper tw = null;
            lock (_triggerLock)
            {
                while (tw == null)
                {
                    if (_timeTriggers.Count > 0)
                        tw = _timeTriggers[0] as TriggerWrapper;
                    if (tw == null)
                        return null;
                    if (!tw.Trigger.GetNextFireTimeUtc().HasValue)
                    {
                        _timeTriggers.Remove(tw);
                        tw = null;
                        continue;
                    }
                    _timeTriggers.Remove(tw);
                    if (ApplyMisfire(tw))
                    {
                        if (tw.Trigger.GetNextFireTimeUtc().HasValue)
                            _timeTriggers.Add(tw);
                        tw = null;
                        continue;
                    }
                    if (tw.Trigger.GetNextFireTimeUtc().Value > noLaterThan)
                    {
                        _timeTriggers.Add(tw);
                        return null;
                    }
                    tw.State = InternalTriggerState.Acquired;
                    //tw.Trigger.FireInstanceId = FiredTriggerRecordId;
                    return tw.Trigger;
                }
            }
            return null;            
        }

        protected bool ApplyMisfire(TriggerWrapper tw)
        {
            DateTime misfireTime = DateTime.UtcNow;
            if (MisfireThreshold > 0)
            {
                misfireTime = misfireTime.AddMilliseconds(-1 * MisfireThreshold);
            }

            NullableDateTime tnft = tw.Trigger.GetNextFireTimeUtc();
            if (!tnft.HasValue || tnft.Value > misfireTime)
                return false;

            tw.Trigger.UpdateAfterMisfire();

            if (!tw.Trigger.GetNextFireTimeUtc().HasValue)
            {
                tw.State = InternalTriggerState.Complete;
                lock (_triggerLock)
                {
                    _timeTriggers.Remove(tw);
                }
            }
            else if (tnft.Equals(tw.Trigger.GetNextFireTimeUtc()))
            {
                return false;
            }
            return true;
        }

        public void ReleaseAcquiredTrigger(Trigger trigger)
        {
            lock (_triggerLock)
            {
                TriggerWrapper tw = _triggersDictionary[trigger.Name] as TriggerWrapper;
                if (tw != null && tw.State == InternalTriggerState.Acquired)
                {
                    tw.State = InternalTriggerState.Waiting;
                    _timeTriggers.Add(tw);
                }
            }
        }

        public TriggerFiredBundle TriggerFired(Trigger trigger)
        {
            lock (_triggerLock)
            {
                TriggerWrapper tw = _triggersDictionary[trigger.Name] as TriggerWrapper;
                // was the trigger deleted since being acquired?
                if (tw == null || tw.Trigger == null)
                    return null;
                // was the trigger completed since being acquired?
                if (tw.State == InternalTriggerState.Complete)
                    return null;
                // was the trigger paused since being acquired?
                if (tw.State == InternalTriggerState.Paused)
                    return null;
                // was the trigger blocked since being acquired?
                if (tw.State == InternalTriggerState.Blocked)
                    return null;
                // was the trigger paused and blocked since being acquired?
                if (tw.State == InternalTriggerState.PausedAndBlocked)
                    return null;

                NullableDateTime prevFireTime = trigger.GetPreviousFireTimeUtc();
                // in case trigger was replaced between acquiring and firering
                _timeTriggers.Remove(tw);
                trigger.Triggered();
                //tw.state = TriggerWrapper.StateExecuting;
                tw.State = InternalTriggerState.Waiting;

                IScheduledJob job = RetrieveJob(trigger.JobName);
                TriggerFiredBundle bndle = new TriggerFiredBundle(job, trigger, false, DateTime.UtcNow,
                                           trigger.GetPreviousFireTimeUtc(), prevFireTime, trigger.GetNextFireTimeUtc());

                NullableDateTime d = tw.Trigger.GetNextFireTimeUtc();
                if (d.HasValue)
                {
                    lock (_triggerLock)
                    {
                        _timeTriggers.Add(tw);
                    }
                }

                return bndle;
            }
        }

        public void TriggeredJobComplete(Trigger trigger, IScheduledJob job, SchedulerInstruction triggerInstCode)
        {
            lock (_triggerLock)
            {
                JobWrapper jw = _jobsDictionary[job.Name] as JobWrapper;
                TriggerWrapper tw = _triggersDictionary[trigger.Name] as TriggerWrapper;

                // even if it was deleted, there may be cleanup to do
                _blockedJobs.Remove(job.Name);

                // check for trigger deleted during execution...
                if (tw != null)
                {
                    if (triggerInstCode == SchedulerInstruction.DeleteTrigger)
                    {
                        //log.Debug("Deleting trigger");
                        NullableDateTime d = trigger.GetNextFireTimeUtc();
                        if (!d.HasValue)
                        {
                            // double check for possible reschedule within job 
                            // execution, which would cancel the need to delete...
                            d = tw.Trigger.GetNextFireTimeUtc();
                            if (!d.HasValue)
                            {
                                RemoveTrigger(trigger.Name);
                            }
                            else
                            {
                                log.Debug("Deleting cancelled - trigger still active");
                            }
                        }
                        else
                        {
                            RemoveTrigger(trigger.Name);
                        }
                    }
                    else if (triggerInstCode == SchedulerInstruction.SetTriggerComplete)
                    {
                        tw.State = InternalTriggerState.Complete;
                        _timeTriggers.Remove(tw);
                    }
                    else if (triggerInstCode == SchedulerInstruction.SetTriggerError)
                    {
                        log.Info(string.Format(CultureInfo.InvariantCulture, "Trigger {0} set to ERROR state.", trigger.Name));
                        tw.State = InternalTriggerState.Error;
                    }
                    else if (triggerInstCode == SchedulerInstruction.SetAllJobTriggersError)
                    {
                        log.Info(string.Format(CultureInfo.InvariantCulture, "All triggers of Job {0} set to ERROR state.", trigger.Name));
                        SetAllTriggersOfJobToState(trigger.Name, InternalTriggerState.Error);
                    }
                    else if (triggerInstCode == SchedulerInstruction.SetAllJobTriggersComplete)
                    {
                        SetAllTriggersOfJobToState(trigger.Name, InternalTriggerState.Complete);
                    }
                }
            }
        }

        private void SetAllTriggersOfJobToState(string jobName, InternalTriggerState state)
        {
            ArrayList tws = GetTriggerWrappersForJob(jobName);
            foreach (TriggerWrapper tw in tws)
            {
                tw.State = state;
                if (state != InternalTriggerState.Waiting)
                {
                    _timeTriggers.Remove(tw);
                }
            }
        }

        private ArrayList GetTriggerWrappersForJob(string jobName)
        {
            ArrayList trigList = new ArrayList();
            lock (_triggerLock)
            {
                for (int i = 0; i < _triggers.Count; i++)
                {
                    TriggerWrapper tw = _triggers[i] as TriggerWrapper;
                    if (tw.JobName.Equals(jobName))
                    {
                        trigList.Add(tw);
                    }
                }
            }
            return trigList;
        }

        #region ISchedulingService Members

        public void Start(ConfigurationSection configuration)
        {
            //TODO
        }

        public void Shutdown()
        {
            //Signal the main processing loop to pause at the next possible point
            lock (_pauseLock)
            {
                _halted = true;

                if (_paused)
                {
                    Monitor.Pulse(_pauseLock);
                }
                else
                {
                    SignalSchedulingChange();
                }
            }
            // Scheduler thread may have be waiting for the fire time of an acquired 
            // trigger and need time to release the trigger once halted, so make sure
            // the thread is dead before continuing to shutdown the job store.
            try
            {
                _thread.Join();
            }
            catch (Exception)
            {
            }
        
        }

        public string AddScheduledJob(int interval, IScheduledJob job)
        {
            return AddScheduledJob(interval, Trigger.RepeatIndefinitely, job);
        }

        public string AddScheduledJob(int interval, int repeatCount, IScheduledJob job)
        {
            // Create trigger that fires indefinitely every <interval> milliseconds
            Trigger trigger = new Trigger("Trigger_" + job.Name, null, DateTime.UtcNow, null, repeatCount, interval);
            ScheduleJob(job, trigger);
            return job.Name;
        }

        public string AddScheduledOnceJob(long timeDelta, IScheduledJob job)
        {
            DateTime startUtc = DateTime.UtcNow;
            startUtc = startUtc.AddMilliseconds(timeDelta);
            return AddScheduledOnceJob(startUtc, job);
        }

        public string AddScheduledOnceJob(DateTime date, IScheduledJob job)
        {
            Trigger trigger = new Trigger("Trigger_" + job.Name, null, date);
            ScheduleJob(job, trigger);
            return job.Name;
        }

        public DateTime ScheduleJob(IScheduledJob job, Trigger trigger)
        {
            if (job == null)
                throw new SchedulerException("Job cannot be null", SchedulerException.ErrorClientError);
            if (trigger == null)
                throw new SchedulerException("Trigger cannot be null", SchedulerException.ErrorClientError);
            if (trigger.JobName == null)
                trigger.JobName = job.Name;
            NullableDateTime ft = trigger.ComputeFirstFireTimeUtc();
            if (!ft.HasValue)
                throw new SchedulerException("Based on configured schedule, the given trigger will never fire.", SchedulerException.ErrorClientError);
            StoreJobAndTrigger(job, trigger);
            NotifySchedulerThread();
            return ft.Value;
        }

        /// <summary>
        /// Remove (delete) the <see cref="IScheduledJob" /> with the given and any <see cref="Trigger" /> s that reference it.
        /// </summary>
        /// <param name="jobName">The name of the job to stop.</param>
        public bool RemoveScheduledJob(string jobName)
        {
            bool found = false;
            Trigger[] triggers = GetTriggersForJob(jobName);
            for (int i = 0; i < triggers.Length; i++)
            {
                Trigger trigger = triggers[i];
                RemoveTrigger(trigger.Name);
                found = true;
            }
            lock (_jobLock)
            {
                object tempObject = _jobsDictionary[jobName];
                _jobsDictionary.Remove(jobName);
                found = (tempObject != null) | found;
            }
            return found;
        }

        public string[] GetScheduledJobNames()
        {
		    ArrayList result = new ArrayList();
            lock (_jobLock)
            {
                foreach (string name in _jobsDictionary.Keys)
                    result.Add(name);
            }
		    return (string[])result.ToArray(typeof(string));
        }

        #endregion

        /// <summary>
        /// Notifies the scheduler thread.
        /// </summary>
        protected internal virtual void NotifySchedulerThread()
        {
            SignalSchedulingChange();
        }
    }

    #region Helper Classes

    internal class JobWrapper
    {
        private IScheduledJob _job;

        public IScheduledJob Job
        {
            get { return _job; }
            set { _job = value; }
        }

        internal JobWrapper(IScheduledJob job)
        {
            _job = job;
        }

        public string Name { get { return _job.Name; } }


        public override bool Equals(object obj)
        {
            if (obj is JobWrapper)
            {
                JobWrapper jw = (JobWrapper)obj;
                if (jw._job.Equals(_job))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _job.Name.GetHashCode();
        }
    }

    /// <summary>
    /// Helper wrapper class.
    /// </summary>
    [CLSCompliant(false)]
    public class TriggerWrapper
    {
        /// <summary>
        /// The trigger
        /// </summary>
        public Trigger _trigger;

        /// <summary>
        /// Current state
        /// </summary>
        private InternalTriggerState _state = InternalTriggerState.Waiting;

        internal InternalTriggerState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public string Name
        {
            get { return _trigger.Name; }
        }

        public Trigger Trigger
        {
            get { return _trigger; }
        }

        internal TriggerWrapper(Trigger trigger)
        {
            _trigger = trigger;
        }

        public string JobName { get { return _trigger.JobName; } }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is TriggerWrapper)
            {
                TriggerWrapper tw = (TriggerWrapper)obj;
                if (tw.Name.Equals(this.Name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _trigger.Name.GetHashCode();
        }
    }

    /// <summary>
    /// Comparer for triggers.
    /// </summary>
    internal class TriggerComparator : IComparer
    {
        public virtual int Compare(object obj1, object obj2)
        {
            TriggerWrapper trig1 = (TriggerWrapper)obj1;
            TriggerWrapper trig2 = (TriggerWrapper)obj2;

            int comp = trig1.Trigger.CompareTo(trig2.Trigger);
            if (comp != 0)
            {
                return comp;
            }

            comp = trig2.Trigger.Priority - trig1.Trigger.Priority;
            if (comp != 0)
            {
                return comp;
            }

            return trig1.Trigger.Name.CompareTo(trig2.Trigger.Name);
        }


        public override bool Equals(object obj)
        {
            return (obj is TriggerComparator);
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    #endregion Helper Classes
}
