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
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using log4net;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Util;
using FluorineFx.Messaging.Rtmp.Event;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Stream;
using FluorineFx.Messaging.Api.Event;
using FluorineFx.Messaging.Api.SO;
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Rtmp.SO;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.IO;
using FluorineFx.Threading;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpServer : DisposableBase
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmpServer));

		event ErrorHandler _onErrorEvent;

        Hashtable _connections;
        ArrayList _socketListeners;

        //Timer _idleTimer;
        //int _idleCheckInterval = 1000 * 60;
        //int _idleTimeOutValue = 1000 * 60;

        //ThreadPoolEx _threadPoolEx;
        BufferPool _bufferPool;
        RtmpHandler _rtmpHandler;
        IEndpoint _endpoint;

        X509Certificate _serverCertificate;

        public RtmpServer(RtmpEndpoint endpoint)
		{
			_connections = new Hashtable();
			_socketListeners = new ArrayList();
            _endpoint = endpoint;
            _rtmpHandler = new RtmpHandler(endpoint);

            try
            {
                if (endpoint.ChannelDefinition.Properties.KeystoreFile != null)
                {

                    FileSystemResource fsr = new FileSystemResource(endpoint.ChannelDefinition.Properties.KeystoreFile);
                    if (fsr.Exists)
                    {
                        if (endpoint.ChannelDefinition.Properties.KeystorePassword != null)
                            _serverCertificate = new X509Certificate2(fsr.File.FullName, endpoint.ChannelDefinition.Properties.KeystorePassword);
                        else
                            _serverCertificate = X509Certificate.CreateFromCertFile(fsr.File.FullName);
                        log.Info(string.Format("Certificate issued to {0} and is valid from {1} until {2}.", _serverCertificate.Subject, _serverCertificate.GetEffectiveDateString(), _serverCertificate.GetExpirationDateString()));
                    }
                    else
                        log.Error("Certificate file not found");
                }
                else
                {
                    if (endpoint.ChannelDefinition.Properties.ServerCertificate != null)
                    {
                        StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), endpoint.ChannelDefinition.Properties.ServerCertificate.StoreName, false);
                        StoreLocation storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), endpoint.ChannelDefinition.Properties.ServerCertificate.StoreLocation, false);
                        X509FindType x509FindType = (X509FindType)Enum.Parse(typeof(X509FindType), endpoint.ChannelDefinition.Properties.ServerCertificate.X509FindType, false);
                        X509Store store = new X509Store(storeName, storeLocation);
                        store.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certificateCollection = store.Certificates.Find(x509FindType, endpoint.ChannelDefinition.Properties.ServerCertificate.FindValue, false);
                        X509Certificate2Enumerator enumerator = certificateCollection.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            _serverCertificate = enumerator.Current;
                            log.Info(string.Format("Certificate issued to {0} and is valid from {1} until {2}.", _serverCertificate.Subject, _serverCertificate.GetEffectiveDateString(), _serverCertificate.GetExpirationDateString()));
                        }
                        else
                            log.Error("Certificate not found");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading certificate.", ex);
            }
		}
        /*
		internal ThreadPoolEx ThreadPoolEx
		{
			get{ return _threadPoolEx; }
		}
        */
        internal BufferPool BufferPool
        {
            get { return _bufferPool; }
        }

        internal IRtmpHandler RtmpHandler
        {
            get { return _rtmpHandler; }
        }

        internal IEndpoint Endpoint
        {
            get { return _endpoint; }
        }

		public event ErrorHandler OnError
		{
			add { _onErrorEvent += value; }
			remove { _onErrorEvent -= value; }
		}

		protected override void Free()
		{
			Stop();
            if (_bufferPool != null)
                _bufferPool.Dispose();
        }

        #region Server Management

        public void Start()
		{
			try
			{
				if( log.IsInfoEnabled )
					log.Info(__Res.GetString(__Res.SocketServer_Start));
				
				//_threadPoolEx = new ThreadPoolEx();
                _bufferPool = new BufferPool(FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize);

				if(!this.IsDisposed)
				{
                    foreach (RtmpSocketListener socketListener in _socketListeners)
					{
						socketListener.Start();
					}
				}
    			//_idleTimer = new Timer(new TimerCallback(IdleCheck), null, _idleCheckInterval, _idleCheckInterval);

				if( log.IsInfoEnabled )
                    log.Info(__Res.GetString(__Res.SocketServer_Started));
			}
			catch(Exception ex)
			{
				if( log.IsFatalEnabled )
					log.Fatal("SocketServer failed", ex);
			}
		}

		private void StopListeners()
		{
			if(!this.IsDisposed)
			{
				RtmpSocketListener[] socketListeners = GetSocketListeners();
				if( socketListeners != null )
				{
                    foreach (RtmpSocketListener socketListener in socketListeners)
					{
						try
						{
							socketListener.Stop();
							RemoveListener(socketListener);
						}
						catch { }
					}
				}
			}
		}

		private void StopConnections()
		{
			if( !this.IsDisposed )
			{
				RtmpServerConnection[] connections = GetConnections();
                if (connections != null)
				{
                    foreach (RtmpServerConnection connection in connections)
					{
						try
						{
                            connection.Close();
						}
						catch { }
					}
				}
			}
		}

		public void Stop()
		{
			if( !this.IsDisposed )
			{
				try
				{
					if( log.IsInfoEnabled )
                        log.Info(__Res.GetString(__Res.SocketServer_Stopping));
					StopListeners();
                    StopConnections();
                    //if (_threadPoolEx != null)
                    //    _threadPoolEx.Shutdown();
                    if (log.IsInfoEnabled)
                        log.Info(__Res.GetString(__Res.SocketServer_Stopped));
				}
				catch(Exception ex)
				{
					if( log.IsFatalEnabled )
                        log.Fatal(__Res.GetString(__Res.SocketServer_Failed), ex);
				}
			}
		}

        /*
        private void IdleCheck(object state)
		{
            if (!IsDisposed)
            {
                //Disable timer event
                _idleTimer.Change(Timeout.Infinite, Timeout.Infinite);
                try
                {
                    int loopSleep = 0;
                    RtmpServerConnection[] connections = GetConnections();
                    if (connections != null)
                    {
                        foreach (RtmpServerConnection connection in connections)
                        {
                            if (IsDisposed)
                                break;
                            try
                            {
                                if (connection != null && connection.IsActive)
                                {
                                    //Check the idle timeout
                                    if (DateTime.Now > (connection.LastAction.AddMilliseconds(_idleTimeOutValue)))
                                    {
                                        //connection.Close();
                                    }
                                }
                            }
                            finally
                            {
                                ThreadPoolEx.LoopSleep(ref loopSleep);
                            }
                        }
                    }
                }
                finally
                {
                    //Restart the timer event
                    if (!IsDisposed)
                        _idleTimer.Change(_idleCheckInterval, _idleCheckInterval);
                }
            }
		}
        */

		internal RtmpSocketListener[] GetSocketListeners()
		{
            RtmpSocketListener[] socketListeners = null;
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
                    socketListeners = new RtmpSocketListener[_socketListeners.Count];
					_socketListeners.CopyTo(socketListeners, 0);
				}

			}
			return socketListeners;
		}

        internal RtmpServerConnection[] GetConnections()
		{
            RtmpServerConnection[] connections = null;
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
                    connections = new RtmpServerConnection[_connections.Count];
                    _connections.Keys.CopyTo(connections, 0);
				}

			}
            return connections;
		}

        internal void AddConnection(RtmpServerConnection connection)
		{
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
                    _connections[connection] = connection;
				}
			}

		}

        internal void RemoveConnection(RtmpServerConnection connection)
		{
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
                    _connections.Remove(connection);
				}
			}
		}

		public void AddListener(IPEndPoint localEndPoint)
		{
			AddListener(localEndPoint, 1);
		}

		public void AddListener(IPEndPoint localEndPoint, int acceptCount)
		{
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
                    RtmpSocketListener socketListener = new RtmpSocketListener(this, localEndPoint, acceptCount);
					_socketListeners.Add(socketListener);
				}
			}
		}

        public void RemoveListener(RtmpSocketListener socketListener)
		{
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
					_socketListeners.Remove(socketListener);
				}
			}
        }

        #endregion Server Management

        internal void InitializeConnection(Socket socket)
		{
			if(!IsDisposed)
			{
                RtmpServerConnection connection = null;
                if (_serverCertificate == null)
                {
                    connection = new RtmpServerConnection(this, new RtmpNetworkStream(socket));
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.Rtmp_SocketListenerAccept, connection.ConnectionId));
                }
                else
                {
                    SslStream sslStream = new SslStream(new NetworkStream(socket, false), false);
                    //sslStream.AuthenticateAsServer(_serverCertificate, false, SslProtocols.Tls, true);
                    sslStream.AuthenticateAsServer(_serverCertificate, false, SslProtocols.Default, false);
                    connection = new RtmpServerConnection(this, new RtmpNetworkStream(socket, sslStream));
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(__Res.GetString(__Res.Rtmp_SocketListenerAccept, connection.ConnectionId));

                        string msg = string.Format("Cipher: {0} strength {1} Hash: {2} strength {3} Key exchange: {4} strength {5} Protocol: {6} Signed: {7} Encrypted: {8}",
                            sslStream.CipherAlgorithm, sslStream.CipherStrength,
                            sslStream.HashAlgorithm, sslStream.HashStrength,
                            sslStream.KeyExchangeAlgorithm, sslStream.KeyExchangeStrength,
                            sslStream.SslProtocol, sslStream.IsSigned, sslStream.IsEncrypted);
                        log.Debug(msg);
                    }
                }

                //We are still in an IOCP thread 
                this.AddConnection(connection);
                //FluorineRtmpContext.Initialize(connection);
                _rtmpHandler.ConnectionOpened(connection);
                connection.BeginReceive(true);
			}
		}

		/// <summary>
		/// Begin disconnect the connection
		/// </summary>
        internal void OnConnectionClose(RtmpServerConnection connection)
		{
			if(!IsDisposed)
			{
                RemoveConnection(connection);
                //connection.Dispose();
			}
		}

		internal void RaiseOnError(Exception exception)
		{
			if(_onErrorEvent != null)
			{
				_onErrorEvent(this, new ServerErrorEventArgs(exception));
			}
		}
	}

	delegate void ErrorHandler(object sender, ServerErrorEventArgs e); 

	/// <summary>
	/// Base event arguments for connection events.
	/// </summary>
	class ServerErrorEventArgs : EventArgs
	{
		Exception _exception;

		public ServerErrorEventArgs(Exception exception)
		{
			_exception = exception;
		}

		#region Properties

		public Exception Exception
		{
			get { return _exception; }
		}

		#endregion
	}
}
