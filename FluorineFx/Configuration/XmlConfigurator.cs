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
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Configuration;

// Import log4net classes.
using log4net;
using log4net.Config;

namespace FluorineFx.Configuration
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class XmlConfigurator : IConfigurationSectionHandler
	{
		public XmlConfigurator()
		{
		}

		#region IConfigurationSectionHandler Members

		public object Create(object parent, object configContext, XmlNode section)
		{
			ILog _log = null;
			try
			{
				_log = LogManager.GetLogger(typeof(XmlConfigurator));
			}
			catch{}

			object settings = null;
			if (section == null) 
				return settings;

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FluorineSettings));
			XmlNodeReader reader = new XmlNodeReader(section);
			try
			{
				settings = xmlSerializer.Deserialize(reader);
			}
			catch(Exception ex)
			{
				if( _log != null && _log.IsErrorEnabled )
					_log.Error(ex.Message, ex);
			}
			finally
			{
				xmlSerializer = null;
			}
			return settings;
		}

		#endregion
	}
}
