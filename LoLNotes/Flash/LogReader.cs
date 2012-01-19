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
using System.IO;
using System.Text.RegularExpressions;
using NotMissing.Logging;

namespace LoLNotes.Flash
{
    public class LogReader : IDisposable
    {
        const string LogMatch = @"^\d+/\d+/\d+ \d+:\d+:\d+\.\d+ \[\w+\]";
        const string ObjectMatch = @"\(([^\)]+)\)#\d+$";


        private StreamReader Reader { get; set; }

        public LogReader(Stream stream)
            : this(new StreamReader(stream))
        {
        }
        public LogReader(StreamReader reader)
        {
            Reader = reader;
        }

        /// <summary>
        /// Read the next line/object from the stream
        /// </summary>
        /// <returns>Object (FlashObject), Line(string) or Null if the line match failed</returns>
        public object Read()
        {
            var line = Reader.ReadLine();
            if (line == null)
                throw new EndOfStreamException();

            var match = Regex.Match(line, LogMatch);
            if (match.Success)
            {
                try
                {
                    match = Regex.Match(line, ObjectMatch);
                    if (match.Success)
                    {
                        var obj = FlashSerializer.Deserialize(Reader);
                        obj.Name = match.Groups[1].Value;
                        return obj;
                    }
                    return line;
                }
                catch (EndOfStreamException)
                {
                    throw; //Pipe was broken, lets rethrow
                }
                catch (Exception ex)
                {
                    StaticLogger.Error(ex);
                }
            }
            return null;
        }


        protected virtual void Dispose(bool disp)
        {
            if (disp)
            {
                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
        }

        ~LogReader()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
