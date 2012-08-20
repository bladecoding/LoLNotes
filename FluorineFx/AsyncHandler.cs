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
using System.Threading;
using System.Web;
using FluorineFx.Threading;

namespace FluorineFx
{
    class AsyncHandler : IAsyncResult
    {
        private bool _completed;
        private readonly Object _state;
        private readonly AsyncCallback _callback;
        private HttpApplication _httpApplication;
        FluorineGateway _gateway;

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return null; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return _completed; }
        }

        #endregion

        public AsyncHandler(AsyncCallback callback, FluorineGateway gateway, HttpApplication httpApplication, Object state)
        {
            _gateway = gateway;
            _callback = callback;
            _httpApplication = httpApplication;
            _state = state;
            _completed = false;
        }

        public void Start()
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncTask), null);
            ThreadPoolEx.Global.QueueUserWorkItem(AsyncTask, null);
        }

        private void AsyncTask(object state)
        {
            // Restore HttpContext
            //CallContext.SetData("HttpContext", _httpApplication.Context);
            HttpContext.Current = _httpApplication.Context;
            _gateway.ProcessRequest(_httpApplication);
            _gateway = null;
            _httpApplication = null;
            _completed = true;
            _callback(this);
        }
    }
}
