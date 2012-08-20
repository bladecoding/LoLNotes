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

using FluorineFx.Messaging.Messages;

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class CachedBody : ResponseBody
	{
		/// <summary>
		/// Initializes a new instance of the CachedBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="content"></param>
		public CachedBody(AMFBody requestBody, object content):base(requestBody, content)
		{
		}

		public CachedBody(AMFBody requestBody, IMessage message, object content):base(requestBody, content)
		{
			//Fix content of body for flex messages
			AcknowledgeMessage responseMessage = new AcknowledgeMessage();
			responseMessage.body = content;
			responseMessage.correlationId = message.messageId;
			responseMessage.destination = message.destination;
			responseMessage.clientId = message.clientId;

			this.Content = responseMessage;
		}

		protected override void WriteBodyData(ObjectEncoding objectEncoding, AMFWriter writer)
		{
			writer.WriteData(objectEncoding, this.Content);
		}

	}
}
