/*
copyright (C) 2011-2012 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


namespace LoLNotes.Util
{
    //Using these methods for the exceptions
    public static class Parse
    {
        public static int Int(string str)
        {
            if (str == "NaN")
                return 0;

            int ret;
            if (int.TryParse(str, out ret))
                return ret;

            throw new FormatException(string.Format("Expected {0} got {1}", ret.GetType(), str));
        }
        public static long Long(string str)
        {
            if (str == "NaN")
                return 0;

            long ret;
            if (long.TryParse(str, out ret))
                return ret;

            throw new FormatException(string.Format("Expected {0} got {1}", ret.GetType(), str));
        }
        public static bool Bool(string str)
        {
            bool ret;
            if (bool.TryParse(str, out ret))
                return ret;

            throw new FormatException(string.Format("Expected {0} got {1}", ret.GetType(), str));
        }
        /// <summary>
        /// Parses dates in the following format "Sat Oct 22 22:52:44 GMT-0400 2011"
        /// </summary>
        /// <param name="str">Date formated as "Sat Oct 22 22:52:44 GMT-0400 2011"</param>
        /// <returns>DateTime Utc</returns>
        /// <exception cref="FormatException">Throws when it is unable to match and parse date.</exception>
        public static DateTime Date(string str)
        {
            var match = Regex.Match(str, @"\w+ (?<month>\w+) (?<day>\d+) (?<hour>\d+):(?<minute>\d+):(?<second>\d+) GMT(?<gmt>[+-]\d\d)\d* (?<year>\d+)");
            if (!match.Success)
                throw new FormatException("Invalid date format " + str);

            string date = string.Format("{0} {1} {2} {3} {4} {5} {6}", match.Groups["month"], match.Groups["day"], match.Groups["hour"], match.Groups["minute"], match.Groups["second"], match.Groups["gmt"], match.Groups["year"]);

            DateTime ret;
            if (!DateTime.TryParseExact(date, "MMM dd HH mm ss zz yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out ret))
                throw new FormatException("Invalid date format " + str);

            return ret;
        }

		public static string ToBase64(string str)
		{
			if (str == null)
				return null;
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
		}
    }
}
