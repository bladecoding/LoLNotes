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

namespace FluorineFx.Threading
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class WorkItemsQueue : IDisposable
	{
		#region Member variables

		/// <summary>
		/// Waiters queue (implemented as stack).
		/// </summary>
		private WaitEntry _headWaitEntry = new WaitEntry();
		/// <summary>
		/// Waiters count
		/// </summary>
		private int _waitersCount = 0;
		/// <summary>
		/// Work items queue
		/// </summary>
		private Queue _workItems = new Queue();
		/// <summary>
		/// Indicate that work items are allowed to be queued
		/// </summary>
		private bool _isWorkItemsQueueActive = true;
		/// <summary>
		/// Each thread in the thread pool keeps its own wait entry.
		/// </summary>
		[ThreadStatic]
		private static WaitEntry _waitEntry;
		/// <summary>
		/// A flag that indicates if the WorkItemsQueue has been disposed.
		/// </summary>
		private bool _isDisposed = false;

		#endregion

		#region Public properties

		/// <summary>
		/// Returns the current number of work items in the queue.
		/// </summary>
		public int Count
		{
			get
			{
				lock(this)
				{
					CheckDisposed();
					return _workItems.Count;
				}
			}
		}

		/// <summary>
		/// Returns the current number of waiters
		/// </summary>
		public int WaitersCount
		{
			get
			{
				lock(this)
				{
					CheckDisposed();
					return _waitersCount;
				}
			}
		}


		#endregion

		#region Public methods

		/// <summary>
		/// Enqueue a work item to the queue.
		/// </summary>
		public bool EnqueueWorkItem(WorkItem workItem)
		{
			// A work item cannot be null, since null is used in the WaitForWorkItem() method to indicate timeout or cancel
            if (workItem == null)
				throw new ArgumentNullException("workItem");

			bool enqueue = true;

			// First check if there is a wait entry waiting for work item. During 
			// the check, timed out waiters are ignored. If there is no waiter then the work item is queued.
			lock(this)
			{
				CheckDisposed();

				if (!_isWorkItemsQueueActive)
					return false;

				while(_waitersCount > 0)
				{
					// Dequeue a waiter.
					WaitEntry waitEntry = PopWaiter();
					// Signal the waiter. On success break the loop
					if (waitEntry.Signal(workItem))
					{
						enqueue = false;
						break;
					}
				}

				if (enqueue)
				{
					// Enqueue the work item
					_workItems.Enqueue(workItem);
				}
			}
			return true;
		}


		/// <summary>
		/// Waits for a work item or exits on timeout or cancel
		/// </summary>
		/// <param name="millisecondsTimeout">Timeout in milliseconds</param>
		/// <param name="cancelEvent">Cancel wait handle</param>
		/// <returns>Returns true if the resource was granted</returns>
		public WorkItem DequeueWorkItem(int millisecondsTimeout, WaitHandle cancelEvent)
		{
			WaitEntry WaitEntry = null;
			WorkItem workItem = null;

			lock(this)
			{
				CheckDisposed();

				// If there are waiting work items then take one and return.
				if (_workItems.Count > 0)
				{
					workItem = _workItems.Dequeue() as WorkItem;
					return workItem;
				}
				else
				{
					// Get the wait entry for the waiters queue
					WaitEntry = GetThreadWaitEntry();
					// Put the waiter with the other waiters
					PushWaitEntry(WaitEntry);
				}
			}

			// Prepare array of wait handle for the WaitHandle.WaitAny()
			WaitHandle [] waitHandles = new WaitHandle [] { WaitEntry.WaitHandle, cancelEvent };

			// Wait for available resource, cancel event, or timeout.
			int index = WaitHandle.WaitAny(waitHandles, millisecondsTimeout, true);

			lock(this)
			{
				// Got a work item.
				bool success = (0 == index);

				// On timeout update the WaitEntry that it is timed out
                if (!success)
				{
					// The Timeout() fails if the waiter has already been signaled
					bool timeout = WaitEntry.Timeout();
					// On timeout remove the WaitEntry from the queue.
					if(timeout)
						RemoveWaiter(WaitEntry, false);
					success = !timeout;
				}

				// On success return the work item
				if (success)
				{
					workItem = WaitEntry.WorkItem;
					if (workItem == null)
						workItem = _workItems.Dequeue() as WorkItem;
				}
			}
			return workItem;
		}

		/// <summary>
		/// Cleanup the work items queue, hence no more work items are allowed to be queued.
		/// </summary>
		private void Cleanup()
		{
			lock(this)
			{
				// Deactivate only once
				if (!_isWorkItemsQueueActive)
					return;
				// Don't queue more work items
				_isWorkItemsQueueActive = false;

				foreach(WorkItem workItem in _workItems)
				{
					workItem.DisposeState();
				}
				// Clear the work items that are already queued
				_workItems.Clear();
				while(_waitersCount > 0)
				{
					WaitEntry WaitEntry = PopWaiter();
					WaitEntry.Timeout();
				}
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Returns the WaitEntry of the current thread
		/// </summary>
		/// <returns></returns>
		/// In order to avoid creation and destuction of WaitEntry
		/// objects each thread has its own WaitEntry object.
		private WaitEntry GetThreadWaitEntry()
		{
			if (null == _waitEntry)
			{
				_waitEntry = new WaitEntry();
			}
			_waitEntry.Reset();
			return _waitEntry;
		}

		/// <summary>
		/// Push a new waiter into the waiter's stack
		/// </summary>
		/// <param name="newWaiterEntry">A waiter to put in the stack</param>
		public void PushWaitEntry(WaitEntry newWaiterEntry)
		{
			// Remove the waiter if it is already in the stack and 
			// update waiter's count as needed
			RemoveWaiter(newWaiterEntry, false);

			// If the stack is empty then newWaiterEntry is the new head of the stack 
			if (null == _headWaitEntry._nextWaiterEntry)
			{
				_headWaitEntry._nextWaiterEntry = newWaiterEntry;
				newWaiterEntry._prevWaiterEntry = _headWaitEntry;

			}
			else
			{
                // If the stack is not empty then put newWaiterEntry as the new head of the stack.
				// Save the old first waiter entry
				WaitEntry oldFirstWaiterEntry = _headWaitEntry._nextWaiterEntry;
				// Update the links
				_headWaitEntry._nextWaiterEntry = newWaiterEntry;
				newWaiterEntry._nextWaiterEntry = oldFirstWaiterEntry;
				newWaiterEntry._prevWaiterEntry = _headWaitEntry;
				oldFirstWaiterEntry._prevWaiterEntry = newWaiterEntry;
			}
			// Increment the number of waiters
			++_waitersCount;
		}

		/// <summary>
		/// Pop a waiter from the waiter's stack
		/// </summary>
		/// <returns>Returns the first waiter in the stack</returns>
		private WaitEntry PopWaiter()
		{
			// Store the current stack head
			WaitEntry oldFirstWaiterEntry = _headWaitEntry._nextWaiterEntry;

			// Store the new stack head
			WaitEntry newHeadWaiterEntry = oldFirstWaiterEntry._nextWaiterEntry;

			// Update the old stack head list links and decrement the number waiters.
			RemoveWaiter(oldFirstWaiterEntry, true);

			// Update the new stack head
			_headWaitEntry._nextWaiterEntry = newHeadWaiterEntry;
			if (null != newHeadWaiterEntry)
			{
				newHeadWaiterEntry._prevWaiterEntry = _headWaitEntry;
			}

			// Return the old stack head
			return oldFirstWaiterEntry;
		}

		/// <summary>
		/// Remove a waiter from the stack
		/// </summary>
		/// <param name="WaitEntry">A waiter entry to remove</param>
		/// <param name="popDecrement">If true the waiter count is always decremented</param>
		private void RemoveWaiter(WaitEntry WaitEntry, bool popDecrement)
		{
			// Store the prev entry in the list
			WaitEntry prevWaiterEntry = WaitEntry._prevWaiterEntry;

			// Store the next entry in the list
			WaitEntry nextWaiterEntry = WaitEntry._nextWaiterEntry;

			// A flag to indicate if we need to decrement the waiters count.
			// If we got here from PopWaiter then we must decrement.
			// If we got here from PushWaitEntry then we decrement only if
			// the waiter was already in the stack.
			bool decrementCounter = popDecrement;

			// Null the waiter's entry links
			WaitEntry._prevWaiterEntry = null;
			WaitEntry._nextWaiterEntry = null;

			// If the waiter entry had a prev link then update it.
			// It also means that the waiter is already in the list and we
			// need to decrement the waiters count.
			if (null != prevWaiterEntry)
			{
				prevWaiterEntry._nextWaiterEntry = nextWaiterEntry;
				decrementCounter = true;
			}

			// If the waiter entry had a next link then update it.
			// It also means that the waiter is already in the list and we
			// need to decrement the waiters count.
			if (null != nextWaiterEntry)
			{
				nextWaiterEntry._prevWaiterEntry = prevWaiterEntry;
				decrementCounter = true;
			}
			if (decrementCounter)
				--_waitersCount;
		}

		#endregion

		#region WaitEntry class 

		/// <summary>
		/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
		/// </summary>
		public sealed class WaitEntry : IDisposable
		{
			#region Member variables

			/// <summary>
			/// Event to signal the waiter that it got the work item.
			/// </summary>
			private AutoResetEvent _waitHandle = new AutoResetEvent(false);
			/// <summary>
			/// Flag to know if this waiter already quited from the queue because of a timeout.
			/// </summary>
			private bool _isTimedout = false;
			/// <summary>
			/// Flag to know if the waiter was signaled and got a work item. 
			/// </summary>
			private bool _isSignaled = false;
			/// <summary>
			/// A work item that passed directly to the waiter without going through the queue.
			/// </summary>
			private WorkItem _workItem = null;

			private bool _isDisposed = false;

			// Linked list members
			internal WaitEntry _nextWaiterEntry = null;
			internal WaitEntry _prevWaiterEntry = null;

			#endregion

			#region Construction

			public WaitEntry()
			{
				Reset();
			}
						
			#endregion

			#region Public methods

			public WaitHandle WaitHandle
			{
				get { return _waitHandle; }
			}

			public WorkItem WorkItem
			{
				get
				{
					lock(this)
					{
						return _workItem;
					}
				}
			}

			/// <summary>
			/// Signal a work item.
			/// </summary>
			/// <returns>Return true on success.</returns>
			/// <remarks>The method fails if Timeout() preceded its call.</remarks>
			public bool Signal(WorkItem workItem)
			{
				lock(this)
				{
					if (!_isTimedout)
					{
						_workItem = workItem;
						_isSignaled = true;
						_waitHandle.Set();
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Mark the wait entry that it has been timed out.
			/// The method fails if Signal() preceded this call.
			/// </summary>
			/// <returns>Return true on success.</returns>
			public bool Timeout()
			{
				lock(this)
				{
					if (!_isSignaled)
					{
						_isTimedout = true;
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Reset the wait entry so it can be used again.
			/// </summary>
			public void Reset()
			{
				_workItem = null;
				_isTimedout = false;
				_isSignaled = false;
				_waitHandle.Reset();
			}

			public void Close()
			{
				if (null != _waitHandle)
				{
					_waitHandle.Close();
					_waitHandle = null;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				if (!_isDisposed)
				{
					Close();
					_isDisposed = true;
				}
			}

			~WaitEntry()
			{
				Dispose();
			}

			#endregion
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (!_isDisposed)
			{
				Cleanup();
				_isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		~WorkItemsQueue()
		{
			Cleanup();
		}

		private void CheckDisposed()
		{
			if(_isDisposed)
				throw new ObjectDisposedException(GetType().ToString());
		}

		#endregion
	}
}
