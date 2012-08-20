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
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using log4net;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Api;
using FluorineFx.Util;
using FluorineFx.Messaging.Messages;
using FluorineFx.Exceptions;
using FluorineFx.Context;
using FluorineFx.Messaging.Endpoints;

namespace FluorineFx.Context
{
    class SessionManager : ISessionListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SessionManager));
        object _objLock = new object();

        MessageBroker _messageBroker;
        Hashtable _sessions;

        private SessionManager()
        {
        }

        internal SessionManager(MessageBroker messageBroker)
		{
            _messageBroker = messageBroker;
            _sessions = new Hashtable();
		}

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _objLock; } }

        public HttpSession GetHttpSession(HttpContext context)
        {
            HttpSession httpSession = null;
            if (context != null && context.Session != null)
            {
                lock (this.SyncRoot)
                {
                    if (_sessions.ContainsKey(context.Session.SessionID))
                    {
                        httpSession = GetSession(context.Session.SessionID) as HttpSession;
                    }
                    else
                    {
                        httpSession = new HttpSession(this, context.Session.SessionID);
                        log.Debug(__Res.GetString(__Res.Session_Create, httpSession.Id));
                        httpSession.AddSessionDestroyedListener(this);
                        context.Session[HttpSession.FluorineSessionAttribute] = httpSession.Id;
                        _sessions[context.Session.SessionID] = httpSession;
                        Renew(httpSession, context.Session.Timeout);
                    }
                }
            }
            //Session state is not enabled, <sessionState mode="Off"/>
            return httpSession;
        }

        public HttpSession GetHttpSession(HttpContext context, string sessionID)
        {
            // By design: the lock is on session state. Only one request (to the same session) is allowed at the same time.
            // The reason for this is because there is no locking code for accessing an object in session.
            HttpSession httpSession = null;
            if (context != null)
            {
                lock (this.SyncRoot)
                {
                    if (_sessions.ContainsKey(sessionID))
                    {
                        httpSession = GetSession(sessionID) as HttpSession;
                    }
                    else
                    {
                        httpSession = new HttpSession(this, sessionID);//Never handled as a new Session
                        log.Debug(__Res.GetString(__Res.Session_Create, httpSession.Id));
                        httpSession.AddSessionDestroyedListener(this);
                        _sessions[httpSession.Id] = httpSession;
                        //Renew(httpSession, context.Session.Timeout);
                    }
                }
            }
            return httpSession;
        }

        private bool IsNewHttpSession()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session.IsNewSession)
                {

                    string cookieHeader = HttpContext.Current.Request.Headers["Cookie"];
                    if ((null != cookieHeader) && (cookieHeader.IndexOf(HttpSession.AspNetSessionIdCookie) >= 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public ISession GetSession(string sessionId)
        {
            lock (this.SyncRoot)
            {
                if (_sessions.ContainsKey(sessionId))
                {
                    HttpRuntime.Cache.Get(sessionId);
                    return _sessions[sessionId] as ISession;
                }
            }
            return null;
        }

        public ISession CreateSession(IConnection connection)
        {
            lock (this.SyncRoot)
            {
                ConnectionSession session = new ConnectionSession(this, connection);
                log.Debug(__Res.GetString(__Res.Session_Create, session.Id));
                session.AddSessionDestroyedListener(this);
                _sessions[session.Id] = session;
                return session;
            }
        }

        internal void Renew(ISession session, int leaseTime)
        {
            lock (this.SyncRoot)
            {
                _sessions[session.Id] = session;
                log.Debug(__Res.GetString(__Res.Session_Lease, session.Id, 0, leaseTime));
                HttpRuntime.Cache.Remove(session.Id);
                if (leaseTime != 0)
                {
                    // Add the Session to the Cache with the expiration item
                    HttpRuntime.Cache.Insert(session.Id, session, null,
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, leaseTime, 0),
                        CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(this.RemovedCallback));
                }
            }
        }

        public ISession RemoveSession(ISession session)
        {
            lock (this.SyncRoot)
            {
                if (_sessions.ContainsKey(session.Id))
                {
                    if (log.IsDebugEnabled)
                        log.Debug(__Res.GetString(__Res.SessionManager_Remove, session.Id));
                    HttpRuntime.Cache.Remove(session.Id);
                    _sessions.Remove(session.Id);
                }
                return session;
            }
        }

        public void CancelTimeout(ISession session)
        {
            HttpRuntime.Cache.Remove(session.Id);
        }

        internal void RemovedCallback(string key, object value, CacheItemRemovedReason callbackReason)
        {
            if (callbackReason == CacheItemRemovedReason.Expired)
            {
                lock (this.SyncRoot)
                {
                    if (_sessions.Contains(key))
                    {
                        try
                        {
                            ISession session = GetSession(key);
                            if (session != null)
                            {
                                if (log.IsDebugEnabled)
                                    log.Debug(__Res.GetString(__Res.SessionManager_CacheExpired, session.Id));
                                _TimeoutContext context = new _TimeoutContext(session);
                                FluorineWebSafeCallContext.SetData(FluorineContext.FluorineContextKey, context);
                                RemoveSession(session);
                                session.Timeout();
                                FluorineWebSafeCallContext.FreeNamedDataSlot(FluorineContext.FluorineContextKey);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (log.IsErrorEnabled)
                                log.Error(__Res.GetString(__Res.SessionManager_CacheExpired, key), ex);
                        }
                    }
                }
            }
        }

        #region ISessionListener Members

        public void SessionCreated(ISession session)
        {
            //NOP
        }

        public void SessionDestroyed(ISession session)
        {
            RemoveSession(session);
        }

        #endregion
    }
}
