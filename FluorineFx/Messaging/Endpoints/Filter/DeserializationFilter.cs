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
using FluorineFx.IO;
using FluorineFx.Context;
using FluorineFx.Configuration;
using FluorineFx.Messaging.Endpoints;

namespace FluorineFx.Messaging.Endpoints.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DeserializationFilter : AbstractFilter
	{
		bool _useLegacyCollection = false;
		/// <summary>
		/// Initializes a new instance of the DeserializationFilter class.
		/// </summary>
		public DeserializationFilter()
		{
		}

		public bool UseLegacyCollection
		{
			get{ return _useLegacyCollection; }
			set{ _useLegacyCollection = value; }
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			AMFDeserializer deserializer = new AMFDeserializer(context.InputStream);
			deserializer.UseLegacyCollection = _useLegacyCollection;
            try
            {
                AMFMessage amfMessage = deserializer.ReadAMFMessage();
                context.AMFMessage = amfMessage;
                context.MessageOutput = new MessageOutput(amfMessage.Version);
                if (deserializer.FailedAMFBodies.Length > 0)
                {
                    AMFBody[] failedAMFBodies = deserializer.FailedAMFBodies;
                    //Write out failed AMFBodies
                    for (int i = 0; i < failedAMFBodies.Length; i++)
                        context.MessageOutput.AddBody(failedAMFBodies[i]);
                }
            }
            catch
            {
                Dump(context.InputStream);
                throw;
            }
		}

		#endregion

        private static void Dump(Stream stream)
        {
            if (FluorineConfiguration.Instance.FluorineSettings.Debug != null &&
                FluorineConfiguration.Instance.FluorineSettings.Debug.Mode != Debug.Off)
            {
                try
                {
                    if (FluorineConfiguration.Instance.FluorineSettings.Debug.DumpPath != null)
                    {
                        string fileName = string.Format("dump_{0}.bin", DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff"));
                        IResource resource = FluorineContext.Current.GetResource(Path.Combine(FluorineConfiguration.Instance.FluorineSettings.Debug.DumpPath, fileName));
                        if (stream.CanSeek)
                            stream.Seek(0, SeekOrigin.Begin);
                        using (Stream output = new FileStream (resource.File.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            byte[] buffer = new byte[1024];
                            int read;
                            while ( (read=stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, read);
                            }
                            output.Close();
                        }
                    }
                }
                catch
                {
                }
            }
        }
	}
}
