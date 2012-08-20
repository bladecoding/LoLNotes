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
using System.Reflection;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Util;
using FluorineFx.Invocation;
using FluorineFx.Collections;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.SO;

namespace FluorineFx.Net
{
    /// <summary>
    /// Represents the method that will handle the Sync event of a RemoteSharedObject object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SyncEventArgs object that contains the event data.</param>
    public delegate void SyncHandler(object sender, SyncEventArgs e);
    /// <summary>
    /// Represents the method that will handle messages sent from the server (when a custom RemoteSharedObject type does not declare the corresponding client side methods).
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SendMessageEventArgs object that contains the event data.</param>
    public delegate void SendMessageHandler(object sender, SendMessageEventArgs e);

    /// <summary>
    /// The SharedObject class is used to access, read and store data on remote shared objects, 
    /// that are shared in real-time by all clients connected to your application.
    /// </summary>
    [CLSCompliant(false)]
    public class RemoteSharedObject : AttributeStore
    {
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(RemoteSharedObject));
#endif
        bool _secure;
        ObjectEncoding _objectEncoding;
        NetConnection _connection;
        /// <summary>
        /// Initial synchronization flag.
        /// </summary>
        bool _initialSyncReceived;

        string _name = string.Empty;
        string _path = string.Empty;
        /// <summary>
        /// true if the client / server created the SO to be persistent
        /// </summary>
        bool _persistentSO = false;
        int _version = 1;
        int _updateCounter = 0;
        bool _modified = false;
        long _lastModified = -1;
        SharedObjectMessage _ownerMessage;
        /// <summary>
        /// Event listener, actually RTMP connection
        /// </summary>
        IEventListener _source = null;


        event ConnectHandler _connectHandler;
        event DisconnectHandler _disconnectHandler;
        event NetStatusHandler _netStatusHandler;
        event SyncHandler _syncHandler;
        event SendMessageHandler _sendMessageHandler;
        
#if !(NET_1_1)
        static Dictionary<string, RemoteSharedObject> SharedObjects;
#else
        static Hashtable SharedObjects;
#endif

