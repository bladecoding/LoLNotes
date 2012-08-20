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
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Api.SO
{
	/// <summary>
	/// Service that manages shared objects.
	/// </summary>
	[CLSCompliant(false)]
    public interface ISharedObjectService : IScopeService
	{
        /// <summary>
        /// Returns a collection of available SharedObject names.
        /// </summary>
        /// <param name="scope">Scope that SharedObjects belong to.</param>
        /// <returns>Collection of available SharedObject names.</returns>
        ICollection GetSharedObjectNames(IScope scope);
        /// <summary>
        /// Creates a new shared object for given scope. Server-side shared objects
        /// (also known as Remote SO) are special kind of objects synchronized between clients.
        /// 
        /// To get an instance of RSO at client-side, use <code>SharedObject.getRemote()</code>.
        /// 
        /// SharedObjects can be persistent and transient. Persistent RSO are stateful, i.e. store their data between sessions.
        /// If you need to store some data on server while clients go back and forth use persistent SO, otherwise perfer usage of transient for
        /// extra performance.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <param name="persistent">Whether SharedObject instance should be persistent or not.</param>
        /// <returns>true if SO was created, false otherwise.</returns>
        bool CreateSharedObject(IScope scope, string name, bool persistent);
        /// <summary>
        /// Returns shared object from given scope by name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>Shared object instance with the specifed name.</returns>
        ISharedObject GetSharedObject(IScope scope, string name);
        /// <summary>
        /// Returns shared object from given scope by name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <param name="persistent">Whether SharedObject instance should be persistent or not.</param>
        /// <returns>Shared object instance with the specifed name.</returns>
        ISharedObject GetSharedObject(IScope scope, string name, bool persistent);
        /// <summary>
        /// Checks whether there is a SharedObject with given scope and name.
        /// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>true if SharedObject exists, false otherwise.</returns>
        bool HasSharedObject(IScope scope, string name);
		/// <summary>
		/// Deletes persistent shared objects specified by name and clears all
		/// properties from active shared objects (persistent and nonpersistent). The
		/// name parameter specifies the name of a shared object, which can include a
		/// slash (/) as a delimiter between directories in the path. The last
		/// element in the path can contain wildcard patterns (for example, a
		/// question mark [?] and an asterisk [*]) or a shared object name. The
		/// ClearSharedObjects() method traverses the shared object hierarchy along
		/// the specified path and clears all the shared objects. Specifying a slash
		/// (/) clears all the shared objects associated with an application
		/// instance.
		/// 
		/// The following values are possible for the soPath parameter: <br />
		/// clears all local and persistent shared objects associated with the instance. <br />
		/// /foo/bar clears the shared object /foo/bar; if bar is a directory name, no shared objects are deleted. <br />
		/// /foo/bar/* clears all shared objects stored under the instance directory
		/// /foo/bar. The bar directory is also deleted if no persistent shared objects are in use within this namespace. <br />
		/// /foo/bar/XX?? clears all shared objects that begin with XX, followed by any two characters. 
		/// If a directory name matches this specification, all the shared objects within this directory are cleared.
		/// 
		/// If you call the ClearSharedObjects() method and the specified path
		/// matches a shared object that is currently active, all its properties are
		/// deleted, and a "clear" event is sent to all subscribers of the shared
		/// object. If it is a persistent shared object, the persistent store is also cleared.
		/// </summary>
        /// <param name="scope">Scope that shared object belongs to.</param>
        /// <param name="name">Name of SharedObject.</param>
        /// <returns>true if successful, false otherwise.</returns>
        bool ClearSharedObjects(IScope scope, string name);
	}
}
