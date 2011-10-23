using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.IO.Streams;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LoLBans
{

    public class LoLConnection : IDisposable
    {
        Thread RecvThread;
        readonly string PipeName;
        const string LogMatch = @"^\d+/\d+/\d+ \d+:\d+:\d+\.\d+ \[\w+\]";
        const string ObjectMatch = @"\(([^\)]+)\)#\d+$";

        public delegate void ProcessObjectD(FlashObject obj);
        public event ProcessObjectD ProcessObject;
        public delegate void ProcessLineD(string line);
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
