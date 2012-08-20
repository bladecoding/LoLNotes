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
#if !SILVERLIGHT
using FluorineFx.Data;
#endif

namespace FluorineFx.Messaging.Messages
{
	/// <summary>
	/// Base class for all messages. Messages have two customizable sections; headers and body. 
	/// The headers property provides access to specialized meta information for a specific message 
	/// instance. 
	/// The headers property is an associative array with the specific header name as the key.
	/// <br/><br/>
	/// The body of a message contains the instance specific data that needs to be delivered and 
	/// processed by the remote destination.
	/// The body is an object and is the payload for a message.
	/// </summary>
    [CLSCompliant(false)]
#if SILVERLIGHT
    public class MessageBase : IMessage
#else
    public class MessageBase : IMessage//, ICloneable
#endif
	{
        /// <summary>
        /// Message headers.
        /// </summary>
#if !(NET_1_1)
        protected Dictionary<string, object> _headers = new Dictionary<string,object>();
#else
        protected Hashtable _headers = new Hashtable();
#endif
        /// <summary>
        /// Timestamp of the message.
        /// </summary>
        protected long _timestamp;
        /// <summary>
        /// Cclient id indicating the client that sent the message.
        /// </summary>
		protected object _clientId;
        /// <summary>
        /// The destination that the message targets. 
        /// </summary>
		protected string _destination;
        /// <summary>
        /// A unique message id.
        /// </summary>
		protected string _messageId;
        /// <summary>
        /// Time to live for the message. This is the number of milliseconds beyond the message timestamp that the message is considered valid and deliverable.
        /// </summary>
		protected long _timeToLive;
        /// <summary>
        /// The body of the message.
        /// </summary>
		protected object _body;

		/// <summary>
		/// Messages pushed from the server may arrive in a batch, with messages in the batch 
		/// potentially targeted to different Consumer instances.
		/// Each message will contain this header identifying the Consumer instance that will 
		/// receive the message.
		/// </summary>
		public const string DestinationClientIdHeader = "DSDstClientId";
		/// <summary>
		/// Messages are tagged with the endpoint id for the Channel they are sent over.
		/// Channels set this value automatically when they send a message.
		/// </summary>
		public const string EndpointHeader = "DSEndpoint";
		/// <summary>
		/// Messages that need to set remote credentials for a destination carry the Base64 encoded 
		/// credentials in this header.
		/// </summary>
		public const string RemoteCredentialsHeader = "DSRemoteCredentials";
		/// <summary>
		/// Messages sent with a defined request timeout use this header.
		/// The request timeout value is set on outbound messages by services or channels and the value 
		/// controls how long the corresponding MessageResponder will wait for an acknowledgement, 
		/// result or fault response for the message before timing out the request.
		/// </summary>
		public const string RequestTimeoutHeader = "DSRequestTimeout";
		/// <summary>
		/// This header is used to transport the global FlexClient Id value in outbound messages 
		/// once it has been assigned by the server.
		/// </summary>
		public const string FlexClientIdHeader = "DSId";

		/// <summary>
		/// Initializes a new instance of the MessageBase class.
		/// </summary>
		public MessageBase()
		{
		}

		#region IMessage Members

        /// <summary>
        /// Gets or sets the client identity indicating which client sent the message.
        /// </summary>
        public object clientId
		{
			get{ return _clientId; }
			set{ _clientId = value; }
		}
		/// <summary>
		/// Gets or sets the message destination.
		/// </summary>
		public string destination
		{
			get{ return _destination; }
			set{ _destination = value; }
		}
        /// <summary>
        /// Gets or sets the identity of the message.
        /// </summary>
        /// <remarks>This field is unique and can be used to correlate a response to the original request message.</remarks>
        public string messageId
		{
			get{ return _messageId; }
			set{ _messageId = value; }
		}
        /// <summary>
        /// Gets or sets the time stamp for the message.
        /// </summary>
        /// <remarks>The time stamp is the date and time that the message was sent.</remarks>
        public long timestamp
		{
			get{ return _timestamp; }
			set{ _timestamp = value; }
		}
        /// <summary>
        /// Gets or sets the validity for the message.
        /// </summary>
        /// <remarks>Time to live is the number of milliseconds that this message remains valid starting from the specified timestamp value.</remarks>
        public long timeToLive
		{
			get{ return _timeToLive; }
			set{ _timeToLive = value; }
		}
        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        /// <remarks>The body is the data that is delivered to the remote destination.</remarks>
        public object body
		{
			get{ return _body; }
			set{ _body = value; }
		}
        /// <summary>
        /// Gets or sets the headers of the message.
        /// </summary>
		/// <remarks>
		/// The headers of a message are an associative array where the key is the header name and the value is the header value.
		/// This property provides access to the specialized meta information for the specific message instance. 
        /// Flex core header names begin with a 'DS' prefix. Custom header names should start with a unique prefix to avoid name collisions.
		/// </remarks>
#if !(NET_1_1)
        public Dictionary<string, object> headers
        {
            get { return _headers; }
            set { _headers = value; }
        }
#else
        public Hashtable headers
		{
			get{ return _headers; }
			set{ _headers = value; }
		}
#endif
        /// <summary>
        /// Retrieves the specified header value.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>The value associated with the specified header name.</returns>
		public object GetHeader(string name)
		{
            if (_headers != null && _headers.ContainsKey(name))
				return _headers[name];
			return null;
		}
        /// <summary>
        /// Sets a header value.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Value associated with the header name.</param>
        public void SetHeader(string name, object value)
		{
			if( _headers == null )
#if !(NET_1_1)
                _headers = new Dictionary<string,object>();
#else
				_headers = new ASObject();
#endif
			_headers[name] = value;
		}
        /// <summary>
        /// Retrieves whether for the specified header name an associated value exists.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns></returns>
        public bool HeaderExists(string name)
		{
			if( _headers != null )
				return _headers.ContainsKey(name);
			return false;
		}

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public IMessage Copy()
        {
            // Allow most-derived type to instantiate the new clone, and all other derived types to copy state into it.
            return CopyImpl(null);
        }

        protected virtual MessageBase CopyImpl(MessageBase clone)
        {
            // Instantiate the clone, if a derived type hasn't already.
            if (clone == null) clone = new MessageBase();
            // Copy our state into the clone.
            clone._body = _body;
            clone._clientId = _clientId;
            clone._destination = _destination;
            clone._messageId = _messageId;
            clone._timestamp = _timestamp;
            clone._timeToLive = _timeToLive;
#if !(NET_1_1)
            clone.headers = new Dictionary<string, object>(_headers);
#else
			clone.headers = _headers.Clone() as Hashtable;
#endif
            return clone;
        }

        /// <summary>
        /// Gets the Flex client id specified in the message headers ("DSId").
        /// </summary>
        /// <returns>The Flex client id.</returns>
        public string GetFlexClientId()
        {
            if (HeaderExists(MessageBase.FlexClientIdHeader))
                return GetHeader(MessageBase.FlexClientIdHeader) as string;
            return null;
        }

        internal void SetFlexClientId(string value)
        {
            SetHeader(MessageBase.FlexClientIdHeader, value);
        }

		#endregion

        #region Message tracing

        static string[] IndentLevels = { "", "  ", "    ", "      ", "        ", "          " };

        internal static String GetIndent(int indentLevel)
        {
            if (indentLevel < IndentLevels.Length) 
                return IndentLevels[indentLevel];
            StringBuilder sb = new StringBuilder();
            sb.Append(IndentLevels[IndentLevels.Length - 1]);
            indentLevel -= IndentLevels.Length - 1;
            for (int i = 0; i < indentLevel; i++)
                sb.Append("  ");
            return sb.ToString();
        }

        internal static String GetFieldSeparator(int indentLevel)
        {
            String indent = GetIndent(indentLevel);
            if (indentLevel > 0)
                indent = Environment.NewLine + indent;
            else
                indent = " ";
            return indent;
        }
        /// <summary>
        /// Returns a string that represents the current MessageBase object.
        /// </summary>
        /// <returns>A string that represents the current MessageBase object.</returns>
        public override string ToString()
        {
            return ToString(1);
        }
        /// <summary>
        /// Returns a string that represents the current MessageBase object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>A string that represents the current MessageBase object.</returns>
        public virtual string ToString(int indentLevel)
        {
            return ToStringHeader(indentLevel) + ToStringFields(indentLevel + 1);
        }
        /// <summary>
        /// Returns a header string that represents the current MessageBase object.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>A header string that represents the current MessageBase object.</returns>
        protected string ToStringHeader(int indentLevel)
        {
            string value = "Flex Message";
            value += " (" + GetType().Name + ") ";
            return value;
        }
        /// <summary>
        /// Returns a string that represents the current MessageBase object fields.
        /// </summary>
        /// <param name="indentLevel">The indentation level used for tracing the message members.</param>
        /// <returns>A string that represents the current MessageBase object fields.</returns>
        protected virtual string ToStringFields(int indentLevel)
        {
            if (_headers != null)
            {
                string separator = GetFieldSeparator(indentLevel);
                string value = string.Empty;
                foreach (DictionaryEntry entry in _headers as IDictionary)
                {
                    string key = entry.Key.ToString();
                    value += separator + "hdr(" + key + ") = ";
                    value += BodyToString(entry.Value, indentLevel + 1);
                }
                return value;
            }
            return string.Empty;
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
            else if (visited.Contains(obj))
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
            if (body is IList)
            {
                if ((visited = CheckVisited(visited, body)) == null)
                    return "<--";

                string sep = GetFieldSeparator(indentLevel);
                StringBuilder sb = new StringBuilder();
                IList list = body as IList;
                if (list.Count > 0)
                    sb.Append(GetFieldSeparator(indentLevel - 1));
                sb.Append("[");
                if (list.Count > 0)
                {
                    sb.Append(sep);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i != 0)
                        {
                            sb.Append(",");
                            sb.Append(sep);
                        }
                        sb.Append(BodyToString(list[i], indentLevel, visited));
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
#if !SILVERLIGHT
            else if (body is UpdateCollectionRange)
            {
                UpdateCollectionRange ucr = body as UpdateCollectionRange;
                string value = (ucr.updateType != 0 ? "delete" : "insert") + "@" + "position = " + ucr.position;
                string sep = GetFieldSeparator(indentLevel - 1);
                value += sep + "identities = ";
                value += BodyToString(ucr.identities, indentLevel, visited);
                return value;
            }
#endif
            else if (body != null)
                return body.ToString();
            else return "null";
        }

        #endregion Message tracing
    }
}
