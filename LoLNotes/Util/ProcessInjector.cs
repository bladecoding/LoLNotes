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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NotMissing.Logging;

namespace LoLNotes.Util
{
	public class ProcessInjector : IDisposable
	{
		readonly byte[] connectcc = new byte[]
		{
			0x55,										//PUSH EBP
			0x8B, 0xEC,									//MOV EBP, ESP
			0x60, 										//PUSHAD
			0x8B, 0x45, 0x0C, 							//MOV EAX, [EBP+C]
			0x66, 0x83, 0x38, 0x02, 					//CMP WORD PTR [EAX], 2
			0x75, 0x12, 								//JNZ SHORT 12h
			0xB9, 0x08, 0x33, 0x00, 0x00,				//MOV ECX, 3308
			0x66, 0x39, 0x48, 0x02,						//CMP [EAX+2], CX
			0x75, 0x07,									//JNZ SHORT 7h
			0xC7, 0x40, 0x04, 0x7F, 0x00, 0x00, 0x01,	//MOV DWORD PTR [EAX+4], 100007Fh
			0x61,										//POPAD
			0xE9, 0x00, 0x00, 0x00, 0x00				//JMP LONG <X>	
		};

		readonly byte[] safecheck = new byte[]
		{
			0x8B, 0xFF,									//MOVE EDI, EDI
			0x55,										//PUSH EBP
			0x8B, 0xEC									//MOV EBP, ESP	
		};

		public string ProcessName { get; protected set; }
		public Thread CheckThread { get; protected set; }
		public Process CurrentProcess { get; protected set; }

		/// <summary>
		/// Called when the IsInjected status changes.
		/// </summary>
		public event EventHandler Injected;

		bool isinjected;
		public bool IsInjected
		{
			get { return isinjected; }
			protected set
			{
				if (isinjected != value)
				{
					isinjected = value;
					if (Injected != null)
						Injected(this, new EventArgs());
				}
			}
		}

		GetModuleFrom From { get; set; }

		public ProcessInjector(string process)
		{
			ProcessName = process;
			CheckThread = new Thread(CheckLoop) { IsBackground = true };
			From = GetModuleFrom.Toolhelp32Snapshot;
		}

		public void Start()
		{
			if (!CheckThread.IsAlive)
				CheckThread.Start();
		}

		/// <summary>
		/// Clears the current process. Used for refreshing if changing 'this.From'.
		/// </summary>
		public void Clear()
		{
			CurrentProcess = null;
		}

		protected void CheckLoop()
		{
			while (CheckThread != null)
			{
				if (CurrentProcess == null || CurrentProcess.HasExited)
				{
					IsInjected = false;
					CurrentProcess = Process.GetProcessesByName(ProcessName).FirstOrDefault();
					if (CurrentProcess != null)
					{
						try
						{
							Inject();
							IsInjected = true;
						}
						catch (FileNotFoundException fe)
						{
							//LoLClient does not have ws2_32 yet. Lets try again in 1 second.
							StaticLogger.Trace(fe.Message);
							CurrentProcess = null;
							Thread.Sleep(1000);
							continue;
						}
						catch (WarningException we)
						{
							IsInjected = true;
							StaticLogger.Info(we.Message);
						}
						catch (NotSupportedException nse)
						{
							StaticLogger.Warning(nse);
						}
						catch (Exception ex)
						{
							StaticLogger.Error(new Exception(string.Format("{0} [{1}]", ex.Message, From), ex));
						}
					}
				}
				Thread.Sleep(500);
			}
		}

		void Inject()
		{
			using (var mem = new ProcessMemory(CurrentProcess.Id))
			{
				using (var notemem = new ProcessMemory(Process.GetCurrentProcess().Id))
				{
					if (mem.Is64Bit())
						throw new NotSupportedException("lolclient is running in 64bit mode which is not supported");

					var connect = new byte[connectcc.Length];
					connectcc.CopyTo(connect, 0);
					int jmpaddrloc = connect.Length - 4;

					var mod = ProcessMemory.GetModule("ws2_32.dll");
					Int32 reladdr = notemem.GetAddress(mod, "connect");
					reladdr -= mod;

					var lolmod = GetModuleAddress(CurrentProcess, mem, "ws2_32.dll");
					if (lolmod == 0)
					{
						throw new FileNotFoundException("Lolclient has not yet loaded ws2_32.dll");
					}
					Int32 connectaddr = lolmod + reladdr;

					var bytes = mem.Read(connectaddr, 5);
					if (bytes[0] == 0xe9)
					{
						throw new WarningException("Connect already redirected");
					}
					if (!bytes.SequenceEqual(safecheck))
					{
						bytes = mem.Read(connectaddr, 20);
						throw new AccessViolationException(string.Format("Connect has unknown bytes [{0},{1}]", Convert.ToBase64String(bytes), From));
					}

					Int32 addr = mem.Alloc(connectcc.Length);
					BitConverter.GetBytes((connectaddr + 5) - (addr + connect.Length)).CopyTo(connect, jmpaddrloc);
					mem.Write(addr, connect);

					var jmp = new byte[5];
					jmp[0] = 0xE9;
					BitConverter.GetBytes(addr - (connectaddr + 5)).CopyTo(jmp, 1);
					mem.Write(connectaddr, jmp);
				}
			}
		}

		Int32 GetModuleAddress(Process curproc, ProcessMemory curmem, string name)
		{
			if (From == GetModuleFrom.ProcessClass)
			{
				var mod = GetModule(curproc.Modules, name);
				if (mod == null)
					return 0;
				return mod.BaseAddress.ToInt32();
			}
			if (From == GetModuleFrom.Mirroring)
			{
				var mod = ProcessMemory.GetModule("ws2_32.dll");
				var info = curmem.VirtualQuery(mod);
				return info.State != ProcessMemory.MemoryState.Free ? mod : 0;
			}
			if (From == GetModuleFrom.Toolhelp32Snapshot)
			{
				var mods = curmem.GetModuleInfos();
				var mod = mods.FirstOrDefault(mi => mi.baseName.ToLowerInvariant() == name);
				if (mod == null)
					return 0;
				return mod.baseOfDll.ToInt32();
			}
			return -1;
		}

		ProcessModule GetModule(ProcessModuleCollection mods, string name)
		{
			name = name.ToLower();
			foreach (ProcessModule mod in mods)
			{
				if (mod.ModuleName.ToLower() == name)
					return mod;
			}
			return null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~ProcessInjector()
		{
			Dispose(false);
		}
		protected virtual void Dispose(bool dispose)
		{
			if (dispose)
			{
				CheckThread = null;
			}
		}

		public enum GetModuleFrom
		{
			Toolhelp32Snapshot,
			ProcessClass,
			Mirroring
		}
	}
}
