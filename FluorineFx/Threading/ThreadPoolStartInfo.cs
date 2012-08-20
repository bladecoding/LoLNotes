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

namespace FluorineFx.Threading
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class ThreadPoolStartInfo
	{
		/// <summary>
		/// Idle timeout in milliseconds.
		/// If a thread is idle for _idleTimeout milliseconds then 
		/// it may quit.
		/// </summary>
		private int _idleTimeout;
		/// <summary>
		/// The lower limit of threads in the pool.
		/// </summary>
		private int _minWorkerThreads;
		/// <summary>
		/// The upper limit of threads in the pool.
		/// </summary>
		private int _maxWorkerThreads;
		/// <summary>
		/// The priority of the threads in the pool
		/// </summary>
		private ThreadPriority _threadPriority;
		/// <summary>
		/// Start suspended until the Start() method is called.
		/// </summary>
		private bool _startSuspended;
		/// <summary>
		/// If this field is not null then the performance counters are enabled
		/// and use the string as the name of the instance.
		/// </summary>
		private string _perfCounterInstanceName;

		/// <summary>
		/// Initializes a new instance of the ThreadPoolStartInfo class.
		/// </summary>
		public ThreadPoolStartInfo()
		{
			_idleTimeout = ThreadPoolEx.DefaultIdleTimeout;
			_minWorkerThreads = ThreadPoolEx.DefaultMinWorkerThreads;
			_maxWorkerThreads = ThreadPoolEx.DefaultMaxWorkerThreads;
			_threadPriority = ThreadPoolEx.DefaultThreadPriority;
			_startSuspended = ThreadPoolEx.DefaultStartSuspended;
			_perfCounterInstanceName = ThreadPoolEx.DefaultPerformanceCounterInstanceName;
		}
		/// <summary>
		/// Initializes a new instance of the ThreadPoolStartInfo class.
		/// </summary>
		/// <param name="threadPoolStartInfo"></param>
        public ThreadPoolStartInfo(ThreadPoolStartInfo threadPoolStartInfo)
        {
            _idleTimeout = threadPoolStartInfo._idleTimeout;
            _minWorkerThreads = threadPoolStartInfo._minWorkerThreads;
            _maxWorkerThreads = threadPoolStartInfo._maxWorkerThreads;
			_threadPriority = threadPoolStartInfo._threadPriority;
			_perfCounterInstanceName = threadPoolStartInfo._perfCounterInstanceName;
			_startSuspended = threadPoolStartInfo._startSuspended;
		}
		/// <summary>
		/// Gets or sets the idle timeout value.
		/// Default value is 60 seconds.
		/// </summary>
		public int IdleTimeout
		{
			get { return _idleTimeout; }
			set { _idleTimeout = value; }
		}
		/// <summary>
		/// Gets or sets the minimum number of threads.
		/// Default value is 0.
		/// </summary>
		public int MinWorkerThreads
		{
			get { return _minWorkerThreads; }
			set { _minWorkerThreads = value; }
		}
		/// <summary>
		/// Gets or sets the maximum number of threads.
		/// Default value is 25.
		/// </summary>
		public int MaxWorkerThreads
		{
			get { return _maxWorkerThreads; }
			set { _maxWorkerThreads = value; }
		}

        /// <summary>
        /// Gets or sets the thread priority.
        /// </summary>
        /// <value>The thread priority.</value>
		public ThreadPriority ThreadPriority
		{
			get { return _threadPriority; }
			set { _threadPriority = value; }
		}

        /// <summary>
        /// Gets or sets the name of the performance counter instance.
        /// </summary>
        /// <value>The name of the performance counter instance.</value>
		public string PerformanceCounterInstanceName
		{
			get { return _perfCounterInstanceName; }
			set { _perfCounterInstanceName = value; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether start suspended until the Start() method is called.
        /// </summary>
        /// <value><c>true</c> if start suspended; otherwise, <c>false</c>.</value>
		public bool StartSuspended
		{
			get { return _startSuspended; }
			set { _startSuspended = value; }
		}
	}
}
