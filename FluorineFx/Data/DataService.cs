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
using System.Text;
using log4net;
using FluorineFx.AMF3;
using FluorineFx.Util;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Endpoints;
using FluorineFx.Data.Messages;

namespace FluorineFx.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal class DataService : MessageService
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(DataService));
		//private ILog _coreDumpLog = null;

        public DataService(MessageBroker messageBroker, ServiceDefinition serviceDefinition)
            : base(messageBroker, serviceDefinition)
		{
            /*
			try
			{
				_coreDumpLog = LogManager.GetLogger("FluorineFx.Dump");
			}
			catch{}
            */
		}

        protected override Destination NewDestination(DestinationDefinition destinationDefinition)
		{
            return new DataDestination(this, destinationDefinition);
		}

		public override object ServiceMessage(IMessage message)
		{
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null )
			{
                //Sub/unsub handled by base class
				return base.ServiceMessage(commandMessage);
			}
			else
			{
				AsyncMessage responseMessage = null;
				DataMessage dataMessage = message as DataMessage;

				DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
				if( dataDestination.SubscriptionManager.GetSubscriber(dataMessage.clientId as string) == null )
				{
					//Subscribe here as DS doesn't send a separate subscribe command
                    CommandMessage commandMessageSubscribe = new CommandMessage();
                    commandMessageSubscribe.destination = dataDestination.Id;
                    commandMessageSubscribe.operation = CommandMessage.SubscribeOperation;
                    commandMessageSubscribe.clientId = dataMessage.clientId as string;
                    string endpointId = dataMessage.GetHeader(MessageBase.EndpointHeader) as string;
                    commandMessageSubscribe.headers[MessageBase.EndpointHeader] = endpointId;
                    string flexClientIdHeader = dataMessage.GetHeader(MessageBase.FlexClientIdHeader) as string;
                    if( flexClientIdHeader != null )
                        commandMessageSubscribe.headers[MessageBase.FlexClientIdHeader] = flexClientIdHeader;
                    IEndpoint endpoint = GetMessageBroker().GetEndpoint(endpointId);
                    endpoint.ServiceMessage(commandMessageSubscribe);//route through the endpoint again
                    //base.ServiceMessage(commandMessageSubscribe);
				}

				switch(dataMessage.operation)
				{
					case DataMessage.FillOperation:
						responseMessage = ExecuteFillOperation(message);
						break;
					case DataMessage.GetOperation:
						responseMessage = ExecuteGetOperation(message);
						break;
					case DataMessage.BatchedOperation:
					case DataMessage.MultiBatchOperation:
					case DataMessage.TransactedOperation:
						responseMessage = ExecuteMultiBatchOperation(message);
						break;
					case DataMessage.PageItemsOperation:
						responseMessage = ExecutePageItemsOperation(message);
						break;
					case DataMessage.PageOperation:
						responseMessage = ExecutePageOperation(message);
						break;
					case DataMessage.ReleaseCollectionOperation:
						responseMessage = ExecuteReleaseCollectionOperation(message);
						break;
					case DataMessage.GetSequenceIdOperation:
						responseMessage = ExecuteGetSequenceIdOperation(message);
						break;
					case DataMessage.ReleaseItemOperation:
						responseMessage = ExecuteReleaseItemOperation(message);
						break;
					default:
						if(log.IsErrorEnabled)
							log.Error(__Res.GetString(__Res.DataService_Unknown, dataMessage.operation));

						responseMessage = new AcknowledgeMessage();
						break;
				}
				responseMessage.clientId = message.clientId;
				responseMessage.correlationId = message.messageId;
				//Dump();
				return responseMessage;
			}
		}

		private AcknowledgeMessage ExecuteFillOperation(IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			AcknowledgeMessage responseMessage = null;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			IList collection = dataDestination.ServiceAdapter.Invoke(message) as IList;
			responseMessage = dataDestination.SequenceManager.ManageSequence(dataMessage, collection);
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteGetOperation(IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			AcknowledgeMessage responseMessage = null;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			object result = dataDestination.ServiceAdapter.Invoke(message);
			if( result == null )
				responseMessage = new AcknowledgeMessage();
			else
			{
				ArrayList collection = new ArrayList(1);
				collection.Add(result);
				responseMessage = dataDestination.SequenceManager.ManageSequence(dataMessage, collection);
			}
			return responseMessage;
		}

		private AcknowledgeMessage ExecutePageItemsOperation(IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			return dataDestination.SequenceManager.GetPageItems(dataMessage);
		}

		private AcknowledgeMessage ExecutePageOperation(IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			return dataDestination.SequenceManager.GetPage(dataMessage);
		}

		private AcknowledgeMessage ExecuteReleaseCollectionOperation(IMessage message)
		{
			AcknowledgeMessage responseMessage = new AcknowledgeMessage();
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.SequenceManager.ReleaseCollectionOperation(dataMessage);
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteReleaseItemOperation(IMessage message)
		{
			AcknowledgeMessage responseMessage = new AcknowledgeMessage();
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.SequenceManager.ReleaseItemOperation(dataMessage);
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteGetSequenceIdOperation(IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			AcknowledgeMessage responseMessage = null;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
            //2 cases: body set or identity set
            if (dataMessage.body != null && dataMessage.body is IList)
            {
                dataMessage.operation = DataMessage.FillOperation;
                IList result = dataDestination.ServiceAdapter.Invoke(message) as IList;//Fill returns an IList
                responseMessage = dataDestination.SequenceManager.ManageSequence(dataMessage, result);
                dataMessage.operation = DataMessage.GetSequenceIdOperation;
            }
            else
            {
                dataMessage.operation = DataMessage.GetOperation;
                object result = dataDestination.ServiceAdapter.Invoke(message);
                if (result != null)
                {
                    ArrayList collection = new ArrayList(1);
                    collection.Add(result);
                    responseMessage = dataDestination.SequenceManager.ManageSequence(dataMessage, collection);
                }
                else
                {
                    SequencedMessage sequencedMessage = new SequencedMessage();
                    sequencedMessage.sequenceId = -1;//no sequence
                    responseMessage = sequencedMessage;
                }
                dataMessage.operation = DataMessage.GetSequenceIdOperation;
            }
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteMultiBatchOperation(IMessage message)
		{
			//May contain multiple batched, create, update or delete operations that involve 
			//more than one destination, that is, more than one remote adapter
			AcknowledgeMessage responseMessage = null;
			DataMessage dataMessage = message as DataMessage;
			IList messages = dataMessage.body as IList;

			DataServiceTransaction dataServiceTransaction = DataServiceTransaction.Begin(this);
			dataServiceTransaction.ClientId = message.clientId as string;
			//correlate al generated messages
			dataServiceTransaction.CorrelationId = message.messageId;

			string currentDestination = null;
			ArrayList currentMessageBatch = new ArrayList();

			for(int i = 0; i < messages.Count; i++)
			{
				DataMessage batchMessage = messages[i] as DataMessage;
				string destination = batchMessage.destination;
				DataDestination dataDestination = GetDestination(batchMessage) as DataDestination;
				
				if( currentDestination != null && destination != currentDestination &&
					currentMessageBatch.Count > 0 )
				{
					MessageBatch messageBatch = ServiceBatch(message, currentMessageBatch);
					dataServiceTransaction.AddProcessedMessageBatch(messageBatch);
					currentMessageBatch = new ArrayList();
				}
				currentMessageBatch.Add(batchMessage);
				currentDestination = destination;

				if(batchMessage is UpdateCollectionMessage)
					dataServiceTransaction.AddClientUpdateCollection(batchMessage as UpdateCollectionMessage);
			}
			if(currentMessageBatch.Count > 0)
			{
				MessageBatch messageBatch = ServiceBatch(message, currentMessageBatch);
				dataServiceTransaction.AddProcessedMessageBatch(messageBatch);
			}
			
			dataServiceTransaction.Commit();
			IList outgoingMessages = dataServiceTransaction.GetOutgoingMessages();
			responseMessage = new AcknowledgeMessage();
			object[] result = new object[outgoingMessages.Count];
			outgoingMessages.CopyTo(result, 0);
			responseMessage.body = result;//outgoingMessages.ToArray(typeof(object));
			return responseMessage;
		}

		private MessageBatch ServiceBatch(IMessage message, ArrayList messages)
		{
			//Send a DataMessage.BatchedOperation to the specific adapter
			DataMessage dataMessage = messages[0] as DataMessage;
			DataMessage adapterDataMessage = null;
			if( messages.Count == 1 && dataMessage.operation == DataMessage.BatchedOperation )
				adapterDataMessage = dataMessage;
			else
			{
				adapterDataMessage = new DataMessage();
				adapterDataMessage.destination = dataMessage.destination;
				adapterDataMessage.operation = DataMessage.BatchedOperation;
				adapterDataMessage.body = messages;
				adapterDataMessage.headers = message.headers;
				adapterDataMessage.clientId = message.clientId;
			}

			DataDestination dataDestination = GetDestination(dataMessage) as DataDestination;
			IList result = dataDestination.ServiceAdapter.Invoke(adapterDataMessage) as IList;

			MessageBatch messageBatch = new MessageBatch(adapterDataMessage, result);
			return messageBatch;
		}

		private void Dump()
		{
            /*
			if(_coreDumpLog != null && _coreDumpLog.IsDebugEnabled)
			{
				DumpContext dumpContext = new DumpContext();
				StringBuilder sb = new StringBuilder();
				dumpContext.AppendLine(string.Empty);
				dumpContext.AppendLine("CORE DUMP START");
				
				Destination[] destinations = this.GetDestinations();
				dumpContext.Indent();
				dumpContext.AppendLine( "Destinations" );
				foreach(Destination destination in destinations)
				{
					dumpContext.Indent();
					destination.Dump(dumpContext);
					dumpContext.Unindent();
				}
				dumpContext.Unindent();
				dumpContext.AppendLine("CORE DUMP END");
				_coreDumpLog.Debug( dumpContext.ToString() );
			}
            */
		}
	}
}
