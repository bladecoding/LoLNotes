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
using System.Net;
using FluorineFx.IO;
using FluorineFx.Messaging.Api.Service;
using System.Collections.Generic;
using FluorineFx.Messaging.Rtmp;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Util;

//Network Security Access Restrictions in Silverlight 2
//http://msdn.microsoft.com/en-us/library/cc645032(VS.95).aspx

namespace FluorineFx.Net
{
    /// <summary>
    /// Represents the method that will handle the NetStatus event of a NetConnection or RemoteSharedObject object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A NetStatusEventArgs object that contains the event data.</param>
    public delegate void NetStatusHandler(object sender, NetStatusEventArgs e);
    /// <summary>
    /// Represents the method that will handle the Connect event of a NetConnection or RemoteSharedObject object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">EventArgs object that contains the event data.</param>
    public delegate void ConnectHandler(object sender, EventArgs e);
    /// <summary>
    /// Represents the method that will handle the Disconnect event of a NetConnection or RemoteSharedObject object. 
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">EventArgs object that contains the event data.</param>
    public delegate void DisconnectHandler(object sender, EventArgs e);

    /// <summary>
    /// The NetConnection class creates a connection between a .NET client and a Flash Media Server application or application server running Flash Remoting.
    /// </summary>
    /// <example>
    /// <code lang="CS">
    /// _netConnection = new NetConnection();
    /// _netConnection.ObjectEncoding = ObjectEncoding.AMF3;
    /// _netConnection.NetStatus += new NetStatusHandler(_netConnection_NetStatus);
    /// _netConnection.Connect(“http://localhost:1781/SilverlightApplicationWeb/Gateway.aspx”);
    /// ...
    /// _netConnection.Call("ServiceLibrary.MyDataService.GetCustomers", new GetCustomersHandler(), new object[] { txtSearch.Text });
    /// 
    /// void _netConnection_NetStatus(object sender, NetStatusEventArgs e)
    /// {
    ///     string level = e.Info[“level”] as string;
    ///     if (level == “error”)
    ///     {
    ///         Log(“Error: ” + e.Info[“code”] as string);
    ///     }
    ///     if (level == “status”)
    ///     {
    ///         Log(“Status: ” + e.Info[“code”] as string);
    ///     }
    /// }
    /// 
    /// public class GetCustomersHandler : IPendingServiceCallback
    /// {
    ///     public GetCustomersHandler()
    ///     {
    ///     }
    /// 
    ///     public void ResultReceived(IPendingServiceCall call)
    ///     {
    ///         object result = call.Result;
    ///         ArrayCollection items = result as ArrayCollection;
    ///         foreach (object item in items)
    ///         {
    ///             ...
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    [CLSCompliant(false)]
    public sealed class NetConnection
    {
        private string _clientId;
        private Uri _uri;
        private object[] _arguments;
        private INetConnectionClient _netConnectionClient;
        private ObjectEncoding _objectEncoding;
        private string _playerVersion;
        private string _swfUrl;
        private string _pageUrl;
        private object _client;
#if !(SILVERLIGHT)
        readonly CookieContainer _cookieContainer;
#endif
        readonly Dictionary<string, AMFHeader> _headers;
        event NetStatusHandler NetStatusHandler;
        event ConnectHandler ConnectHandler;
        event DisconnectHandler DisconnectHandler;

