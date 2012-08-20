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
using System.Collections.Generic;
using FluorineFx.Threading;
#if !SILVERLIGHT
using log4net;
#endif

namespace FluorineFx.Util
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class ObjectPool<T> : DisposableBase
    {
#if !SILVERLIGHT
        private static readonly ILog Log = LogManager.GetLogger(typeof(ObjectPool<>));
#endif
        private readonly int _capacity;
        private readonly int _growth;
        private readonly bool _forceGC;
        private readonly FastReaderWriterLock _lock;
        private Queue<T> _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the object pool object initially contains.</param>
        public ObjectPool(int capacity)
            : this(capacity, 10, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the object pool object initially contains.</param>
        /// <param name="growth">The number of elements reserved in the object pool when there are no available objects.</param>
        public ObjectPool(int capacity, int growth)
            : this(capacity, growth, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the object pool object initially contains.</param>
        /// <param name="growth">The number of elements reserved in the object pool when there are no available objects.</param>
        /// <param name="forceGCOnGrowth">If set to <c>true</c> forces GC on growth.</param>
        public ObjectPool(int capacity, int growth, bool forceGCOnGrowth)
        {
            _lock = new FastReaderWriterLock();
            _forceGC = forceGCOnGrowth;
            _growth = growth;
            _capacity = capacity;
            if (_forceGC)
                GC.WaitForPendingFinalizers();
        }


        #region IDisposable Members

        /// <summary>
        /// Free managed resources.
        /// </summary>
        protected override void Free()
        {
            try
            {
                _lock.AcquireWriterLock();
                if (_queue != null)
                {
                    while (_queue.Count > 0)
                    {
                        try
                        {
                            using (_queue.Dequeue() as IDisposable)
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            Unreferenced.Parameter(ex);
                        }
                    }
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
            base.Free();
        }

        #endregion IDisposable Members

        /// <summary>
        /// Reserve new objects in the object pool.
        /// </summary>
        /// <param name="count">The number of elements reserved in the object pool.</param>
        private void AddObjects(int count)
        {
#if !SILVERLIGHT
            Log.Debug(string.Format("ObjectPool creating {0} pooled objects", count));
#endif
            if (_forceGC)
                GC.Collect();
            if( _queue == null )
                _queue = new Queue<T>(_capacity);
            for (int i = 1; i <= count; i++)
            {
                T obj = GetObject();
                _queue.Enqueue(obj);
            }
            if (_forceGC)
                GC.Collect();
        }

        /// <summary>
        /// Releases the object back to the object pool.
        /// </summary>
        /// <param name="obj">The object to check in.</param>
        public void CheckIn(T obj)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ObjectPool");
            try
            {
                _lock.AcquireWriterLock();
                if (_queue == null)
                    throw new InvalidOperationException("Invalid CheckIn operation");
                _queue.Enqueue(obj);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Aquires an object from the object pool.
        /// </summary>
        /// <returns>An object from the object pool.</returns>
        public T CheckOut()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ObjectPool");

            try
            {
                _lock.AcquireWriterLock();
                if (_queue == null || _queue.Count == 0)
                    AddObjects(_growth);
                return _queue.Dequeue();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Creates instances of the object pool element's class.
        /// </summary>
        /// <returns>A new object instance.</returns>
        protected virtual T GetObject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the length of the object pool.
        /// </summary>
        /// <value>The length of the object pool.</value>
        protected int Length
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("ObjectPool");
                try
                {
                    _lock.AcquireReaderLock();
                    return _queue != null ? _queue.Count : 0;
                }
                finally
                {
                    _lock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Gets the growth parameter of the object pool.
        /// </summary>
        /// <value>The growth parameter of the object pool.</value>
        public int Growth
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("ObjectPool");
                return _growth;
            }
        }
    }
}