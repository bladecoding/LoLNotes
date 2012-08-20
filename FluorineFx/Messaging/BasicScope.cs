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
#if !(NET_1_1)
using System.Collections.Generic;
using FluorineFx.Collections.Generic;
#endif
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Event;

namespace FluorineFx.Messaging
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [CLSCompliant(false)]
	public class BasicScope : PersistableAttributeStore, IBasicScope
	{
        object _syncLock = new object();
        private IScope _parent;
#if !(NET_1_1)
        private CopyOnWriteArray<IEventListener> _listeners = new CopyOnWriteArray<IEventListener>();
#else
		private CopyOnWriteArray _listeners = new CopyOnWriteArray();
#endif
        /// <summary>
        /// Set to true to prevent the scope from being freed upon disconnect.
        /// </summary>
        protected bool _keepOnDisconnect = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicScope"/> class.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="type">The scope type.</param>
        /// <param name="name">The scope name.</param>
        /// <param name="persistent">A value indicating whether this instance is a persistent scope.</param>
		public BasicScope(IScope parent, string type, string name, bool persistent) : base(type, name, null, persistent)
		{
			_parent = parent;
		}

		#region IBasicScope Members

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        /// <summary>
        /// Checks whether the scope has a parent.
        /// You can think of scopes as of tree items
        /// where scope may have a parent and children (child).
        /// </summary>
		public bool HasParent
		{
			get{ return _parent != null; }
		}
        /// <summary>
        /// Get this scope's parent.
        /// </summary>
		public virtual IScope Parent
		{
			get{ return _parent; }
			set{ _parent = value; }
		}
        /// <summary>
        /// Get the scopes depth, how far down the scope tree is it. The lowest depth
        /// is 0x00, the depth of Global scope. Application scope depth is 0x01. Room
        /// depth is 0x02, 0x03 and so forth.
        /// </summary>
		public int Depth
		{
			get
			{ 
				if( HasParent )
					return _parent.Depth + 1;
				else
					return 0;
			}
		}
        /// <summary>
        /// Gets the full absolute path.
        /// </summary>
        public override string Path
		{
			get
			{
				if( HasParent )
					return _parent.Path + "/" + _parent.Name;
				else
					return string.Empty;
			}
		}

		#endregion

        /// <summary>
        /// Add event listener to this observable.
        /// </summary>
        /// <param name="listener">Event listener.</param>
		public virtual void AddEventListener(IEventListener listener) 
		{
			_listeners.Add(listener);
		}

        /// <summary>
        /// Remove event listener from this observable.
        /// </summary>
        /// <param name="listener">Event listener.</param>
		public virtual void RemoveEventListener(IEventListener listener) 
		{
			_listeners.Remove(listener);
            if (!_keepOnDisconnect && ScopeUtils.IsRoom(this) && _listeners.Count == 0) 
			{
				// Delete empty rooms
				_parent.RemoveChildScope(this);
			}
		}

        /// <summary>
        /// Get the event listeners collection.
        /// </summary>
        /// <returns>Collection of event listeners.</returns>
		public ICollection GetEventListeners()
		{
			return _listeners;
		}
        /// <summary>
        /// Determines whether scope has attached event listeners.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if scope has event listeners; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEventListeners()
        {
            return _listeners.Count > 0;
        }
        /// <summary>
        /// Handle an event.
        /// </summary>
        /// <param name="event">Event to handle.</param>
        /// <returns>true if event was handled, false if it should bubble.</returns>
        public bool HandleEvent(IEvent @event) 
		{
			return false;
		}
        /// <summary>
        /// Notifies the event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void NotifyEvent(IEvent @event) 
		{
		}
        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="event">Event to dispatch.</param>
        public virtual void DispatchEvent(IEvent @event) 
		{
			foreach(IEventListener listener in _listeners) 
			{
                if (@event.Source == null || @event.Source != listener) 
				{
                    listener.NotifyEvent(@event);
				}
			}
		}

		#region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the BasicScope.
        /// </summary>
        /// <returns>
        /// An IEnumerator object that can be used to iterate through the collection.
        /// </returns>
		public new virtual IEnumerator GetEnumerator()
		{
			return null;
		}

		#endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
		public override string ToString()
		{
			return this.Name;
		}


	}
}
