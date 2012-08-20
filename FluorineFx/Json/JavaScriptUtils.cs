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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using FluorineFx.Util;

namespace FluorineFx.Json
{
	internal class JavaScriptUtils
	{
		public static void WriteEscapedJavaScriptChar(TextWriter writer, char c, char delimiter)
		{
			switch (c)
			{
				case '\t':
					writer.Write(@"\t");
					break;
				case '\n':
					writer.Write(@"\n");
					break;
				case '\r':
					writer.Write(@"\r");
					break;
				case '\f':
					writer.Write(@"\f");
					break;
				case '\b':
					writer.Write(@"\b");
					break;
				case '\\':
					writer.Write(@"\\");
					break;
					//case '<':
					//case '>':
					//case '\'':
					//  StringUtils.WriteCharAsUnicode(writer, c);
					//  break;
				case '\'':
					// only escape if this charater is being used as the delimiter
					writer.Write((delimiter == '\'') ? @"\'" : @"'");
					break;
				case '"':
					// only escape if this charater is being used as the delimiter
					writer.Write((delimiter == '"') ? "\\\"" : @"""");
					break;
				default:
					if (c > '\u001f')
						writer.Write(c);
					else
						StringUtils.WriteCharAsUnicode(writer, c);
					break;
			}
		}

		public static void WriteEscapedJavaScriptString(TextWriter writer, string value, char delimiter, bool appendDelimiters)
		{
			// leading delimiter
			if (appendDelimiters)
				writer.Write(delimiter);

			if (value != null)
			{
				for (int i = 0; i < value.Length; i++)
				{
					WriteEscapedJavaScriptChar(writer, value[i], delimiter);
				}
			}

			// trailing delimiter
			if (appendDelimiters)
				writer.Write(delimiter);
		}

		public static string ToEscapedJavaScriptString(string value)
		{
			return ToEscapedJavaScriptString(value, '"', true);
		}

		public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters)
		{
			using (StringWriter w = StringUtils.CreateStringWriter(StringUtils.GetLength(value) != 0 ? StringUtils.GetLength(value) : 16))
			{
				WriteEscapedJavaScriptString(w, value, delimiter, appendDelimiters);
				return w.ToString();
			}
		}
	}
}