        /// <summary>
        /// Initializes a new instance of the NetConnection class.
        /// </summary>
        public NetConnection()
        {
            _clientId = null;
            _playerVersion = "WIN 9,0,115,0";
            _objectEncoding = ObjectEncoding.AMF0;
            _headers = new Dictionary<string,AMFHeader>();
            _client = this;
#if !(SILVERLIGHT)
            _cookieContainer = new CookieContainer();
#endif
            TypeHelper._Init();
        }
        /// <summary>
        /// Dispatched when a NetConnection instance is reporting its status or error condition.
        /// </summary>
        public event NetStatusHandler NetStatus
        {
            add { NetStatusHandler += value; }
            remove { NetStatusHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a NetConnection instance is connected.
        /// </summary>
        public event ConnectHandler OnConnect
        {
            add { ConnectHandler += value; }
            remove { ConnectHandler -= value; }
        }
        /// <summary>
        /// Dispatched when a NetConnection instance is disconnected.
        /// </summary>
        public event DisconnectHandler OnDisconnect
        {
            add { DisconnectHandler += value; }
            remove { DisconnectHandler -= value; }
        }
        /// <summary>
        /// Gets URI of the application on the server.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
        }
        /// <summary>
        /// Get or sets the player version string sent from .NET code.
        /// </summary>
        public string PlayerVersion
        {
            get { return _playerVersion; }
            set { _playerVersion = value; }
        }
        /// <summary>
        /// Gets or sets the URL of the source SWF file making the connection.
        /// </summary>
        /// <value>The SWF URL.</value>
        public string SwfUrl
        {
            get { return _swfUrl; }
            set { _swfUrl = value; }
        }
        /// <summary>
        /// Gets or sets the URL of the web page from which the SWF file was loaded.
        /// </summary>
        /// <value>The page URL.</value>
        public string PageUrl
        {
            get { return _pageUrl; }
            set { _pageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the object encoding (AMF version) for this NetConnection object. Default is ObjectEncoding.AMF0.
        /// </summary>
        public ObjectEncoding ObjectEncoding
        {
            get { return _objectEncoding; }
            set { _objectEncoding = value; }
        }
        /// <summary>
        /// Indicates the object on which callback methods should be invoked. The default is this NetConnection instance.
        /// If you set the client property to another object, callback methods will be invoked on that object. 
        /// </summary>
        public Object Client
        {
            get { return _client; }
            set
            {
                ValidationUtils.ArgumentNotNull(value, "Client");
                _client = value;
            }
        }
        /// <summary>
        /// Gets the client identity.
        /// </summary>
        public string ClientId
        {
            get { return _clientId; }
        }
#if !(SILVERLIGHT)
        /// <summary>
        /// Gets CookieContainer for HTTP requests.
        /// </summary>
        public CookieContainer CookieContainer
        {
            get { return _cookieContainer; }
        }
#endif
        internal void SetClientId(string clientId)
        {
            _clientId = clientId;
        }

        internal Dictionary<string, AMFHeader> Headers
        {
            get { return _headers; }
        }

        internal INetConnectionClient NetConnectionClient
        {
            get { return _netConnectionClient; }
        }

        /// <summary>
        /// Indicates whether this connection has connected to a server through a persistent RTMP connection (true) or not (false).
        /// It is always true for AMF connections to application servers.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (_netConnectionClient != null)
                    return _netConnectionClient.Connected;
                return false;
            }
        }

        /// <summary>
        /// Adds a context header to the Action Message Format (AMF) packet structure.
        /// This header is sent with every future AMF packet.
        /// To remove a header call AddHeader with the name of the header to remove an undefined object.
        /// </summary>
        /// <param name="operation">Identifies the header and the ActionScript object data associated with it.</param>
        /// <param name="mustUnderstand">A value of true indicates that the server must understand and process this header before it handles any of the following headers or messages.</param>
        /// <param name="param">Any ActionScript object or null.</param>
        /// <remarks>Not implemented.</remarks>
        public void AddHeader(string operation, bool mustUnderstand, object param)
        {
            if (param == null)
            {
                if (_headers.ContainsKey(operation))
                    _headers.Remove(operation);
                return;
            }
            AMFHeader header = new AMFHeader(operation, mustUnderstand, param);
            _headers[operation] = header;
        }
        /// <summary>
        /// Authenticates a user with a credentials header
        /// </summary>
        /// <param name="userid">A username to be used by the server for authentication.</param>
        /// <param name="password"> password to be used by the server for authentication.</param>
        public void SetCredentials(string userid, string password)
        {
            ASObject aso = new ASObject();
            aso.Add("userid", userid);
            aso.Add("password", password);
            AddHeader("Credentials", false, aso);
        }
        /// <summary>
        /// Opens a connection to a server. Through this connection, you can invoke commands on a remote server. 
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="arguments">Optional parameters of any type to be passed to the application specified in command.
        /// If the application is unable to process the parameters in the order in which they are received, NetStatusEvent is dispatched with the code property set to NetConnection.Connect.Rejected
        /// </param>
        /// <remarks>
        /// Set the command parameter to the URI of the application on the server that runs when the connection is made.
        /// Use the following format. protocol://host[:port]/appname/[instanceName]
        /// </remarks>
        public void Connect(string command, params object[] arguments)
        {
            _uri = new Uri(command);
            _arguments = arguments;
            Connect();
        }
        /// <summary>
        /// Asynchronous version of Connect.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="callback">Callback object.</param>
        /// <param name="state"></param>
        /// <param name="arguments">Optional parameters of any type to be passed to the application specified in command.</param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string command, AsyncCallback callback, object state, params object[] arguments)
        {
            _uri = new Uri(command);
            _arguments = arguments;
            // Create IAsyncResult object identifying the asynchronous operation
            AsyncResultNoResult ar = new AsyncResultNoResult(callback, state);
            // Use a thread pool thread to perform the operation
            System.Threading.ThreadPool.QueueUserWorkItem(DoConnect, ar);
			// Return the IAsyncResult to the caller
            return ar;
        }

        /// <summary>
        /// Asynchronous version of Connect.
        /// </summary>
        /// <param name="asyncResult"></param>
        public void EndConnect(IAsyncResult asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            // Wait for operation to complete, then return result or throw exception
            if (ar != null) ar.EndInvoke();
        }

        private void DoConnect(object asyncResult)
        {
            AsyncResultNoResult ar = asyncResult as AsyncResultNoResult;
            try
            {
                // Perform the operation; if sucessful set the result
                Connect();
                if (ar != null) ar.SetAsCompleted(null, false);
            }
            catch (Exception ex)
            {
                // If operation fails, set the exception
                if (ar != null) ar.SetAsCompleted(ex, false);
            }
        }

        private void Connect()
        {
            if (_uri.Scheme == "http" || _uri.Scheme == "https")
            {
#if !(SILVERLIGHT)
                if( ServicePointManager.ServerCertificateValidationCallback == null )
                    ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };
#endif
                _netConnectionClient = new RemotingClient(this);
                _netConnectionClient.Connect(_uri.ToString(), _arguments);
                return;
            }
            if (_uri.Scheme == "rtmp")
            {
                _netConnectionClient = new RtmpClient(this);
                _netConnectionClient.Connect(_uri.ToString(), _arguments);
                return;
            }
            throw new UriFormatException();
        }

