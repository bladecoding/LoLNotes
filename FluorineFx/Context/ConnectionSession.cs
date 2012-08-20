/*
	Fluorine Projector SWF2Exe open source library based on Flash Remoting
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
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp;

namespace FluorineFx.Context
{
    class ConnectionSession : Session
    {
        IConnection _connection;

        internal ConnectionSession(SessionManager sessionManager, IConnection connection)
            : base(sessionManager, Guid.NewGuid().ToString("D") /*connection.ConnectionId*/)
        {
            _connection = connection;
        }

        #region ISession Members

        public override void Add(string name, object value)
        {
            _connection.SetAttribute(name, value);
        }

        public override void Clear()
        {
            _connection.RemoveAttributes();
        }

        public override void Remove(string name)
        {
            _connection.RemoveAttribute(name);
        }

        public override void RemoveAll()
        {
            _connection.RemoveAttributes();
        }

        public override object this[string name]
        {
            get
            {
                return _connection.GetAttribute(name);
            }
            set
            {
                _connection.SetAttribute(name, value);
            }
        }

        /// <summary>
        /// Pushes a message to a remote client.
        /// </summary>
        /// <param name="message">Message to push.</param>
        /// <param name="messageClient">The MessageClient subscription that this message targets.</param>
        public override void Push(IMessage message, IMessageClient messageClient)
        {
            if( _connection is RtmpConnection )
                (_connection as RtmpConnection).Push(message, messageClient);
        }

        #endregion

        #region ICollection Members

        public override void CopyTo(Array array, int index)
        {
            _connection.CopyTo(array as object[], index);
        }

        public override int Count
        {
            get { return _connection.AttributesCount; }
        }

        public override bool IsSynchronized
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            return _connection.GetEnumerator();
        }

        #endregion

        public override void Invalidate()
        {
            base.Invalidate();
            _connection = null;
        }
    }
}
