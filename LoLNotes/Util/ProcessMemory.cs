/*
copyright (C) 2011 by high828@gmail.com

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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LoLNotes.Util
{
	public class ProcessMemory : IDisposable
	{
		public IntPtr Handle { get; protected set; }


		public ProcessMemory(int id)
		{
			Handle = OpenProcess(PROCESS_ALL_ACCESS, 0, id);
			if (Handle == IntPtr.Zero)
				throw new Win32Exception();
		}

		public Int32 Alloc(int len)
		{
			Int32 ret = VirtualAllocEx(Handle, 0, len, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
			if (ret == 0)
				throw new Win32Exception();
			return ret;
		}

		public void Write(Int32 addr, byte[] bytes)
		{
			IntPtr t;
			if (!WriteProcessMemory(Handle, (IntPtr)addr, bytes, bytes.Length, out t))
				throw new Win32Exception();
		}
		public byte[] Read(Int32 addr, int len)
		{
			var ret = new byte[len];
			IntPtr t;
			if (!ReadProcessMemory(Handle, (IntPtr)addr, ret, ret.Length, out t))
				throw new Win32Exception();
			return ret;
		}

		public Int32 LoadModule(string name)
		{
			int ret = LoadLibrary(name);
			if (ret == 0)
				throw new Win32Exception();
			return ret;
		}

		public Int32 GetAddress(Int32 mod, string name)
		{
			Int32 ret = GetProcAddress(mod, name);
			if (ret == 0)
				throw new Win32Exception();
			return ret;
		}

		public bool Is64Bit()
		{
			if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major > 5)
			{
				bool ret;
				if (!IsWow64Process(Handle, out ret))
					throw new Win32Exception();
				//IsWow64Process only checks if its a 32 bit process on a x64 machine.
				//Will return false if its a 64 bit process or if its a 32bit on x84.
				return !Wow.Is64BitOperatingSystem || !ret;
			}
			return false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~ProcessMemory()
		{
			Dispose(false);
		}
		protected virtual void Dispose(bool dispose)
		{
			if (dispose)
			{
				if (Handle != IntPtr.Zero)
				{
					CloseHandle(Handle);
					Handle = IntPtr.Zero;
				}
			}
		}


		public const uint PROCESS_ALL_ACCESS = 0x1FFFFF;
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, Int32 dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern Int32 CloseHandle(IntPtr hObject);
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, Int32 size, out IntPtr lpNumberOfBytesRead);
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
		[Flags]
		public enum AllocationType
		{
			Commit = 0x1000,
			Reserve = 0x2000,
			Decommit = 0x4000,
			Release = 0x8000,
			Reset = 0x80000,
			Physical = 0x400000,
			TopDown = 0x100000,
			WriteWatch = 0x200000,
			LargePages = 0x20000000
		}

		[Flags]
		public enum MemoryProtection
		{
			Execute = 0x10,
			ExecuteRead = 0x20,
			ExecuteReadWrite = 0x40,
			ExecuteWriteCopy = 0x80,
			NoAccess = 0x01,
			ReadOnly = 0x02,
			ReadWrite = 0x04,
			WriteCopy = 0x08,
			GuardModifierflag = 0x100,
			NoCacheModifierflag = 0x200,
			WriteCombineModifierflag = 0x400
		}
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern Int32 VirtualAllocEx(IntPtr hProcess, Int32 lpAddress, Int32 dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern Int32 GetProcAddress(Int32 hModule, string procedureName);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "LoadLibraryA")]
		public static extern Int32 LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool IsWow64Process(IntPtr processHandle, out bool wow64Process);
	}
}
