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

namespace FluorineFx.Messaging.Rtmp.SO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class SharedObjectEvent : ISharedObjectEvent
	{
		private SharedObjectEventType _type;
		private string _key;
		private object _value;

        /// <summary>
        /// Initializes a new instance of the SharedObjectEvent class with the given initial value.
        /// </summary>
        /// <param name="type">Type of the event.</param>
        /// <param name="key">Key of the event.</param>
        /// <param name="value">Value of the event.</param>
		public SharedObjectEvent(SharedObjectEventType type, String key, Object value)
		{
			_type = type;
			_key = key;
			_value = value;
		}

		#region ISharedObjectEvent Members

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public SharedObjectEventType Type
		{
			get
			{
				return _type;
			}
		}
        /// <summary>
        /// Gets the key of the event.
        /// </summary>
        public string Key
		{
			get
			{
				return _key;
			}
		}
        /// <summary>
        /// Gets the value of the event.
        /// </summary>
        public object Value
		{
			get
			{
				return _value;
			}
		}

		#endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
		public override string ToString() 
		{
			return "SOEvent(" + _type + ", " + _key + ", " + _value + ")";
		}
	}
}
