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
#endif
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Collections.Generic;

namespace FluorineFx.Messaging.Rtmp.SO
{
	/// <summary>
    /// Shared object event.
	/// </summary>
    [CLSCompliant(false)]
    public class SharedObjectMessage : BaseEvent, ISharedObjectMessage
	{
        /// <summary>
        /// Shared object event name.
        /// </summary>
		private string _name;
        /// <summary>
        /// Shared object events chain.
        /// </summary>
        private ConcurrentLinkedQueue<ISharedObjectEvent> _events = new ConcurrentLinkedQueue<ISharedObjectEvent>();
        /// <summary>
        /// Shared object version, used for synchronization purposes.
        /// </summary>
		private int _version = 0;
        /// <summary>
        /// Indicates whether shared object is persistent.
        /// </summary>
		private bool _persistent = false;

        /// <summary>
        /// Initializes a new instance of the SharedObjectMessage class with given name, version and persistence flag.
        /// </summary>
        /// <param name="name">Event name.</param>
        /// <param name="version">Shared object version.</param>
        /// <param name="persistent">Indicates whether shared object is persistent.</param>
        internal SharedObjectMessage(string name, int version, bool persistent)
            : this(null, name, version, persistent)
		{
		}
        /// <summary>
        /// Initializes a new instance of the SharedObjectMessage class with given listener, name, version and persistence flag.
        /// </summary>
        /// <param name="source">Event listener.</param>
        /// <param name="name">Event name.</param>
        /// <param name="version">Shared object version.</param>
        /// <param name="persistent">Indicates whether shared object is persistent.</param>
        internal SharedObjectMessage(IEventListener source, string name, int version, bool persistent)
            : base(EventType.SHARED_OBJECT, Constants.TypeSharedObject, source)
		{
			_name = name;
			_version = version;
			_persistent = persistent;
		}


		#region ISharedObjectMessage Members

        /// <summary>
        /// Gets shared object event name.
        /// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

        internal void SetName(string name)
        {
            _name = name;
        }
        /// <summary>
        /// Gets shared object version.
        /// </summary>
		public int Version
		{
			get
			{
				return _version;
			}
		}
        /// <summary>
        /// Gets a value indicating whether the shared object is persistent.
        /// </summary>
		public bool IsPersistent
		{
			get
			{
				return _persistent;
			}
		}

        internal void SetIsPersistent(bool persistent)
        {
            _persistent = persistent;
        }
        /// <summary>
        /// Add a shared object event.
        /// </summary>
        /// <param name="type">Event type.</param>
        /// <param name="key">Handler key.</param>
        /// <param name="value">Event value.</param>
		public void AddEvent(SharedObjectEventType type, string key, object value)
		{
			_events.Enqueue(new SharedObjectEvent(type, key, value));
		}
        /// <summary>
        /// Add a shared object event.
        /// </summary>
        /// <param name="sharedObjectEvent">Shared object event.</param>
        public void AddEvent(ISharedObjectEvent sharedObjectEvent)
		{
            _events.Enqueue(sharedObjectEvent);
		}
        /// <summary>
        /// Clear shared object.
        /// </summary>
		public void Clear()
		{
			_events.Clear();
		}
        /// <summary>
        /// Gets a value indicating whether the shared object is empty.
        /// </summary>
		public bool IsEmpty
		{
			get
			{
				return _events.Count == 0;
			}
		}

		#endregion

        /// <summary>
        /// Returns a set of ISharedObjectEvent objects containing informations what to change.
        /// </summary>
        public IQueue<ISharedObjectEvent> Events
        {
            get
            {
                return _events;
            }
        }
        /// <summary>
        /// Add a list of shared object events.
        /// </summary>
        /// <param name="events">List of shared object events.</param>
        public void AddEvents(IEnumerable<ISharedObjectEvent> events)
        {
            _events.AddRange(events);
        }

        #region IEvent Members

        /// <summary>
        /// Gets event context object.
        /// </summary>
        public override object Object
		{
			get
			{
				return this.Events;
			}
		}

		#endregion

        /// <summary>
        /// Returns a string that represents the current event object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the header members.</param>
        /// <returns>A string that represents the current event object fields.</returns>
        protected override string ToStringFields(int indentLevel)
        {
            string sep = GetFieldSeparator(indentLevel);
            string value = base.ToStringFields(indentLevel);
            value += sep + "events = ";
            string sep2 = GetFieldSeparator(indentLevel + 1);
            foreach (ISharedObjectEvent @event in _events)
            {
                //value += sep2 + @event.ToString();
                value += sep2 + "SOEvent(" + @event.Type.ToString() + ", " + @event.Key + ", " + BodyToString(@event.Value, indentLevel + 2) + ")";
            }
            return value;
        }
	}
}
