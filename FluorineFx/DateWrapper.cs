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
using System.Security;
using System.Security.Permissions;
using FluorineFx.Context;

namespace FluorineFx
{
	/// <summary>
	/// Helper class used for date, timezone management.
	/// </summary>
	/// <remarks>DateWrapper uses time zone in the last Date encountered during deserialization.</remarks>
	public sealed class DateWrapper
	{
        /// <summary>
        /// This member supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string FluorineTimezoneKey = "__@fluorinetimezone";

		/// <summary>
		/// Initializes a new instance of the DateWrapper class.
		/// </summary>
		internal DateWrapper()
		{
		}

		internal static int GetTimeZone()
		{
            object value = FluorineWebSafeCallContext.GetData(FluorineTimezoneKey);
            if( value != null )
                System.Convert.ToInt32(value);
			return 0;
		}

		internal static void SetTimeZone(int timezone)
		{
            FluorineWebSafeCallContext.SetData(FluorineTimezoneKey, timezone);
		}
		/// <summary>
		/// Gets the client time zone.
		/// </summary>
		public static TimeSpan ClientTimeZone
		{
			get{ return new TimeSpan(GetTimeZone(), 0, 0); }
		}
		/// <summary>
		/// Gets the server time zone.
		/// </summary>
		public static TimeSpan ServerTimeZone
		{
			get{ return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Today); }
		}
		/// <summary>
		/// Get the date according to client timezone.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime GetClientDate(DateTime date)
		{
			return date.Add( ClientTimeZone );
		}
		/// <summary>
		/// Get the date according to server timezone.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime GetServerDate(DateTime date)
		{
			return date.Add( ServerTimeZone );
		}
	}
}
