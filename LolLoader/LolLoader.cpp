// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include <string>
#include <algorithm>
#include "Detours\Detours.h"
#include <vector>
#include <iostream>
#include <WinSock2.h>
#include <boost/interprocess/sync/sharable_lock.hpp>
#include <boost/thread/condition.hpp>
#include <boost/thread/mutex.hpp>
#include <boost/thread/recursive_mutex.hpp>
#include <boost/thread/thread.hpp>
#include "BufferQueue.h"

bool hasEnding (std::string const &fullString, std::string const &ending)
{
    if (fullString.length() >= ending.length()) {
        return (0 == fullString.compare (fullString.length() - ending.length(), ending.length(), ending));
    } else {
        return false;
    }
}
void lower(std::string &data)
{
	std::transform(data.begin(), data.end(),
	data.begin(), ::tolower);
}

bool islol()
{
	CHAR buffer[1000];
	DWORD length = GetModuleFileNameA(NULL, buffer, 1000);
	std::string str(buffer, length);
	lower(str);

	return hasEnding(str, "lolclient.exe");
}

std::wstring StringToWString(const std::string& s)
{
	std::wstring temp(s.length(),L' ');
	std::copy(s.begin(), s.end(), temp.begin());
	return temp;
}


std::string WStringToString(const std::wstring& s)
{
	std::string temp(s.length(), ' ');
	std::copy(s.begin(), s.end(), temp.begin());
	return temp;
}


bool IsLoL = false;
HANDLE LogHandle = 0;
HANDLE SelfLogHandle = 0;
boost::condition cond;
boost::mutex mutex;
const int buffermax = 0x500000;
const int datamax = 0xA00000;
const LPTSTR pipename = TEXT("\\\\.\\pipe\\lolbans");
DWORD timeout = 0;
BufferQueue Buffers(buffermax);

static HANDLE (WINAPI * TrueCreateFileW)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) = CreateFileW;
static BOOL (WINAPI * TrueWriteFile)(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped) = WriteFile;

std::string format(const char *fmt, ...) 
{ 
   using std::string;
   using std::vector;

   string retStr("");

   if (NULL != fmt)
   {
      va_list marker = NULL; 

      // initialize variable arguments
      va_start(marker, fmt); 
      
      // Get formatted string length adding one for NULL
      size_t len = _vscprintf(fmt, marker) + 1;
               
      // Create a char vector to hold the formatted string.
      vector<char> buffer(len, '\0');
      int nWritten = _vsnprintf_s(&buffer[0], buffer.size(), len, fmt, marker);    

      if (nWritten > 0)
      {
         retStr = &buffer[0];
      }
            
      // Reset variable arguments
      va_end(marker); 
   }

   return retStr; 
}
static BOOL WriteString(HANDLE file, char* fmt, ...)
{
	if (file == INVALID_HANDLE_VALUE)
		return false;
	using std::string;
	using std::vector;

	string retStr("");

	if (NULL != fmt)
	{
		va_list marker = NULL; 

		// initialize variable arguments
		va_start(marker, fmt); 
      
		// Get formatted string length adding one for NULL
		size_t len = _vscprintf(fmt, marker) + 1;
               
		// Create a char vector to hold the formatted string.
		vector<char> buffer(len, '\0');
		int nWritten = _vsnprintf_s(&buffer[0], buffer.size(), len, fmt, marker);    

		if (nWritten > 0)
		{
			retStr = &buffer[0];
		}
            
		// Reset variable arguments
		va_end(marker); 
	}

	DWORD written;
	return WriteFile(file, (LPCVOID)retStr.c_str(), retStr.length(), &written, NULL);
}
static std::string GetDirectory(const std::string &data)
{
	std::string str = data;
	for(int i = 0; i < str.length(); i++)
	{
		if (str[i] == '/')
			str[i] = '\\';
	}

	int find = str.find_last_of('\\');
	if (find != -1)
		str = str.substr(0, find);

	return str;
}

static std::string GetModuleName(HMODULE module)
{
	char buffer[1000];
	DWORD len = GetModuleFileNameA(module, buffer, 1000);
	return std::string(buffer, len);
}

