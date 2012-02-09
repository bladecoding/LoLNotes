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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace LoLNotes.Util
{
	public class ProcessMemory : IDisposable
	{
		public int ProcessId { get; protected set; }
		public IntPtr Handle { get; protected set; }


		public ProcessMemory(int id)
		{
			ProcessId = id;
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
		public Int32 GetAddress(string modname, string name)
		{
			return GetAddress(GetModule(modname), name);
		}

		public bool Is64Bit()
		{
			if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major > 5)
			{
				if (Wow.Is64BitOperatingSystem)
				{
					bool ret;
					if (!IsWow64Process(Handle, out ret))
						throw new Win32Exception();
					//IsWow64Process only checks if its a 32 bit process on a x64 machine.
					//Will return false if its a 64 bit process or if its a 32bit on x84.
					return !ret;
				}
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
			if (Handle != IntPtr.Zero)
			{
				CloseHandle(Handle);
				Handle = IntPtr.Zero;
			}
		}


		const uint PROCESS_ALL_ACCESS = 0x1FFFFF;
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, Int32 dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CloseHandle(IntPtr hObject);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		static extern bool CloseHandle(HandleRef handle);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, Int32 size, out IntPtr lpNumberOfBytesRead);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
		[Flags]
		enum AllocationType
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
		enum MemoryProtection
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
		static extern Int32 GetProcAddress(Int32 hModule, string procedureName);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "LoadLibraryA")]
		static extern Int32 LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		static extern bool IsWow64Process(IntPtr processHandle, out bool wow64Process);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		extern static IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		extern static bool DuplicateHandle([In] IntPtr hSourceProcessHandle, [In] IntPtr hSourceHandle, [In] IntPtr hTargetProcessHandle, [In, Out] ref IntPtr lpTargetHandle, [In] uint dwDesiredAccess, [In] bool bInheritHandle, [In] uint dwOptions);

		[DllImport("kernel32.dll")]
		static extern Int32 GetModuleHandle(string lpModuleName);

		public static Int32 GetModule(string modname)
		{
			var ret = GetModuleHandle(modname);
			if (ret == 0)
				throw new Win32Exception();
			return ret;
		}

		public void DuplicateMutex(Mutex mutex)
		{
			var targethandle = IntPtr.Zero;
			if (!DuplicateHandle(GetCurrentProcess(), mutex.SafeWaitHandle.DangerousGetHandle(), Handle, ref targethandle, 0, false, 2))
				throw new Win32Exception();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern ToolHelpHandle CreateToolhelp32Snapshot(int flags, int processId);

		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct MODULEENTRY32
		{
			public uint dwSize;
			public uint th32ModuleID;
			public uint th32ProcessID;
			public uint GlblcntUsage;
			public uint ProccntUsage;
			public IntPtr modBaseAddr;
			public uint modBaseSize;
			public IntPtr hModule;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string szModule;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExePath;
		};

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool Module32First(ToolHelpHandle hSnapshot, ref MODULEENTRY32 lpme);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool Module32Next(ToolHelpHandle hSnapshot, ref MODULEENTRY32 lpme);

		public class ModuleInfo
		{
			public string baseName;
			public IntPtr baseOfDll;
			public IntPtr entryPoint;
			public string fileName;
			public int Id;
			public int sizeOfImage;
		}
		public class ToolHelpHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			private ToolHelpHandle()
				: base(true)
			{
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			protected override bool ReleaseHandle()
			{
				return CloseHandle(handle);
			}
		}

		public IEnumerable<ModuleInfo> GetModuleInfos()
		{
			ToolHelpHandle ptr = null;
			var list = new List<ModuleInfo>();
			try
			{
				ptr = CreateToolhelp32Snapshot(0x8, ProcessId);
				if (ptr.IsInvalid)
				{
					throw new Win32Exception();
				}
				var me32 = new MODULEENTRY32();
				me32.dwSize = (uint)Marshal.SizeOf(me32);

				if (Module32First(ptr, ref me32))
				{
					do
					{
						ModuleInfo info = new ModuleInfo
						{
							baseName = me32.szModule,
							fileName = me32.szExePath,
							baseOfDll = me32.modBaseAddr,
							sizeOfImage = (int)me32.modBaseSize,
							Id = (int)me32.th32ModuleID
						};
						list.Add(info);
						me32.dwSize = (uint)Marshal.SizeOf(me32);
					}
					while (Module32Next(ptr, ref me32));
				}
				if (Marshal.GetLastWin32Error() != 18) //ERROR_NO_MORE_FILES
					throw new Win32Exception();
			}
			finally
			{
				if (ptr != null && !ptr.IsInvalid)
				{
					ptr.Close();
				}
			}
			return list;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MEMORY_BASIC_INFORMATION
		{
			public IntPtr BaseAddress;
			public IntPtr AllocationBase;
			public uint AllocationProtect;
			public UIntPtr RegionSize;
			public uint State;
			public uint Protect;
			public uint Type;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern Int32 VirtualQueryEx(IntPtr handle, Int32 address, ref MEMORY_BASIC_INFORMATION buffer, Int32 sizeOfBuffer);

		public MEMORY_BASIC_INFORMATION VirtualQuery(Int32 address)
		{
			var info = new MEMORY_BASIC_INFORMATION();
			var ret = VirtualQueryEx(Handle, address, ref info, Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
			if (ret == 0)
				throw new Win32Exception();
			return info;
		}

		public static class MemoryState
		{
			public static uint Commit = 0x1000;
			public static uint Free = 0x10000;
			public static uint Reserve = 0x2000;
		}

	}
}
