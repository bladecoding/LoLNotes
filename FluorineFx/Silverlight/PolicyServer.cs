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
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace FluorineFx.Silverlight
{
    class PolicyConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PolicyServer));

        private PolicyServer _policyServer;
        private Socket _connection;
        private byte[] _buffer;
        private int _received;
        private byte[] _policy;
        private EndPoint _endpoint;
        /// <summary>
        /// The request that we're expecting from the client
        /// </summary>
        private static string _policyRequestString = "<policy-file-request/>";

        public Socket Socket
        {
            get { return _connection; }
        }

        public PolicyConnection(PolicyServer policyServer, Socket client, byte[] policy)
        {
            _policyServer = policyServer;
            _connection = client;
            _endpoint = _connection.RemoteEndPoint;
            _policy = policy;
            _buffer = new byte[_policyRequestString.Length];
            _received = 0;
            try
            {
                // Receive the request from the client                
                _connection.BeginReceive(_buffer, 0, _policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException ex)
            {
                if (log.IsDebugEnabled)
                    log.Debug("Socket exception", ex);
                _connection.Close();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Failed starting a policy connection", ex);
            }
        }

        private void OnReceive(IAsyncResult res)
        {
            try
            {
                if (log.IsDebugEnabled)
                    log.Debug("Policy connection receiving request");

                _received += _connection.EndReceive(res);
                if (_received < _policyRequestString.Length)
                {
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Policy connection received partial request: {0} bytes", _received));

                    _connection.BeginReceive(_buffer, _received, _policyRequestString.Length - _received, SocketFlags.None, new AsyncCallback(OnReceive), null);
                    return;
                }
                string request = System.Text.Encoding.UTF8.GetString(_buffer, 0, _received);
#if !(NET_1_1)
                if (StringComparer.InvariantCultureIgnoreCase.Compare(request, _policyRequestString) != 0)
#else
				if (request != _policyRequestString)
#endif
                {
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Policy connection could not handle request: {0}", request));

                    _policyServer.RaiseDisconnect(_endpoint);
                    _connection.Close();
                    return;
                }
                // Sending the policy                
                if (log.IsDebugEnabled)
                    log.Debug("Policy connection sending policy stream");
                _connection.BeginSend(_policy, 0, _policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (ObjectDisposedException)
            {
                //The underlying socket may be closed
                _policyServer.RaiseDisconnect(_endpoint);
            }
            catch (SocketException ex)
            {
                if (log.IsDebugEnabled)
                    log.Debug("Socket exception", ex);

                _policyServer.RaiseDisconnect(_endpoint);
                _connection.Close();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Policy connection failed", ex);
            }
        }

        public void OnSend(IAsyncResult res)
        {
            try
            {
                _connection.EndSend(res);
                _connection.Close();
            }
            catch (ObjectDisposedException)
            {
                //The underlying socket may be closed
            }
            catch (SocketException ex)
            {
                if (log.IsDebugEnabled)
                    log.Debug("Socket exception", ex);
                _connection.Close();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Policy connection failed", ex);
            }
            finally
            {
                _policyServer.RaiseDisconnect(_endpoint);
            }
        }
    }

    public delegate void ConnectHandler(object sender, ConnectEventArgs e);
    public delegate void DisconnectHandler(object sender, DisconnectEventArgs e);

    /// <summary>
    /// Event argument for connection events.
    /// </summary>
    public class ConnectEventArgs : EventArgs
    {
        EndPoint _endPoint;

        internal ConnectEventArgs(EndPoint endPoint)
        {
            _endPoint = endPoint;
        }
        /// <summary>
        /// Gets the network address.
        /// </summary>
        public EndPoint EndPoint { get { return _endPoint; } }
    }
    /// <summary>
    /// Event argument for disconnection events.
    /// </summary>
    public class DisconnectEventArgs : EventArgs
    {
        EndPoint _endPoint;

        internal DisconnectEventArgs(EndPoint endPoint)
        {
            _endPoint = endPoint;
        }
        /// <summary>
        /// Gets the network address.
        /// </summary>
        public EndPoint EndPoint { get { return _endPoint; } }
    }

    /// <summary>
    /// Silverlight policy server implementation.
    /// </summary>
    public class PolicyServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PolicyServer));

        private Socket _listener;
        private byte[] _policy;

        event ConnectHandler _connectHandler;
        event DisconnectHandler _disconnectHandler;

        public event ConnectHandler Connect
        {
            add { _connectHandler += value; }
            remove { _connectHandler -= value; }
        }

        public event DisconnectHandler Disconnect
        {
            add { _disconnectHandler += value; }
            remove { _disconnectHandler -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyFile">The path of the socket policy file</param>
        public PolicyServer(string policyFile)
        {
            // Load the policy file            
            try
            {
	            if (log.IsDebugEnabled)
                    log.Debug("Starting FluorineFx Silverlight Policy Server");
				
                FileStream policyStream = new FileStream(policyFile, FileMode.Open);
                _policy = new byte[policyStream.Length];
                policyStream.Read(_policy, 0, _policy.Length);
                policyStream.Close();
                // Create the Listening Socket            
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)SocketOptionName.NoDelay, 0);
                _listener.Bind(new IPEndPoint(IPAddress.Any, 943));
                _listener.Listen(10);
                _listener.BeginAccept(new AsyncCallback(OnConnection), null);
                if (log.IsDebugEnabled)
                    log.Debug("Started FluorineFx Silverlight Policy Server");
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Error starting FluorineFx Silverlight Policy Server", ex);
            }
        }

        public void OnConnection(IAsyncResult res)
        {
            Socket client = null;
            try
            {
                client = _listener.EndAccept(res);
                if (_connectHandler != null)
                    _connectHandler(this, new ConnectEventArgs(client.RemoteEndPoint));
            }
            catch (SocketException ex)
            {
                if (log.IsDebugEnabled)
                    log.Debug("Socket exception", ex);
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            PolicyConnection pc = new PolicyConnection(this, client, _policy);
            _listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }

        public void Close()
        {
            try
            {
                if (_listener != null)
                    _listener.Close();
                if (log.IsDebugEnabled)
                    log.Debug("Stopped FluorineFx Silverlight Policy Server");
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("Error stopping FluorineFx Silverlight Policy Server", ex);
            }
        }

        internal void RaiseDisconnect(EndPoint endpoint)
        {
            try
            {
                if (_disconnectHandler != null)
                {
                    _disconnectHandler(this, new DisconnectEventArgs(endpoint));
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.Error("RaiseDisconnect exception", ex);
            }
        }
    }
}