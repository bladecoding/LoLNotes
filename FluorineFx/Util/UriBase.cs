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
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;

namespace FluorineFx.Util
{
	/// <summary>
	/// Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
	/// protocol://user:password?host:port/path?param1=value&amp;param2=value2&amp;...
	/// </summary>
	public class UriBase
	{
		string _user;
		string _password;
		string _path;
		string _host;
		string _protocol;
		string _port;
		NameValueCollection _parameters;

		/// <summary>
		/// Initializes a new instance of the UriBase class.
		/// </summary>
		/// <param name="uri"></param>
		public UriBase(UriBase uri)
		{
			Clear();
			ParseUri(uri.Uri);
		}
		/// <summary>
		/// Initializes a new instance of the UriBase class.
		/// </summary>
		public UriBase()
		{
			Clear();
		}
		/// <summary>
		/// Initializes a new instance of the UriBase class.
		/// </summary>
		/// <param name="uri"></param>
		public UriBase(string uri)
		{
			Clear();
			ParseUri(uri);
		}
		/// <summary>
		/// Initializes a new instance of the UriBase class.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <param name="path"></param>
		/// <param name="host"></param>
		/// <param name="protocol"></param>
		/// <param name="port"></param>
		/// <param name="parameters"></param>
		public UriBase( string user, string password, string path, string host, string protocol, string port, NameValueCollection parameters )
		{
			_user = user;
			_password = password;
			_path = path;
			_host = host;
			_protocol = protocol;
			_port = port;
			_parameters = parameters;
		}
		/// <summary>
		/// Initializes a new instance of the UriBase class.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <param name="path"></param>
		/// <param name="host"></param>
		/// <param name="protocol"></param>
		/// <param name="port"></param>
		public UriBase( string user, string password, string path, string host, string protocol, string port )
		{
			_user = user;
			_password = password;
			_path = path;
			_host = host;
			_protocol = protocol;
			_port = port;
		}
		/// <summary>
		/// Gets or sets the path specified in the url.
		/// </summary>
		public string Path
		{
			get{ return _path; }
			set
			{ 
				if( _path != value )
					_path = value; 
			}
		}
		/// <summary>
		/// Gets or sets the host specified in the url.
		/// </summary>
		public string Host
		{
			get{ return _host; }
			set
			{ 
				if( _host != value )
					_host = value;
			}
		}
		/// <summary>
		/// Gets or sets the additional parameters specified in the url.
		/// </summary>
		public NameValueCollection Parameters
		{
			get{ return _parameters; }
			set
			{ 
				if( _parameters != value )
					_parameters = value;
			}
		}
		/// <summary>
		/// Gets or sets the host password in the url.
		/// </summary>
		public string Password
		{
			get{ return _password; }
			set
			{ 
				if( _password != value )
					_password = value;
			}
		}
		/// <summary>
		/// Gets or sets the port specified in the url.
		/// </summary>
		public string Port
		{
			get{ return _port; }
			set
			{ 
				if( _port != value )
					_port = value;
			}
		}
		/// <summary>
		/// Gets or sets the protocol specified in the url.
		/// </summary>
		public string Protocol
		{
			get{ return _protocol; }
			set
			{
				if( _protocol != value )
					_protocol = value;
			}
		}
		/// <summary>
		/// Gets or sets user specified in the url.
		/// </summary>
		public string User
		{
			get{ return _user; }
			set
			{
				if( _user != value )
					_user = value;
			}
		}
        /// <summary>
        /// Gets or sets the url.
        /// </summary>
		public string Uri
		{
			get
			{ 
				StringBuilder tempUrl = new StringBuilder();
				if( this.Protocol != null )
				{
					tempUrl.Append(this.Protocol);
					tempUrl.Append("://");
					if( this.User != null && this.User != string.Empty )
						tempUrl.Append(string.Format("{0}:{1}@", this.User, this.Password));
					if( this.Host != null )
						tempUrl.Append(this.Host);
                    if (this.Port != null)
                    {
                        tempUrl.Append(":");
                        tempUrl.Append(this.Port);
                    }
                    if (this.Path != null && this.Path != string.Empty)
						tempUrl.Append(string.Format("/{0}", this.Path));
					else
						tempUrl.Append("/");
				}
				if( _parameters != null )
				{
					for(int i = 0; i < _parameters.Count; i++)
					{
						string key = _parameters.GetKey(i);
						string value = _parameters.Get(i);
						if( i == 0 )
							tempUrl.Append("?");
						else
							tempUrl.Append("&");
						tempUrl.Append(key);
						tempUrl.Append("=");
						tempUrl.Append(value);
					}
				}
				return tempUrl.ToString();
			}
			set
			{
				ParseUri(value);
			}
		}
        /// <summary>
        /// Clears the url.
        /// </summary>
		public void Clear()
		{
			this._protocol = "";
			this._host = "";
			this._port = null;
			this._path = "";
			this._user = "";
			this._password = "";
			this._parameters = null;
		}

