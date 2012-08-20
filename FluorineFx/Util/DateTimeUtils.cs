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
using System.Globalization;

#if NET_1_1
using FluorineFx.Util.Nullables;
#else
using NullableDateTime = System.Nullable<System.DateTime>;
#endif

namespace FluorineFx.Util
{
    /// <summary>
    /// DateTime related utility methods.
    /// </summary>
    public abstract class DateTimeUtils
    {
        private static readonly string[] _internetDateFormats =
        {
                "dd MMM yyyy HH':'mm",
                "dd MMM yyyy HH':'mm':'ss",
                "ddd, dd MMM yyyy HH':'mm",
                "ddd, dd MMM yyyy HH':'mm':'ss",
        };

        /// <summary>
        /// Assumes that given input is in UTC and sets the kind to be UTC.
        /// Just a precaution if somebody does not set it explicitly.
        /// <strong>This only works in .NET Framework 2.0 onwards.</strong>
        /// </summary>
        /// <param name="dt">The datetime to check.</param>
        /// <returns>DateTime with kind set to UTC.</returns>
        public static DateTime AssumeUniversalTime(DateTime dt)
        {
#if NET_1_1
            // can't really do anything in 1.x
            return dt;
#else
            return new DateTime(dt.Ticks, DateTimeKind.Utc);
#endif
        }

        /// <summary>
        /// Assumes that given input is in UTC and sets the kind to be UTC.
        /// Just a precaution if somebody does not set it explicitly.
        /// </summary>
        /// <param name="dt">The datetime to check.</param>
        /// <returns>DateTime with kind set to UTC.</returns>
        public static NullableDateTime AssumeUniversalTime(NullableDateTime dt)
        {
            if (dt.HasValue)
            {
                return AssumeUniversalTime(dt.Value);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Returns a date from a string using the internet format.
        /// </summary>
        /// <param name="input">Date string using the internet format.</param>
        /// <returns>DateTime parsed from string.</returns>
        public static DateTime ParseInternetDate(string input)
        {
            ValidationUtils.ArgumentNotNull(input, "input");
            if (input.Length < _internetDateFormats[0].Length)
                throw new ArgumentException("input");

            //
            // Parse according to the following syntax:
            //
            //   date-time   =  [ day "," ] date time ; dd mm yy
            //                                        ;  hh:mm:ss zzz
            //  
            //   day         =  "Mon"  / "Tue" /  "Wed"  / "Thu"
            //               /  "Fri"  / "Sat" /  "Sun"
            //  
            //   date        =  1*2DIGIT month 2DIGIT ; day month year
            //                                        ;  e.g. 20 Jun 82
            //  
            //   month       =  "Jan"  /  "Feb" /  "Mar"  /  "Apr"
            //               /  "May"  /  "Jun" /  "Jul"  /  "Aug"
            //               /  "Sep"  /  "Oct" /  "Nov"  /  "Dec"
            //  
            //   time        =  hour zone             ; ANSI and Military
            //  
            //   hour        =  2DIGIT ":" 2DIGIT [":" 2DIGIT]
            //                                        ; 00:00:00 - 23:59:59
            //  
            //   zone        =  "UT"  / "GMT"         ; Universal Time
            //                                        ; North American : UT
            //               /  "EST" / "EDT"         ;  Eastern:  - 5/ - 4
            //               /  "CST" / "CDT"         ;  Central:  - 6/ - 5
            //               /  "MST" / "MDT"         ;  Mountain: - 7/ - 6
            //               /  "PST" / "PDT"         ;  Pacific:  - 8/ - 7
            //               /  1ALPHA                ; Military: Z = UT;
            //                                        ;  A:-1; (J not used)
            //                                        ;  M:-12; N:+1; Y:+12
            //               / ( ("+" / "-") 4DIGIT ) ; Local differential
            //                                        ;  hours+min. (HHMM)
            //
            // For more information, see:
            // http://www.w3.org/Protocols/rfc822/#z28
            //

            //
            // Start by processing the time zone component, which is the 
            // part that cannot be delegated to DateTime.ParseExact.
            //

            int zzz; // time zone offset stored as HH * 100 + MM

            int zoneSpaceIndex = input.LastIndexOf(' ');

            if (zoneSpaceIndex <= 0)
                throw new FormatException();

            string zone = input.Substring(zoneSpaceIndex + 1);

            if (zone.Length == 0)
                throw new FormatException("Missing time zone.");

            switch (zone)
            {
                //
                // Greenwich Mean Time (GMT) or Universal Time (UT)
                //

                case "UT":
                case "GMT": zzz = +0000; break;

                //
                // Common North American time zones
                //

                case "EDT": zzz = -0400; break;
                case "EST":
                case "CDT": zzz = -0500; break;
                case "CST":
                case "MDT": zzz = -0600; break;
                case "MST":
                case "PDT": zzz = -0700; break;
                case "PST": zzz = -0800; break;

                //
                // Local differential = ( "+" / "-" ) HHMM
                //

                default:
                    {
                        if (zone.Length < 4)
                            throw new FormatException("Length of local differential component must be at least 4 characters (HHMM).");

                        try
                        {
                            zzz = int.Parse(zone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException e)
                        {
                            throw new FormatException("Invalid local differential.", e);
                        }

                        break;
                    }
            }

            //
            // Strip the time zone component along with any trailing space 
            // and parse out just the time piece by simply delegating to 
            // DateTime.ParseExact.
            //

            input = input.Substring(0, zoneSpaceIndex).TrimEnd();
            DateTime time = DateTime.ParseExact(input, _internetDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite);

            //
            // Subtract the offset to produce zulu time and then return the 
            // result as local time.
            //

            TimeSpan offset = new TimeSpan(zzz / 100, zzz % 100, 0);
            return time.Subtract(offset).ToLocalTime();
        }
    }
}
