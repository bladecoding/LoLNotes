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
using FluorineFx.Messaging.Api;
using FluorineFx.Exceptions;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ScopeResolver : IScopeResolver
	{
		protected IGlobalScope _globalScope;

		public ScopeResolver(IGlobalScope globalScope)
		{
			_globalScope = globalScope;
		}

		public IGlobalScope GlobalScope
		{
			get{ return _globalScope; }
		}

		#region IScopeResolver Members

		public IScope ResolveScope(string path)
		{
            // Start from global scope
            return ResolveScope(_globalScope, path);
		}

        /// <summary>
        /// Resolves scope from given root using scope resolver.
        /// </summary>
        /// <param name="root">Scope to start from.</param>
        /// <param name="path">Path to resolve.</param>
        /// <returns></returns>
        public IScope ResolveScope(IScope root, string path)
        {
            // Start from root scope
            IScope scope = root;
            // If there's no path return root scope (e.i. root path scope)
            if (path == null)
            {
                return scope;
            }
            string[] parts = path.Split(new char[] { '/' });
            foreach (string element in parts)
            {
                string room = element;
                if (room == string.Empty)
                {
                    // Skip empty path elements
                    continue;
                }

                if (scope.HasChildScope(room))
                {
                    scope = scope.GetScope(room);
                }
                else if (!scope.Equals(root))
                {
                    // Synchronizing to make sure a subscope with the same name is not created multiple times.
                    lock (scope.SyncRoot)
                    {
                        // Check again as a different thread might have created the child while we waited for the synchronized block.
                        if (scope.HasChildScope(room))
                        {
                            scope = scope.GetScope(room);
                        }
                        else if (scope.CreateChildScope(room))
                        {
                            scope = scope.GetScope(room);
                        }
                        else
                            throw new ScopeNotFoundException(scope, room);
                    }
                }
                else
                    throw new ScopeNotFoundException(scope, room);

			    if (scope is WebScope && ((WebScope)scope).IsShuttingDown) 
                {
				    throw new ScopeShuttingDownException(scope);
			    }
            }
            return scope;
        }

		#endregion
	}
}
