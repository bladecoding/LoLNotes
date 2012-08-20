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
using FluorineFx.Messaging.Api.Persistence;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Context;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ScopeContext : IScopeContext
	{
		private string _contextPath = string.Empty;
		private IScopeResolver _scopeResolver;
		private IClientRegistry _clientRegistry;
		private IServiceInvoker _serviceInvoker;
		private IPersistenceStore _persistanceStore;

		private ScopeContext()
		{
		}

        public ScopeContext(string contextPath, IClientRegistry clientRegistry, IScopeResolver scopeResolver, IServiceInvoker serviceInvoker, IPersistenceStore persistanceStore)
		{
			_contextPath = contextPath;
			_clientRegistry = clientRegistry;
			_scopeResolver = scopeResolver;
			_persistanceStore = persistanceStore;
            _serviceInvoker = serviceInvoker;
		}

		public string ContextPath
		{
			get{ return _contextPath; }
		}

        #region IScopeContext Members

        public IScopeResolver ScopeResolver
        {
            get { return _scopeResolver; }
        }

        public IClientRegistry ClientRegistry
        {
            get { return _clientRegistry; }
        }

        public IServiceInvoker ServiceInvoker
        {
            get { return _serviceInvoker; }
        }

        public IPersistenceStore PersistenceStore
        {
            get { return _persistanceStore; }
        }

		public IScope ResolveScope(string path)
		{
			return _scopeResolver.ResolveScope(path);
		}

        public IScope ResolveScope(IScope root, string path)
        {
            return _scopeResolver.ResolveScope(root, path);
        }

		public IScope GetGlobalScope()
		{
			return _scopeResolver.GlobalScope;
		}

		public IScopeHandler LookupScopeHandler(string contextPath)
		{
			return null;
		}

        /// <summary>
        /// Return an <see cref="FluorineFx.Context.IResource"/> handle for the
        /// </summary>
        /// <param name="location">The resource location.</param>
        /// <returns>An appropriate <see cref="FluorineFx.Context.IResource"/> handle.</returns>
        public IResource GetResource(string location)
        {
            //return FluorineContext.Current.GetResource(location);
            return new FileSystemResource(location);
        }

		#endregion
	}
}
