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
#if (NET_1_1)
#else
using System.Collections.Generic;
#endif

namespace FluorineFx.Util
{
    /// <summary>
    /// Utility class offering convenient methods for validating and for rejecting empty fields. 
    /// </summary>
    public abstract class ValidationUtils
    {
        private ValidationUtils() { }

        /// <summary>
        /// Reject the given parameter if the value is null or empty.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            if (value.Length == 0)
                throw new ArgumentException(string.Format("'{0}' cannot be empty.", parameterName), parameterName);
        }
        /// <summary>
        /// Reject the given parameter if the value is null, empty or contains only whitespace.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNullOrEmptyOrWhitespace(string value, string parameterName)
        {
            ArgumentNotNullOrEmpty(value, parameterName);

            if (StringUtils.IsWhiteSpace(value))
                throw new ArgumentException(string.Format("'{0}' cannot only be whitespace.", parameterName), parameterName);
        }
        /// <summary>
        /// Reject the given parameter if it is not an Enum type.
        /// </summary>
        /// <param name="enumType">Parameter type.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentTypeIsEnum(Type enumType, string parameterName)
        {
            ArgumentNotNull(enumType, "enumType");

            if (!enumType.IsEnum)
                throw new ArgumentException(string.Format("Type {0} is not an Enum.", enumType), parameterName);
        }
        /// <summary>
        /// Reject the given collection parameter if the value is null or empty.
        /// </summary>
        /// <param name="collection">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName)
        {
            ArgumentNotNullOrEmpty(collection, parameterName, string.Format("Collection '{0}' cannot be empty.", parameterName));
        }
        /// <summary>
        /// Reject the given collection parameter if the value is null or empty.
        /// </summary>
        /// <param name="collection">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName, string message)
        {
            if (collection == null)
                throw new ArgumentNullException(parameterName);

            if (collection.Count == 0)
                throw new ArgumentException(message, parameterName);
        }

#if !(NET_1_1)
        /// <summary>
        /// Reject the given collection parameter if the value is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName)
        {
            ArgumentNotNullOrEmpty<T>(collection, parameterName, string.Format("Collection '{0}' cannot be empty.", parameterName));
        }
        /// <summary>
        /// Reject the given collection parameter if the value is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName, string message)
        {
            if (collection == null)
                throw new ArgumentNullException(parameterName);

            if (collection.Count == 0)
                throw new ArgumentException(message, parameterName);
        }
        /// <summary>
        /// Reject the given parameter if the value is not a positive number.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentIsPositive<T>(T value, string parameterName) where T : struct, IComparable<T>
        {
            if (value.CompareTo(default(T)) != 1)
                throw new ArgumentOutOfRangeException(parameterName, "Positive number required.");
        }

#endif
        /// <summary>
        /// Reject the given parameter if the value is null.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNull(object value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }
        /// <summary>
        /// Reject the given parameter if the value is not a positive number.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotNegative(int value, string parameterName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Argument cannot be negative.");
        }
        /// <summary>
        /// Reject the given parameter if the value is not a positive number.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentNotNegative(int value, string parameterName, string message)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }
        /// <summary>
        /// Reject the given parameter if the value is 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        public static void ArgumentNotZero(int value, string parameterName)
        {
            if (value == 0)
                throw new ArgumentOutOfRangeException(parameterName, "Argument cannot be zero.");
        }
        /// <summary>
        /// Reject the given parameter if the value is 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentNotZero(int value, string parameterName, string message)
        {
            if (value == 0)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }
        /// <summary>
        /// Reject the given parameter if the value is a positive number.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentIsPositive(int value, string parameterName, string message)
        {
            if (value > 0)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }
        /// <summary>
        /// Raise an ObjectDisposedException.
        /// </summary>
        /// <param name="disposed">If true then raise an ObjectDisposedException.</param>
        /// <param name="objectType">Object type.</param>
        public static void ObjectNotDisposed(bool disposed, Type objectType)
        {
            if (disposed)
                throw new ObjectDisposedException(objectType.Name);
        }
        /// <summary>
        /// Reject the given parameter if the condition is not set.
        /// </summary>
        /// <param name="condition">If set to false throws ArgumentException.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason of the exception.</param>
        public static void ArgumentConditionTrue(bool condition, string parameterName, string message)
        {
            if (!condition)
                throw new ArgumentException(message, parameterName);
        }
        /// <summary>
        /// Reject the given variable if the value is null.
        /// </summary>
        /// <param name="value">The variable value.</param>
        /// <param name="variableName">The variable name.</param>
		public static void ObjectNotNull(object value, string variableName)
		{
			if (value == null)
				throw new NullReferenceException(string.Format("{0} cannot be null.", variableName));
		}
    }
}