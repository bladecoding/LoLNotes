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

using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if (NET_1_1)
[assembly: AssemblyTitle("FluorineFx for .NET Framework 1.1")]
#elif (NET_2_0)
[assembly: AssemblyTitle("FluorineFx for .NET Framework 2.0")]
#elif (NET_3_5)
[assembly: AssemblyTitle("FluorineFx for .NET Framework 3.5")]
#elif (NET_4_0)
[assembly: AssemblyTitle("FluorineFx for .NET Framework 4.0")]
#elif (MONO)
[assembly: AssemblyTitle("FluorineFx for MONO 2.0")]
#else
[assembly: AssemblyTitle("FluorineFx")]
#endif

#if DEBUG
[assembly: AssemblyDescription("FluorineFx for .NET [Debug build]")]
#else
[assembly: AssemblyDescription("FluorineFx for .NET [Retail]")]
#endif

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("FluorineFx.com")]
[assembly: AssemblyProduct("FluorineFx")]
[assembly: AssemblyCopyright("FluorineFx.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		
// Configure log4net using the .log4net file
[assembly: log4net.Config.XmlConfigurator(ConfigFile="log4net.config",Watch=true)]
// The config file will be watched for changes.
[assembly: System.Security.AllowPartiallyTrustedCallers()]
[assembly: System.CLSCompliant(true)]

#if (NET_4_0)
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
#endif
//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("1.0.0.17")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
#if STRONG
[assembly: AssemblyDelaySign(false)]
#if MONO
[assembly: AssemblyKeyFile("../../snk/fluorine.snk")]
#else
[assembly: AssemblyKeyFile("..\\..\\..\\snk\\fluorine.snk")]
#endif
[assembly: AssemblyKeyName("")]
#endif
