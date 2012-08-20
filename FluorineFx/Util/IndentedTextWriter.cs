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
using System.IO;
using System.Text;

namespace FluorineFx.Util
{
    /// <summary>
    /// Drop-in replacement for <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
    /// that does not require a full-trust link and inheritance demand.
    /// Provides a text writer that can indent new lines by a tab string token.
    /// </summary>
    public sealed class IndentedTextWriter : TextWriter
    {
        private TextWriter _writer;
        private int _level;
        private bool _tabsPending;
        private string _tab;
        /// <summary>
        /// Specifies the default tab string. This field is constant.
        /// </summary>
        public const string DefaultTabString = "\x20\x20\x20\x20";

        /// <summary>
        /// Initializes a new instance of the IndentedTextWriter class using the specified text writer and default tab string.
        /// </summary>
        /// <param name="writer">The TextWriter to use for output.</param>
        public IndentedTextWriter(TextWriter writer)
            : this(writer, DefaultTabString)
        {
        }
        /// <summary>
        /// Initializes a new instance of the IndentedTextWriter class using the specified text writer and tab string.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tabString"></param>
        public IndentedTextWriter(TextWriter writer, string tabString)
            : base(CultureInfo.InvariantCulture)
        {
            _writer = writer;
            _tab = tabString;
            _level = 0;
            _tabsPending = false;
        }
        /// <summary>
        /// Gets the encoding for the text writer to use.
        /// </summary>
        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }
        /// <summary>
        /// Gets or sets the new line character to use.
        /// </summary>
        public override string NewLine
        {
            get { return _writer.NewLine; }

            set { _writer.NewLine = value; }
        }
        /// <summary>
        /// Gets or sets the number of spaces to indent.
        /// </summary>
        public int Indent
        {
            get { return _level; }
            set { _level = value < 0 ? 0 : value; }
        }
        /// <summary>
        /// Gets the TextWriter to use.
        /// </summary>
        public TextWriter InnerWriter
        {
            get { return _writer; }
        }

        internal string TabString
        {
            get { return _tab; }
        }
        /// <summary>
        /// Closes the document being written to.
        /// </summary>
        public override void Close()
        {
            _writer.Close();
        }
        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }
        /// <summary>
        ///  Writes the specified string to the text stream.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public override void Write(string s)
        {
            WritePendingTabs();
            _writer.Write(s);
        }
        /// <summary>
        /// Writes the text representation of a Boolean value to the text stream.
        /// </summary>
        /// <param name="value">The Boolean value to write.</param>
        public override void Write(bool value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void Write(char value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes a character array to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write.</param>
        public override void Write(char[] buffer)
        {
            WritePendingTabs();
            _writer.Write(buffer);
        }
        /// <summary>
        /// Writes a subarray of characters to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">Starting index in the buffer.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void Write(char[] buffer, int index, int count)
        {
            WritePendingTabs();
            _writer.Write(buffer, index, count);
        }
        /// <summary>
        /// Writes the text representation of a Double to the text stream.
        /// </summary>
        /// <param name="value">The double to write.</param>
        public override void Write(double value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes the text representation of a Single to the text stream.
        /// </summary>
        /// <param name="value">The single to write.</param>
        public override void Write(float value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes the text representation of an integer to the text stream.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        public override void Write(int value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes the text representation of an 8-byte integer to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte integer to write.</param>
        public override void Write(long value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes the text representation of an object to the text stream.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public override void Write(object value)
        {
            WritePendingTabs();
            _writer.Write(value);
        }
        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">The object to write into the formatted string.</param>
        public override void Write(string format, object arg0)
        {
            WritePendingTabs();
            _writer.Write(format, arg0);
        }
        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string to use.</param>
        /// <param name="arg0">The first object to write into the formatted string.</param>
        /// <param name="arg1">The second object to write into the formatted string.</param>
        public override void Write(string format, object arg0, object arg1)
        {
            WritePendingTabs();
            _writer.Write(format, arg0, arg1);
        }
        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string to use.</param>
        /// <param name="arg">The argument array to output.</param>
        public override void Write(string format, params object[] arg)
        {
            WritePendingTabs();
            _writer.Write(format, arg);
        }
        /// <summary>
        /// Writes the specified string to a line without tabs.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public void WriteLineNoTabs(string s)
        {
            _writer.WriteLine(s);
        }
        /// <summary>
        /// Writes the specified string, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public override void WriteLine(string s)
        {
            WritePendingTabs();
            _writer.WriteLine(s);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes a line terminator.
        /// </summary>
        public override void WriteLine()
        {
            WritePendingTabs();
            _writer.WriteLine();
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of a Boolean, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The Boolean to write.</param>
        public override void WriteLine(bool value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes a character, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void WriteLine(char value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes a character array, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write.</param>
        public override void WriteLine(char[] buffer)
        {
            WritePendingTabs();
            _writer.WriteLine(buffer);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes a subarray of characters, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">Starting index in the buffer.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteLine(char[] buffer, int index, int count)
        {
            WritePendingTabs();
            _writer.WriteLine(buffer, index, count);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of a Double, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The double to write.</param>
        public override void WriteLine(double value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of a Single, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The single to write.</param>
        public override void WriteLine(float value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of an integer, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        public override void WriteLine(int value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of an 8-byte integer, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte integer to write.</param>
        public override void WriteLine(long value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes the text representation of an object, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public override void WriteLine(object value)
        {
            WritePendingTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">The object to write into the formatted string.</param>
        public override void WriteLine(string format, object arg0)
        {
            WritePendingTabs();
            _writer.WriteLine(format, arg0);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string to use.</param>
        /// <param name="arg0">The first object to write into the formatted string.</param>
        /// <param name="arg1">The second object to write into the formatted string.</param>
        public override void WriteLine(string format, object arg0, object arg1)
        {
            WritePendingTabs();
            _writer.WriteLine(format, arg0, arg1);
            _tabsPending = true;
        }
        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format">The formatting string to use.</param>
        /// <param name="arg">The argument array to output.</param>
        public override void WriteLine(string format, params object[] arg)
        {
            WritePendingTabs();
            _writer.WriteLine(format, arg);
            _tabsPending = true;
        }

        private void WritePendingTabs()
        {
            if (!_tabsPending)
                return;

            _tabsPending = false;

            for (int i = 0; i < _level; i++)
                _writer.Write(_tab);
        }
    }
}
