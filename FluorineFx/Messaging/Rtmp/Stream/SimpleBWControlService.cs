/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections;
using System.Threading;
using log4net;
using FluorineFx.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Configuration;

namespace FluorineFx.Messaging.Rtmp.Stream
{
    /// <summary>
    /// 	<para>A simple implementation of bandwidth controller. The initial burst, if not
    ///     specified by user, is half of the property "defaultCapacity".</para>
    /// 	<para>Following is the reference information for the future optimization on
    ///     threading.</para>
    /// 	<para>The threads that may access this object concurrently are:</para>
    /// 	<para>Thread A that makes token request.</para>
    /// 	<para>Thread B that makes token request.</para>
    /// 	<para>Thread C that distributes tokens and call the callbacks. (Timer)</para>
    /// 	<para>Thread D that updates the bw config of a controllable.</para>
    /// 	<para>Thread E that resets a bucket.</para>
    /// 	<para>Thread F that unregisters a controllable.</para>
    /// 	<para>The implementation now synchronizes on each context to make sure only one
    ///     thread is accessing the context object at a time.</para>
    /// </summary>
    class SimpleBWControlService : IBWControlService
    {
        private static ILog log = LogManager.GetLogger(typeof(SimpleBWControlService));

        object _syncLock = new object();

        /// <summary>
        /// Map(IBWControllable, BWContext)
        /// </summary>
        protected SynchronizedHashtable _contextMap = new SynchronizedHashtable();
        System.Timers.Timer _tokenDistributor;
        protected long _interval;
        protected long _defaultCapacity;

        public SimpleBWControlService()
        {
            _interval = FluorineConfiguration.Instance.FluorineSettings.BWControlService.Interval;
            _defaultCapacity = FluorineConfiguration.Instance.FluorineSettings.BWControlService.DefaultCapacity;
        }

        internal long DefaultCapacity
        {
            get { return _defaultCapacity; }
        }