        /// <summary>
        /// Closes the connection that was opened with the server and dispatches the netStatus event with a code property of NetConnection.Connect.Close.
        /// </summary>
        public void Close()
        {
            if (_netConnectionClient != null)
            {
                _netConnectionClient.Close();
            }
            _netConnectionClient = null;
        }

        /// <summary>
        /// Servers like Wowza may call "close" on NetConnection.
        /// </summary>
        public void close()
        {
            Close();
        }

        internal void RaiseNetStatus(Exception exception)
        {
            if (NetStatusHandler != null)
            {
                NetStatusHandler(this, new NetStatusEventArgs(exception));
            }
        }

        internal void RaiseNetStatus(string code, Exception exception)
        {
            if (NetStatusHandler != null)
            {
                NetStatusHandler(this, new NetStatusEventArgs(code, exception));
            }
        }

        internal void RaiseNetStatus(ASObject info)
        {
            if (NetStatusHandler != null)
            {
                NetStatusHandler(this, new NetStatusEventArgs(info));
            }
        }

        internal void RaiseNetStatus(string message)
        {
            if (NetStatusHandler != null)
            {
                NetStatusHandler(this, new NetStatusEventArgs(message));
            }
        }

        internal void RaiseOnConnect()
        {
            if (ConnectHandler != null)
            {
                ConnectHandler(this, new EventArgs());
            }
        }