		private void InternalParseUri(string url)
		{
			string tempUrl;
			Regex regex;
			Match match;
			string user;
			string password;
			string path;
			string host;
			string protocol;
			string port;
			string parameters;
			NameValueCollection parametersCollection;
			string parameterPart;
			string[] parameter;
			char[] separator;
			IEnumerator enumerator;
			try
			{
				tempUrl = url;
				if (tempUrl.Length == 0)
					tempUrl = ":///";
 
				regex = new Regex(@"^(?<protocol>[\w\%]*)://((?'username'[\w\%]*)(:(?'password'[\w\%]*))?@)?(?'host'[\{\}\w\.\(\)\-\%\\\$]*)(:?(?'port'[\{\}\w\.]+))?(/(?'path'[^?]*)?(\?(?'params'.*))?)?");
				match = regex.Match(tempUrl);
				if (!match.Success)
				{
					throw new ApplicationException("This Uri cannot be parsed.");
				}

				user = HttpUtility.UrlDecode(match.Result("${username}"));
				password = HttpUtility.UrlDecode(match.Result("${password}"));
				path = HttpUtility.UrlDecode(match.Result("${path}"));
				host = HttpUtility.UrlDecode(match.Result("${host}"));
				protocol = HttpUtility.UrlDecode(match.Result("${protocol}"));
				port = null;
				if (match.Result("${port}").Length != 0)
				{
					//port = int.Parse(match.Result("${port}"));
					port = match.Result("${port}");
				}
				//if (port < 0 || port > 65535)
				//	throw new ApplicationException("This Uri cannot be parsed. Invalid Port number.");
 
				parameters = match.Result("${params}");
				parametersCollection = new NameValueCollection();
				if( parameters != null && parameters != string.Empty )
				{
					separator = new char[1]{ '&' };
					string[] splittedParameters = parameters.Split(separator);
					enumerator = splittedParameters.GetEnumerator();
					while (enumerator.MoveNext())
					{
						parameterPart = ((string)enumerator.Current);
						separator = new char[1]{ '=' };
						parameter = parameterPart.Split(separator, 2);
						if (parameter.Length != 2)
						{
							throw new ApplicationException("This Uri cannot be parsed. Invalid parameter.");

						}
						parametersCollection.Add(HttpUtility.UrlDecode(parameter[0]), HttpUtility.UrlDecode(parameter[1])); 
					}
				}
				this._user = user;
				this._password = password;
				this._path = path;
				this._host = host;
				this._protocol = protocol;
				this._port = port;
				this._parameters = parametersCollection; 
			}
			catch (Exception ex)
			{
				if(ex is ApplicationException)
					throw;
				throw new ApplicationException("This Uri cannot be parsed.", ex);
			}
		}

		/// <summary>
		/// Parse the uri.
		/// </summary>
		/// <param name="uri"></param>
		protected void ParseUri(string uri)
		{
			InternalParseUri(uri);
		}

		/// <summary>
		/// Returns whether the value of the called object is equal to that of the given object.
		/// Equality here means if all the fields are the same.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public bool EqualTo(UriBase uri)
		{
			if( uri == null )
				return false;
			return this.Uri == uri.Uri;
		}
		/// <summary>
		/// Copy content of this object into the given object.
		/// </summary>
		/// <param name="uri"></param>
		public void CopyTo(UriBase uri)
		{
			this.Uri = uri.Uri;
		}

	}
}