        static RemoteSharedObject()
        {
#if !(NET_1_1)
            SharedObjects = new Dictionary<string, RemoteSharedObject>();
#else
            SharedObjects = new Hashtable();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the NetConnection class.
        /// </summary>
        /// <remarks>Do not create directly RemoteSharedObject objects, use the RemoteSharedObject.GetRemote method instead.</remarks>
        public RemoteSharedObject()
        {
        }

        private RemoteSharedObject(string name, string remotePath, object persistence, bool secure)
        {
            _name = name;
            _path = remotePath;
            _persistentSO = !false.Equals(persistence);
            _secure = secure;
            _objectEncoding = ObjectEncoding.AMF0;
            _initialSyncReceived = false;
            _ownerMessage = new SharedObjectMessage(null, null, -1, false);
        }

        /// <summary>
        /// Gets or sets the object encoding (AMF version) for this shared object. Default is ObjectEncoding.AMF0
        /// </summary>
        public ObjectEncoding ObjectEncoding
        {
            get { return _objectEncoding; }
            set { _objectEncoding = value; }
        }
        /// <summary>
        /// Dispatched when a SharedObject instance is reporting its status or error condition.
        /// </summary>
        public event NetStatusHandler NetStatus
        {
            add { _netStatusHandler += value; }
            remove { _netStatusHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a SharedObject instance has been updated by the server.
        /// </summary>
        public event SyncHandler Sync
        {
            add { _syncHandler += value; }
            remove { _syncHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a SharedObject instance receives a message from the server.
        /// </summary>
        public event SendMessageHandler SendMessage
        {
            add { _sendMessageHandler += value; }
            remove { _sendMessageHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a SharedObject instance is connected.
        /// </summary>
        public event ConnectHandler OnConnect
        {
            add { _connectHandler += value; }
            remove { _connectHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a SharedObject instance is disconnected.
        /// </summary>
        public event DisconnectHandler OnDisconnect
        {
            add { _disconnectHandler += value; }
            remove { _disconnectHandler -= value; }
        }
        /// <summary>
        /// Indicates whether the shared object data is persistent.
        /// </summary>
        public bool IsPersistentObject
        {
            get { return _persistentSO; }
        }
        /// <summary>
        /// Indicates whether this SharedObject has connected to the server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return _initialSyncReceived;
            }
        }
        /// <summary>
        /// Returns a reference to an object that can be shared across multiple clients by means of a server, such as Flash Media Server.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="remotePath">The URI of the server on which the shared object will be stored. This URI must be identical to the URI of the NetConnection object passed to the SharedObject.Connect() method.</param>
        /// <param name="persistence">Specifies whether the attributes of the shared object's data property are persistent locally, remotely, or both.</param>
        /// <returns>A reference to an object that can be shared across multiple clients.</returns>
        /// <example>
        /// NetConnection myNC = new NetConnection();
        /// myNC.Connect("rtmp://[yourDomain].com/applicationName");
        /// ...
        /// RemoteSharedObject myRemoteSO = SharedObject.GetRemote("mo", myNC.uri, false);
        /// myRemoteSO.Connect(myNC);
        /// </example>
        public static RemoteSharedObject GetRemote(string name, string remotePath, object persistence)
        {
            return GetRemote(typeof(RemoteSharedObject), name, remotePath, persistence, false);
        }
        /// <summary>
        /// Returns a reference to an object that can be shared across multiple clients by means of a server, such as Flash Media Server.
        /// </summary>
        /// <param name="type">Custom RemoteSharedObject type.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="remotePath">The URI of the server on which the shared object will be stored. This URI must be identical to the URI of the NetConnection object passed to the SharedObject.Connect() method.</param>
        /// <param name="persistence">Specifies whether the attributes of the shared object's data property are persistent locally, remotely, or both.</param>
        /// <returns>A reference to an object that can be shared across multiple clients.</returns>
        /// <example>
        /// NetConnection myNC = new NetConnection();
        /// myNC.Connect("rtmp://[yourDomain].com/applicationName");
        /// ...
        /// RemoteSharedObject myRemoteSO = SharedObject.GetRemote("mo", myNC.uri, false);
        /// myRemoteSO.Connect(myNC);
        /// </example>
        public static RemoteSharedObject GetRemote(Type type, string name, string remotePath, object persistence)
        {
            return GetRemote(type, name, remotePath, persistence, false);
        }
        /// <summary>
        /// Returns a reference to an object that can be shared across multiple clients by means of a server, such as Flash Media Server.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="remotePath">The URI of the server on which the shared object will be stored. This URI must be identical to the URI of the NetConnection object passed to the SharedObject.Connect() method.</param>
        /// <param name="persistence">Specifies whether the attributes of the shared object's data property are persistent locally, remotely, or both.</param>
        /// <param name="secure">Not supported.</param>
        /// <returns>A reference to an object that can be shared across multiple clients.</returns>
        /// <example>
        /// NetConnection myNC = new NetConnection();
        /// myNC.Connect("rtmp://[yourDomain].com/applicationName");
        /// SharedObject myRemoteSO = SharedObject.GetRemote("mo", myNC.uri, false);
        /// myRemoteSO.Connect(myNC);
        /// </example>
        public static RemoteSharedObject GetRemote(string name, string remotePath, object persistence, bool secure)
        {
            return GetRemote(typeof(RemoteSharedObject), name, remotePath, persistence, secure);
        }

        private static RemoteSharedObject GetRemote(Type type, string name, string remotePath, object persistence, bool secure)
        {
            lock ((SharedObjects as ICollection).SyncRoot)
            {
                if (SharedObjects.ContainsKey(name))
                    return SharedObjects[name] as RemoteSharedObject;
                RemoteSharedObject rso = Activator.CreateInstance(type) as RemoteSharedObject;
                ValidationUtils.ArgumentConditionTrue(rso != null, "type", "Expecting a RemoteSharedObject type");
                rso._name = name;
                rso._path = remotePath;
                rso._persistentSO = !false.Equals(persistence);
                rso._secure = secure;
                rso._objectEncoding = ObjectEncoding.AMF0;
                rso._initialSyncReceived = false;
                rso._ownerMessage = new SharedObjectMessage(null, null, -1, false);
                SharedObjects[name] = rso;
                return rso;
            }
        }

        /// <summary>
        /// Connects to a remote shared object on the server through the specified connection. Use this method after 
        /// issuing SharedObject.GetRemote(...). After a successful connection, the sync event is dispatched. 
        /// </summary>
        /// <param name="connection">A NetConnection object (such as one used to communicate with Flash Media Server) that is using the Real-Time Messaging Protocol (RTMP).</param>
        public void Connect(NetConnection connection)
        {
            Connect(connection, null);
        }

        /// <summary>
        /// Connects to a remote shared object on the server through the specified connection. Use this method after 
        /// issuing SharedObject.GetRemote(...). After a successful connection, the sync event is dispatched. 
        /// </summary>
        /// <param name="connection">A NetConnection object (such as one used to communicate with Flash Media Server) that is using the Real-Time Messaging Protocol (RTMP).</param>
        /// <param name="parameters">Parameters.</param>
        public void Connect(NetConnection connection, string parameters)
        {
            if (_initialSyncReceived)
                throw new InvalidOperationException("SharedObject already connected");
            ValidationUtils.ArgumentNotNull(connection, "connection");
            ValidationUtils.ArgumentNotNull(connection.Uri, "connection");
            ValidationUtils.ArgumentConditionTrue(connection.Uri.Scheme == "rtmp", "connection", "NetConnection object must use the Real-Time Messaging Protocol (RTMP)");
            ValidationUtils.ArgumentConditionTrue(connection.Connected, "connection", "NetConnection object must be connected");
            _connection = connection;
            _initialSyncReceived = false;

            FluorineFx.Messaging.Rtmp.SO.SharedObjectMessage message;
            if (connection.ObjectEncoding == ObjectEncoding.AMF0)
                message = new FluorineFx.Messaging.Rtmp.SO.SharedObjectMessage(_name, _version, _persistentSO);
            else
                message = new FluorineFx.Messaging.Rtmp.SO.FlexSharedObjectMessage(_name, _version, _persistentSO);
            FluorineFx.Messaging.Rtmp.SO.SharedObjectEvent evt = new FluorineFx.Messaging.Rtmp.SO.SharedObjectEvent(FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.SERVER_CONNECT, null, null);
            message.AddEvent(evt);
            _connection.NetConnectionClient.Write(message);
        }
        /// <summary>
        /// Closes the connection between a remote shared object and the server.
        /// </summary>
        public void Close()
        {
            if (_initialSyncReceived && _connection != null && _connection.Connected)
            {
                FluorineFx.Messaging.Rtmp.SO.SharedObjectMessage message;
                if (_connection.ObjectEncoding == ObjectEncoding.AMF0)
                    message = new FluorineFx.Messaging.Rtmp.SO.SharedObjectMessage(_name, _version, _persistentSO);
                else
                    message = new FluorineFx.Messaging.Rtmp.SO.FlexSharedObjectMessage(_name, _version, _persistentSO);
                FluorineFx.Messaging.Rtmp.SO.SharedObjectEvent evt = new FluorineFx.Messaging.Rtmp.SO.SharedObjectEvent(FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.SERVER_DISCONNECT, null, null);
                message.AddEvent(evt);
                _connection.NetConnectionClient.Write(message);

                // clear collections
                base.RemoveAttributes();
                _ownerMessage.Events.Clear();
            }
            _initialSyncReceived = false;
        }
        /// <summary>
        /// Updates the value of a property in a shared object and indicates to the server that the value of the property has changed. 
        /// The SetProperty() method explicitly marks properties as changed, or dirty. 
        /// </summary>
        /// <param name="propertyName">The name of the property in the shared object.</param>
        /// <param name="value">The value of the property or null to delete the property.</param>
        public void SetProperty(string propertyName, object value)
        {
            SetAttribute(propertyName, value);
        }
        /// <summary>
        /// Indicates to the server that the value of a property in the shared object has changed. This method marks properties as dirty, which means changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        public void SetDirty(string propertyName)
        {
            _source = _connection.NetConnectionClient.Connection;
            _ownerMessage.AddEvent(SharedObjectEventType.SERVER_SET_ATTRIBUTE, propertyName, GetAttribute(propertyName));
            _modified = true; 
            NotifyModified();
        }
        /// <summary>
        /// Broadcasts a message to all clients connected to a remote shared object, including the client that sent the message.
        /// </summary>
        /// <param name="handler">A string that identifies the message; the name of a handler functions attached to the shared object.</param>
        /// <param name="arguments">One or more arguments.</param>
        public void Send(string handler, params object[] arguments)
        {
            _ownerMessage.AddEvent(SharedObjectEventType.SERVER_SEND_MESSAGE, handler, arguments);
            _modified = true;
            NotifyModified();
        }

        internal static void Dispatch(SharedObjectMessage message)
        {
            RemoteSharedObject rso = null;
            lock ((SharedObjects as ICollection).SyncRoot)
            {
                if( SharedObjects.ContainsKey(message.Name) )
                    rso = SharedObjects[message.Name] as RemoteSharedObject;
            }
            if (rso != null)
            {
                try
                {
                    rso.DispatchSharedObjectMessage(message);
                }
                catch (Exception ex)
                {
                    rso.RaiseNetStatus(ex);
                }
            }
        }

        internal void DispatchSharedObjectMessage(SharedObjectMessage message)
        {
#if !(NET_1_1)
            List<ASObject> changeList = null;
            List<ASObject> notifications = null;
            List<SendMessageEventArgs> messages = null;
#else
            ArrayList changeList = null;
            ArrayList notifications = null;
            ArrayList messages = null;
#endif
            bool raiseOnConnect = false;
            foreach (ISharedObjectEvent sharedObjectEvent in message.Events)
            {
                switch (sharedObjectEvent.Type)
                {
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_INITIAL_DATA:
                        if (message.Version > 0)
                            _version = message.Version;
                        _attributes.Clear();
                        _initialSyncReceived = true;
                        //Delay the connection notification until the attribute store has been populated
                        //RaiseOnConnect();
                        raiseOnConnect = true;
                        break;
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_UPDATE_DATA:
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_UPDATE_ATTRIBUTE:
                        {
                            ASObject infoObject = new ASObject();
                            infoObject["code"] = "change";
                            infoObject["name"] = sharedObjectEvent.Key;
                            infoObject["oldValue"] = this.GetAttribute(sharedObjectEvent.Key);
                            //Do not update the attribute store if this is a notification that the SetAttribute is accepted
                            if (sharedObjectEvent.Type != FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_UPDATE_ATTRIBUTE)
                                _attributes[sharedObjectEvent.Key] = sharedObjectEvent.Value;
                            if (changeList == null)
#if !(NET_1_1)
                                changeList = new List<ASObject>();
#else
                                changeList = new ArrayList();
#endif
                            changeList.Add(infoObject);
                        }
                        break;
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_CLEAR_DATA:
                        {
                            ASObject infoObject = new ASObject();
                            infoObject["code"] = "clear";
                            if (changeList == null)
#if !(NET_1_1)
                                changeList = new List<ASObject>();
#else
                                changeList = new ArrayList();
#endif
                            changeList.Add(infoObject);
                            _attributes.Clear();
                        }
                        break;
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_DELETE_ATTRIBUTE:
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_DELETE_DATA:
                        {
                            _attributes.Remove(sharedObjectEvent.Key);
                            ASObject infoObject = new ASObject();
                            infoObject["code"] = "delete";
                            infoObject["name"] = sharedObjectEvent.Key;
                            if (changeList == null)
#if !(NET_1_1)
                                changeList = new List<ASObject>();
#else
                                changeList = new ArrayList();
#endif
                            changeList.Add(infoObject);
                        }
                        break;
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_STATUS:
                        {
                            ASObject infoObject = new ASObject();
                            infoObject["level"] = sharedObjectEvent.Value;
                            infoObject["code"] = sharedObjectEvent.Key;
                            if( notifications == null )
#if !(NET_1_1)
                                notifications = new List<ASObject>();
#else
                                notifications = new ArrayList();
#endif
                            notifications.Add(infoObject);
                        }
                        break;
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.CLIENT_SEND_MESSAGE:
                    case FluorineFx.Messaging.Rtmp.SO.SharedObjectEventType.SERVER_SEND_MESSAGE:
                        {
                            string handler = sharedObjectEvent.Key;
                            IList arguments = sharedObjectEvent.Value as IList;
                            MethodInfo mi = MethodHandler.GetMethod(this.GetType(), handler, arguments);
                            if (mi != null)
                            {
                                ParameterInfo[] parameterInfos = mi.GetParameters();
                                object[] args = new object[parameterInfos.Length];
                                arguments.CopyTo(args, 0);
                                TypeHelper.NarrowValues(args, parameterInfos);
                                try
                                {
                                    InvocationHandler invocationHandler = new InvocationHandler(mi);
                                    object result = invocationHandler.Invoke(this, args);
                                }
                                catch (Exception exception)
                                {
#if !SILVERLIGHT
                                    log.Error("Error while invoking method " + handler + " on shared object", exception);
#endif
                                }
                            }
                            else
                            {
                                if (messages == null)
#if !(NET_1_1)
                                    messages = new List<SendMessageEventArgs>();
#else
                                    messages = new ArrayList();
#endif
                                messages.Add(new SendMessageEventArgs(handler, arguments));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            if (raiseOnConnect)
                RaiseOnConnect();

            if (changeList != null && changeList.Count > 0)
            {
#if !(NET_1_1)
                RaiseSync(changeList.ToArray());
#else
                RaiseSync(changeList.ToArray(typeof(ASObject)) as ASObject[]);
#endif
            }
            if (notifications != null )
            {
                foreach (ASObject infoObject in notifications)
                    RaiseNetStatus(infoObject);
            }
            if (messages != null)
            {
                foreach (SendMessageEventArgs e in messages)
                    RaiseSendMessage(e);
            }
        }

        internal static void DispatchDisconnect(NetConnection connection)
        {
            lock ((SharedObjects as ICollection).SyncRoot)
            {
                foreach (RemoteSharedObject rso in SharedObjects.Values)
                {
                    if (rso._connection == connection)
                        rso.RaiseDisconnect();
                }
            }
        }

        internal void RaiseOnConnect()
        {
            if (_connectHandler != null)
            {
                _connectHandler(this, new EventArgs());
            }
        }

        internal void RaiseDisconnect()
        {
            _initialSyncReceived = false;//Disconnected RSO
            if (_disconnectHandler != null)
            {
                _disconnectHandler(this, new EventArgs());
            }
        }

        internal void RaiseSync(ASObject[] changeList)
        {
            if (_syncHandler != null)
            {
                _syncHandler(this, new SyncEventArgs(changeList));
            }
        }

        internal void RaiseNetStatus(ASObject info)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(info));
            }
        }

        internal void RaiseNetStatus(Exception exception)
        {
            if (_netStatusHandler != null)
            {
                _netStatusHandler(this, new NetStatusEventArgs(exception));
            }
        }

        internal void RaiseSendMessage(SendMessageEventArgs e)
        {
            if (_sendMessageHandler != null)
            {
                _sendMessageHandler(this, e);
            }
        }

        /// <summary>
        /// Sets an attribute on this object.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>true if the attribute value changed otherwise false</returns>
        public sealed override bool SetAttribute(string name, object value)
        {
            _ownerMessage.AddEvent(SharedObjectEventType.SERVER_SET_ATTRIBUTE, name, value);
            if (value == null)
            {
                RemoveAttribute(name);
                return true;
            }
            if (base.SetAttribute(name, value))
            {
                _modified = true;
                NotifyModified();
                return true;
            }
            NotifyModified();
            return false;
        }
        /// <summary>
        /// Removes an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>true if the attribute was found and removed otherwise false.</returns>
        public sealed override bool RemoveAttribute(string name)
        {
            if (base.RemoveAttribute(name))
            {
                _modified = true;
                _ownerMessage.AddEvent(SharedObjectEventType.SERVER_DELETE_ATTRIBUTE, name, null);
                NotifyModified();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes all attributes.
        /// </summary>
        public sealed override void RemoveAttributes()
        {
            foreach(string key in GetAttributeNames())
            {
                _ownerMessage.AddEvent(SharedObjectEventType.SERVER_DELETE_ATTRIBUTE, key, null);
            }
            // Clear data
            base.RemoveAttributes();
            // Mark as modified
            _modified = true;
            NotifyModified();
        }
        /// <summary>
        /// Returns the value for a given attribute and sets it if it doesn't exist.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">Attribute's default value.</param>
        /// <returns>The attribute value.</returns>
        public sealed override object GetAttribute(string name, object value)
        {
            if (name == null)
                return null;

            if (!HasAttribute(name))
                SetAttribute(name, value);
            return GetAttribute(name);
        }
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
#if !(NET_1_1)
        public sealed override void SetAttributes(IDictionary<string, object> values)
        {
            if (values == null)
                return;

            BeginUpdate();
            try
            {
                foreach (KeyValuePair<string, object> entry in values)
                {
                    SetAttribute(entry.Key, entry.Value);
                }
            }
            finally
            {
                EndUpdate();
            }
        }
#else
        public sealed override void SetAttributes(IDictionary values)
        {
            if (values == null)
                return;

            BeginUpdate();
            try
            {
                foreach (DictionaryEntry entry in values)
                {
                    SetAttribute(entry.Key.ToString(), entry.Value);
                }
            }
            finally
            {
                EndUpdate();
            }
        }
#endif

        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Attribute store.</param>
        public sealed override void SetAttributes(IAttributeStore values)
        {
            if (values == null)
                return;

            BeginUpdate();
            try
            {
                foreach (string name in values.GetAttributeNames())
                {
                    SetAttribute(name, values.GetAttribute(name));
                }
            }
            finally
            {
                EndUpdate();
            }
        }
        /// <summary>
        /// Start performing multiple updates to the shared object.
        /// </summary>
        public void BeginUpdate()
        {
            _updateCounter += 1;
        }
        /// <summary>
        /// The multiple updates are complete, notify server about all changes at once.
        /// </summary>
        public void EndUpdate()
        {
            _updateCounter -= 1;
            if (_updateCounter == 0)
            {
                NotifyModified();
            }
        }

        private void UpdateVersion()
        {
            _version += 1;
        }

        private void NotifyModified()
        {
            if (_updateCounter > 0)
            {
                // Inside a BeginUpdate/EndUpdate block
                return;
            }

            if (_modified)
            {
                _modified = false;
                // increase version of SO
                UpdateVersion();
                _lastModified = System.Environment.TickCount;
            }
            SendUpdates();
        }

        /// <summary>
        /// Send update notification over data channel of RTMP connection
        /// </summary>
        private void SendUpdates()
        {
            if (_ownerMessage.Events.Count != 0)
            {
                if (_connection.NetConnectionClient.Connection != null)
                {
                    RtmpConnection connection = _connection.NetConnectionClient.Connection as RtmpConnection;
                    // Only send updates when issued through RTMP request
                    RtmpChannel channel = connection.GetChannel((byte)3);

                    // Send update to "owner" of this update request
                    SharedObjectMessage syncOwner;
                    if (connection.ObjectEncoding == ObjectEncoding.AMF0)
                        syncOwner = new SharedObjectMessage(null, _name, _version, this.IsPersistentObject);
                    else
                        syncOwner = new FlexSharedObjectMessage(null, _name, _version, this.IsPersistentObject);
                    syncOwner.AddEvents(_ownerMessage.Events);

                    if (channel != null)
                    {
                        channel.Write(syncOwner);
                    }
                    else
                    {
#if !SILVERLIGHT
                        log.Warn(__Res.GetString(__Res.Channel_NotFound));
#endif
                    }
                }
                _ownerMessage.Events.Clear();
            }
        }
    }
}
