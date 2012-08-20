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
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Messaging.Rtmp.Event;

namespace FluorineFx.Net
{
    /// <summary>
    /// This interface supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    interface INetConnectionClient
    {
        void Connect(string command, params object[] arguments);
        void Close();
        bool Connected { get; }
        void Call(string command, IPendingServiceCallback callback, params object[] arguments);
        void Call<T>(string command, Responder<T> responder, params object[] arguments);
        void Call(string endpoint, string destination, string source, string operation, IPendingServiceCallback callback, params object[] arguments);
        void Call<T>(string endpoint, string destination, string source, string operation, Responder<T> responder, params object[] arguments);
        void Write(IRtmpEvent message);
        IConnection Connection { get; }
    }
}
