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
using System.Xml;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Security;
using System.Text;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Data;
using System.Data.SqlTypes;
using System.Web;
using log4net;
#endif
#if !NET_2_0
using System.Xml.Linq;
#endif
using FluorineFx.Configuration;
using FluorineFx.Util;

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class TypeHelper
	{
#if SILVERLIGHT
        static object _syncLock = new object();
#endif
#if !SILVERLIGHT
        private static readonly ILog log = LogManager.GetLogger(typeof(TypeHelper));
#endif

		static TypeHelper()
		{
            DefaultSByteNullValue = (SByte)GetNullValue(typeof(SByte));
            DefaultInt16NullValue = (Int16)GetNullValue(typeof(Int16));
            DefaultInt32NullValue = (Int32)GetNullValue(typeof(Int32));
            DefaultInt64NullValue = (Int64)GetNullValue(typeof(Int64));
            DefaultByteNullValue = (Byte)GetNullValue(typeof(Byte));
            DefaultUInt16NullValue = (UInt16)GetNullValue(typeof(UInt16));
            DefaultUInt32NullValue = (UInt32)GetNullValue(typeof(UInt32));
            DefaultUInt64NullValue = (UInt64)GetNullValue(typeof(UInt64));
            DefaultCharNullValue = (Char)GetNullValue(typeof(Char));
            DefaultSingleNullValue = (Single)GetNullValue(typeof(Single));
            DefaultDoubleNullValue = (Double)GetNullValue(typeof(Double));
            DefaultBooleanNullValue = (Boolean)GetNullValue(typeof(Boolean));

            DefaultStringNullValue = (String)GetNullValue(typeof(String));
            DefaultDateTimeNullValue = (DateTime)GetNullValue(typeof(DateTime));
            DefaultDecimalNullValue = (Decimal)GetNullValue(typeof(Decimal));
            DefaultGuidNullValue = (Guid)GetNullValue(typeof(Guid));
            DefaultXmlReaderNullValue = (XmlReader)GetNullValue(typeof(XmlReader));
#if !SILVERLIGHT
            DefaultXmlDocumentNullValue = (XmlDocument)GetNullValue(typeof(XmlDocument));
#endif
#if !NET_2_0
            _defaultXDocumentNullValue = (XDocument)GetNullValue(typeof(XDocument));
            _defaultXElementNullValue = (XElement)GetNullValue(typeof(XElement));
#endif
            _Init();
        }

#if SILVERLIGHT
        static Assembly[] Assemblies;
#endif

        internal static void _Init()
        {
#if SILVERLIGHT
            if (Assemblies == null)
            {
                lock (_syncLock)
                {
                    if (Assemblies == null)
                    {
                        List<Assembly> assemblies = new List<Assembly>();
                        foreach (System.Windows.AssemblyPart ap in System.Windows.Deployment.Current.Parts)
                        {
                            System.Windows.Resources.StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri(ap.Source, UriKind.Relative));
                            Assembly assembly = new System.Windows.AssemblyPart().Load(sri.Stream);
                            assemblies.Add(assembly);
                        }
                        Assemblies = assemblies.ToArray();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns></returns>
        static public Assembly[] GetAssemblies()
        {
#if SILVERLIGHT
            lock (_syncLock)
            {
                return Assemblies;
            }
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        static public Type Locate(string typeName)
		{
			if( string.IsNullOrEmpty(typeName) )
				return null;
            Assembly[] assemblies = GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies[i];
				Type type = assembly.GetType(typeName, false);
				if (type != null)
					return type;
			}
			return null;
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="lac"></param>
        /// <returns></returns>
		static public Type LocateInLac(string typeName, string lac)
		{
			if( lac == null  )
				return null;
			if( string.IsNullOrEmpty(typeName) )
				return null;
			foreach (string file in Directory.GetFiles(lac, "*.dll"))
			{
				try
				{
#if !SILVERLIGHT
                    log.Debug(__Res.GetString(__Res.TypeHelper_Probing, file, typeName));
#endif
					Assembly assembly = Assembly.LoadFrom(file);
					Type type = assembly.GetType(typeName, false);
					if (type != null)
						return type;
				}
#if !SILVERLIGHT
				catch(Exception ex)
				{
                    if(log.IsWarnEnabled )
					{
						log.Warn(__Res.GetString(__Res.TypeHelper_LoadDllFail, file));
						log.Warn(ex.Message);
					}
				}
#else
                catch (Exception)
                {
                }
#endif
			}
            foreach (string dir in Directory.GetDirectories(lac))
            {
                Type type = LocateInLac(typeName, dir);
                if (type != null)
                    return type;
            }
            return null;
		}

#if !SILVERLIGHT
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="lac"></param>
        /// <param name="excludedBaseTypes"></param>
        /// <returns></returns>
		static public Type[] SearchAllTypes(string lac, Hashtable excludedBaseTypes)
		{
			ArrayList result = new ArrayList();
			foreach (string file in Directory.GetFiles(lac, "*.dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom(file);
					if (assembly == Assembly.GetExecutingAssembly())
						continue;
					foreach (Type type in assembly.GetTypes())
					{
						if (excludedBaseTypes != null)
						{
							if (excludedBaseTypes.ContainsKey(type))
								continue;
							if (type.BaseType != null && excludedBaseTypes.ContainsKey(type.BaseType))
								continue;
						}
						result.Add(type);
					}
				}
				catch(Exception ex)
				{
					if( log.IsWarnEnabled )
					{
                        log.Warn(__Res.GetString(__Res.TypeHelper_LoadDllFail, file));
                        log.Warn(ex.Message);
					}
				}			
			}
			return (Type[])result.ToArray(typeof(Type));
		}
#endif
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
		public static bool SkipMethod(MethodInfo methodInfo)
		{
			if (methodInfo.ReturnType == typeof(IAsyncResult))
				return true;
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				if (parameterInfo.ParameterType == typeof(IAsyncResult))
					return true;
			}
			return false;
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public static string GetDescription(Type type)
		{
            Attribute attribute = ReflectionUtils.GetAttribute(typeof(DescriptionAttribute), type, false);
			if (attribute != null)
				return ((DescriptionAttribute) attribute).Description;
			return null;
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
		public static string GetDescription(MethodInfo methodInfo)
		{
            Attribute attribute = ReflectionUtils.GetAttribute(typeof(DescriptionAttribute), methodInfo, false);
            if (attribute != null)
                return ((DescriptionAttribute) attribute).Description;
            return null;
		}

		internal static void NarrowValues(object[] values, ParameterInfo[] parameterInfos)
		{
			//Narrow down convertibe types (double for example)
			for (int i = 0; values != null && i < values.Length; i++)
			{
				object value = values[i];
				values[i] = ChangeType(value, parameterInfos[i].ParameterType);
			}
		}

		internal static object GetNullValue(Type type)
		{
            if (type == null) throw new ArgumentNullException("type");

			if( FluorineConfiguration.Instance.NullableValues != null )
			{
                if (FluorineConfiguration.Instance.NullableValues.ContainsKey(type))
                    return FluorineConfiguration.Instance.NullableValues[type];
			}
            if (type.IsValueType)
            {
                /* Not supported
                if (type.IsEnum)
                    return GetEnumNullValue(type);
                */
                if (type.IsPrimitive)
                {
                    if (type == typeof(Int32)) return 0;
                    if (type == typeof(Double)) return (Double)0;
                    if (type == typeof(Int16)) return (Int16)0;
                    if (type == typeof(Boolean)) return false;
                    if (type == typeof(SByte)) return (SByte)0;
                    if (type == typeof(Int64)) return (Int64)0;
                    if (type == typeof(Byte)) return (Byte)0;
                    if (type == typeof(UInt16)) return (UInt16)0;
                    if (type == typeof(UInt32)) return (UInt32)0;
                    if (type == typeof(UInt64)) return (UInt64)0;
                    if (type == typeof(Single)) return (Single)0;
                    if (type == typeof(Char)) return new char();
                }
                else
                {
                    if (type == typeof(DateTime)) return DateTime.MinValue;
                    if (type == typeof(Decimal)) return 0m;
                    if (type == typeof(Guid)) return Guid.Empty;

#if !SILVERLIGHT
                    if (type == typeof(SqlInt32)) return SqlInt32.Null;
                    if (type == typeof(SqlString)) return SqlString.Null;
                    if (type == typeof(SqlBoolean)) return SqlBoolean.Null;
                    if (type == typeof(SqlByte)) return SqlByte.Null;
                    if (type == typeof(SqlDateTime)) return SqlDateTime.Null;
                    if (type == typeof(SqlDecimal)) return SqlDecimal.Null;
                    if (type == typeof(SqlDouble)) return SqlDouble.Null;
                    if (type == typeof(SqlGuid)) return SqlGuid.Null;
                    if (type == typeof(SqlInt16)) return SqlInt16.Null;
                    if (type == typeof(SqlInt64)) return SqlInt64.Null;
                    if (type == typeof(SqlMoney)) return SqlMoney.Null;
                    if (type == typeof(SqlSingle)) return SqlSingle.Null;
                    if (type == typeof(SqlBinary)) return SqlBinary.Null;
#endif
                }
            }
            else
            {
                if (type == typeof(String)) return null;// string.Empty;
                if (type == typeof(DBNull)) return DBNull.Value;
            }
            return null;
        }

		internal static object CreateInstance(Type type)
		{
		    //Is this a generic type definition?
			if (ReflectionUtils.IsGenericType(type))
			{
				Type genericTypeDefinition = ReflectionUtils.GetGenericTypeDefinition(type);
				// Get the generic type parameters or type arguments.
				Type[] typeParameters = ReflectionUtils.GetGenericArguments(type);

				// Construct an array of type arguments to substitute for 
				// the type parameters of the generic class.
				// The array must contain the correct number of types, in 
				// the same order that they appear in the type parameter 
				// list.
				// Construct the type Dictionary<String, Example>.
				Type constructed = ReflectionUtils.MakeGenericType(genericTypeDefinition, typeParameters);
				object obj = Activator.CreateInstance(constructed);
				if (obj == null)
				{
#if !SILVERLIGHT
                    if(log != null && log.IsErrorEnabled)
					{
						string msg = string.Format("Could not instantiate the generic type {0}.", type.FullName);
						log.Error(msg);
					}
#endif
				}
				return obj;
			}
		    return Activator.CreateInstance(type);
		}

	    /// <summary>
        /// Detects the MONO runtime.
        /// </summary>
		public static bool IsMono
		{
			get
			{
				return (typeof(object).Assembly.GetType("System.MonoType") != null);
			}
		}

        class Location
        {
            private readonly string _path;
            private readonly string _description;

            public Location(string path, string description)
            {
                _path = path;
                _description = description;
            }

            public string Path
            {
                get { return _path; }
            }

            public string Description
            {
                get { return _description; }
            }
        }

        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>Lac locations used for assembly probing by FluorineFx.</returns>
		public static string[] GetLacLocations()
		{
#if !FXCLIENT
            List<Location> locations = new List<Location>();

            try
            {
                try
                {
                    // Dynamically-created assemblies
                    if (IsMono)
                    {
                        log.Debug("Checking Mono AppDomain.SetupInformation.DynamicBase");
                        // http://lists.ximian.com/pipermail/mono-list/2005-May/027274.html
                        // DynamicDirectory on Mono cannot be accessed
                        if (AppDomain.CurrentDomain.SetupInformation.DynamicBase != null)
                        {
                            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.DynamicBase);
                            Location location = new Location(path, "Mono AppDomain.SetupInformation.DynamicBase");
                            locations.Add(location);
                        }
                    }
                    else
                    {
                        log.Debug("Checking AppDomain.DynamicDirectory");
                        //.NET2 assemblies in DynamicDirectory
                        if (AppDomain.CurrentDomain.DynamicDirectory != null)
                        {
                            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.DynamicDirectory);
                            Location location = new Location(path, "AppDomain.DynamicDirectory");
                            locations.Add(location);
                        }
                    }
                }
                catch (SecurityException)
                {
                }

                try
                {
                    // If we are hosted in a web application check PhysicalApplicationPath\bin too
                    if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    {
                        log.Debug("Checking ASP.NET Bin folder");
                        string path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Bin");
                        if (Directory.Exists(path))
                        {
                            Location location = new Location(path, "ASP.NET Bin folder");
                            locations.Add(location);
                        }
                        else
                        {
                            path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "bin");
                            if (Directory.Exists(path))
                            {
                                Location location = new Location(path, "ASP.NET bin folder");
                                locations.Add(location);
                            }
                        }
                    }
                }
                catch (SecurityException)
                {
                }

                log.Debug("Checking FluorineFx location");
                try
                {
                    // This is the FluorineFx path
                    // We leave it lastly because of the ASP.NET shadow copying feature (a setup of assemblies with the same identity but at different location)
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (path != null)
                    {
                        Location location = new Location(path, "FluorineFx location");
                        locations.Add(location);
                    }
                }
                catch (SecurityException)
                {
                }

                // Sort out unique root paths
                for (int i = locations.Count - 1; i >= 0; i--)
                {
                    Location location1 = locations[i];
                    for (int j = locations.Count - 1; j >= 0; j--)
                    {
                        Location location2 = locations[j];
                        if (i != j && PathUtil.ContainsPath(location1.Path, location2.Path))
                        {
                            locations.RemoveAt(j);
                            break;
                        }
                    }
                }
                string[] lacLocations = new string[locations.Count];
                for (int i = 0; i < locations.Count; i++)
                {
                    lacLocations[i] = locations[i].Path;
                    if (log.IsDebugEnabled)
                        log.Debug(string.Format("Adding LAC location: {0}, path: {1}", locations[i].Description, locations[i].Path));
                }
                return lacLocations;
            }
            catch (Exception ex)
            {
                log.Error("An error occurred while configuring LAC locations. This may lead to assembly load failures.", ex);
            }
            return new string[0];
#else
            return new string[0];
#endif
		}
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetTypeIsAccessible(Type type)
        {
            if (type == null)
                return false;
            if (type.Assembly == typeof(TypeHelper).Assembly)
                return false;
#if !FXCLIENT
            if (FluorineConfiguration.Instance.RemotingServiceAttributeConstraint == RemotingServiceAttributeConstraint.Access)
            {
                //Additional check for RemotingServiceAttribute presence
                Attribute attribute = ReflectionUtils.GetAttribute(typeof(RemotingServiceAttribute), type, true);
                if (attribute != null)
                    return true;
                return false;
            }
            return true;
#else
            return true;
#endif
        }

        /// <summary>
        /// Returns the underlying type argument of the specified type.
        /// </summary>
        /// <param name="type">A <see cref="System.Type"/> instance. </param>
        /// <returns><list>
        /// <item>The type argument of the type parameter,
        /// if the type parameter is a closed generic nullable type.</item>
        /// <item>The underlying Type of enumType, if the type parameter is an enum type.</item>
        /// <item>Otherwise, the type itself.</item>
        /// </list>
        /// </returns>
        public static Type GetUnderlyingType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (ReflectionUtils.IsNullable(type))
				type = type.GetGenericArguments()[0];
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            return type;
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCSharpName(Type type)
        {
            int dimensions = 0;
            while (type.IsArray)
            {
                type = type.GetElementType();
                dimensions++;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append(".");

            Type[] parameters = Type.EmptyTypes;
            if (ReflectionUtils.IsGenericType(type))
            {
                if (ReflectionUtils.GetGenericArguments(type) != null)
                    parameters = ReflectionUtils.GetGenericArguments(type);
            }
            GetCSharpName(type, parameters, 0, sb);
            for (int i = 0; i < dimensions; i++)
            {
                sb.Append("[]");
            }
            return sb.ToString();
         }

        private static int GetCSharpName(Type type, Type[] parameters, int index, StringBuilder sb)
        {
            if (type.DeclaringType != null && type.DeclaringType != type)
            {
                index = GetCSharpName(type.DeclaringType, parameters, index, sb);
                sb.Append(".");
            }
            string name = type.Name;
            int length = name.IndexOf('`');
            if (length < 0)
                length = name.IndexOf('!');
            if (length > 0)
            {
                sb.Append(name.Substring(0, length));
                sb.Append("<");
                int paramCount = int.Parse(name.Substring(length + 1), System.Globalization.CultureInfo.InvariantCulture) + index;
                while (index < paramCount)
                {
                    sb.Append(GetCSharpName(parameters[index]));
                    if (index < (paramCount - 1))
                    {
                        sb.Append(",");
                    }
                    index++;
                }
                sb.Append(">");
                return index;
            }
            sb.Append(name);
            return index;
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsAssignable(object obj, Type targetType)
        {
            return IsAssignable(obj, targetType, ReflectionUtils.IsNullable(targetType));
        }

        private static bool IsAssignable(object obj, Type targetType, bool isNullable)
        {
            if (obj != null && targetType.IsAssignableFrom(obj.GetType()))
                return true;//targetType can be assigned from an instance of the obj's Type
            if (isNullable && obj == null )
                return true;//null is assignable to a nullable type
            if (targetType.IsArray)
            {
                if (null == obj)
                    return true;
                Type srcType = obj.GetType();

                if (srcType == targetType)
                    return true;

                if (srcType.IsArray)
                {
                    Type srcElementType = srcType.GetElementType();
                    Type dstElementType = targetType.GetElementType();

                    if (srcElementType.IsArray != dstElementType.IsArray
                        || (srcElementType.IsArray &&
                            srcElementType.GetArrayRank() != dstElementType.GetArrayRank()))
                    {
                        return false;
                    }

                    Array srcArray = (Array)obj;
                    int rank = srcArray.Rank;
                    if (rank == 1 && 0 == srcArray.GetLowerBound(0))
                    {
                        int arrayLength = srcArray.Length;
                        // Int32 is assignable from UInt32, SByte from Byte and so on.
                        if (dstElementType.IsAssignableFrom(srcElementType))
                            return true;
                        //This is a costly operation
                        for (int i = 0; i < arrayLength; ++i)
                            if (!IsAssignable(srcArray.GetValue(i), dstElementType))
                                return false;
                    }
                    else
                    {
                        //This is a costly operation
                        int arrayLength = 1;
                        int[] dimensions = new int[rank];
                        int[] indices = new int[rank];
                        int[] lbounds = new int[rank];

                        for (int i = 0; i < rank; ++i)
                        {
                            arrayLength *= (dimensions[i] = srcArray.GetLength(i));
                            lbounds[i] = srcArray.GetLowerBound(i);
                        }

                        for (int i = 0; i < arrayLength; ++i)
                        {
                            int index = i;
                            for (int j = rank - 1; j >= 0; --j)
                            {
                                indices[j] = index % dimensions[j] + lbounds[j];
                                index /= dimensions[j];
                            }
                            if (!IsAssignable(srcArray.GetValue(indices), dstElementType))
                                return false;
                        }
                    }
                    return true;
                }
            }
            else if (targetType.IsEnum)
            {
                try
                {
                    if (obj != null)
                    {
                        Enum.Parse(targetType, obj.ToString(), true);
                        return true;
                    }
                    return false;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            if (obj != null)
            {
                TypeConverter typeConverter = ReflectionUtils.GetTypeConverter(obj);//TypeDescriptor.GetConverter(obj);
                if (typeConverter != null && typeConverter.CanConvertTo(targetType))
                    return true;
                typeConverter = ReflectionUtils.GetTypeConverter(targetType);// TypeDescriptor.GetConverter(targetType);
                if (typeConverter != null && typeConverter.CanConvertFrom(obj.GetType()))
                    return true;

                //Collections
                if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.Generic.ICollection`1") && obj is IList)
                {
                    //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                    Type[] typeParameters = ReflectionUtils.GetGenericArguments(targetType);
                    if (typeParameters != null && typeParameters.Length == 1)
                    {
                        //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                        Type typeGenericICollection = targetType.GetInterface("System.Collections.Generic.ICollection`1", true);
                        return typeGenericICollection != null;
                    }
                    return false;
                }
                if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.IList") && obj is IList)
                {
                    return true;
                }

                if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.Generic.IDictionary`2") && obj is IDictionary)
                {
                    Type[] typeParameters = ReflectionUtils.GetGenericArguments(targetType);
                    if (typeParameters != null && typeParameters.Length == 2)
                    {
                        //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                        Type typeGenericIDictionary = targetType.GetInterface("System.Collections.Generic.IDictionary`2", true);
                        return typeGenericIDictionary != null;
                    }
                    return false;
                }
                if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.IDictionary") && obj is IDictionary)
                {
                    return true;
                }
            }
            else
            {
#if !SILVERLIGHT
                if (targetType is INullable)
                    return true;
#endif
                if (targetType.IsValueType)
                {
                    if (FluorineConfiguration.Instance.AcceptNullValueTypes)
                    {
                        // Any value-type that is not explicitly initialized with a value will 
                        // contain the default value for that object type.
                        return true;
                    }
                    return false;
                }
                return true;
            }

            try
            {
                if (isNullable)
                {
                    switch (Type.GetTypeCode(GetUnderlyingType(targetType)))
                    {
                        case TypeCode.Char: return CanConvertToNullableChar(obj);
                    }
                    if (typeof(Guid) == targetType) return CanConvertToNullableGuid(obj);
                }
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Char: return CanConvertToChar(obj);
                }
                if (typeof(Guid) == targetType) return CanConvertToGuid(obj);
            }
            catch (InvalidCastException)
            {
            }

#if !SILVERLIGHT && !NET_2_0
            if (typeof(System.Xml.Linq.XDocument) == targetType && obj is XmlDocument) return true;
            if (typeof(System.Xml.Linq.XElement) == targetType && obj is XmlDocument) return true;
#endif

            return false;
        }
        /// <summary>
        /// This method supports the Fluorine infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        static public object ChangeType(object value, Type targetType)
        {
            return ConvertChangeType(value, targetType, ReflectionUtils.IsNullable(targetType));
        }

        static T Cast<T>(object obj) where T : class
        {
            return obj as T;
        }

        private static object ConvertChangeType(object value, Type targetType, bool isNullable)
        {
            if (targetType.IsArray)
            {
                if (null == value)
                    return null;

                Type srcType = value.GetType();

                if (srcType == targetType)
                    return value;

                if (srcType.IsArray)
                {
                    Type srcElementType = srcType.GetElementType();
                    Type dstElementType = targetType.GetElementType();

                    if (srcElementType.IsArray != dstElementType.IsArray
                        || (srcElementType.IsArray &&
                            srcElementType.GetArrayRank() != dstElementType.GetArrayRank()))
                    {
                        throw new InvalidCastException(string.Format("Can not convert array of type '{0}' to array of '{1}'.", srcType.FullName, targetType.FullName));
                    }

                    Array srcArray = (Array)value;
                    Array dstArray;

                    int rank = srcArray.Rank;

                    if (rank == 1 && 0 == srcArray.GetLowerBound(0))
                    {
                        int arrayLength = srcArray.Length;

                        dstArray = Array.CreateInstance(dstElementType, arrayLength);

                        // Int32 is assignable from UInt32, SByte from Byte and so on.
                        //
                        if (dstElementType.IsAssignableFrom(srcElementType))
                            Array.Copy(srcArray, dstArray, arrayLength);
                        else
                            for (int i = 0; i < arrayLength; ++i)
                                dstArray.SetValue(ConvertChangeType(srcArray.GetValue(i), dstElementType, isNullable), i);
                    }
                    else
                    {
#if !SILVERLIGHT
                        int arrayLength = 1;
                        int[] dimensions = new int[rank];
                        int[] indices = new int[rank];
                        int[] lbounds = new int[rank];

                        for (int i = 0; i < rank; ++i)
                        {
                            arrayLength *= (dimensions[i] = srcArray.GetLength(i));
                            lbounds[i] = srcArray.GetLowerBound(i);
                        }

                        dstArray = Array.CreateInstance(dstElementType, dimensions, lbounds);
                        for (int i = 0; i < arrayLength; ++i)
                        {
                            int index = i;
                            for (int j = rank - 1; j >= 0; --j)
                            {
                                indices[j] = index % dimensions[j] + lbounds[j];
                                index /= dimensions[j];
                            }

                            dstArray.SetValue(ConvertChangeType(srcArray.GetValue(indices), dstElementType, isNullable), indices);
                        }
#else
                        throw new InvalidCastException();
#endif
                    }

                    return dstArray;
                }
            }
            else if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, value.ToString(), true);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidCastException(__Res.GetString(__Res.TypeHelper_ConversionFail), ex);
                }
            }

            if (isNullable)
            {
                switch (Type.GetTypeCode(GetUnderlyingType(targetType)))
                {
                    case TypeCode.Boolean:  return ConvertToNullableBoolean (value);
                    case TypeCode.Byte:     return ConvertToNullableByte    (value);
                    case TypeCode.Char:     return ConvertToNullableChar    (value);
                    case TypeCode.DateTime: return ConvertToNullableDateTime(value);
                    case TypeCode.Decimal:  return ConvertToNullableDecimal (value);
                    case TypeCode.Double:   return ConvertToNullableDouble  (value);
                    case TypeCode.Int16:    return ConvertToNullableInt16   (value);
                    case TypeCode.Int32:    return ConvertToNullableInt32   (value);
                    case TypeCode.Int64:    return ConvertToNullableInt64   (value);
                    case TypeCode.SByte:    return ConvertToNullableSByte   (value);
                    case TypeCode.Single:   return ConvertToNullableSingle  (value);
                    case TypeCode.UInt16:   return ConvertToNullableUInt16  (value);
                    case TypeCode.UInt32:   return ConvertToNullableUInt32  (value);
                    case TypeCode.UInt64:   return ConvertToNullableUInt64  (value);
                }
                if (typeof(Guid) == GetUnderlyingType(targetType)) return ConvertToNullableGuid(value);
            }

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Boolean: return ConvertToBoolean(value);
                case TypeCode.Byte: return ConvertToByte(value);
                case TypeCode.Char: return ConvertToChar(value);
                case TypeCode.DateTime: return ConvertToDateTime(value);
                case TypeCode.Decimal: return ConvertToDecimal(value);
                case TypeCode.Double: return ConvertToDouble(value);
                case TypeCode.Int16: return ConvertToInt16(value);
                case TypeCode.Int32: return ConvertToInt32(value);
                case TypeCode.Int64: return ConvertToInt64(value);
                case TypeCode.SByte: return ConvertToSByte(value);
                case TypeCode.Single: return ConvertToSingle(value);
                case TypeCode.String: return ConvertToString(value);
                case TypeCode.UInt16: return ConvertToUInt16(value);
                case TypeCode.UInt32: return ConvertToUInt32(value);
                case TypeCode.UInt64: return ConvertToUInt64(value);
            }

            if (typeof(Guid) == targetType) return ConvertToGuid(value);
