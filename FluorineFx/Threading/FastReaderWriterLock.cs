using System;
using System.Diagnostics;
using System.Threading;

namespace FluorineFx.Threading
{
    /// <summary>
    /// A reader-writer lock implementation that is intended to be simple, yet very
    /// efficient.  In particular only 1 interlocked operation is taken for any lock 
    /// operation (we use spin locks to achieve this).  The spin lock is never held
    /// for more than a few instructions (in particular, we never call event APIs
    /// or in fact any non-trivial API while holding the spin lock).   
    /// 
    /// This class was derived from the ReaderWriterLock presented by Vance Morrison.
    /// </summary>
    public sealed class FastReaderWriterLock
    {
        // Lock specifiation for myLock:  This lock protects exactly the local fields associted
        // instance of MyReaderWriterLock.  It does NOT protect the memory associated with the
        // the events that hang off this lock (eg writeEvent, readEvent upgradeEvent).
        int _lock;

        // Who owns the lock owners > 0 => readers
        // owners = -1 means there is one writer.
        int _owners;

        // These variables allow use to avoid Setting events (which is expensive) if we don't have to. 
        uint _numWriteWaiters;        // maximum number of threads that can be doing a WaitOne on the writeEvent 
        uint _numReadWaiters;         // maximum number of threads that can be doing a WaitOne on the readEvent
        uint _numUpgradeWaiters;      // maximum number of threads that can be doing a WaitOne on the upgradeEvent (at most 1). 

        // conditions we wait on. 
        EventWaitHandle _writeEvent;    // threads waiting to aquire a write lock go here.
        EventWaitHandle _readEvent;     // threads waiting to aquire a read lock go here (will be released in bulk)
        EventWaitHandle _upgradeEvent;  // thread waiting to upgrade a read lock to a write lock go here (at most one)

        int _owningThreadId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastReaderWriterLock"/> class.
        /// </summary>
        public FastReaderWriterLock()
        {
            // All state can start out zeroed. 
        }

        /// <summary>
        /// Acquires a reader lock.
        /// </summary>
        public void AcquireReaderLock()
        {
            AcquireReaderLock(Timeout.Infinite);
        }

