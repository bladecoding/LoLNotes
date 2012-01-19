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
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NotMissing.Logging;

namespace LoLNotes.Flash
{
    public class PipeProcessor : IFlashProcessor, IDisposable
    {
        Thread RecvThread;
        readonly string PipeName;


        public event ProcessObjectD ProcessObject;
        public event ProcessLineD ProcessLine;
        /// <summary>
        /// Called when the IsConnected status changes.
        /// </summary>
        public event Action<object> Connected;

        bool isconnected;
        public bool IsConnected
        {
            get { return isconnected; }
            protected set
            {
                bool old = isconnected;
                isconnected = value;
                if (old != value && Connected != null)
                    Connected(this);
            }
        }


        public PipeProcessor(string pipename)
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
                        using (var reader = new LogReader(pipe))
                        {
                            pipe.Connect(0);

                            IsConnected = pipe.IsConnected;

                            while (pipe.IsConnected)
                            {
                                var obj = reader.Read();
                                if (obj != null)
                                {
                                    if (obj is FlashObject)
                                        DoProcessObject((FlashObject)obj);
                                    else if (obj is string)
                                        DoProcessLine((string)obj);
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    //Pipe was broken, lets start listening again
                }
                catch (TimeoutException)
                {
                }
                catch (Exception ex)
                {
                    StaticLogger.Error(ex);
                }
                finally
                {
                    IsConnected = false;
                }

                Thread.Sleep(500);
            }
        }

        protected virtual void DoProcessObject(FlashObject obj)
        {
            if (ProcessObject != null)
                ProcessObject(obj);
        }
        protected virtual void DoProcessLine(string str)
        {
            if (ProcessLine != null)
                ProcessLine(str);
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
                ProcessObject = null;
                ProcessLine = null;
                Connected = null;
            }
        }
        ~PipeProcessor()
        {
            Dispose(false);
        }
        #endregion
    }
}
