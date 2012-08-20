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

namespace FluorineFx.Util
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class NumberUtils
	{
		public NumberUtils()
		{
		}

		/// <summary>
		/// Determines whether the supplied <paramref name="number"/> is an integer.
		/// </summary>
		/// <param name="number">The object to check.</param>
		/// <returns>
		/// <see lang="true"/> if the supplied <paramref name="number"/> is an integer.
		/// </returns>
		public static bool IsInteger(object number)
		{
			return (number is Int32 || number is Int16 || number is Int64 || number is UInt32
				|| number is UInt16 || number is UInt64 || number is Byte || number is SByte);
		}

		/// <summary>
		/// Determines whether the supplied <paramref name="number"/> is a decimal number.
		/// </summary>
		/// <param name="number">The object to check.</param>
		/// <returns>
		/// <see lang="true"/> if the supplied <paramref name="number"/> is a decimal number.
		/// </returns>
		public static bool IsDecimal(object number)
		{
			return (number is Single || number is Double || number is Decimal);
		}

		/// <summary>
		/// Determines whether the supplied <paramref name="number"/> is decimal number.
		/// </summary>
		/// <param name="number">The object to check.</param>
		/// <returns>
		/// 	<c>true</c> if the specified object is decimal number; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNumber(object number)
		{
			return (IsInteger(number) || IsDecimal(number));
		}

		/// <summary>
		/// Is the supplied <paramref name="number"/> equal to zero (0)?
		/// </summary>
		/// <param name="number">The number to check.</param>
		/// <returns>
		/// <see lang="true"/> id the supplied <paramref name="number"/> is equal to zero (0).
		/// </returns>
		public static bool IsZero(object number) 
		{
			if (number is Int32) return ((Int32) number) == 0;
			else if (number is Int16) return ((Int16) number) == 0;
			else if (number is Int64) return ((Int64) number) == 0;
			else if (number is UInt16) return ((Int32) number) == 0;
			else if (number is UInt32) return ((Int64) number) == 0;
			else if (number is UInt64) return (Convert.ToDecimal(number) == 0);
			else if (number is Byte) return ((Int16) number) == 0;
			else if (number is SByte) return ((Int16) number) == 0;
			else if (number is Single) return ((Single) number) == 0f;
			else if (number is Double) return ((Double) number) == 0d;
			else if (number is Decimal) return ((Decimal) number) == 0m;
			return false;
		}

		/// <summary>
		/// Negates the supplied <paramref name="number"/>.
		/// </summary>
		/// <param name="number">The number to negate.</param>
		/// <returns>The supplied <paramref name="number"/> negated.</returns>
		/// <exception cref="System.ArgumentException">
		/// If the supplied <paramref name="number"/> is not a supported numeric type.
		/// </exception>
		public static object Negate(object number)
		{
			if (number is Int32) return -((Int32) number);
			else if (number is Int16) return -((Int16) number);
			else if (number is Int64) return -((Int64) number);
			else if (number is UInt16) return -((Int32) number);
			else if (number is UInt32) return -((Int64) number);
			else if (number is UInt64) return -(Convert.ToDecimal(number));
			else if (number is Byte) return -((Int16) number);
			else if (number is SByte) return -((Int16) number);
			else if (number is Single) return -((Single) number);
			else if (number is Double) return -((Double) number);
			else if (number is Decimal) return -((Decimal) number);
			else
			{
				throw new ArgumentException(string.Format("'{0}' is not one of the supported numeric types.", number));
			}
		}

		/// <summary>
		/// Adds the specified numbers.
		/// </summary>
		/// <param name="m">The first number.</param>
		/// <param name="n">The second number.</param>
		public static object Add(object m, object n)
		{
			CoerceTypes(ref m, ref n);

			if (n is Int32) return (Int32) m + (Int32) n;
			else if (n is Int16) return (Int16) m + (Int16) n;
			else if (n is Int64) return (Int64) m + (Int64) n;
			else if (n is UInt16) return (UInt16) m + (UInt16) n;
			else if (n is UInt32) return (UInt32) m + (UInt32) n;
			else if (n is UInt64) return (UInt64) m + (UInt64) n;
			else if (n is Byte) return (Byte) m + (Byte) n;
			else if (n is SByte) return (SByte) m + (SByte) n;
			else if (n is Single) return (Single) m + (Single) n;
			else if (n is Double) return (Double) m + (Double) n;
			else if (n is Decimal) return (Decimal) m + (Decimal) n;

			return null;
		}

		/// <summary>
		/// Subtracts the specified numbers.
		/// </summary>
		/// <param name="m">The first number.</param>
		/// <param name="n">The second number.</param>
		public static object Subtract(object m, object n)
		{
			CoerceTypes(ref m, ref n);

			if (n is Int32) return (Int32) m - (Int32) n;
			else if (n is Int16) return (Int16) m - (Int16) n;
			else if (n is Int64) return (Int64) m - (Int64) n;
			else if (n is UInt16) return (UInt16) m - (UInt16) n;
			else if (n is UInt32) return (UInt32) m - (UInt32) n;
			else if (n is UInt64) return (UInt64) m - (UInt64) n;
			else if (n is Byte) return (Byte) m - (Byte) n;
			else if (n is SByte) return (SByte) m - (SByte) n;
			else if (n is Single) return (Single) m - (Single) n;
			else if (n is Double) return (Double) m - (Double) n;
			else if (n is Decimal) return (Decimal) m - (Decimal) n;

			return null;
		}

		/// <summary>
		/// Multiplies the specified numbers.
		/// </summary>
		/// <param name="m">The first number.</param>
		/// <param name="n">The second number.</param>
		public static object Multiply(object m, object n)
		{
			CoerceTypes(ref m, ref n);

			if (n is Int32) return (Int32) m*(Int32) n;
			else if (n is Int16) return (Int16) m*(Int16) n;
			else if (n is Int64) return (Int64) m*(Int64) n;
			else if (n is UInt16) return (UInt16) m*(UInt16) n;
			else if (n is UInt32) return (UInt32) m*(UInt32) n;
			else if (n is UInt64) return (UInt64) m*(UInt64) n;
			else if (n is Byte) return (Byte) m*(Byte) n;
			else if (n is SByte) return (SByte) m*(SByte) n;
			else if (n is Single) return (Single) m*(Single) n;
			else if (n is Double) return (Double) m*(Double) n;
			else if (n is Decimal) return (Decimal) m*(Decimal) n;

			return null;
		}

		/// <summary>
		/// Divides the specified numbers.
		/// </summary>
		/// <param name="m">The first number.</param>
		/// <param name="n">The second number.</param>
		public static object Divide(object m, object n)
		{
			CoerceTypes(ref m, ref n);

			if (n is Int32) return (Int32) m/(Int32) n;
			else if (n is Int16) return (Int16) m/(Int16) n;
			else if (n is Int64) return (Int64) m/(Int64) n;
			else if (n is UInt16) return (UInt16) m/(UInt16) n;
			else if (n is UInt32) return (UInt32) m/(UInt32) n;
			else if (n is UInt64) return (UInt64) m/(UInt64) n;
			else if (n is Byte) return (Byte) m/(Byte) n;
			else if (n is SByte) return (SByte) m/(SByte) n;
			else if (n is Single) return (Single) m/(Single) n;
			else if (n is Double) return (Double) m/(Double) n;
			else if (n is Decimal) return (Decimal) m/(Decimal) n;

			return null;
		}

		/// <summary>
		/// Calculates remainder for the specified numbers.
		/// </summary>
		/// <param name="m">The first number (dividend).</param>
		/// <param name="n">The second number (divisor).</param>
		public static object Modulus(object m, object n)
		{
			CoerceTypes(ref m, ref n);

			if (n is Int32) return (Int32) m%(Int32) n;
			else if (n is Int16) return (Int16) m%(Int16) n;
			else if (n is Int64) return (Int64) m%(Int64) n;
			else if (n is UInt16) return (UInt16) m%(UInt16) n;
			else if (n is UInt32) return (UInt32) m%(UInt32) n;
			else if (n is UInt64) return (UInt64) m%(UInt64) n;
			else if (n is Byte) return (Byte) m%(Byte) n;
			else if (n is SByte) return (SByte) m%(SByte) n;
			else if (n is Single) return (Single) m%(Single) n;
			else if (n is Double) return (Double) m%(Double) n;
			else if (n is Decimal) return (Decimal) m%(Decimal) n;

			return null;
		}

		/// <summary>
		/// Raises first number to the power of the second one.
		/// </summary>
		/// <param name="m">The first number.</param>
		/// <param name="n">The second number.</param>
		public static object Power(object m, object n)
		{
			return Math.Pow(Convert.ToDouble(m), Convert.ToDouble(n));
		}


		/// <summary>
		/// Coerces the types so they can be compared.
		/// </summary>
		/// <param name="m">The right.</param>
		/// <param name="n">The left.</param>
		public static void CoerceTypes(ref object m, ref object n)
		{
            TypeCode leftTypeCode = System.Convert.GetTypeCode(m);
            TypeCode rightTypeCode = System.Convert.GetTypeCode(n);

			if (leftTypeCode > rightTypeCode)
			{
#if SILVERLIGHT
#else
                n = System.Convert.ChangeType(n, leftTypeCode, null);
#endif
            }
			else
			{
#if SILVERLIGHT
                m = System.Convert.ChangeType(m, rightTypeCode, null);
#else
                m = System.Convert.ChangeType(m, rightTypeCode);
#endif
            }
		}

		public static int HexToInt(char h)
		{
			if ((h >= '0') && (h <= '9'))
			{
				return (h - '0');
			}
			if ((h >= 'a') && (h <= 'f'))
			{
				return ((h - 'a') + 10);
			}
			if ((h >= 'A') && (h <= 'F'))
			{
				return ((h - 'A') + 10);
			}
			return -1;
		}

		public static char IntToHex(int n)
		{
			if (n <= 9)
			{
				return (char)(n + 48);
			}
			return (char)((n - 10) + 97);
		}
	}
}
