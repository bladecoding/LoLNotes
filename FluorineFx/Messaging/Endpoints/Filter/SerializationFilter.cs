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
using System.IO;

using FluorineFx.Messaging.Endpoints;
using FluorineFx.Context;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SerializationFilter : AbstractFilter
	{
		bool _useLegacyCollection = false;
        bool _useLegacyThrowable = true;
		/// <summary>
		/// Initializes a new instance of the SerializationFilter class.
		/// </summary>
		public SerializationFilter()
		{
		}
        /// <summary>
        /// Gets or sets whether legacy collection serialization is used for AMF3.
        /// </summary>
		public bool UseLegacyCollection
		{
			get{ return _useLegacyCollection; }
			set{ _useLegacyCollection = value; }
		}
        /// <summary>
        /// Gets or sets whether legacy exception serialization is used for AMF3.
        /// </summary>
        public bool UseLegacyThrowable
        {
            get { return _useLegacyThrowable; }
            set { _useLegacyThrowable = value; }
        }

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{            
			AMFSerializer serializer = new AMFSerializer(context.OutputStream);
			serializer.UseLegacyCollection = _useLegacyCollection;
            serializer.UseLegacyThrowable = _useLegacyThrowable;
			serializer.WriteMessage(context.MessageOutput);
			serializer.Flush();
            

			//Serialization/deserialization debugging
            //Note: this will not work correctly with optimizers (different ClassDefinitions from server and client)
            /*
            MemoryStream ms = new MemoryStream();
            AMFSerializer testSerializer = new AMFSerializer(ms);
            testSerializer.UseLegacyCollection = _useLegacyCollection;
            testSerializer.UseLegacyThrowable = _useLegacyThrowable;
            testSerializer.WriteMessage(context.MessageOutput);
            testSerializer.Flush();
            ms.Position = 0;
            AMFDeserializer testDeserializer = new AMFDeserializer(ms);
            testDeserializer.UseLegacyCollection = _useLegacyCollection;
            AMFMessage amfMessageOut = testDeserializer.ReadAMFMessage();
            ms.Position = 0;
            byte[] buffer = ms.ToArray();
            context.OutputStream.Write(buffer, 0, buffer.Length);
            */
        }

		#endregion
	}
}
