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
using System.Reflection;
#if !SILVERLIGHT
using log4net;
#endif

namespace FluorineFx.Invocation
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class InvocationHandler
	{
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(InvocationHandler));
#endif
		MethodInfo _methodInfo;
		/// <summary>
		/// Initializes a new instance of the InvocationHandler class.
		/// </summary>
		/// <param name="methodInfo"></param>
		public InvocationHandler(MethodInfo methodInfo)
		{
			_methodInfo = methodInfo;
		}

		public object Invoke(object obj, object[] arguments)
		{
#if !SILVERLIGHT
            if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.Invoke_Method, _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name));
#endif

			object result = _methodInfo.Invoke( obj, arguments );

			object[] attributes = _methodInfo.GetCustomAttributes( false );
			if( attributes != null && attributes.Length > 0 )
			{
				InvocationManager invocationManager = new InvocationManager();
				invocationManager.Result = result;
				for(int i = 0; i < attributes.Length; i++)
				{
					Attribute attribute = attributes[i] as Attribute;
					if( attribute is IInvocationCallback )
					{
						IInvocationCallback invocationCallback = attribute as IInvocationCallback;
						invocationCallback.OnInvoked(invocationManager, _methodInfo, obj, arguments, result);
					}
				}
				for(int i = 0; i < attributes.Length; i++)
				{
					Attribute attribute = attributes[i] as Attribute;
					if( attribute is IInvocationResultHandler )
					{
						IInvocationResultHandler invocationResultHandler = attribute as IInvocationResultHandler;
						invocationResultHandler.HandleResult(invocationManager, _methodInfo, obj, arguments, result);
					}
				}
				return invocationManager.Result;
			}
			return result;
		}


	}
}