DWORD WINAPI InternalClientLoop(LPVOID ptr)
{
	while (true)
	{
		WriteString(SelfLogHandle, "[LoLBans] Creating pipe\n");
		HANDLE server = CreateNamedPipe( 
			pipename,				  // pipe name 
			PIPE_ACCESS_DUPLEX,       // read access 
			PIPE_TYPE_BYTE |       // message type pipe 
			PIPE_READMODE_BYTE |   // message-read mode 
			PIPE_WAIT,                // blocking mode 
			PIPE_UNLIMITED_INSTANCES, // max. instances  
			datamax,                  // output buffer size 
			datamax,                  // input buffer size 
			0,                        // client time-out 
			NULL);                    // default security attribute 
		if (server == INVALID_HANDLE_VALUE)
		{
			WriteString(SelfLogHandle, "[LoLBans] Failed to create pipe (%ld)\n", GetLastError());
			return -1;
		}

		WriteString(SelfLogHandle, "[LoLBans] Accepting on %ld\n", server);

		if (!(ConnectNamedPipe(server, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED)))
		{
			WriteString(SelfLogHandle, "[LoLBans] Failed to accept client (%ld)\n", GetLastError());
			return -1;
		}

		WriteString(SelfLogHandle, "[LoLBans] Accepted\n");

		while (true)
		{
			{
				boost::mutex::scoped_lock lk(mutex);
				while (Buffers.Size() < 1)
					cond.wait(lk);
			}

			{
				boost::mutex::scoped_lock lk(mutex);

				Buffer buf = Buffers.PopBuffer();
				if (buf.Size > 0)
				{
					DWORD written;
					if (!WriteFile(server, buf.Data.get(), buf.Size, &written, NULL))
					{
						WriteString(SelfLogHandle, "[LoLBans] Failed to send to client (%ld)\n", WSAGetLastError());
						break;
					}
					WriteString(SelfLogHandle, "[LoLBans] Sent %d bytes\n", buf.Size);
					FlushFileBuffers(server);
				}
			}
		}

		CloseHandle(server);
	}
	return 0;
}

DWORD WINAPI ClientLoop(LPVOID ptr)
{
	WriteString(SelfLogHandle, "[LoLBans] Pipe loop created\n");
	DWORD ret = InternalClientLoop(ptr);
	WriteString(SelfLogHandle, "[LoLBans] Pipe loop ended\n");
	return ret;
}

HANDLE WINAPI MyCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
{
	HANDLE ret = TrueCreateFileW(lpFileName, dwDesiredAccess, dwShareMode | FILE_SHARE_READ, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

	if (!LogHandle)
	{
		std::wstring wfile(lpFileName);
		std::string file = WStringToString(wfile);

		lower(file);
		if (hasEnding(file, ".log") && file.find("lolclient") != -1)
		{
			if (!SelfLogHandle)
			{
				SelfLogHandle = CreateFileA("lolbans.log", GENERIC_ALL, FILE_SHARE_READ, NULL, NULL, NULL, NULL);
			}
			if (!CreateThread(NULL, NULL, ClientLoop, NULL, NULL, NULL))
			{
				WriteString(SelfLogHandle, "[LoLBans] Failed to create server thread (%ld)\n", WSAGetLastError());
			}
			else
			{
				WriteString(SelfLogHandle, "[LoLBans] Started\n");
				LogHandle = ret;
			}
		}
	}
    return ret;
}

BOOL WINAPI MyWriteFile(HANDLE hFile, LPCVOID lpBuffer, DWORD nNumberOfBytesToWrite, LPDWORD lpNumberOfBytesWritten, LPOVERLAPPED lpOverlapped)
{
	if (LogHandle && hFile == LogHandle)
	{
		{
			boost::mutex::scoped_lock lk(mutex);
			if (nNumberOfBytesToWrite > buffermax)
			{
				WriteString(SelfLogHandle, "[LoLBans] Buffer exceeds %d", buffermax);
			}
			else
			{
				Buffers.ForcePushBuffer((char*)lpBuffer, nNumberOfBytesToWrite);
				cond.notify_all();
			}
		}
	}

    return TrueWriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
}

DWORD WINAPI UnloadSelf(LPVOID ptr)
{
	FreeLibraryAndExitThread((HMODULE)ptr, 0);
	return 0;
}

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	LONG error;
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
		{
			IsLoL = islol();

			if (IsLoL)
			{
				DetourTransactionBegin();
				DetourUpdateThread(GetCurrentThread());
				DetourAttach(&(PVOID&)TrueCreateFileW, MyCreateFileW);
				error = DetourTransactionCommit();

				DetourTransactionBegin();
				DetourUpdateThread(GetCurrentThread());
				DetourAttach(&(PVOID&)TrueWriteFile, MyWriteFile);
				error = DetourTransactionCommit();
			}
			else
			{
				CreateThread(NULL, NULL, UnloadSelf, hModule, NULL, NULL);
			}

			break;
		}
		case DLL_PROCESS_DETACH:
		{
			if (IsLoL)
			{
				DetourTransactionBegin();
				DetourUpdateThread(GetCurrentThread());
				DetourDetach(&(PVOID&)TrueCreateFileW, MyCreateFileW);
				error = DetourTransactionCommit();

				DetourTransactionBegin();
				DetourUpdateThread(GetCurrentThread());
				DetourDetach(&(PVOID&)TrueWriteFile, MyWriteFile);
				error = DetourTransactionCommit();
			}

			break;
		}
	}
	return TRUE;
}

