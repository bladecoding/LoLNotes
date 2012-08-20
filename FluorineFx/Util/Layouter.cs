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
using System.Text;

namespace FluorineFx.Util
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// Pretty-print code formatted using line breaks and indentation.
	/// </summary>
	sealed class Layouter
	{
		StringBuilder _sb;
		Stack _blockStack;
		int _indent;

		public Layouter()
		{
			_sb = new StringBuilder();
			_blockStack = new Stack();
			_indent = 0;
		}
		/// <summary>
		/// Begin a block.
		/// </summary>
		public void Begin()
		{
			_blockStack.Push(_indent);
			_indent += 4;
		}
		/// <summary>
		/// Ends the innermost block.
		/// </summary>
		public void End()
		{
			_indent = (int)_blockStack.Pop();
		}
		/// <summary>
		/// Appends a formatted string, which contains zero or more format specifications, to this instance. Each format specification is replaced by the string representation of a corresponding object argument.
		/// </summary>
		/// <param name="format">A string containing zero or more format specifications.</param>
		/// <param name="args">An array of objects to format.</param>
		/// <returns>A reference to this instance with format appended. Any format specification in format is replaced by the string representation of the corresponding object argument.</returns>
		public StringBuilder AppendFormat(string format, params object[] args)
		{
			_sb.Append(new string(' ', _indent)); 
			_sb.AppendFormat(format, args);
			return _sb.Append("\n");
		}
		/// <summary>
		/// Appends a copy of the specified string to the end of this instance.
		/// </summary>
		/// <param name="value">The String to append.</param>
		/// <returns>A reference to this instance after the append operation has occurred.</returns>
		public StringBuilder Append(string value)
		{
            _sb.Append(new string(' ', _indent));
            _sb.Append(value);
            return _sb.Append("\n");
		}

		public override string ToString()
		{
			return _sb.ToString();
		}

        public StringBuilder Builder
        {
            get { return _sb; }
        }
	}
}
