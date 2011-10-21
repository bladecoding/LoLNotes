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