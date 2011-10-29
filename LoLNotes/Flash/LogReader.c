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

        public LogReader(StreamReader reader)
        {
            Reader = reader;
        }

        /// <summary>
        /// Read the next line/object from the stream
        /// </summary>
        /// <returns>Object (FlashObject), Line(string) or Null</returns>
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
