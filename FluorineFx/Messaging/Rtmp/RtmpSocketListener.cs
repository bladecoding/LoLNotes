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
using System.Net.Sockets;
using System.Threading;
using System.Configuration;
using log4net;
using FluorineFx.Util;
using FluorineFx.Threading;
using FluorineFx.Configuration;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpSocketListener : DisposableBase
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(RtmpSocketListener));
        private IPEndPoint _endPoint;
        private Socket _socket;
        private int _acceptCount;
        RtmpServer _rtmpServer;

        public RtmpSocketListener(RtmpServer rtmpServer, IPEndPoint endPoint, int acceptCount)
		{
            _rtmpServer = rtmpServer;
			_endPoint = endPoint;
			_acceptCount = acceptCount;
		}

		protected override void Free()
		{
            try
            {
                _socket.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
		}

        internal Socket Socket { get { return _socket; } }
        internal RtmpServer RtmpServer { get { return _rtmpServer; } }

		public void Start()
		{
			try
			{
                _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_socket.Bind(_endPoint);
                //Set to SOMAXCONN (0x7FFFFFFF), the underlying service provider will set the backlog to a maximum reasonable value
                _socket.Listen(0x7FFFFFFF);
                int loopCount = 0;
				for(int i = 0; i < _acceptCount; i++)
				{
					_socket.BeginAccept(new AsyncCallback(BeginAcceptCallback), this);
                    ThreadPoolEx.LoopSleep(ref loopCount);
				}
			}
			catch(Exception ex)
			{
                //log.Fatal(__Res.GetString(__Res.SocketServer_ListenerFail), ex);
                HandleError(ex);
			}
		}

		public void Stop()
		{
		}

		internal void BeginAcceptCallback(IAsyncResult ar)
		{
			if(!this.IsDisposed)
			{
                RtmpSocketListener listener = ar.AsyncState as RtmpSocketListener;
                try
                {
                    //Get accepted socket
                    Socket acceptedSocket = listener.Socket.EndAccept(ar);
                    if (acceptedSocket.Connected == false)
                        return;

#if !(NET_1_1)
                    acceptedSocket.NoDelay = FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.TcpNoDelay;//true;
#endif
                    //Adjust buffer size
#if NET_1_1
					try
					{
					    acceptedSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize);
                    }
					catch(SocketException ex)
					{
                        log.Warn(__Res.GetString(__Res.SocketServer_SocketOptionFail, "ReceiveBuffer"), ex);
					}
					try
					{
					    acceptedSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.SendBufferSize);
                    }
					catch(SocketException ex)
					{
                        log.Warn(__Res.GetString(__Res.SocketServer_SocketOptionFail, "SendBuffer"), ex);
					}
#else
                    acceptedSocket.ReceiveBufferSize = FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.ReceiveBufferSize;
                    acceptedSocket.SendBufferSize = FluorineConfiguration.Instance.FluorineSettings.RtmpServer.RtmpTransportSettings.SendBufferSize;
#endif

                    listener.RtmpServer.InitializeConnection(acceptedSocket);
                }
                catch (Exception ex)
                {
                    //log.Error(__Res.GetString(__Res.SocketServer_ListenerFail), ex);
                    if (HandleError(ex))
                        listener.RtmpServer.RaiseOnError(ex);
                }
                finally
                {
                    //Continue to accept
                    listener.Socket.BeginAccept(new AsyncCallback(BeginAcceptCallback), listener);
                }
			}
		}

        private bool HandleError(Exception exception)
        {
            SocketException socketException = exception as SocketException;
            if (exception.InnerException != null && exception.InnerException is SocketException)
                socketException = exception.InnerException as SocketException;

            bool error = true;
            if (socketException != null && socketException.ErrorCode == 995)//WSA_OPERATION_ABORTED (The I/O operation has been aborted because of either a thread exit or an application request)
                error = false;

            if (error && log.IsErrorEnabled)
                log.Error(__Res.GetString(__Res.SocketServer_ListenerFail), exception);
            return error;
        }
	}
}
