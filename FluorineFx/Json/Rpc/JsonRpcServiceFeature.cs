//JSON RPC based on Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
//http://jayrock.berlios.de/
using System;
using System.ComponentModel;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;
using FluorineFx.Messaging;
using FluorineFx.Util;
using FluorineFx.Json.Services;

namespace FluorineFx.Json.Rpc
{
    abstract class JsonRpcServiceFeature : IHttpHandler
    {
        private HttpContext _context;
        private readonly MessageBroker _messageBroker;

        protected JsonRpcServiceFeature(MessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public HttpContext Context
        {
            get { return _context; }
        }

        public HttpApplication ApplicationInstance
        {
            get { return Context.ApplicationInstance; }
        }

        public HttpApplicationState Application
        {
            get { return Context.Application; }
        }

        public HttpServerUtility Server
        {
            get { return Context.Server; }
        }

        public HttpSessionState Session
        {
            get { return Context.Session; }
        }

        public HttpRequest Request
        {
            get { return Context.Request; }
        }

        public HttpResponse Response
        {
            get { return Context.Response; }
        }

        public IPrincipal User
        {
            get { return Context.User; }
        }

        public virtual MessageBroker MessageBroker
        {
            get { return _messageBroker; }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            _context = context;
            ProcessRequest();
        }

        protected abstract void ProcessRequest();

        //
        // NOTE! IsReusable is discouraged from being overridden by a subclass
        // because this implementation assumes that the context will only be
        // established once per request.
        //

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
