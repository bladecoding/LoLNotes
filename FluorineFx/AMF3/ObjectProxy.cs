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
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx;

namespace FluorineFx.AMF3
{
	/// <summary>
	/// Flex ObjectProxy class.
	/// </summary>
    [CLSCompliant(false)]
#if !(NET_1_1)
    public class ObjectProxy : Dictionary<string, Object>, IExternalizable
#else
    public class ObjectProxy : Hashtable, IExternalizable
#endif
	{
        /// <summary>
        /// Initializes a new instance of the ObjectProxy class.
        /// </summary>
		public ObjectProxy()
		{
		}

		#region IExternalizable Members

        /// <summary>
        /// Decode the ObjectProxy from a data stream.
        /// </summary>
        /// <param name="input">IDataInput interface.</param>
        public void ReadExternal(IDataInput input)
		{
			object value = input.ReadObject();
			if( value is IDictionary )
			{
                IDictionary dictionary = value as IDictionary;
                foreach (DictionaryEntry entry in dictionary)
				{
					this.Add(entry.Key as string, entry.Value);
				}
			}
		}
        /// <summary>
        /// Encode the ObjectProxy for a data stream.
        /// </summary>
        /// <param name="output">IDataOutput interface.</param>
        public void WriteExternal(IDataOutput output)
		{
			ASObject asObject = new ASObject();
#if !(NET_1_1)
			foreach(KeyValuePair<string, object> entry in this)
#else
			foreach(DictionaryEntry entry in this)
#endif
            {
				asObject.Add(entry.Key, entry.Value);
			}
			output.WriteObject(asObject);
		}

		#endregion
	}
}
