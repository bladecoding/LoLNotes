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
using System.IO;
#if !SILVERLIGHT
using log4net;
#endif
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;

namespace FluorineFx.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFSerializer : AMFWriter
	{
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(AMFSerializer));
#endif

		/// <summary>
		/// Initializes a new instance of the AMFSerializer class.
		/// </summary>
		/// <param name="stream"></param>
		public AMFSerializer(Stream stream) : base(stream)
		{
		}
        /// <summary>
        /// Initializes a new instance of the AMFSerializer class.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="stream"></param>
        internal AMFSerializer(AMFWriter writer, Stream stream)
            : base(writer, stream)
        {
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="amfMessage"></param>
        [CLSCompliant(false)]
        public void WriteMessage(AMFMessage amfMessage)
		{
			try
			{
				base.WriteShort(amfMessage.Version);
				int headerCount = amfMessage.HeaderCount;
				base.WriteShort(headerCount);
				for(int i = 0; i < headerCount; i++)
				{
					this.WriteHeader(amfMessage.GetHeaderAt(i), ObjectEncoding.AMF0);
				}
				int bodyCount = amfMessage.BodyCount;
				base.WriteShort(bodyCount);
				for(int i = 0; i < bodyCount; i++)
				{
					ResponseBody responseBody = amfMessage.GetBodyAt(i) as ResponseBody;
                    if (responseBody != null && !responseBody.IgnoreResults)
                    {
                        //Try to catch serialization errors
                        if (this.BaseStream.CanSeek)
                        {
                            long position = this.BaseStream.Position;

                            try
                            {
                                responseBody.WriteBody(amfMessage.ObjectEncoding, this);
                            }
                            catch (Exception exception)
                            {
                                this.BaseStream.Seek(position, SeekOrigin.Begin);
                                //this.BaseStream.Position = position;

#if !SILVERLIGHT
                                if (log.IsFatalEnabled)
                                    log.Fatal(__Res.GetString(__Res.Amf_SerializationFail), exception);
#endif

                                ErrorResponseBody errorResponseBody;
                                if (responseBody.RequestBody.IsEmptyTarget)
                                {
                                    object content = responseBody.RequestBody.Content;
                                    if (content is IList)
                                        content = (content as IList)[0];
                                    IMessage message = content as IMessage;
                                    MessageException messageException = new MessageException(exception);
                                    messageException.FaultCode = __Res.GetString(__Res.Amf_SerializationFail);
                                    errorResponseBody = new ErrorResponseBody(responseBody.RequestBody, message, messageException);
                                }
                                else
                                    errorResponseBody = new ErrorResponseBody(responseBody.RequestBody, exception);

                                try
                                {
                                    errorResponseBody.WriteBody(amfMessage.ObjectEncoding, this);
                                }
#if !SILVERLIGHT
                                catch (Exception exception2)
                                {
                                    if (log.IsFatalEnabled)
                                        log.Fatal(__Res.GetString(__Res.Amf_ResponseFail), exception2);
                                    throw;
                                }
#else
                                catch (Exception)
                                {
                                    throw;
                                }
#endif
                            }
                        }
                        else
                            responseBody.WriteBody(amfMessage.ObjectEncoding, this);
                    }
                    else
                    {
                        AMFBody amfBody = amfMessage.GetBodyAt(i);
                        FluorineFx.Util.ValidationUtils.ObjectNotNull(amfBody, "amfBody");
                        amfBody.WriteBody(amfMessage.ObjectEncoding, this);
                    }
				}
			}
#if !SILVERLIGHT
			catch(Exception exception)
			{
                if( log.IsFatalEnabled )
                    log.Fatal(__Res.GetString(__Res.Amf_SerializationFail), exception);
				throw;
			}
#else
            catch (Exception)
            {
                throw;
            }
#endif
        }

		private void WriteHeader(AMFHeader header, ObjectEncoding objectEncoding)
		{
			base.Reset();
			base.WriteUTF(header.Name);
			base.WriteBoolean(header.MustUnderstand);
			base.WriteInt32(-1);
			base.WriteData(objectEncoding, header.Content);
		}
 	}
}
