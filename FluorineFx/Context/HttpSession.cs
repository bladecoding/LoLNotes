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
using System.Web;
using System.Security.Principal;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp;

namespace FluorineFx.Context
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class HttpSession : Session
    {
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineSessionAttribute = "__@fluorinesession";
        public const string AspNetSessionIdCookie = "ASP.NET_SessionId";

        private HttpSession(SessionManager sessionManager)
            : base(sessionManager)
        {
        }

        public HttpSession(SessionManager sessionManager, string id)
            : base(sessionManager, id)
        {
        }

        #region ISession Members

        public override void Add(string name, object value)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Add(name, value);
        }

        public override void Clear()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Clear();
        }

        public override void Remove(string name)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Remove(name);
        }

        public override void RemoveAll()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.RemoveAll();
        }

        public override object this[string name]
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    return HttpContext.Current.Session[name];
                return null;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    HttpContext.Current.Session[name] = value;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Abandon();
            }
        }

        #endregion


        #region ICollection Members

        public override void CopyTo(Array array, int index)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                HttpContext.Current.Session.CopyTo(array, index);
        }

        public override int Count
        {
            get 
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    return HttpContext.Current.Session.Count;
                return 0;
            }
        }

        public override bool IsSynchronized
        {
            get 
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    return HttpContext.Current.Session.IsSynchronized;
                return false;
            }
        }

        #endregion

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                return HttpContext.Current.Session.GetEnumerator();
            return null;
        }

        #endregion
    }
}
