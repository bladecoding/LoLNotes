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

//Jeffrey Richter - Concurrent Affairs

namespace FluorineFx.Util
{
    internal class AsyncResultNoResult : IAsyncResult
    {
        // Fields set at construction which never change while operation is pending
        private readonly AsyncCallback _asyncCallback;
        private readonly Object _asyncState;

        // Fields set at construction which do change after operation completes
        private const Int32 StatePending = 0;
        private const Int32 StateCompletedSynchronously = 1;
        private const Int32 StateCompletedAsynchronously = 2;
        private Int32 _completedState = StatePending;

        // Field that may or may not get set depending on usage
#if NET_1_1
		private object _asyncWaitHandle;
#else
        private ManualResetEvent _asyncWaitHandle;
#endif

        // Fields set when operation completes
        private Exception _exception;

        public AsyncResultNoResult(AsyncCallback asyncCallback, Object state)
        {
            _asyncCallback = asyncCallback;
            _asyncState = state;
        }

        public void SetAsCompleted(Exception exception, Boolean completedSynchronously)
        {
            // Passing null for exception means no error occurred. 
            // This is the common case
            _exception = exception;

            // The m_CompletedState field MUST be set prior calling the callback
            Int32 prevState = Interlocked.Exchange(ref _completedState, completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously);
            if (prevState != StatePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (_asyncWaitHandle != null) 
#if NET_1_1
				(_asyncWaitHandle as ManualResetEvent).Set();
#else
				_asyncWaitHandle.Set();
#endif

            // If a callback method was set, call it
            if (_asyncCallback != null) _asyncCallback(this);
        }

        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke 
            // for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                _asyncWaitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occured, throw it
            if (_exception != null) throw _exception;
        }

        #region Implementation of IAsyncResult

        public Object AsyncState { get { return _asyncState; } }

        public Boolean CompletedSynchronously
        {
            get
            {
#if SILVERLIGHT
                return _completedState == StateCompletedSynchronously;
#else
                return Thread.VolatileRead(ref _completedState) == StateCompletedSynchronously;
#endif
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitHandle == null)
                {
                    Boolean done = IsCompleted;
                    ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref _asyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose 
                        // the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
#if NET_1_1
							(_asyncWaitHandle as ManualResetEvent).Set();
#else
							_asyncWaitHandle.Set();
#endif
                        }
                    }
                }
#if NET_1_1
				return _asyncWaitHandle as ManualResetEvent;
#else
				return _asyncWaitHandle;
#endif
			}
        }

        public Boolean IsCompleted
        {
            get
            {
#if SILVERLIGHT
                return _completedState != StatePending;
#else
                return Thread.VolatileRead(ref _completedState) != StatePending;
#endif
            }
        }
        #endregion
    }
}
