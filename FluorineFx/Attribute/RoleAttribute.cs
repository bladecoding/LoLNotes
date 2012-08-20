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

namespace FluorineFx
{
	/// <summary>
	/// Indicates a declarative security check on a service method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class RoleAttribute : System.Attribute
	{
		string _roles;

		/// <summary>
		/// Initializes a new instance of the RoleAttribute class.
		/// </summary>
		/// <param name="roles">Comma-separated list of roles.</param>
        [Obsolete("It is recommended to define security constraints in the security section of the services configuration file.")]
        public RoleAttribute(string roles)
		{
			_roles = roles;
		}
		/// <summary>
		/// Gets the comma-separated list of roles.
		/// </summary>
		public string Roles
		{
			get{ return _roles; }
		}
	}
}
