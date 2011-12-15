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

#include "stdafx.h"
#include <Windows.h>
#include <string>
#include <algorithm>
#include "Detours\Detours.h"
#include <vector>
#include <iostream>
#include <boost/interprocess/sync/sharable_lock.hpp>
#include <boost/thread/condition.hpp>
#include <boost/thread/mutex.hpp>
#include <boost/thread/recursive_mutex.hpp>
#include <boost/thread/thread.hpp>
#include <WinSock2.h>

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
const LPTSTR pipename = TEXT("\\\\.\\pipe\\lolnotes");
DWORD timeout = 0;

static BOOL (WINAPI * Trueconnect)(SOCKET s, const struct sockaddr FAR * name, int namelen) = connect;

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

DWORD WINAPI UnloadSelf(LPVOID ptr)
{
	FreeLibraryAndExitThread((HMODULE)ptr, 0);
	return 0;
}

int __stdcall Myconnect(SOCKET s, const struct sockaddr FAR * name, int namelen)
{
	sockaddr_in * in = (sockaddr_in*)name;

	char* ip = inet_ntoa(in->sin_addr);

	WriteString(SelfLogHandle, "Connecting to %s:%d (connect)\n", ip, _byteswap_ushort(in->sin_port));

	/*hostent * host = gethostbyname("prod.na1.lol.riotgames.com");
	DWORD hostip = ((in_addr**)host->h_addr_list)[0]->s_addr;
	if (in->sin_addr.s_addr == hostip)
	{
		in->sin_addr.s_addr = inet_addr("127.0.0.1");
	}*/

	if (in->sin_family == AF_INET && _byteswap_ushort(in->sin_port) == 2099)
	{
		in->sin_addr.s_addr = inet_addr("127.0.0.1");
	}

	return Trueconnect(s, name, namelen);
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
				std::string dir = GetDirectory(GetModuleName(NULL));
				std::string file = dir + "\\lolnotes.log";
				SelfLogHandle = CreateFileA(file.c_str(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, NULL, NULL);

				DetourTransactionBegin();
				DetourUpdateThread(GetCurrentThread());
				DetourAttach(&(PVOID&)Trueconnect, Myconnect);
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
				DetourDetach(&(PVOID&)Trueconnect, Myconnect);
				error = DetourTransactionCommit();
			}

			break;
		}
	}
	return TRUE;
}

