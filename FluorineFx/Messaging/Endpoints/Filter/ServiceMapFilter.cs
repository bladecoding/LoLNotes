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
using log4net;
using FluorineFx.Exceptions;
using FluorineFx.Diagnostic;
using FluorineFx.Security;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Messaging;
using FluorineFx.Configuration;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ServiceMapFilter : AbstractFilter
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceMapFilter));
		EndpointBase _endpoint;

		/// <summary>
		/// Initializes a new instance of the ServiceMapFilter class.
		/// </summary>
		/// <param name="endpoint"></param>
		public ServiceMapFilter(EndpointBase endpoint)
		{
			_endpoint = endpoint;
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			for(int i = 0; i < context.AMFMessage.BodyCount; i++)
			{
				AMFBody amfBody = context.AMFMessage.GetBodyAt(i);

				if( !amfBody.IsEmptyTarget )
				{//Flash
					if( FluorineConfiguration.Instance.ServiceMap != null )
					{

						string typeName = amfBody.TypeName;
						string method = amfBody.Method;
						if( typeName != null && FluorineConfiguration.Instance.ServiceMap.Contains(typeName) )
						{
							string serviceLocation = FluorineConfiguration.Instance.ServiceMap.GetServiceLocation(typeName);
							method = FluorineConfiguration.Instance.ServiceMap.GetMethod(typeName, method);
							string target = serviceLocation + "." + method;
							if( log != null && log.IsDebugEnabled )
								log.Debug(__Res.GetString(__Res.Service_Mapping, amfBody.Target, target));
							amfBody.Target = target;
						}
					}
				}
			}
		}

		#endregion
	}
}
