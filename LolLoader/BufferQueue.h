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

#pragma once

#include <windows.h>
#include <deque>
#include <boost/interprocess/sync/sharable_lock.hpp>
#include <boost/thread/condition.hpp>
#include <boost/thread/mutex.hpp>
#include <boost/thread/recursive_mutex.hpp>
#include <boost/thread/thread.hpp>

class Buffer
{
public:
	boost::shared_ptr<char> Data;
	int Size;
	Buffer()
	{
		Size = 0;
	}
	Buffer(char* ptr, int size)
	{
		Data.reset(new char[size]);
		memcpy(Data.get(), ptr, size);
		Size = size;
	}
};

class BufferQueue
{
public:

	std::deque<Buffer> Data;
	int MaxSize;
	BufferQueue(int maxsize)
	{
		MaxSize = maxsize;
	}

	DWORD Size()
	{
		DWORD ret = 0; 
		for (std::deque<Buffer>::iterator i = Data.begin(); i != Data.end(); i++)
		{
			ret += i->Size;
		}
		return ret;
	}

	bool PushBuffer(char* ptr, int size)
	{
		if (size > MaxSize)
			return false;
		if (Size() + size > MaxSize)
			return false;
		Data.push_back(Buffer(ptr, size));
		return true;
	}
	bool ForcePushBuffer(char* ptr, int size)
	{
		if (size > MaxSize)
			return false;
		while (Size() + size > MaxSize)
			Data.pop_front();
		Data.push_back(Buffer(ptr, size));
		return true;
	}
	Buffer PopBuffer()
	{
		if (Data.size() < 1)
			return Buffer();
		Buffer ret = Data.front();
		Data.pop_front();
		return ret;
	}

};