        internal void RaiseDisconnect()
        {
            RemoteSharedObject.DispatchDisconnect(this);
            if (DisconnectHandler != null)
            {
                DisconnectHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Invokes a command or method on the server to which this connection is connected.
        /// </summary>
        /// <param name="command">A method specified in object path form.</param>
        /// <param name="callback">An optional object that is used to handle return values from the server.</param>
        /// <param name="arguments">Optional arguments. These arguments are passed to the method specified in the command parameter when the method is executed on the remote application server.</param>
        [ObsoleteAttribute("Overload of the Call method which accepts IPendingServiceCallback has been deprecated. Please investigate the use of the overload that accepts a Responder<T> instead.")]
        public void Call(string command, IPendingServiceCallback callback, params object[] arguments)
        {
            _netConnectionClient.Call(command, callback, arguments);
        }

        /// <summary>
        /// Invokes a command or method on the server to which this connection is connected.
        /// </summary>
        /// <typeparam name="T">Return type from a remote method invocation.</typeparam>
        /// <param name="command">A method specified in object path form.</param>
        /// <param name="responder">An optional object that is used to handle return values from the server.</param>
        /// <param name="arguments">Optional arguments. These arguments are passed to the method specified in the command parameter when the method is executed on the remote application server.</param>
        public void Call<T>(string command, Responder<T> responder, params object[] arguments)
        {
            _netConnectionClient.Call(command, responder, arguments);
        }

        /// <summary>
        /// Invokes a command or method on the server to which this connection is connected.
        /// </summary>
        /// <param name="endpoint">Flex RPC endpoint name.</param>
        /// <param name="destination">Flex RPC message destination.</param>
        /// <param name="source">The name of the service to be called including namespace name.</param>
        /// <param name="operation">The name of the remote method/operation that should be called.</param>
        /// <param name="callback">An optional object that is used to handle return values from the server.</param>
        /// <param name="arguments">Optional arguments. These arguments are passed to the method specified in the command parameter when the method is executed on the remote application server.</param>
        /// <remarks>
        /// For a RTMP connection this method throws a NotSupportedException.
        /// </remarks>
        [ObsoleteAttribute("Overload of the Call method which accepts IPendingServiceCallback has been deprecated. Please investigate the use of the overload that accepts a Responder<T> instead.")]
        public void Call(string endpoint, string destination, string source, string operation, IPendingServiceCallback callback, params object[] arguments)
        {
            _netConnectionClient.Call(endpoint, destination, source, operation, callback, arguments);
        }

        /// <summary>
        /// Invokes a command or method on the server to which this connection is connected.
        /// </summary>
        /// <typeparam name="T">Return type from a remote method invocation.</typeparam>
        /// <param name="endpoint">Flex RPC endpoint name.</param>
        /// <param name="destination">Flex RPC message destination.</param>
        /// <param name="source">The name of the service to be called including namespace name.</param>
        /// <param name="operation">The name of the remote method/operation that should be called.</param>
        /// <param name="responder">An optional object that is used to handle return values from the server.</param>
        /// <param name="arguments">Optional arguments. These arguments are passed to the method specified in the command parameter when the method is executed on the remote application server.</param>
        /// <remarks>
        /// For RTMP connection this method throws a NotSupportedException.
        /// </remarks>
        public void Call<T>(string endpoint, string destination, string source, string operation, Responder<T> responder, params object[] arguments)
        {
            _netConnectionClient.Call(endpoint, destination, source, operation, responder, arguments);
        }

        internal void OnSharedObject(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, SharedObjectMessage message)
        {
            RemoteSharedObject.Dispatch(message);
        }
    }
}
