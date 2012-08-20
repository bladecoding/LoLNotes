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
using System.Web.Security;
using System.Web.Caching;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading;
using System.Security.Cryptography;
using FluorineFx.IO;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Security;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Context
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class FluorineWebContext : FluorineContext
	{
        private FluorineWebContext()
		{
        }

        internal static void Initialize()
        {
            HttpContext.Current.Items[FluorineContext.FluorineContextKey] = new FluorineWebContext();
        }
		/// <summary>
		/// Gets a key-value collection that can be used to organize and share data between an IHttpModule and an IHttpHandler during an HTTP request.
		/// </summary>
		public IDictionary Items
		{ 
			get{ return HttpContext.Current.Items; }
		}
		/// <summary>
		/// Gets the physical drive path of the application directory for the application hosted in the current application domain.
		/// </summary>
		public override string RootPath
		{ 
			get
			{
				return HttpRuntime.AppDomainAppPath;
			}
		}

		/// <summary>
		/// Gets the virtual path of the current request.
		/// </summary>
		public override string RequestPath 
		{ 
			get { return HttpContext.Current.Request.Path; }
		}
		/// <summary>
		/// Gets the ASP.NET application's virtual application root path on the server.
		/// </summary>
		public override string RequestApplicationPath
		{ 
			get { return HttpContext.Current.Request.ApplicationPath; }
		}

		public override string PhysicalApplicationPath
		{ 
			get
			{
				return HttpContext.Current.Request.PhysicalApplicationPath;
			}
		}

        public override string ApplicationPath 
        {
            get
            {
                string applicationPath = "";
                //if (httpApplication.Request.Url != null)
                // Nick Farina: We need to cast to object first because the mono framework doesn't 
                // have the Uri.operator!=() method that the MS compiler adds. 
                if ((object)HttpContext.Current.Request.Url != null)
                    applicationPath = HttpContext.Current.Request.Url.AbsoluteUri.Substring(
                        0, HttpContext.Current.Request.Url.AbsoluteUri.ToLower().IndexOf(
                        HttpContext.Current.Request.ApplicationPath.ToLower(),
                        HttpContext.Current.Request.Url.AbsoluteUri.ToLower().IndexOf(
                        HttpContext.Current.Request.Url.Authority.ToLower()) +
                        HttpContext.Current.Request.Url.Authority.Length) +
                        HttpContext.Current.Request.ApplicationPath.Length);
                return applicationPath;
            }
        }
		/// <summary>
		/// Gets the absolute URI from the URL of the current request.
		/// </summary>
		public override string AbsoluteUri
		{ 
			get{ return HttpContext.Current.Request.Url.AbsoluteUri; }
		}

		public override string ActivationMode
		{ 
			get
			{
				//if( Environment.UserInteractive )
				//	return null;
				try
				{
					if( HttpContext.Current != null )
						return HttpContext.Current.Request.QueryString["activate"] as string;
				}
				catch(HttpException)//Request is not available in this context
				{
				}
				return null;
			}
		}

        /*
		public static string GetFormsAuthCookieName()
		{
			string formsCookieName = Environment.UserInteractive ? ".ASPXAUTH" : FormsAuthentication.FormsCookieName;
			return formsCookieName;
		}
        */
	}
}
