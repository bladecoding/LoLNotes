using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Streams;
using System.Text;
using System.Text.RegularExpressions;

namespace LoLBans
{
    public class FlashSerializer
    {
        public static FlashObject Deserialize(Stream stream)
        {
            var ret = new FlashObject("Base");
            var current = ret;
            var levels = new Stack<int>();

            do
            {
                var line = stream.ReadLine();
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
                            kv.Value = kv.Value.Substring(1) + ParseString(stream);
                            stream.ReadLine(); //Read the newline after the quote
                        }
                    }
                    current[kv.Key] = new FlashObject(kv.Key, kv.Value);
                }
            }
            while (current != ret);

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

        static string ParseString(Stream stream)
        {
            var sb = new StringBuilder();

            const int maxiters = 100000;
            for(int i = 0; i < maxiters; i++)
            {
                char c = (char)stream.ReadInt8();
                if (c == '"')
                    return sb.ToString();
                if (c == '\\')
                    sb.Append(DecodeEscaped((char)stream.ReadInt8()));
                else
                    sb.Append(c);
            }
            throw new Exception("String exceeds max size allowed");
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
            var match = Regex.Match(str, (str[0] == '[') ? "^\\[?(.+?)]?\\s(.+)$" : "^(.+)\\s=?\\s(.+)$");
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
