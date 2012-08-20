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
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using log4net;
using FluorineFx;
using FluorineFx.IO;
using FluorineFx.Context;
using FluorineFx.Exceptions;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Endpoints;

namespace FluorineFx.Security
{
    class LoginManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginManager));

        private const int NoLoginCommand = 10053;

        ILoginCommand _loginCommand;
        static string FormsAuthCookieName;

        public ILoginCommand LoginCommand
        {
            get { return _loginCommand; }
            set { _loginCommand = value; }
        }
        bool _perClientAuthentication;

        public bool IsPerClientAuthentication
        {
            get { return _perClientAuthentication; }
            set { _perClientAuthentication = value; }
        }

        static LoginManager()
        {
            FormsAuthCookieName = Environment.UserInteractive ? ".ASPXAUTH" : FormsAuthentication.FormsCookieName;
        }

        public LoginManager()
        {
        }
        /// <summary>
        /// The gateway calls this method to perform programmatic, custom authentication.
        /// </summary>
        /// <param name="username">The principal being authenticated.</param>
        /// <param name="credentials">The credentials are passed as a dictionary to allow for extra properties to be passed in the future. For now, only a "password" property is sent.</param>
        /// <returns>A principal object represents the security context of the user.</returns>
        public IPrincipal Login(string username, IDictionary credentials)
        {
            if (_loginCommand != null)
            {
                IPrincipal principal = _loginCommand.DoAuthentication(username, credentials);
                if (principal == null)
                    throw new SecurityException(__Res.GetString(__Res.Security_AccessNotAllowed));
                this.Principal = principal;
                System.Threading.Thread.CurrentPrincipal = principal;
                // Attach the new principal object to the current Context object
                if (HttpContext.Current != null)
                    HttpContext.Current.User = principal;
                return principal;
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error(__Res.GetString(__Res.Security_LoginMissing));
                throw new UnauthorizedAccessException(__Res.GetString(__Res.Security_LoginMissing));
            }
        }

        string EncryptCredentials(string userId, string password)
        {
            //string uniqueKey = endpoint.Id;//HttpContext.Current.Session.SessionID;
            HttpCookie cookie = FormsAuthentication.GetAuthCookie("fluorine", false);
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

            string cacheKey = string.Join("|", new string[] { GenericLoginCommand.FluorineTicket, /*uniqueKey,*/ userId, password });
            // Store the Guid inside the Forms Ticket with all the attributes aligned with the config Forms section.
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, cacheKey, ticket.CookiePath);
            return FormsAuthentication.Encrypt(newTicket);
        }

        public void RestorePrincipal()
        {
            if (this.Principal == null)
            {
                //If user is already authenticated
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request.IsAuthenticated)
                    {
                        /*
                        if (HttpContext.Current.User.Identity is FormsIdentity)
                        {
                            FormsIdentity formsIdentity = HttpContext.Current.User.Identity as FormsIdentity;
                            if (formsIdentity.Ticket.UserData == null || !formsIdentity.Ticket.UserData.StartsWith(FluorineContext.FluorineTicket))
                                this.Principal = HttpContext.Current.User;
                        }
                        else
                            this.Principal = HttpContext.Current.User;
                        */
                        this.Principal = HttpContext.Current.User;
                    }
                    /*
                    HttpCookie authCookie = HttpContext.Current.Request.Cookies.Get(FormsAuthCookieName);
                    if (authCookie != null)
                    {
                        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        if (ticket != null)
                        {
                            if (ticket.UserData != null && ticket.UserData.StartsWith(FluorineContext.FluorineTicket))
                            {
                                //Get the principal as the cache lost the data
                                string[] userData = ticket.UserData.Split(new char[] { '|' });
                                string userId = userData[2];
                                string password = userData[3];
                                Hashtable credentials = new Hashtable(1);
                                credentials["password"] = password;
                                Login(userId, credentials);
                            }
                        }
                        else
                        {
                            //This is not our cookie so rely on application's authentication
                            this.Principal = System.Threading.Thread.CurrentPrincipal;
                        }
                    }
                    */
                }
            }
            System.Threading.Thread.CurrentPrincipal = this.Principal;
            // Attach the new principal object to the current Context object
            if (HttpContext.Current != null)
                HttpContext.Current.User = this.Principal;
        }

        public void RestorePrincipal(string key)
        {
            if (this.Principal == null)
            {
                if (key != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(key);
                    if (ticket != null)
                    {
                        //Get the principal as the cache lost the data
                        string[] userData = ticket.UserData.Split(new char[] { '|' });
                        string userId = userData[2];
                        string password = userData[3];
                        Hashtable credentials = new Hashtable(1);
                        credentials["password"] = password;
                        Login(userId, credentials);
                    }
                }
            }
            System.Threading.Thread.CurrentPrincipal = this.Principal;
            if (HttpContext.Current != null)
                HttpContext.Current.User = this.Principal;
        }

        public bool Logout()
        {
            bool result = false;
            if (_loginCommand != null)
            {
                result = _loginCommand.Logout(this.Principal);
                if (this.IsPerClientAuthentication)
                {
                    if (FluorineContext.Current.Client != null)
                        FluorineContext.Current.Client.Principal = null;
                }
                else
                {
                    if (FluorineContext.Current.Session != null)
                        FluorineContext.Current.Session.Invalidate();
                }
            }
            else
            {
                if (FluorineContext.Current.Session != null)
                    FluorineContext.Current.Session.Invalidate();

                if (log.IsErrorEnabled)
                    log.Error(__Res.GetString(__Res.Security_LoginMissing));
                //FluorineFx.Messaging.SecurityException se = new FluorineFx.Messaging.SecurityException(NoLoginCommand, FluorineFx.Messaging.SecurityException.ServerAuthorizationCode);
                //throw se;
                throw new UnauthorizedAccessException(__Res.GetString(__Res.Security_LoginMissing));
            }
            if (HttpContext.Current != null)
            {
                /*
                HttpCookie authCookie = HttpContext.Current.Request.Cookies.Get(FormsAuthCookieName);
                if (authCookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (ticket != null && ticket.UserData != null && ticket.UserData.StartsWith(FluorineContext.FluorineTicket))
                    {
                        HttpRuntime.Cache.Remove(ticket.UserData);
                    }
                }
                */
                FormsAuthentication.SignOut();

            }
            if (AMFContext.Current != null)
            {
                AMFContext amfContext = AMFContext.Current;
                AMFHeader amfHeader = amfContext.AMFMessage.GetHeader(AMFHeader.CredentialsIdHeader);
                if (amfHeader != null)
                {
                    amfContext.AMFMessage.RemoveHeader(AMFHeader.CredentialsIdHeader);
                    ASObject asoObjectCredentialsId = new ASObject();
                    asoObjectCredentialsId["name"] = AMFHeader.CredentialsIdHeader;
                    asoObjectCredentialsId["mustUnderstand"] = false;
                    asoObjectCredentialsId["data"] = null;//clear
                    AMFHeader headerCredentialsId = new AMFHeader(AMFHeader.RequestPersistentHeader, true, asoObjectCredentialsId);
                    amfContext.MessageOutput.AddHeader(headerCredentialsId);
                }
            } 
            return result;
        }

        public IPrincipal Principal
        {
            get 
            {
                if (FluorineContext.Current != null)
                {
                    if (this.IsPerClientAuthentication)
                    {
                        if (FluorineContext.Current.Client != null)
                            return FluorineContext.Current.Client.Principal;
                    }
                    else
                    {
                        if (FluorineContext.Current.Session != null)
                            return FluorineContext.Current.Session.Principal;
                        else
                        {
                            if (log.IsWarnEnabled)
                            {
                                string msg = "The operation cannot complete because session state cannot be acquired (check if ASP.NET session state is disabled)";
                                log.Warn(msg);
                                msg = "If session state is disabled try using per-client authentication in the LoginCommand section";
                                log.Warn(msg);
                            }
                        }
                    }
                }
                return null;
            }
            set 
            {
                if (FluorineContext.Current != null)
                {
                    if (this.IsPerClientAuthentication)
                    {
                        Debug.Assert(FluorineContext.Current != null && FluorineContext.Current.Client != null);
                        if (FluorineContext.Current.Client != null)
                        {
                            FluorineContext.Current.Client.Principal = value;
                        }
                    }
                    else
                    {
                        //Debug.Assert(FluorineContext.Current != null && FluorineContext.Current.Session != null);
                        if (FluorineContext.Current.Session != null)
                        {
                            FluorineContext.Current.Session.Principal = value;
                        }
                        else
                        {
                            if (log.IsWarnEnabled)
                            {
                                string msg = "The operation cannot complete because session state cannot be acquired (check if ASP.NET session state is disabled)";
                                log.Warn(msg);
                                msg = "If session state is disabled try using per-client authentication in the LoginCommand section";
                                log.Warn(msg);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks security for the given destination instance.
        /// </summary>
        /// <param name="destination">The destination that should process messages.</param>
        public void CheckSecurity(Destination destination)
        {
            if (destination == null)
                throw new FluorineException(__Res.GetString(__Res.Invalid_Destination, "null"));
            if (destination.DestinationDefinition != null)
            {
                string[] roles = destination.DestinationDefinition.GetRoles();
                if (roles != null)
                {
                    bool authorized = DoAuthorization(roles);
                    if (!authorized)
                        throw new UnauthorizedAccessException(__Res.GetString(__Res.Security_AccessNotAllowed));
                }
            }
        }

        /// <summary>
        /// Performs authorization for the current user.
        /// </summary>
        /// <param name="roles">List of roles.</param>
        /// <returns>true if the current user is authorized, otherwise, false.</returns>
        /// <remarks>
        /// If Thread.CurrentPrincipal is not set this method will throw an UnauthorizedAccessException.
        /// If the MessageBroker of this Destination does not have a valid Login Command this method will throw an UnauthorizedAccessException.
        /// </remarks>
        public bool DoAuthorization(string[] roles)
        {
            if (this.Principal == null)
                throw new SecurityException(__Res.GetString(__Res.Security_AccessNotAllowed));
            if (_loginCommand != null)
            {
                bool authorized = _loginCommand.DoAuthorization(this.Principal, roles);
                return authorized;
            }
            else
            {
                //Do not force the use of a LoginCommand if we have custom authentication performed in the asp.net application
                if (this.Principal.Identity != null && this.Principal.Identity.IsAuthenticated)
                {
                    foreach (string role in roles)
                    {
                        if (this.Principal.IsInRole(role))
                            return true;
                    }
                }
                throw new UnauthorizedAccessException(__Res.GetString(__Res.Security_LoginMissing));
            }
        }
    }
}
