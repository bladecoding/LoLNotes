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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NotMissing.Logging;

namespace LoLNotes.Util
{
	public class SimpleLogTimer : IDisposable
	{
		readonly Stopwatch m_Watch;
		string m_Message;
		Levels m_Level;

		protected SimpleLogTimer(Levels level, string message)
		{
			m_Level = level;
			m_Watch = Stopwatch.StartNew();
			m_Message = message;
		}

		public static SimpleLogTimer Start(Levels level, string message)
		{
			return new SimpleLogTimer(level, message);
		}
		public static SimpleLogTimer Start(string message)
		{
			return Start(Levels.Trace, message);
		}

		#region Dispose

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_Watch.Stop();
				StaticLogger.Log(m_Level, string.Format("[Timing] {0} in {1}ms", m_Message, m_Watch.ElapsedMilliseconds));
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~SimpleLogTimer()
		{
			Dispose(false);
		}

		#endregion
	}
}
