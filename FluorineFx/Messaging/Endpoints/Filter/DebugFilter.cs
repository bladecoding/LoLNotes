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

using FluorineFx.Messaging.Endpoints;
using FluorineFx.Diagnostic;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DebugFilter : AbstractFilter
	{
		/// <summary>
		/// Initializes a new instance of the DebugFilter class.
		/// </summary>
		public DebugFilter()
		{
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			AMFMessage amfMessage = context.AMFMessage;
			AMFHeader amfHeader = amfMessage.GetHeader( AMFHeader.DebugHeader );
			if( amfHeader != null )
			{
				//The value of the header
				ASObject asObject = amfHeader.Content as ASObject;
				//["error"]: {true}
				//["trace"]: {true}
				//["httpheaders"]: {false}
				//["coldfusion"]: {true}
				//["amf"]: {false}
				//["m_debug"]: {true}
				//["amfheaders"]: {false}
				//["recordset"]: {true}

				AMFBody amfBody = amfMessage.GetBodyAt(amfMessage.BodyCount-1);//last body
				ResponseBody amfBodyOut = new ResponseBody();
				amfBodyOut.Target = amfBody.Response + AMFBody.OnDebugEvents;
				amfBodyOut.Response = null;
				amfBodyOut.IsDebug = true;

				ArrayList headerResults = new ArrayList();
				ArrayList result = new ArrayList();
				headerResults.Add( result );
				

				if( (bool)asObject["httpheaders"] == true ) 
				{ 
					//If the client wants http headers
					result.Add( new HttpHeader( ) );
				} 
				if( (bool)asObject["amfheaders"] == true ) 
				{
					result.Add( new AMFRequestHeaders( amfMessage ) );
				}

				ArrayList traceStack = NetDebug.GetTraceStack();
				if( (bool)asObject["trace"] == true  && traceStack != null && traceStack.Count > 0 )
				{
					ArrayList tmp = new ArrayList( traceStack );
					result.Add( new TraceHeader( tmp ) );
					NetDebug.Clear();
				}

				//http://osflash.org/amf/envelopes/remoting/debuginfo

				amfBodyOut.Content = headerResults;
				context.MessageOutput.AddBody(amfBodyOut);
			}
			NetDebug.Clear();
		}

		#endregion

	}
}
