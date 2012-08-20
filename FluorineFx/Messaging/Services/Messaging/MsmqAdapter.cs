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

#if !MONO	

using System;
using System.Collections;
using System.Reflection;
using System.Messaging;
using log4net;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Config;

namespace FluorineFx.Messaging.Services.Messaging
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    class MsmqAdapter : ServiceAdapter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MsmqAdapter));

        MsmqProperties _msmqSettings;
        MessageQueue _messageQueue;
        IMessageFormatter _messageFormatter;
        Hashtable _consumers;

        public override void Init()
        {
            base.Init();
            _consumers = new Hashtable();
            _msmqSettings = DestinationDefinition.Properties.Msmq;
            if (_msmqSettings != null)
            {
                MessageQueue.EnableConnectionCache = false;
                log.Debug(__Res.GetString(__Res.Msmq_StartQueue, _msmqSettings.Name));
                _messageQueue = new MessageQueue(_msmqSettings.Name);
                string messageFormatterName = MsmqProperties.BinaryMessageFormatter;
                if (!string.IsNullOrEmpty(_msmqSettings.Formatter))
                    messageFormatterName = _msmqSettings.Formatter;
                log.Debug(__Res.GetString(__Res.Msmq_InitFormatter, messageFormatterName));
                if (messageFormatterName == MsmqProperties.BinaryMessageFormatter)
                {
                    _messageFormatter = new BinaryMessageFormatter();
                    _messageQueue.Formatter = _messageFormatter;
                }
                else if (messageFormatterName.StartsWith(MsmqProperties.XmlMessageFormatter))
                {
                    string[] formatterParts = messageFormatterName.Split(new char[]{';'});
                    Type[] types;
                    if (formatterParts.Length == 1)
                        types = new Type[] { typeof(string) };
                    else
                    {
                        types = new Type[formatterParts.Length-1];
                        for (int i = 1; i < formatterParts.Length; i++)
                        {
                            Type type = ObjectFactory.Locate(formatterParts[i]);
                            if (type != null)
                                types[i-1] = type;
                            else
                                log.Error(__Res.GetString(__Res.Type_InitError, formatterParts[i]));
                        }
                    }
                    _messageFormatter = new XmlMessageFormatter(types);
                    _messageQueue.Formatter = _messageFormatter;
                }
                else
                {
                    log.Error(__Res.GetString(__Res.Type_InitError, messageFormatterName));
                    _messageQueue.Close();
                    _messageQueue = null;
                }
            }
            else
                log.Error(__Res.GetString(__Res.ServiceAdapter_MissingSettings));
        }

        public override void Stop()
        {
            log.Debug(__Res.GetString(__Res.ServiceAdapter_Stop));
            if (_messageQueue != null)
            {
                EnableReceiving(false);
                _messageQueue.Close();
            }
            base.Stop();
        }

        private void EnableReceiving(bool enable)
        {
            log.Debug(__Res.GetString(__Res.Msmq_Enable, enable));
            if (enable)
            {
                if (_messageQueue != null )
                {
                    _messageQueue.ReceiveCompleted += OnReceiveCompleted;
                    _messageQueue.BeginReceive(new TimeSpan(1, 0, 0), _messageQueue);
                }
            }
            else
            {
                if (_messageQueue != null )
                {
                    _messageQueue.ReceiveCompleted -= OnReceiveCompleted;
                }
            }
        }

        public override bool HandlesSubscriptions
        {
            get
            {
                return true;
            }
        }

        public override object Manage(CommandMessage commandMessage)
        {
            object result = base.Manage(commandMessage);
            switch (commandMessage.operation)
            {
                case CommandMessage.SubscribeOperation:
                    lock (SyncRoot)
                    {
                        bool enableReceive = _consumers.Count == 0;
                        _consumers[commandMessage.clientId] = null;
                        if (enableReceive)
                            EnableReceiving(true);
                    }
                    break;
                case CommandMessage.UnsubscribeOperation:
                    lock (SyncRoot)
                    {
                        if (_consumers.Contains(commandMessage.clientId))
                        {
                            _consumers.Remove(commandMessage.clientId);
                            if (_consumers.Count == 0)
                                EnableReceiving(false);
                        }
                    }
                    break;
            }
            return result;
        }

        void OnReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            Message message = ((MessageQueue)e.AsyncResult.AsyncState).EndReceive(e.AsyncResult);
            log.Debug(__Res.GetString(__Res.Msmq_Receive, message.Label, message.Id));
            try
            {
                AsyncMessage asyncMessage = ConvertMsmqMessage(message);
                MessageService messageService = Destination.Service as MessageService;
                if (messageService != null) messageService.PushMessageToClients(asyncMessage);
                //msgBroker.RouteMessage(msg);
            }
            catch (System.Runtime.Serialization.SerializationException ex)
            {
                log.Error(__Res.GetString(__Res.Msmq_Poison), ex);
            }
            // start looking for the next message
            IAsyncResult asyncResult = ((MessageQueue)e.AsyncResult.AsyncState).BeginReceive(new TimeSpan(1, 0, 0), ((MessageQueue)e.AsyncResult.AsyncState));
        }

        private AsyncMessage ConvertMsmqMessage(Message message)
        {
            AsyncMessage asyncMessage = new AsyncMessage();
            asyncMessage.body = message.Body;
            asyncMessage.destination = DestinationDefinition.Id;
            asyncMessage.clientId = Guid.NewGuid().ToString("D");
            asyncMessage.messageId = Guid.NewGuid().ToString("D");
            asyncMessage.timestamp = Environment.TickCount;

            asyncMessage.headers["MSMQId"] = message.Id;
            asyncMessage.headers["MSMQLabel"] = message.Label;
            //asyncMessage.headers["MSMQBody"] = message.Body;
            if (message.Body != null)
            {
                PropertyInfo[] propertyInfos = message.Body.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo pi in propertyInfos)
                {
                    if (pi.GetGetMethod() != null && pi.GetGetMethod().GetParameters().Length == 0)
                        asyncMessage.headers[pi.Name] = pi.GetValue(message.Body, null);
                }
                FieldInfo[] fieldInfos = message.Body.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo fi in fieldInfos)
                {
                    asyncMessage.headers[fi.Name] = fi.GetValue(message.Body);
                }
            }
            return asyncMessage;
        }

        public override object Invoke(IMessage message)
        {
            if (_messageQueue != null)
            {
                log.Debug(__Res.GetString(__Res.Msmq_Send, message.clientId));

                Message mqMessage = new Message();
                mqMessage.Formatter = _messageFormatter;
                mqMessage.Body = message.body;
                if (_msmqSettings != null && _msmqSettings.Label != null)
                    mqMessage.Label = _msmqSettings.Label;
                else
                    mqMessage.Label = message.clientId.ToString();
                _messageQueue.Send(mqMessage);
            }
            return null;
        }
    }
}

#endif
