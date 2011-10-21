using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

namespace LoLBans
{
    public static class Wow
    {
        const string AppInitDef = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows";
        const string AppInit32On64 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Windows";
        public static List<string> AppInitDlls32
        {
            get
            {
                return GetAppInitDlls(Is64BitOperatingSystem ? AppInit32On64 : AppInitDef);
            }
            set
            {
                SetAppInitDlls(Is64BitOperatingSystem ? AppInit32On64 : AppInitDef, value);
            }
        }

        public static List<string> GetAppInitDlls(string path)
        {
            var reg = Registry.LocalMachine.OpenSubKey(path);
            if (reg == null)
                throw new NullReferenceException("AppInit key null");
            var str = (string)reg.GetValue("AppInit_DLLs");
            if (str == null)
                throw new NullReferenceException("AppInit dlls null");
            return str.Split(';', ' ').Select(s => s.Trim()).Where(s => s != "").ToList();
        }
        public static void SetAppInitDlls(string path, List<string> dlls)
        {
            var reg = Registry.LocalMachine.OpenSubKey(path, true);
            if (reg == null)
                throw new NullReferenceException("AppInit key null");
            reg.SetValue("AppInit_DLLs", string.Join("; ", dlls.ToArray()));
            reg.SetValue("LoadAppInit_DLLs", 1);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

        public static string GetShortPath(string path)
        {
            char[] buffer = new char[256];
            int size = GetShortPathName(path, buffer, buffer.Length);
            return new string(buffer, 0, size);
        }

        public static bool IsAdministrator
        {
            get
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
                // Clearly if this is a 64-bit process we must be on a 64-bit OS.
                if (Is64BitProcess)
                    return true;
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
                return GetProcAddress(hModule, methodName) != IntPtr.Zero;
            return false;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);
    }
}
