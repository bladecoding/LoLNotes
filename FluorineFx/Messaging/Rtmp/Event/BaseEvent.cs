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
using System.Text;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.Messaging.Rtmp.Event
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
	public class BaseEvent : IRtmpEvent
	{
        /// <summary>
        /// Event RTMP packet header.
        /// </summary>
		protected RtmpHeader _header;
        /// <summary>
        /// Event target object.
        /// </summary>
		protected object _object;
        /// <summary>
        /// Event timestamp.
        /// </summary>
		protected int _timestamp;
        /// <summary>
        /// Event extended timestamp.
        /// </summary>
        private int _extendedTimestamp = -1;
        /// <summary>
        /// Event data type.
        /// </summary>
		protected byte _dataType;
        /// <summary>
        /// Event type.
        /// </summary>
		protected EventType _eventType;
        /// <summary>
        /// Event listener.
        /// </summary>
		protected IEventListener _source;

        internal BaseEvent(EventType eventType)
		{
			_eventType = eventType;
			_object = null;
		}

        internal BaseEvent(EventType eventType, byte dataType, IEventListener source) 
		{
			_dataType = dataType;
			_eventType = eventType;
			_source = source;
		}

        /// <summary>
        /// Gets or sets event type.
        /// </summary>
        public EventType EventType
		{ 
			get{ return _eventType; }
			set{ _eventType = value; }
		}
        /// <summary>
        /// Gets or sets the RTMP packet header.
        /// </summary>
		public RtmpHeader Header
		{
			get{ return _header; }
			set{ _header = value; }
		}
        /// <summary>
        /// Gets event context object.
        /// </summary>
		public virtual object Object
		{ 
			get{ return _object; }
		}
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
		public int Timestamp
		{
			get{ return _timestamp; }
			set{ _timestamp = value; }
		}
        /// <summary>
        /// Gets or sets the extended timestamp.
        /// </summary>
        /// <value>The extended timestamp.</value>
        public int ExtendedTimestamp
        {
            get { return _extendedTimestamp; }
            set { _extendedTimestamp = value; }
        }
        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public byte DataType
		{
			get{ return _dataType; }
			set{ _dataType = value; }
		}
        /// <summary>
        /// Gets or sets the event listener.
        /// </summary>
		public IEventListener Source
		{
			get{ return _source; }
			set{ _source = value; }
		}
        /// <summary>
        /// Gets whether event has source (event listeners).
        /// </summary>
		public bool HasSource
		{
			get{ return _source != null; }
		}

        internal String GetFieldSeparator(int indentLevel)
        {
            return MessageBase.GetFieldSeparator(indentLevel);
        }

        internal String GetIndent(int indentLevel)
        {
            return MessageBase.GetIndent(indentLevel);
        }
        /// <summary>
        /// Returns a string that represents the current event object.
        /// </summary>
        /// <returns>A string that represents the current event object.</returns>
        public override string ToString()
        {
            return ToString(1);
        }
        /// <summary>
        /// Returns a string that represents the current event object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the event members.</param>
        /// <returns>A string that represents the current event object.</returns>
        public string ToString(int indentLevel)
        {
            return ToStringHeader(indentLevel) + ToStringFields(indentLevel + 1);
        }
        /// <summary>
        /// Returns a header string that represents the current event object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the event members.</param>
        /// <returns>A header string that represents the current event object.</returns>
        protected virtual string ToStringHeader(int indentLevel)
        {
            //string value = "RTMP Event";
            string value = this.GetType().Name;
            return value;
        }
        /// <summary>
        /// Returns a string that represents the current event object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the header members.</param>
        /// <returns>A string that represents the current event object fields.</returns>
        protected virtual string ToStringFields(int indentLevel)
        {
            String sep = GetFieldSeparator(indentLevel);
            String value = sep + "event = " + EventTypeToString((int)_eventType);
            value += sep + "timestamp = " + _timestamp;
            return value;
        }
        /// <summary>
        /// Returns a string that represents body object.
        /// </summary>
        /// <param name="body">An object to trace.</param>
        /// <param name="indentLevel">The indentation level used for tracing object members.</param>
        /// <returns>A string that represents body object.</returns>
        protected string BodyToString(object body, int indentLevel)
        {
            return BodyToString(body, indentLevel, null);
        }
        /// <summary>
        /// Returns a string that represents body object.
        /// </summary>
        /// <param name="body">An object to trace.</param>
        /// <param name="indentLevel">The indentation level used for tracing object members.</param>
        /// <param name="visited">Dictionary to handle circular references.</param>
        /// <returns>A string that represents body object.</returns>
        protected string BodyToString(object body, int indentLevel, IDictionary visited)
        {
            try
            {
                indentLevel = indentLevel + 1;
                if (visited == null && indentLevel > 18)
                    return Environment.NewLine + GetFieldSeparator(indentLevel) + "<..max-depth-reached..>";
                return InternalBodyToString(body, indentLevel, visited);
            }
            catch (Exception ex)
            {
                return "Exception in BodyToString: " + ex.Message;
            }
        }
        /// <summary>
        /// Returns a string that represents body object.
        /// </summary>
        /// <param name="body">An object to trace.</param>
        /// <param name="indentLevel">The indentation level used for tracing object members.</param>
        /// <returns>A string that represents body object.</returns>
        protected string InternalBodyToString(object body, int indentLevel)
        {
            return InternalBodyToString(body, indentLevel, null);
        }

        private IDictionary CheckVisited(IDictionary visited, object obj)
        {
            if (visited == null)
#if !(NET_1_1)
                visited = new Dictionary<object, bool>();
#else
                visited = new Hashtable();
#endif
            else if (!visited.Contains(obj))
                return null;
            visited.Add(obj, true);
            return visited;
        }
        /// <summary>
        /// Returns a string that represents body object.
        /// </summary>
        /// <param name="body">An object to trace.</param>
        /// <param name="indentLevel">The indentation level used for tracing object members.</param>
        /// <param name="visited">Dictionary to handle circular references.</param>
        /// <returns>A string that represents body object.</returns>
        protected string InternalBodyToString(object body, int indentLevel, IDictionary visited)
        {
            if (body is object[])
            {
                if ((visited = CheckVisited(visited, body)) == null)
                    return "<--";

                string sep = GetFieldSeparator(indentLevel);
                StringBuilder sb = new StringBuilder();
                object[] arr = body as object[];
                if (arr.Length > 0)
                    sb.Append(GetFieldSeparator(indentLevel - 1));
                sb.Append("[");
                if (arr.Length > 0)
                {
                    sb.Append(sep);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i != 0)
                        {
                            sb.Append(",");
                            sb.Append(sep);
                        }
                        sb.Append(BodyToString(arr[i], indentLevel, visited));
                    }
                    sb.Append(GetFieldSeparator(indentLevel - 1));
                }
                sb.Append("]");
                return sb.ToString();
            }
            else if (body is IDictionary)
            {
                IDictionary bodyMap = body as IDictionary;
                StringBuilder buf = new StringBuilder();
                buf.Append("{");
                bool first = true;
                foreach (DictionaryEntry entry in bodyMap)
                {
                    if (!first)
                        buf.Append(", ");
                    object key = entry.Key;
                    object value = entry.Value;
                    buf.Append(key == this ? "(recursive map as key)" : key);
                    buf.Append("=");
                    if (value == this)
                        buf.Append("(recursive map as value)");
                    else
                        buf.Append(BodyToString(value, indentLevel + 1, visited));
                    first = false;
                }
                buf.Append("}");
                return buf.ToString();
            }
            else if (body is MessageBase)
            {
                return (body as MessageBase).ToString(indentLevel);
            }
            else if (body != null)
                return body.ToString();
            else return "null";
        }

        static string[] EventTypeNames = { "system", "status", "service_call", "shared_object", "stream_control", "stream_data", "client", "server"};

        private static string EventTypeToString(int type)
        {
            if (type < 0 || type >= EventTypeNames.Length)
                return "invalid event type";
            return EventTypeNames[type];
        }
	}
}