#if !SILVERLIGHT
            if (typeof(XmlDocument) == targetType) return ConvertToXmlDocument(value);
#endif
#if !SILVERLIGHT && !NET_2_0
            if (typeof(System.Xml.Linq.XDocument) == targetType) return ConvertToXDocument(value);
            if (typeof(System.Xml.Linq.XElement) == targetType) return ConvertToXElement(value);
#endif
            if (typeof(byte[]) == targetType) return ConvertToByteArray(value);
            if (typeof(char[]) == targetType) return ConvertToCharArray(value);

#if !SILVERLIGHT
            if (typeof(SqlInt32) == targetType) return ConvertToSqlInt32(value);
            if (typeof(SqlString) == targetType) return ConvertToSqlString(value);
            if (typeof(SqlDecimal) == targetType) return ConvertToSqlDecimal(value);
            if (typeof(SqlDateTime) == targetType) return ConvertToSqlDateTime(value);
            if (typeof(SqlBoolean) == targetType) return ConvertToSqlBoolean(value);
            if (typeof(SqlMoney) == targetType) return ConvertToSqlMoney(value);
            if (typeof(SqlGuid) == targetType) return ConvertToSqlGuid(value);
            if (typeof(SqlDouble) == targetType) return ConvertToSqlDouble(value);
            if (typeof(SqlByte) == targetType) return ConvertToSqlByte(value);
            if (typeof(SqlInt16) == targetType) return ConvertToSqlInt16(value);
            if (typeof(SqlInt64) == targetType) return ConvertToSqlInt64(value);
            if (typeof(SqlSingle) == targetType) return ConvertToSqlSingle(value);
            if (typeof(SqlBinary) == targetType) return ConvertToSqlBinary(value);
