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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace LoLNotes.Util
{
    public static class AppInit
    {
        private const string AppInitDef = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows";
        private const string AppInit32On64 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Windows";

        public static List<string> AppInitDlls32
        {
            get
            {
                return GetAppInitDlls(Wow.Is64BitOperatingSystem ? AppInit32On64 : AppInitDef);
            }
            set
            {
                SetAppInitDlls(Wow.Is64BitOperatingSystem ? AppInit32On64 : AppInitDef, value);
            }
        }

        public static List<string> GetAppInitDlls(string path)
        {
            var reg = Registry.LocalMachine.OpenSubKey(path);
            if (reg == null)
                throw new NullReferenceException("AppInit key null");
            var str = (string)reg.GetValue("AppInit_DLLs");
            if (str == null)
                return new List<string>();
            return str.Split(';', ' ').Select(s => s.Trim()).Where(s => s != "").ToList();
        }

        public static void SetAppInitDlls(string path, List<string> dlls)
        {
            var reg = Registry.LocalMachine.OpenSubKey(path, true);
            if (reg == null)
                throw new NullReferenceException("AppInit key null");
            reg.SetValue("AppInit_DLLs", String.Join("; ", dlls.ToArray()));
            reg.SetValue("LoadAppInit_DLLs", 1);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

        public static string GetShortPath(string path)
        {
            char[] buffer = new char[256];
            int size = GetShortPathName(path, buffer, buffer.Length);
            return new string(buffer, 0, size);
        }
    }
}
