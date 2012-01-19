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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LoLNotes.Flash
{
    public class FlashSerializer
    {
        public static FlashObject Deserialize(StreamReader reader)
        {
            var ret = new FlashObject("Base");
            var current = ret;
            var levels = new Stack<int>();
            levels.Push(0);

            while (levels.Count > 0)
            {
                if (reader.Peek() != ' ') //No Space? Well then it must be the end of the object
                    return ret;

                var line = reader.ReadLine();
                var kv = MatchLine(line);
                if (kv == null)
                    throw new NotSupportedException("Unable to parse (" + line + ")");

                while (levels.Count > 0 && GetLevel(line) <= levels.Peek())
                {
                    current = current.Parent;
                    levels.Pop();
                }

                var objname = GetObjectName(line);
                if (objname != null)
                {
                    var tmp = new FlashObject(objname, kv.Key, kv.Value) { Parent = current };
                    current[kv.Key] = tmp;
                    current = tmp;
                    levels.Push(GetLevel(line));
                }
                else
                {
                    if (kv.Value.Length > 0 && kv.Value[0] == '"')
                    {
                        var str = ParseString(kv.Value.Substring(1));
                        if (str != null)
                        {
                            //Singleline quote
                            kv.Value = str;
                        }
                        else
                        {
                            //Multiline quote
                            kv.Value = kv.Value.Substring(1) + ParseString(reader);
                            reader.ReadLine(); //Read the newline after the quote
                        }
                    }
                    current[kv.Key] = new FlashObject(kv.Key, kv.Value);
                }
            }

            return ret;
        }

        static string ParseString(string str)
        {
            var sb = new StringBuilder();

            if (str.Length < 1)
                return str;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '"')
                    return sb.ToString();
                if (str[i] == '\\')
                    sb.Append(DecodeEscaped(str[++i]));
                else
                    sb.Append(str[i]);
            }
            return null;
        }
        static char DecodeEscaped(char c)
        {
            switch (c)
            {
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
            }
            return c;
        }

        static string ParseString(StreamReader reader)
        {
            var sb = new StringBuilder();

            const int maxiters = 100000;
            for(int i = 0; i < maxiters; i++)
            {
                char c = (char)ReadByte(reader);
                if (c == '"')
                    return sb.ToString();
                if (c == '\\')
                    sb.Append(DecodeEscaped((char)ReadByte(reader)));
                else
                    sb.Append(c);
            }
            throw new Exception("String exceeds max size allowed");
        }

        static int ReadByte(StreamReader reader)
        {
            var num = reader.Read();
            if (num == -1)
                throw new EndOfStreamException();
            return num;
        }

        static int GetLevel(string str)
        {
            int num = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ')
                    break;
                num++;
            }
            return num / 2;
        }

        static string GetObjectName(string str)
        {
            var match = Regex.Match(str, "\\((.*)\\)#\\d+$");
            return match.Success ? match.Groups[1].Value : null;
        }

        static KeyValue MatchLine(string str)
        {
            str = str.TrimStart(' ');
            if (str.Length < 1)
                return null;
            //[0] a
            //0 = a
            var match = Regex.Match(str, (str[0] == '[') ? "^\\[?(.+?)]?\\s(.+)$" : "^(.+)?\\s=\\s(.+)?$");
            return match.Success ? new KeyValue(match.Groups[1].Value, match.Groups[2].Value) : null;
        }

        [DebuggerDisplay("{Key}: {Value}")]
        private class KeyValue
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public KeyValue(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