#endif
            if (value == null)
                return null;
            //Check whether the target Type can be assigned from the value's Type
            if (targetType.IsAssignableFrom(value.GetType()))
                return value;//Skip further adapting

            //Try to convert using a type converter
            TypeConverter typeConverter = ReflectionUtils.GetTypeConverter(targetType);// TypeDescriptor.GetConverter(targetType);
            if (typeConverter != null && typeConverter.CanConvertFrom(value.GetType()))
                return typeConverter.ConvertFrom(value);
            //Custom type converters handled here (for example ByteArray)
            typeConverter = ReflectionUtils.GetTypeConverter(value);// TypeDescriptor.GetConverter(value);
            if (typeConverter != null && typeConverter.CanConvertTo(targetType))
                return typeConverter.ConvertTo(value, targetType);

            if (targetType.IsInterface)
            {
                MethodInfo castMethod = typeof(TypeHelper).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(targetType);
                object castedObject = castMethod.Invoke(null, new object[] { value });
                if (castedObject != null)
                    return castedObject;
            }
            //Collections
            if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.Generic.ICollection`1") && value is IList)
            {
                object obj = null;
                if (CollectionUtils.IsListType(targetType))
                    obj = CollectionUtils.CreateList(targetType);
                if( obj == null )
                    obj = CreateInstance(targetType);
                if (obj != null)
                {
                    //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                    Type[] typeParameters = ReflectionUtils.GetGenericArguments(targetType);
                    if (typeParameters != null && typeParameters.Length == 1)
                    {
                        //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                        Type typeGenericICollection = targetType.GetInterface("System.Collections.Generic.ICollection`1", true);
                        MethodInfo miAddCollection = typeGenericICollection.GetMethod("Add");
                        IList source = value as IList;
                        for (int i = 0; i < (value as IList).Count; i++)
                            miAddCollection.Invoke(obj, new object[] { ChangeType(source[i], typeParameters[0]) });
                    }
                    else
                    {
#if !SILVERLIGHT
                        if (log.IsErrorEnabled)
                            log.Error(string.Format("{0} type arguments of the generic type {1} expecting 1.", typeParameters.Length, targetType.FullName));
#endif
                    }
                    return obj;
                }
            }
            if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.IList") && value is IList)
            {
                object obj = CreateInstance(targetType);
                if (obj != null)
                {
                    IList source = value as IList;
                    IList destination = obj as IList;
                    for (int i = 0; i < source.Count; i++)
                        destination.Add(source[i]);
                    return obj;
                }
            }
            if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.Generic.IDictionary`2") && value is IDictionary)
            {
                object obj = CreateInstance(targetType);
                if (obj != null)
                {
                    IDictionary source = value as IDictionary;
                    Type[] typeParameters = ReflectionUtils.GetGenericArguments(targetType);
                    if (typeParameters != null && typeParameters.Length == 2)
                    {
                        //For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
                        Type typeGenericIDictionary = targetType.GetInterface("System.Collections.Generic.IDictionary`2", true);
                        MethodInfo miAddCollection = typeGenericIDictionary.GetMethod("Add");
                        IDictionary dictionary = value as IDictionary;
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            miAddCollection.Invoke(obj, new object[] {
                                ChangeType(entry.Key, typeParameters[0]),
                                ChangeType(entry.Value, typeParameters[1]) 
                            });
                        }
                    }
                    else
                    {
#if !SILVERLIGHT
                        if (log.IsErrorEnabled)
                            log.Error(string.Format("{0} type arguments of the generic type {1} expecting 1.", typeParameters.Length, targetType.FullName));
#endif
                    }
                    return obj;
                }
            }

            if (ReflectionUtils.ImplementsInterface(targetType, "System.Collections.IDictionary") && value is IDictionary)
            {
                object obj = CreateInstance(targetType);
                if (obj != null)
                {
                    IDictionary source = value as IDictionary;
                    IDictionary destination = obj as IDictionary;
                    foreach (DictionaryEntry entry in source)
                        destination.Add(entry.Key, entry.Value);
                    return obj;
                }
            }

            return System.Convert.ChangeType(value, targetType, null);
        }

        #region Nullable Types

        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 8-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 8-bit signed integer.</returns>
        [CLSCompliant(false)]
        public static SByte? ConvertToNullableSByte(object value)
        {
            if (value is SByte) return (SByte?)value;
            if (value == null)  return null;
            return Util.Convert.ToNullableSByte(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 16-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 16-bit signed integer.</returns>
        public static Int16? ConvertToNullableInt16(object value)
        {
            if (value is Int16) return (Int16?)value;
            if (value == null)  return null;

            return Util.Convert.ToNullableInt16(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 32-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 32-bit signed integer.</returns>
        public static Int32? ConvertToNullableInt32(object value)
        {
            if (value is Int32) return (Int32?)value;
            if (value == null)  return null;

            return Util.Convert.ToNullableInt32(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 64-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 64-bit signed integer.</returns>
        public static Int64? ConvertToNullableInt64(object value)
        {
            if (value is Int64) return (Int64?)value;
            if (value == null)  return null;

            return Util.Convert.ToNullableInt64(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 8-bit unsigned integer.</returns>
        public static Byte? ConvertToNullableByte(object value)
        {
            if (value is Byte) return (Byte?)value;
            if (value == null) return null;

            return Util.Convert.ToNullableByte(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 16-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt16? ConvertToNullableUInt16(object value)
        {
            if (value is UInt16) return (UInt16?)value;
            if (value == null)   return null;

            return Util.Convert.ToNullableUInt16(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 32-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt32? ConvertToNullableUInt32(object value)
        {
            if (value is UInt32) return (UInt32?)value;
            if (value == null)   return null;

            return Util.Convert.ToNullableUInt32(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable 64-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt64? ConvertToNullableUInt64(object value)
        {
            if (value is UInt64) return (UInt64?)value;
            if (value == null)   return null;

            return Util.Convert.ToNullableUInt64(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable Unicode character.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable Unicode character.</returns>
        public static Char? ConvertToNullableChar(object value)
        {
            if (value is Char) return (Char?)value;
            if (value == null) return null;

            return Util.Convert.ToNullableChar(value);
        }
        /// <summary>
        /// Checks whether the specified Object can be converted to a nullable Unicode character.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>true if the specified Object can be converted to a nullable Unicode character, false otherwise.</returns>
        public static bool CanConvertToNullableChar(object value)
        {
            if (value is Char) return true;
            if (value == null) return true;
            return Util.Convert.CanConvertToNullableChar(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable double-precision floating point number.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable double-precision floating point number.</returns>
        public static Double? ConvertToNullableDouble(object value)
        {
            if (value is Double) return (Double?)value;
            if (value == null)   return null;

            return Util.Convert.ToNullableDouble(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable single-precision floating point number.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable single-precision floating point number.</returns>
        public static Single? ConvertToNullableSingle(object value)
        {
            if (value is Single) return (Single?)value;
            if (value == null)   return null;

            return Util.Convert.ToNullableSingle(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent to a nullable Boolean value.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable Boolean value.</returns>
        public static Boolean? ConvertToNullableBoolean(object value)
        {
            if (value is Boolean) return (Boolean?)value;
            if (value == null)    return null;

            return Util.Convert.ToNullableBoolean(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable DateTime.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable DateTime.</returns>
        public static DateTime? ConvertToNullableDateTime(object value)
        {
            if (value is DateTime) return (DateTime?)value;
            if (value == null)     return null;

            return Util.Convert.ToNullableDateTime(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable Decimal.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable Decimal.</returns>
        public static Decimal? ConvertToNullableDecimal(object value)
        {
            if (value is Decimal) return (Decimal?)value;
            if (value == null)    return null;

            return Util.Convert.ToNullableDecimal(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent nullable Guid.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent nullable Guid.</returns>
        public static Guid? ConvertToNullableGuid(object value)
        {
            if (value is Guid) return (Guid?)value;
            if (value == null) return null;

            return Util.Convert.ToNullableGuid(value);
        }
        /// <summary>
        /// Checks whether the specified Object can be converted to a nullable Guid.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>true if the specified Object can be converted to a nullable Guid, false otherwise.</returns>
        public static bool CanConvertToNullableGuid(object value)
        {
            if (value is Guid) return true;
            if (value == null) return true;
            return Util.Convert.CanConvertToNullableGuid(value);
        }
        #endregion

        #region Primitive Types

        static readonly SByte DefaultSByteNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 8-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 8-bit signed integer.</returns>
        [CLSCompliant(false)]
        public static SByte ConvertToSByte(object value)
        {
            return
                value is SByte ? (SByte)value :
                value == null ? DefaultSByteNullValue :
                    Util.Convert.ToSByte(value);
        }
        static readonly Int16 DefaultInt16NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 16-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 16-bit signed integer.</returns>
        public static Int16 ConvertToInt16(object value)
        {
            return
                value is Int16 ? (Int16)value :
                value == null ? DefaultInt16NullValue :
                    Util.Convert.ToInt16(value);
        }
        
        static readonly Int32 DefaultInt32NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 32-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 32-bit signed integer.</returns>
        public static Int32 ConvertToInt32(object value)
        {
            return
                value is Int32 ? (Int32)value :
                value == null ? DefaultInt32NullValue :
                    Util.Convert.ToInt32(value);
        }

        static readonly Int64 DefaultInt64NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 64-bit signed integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 64-bit signed integer.</returns>
        public static Int64 ConvertToInt64(object value)
        {
            return
                value is Int64 ? (Int64)value :
                value == null ? DefaultInt64NullValue :
                    Util.Convert.ToInt64(value);
        }

        static readonly Byte DefaultByteNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 8-bit unsigned integer.</returns>
        public static Byte ConvertToByte(object value)
        {
            return
                value is Byte ? (Byte)value :
                value == null ? DefaultByteNullValue :
                    Util.Convert.ToByte(value);
        }

        static readonly UInt16 DefaultUInt16NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 16-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt16 ConvertToUInt16(object value)
        {
            return
                value is UInt16 ? (UInt16)value :
                value == null ? DefaultUInt16NullValue :
                    Util.Convert.ToUInt16(value);
        }
        static readonly UInt32 DefaultUInt32NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 32-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt32 ConvertToUInt32(object value)
        {
            return
                value is UInt32 ? (UInt32)value :
                value == null ? DefaultUInt32NullValue :
                    Util.Convert.ToUInt32(value);
        }

        static readonly UInt64 DefaultUInt64NullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent 64-bit unsigned integer.</returns>
        [CLSCompliant(false)]
        public static UInt64 ConvertToUInt64(object value)
        {
            return
                value is UInt64 ? (UInt64)value :
                value == null ? DefaultUInt64NullValue :
                    Util.Convert.ToUInt64(value);
        }

        static readonly Char DefaultCharNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent Unicode character.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent Unicode character.</returns>
        public static Char ConvertToChar(object value)
        {
            return
                value is Char ? (Char)value :
                value == null ? DefaultCharNullValue :
                    Util.Convert.ToChar(value);
        }
        /// <summary>
        /// Checks whether the specified Object can be converted to a Unicode character.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>true if the specified Object can be converted to a Unicode character, false otherwise.</returns>
        public static bool CanConvertToChar(object value)
        {
            return
                value is Char ? true :
                value == null ? true :
                    Util.Convert.CanConvertToChar(value);
        }

        static readonly Single DefaultSingleNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent single-precision floating point number.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent single-precision floating point number.</returns>
        public static Single ConvertToSingle(object value)
        {
            return
                value is Single ? (Single)value :
                value == null ? DefaultSingleNullValue :
                    Util.Convert.ToSingle(value);
        }

        static readonly Double DefaultDoubleNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent double-precision floating point number.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent double-precision floating point number.</returns>
        public static Double ConvertToDouble(object value)
        {
            return
                value is Double ? (Double)value :
                value == null ? DefaultDoubleNullValue :
                    Util.Convert.ToDouble(value);
        }

        static readonly Boolean DefaultBooleanNullValue;
        /// <summary>
        /// Checks whether the specified Object can be converted to a Boolean value.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent Boolean value.</returns>
        public static Boolean ConvertToBoolean(object value)
        {
            return
                value is Boolean ? (Boolean)value :
                value == null ? DefaultBooleanNullValue :
                    Util.Convert.ToBoolean(value);
        }

        #endregion

        #region Simple Types

        static readonly string DefaultStringNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent String.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent String.</returns>
        public static String ConvertToString(object value)
        {
            return
                value is String ? (String)value :
                value == null ? DefaultStringNullValue :
                    Util.Convert.ToString(value);
        }

        static readonly DateTime DefaultDateTimeNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent DateTime.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent DateTime.</returns>
        public static DateTime ConvertToDateTime(object value)
        {
            return
                value is DateTime ? (DateTime)value :
                value == null ? DefaultDateTimeNullValue :
                    Util.Convert.ToDateTime(value);
        }

        static readonly decimal DefaultDecimalNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent Decimal.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent Decimal.</returns>
        public static Decimal ConvertToDecimal(object value)
        {
            return
                value is Decimal ? (Decimal)value :
                value == null ? DefaultDecimalNullValue :
                    Util.Convert.ToDecimal(value);
        }

        static readonly Guid DefaultGuidNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent Guid.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent Guid.</returns>
        public static Guid ConvertToGuid(object value)
        {
            return
                value is Guid ? (Guid)value :
                value == null ? DefaultGuidNullValue :
                    Util.Convert.ToGuid(value);
        }
        /// <summary>
        /// Checks whether the specified Object can be converted to a Guid.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>true if the specified Object can be converted to a Guid, false otherwise.</returns>
        public static bool CanConvertToGuid(object value)
        {
            return
                value is Guid ? true :
                value == null ? true :
                    Util.Convert.CanConvertToGuid(value);
        }

        static readonly XmlReader DefaultXmlReaderNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent XmlReader.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent XmlReader.</returns>
        public static XmlReader ConvertToXmlReader(object value)
        {
            return
                value is XmlReader ? (XmlReader)value :
                value == null ? DefaultXmlReaderNullValue :
                    Util.Convert.ToXmlReader(value);
        }
#if !SILVERLIGHT
        static readonly XmlDocument DefaultXmlDocumentNullValue;
        /// <summary>
        /// Converts the value of the specified Object to its equivalent XmlDocument.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent XmlDocument.</returns>
        public static XmlDocument ConvertToXmlDocument(object value)
        {
            return
                value is XmlDocument ? (XmlDocument)value :
                value == null ? DefaultXmlDocumentNullValue :
                    Util.Convert.ToXmlDocument(value);
        }
#endif
#if !NET_2_0
        static XDocument _defaultXDocumentNullValue;
        public static XDocument ConvertToXDocument(object value)
        {
            return
                value is XDocument ? (XDocument)value :
                value == null ? _defaultXDocumentNullValue :
                    FluorineFx.Util.Convert.ToXDocument(value);
        }

        static XElement _defaultXElementNullValue;
        public static XElement ConvertToXElement(object value)
        {
            return
                value is XElement ? (XElement)value :
                value == null ? _defaultXElementNullValue :
                    FluorineFx.Util.Convert.ToXElement(value);
        }

#endif
        /// <summary>
        /// Converts the value of the specified Object to a byte array.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The byte array.</returns>
        public static byte[] ConvertToByteArray(object value)
        {
            return
                value is byte[] ? (byte[])value :
                value == null ? null :
                    Util.Convert.ToByteArray(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to a character array.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The character array.</returns>
        public static char[] ConvertToCharArray(object value)
        {
            return
                value is char[] ? (char[])value :
                value == null ? null :
                    Util.Convert.ToCharArray(value);
        }

        #endregion

        #region SqlTypes
#if !SILVERLIGHT
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlByte.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlByte.</returns>
        public static SqlByte ConvertToSqlByte(object value)
        {
            return
                value == null ? SqlByte.Null :
                value is SqlByte ? (SqlByte)value :
                    Util.Convert.ToSqlByte(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlInt16.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlInt16.</returns>
        public static SqlInt16 ConvertToSqlInt16(object value)
        {
            return
                value == null ? SqlInt16.Null :
                value is SqlInt16 ? (SqlInt16)value :
                    Util.Convert.ToSqlInt16(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlInt32.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlInt32.</returns>
        public static SqlInt32 ConvertToSqlInt32(object value)
        {
            return
                value == null ? SqlInt32.Null :
                value is SqlInt32 ? (SqlInt32)value :
                    Util.Convert.ToSqlInt32(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlInt64.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlInt64.</returns>
        public static SqlInt64 ConvertToSqlInt64(object value)
        {
            return
                value == null ? SqlInt64.Null :
                value is SqlInt64 ? (SqlInt64)value :
                    Util.Convert.ToSqlInt64(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlSingle.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlSingle.</returns>
        public static SqlSingle ConvertToSqlSingle(object value)
        {
            return
                value == null ? SqlSingle.Null :
                value is SqlSingle ? (SqlSingle)value :
                    Util.Convert.ToSqlSingle(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlBoolean.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlBoolean.</returns>
        public static SqlBoolean ConvertToSqlBoolean(object value)
        {
            return
                value == null ? SqlBoolean.Null :
                value is SqlBoolean ? (SqlBoolean)value :
                    Util.Convert.ToSqlBoolean(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlDouble.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlDouble.</returns>
        public static SqlDouble ConvertToSqlDouble(object value)
        {
            return
                value == null ? SqlDouble.Null :
                value is SqlDouble ? (SqlDouble)value :
                    Util.Convert.ToSqlDouble(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlDateTime.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlDateTime.</returns>
        public static SqlDateTime ConvertToSqlDateTime(object value)
        {
            return
                value == null ? SqlDateTime.Null :
                value is SqlDateTime ? (SqlDateTime)value :
                    Util.Convert.ToSqlDateTime(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlDecimal.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlDecimal.</returns>
        public static SqlDecimal ConvertToSqlDecimal(object value)
        {
            return
                value == null ? SqlDecimal.Null :
                value is SqlDecimal ? (SqlDecimal)value :
                value is SqlMoney ? ((SqlMoney)value).ToSqlDecimal() :
                    Util.Convert.ToSqlDecimal(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlMoney.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlMoney.</returns>
        public static SqlMoney ConvertToSqlMoney(object value)
        {
            return
                value == null ? SqlMoney.Null :
                value is SqlMoney ? (SqlMoney)value :
                value is SqlDecimal ? ((SqlDecimal)value).ToSqlMoney() :
                    Util.Convert.ToSqlMoney(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlString.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlString.</returns>
        public static SqlString ConvertToSqlString(object value)
        {
            return
                value == null ? SqlString.Null :
                value is SqlString ? (SqlString)value :
                    Util.Convert.ToSqlString(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlBinary.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlBinary.</returns>
        public static SqlBinary ConvertToSqlBinary(object value)
        {
            return
                value == null ? SqlBinary.Null :
                value is SqlBinary ? (SqlBinary)value :
                    Util.Convert.ToSqlBinary(value);
        }
        /// <summary>
        /// Converts the value of the specified Object to its equivalent SqlGuid.
        /// </summary>
        /// <param name="value">An Object.</param>
        /// <returns>The equivalent SqlGuid.</returns>
        public static SqlGuid ConvertToSqlGuid(object value)
        {
            return
                value == null ? SqlGuid.Null :
                value is SqlGuid ? (SqlGuid)value :
                    Util.Convert.ToSqlGuid(value);
        }
#endif
        #endregion

        #region DataSet conversions
#if !SILVERLIGHT
        /// <summary>
        /// Converts the specified DataTable to its equivalent ASObject.
        /// </summary>
        /// <param name="dataTable">A DataTable.</param>
        /// <param name="stronglyTyped">Indicates whether the ASObject is strongly typed (AS2 Recordset class).</param>
        /// <returns>The equivalent ASObject.</returns>
        public static ASObject ConvertDataTableToASO(DataTable dataTable, bool stronglyTyped)
        {
            if (dataTable.ExtendedProperties.Contains("DynamicPage"))
                return ConvertPageableDataTableToASO(dataTable, stronglyTyped);
            ASObject recordset = new ASObject();
            if (stronglyTyped)
                recordset.TypeName = "RecordSet";

            ASObject asObject = new ASObject();
            if (dataTable.ExtendedProperties["TotalCount"] != null)
                asObject["totalCount"] = (int)dataTable.ExtendedProperties["TotalCount"];
            else
                asObject["totalCount"] = dataTable.Rows.Count;

            if (dataTable.ExtendedProperties["Service"] != null)
                asObject["serviceName"] = "rs://" + dataTable.ExtendedProperties["Service"];
            else
                asObject["serviceName"] = "FluorineFx.PageableResult";
            asObject["version"] = 1;
            asObject["cursor"] = 1;
            if (dataTable.ExtendedProperties["RecordsetId"] != null)
                asObject["id"] = dataTable.ExtendedProperties["RecordsetId"] as string;
            else
                asObject["id"] = null;
            string[] columnNames = new string[dataTable.Columns.Count];
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                columnNames[i] = dataTable.Columns[i].ColumnName;
            }
            asObject["columnNames"] = columnNames;
            object[] rows = new object[dataTable.Rows.Count];
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                rows[i] = dataTable.Rows[i].ItemArray;
            }
            asObject["initialData"] = rows;

            recordset["serverInfo"] = asObject;
            return recordset;
        }
        /// <summary>
        /// Converts the specified DataTable to its equivalent ASObject (pageable RecordSet).
        /// </summary>
        /// <param name="dataTable">A DataTable.</param>
        /// <param name="stronglyTyped">Indicates whether the ASObject is strongly typed (AS2 RecordSetPage class).</param>
        /// <returns>The equivalent ASObject.</returns>
        public static ASObject ConvertPageableDataTableToASO(DataTable dataTable, bool stronglyTyped)
        {
            ASObject recordSetPage = new ASObject();
            if (stronglyTyped)
                recordSetPage.TypeName = "RecordSetPage";
            recordSetPage["Cursor"] = (int)dataTable.ExtendedProperties["Cursor"];//pagecursor

            ArrayList rows = new ArrayList();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                rows.Add(dataTable.Rows[i].ItemArray);
            }
            recordSetPage["Page"] = rows; ;
            return recordSetPage;
        }
        /// <summary>
        /// Converts the specified DataSet to its equivalent ASObject.
        /// </summary>
        /// <param name="dataSet">A DataSet.</param>
        /// <param name="stronglyTyped">Indicates whether the ASObject is strongly typed (property values of the root ASObject will be AS2 RecordSet objects).</param>
        /// <returns>The equivalent ASObject.</returns>
        public static ASObject ConvertDataSetToASO(DataSet dataSet, bool stronglyTyped)
        {
            ASObject asDataSet = new ASObject();
            if (stronglyTyped)
                asDataSet.TypeName = "DataSet";
            DataTableCollection dataTableCollection = dataSet.Tables;
            foreach (DataTable dataTable in dataTableCollection)
            {
                asDataSet[dataTable.TableName] = ConvertDataTableToASO(dataTable, stronglyTyped);
            }
            return asDataSet;
        }
#endif
        #endregion DataSet conversions
    }
}