        internal SynchronizedHashtable ContextMap
        {
            get { return _contextMap; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. 
        /// </summary>
        public object SyncRoot { get { return _syncLock; } }

        #region IService Members

        public void Start(ConfigurationSection configuration)
        {
            _tokenDistributor = new System.Timers.Timer();
            _tokenDistributor.Elapsed += new System.Timers.ElapsedEventHandler(TokenDistributor_Elapsed);
            _tokenDistributor.Interval = _interval;
            _tokenDistributor.AutoReset = true;
            _tokenDistributor.Enabled = true;
        }

        void TokenDistributor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_contextMap.Count == 0)
            {
                // Early bail out, nothing to do.
                return;
            }
            lock (this.SyncRoot)
            {
                ICollection contexts = _contextMap.Values;
                foreach (BWContext context in contexts)
                {
                    lock (context)
                    {
                        if (context.bwConfig != null)
                        {
                            long t = System.Environment.TickCount;
                            long delta = t - context.lastSchedule;
                            context.lastSchedule = t;
                            if (context.bwConfig[3] >= 0)
                            {
                                if (_defaultCapacity >= context.tokenRc[3])
                                {
                                    context.tokenRc[3] += (double)(context.bwConfig[3]) * delta / 8000;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    if (context.bwConfig[i] >= 0 && _defaultCapacity >= context.tokenRc[i])
                                    {
                                        context.tokenRc[i] += (double)(context.bwConfig[i]) * delta / 8000;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (BWContext context in contexts)
                {
                    lock (context)
                    {
                        // Notify all blocked requests
                        Monitor.PulseAll(context);
                        // Notify all callbacks
                        InvokeCallback(context);
                    }
                }
            }
        }

        public void Shutdown()
        {
            _tokenDistributor.Enabled = false;
        }

        #endregion

        #region IBWControlService Members

        public IBWControlContext RegisterBWControllable(IBWControllable bc)
        {
            BWContext context = new BWContext(bc);
            long[] channelInitialBurst = null;
            if (bc.BandwidthConfiguration != null)
            {
                context.bwConfig = new long[4];
                for (int i = 0; i < 4; i++)
                {
                    context.bwConfig[i] = bc.BandwidthConfiguration.GetChannelBandwidth()[i];
                }
                channelInitialBurst = bc.BandwidthConfiguration.GetChannelInitialBurst();
            }
            context.buckets[0] = new Bucket(this, bc, 0);
            context.buckets[1] = new Bucket(this, bc, 1);
            context.buckets[2] = new Bucket(this, bc, 2);
            context.tokenRc = new double[4];
            if (context.bwConfig != null)
            {
                // Set the initial value to token resources as "defaultCapacity/2"
                for (int i = 0; i < 4; i++)
                {
                    if (channelInitialBurst[i] >= 0)
                    {
                        context.tokenRc[i] = channelInitialBurst[i];
                    }
                    else
                    {
                        context.tokenRc[i] = _defaultCapacity / 2;
                    }
                }
                context.lastSchedule = System.Environment.TickCount;
            }
            else
            {
                context.lastSchedule = -1;
            }
            lock (this.SyncRoot)
            {
                _contextMap.Add(bc, context);
            }
            return context;
        }

        public void UnregisterBWControllable(IBWControlContext context)
        {
            ResetBuckets(context);
            lock (this.SyncRoot)
            {
                _contextMap.Remove(context.GetBWControllable());
            }
        }

        public IBWControlContext LookupContext(IBWControllable bc)
        {
            lock (this.SyncRoot)
            {
                return _contextMap[bc] as IBWControlContext;
            }
        }

        public void UpdateBWConfigure(IBWControlContext context)
        {
            if (!(context is BWContext)) return;
            BWContext c = (BWContext)context;
            IBWControllable bc = c.GetBWControllable();
            lock (c)
            {
                if (bc.BandwidthConfiguration == null)
                {
                    c.bwConfig = null;
                    c.lastSchedule = -1;
                }
                else
                {
                    long[] oldConfig = c.bwConfig;
                    c.bwConfig = new long[4];
                    for (int i = 0; i < 4; i++)
                    {
                        c.bwConfig[i] = bc.BandwidthConfiguration.GetChannelBandwidth()[i];
                    }
                    if (oldConfig == null)
                    {
                        // Initialize the last schedule timestamp if necessary
                        c.lastSchedule = System.Environment.TickCount;
                        long[] channelInitialBurst = bc.BandwidthConfiguration.GetChannelInitialBurst();
                        // Set the initial value to token resources as "defaultCapacity/2"
                        for (int i = 0; i < 4; i++)
                        {
                            if (channelInitialBurst[i] >= 0)
                            {
                                c.tokenRc[i] = channelInitialBurst[i];
                            }
                            else
                            {
                                c.tokenRc[i] = _defaultCapacity / 2;
                            }
                        }
                    }
                    else
                    {
                        // we have scheduled before, so migration of token is needed
                        if (c.bwConfig[Constants.OverallChannel] >= 0 &&
                                oldConfig[Constants.OverallChannel] < 0)
                        {
                            c.tokenRc[Constants.OverallChannel] +=
                                c.tokenRc[Constants.AudioChannel] +
                                c.tokenRc[Constants.VideoChannel] +
                                c.tokenRc[Constants.DataChannel];
                            for (int i = 0; i < 3; i++)
                            {
                                c.tokenRc[i] = 0;
                            }
                        }
                        else if (c.bwConfig[Constants.OverallChannel] < 0 &&
                                oldConfig[Constants.OverallChannel] >= 0)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (c.bwConfig[i] >= 0)
                                {
                                    c.tokenRc[i] += c.tokenRc[Constants.OverallChannel];
                                    break;
                                }
                            }
                            c.tokenRc[Constants.OverallChannel] = 0;
                        }
                    }
                }
            }
        }

        public void ResetBuckets(IBWControlContext context)
        {
            if (!(context is BWContext)) return;
            BWContext c = (BWContext)context;
            for (int i = 0; i < 3; i++)
            {
                c.buckets[i].Reset();
            }
        }

        public ITokenBucket GetAudioBucket(IBWControlContext context)
        {
            if (!(context is BWContext)) return null;
            BWContext c = (BWContext)context;
            return c.buckets[0];
        }

        public ITokenBucket GetVideoBucket(IBWControlContext context)
        {
            if (!(context is BWContext)) return null;
            BWContext c = (BWContext)context;
            return c.buckets[1];
        }

        public ITokenBucket GetDataBucket(IBWControlContext context)
        {
            if (!(context is BWContext)) return null;
            BWContext c = (BWContext)context;
            return c.buckets[2];
        }

        #endregion

        private bool ProcessRequest(TokenRequest request)
        {
            IBWControllable bc = request.initialBC;
            while (bc != null)
            {
                BWContext context = _contextMap[bc] as BWContext;
                if (context == null)
                {
                    RollbackRequest(request);
                    return false;
                }
                lock (context)
                {
                    if (context.bwConfig != null)
                    {
                        bool result;
                        if (request.type == TokenRequestType.BLOCKING)
                        {
                            result = ProcessBlockingRequest(request, context);
                        }
                        else if (request.type == TokenRequestType.NONBLOCKING)
                        {
                            result = ProcessNonblockingRequest(request, context);
                        }
                        else
                        {
                            result = ProcessBestEffortRequest(request, context);
                        }
                        if (!result)
                        {
                            // for non-blocking mode, the callback is
                            // recorded and will be rolled back when being reset,
                            // so we don't need to do rollback here.
                            if (request.type != TokenRequestType.NONBLOCKING)
                            {
                                RollbackRequest(request);
                            }
                            return false;
                        }
                    }
                    TokenRequestContext requestContext = new TokenRequestContext();
                    requestContext.acquiredToken = request.requestToken;
                    requestContext.bc = bc;
                    request.acquiredStack.Push(requestContext);
                }
                bc = bc.GetParentBWControllable();
            }
            // for best effort request, we need to rollback over-charged tokens
            if (request.type == TokenRequestType.BEST_EFFORT)
            {
                RollbackRequest(request);
            }
            return true;
        }

        private bool ProcessBlockingRequest(TokenRequest request, BWContext context)
        {
            context.timeToWait = request.timeout;
            do
            {
                if (context.bwConfig[3] >= 0)
                {
                    if (context.tokenRc[3] >= request.requestToken)
                    {
                        context.tokenRc[3] -= request.requestToken;
                        request.timeout = context.timeToWait;
                        return true;
                    }
                }
                else
                {
                    if (context.tokenRc[request.channel] < 0) return true;
                    if (context.tokenRc[request.channel] >= request.requestToken)
                    {
                        context.tokenRc[request.channel] -= request.requestToken;
                        request.timeout = context.timeToWait;
                        return true;
                    }
                }
                long beforeWait = System.Environment.TickCount;
                try
                {
                    Monitor.Wait(this, (int)context.timeToWait);
                }
                catch (ThreadInterruptedException)
                {
                }
                context.timeToWait -= System.Environment.TickCount - beforeWait;
            } while (context.timeToWait > 0);
            return false;
        }

        private bool ProcessNonblockingRequest(TokenRequest request, BWContext context)
        {
            if (context.bwConfig[3] >= 0)
            {
                if (context.tokenRc[3] >= request.requestToken)
                {
                    context.tokenRc[3] -= request.requestToken;
                    return true;
                }
            }
            else
            {
                if (context.tokenRc[request.channel] < 0) return true;
                if (context.tokenRc[request.channel] >= request.requestToken)
                {
                    context.tokenRc[request.channel] -= request.requestToken;
                    return true;
                }
            }
            (context.pendingRequestArray[request.channel] as IList).Add(request);
            return false;
        }

        private bool ProcessBestEffortRequest(TokenRequest request, BWContext context)
        {
            if (context.bwConfig[3] >= 0)
            {
                if (context.tokenRc[3] >= request.requestToken)
                {
                    context.tokenRc[3] -= request.requestToken;
                }
                else
                {
                    request.requestToken = context.tokenRc[3];
                    context.tokenRc[3] = 0;
                }
            }
            else
            {
                if (context.tokenRc[request.channel] < 0) return true;
                if (context.tokenRc[request.channel] >= request.requestToken)
                {
                    context.tokenRc[request.channel] -= request.requestToken;
                }
                else
                {
                    request.requestToken = context.tokenRc[request.channel];
                    context.tokenRc[request.channel] = 0;
                }
            }
            if (request.requestToken == 0) return false;
            else return true;
        }

        private void InvokeCallback(BWContext context)
        {
            // loop through all channels in a context
            for (int i = 0; i < 3; i++)
            {
                IList pendingList = context.pendingRequestArray[i] as IList;
                if (pendingList.Count > 0)
                {
                    // loop through all pending requests in a channel
                    foreach (TokenRequest request in pendingList)
                    {
                        IBWControllable bc = context.GetBWControllable();
                        while (bc != null)
                        {
                            BWContext c = _contextMap[bc] as BWContext;
                            if (c == null)
                            {
                                // context has been unregistered, we should ignore
                                // this callback
                                break;
                            }
                            lock (c)
                            {
                                if (c.bwConfig != null && !ProcessNonblockingRequest(request, c))
                                {
                                    break;
                                }
                            }
                            TokenRequestContext requestContext = new TokenRequestContext();
                            requestContext.acquiredToken = request.requestToken;
                            requestContext.bc = bc;
                            request.acquiredStack.Push(requestContext);
                            bc = bc.GetParentBWControllable();
                        }
                        if (bc == null)
                        {
                            // successfully got the required tokens
                            try
                            {
                                request.callback.Available(context.buckets[request.channel], (long)request.requestToken);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error calling request's callback", ex);
                            }
                        }
                    }
                    pendingList.Clear();
                }
            }
        }

        /**
         * Give back the acquired tokens due to failing to accomplish the requested
         * operation or over-charged tokens in the case of best-effort request.
         * @param request
         */
        private void RollbackRequest(TokenRequest request)
        {
            while (request.acquiredStack.Count > 0)
            {
                TokenRequestContext requestContext = request.acquiredStack.Pop() as TokenRequestContext;
                BWContext context = _contextMap[requestContext.bc] as BWContext;
                if (context != null)
                {
                    lock (context)
                    {
                        if (context.bwConfig != null)
                        {
                            if (context.bwConfig[3] >= 0)
                            {
                                if (request.type == TokenRequestType.BEST_EFFORT)
                                {
                                    context.tokenRc[3] += requestContext.acquiredToken - request.requestToken;
                                }
                                else
                                {
                                    context.tokenRc[3] += requestContext.acquiredToken;
                                }
                            }
                            else
                            {
                                if (context.bwConfig[request.channel] >= 0)
                                {
                                    if (request.type == TokenRequestType.BEST_EFFORT)
                                    {
                                        context.tokenRc[request.channel] += requestContext.acquiredToken - request.requestToken;
                                    }
                                    else
                                    {
                                        context.tokenRc[request.channel] += requestContext.acquiredToken;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        class Bucket : ITokenBucket
        {
            private static ILog log = LogManager.GetLogger(typeof(Bucket));

            private IBWControllable _bc;
            private int _channel;
            private SimpleBWControlService _simpleBWControlService;

            public Bucket(SimpleBWControlService simpleBWControlService, IBWControllable bc, int channel)
            {
                _bc = bc;
                _channel = channel;
                _simpleBWControlService = simpleBWControlService;
            }

            #region ITokenBucket Members

            public bool AcquireToken(long tokenCount, long wait)
            {
                if (wait < 0) return false;
                TokenRequest request = new TokenRequest();
                request.type = TokenRequestType.BLOCKING;
                request.timeout = wait;
                request.channel = _channel;
                request.initialBC = _bc;
                request.requestToken = tokenCount;
                return _simpleBWControlService.ProcessRequest(request);
            }

            public bool AcquireTokenNonblocking(long tokenCount, ITokenBucketCallback callback)
            {
                TokenRequest request = new TokenRequest();
                request.type = TokenRequestType.NONBLOCKING;
                request.callback = callback;
                request.channel = _channel;
                request.initialBC = _bc;
                request.requestToken = tokenCount;
                return _simpleBWControlService.ProcessRequest(request);
            }

            public long AcquireTokenBestEffort(long upperLimitCount)
            {
                TokenRequest request = new TokenRequest();
                request.type = TokenRequestType.BEST_EFFORT;
                request.channel = _channel;
                request.initialBC = _bc;
                request.requestToken = upperLimitCount;
                if (_simpleBWControlService.ProcessRequest(request))
                {
                    return (long)request.requestToken;
                }
                else
                {
                    return 0;
                }
            }

            public long Capacity
            {
                get { return _simpleBWControlService.DefaultCapacity; }
            }

            public double Speed
            {
                get
                {
                    BWContext context = _simpleBWControlService.ContextMap[_bc] as BWContext;
                    if (context.bwConfig[3] >= 0)
                    {
                        return context.bwConfig[3] * 1000 / 8;
                    }
                    else
                    {
                        if (context.bwConfig[_channel] >= 0)
                        {
                            return context.bwConfig[_channel] * 1000 / 8;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }

            public void Reset()
            {
                // TODO wake up all blocked threads
                IBWControllable bc = _bc;
                while (bc != null)
                {
                    BWContext context = _simpleBWControlService.ContextMap[bc] as BWContext;
                    if (context == null)
                        break;
                    lock (context)
                    {
                        IList pendingList = context.pendingRequestArray[_channel] as IList;
                        TokenRequest toRemove = null;
                        foreach (TokenRequest request in pendingList)
                        {
                            if (request.initialBC == _bc)
                            {
                                _simpleBWControlService.RollbackRequest(request);
                                toRemove = request;
                                break;
                            }
                        }
                        if (toRemove != null)
                        {
                            pendingList.Remove(toRemove);
                            try
                            {
                                toRemove.callback.Reset(this, (long)toRemove.requestToken);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error reset request's callback", ex);
                            }
                            break;
                        }
                    }
                    bc = bc.GetParentBWControllable();
                }
            }

            #endregion
        }

        class TokenRequest
        {
            public TokenRequestType type;
            public ITokenBucketCallback callback;
            public long timeout;
            public int channel;
            public IBWControllable initialBC;
            public double requestToken;
            /// <summary>
            /// Stack(TokenRequestContext)
            /// </summary>
            public Stack acquiredStack = new Stack();
        }

        class TokenRequestContext
        {
            public IBWControllable bc;
            public double acquiredToken;
        }

        enum TokenRequestType
        {
            BLOCKING,
            NONBLOCKING,
            BEST_EFFORT
        }

        class BWContext : IBWControlContext
        {
            public long[] bwConfig;
            public double[] tokenRc = new double[4];
            public ITokenBucket[] buckets = new ITokenBucket[3];
            /// <summary>
            /// List(TokenRequest)
            /// </summary>
            public ArrayList pendingRequestArray;
            public long lastSchedule;
            public long timeToWait;

            private IBWControllable _controllable;

            public BWContext(IBWControllable controllable)
            {
                _controllable = controllable;
                pendingRequestArray = new ArrayList();
                pendingRequestArray.AddRange(new IList[] { new CopyOnWriteArray(), new CopyOnWriteArray(), new CopyOnWriteArray() });
            }


            #region IBWControlContext Members

            public IBWControllable GetBWControllable()
            {
                return _controllable;
            }

            #endregion
        }
    }
}
