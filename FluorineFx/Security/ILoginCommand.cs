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
using System.Security;
using System.Security.Principal;

namespace FluorineFx.Security
{
	/// <summary>
	/// Custom login adapter interface used for custom authentication.
	/// Checks a user's credentials and retrieves a principal for the gateway.
	/// </summary>
	public interface ILoginCommand
	{
		//The class name of the implementation of this interface is configured in the gateway configuration's security section and is instantiated using reflection

		/// <summary>
		/// Called to initialize a login command prior to authentication/authorization requests.
		/// </summary>
		void Start();
		/// <summary>
		/// Called to free up resources used by the login command.
		/// </summary>
		void Stop();
		/// <summary>
		/// Attempts to log a user out from their session.
		/// </summary>
		/// <param name="principal">The principal to logout.</param>
		/// <returns>A Boolean value indicating whether the principal has been logged out.</returns>
		bool Logout(IPrincipal principal);
		/// <summary>
		/// The gateway calls this method to perform programmatic authorization.
		/// </summary>
		/// <param name="principal">The principal being checked for authorization.</param>
		/// <param name="roles">A List of role names to check, all members should be strings.</param>
		/// <returns>A Boolean value indicating whether the principal has been authorized.</returns>
		bool DoAuthorization(IPrincipal principal, IList roles);
		/// <summary>
		/// The gateway calls this method to perform programmatic, custom authentication.
		/// </summary>
		/// <param name="username">The principal being authenticated.</param>
        /// <param name="credentials">The credentials are passed as a dictionary to allow for extra properties to be passed in the future. For now, only a "password" property is sent.</param>
		/// <returns>A principal object that represents the security context of the user.</returns>
		IPrincipal DoAuthentication(string username, IDictionary credentials);
	}
}
