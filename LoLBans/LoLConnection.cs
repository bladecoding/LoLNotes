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
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.Streams;
using System.Text.RegularExpressions;
using System.Threading;

namespace LoLNotes
{
    public class LoLConnection : IFlashProcessor, IDisposable
    {
        Thread RecvThread;
        readonly string PipeName;
        const string LogMatch = @"^\d+/\d+/\d+ \d+:\d+:\d+\.\d+ \[\w+\]";
        const string ObjectMatch = @"\(([^\)]+)\)#\d+$";

        public event ProcessObjectD ProcessObject;
        public event ProcessLineD ProcessLine; 

        public LoLConnection(string pipename)
        {
            PipeName = pipename;
        }

        public virtual void Start()
        {
            if (RecvThread == null)
            {
                RecvThread = new Thread(RecvLoop) { IsBackground = true };
                RecvThread.Start();
            }
        }

        public virtual void Stop()
        {
            RecvThread = null;
        }

        protected virtual void RecvLoop()
        {
            while (RecvThread != null)
            {
                try
                {
                    using (var pipe = new NamedPipeClientStream(PipeName))
                    {
                        pipe.Connect();

                        while (pipe.IsConnected)
                        {
                            var line = pipe.ReadLine();
                            var match = Regex.Match(line, LogMatch);
                            if (match.Success)
                            {
                                try
                                {
                                    match = Regex.Match(line, ObjectMatch);
                                    if (match.Success)
                                    {
                                        var obj = FlashSerializer.Deserialize(pipe);
                                        obj.Name = match.Groups[1].Value;
                                        DoProcessObject(obj);
                                    }
                                    else
                                    {
                                        DoProcessLine(line);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //TODO: Implement logging
                                    Debug.WriteLine(ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Implement logging
                    Debug.WriteLine(ex);
                }
            }
        }

        protected virtual void DoProcessObject(FlashObject obj)
        {
            if (ProcessObject != null)
                ThreadPool.QueueUserWorkItem(state => ProcessObject(obj));
        }
        protected virtual void DoProcessLine(string str)
        {
            if (ProcessLine != null)
                ThreadPool.QueueUserWorkItem(state => ProcessLine(str));
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                RecvThread = null;
            }
        }
        ~LoLConnection()
        {
            Dispose(false);
        }
        #endregion
    }
}
