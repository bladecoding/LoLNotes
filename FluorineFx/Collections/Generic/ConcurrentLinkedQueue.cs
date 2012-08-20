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
using System.Collections.Generic;
using FluorineFx.Util;

namespace FluorineFx.Collections.Generic
{
    /// <summary>
    /// A thread-safe variable size first-in-first-out (FIFO) collection of instances of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the queue.</typeparam>
    public class ConcurrentLinkedQueue<T> : IQueue<T>
    {
		private Node<T> _head;
		private Node<T> _tail;
        private object _objLock = new object();

        /// <summary>
        /// A node object that holds a value and a pointer to another node object.
        /// </summary>
        /// <typeparam name="TNode">The type of the value held.</typeparam>
        internal class Node<TNode>
        {
            /// <summary>
            /// The value held by this node.
            /// </summary>
            public TNode Item;

            /// <summary>
            /// The next node in the link chain.
            /// </summary>
            /// <remarks>
            /// We don't make this a property because then it couldn't be used
            /// as a pass-by-ref parameter, which is where it will spend most 
            /// of it's time.
            /// </remarks>
            public Node<TNode> Next;

            /// <summary>
            /// Creates a new <see cref="Node{TNode}" />.
            /// </summary>
            public Node()
            {
            }

            /// <summary>
            /// Creates a new <see cref="Node{T}" /> with the given item as the value.
            /// </summary>
            /// <param name="item">the item to store</param>
            public Node(TNode item)
            {
                Item = item;
            }
        }

		/// <summary>
        /// Initializes a new instance of the ConcurrentLinkedQueue class.
		/// </summary>
        public ConcurrentLinkedQueue()
		{
			_tail = _head = new Node<T>();
		}
        /// <summary>
        /// Initializes a new instance of the ConcurrentLinkedQueue class.
        /// </summary>
        /// <param name="elements">The list of objects to add.</param>
        public ConcurrentLinkedQueue(IEnumerable<T> elements)
        {
            _tail = _head = new Node<T>();
            AddRange(elements);
        }

		/// <summary>
        /// Inserts the specified element at the tail of this queue.
		/// </summary>
		/// <param name="item">The item to insert in the queue.</param>
		public void Enqueue(T item)
		{
			Node<T> oldTail = null;
			Node<T> oldTailNext;
            Node<T> newNode = new Node<T>(item);
			bool newNodeWasAdded = false;
			while(!newNodeWasAdded)
			{
				oldTail = _tail;
				oldTailNext = oldTail.Next;

				if(_tail == oldTail)
				{
					if(oldTailNext == null)
					{
                        newNodeWasAdded = ThreadingUtils.CompareAndSwap(ref _tail.Next, null, newNode);
					}
					else
					{
                        ThreadingUtils.CompareAndSwap(ref _tail, oldTail, oldTailNext);
					}
				}
			}
            ThreadingUtils.CompareAndSwap(ref _tail, oldTail, newNode);
		}

		/// <summary>
		/// Removes an item from the queue.
		/// </summary>
		/// <param name="item">The dequeued item.</param>
		/// <returns>
        /// The dequeued item, or the default value for the element type if the queue was empty.
		/// </returns>
		public bool Dequeue(out T item)
		{
			item = default(T);
			Node<T> oldHead = null;

			bool haveAdvancedHead = false;
			while(!haveAdvancedHead)
			{
				oldHead = _head;
				Node<T> oldTail = _tail;
				Node<T> oldHeadNext = oldHead.Next;
				if(oldHead == _head)
				{
					if(oldHead == oldTail)
					{
						if(oldHeadNext == null)
							return false;
                        ThreadingUtils.CompareAndSwap(ref _tail, oldTail, oldHeadNext);
					}
					else
					{
						item = oldHeadNext.Item;
						haveAdvancedHead = ThreadingUtils.CompareAndSwap(ref _head, oldHead, oldHeadNext);
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Removes an item from the queue.
		/// </summary>
        /// <returns>The dequeued item.</returns>
		public T Dequeue()
		{
			T result;
			Dequeue(out result);
			return result;
		}

        /// <summary>
        /// Adds all of the elements of the supplied
        /// <paramref name="elements"/>list to the end of this list.
        /// </summary>
        /// <param name="elements">The list of objects to add.</param>
        public void AddRange(IEnumerable<T> elements)
        {
            foreach (T obj in elements)
            {
                Enqueue(obj);
            }
        }
        /// <summary>
        /// Removes all elements from the queue.
        /// </summary>
        public void Clear()
        {
            T result;
            while (Dequeue(out result))
                ;
        }
        /// <summary>
        /// Returns the first actual (non-header) node on list.
        /// </summary>
        /// <returns>First node.</returns>
        Node<T> First()
        {
            for (; ; )
            {
                Node<T> h = _head;
                Node<T> t = _tail;
                Node<T> first = h.Next;
                if (h == _head)
                {
                    if (h == t)
                    {
                        if (first == null)
                            return null;
                        else
                            ThreadingUtils.CompareAndSwap(ref _tail, t, first);
                    }
                    else
                    {
                        if (first.Item != null)
                            return first;
                        else // remove deleted node and continue
                            ThreadingUtils.CompareAndSwap(ref _head, h, first);
                    }
                }
            }
        }


        #region ICollection Members

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        /// <summary>
        /// Returns the number of elements in this queue.
        /// </summary>
        /// <remarks>
        /// Beware that, unlike in most collections, this method is not a constant-time operation. Because of the
        /// asynchronous nature of these queues, determining the current number of elements requires an O(n) traversal.
        /// </remarks>
        public int Count
        {
            get 
            {
                int count = 0;
                for (Node<T> p = First(); p != null; p = p.Next)
                {
                    if (p.Item != null)
                    {
                        // Collections.size() spec says to max out
                        if (++count == int.MaxValue)
                            break;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the ConcurrentLinkedQueue is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return true; }
        }
        /// <summary>
        /// Gets an object that can be used to synchronize access to the ConcurrentLinkedQueue.
        /// </summary>
        public object SyncRoot
        {
            get{ return _objLock; }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        internal struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            ConcurrentLinkedQueue<T> _queue;
            /// <summary>
            /// Next node to return item for.
            /// </summary>
            Node<T> _nextNode;
            /// <summary>
            /// NextItem holds on to item fields because once we claim that an element exists we must return it
            /// </summary>
            T _nextItem;

            internal Enumerator(ConcurrentLinkedQueue<T> queue)
            {
                _queue = queue;
                _nextNode = null;
                _nextItem = default(T);
            }

            public void Dispose()
            {
                _nextNode = null;
                _nextItem = default(T);
                _queue = null;
            }

            public bool MoveNext()
            {
                Object x = _nextItem;
                Node<T> p = (_nextNode == null) ? _queue.First() : _nextNode.Next;
                for (; ; )
                {
                    if (p == null)
                    {
                        _nextNode = null;
                        _nextItem = default(T);
                        return false;
                    }
                    T item = p.Item;
                    if (item != null)
                    {
                        _nextNode = p;
                        _nextItem = item;
                        return true;
                    }
                    else // skip over nulls
                        p = p.Next;
                }
            }

            public T Current
            {
                get
                {
                    if( _nextNode == null )
                        throw new InvalidOperationException();
                    return _nextItem;
                }
            }

            object IEnumerator.Current
            {
                get{ return Current; }
            }

            void IEnumerator.Reset()
            {
                _nextNode = null;
                _nextItem = default(T);
            }
        }
    }
}
