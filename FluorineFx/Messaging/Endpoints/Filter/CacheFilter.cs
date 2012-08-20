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
using System.Configuration;
using System.Web;
using log4net;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Configuration;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class CacheFilter : AbstractFilter
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(CacheFilter));

		public CacheFilter()
		{
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			MessageOutput messageOutput = context.MessageOutput;
			if( FluorineConfiguration.Instance.CacheMap != null && FluorineConfiguration.Instance.CacheMap.Count > 0 )
			{
				for(int i = 0; i < context.AMFMessage.BodyCount; i++)
				{
					AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
					//Check if response exists.
					ResponseBody responseBody = messageOutput.GetResponse(amfBody);
					if( responseBody != null )
					{
						//AuthenticationFilter may insert response.
						continue;
					}

					if( !amfBody.IsEmptyTarget )
					{
						string source = amfBody.Target;
						IList arguments = amfBody.GetParameterList();
						string key = FluorineFx.Configuration.CacheMap.GenerateCacheKey(source, arguments);
						//Flash message
						if( FluorineConfiguration.Instance.CacheMap.ContainsValue(key) )
						{
							object cachedContent = FluorineConfiguration.Instance.CacheMap.Get(key);

							if( log != null && log.IsDebugEnabled )
								log.Debug( __Res.GetString(__Res.Cache_HitKey, amfBody.Target, key));
							
							CachedBody cachedBody = new CachedBody(amfBody, cachedContent);
							messageOutput.AddBody(cachedBody);
						}
					}
				}
			}
		}

		#endregion

	}
}