        /// <summary>
        /// Acquires a reader lock.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
        public void AcquireReaderLock(int millisecondsTimeout)
        {
            EnterLock();
            for (; ; )
            {
                // We can enter a read lock if there are only read-locks have been given out
                // and a writer is not trying to get in.  
                if (_owners >= 0 && _numWriteWaiters == 0)
                {
                    // Good case, there is no contention, we are basically done
                    _owners++;       // Indicate we have another reader
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait.  
                if (_readEvent == null)      // Create the needed event 
                {
                    LazyCreateEvent(ref _readEvent, false);
                    continue;   // since we left the lock, start over. 
                }

                WaitOnEvent(_readEvent, ref _numReadWaiters, millisecondsTimeout);
            }
            ExitLock();
        }

        /// <summary>
        /// Acquires a writer lock.
        /// </summary>
        public void AcquireWriterLock()
        {
            AcquireWriterLock(Timeout.Infinite);
        }

        /// <summary>
        /// Acquires a writer lock.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
        public void AcquireWriterLock(int millisecondsTimeout)
        {
            EnterLock();
            for (; ; )
            {
                if (_owners == 0)
                {
                    // Good case, there is no contention, we are basically done
                    _owners = -1;    // indicate we have a writer.
                    _owningThreadId = Thread.CurrentThread.ManagedThreadId;
                    break;
                }
                else if ((_owners < 0) && (_owningThreadId == Thread.CurrentThread.ManagedThreadId))
                {
                    // Recursive lock
                    _owners--;
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait.
                if (_writeEvent == null)     // create the needed event.
                {
                    LazyCreateEvent(ref _writeEvent, true);
                    continue;   // since we left the lock, start over. 
                }

                WaitOnEvent(_writeEvent, ref _numWriteWaiters, millisecondsTimeout);
            }
            ExitLock();
        }

        /// <summary>
        /// Upgrades to writer lock.
        /// </summary>
        public void UpgradeToWriterLock()
        {
            UpgradeToWriterLock(Timeout.Infinite);
        }

        /// <summary>
        /// Upgrades to writer lock.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
        public void UpgradeToWriterLock(int millisecondsTimeout)
        {
            EnterLock();
            for (; ; )
            {
                Debug.Assert(_owners > 0, "Upgrading when no reader lock held");
                if (_owners == 1)
                {
                    // Good case, there is no contention, we are basically done
                    _owners = -1;    // indicate we have a writer. 
                    _owningThreadId = Thread.CurrentThread.ManagedThreadId;
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait. 
                if (_upgradeEvent == null)   // Create the needed event
                {
                    LazyCreateEvent(ref _upgradeEvent, false);
                    continue;   // since we left the lock, start over. 
                }

                if (_numUpgradeWaiters > 0)
                {
                    ExitLock();
                    throw new Exception("UpgradeToWriterLock already in process.  Deadlock!");
                }

                WaitOnEvent(_upgradeEvent, ref _numUpgradeWaiters, millisecondsTimeout);
            }
            ExitLock();
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            EnterLock();
            Debug.Assert(_owners > 0, "ReleasingReaderLock: releasing lock and no read lock taken");
            --_owners;
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
            EnterLock();
            Debug.Assert(_owningThreadId == Thread.CurrentThread.ManagedThreadId, "Calling ReleaseWriterLock from wrong thread");
            //Debug.Assert(_owners == -1, "Calling ReleaseWriterLock when no write lock is held");
            Debug.Assert(_owners < 0, "Calling ReleaseWriterLock when no write lock is held");
            if (++_owners == 0)
            {
                _owningThreadId = 0;
            }
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>
        /// Downgrades to reader lock.
        /// </summary>
        public void DowngradeToReaderLock()
        {
            EnterLock();
            Debug.Assert(_owningThreadId == Thread.CurrentThread.ManagedThreadId, "Calling DowngradeToReaderLock from wrong thread");
            //Debug.Assert(_owners == -1, "Downgrading when no writer lock held");
            Debug.Assert(_owners < 0, "Downgrading when no writer lock held");
            //_owners = 1;
            if (++_owners == 0)
            {
                _owningThreadId = 0;
                _owners = 1;
            }
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>
        /// A routine for lazily creating a event outside the lock (so if errors
        /// happen they are outside the lock and that we don't do much work
        /// while holding a spin lock).  If all goes well, reenter the lock and
        /// set 'waitEvent' 
        /// </summary>
        private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
        {
            Debug.Assert(IsLockHeld);
            Debug.Assert(waitEvent == null);

            ExitLock();
            EventWaitHandle newEvent;
            if (makeAutoResetEvent)
                newEvent = new AutoResetEvent(false);
            else
                newEvent = new ManualResetEvent(false);
            EnterLock();
            if (waitEvent == null)          // maybe someone snuck in. 
                waitEvent = newEvent;
        }

        /// <summary>
        /// Waits on 'waitEvent' with a timeout of 'millisceondsTimeout.  
        /// Before the wait 'numWaiters' is incremented and is restored before leaving this routine.
        /// </summary>
        private void WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
        {
            Debug.Assert(IsLockHeld);

            waitEvent.Reset();
            numWaiters++;

            bool waitSuccessful = false;
            ExitLock();      // Do the wait outside of any lock 
            try
            {
#if SILVERLIGHT
                if (!waitEvent.WaitOne(millisecondsTimeout))
                    throw new Exception("ReaderWriterLock timeout expired");
#else
                if (!waitEvent.WaitOne(millisecondsTimeout, false))
                    throw new Exception("ReaderWriterLock timeout expired");
#endif
                waitSuccessful = true;
            }
            finally
            {
                EnterLock();
                --numWaiters;
                if (!waitSuccessful)        // We are going to throw for some reason.  Exit myLock. 
                    ExitLock();
            }
        }

        /// <summary>
        /// Determines the appropriate events to set, leaves the locks, and sets the events. 
        /// </summary>
        private void ExitAndWakeUpAppropriateWaiters()
        {
            Debug.Assert(IsLockHeld);

            if (_owners == 0 && _numWriteWaiters > 0)
            {
                ExitLock();      // Exit before signaling to improve efficiency (wakee will need the lock)
                _writeEvent.Set();   // release one writer. 
            }
            else if (_owners == 1 && _numUpgradeWaiters != 0)
            {
                ExitLock();          // Exit before signaling to improve efficiency (wakee will need the lock)
                _upgradeEvent.Set();     // release all upgraders (however there can be at most one). 
                // two threads upgrading is a guarenteed deadlock, so we throw in that case. 
            }
            else if (_owners >= 0 && _numReadWaiters != 0)
            {
                ExitLock();    // Exit before signaling to improve efficiency (wakee will need the lock)
                _readEvent.Set();  // release all readers. 
            }
            else
                ExitLock();
        }

        private void EnterLock()
        {
            if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                EnterLockSpin();
        }

        private void EnterLockSpin()
        {
            for (int i = 0; ; i++)
            {
#if !SILVERLIGHT
                if (i < 3 && Environment.ProcessorCount > 1)
                    Thread.SpinWait(20);    // Wait a few dozen instructions to let another processor release lock. 
                else
                    Thread.Sleep(0);        // Give up my quantum.  
#else
                //Environment.ProcessorCount EnvironmentPermission required (SecurityCritical in Silverlight)
                Thread.Sleep(0);
#endif
                if (Interlocked.CompareExchange(ref _lock, 1, 0) == 0)
                    return;
            }
        }
        private void ExitLock()
        {
            Debug.Assert(_lock != 0, "Exiting spin lock that is not held");
            _lock = 0;
        }

        private bool IsLockHeld { get { return _lock != 0; } }

        #region Helpers

        /// <summary>
        /// Gets the read-side lockable.
        /// </summary>
        public ILockable ReadLock
        {
            get { return new ReadLockable(this); }
        }

        /// <summary>
        /// Gets the write-side lockable.
        /// </summary>
        public ILockable WriteLock
        {
            get { return new WriteLockable(this); }
        }

        /// <summary>
        /// Lockable object that controls the read-side of the FastReaderWriterLock
        /// </summary>
        internal class ReadLockable : ILockable
        {
            private readonly FastReaderWriterLock _lockObj;

            public IDisposable Acquire()
            {
                return new ReaderLock(_lockObj);
            }

            internal ReadLockable(FastReaderWriterLock lockObj)
            {
                _lockObj = lockObj;
            }
        }

        /// <summary>
        /// Lockable object that controls the write-side of the FastReaderWriterLock
        /// </summary>
        internal class WriteLockable : ILockable
        {
            private readonly FastReaderWriterLock _lockObj;

            public IDisposable Acquire()
            {
                return new WriterLock(_lockObj);
            }

            internal WriteLockable(FastReaderWriterLock lockObj)
            {
                _lockObj = lockObj;
            }
        }

        internal class ReaderLock : IDisposable
        {
            private readonly FastReaderWriterLock _lockObj;

            internal ReaderLock(FastReaderWriterLock lockObj)
            {
                _lockObj = lockObj;
                _lockObj.AcquireReaderLock();
            }

            #region IDisposable Members

            public void Dispose()
            {
                _lockObj.ReleaseReaderLock();
            }

            #endregion
        }

        internal class WriterLock : IDisposable
        {
            private readonly FastReaderWriterLock _lockObj;

            internal WriterLock(FastReaderWriterLock lockObj)
            {
                _lockObj = lockObj;
                _lockObj.AcquireWriterLock();
            }

            #region IDisposable Members

            public void Dispose()
            {
                _lockObj.ReleaseWriterLock();
            }

            #endregion
        }

        #endregion Helpers
    }

    /// <summary>
    /// Locking mechanism.
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// Acquires the lock; the lock is released when the disposable object that was returned is disposed.
        /// </summary>
        /// <returns>A disposable object wrapping the lock.</returns>
        IDisposable Acquire();
    }
}
