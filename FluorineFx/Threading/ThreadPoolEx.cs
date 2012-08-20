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
using System.Threading;
using System.Collections;
using System.Net;
using System.Security;
using FluorineFx.Collections;
using FluorineFx.Configuration;

namespace FluorineFx.Threading
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class ThreadPoolEx  : IDisposable
	{
		#region Constants
		/// <summary>
		/// Default minimum number of threads the thread pool contains. (0)
		/// </summary>
		public const int DefaultMinWorkerThreads = 0;
		/// <summary>
		/// Default maximum number of threads the thread pool contains.
		/// </summary>
		public const int DefaultMaxWorkerThreads = 200;
		/// <summary>
		/// Default idle timeout in milliseconds. (One minute)
		/// </summary>
		public const int DefaultIdleTimeout = 60*1000; // One minute
		/// <summary>
		/// The default is not to use the performance counters.
		/// </summary>
		public static readonly string DefaultPerformanceCounterInstanceName = null;
		/// <summary>
		/// The default is to work on work items as soon as they arrive.
		/// </summary>
		public const bool DefaultStartSuspended = false;
		/// <summary>
		/// The default thread priority.
		/// </summary>
		public const ThreadPriority DefaultThreadPriority = ThreadPriority.Normal;
		#endregion Constants

		/// <summary>
		/// Start information to use. 
		/// </summary>
		private ThreadPoolStartInfo _threadPoolStartInfo = new ThreadPoolStartInfo();
		/// <summary>
		/// Queue of work items.
		/// </summary>
		private WorkItemsQueue _workItemsQueue = new WorkItemsQueue();
		/// <summary>
		/// Hashtable of all the threads in the thread pool.
		/// </summary>
		//private Hashtable _workerThreads = Hashtable.Synchronized(new Hashtable());
        private SynchronizedHashtable _workerThreads = new SynchronizedHashtable();
		/// <summary>
		/// Number of running threads(not idle).
		/// </summary>
		private int _inUseWorkerThreads = 0;
		/// <summary>
		/// Signaled when the thread pool is idle.
		/// </summary>
		private ManualResetEvent _isIdleWaitHandle = new ManualResetEvent(true);
		/// <summary>
		/// An event to signal all the threads to quit immediately.
		/// </summary>
		private ManualResetEvent _shuttingDownEvent = new ManualResetEvent(false);
		/// <summary>
		/// A flag to indicate the threads to quit.
		/// </summary>
		private bool _shutdown = false;
		/// <summary>
		/// Counts the threads created in the pool.
		/// </summary>
		private int _threadCounter = 0;
		/// <summary>
		/// Indicate that the object has been disposed.
		/// </summary>
		private bool _isDisposed = false;
		/// <summary>
		/// Processed work items counter.
		/// </summary>
		private long _workItemsProcessed = 0;
		/// <summary>
		/// Total number of work items that are stored in the work items queue 
		/// plus the work items that the threads in the pool are working on.
		/// </summary>
		private int _currentWorkItemsCount = 0;
		/// <summary>
		/// Contains the name of this instance of ThreadPoolEx.
		/// </summary>
		private string _name = "ThreadPoolEx";

		/// <summary>
		/// A reference from each thread in the thread pool to its ThreadPoolEx object container.
		/// </summary>
		[ThreadStatic]
		private static ThreadPoolEx _threadPool;
		/// <summary>
		/// A reference to the current work item a thread from the thread pool is executing.
		/// </summary>
		[ThreadStatic]
		private static WorkItem _currentWorkItem;		
		
		/// <summary>
		/// Initializes a new instance of the ThreadPoolEx class.
		/// </summary>
		public ThreadPoolEx()
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the ThreadPoolEx class.
		/// </summary>
		/// <param name="idleTimeout">Idle timeout in milliseconds.</param>
		public ThreadPoolEx(int idleTimeout)
		{
			_threadPoolStartInfo.IdleTimeout = idleTimeout;
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the ThreadPoolEx class.
		/// </summary>
		/// <param name="idleTimeout">Idle timeout in milliseconds.</param>
		/// <param name="maxWorkerThreads">Upper limit of threads in the pool.</param>
		public ThreadPoolEx(int idleTimeout, int maxWorkerThreads)
		{
			_threadPoolStartInfo.IdleTimeout = idleTimeout;
			_threadPoolStartInfo.MaxWorkerThreads = maxWorkerThreads;
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the ThreadPoolEx class.
		/// </summary>
		/// <param name="idleTimeout">Idle timeout in milliseconds.</param>
		/// <param name="maxWorkerThreads">Upper limit of threads in the pool.</param>
		/// <param name="minWorkerThreads">Lower limit of threads in the pool.</param>
		public ThreadPoolEx(int idleTimeout, int maxWorkerThreads, int minWorkerThreads)
		{
			_threadPoolStartInfo.IdleTimeout = idleTimeout;
			_threadPoolStartInfo.MaxWorkerThreads = maxWorkerThreads;
			_threadPoolStartInfo.MinWorkerThreads = minWorkerThreads;
			Initialize();
		}

		#region IDisposable Members

        /// <summary>
        /// Frees the resources of the current ThreadPoolEx object before it is reclaimed by the garbage collector.
        /// </summary>
		~ThreadPoolEx()
		{
			Dispose();
		}
        /// <summary>
        /// Releases resources used by the ThreadPoolEx object.
        /// </summary>
		public void Dispose()
		{
			if (!_isDisposed)
			{
				if (!_shutdown)
					Shutdown();

				if (null != _shuttingDownEvent)
				{
					_shuttingDownEvent.Close();
					_shuttingDownEvent = null;
				}
				_workerThreads.Clear();
				_isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		private void CheckDisposed()
		{
			if(_isDisposed)
				throw new ObjectDisposedException(GetType().ToString());
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the ThreadPool instance
		/// </summary>
		public string Name 
		{ 
			get{ return _name; }
			set{ _name = value; }
		}

		/// <summary>
		/// Get the number of running (not idle) threads in the thread pool.
		/// </summary>
		public int InUseThreads 
		{ 
			get 
			{ 
				CheckDisposed();
				return _inUseWorkerThreads; 
			} 
		}
        /// <summary>
        /// Gets the number of available threads in the pool.
        /// </summary>
        public int AvailableThreads
        {
            get { return _threadPoolStartInfo.MaxWorkerThreads - _inUseWorkerThreads; }
        }

		/// <summary>
		/// Get the number of work items in the queue.
		/// </summary>
		public int WaitingCallbacks 
		{ 
			get 
			{ 
				CheckDisposed();
				return _workItemsQueue.Count;
			} 
		}

		#endregion Properties

		private void Initialize()
		{
			ValidateStartInfo();
			StartOptimalNumberOfThreads();
		}

		private void ValidateStartInfo()
		{
			if (_threadPoolStartInfo.MinWorkerThreads < 0)
				throw new ArgumentOutOfRangeException("MinWorkerThreads", "MinWorkerThreads cannot be negative");
			if (_threadPoolStartInfo.MaxWorkerThreads <= 0)
				throw new ArgumentOutOfRangeException("MaxWorkerThreads", "MaxWorkerThreads must be greater than zero");
			if (_threadPoolStartInfo.MinWorkerThreads > _threadPoolStartInfo.MaxWorkerThreads)
				throw new ArgumentOutOfRangeException("MinWorkerThreads, maxWorkerThreads",  "MaxWorkerThreads must be greater or equal to MinWorkerThreads");
		}

		private void StartOptimalNumberOfThreads()
		{
			int threadsCount = Math.Max(_workItemsQueue.Count, _threadPoolStartInfo.MinWorkerThreads);
			threadsCount = Math.Min(threadsCount, _threadPoolStartInfo.MaxWorkerThreads);
			StartThreads(threadsCount);
		}

		/// <summary>
		/// Starts new threads.
		/// </summary>
		/// <param name="threadsCount">The number of threads to start.</param>
		private void StartThreads(int threadsCount)
		{
			if (_threadPoolStartInfo.StartSuspended)
				return;

			lock(_workerThreads.SyncRoot)
			{
				// Don't start threads on shut down
				if (_shutdown)
					return;

				for(int i = 0; i < threadsCount; ++i)
				{
					// Upper limit
					if (_workerThreads.Count >= _threadPoolStartInfo.MaxWorkerThreads)
						return;
					// Create a new thread
					Thread workerThread = new Thread(new ThreadStart(ProcessQueuedItems));
					// Configure the new thread and start it
					workerThread.Name = _name + " Thread #" + _threadCounter;
					workerThread.IsBackground = true;
					workerThread.Priority = ThreadPriority.Normal;
					workerThread.Start();
					++_threadCounter;
					// Add the new thread to the hashtable and update its creation time.
					_workerThreads[workerThread] = DateTime.Now;
				}
			}
		}
		
		private void IncrementWorkItemsCount()
		{
			int count = Interlocked.Increment(ref _currentWorkItemsCount);
			if (count == 1) 
				_isIdleWaitHandle.Reset();
		}

		private void DecrementWorkItemsCount()
		{
			++_workItemsProcessed;
			int count = Interlocked.Decrement(ref _currentWorkItemsCount);
			if (count == 0) 
				_isIdleWaitHandle.Set();
		}

		/// <summary>
		/// Waits on the queue for a work item, shutdown, or timeout.
		/// </summary>
		/// <returns>
		/// Returns the WaitingCallback or null in case of timeout or shutdown.
		/// </returns>
		private WorkItem Dequeue()
		{
			WorkItem workItem = _workItemsQueue.DequeueWorkItem(_threadPoolStartInfo.IdleTimeout, _shuttingDownEvent);
			return workItem;
		}

		/// <summary>
		/// Adds a new work item to the queue.
		/// </summary>
		/// <param name="workItem">The work item to queue.</param>
		private void Enqueue(WorkItem workItem)
		{
			Enqueue(workItem, true);
		}
		/// <summary>
		/// Adds a new work item to the queue.
		/// </summary>
		/// <param name="workItem">The work item to queue.</param>
		/// <param name="incrementWorkItems"></param>
		internal void Enqueue(WorkItem workItem, bool incrementWorkItems)
		{
			if (incrementWorkItems)
				IncrementWorkItemsCount();

			_workItemsQueue.EnqueueWorkItem(workItem);
			workItem.SetQueueTime();
			// If all the threads are busy then try to create a new one
			if ((InUseThreads + WaitingCallbacks) > _workerThreads.Count) 
			{
				StartThreads(1);
			}
		}

		/// <summary>
		/// A worker thread method that processes work items from the work items queue.
		/// </summary>
		private void ProcessQueuedItems()
		{
			// Initialize the _threadPool variable
			_threadPool = this;

			try
			{
				bool inUseWorkerThreadsWasIncremented = false;

				// Process until shutdown.
				while(!_shutdown)
				{
					// Update the last time this thread was alive.
					_workerThreads[Thread.CurrentThread] = DateTime.Now;
					// Wait for a work item, shutdown, or timeout
					WorkItem workItem = Dequeue();
					// Update the last time this thread was alive.
					_workerThreads[Thread.CurrentThread] = DateTime.Now;
					// On timeout or shut down.
					if (workItem == null)
					{
						// Double lock for quit.
						if (_workerThreads.Count > _threadPoolStartInfo.MinWorkerThreads)
						{
							lock(_workerThreads.SyncRoot)
							{
								if (_workerThreads.Count > _threadPoolStartInfo.MinWorkerThreads)
								{
									//Quit
									if (_workerThreads.Contains(Thread.CurrentThread))
									{
										_workerThreads.Remove(Thread.CurrentThread);
									}
									break;
								}
							}
						}
					}
					// If we didn't quit then skip to the next iteration.
					if (workItem == null)
						continue;

					try 
					{
						// Initialize the value to false
						inUseWorkerThreadsWasIncremented = false;

						// Change the state of the work item to 'in progress' if possible.
						// The cancel mechanism doesn't delete items from the queue,  it marks the work item as canceled, 
						// and when the work item is dequeued, we just skip it.
						if (!workItem.StartingWorkItem())
							continue;
						// Execute the callback.  Make sure to accurately record how many callbacks are currently executing.
						int inUseWorkerThreads = Interlocked.Increment(ref _inUseWorkerThreads);
						// Mark that the _inUseWorkerThreads incremented, so in the finally{} statement we will decrement it correctly.
						inUseWorkerThreadsWasIncremented = true;
						// Set the _currentWorkItem to the current work item
						_currentWorkItem = workItem;
						ExecuteWorkItem(workItem);
					}
					catch(Exception)
					{
						// Do nothing
					}
					finally
					{
						if (workItem != null)
							workItem.DisposeState();
						_currentWorkItem = null;
						// Decrement the _inUseWorkerThreads only if we had  incremented it.
						if (inUseWorkerThreadsWasIncremented)
						{
							int inUseWorkerThreads = Interlocked.Decrement(ref _inUseWorkerThreads);
						}
						// Decrement the number of work items here so the idle ManualResetEvent won't fluctuate.
						DecrementWorkItemsCount();
					}
				}
			} 
			catch(ThreadAbortException)
			{
				// Handle the abort exception gracfully.
				Thread.ResetAbort();
			}
			catch(Exception)
			{
			}
			finally
			{
				if (_workerThreads.Contains(Thread.CurrentThread))
					_workerThreads.Remove(Thread.CurrentThread);
			}
		}

		private void ExecuteWorkItem(WorkItem workItem)
		{
			try
			{
				workItem.Execute();
			}
			catch
			{
				throw;
			}
			finally
			{
			}
		}

		/// <summary>
		/// Forces the ThreadPool to shutdown.
		/// </summary>
		public void Shutdown()
		{
			Shutdown(true, 0);
		}
        /// <summary>
        /// Forces the ThreadPool to shutdown.
        /// </summary>
        /// <param name="forceAbort"></param>
        /// <param name="timeout"></param>
		public void Shutdown(bool forceAbort, TimeSpan timeout)
		{
			Shutdown(forceAbort, (int)timeout.TotalMilliseconds);
		}

		/// <summary>
		/// Empty the queue of work items and abort the threads in the pool.
		/// </summary>
		public void Shutdown(bool forceAbort, int millisecondsTimeout)
		{
			CheckDisposed();

			Thread [] threads = null;
			lock(_workerThreads.SyncRoot)
			{
				// Shutdown the work items queue
				_workItemsQueue.Dispose();
				// Signal the threads to exit
				_shutdown = true;
				_shuttingDownEvent.Set();
				// Make a copy of the threads' references in the pool
				threads = new Thread [_workerThreads.Count];
				_workerThreads.Keys.CopyTo(threads, 0);
			}

			int millisecondsLeft = millisecondsTimeout;
			DateTime start = DateTime.Now;
			bool waitInfinitely = (Timeout.Infinite == millisecondsTimeout);
			bool timeout = false;

			// At each iteration we update the time left for the timeout.
			foreach(Thread thread in threads)
			{
				// Join does not work with negative numbers
				if (!waitInfinitely && (millisecondsLeft < 0))
				{
					timeout = true;
					break;
				}
				// Wait for the thread to terminate
				bool success = thread.Join(millisecondsLeft);
				if(!success)
				{
					timeout = true;
					break;
				}
				if(!waitInfinitely)
				{
					// Update the time left to wait
					TimeSpan ts = DateTime.Now - start;
					millisecondsLeft = millisecondsTimeout - (int)ts.TotalMilliseconds;
				}
			}

			if (timeout && forceAbort)
			{
				// Abort the threads in the pool
				foreach(Thread thread in threads)
				{
					if ((thread != null) && thread.IsAlive) 
					{
						try 
						{
							thread.Abort("Shutdown");
						}
						catch(SecurityException)
						{
						}
						catch(ThreadStateException)
						{
						}
					}
				}
			}
		}

		/// <summary>
		/// Queues a user work item to the thread pool.
		/// </summary>
		/// <param name="callback">A WaitCallback representing the delegate to invoke when the thread in the thread pool picks up the work item.</param>
		/// <param name="state">The object that is passed to the delegate when serviced from the thread pool.</param>
		/// <returns></returns>
		public void QueueUserWorkItem(WaitCallback callback, object state)
		{
			CheckDisposed();
			WorkItem workItem = new WorkItem(callback, state);
			Enqueue(workItem);
			//return workItem.GetWorkItemResult();
		}


		internal static void LoopSleep(ref int loopIndex)
        {
#if !(NET_1_1)
            if (Environment.ProcessorCount == 1 || (++loopIndex % (Environment.ProcessorCount * 50)) == 0)
            {
                //Single-core
                //Switch to another running thread
                Thread.Sleep(5);
            }
            else
            {
                //Multi-core / HT
                //Loop n iterations!
                Thread.SpinWait(20);
            }
#else
			Thread.Sleep(5);
#endif
		}

        private static ThreadPoolEx GlobalThreadPool;

        /// <summary>
        /// Gets the global ThreadPoolEx instance.
        /// </summary>
        public static ThreadPoolEx Global
        {
            get
            {
                if (GlobalThreadPool == null)
                {
                    lock (typeof(ThreadPoolEx))
                    {
                        if (GlobalThreadPool == null)
                        {
                            GlobalThreadPool = new ThreadPoolEx(DefaultIdleTimeout, FluorineConfiguration.Instance.FluorineSettings.Runtime.MaxWorkerThreads, FluorineConfiguration.Instance.FluorineSettings.Runtime.MinWorkerThreads);
                        }
                    }
                }
                return GlobalThreadPool;
            }
        }
	}
}
