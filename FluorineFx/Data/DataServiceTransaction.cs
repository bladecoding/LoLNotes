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
using System.Security;
using System.Security.Permissions;
using log4net;
using FluorineFx.Exceptions;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services.Messaging;
using FluorineFx.Data.Messages;
using FluorineFx.Context;

namespace FluorineFx.Data
{
	//http://livedocs.adobe.com/flex/2/fds2javadoc/flex/data/DataServiceTransaction.html

	/// <summary>
	/// Describes the current state of the DataServiceTransaction
	/// </summary>
	public enum TransactionState
	{
		/// <summary>
		/// Transactions in this state are waiting to be committed or rolled back.
		/// </summary>
		Active,
		/// <summary>
		/// Transactions in this state have been committed.
		/// </summary>
		Committed,
		/// <summary>
		/// Transactions in this state have been rolled back.
		/// </summary>
		RolledBack
	}

	/// <summary>
	/// A DataServiceTransaction instance is created for each operation that modifies the state of 
	/// objects managed by Data Management Services. You can use this class from server-side code to 
	/// push changes to managed data stored on clients as long as they have the autoSyncEnabled 
	/// property set to true.
	/// </summary>
	public class DataServiceTransaction
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(DataServiceTransaction));

        sealed class RefreshFillData
        {
            string _destination;
            IList _parameters;

            public RefreshFillData(string destination, IList parameters)
            {
                _destination = destination;
                _parameters = parameters;
            }

            public string Destination { get { return _destination; } }
            public IList Parameters { get { return _parameters; } }
        }

		DataService _dataService;
		bool _sendMessagesToPeers;
		bool _rollbackOnly;
		string _clientId;
		string _correlationId;
		TransactionState _transactionState;

        ArrayList _refreshFills;
		ArrayList	_processedMessageBatches;
		Hashtable	_updateCollectionMessages;
		Hashtable	_clientUpdateCollectionMessages;
		ArrayList	_outgoingMessages;
		ArrayList	_pushMessages;
		static int	_idCounter;

		private DataServiceTransaction(DataService dataService)
		{
			_transactionState = TransactionState.Active;
			_dataService = dataService;
			_sendMessagesToPeers = true;
			_rollbackOnly = false;
			_outgoingMessages = new ArrayList();
			_processedMessageBatches = new ArrayList(1);
#if (NET_1_1)
			_updateCollectionMessages = new Hashtable(new ListHashCodeProvider(), new ListComparer());
			_clientUpdateCollectionMessages =  new Hashtable(new ListHashCodeProvider(), new ListComparer());
#else
            _updateCollectionMessages = new Hashtable(new ListHashCodeProvider());
			_clientUpdateCollectionMessages = new Hashtable(new ListHashCodeProvider());
#endif
        }

		/// <summary>
		/// Gets the current state of the DataServiceTransaction.
		/// </summary>
		public TransactionState TransactionState
		{
			get{ return _transactionState; }
		}

		internal string ClientId
		{
			get{ return _clientId; }
			set{ _clientId = value; }
		}

		internal string CorrelationId
		{
			get{ return _correlationId; }
			set{ _correlationId = value; }
		}

		/// <summary>
		/// Gets the current DataServiceTransaction if one has been associated with the current thread.
		/// When you begin a DataServiceTransaction, it is automatically associated with your thread.
		/// If you want to add changes while in an adapter/assembler's sync method, you can use this 
		/// method to get a reference to the existing DataServiceTransaction. 
		/// In this case, you can call updateItem, createItem, deleteItem, and so forth, but you do not 
		/// call commit or rollback yourself as this is done by the data services code when the 
		/// sync method completes.
		/// </summary>
		/// <value>The current DataServiceTransaction or null if there is no transaction currently associated with the current thread.</value>
		public static DataServiceTransaction CurrentDataServiceTransaction
		{
			get
			{
                return FluorineWebSafeCallContext.GetData(FluorineContext.FluorineDataServiceTransaction) as DataServiceTransaction;
			}
		}

        private static void SetCurrentDataServiceTransaction(DataServiceTransaction dataServiceTransaction)
        {
            FluorineWebSafeCallContext.SetData(FluorineContext.FluorineDataServiceTransaction, dataServiceTransaction);
        }

		/// <summary>
		/// Starts a DataServiceTransaction that you can use to send changes to clients. Use this method when 
		/// you want to push changes to clients when you are not in the midst of an adapter/assembler's method.
		/// If you are being called from within the context of an assembler method (or if you are not 
		/// sure), you should call the CurrentDataServiceTransaction property. If that returns null, you can then 
		/// use this method to start one. 
		/// If you call this method, you must either call commit or rollback to complete the transaction. 
		/// You should make sure that this commit or rollback occurs no matter what else happens - it almost 
		/// always must be in a finally block in your code. 
		/// If you are in an assembler method, you should not commit or rollback the transaction as that 
		/// happens in the data services code when it completes. Instead, if you want to rollback the 
		/// transaction, call SetRollbackOnly. 
		/// </summary>
		/// <param name="serverId">Identifies the MessageBroker that created the Data Management Services destination you want to manipulate using this api. Typically there is only one MessageBroker for each web application and in this case, you can pass in null.</param>
		/// <returns></returns>
		public static DataServiceTransaction Begin(string serverId)
		{
			if( DataServiceTransaction.CurrentDataServiceTransaction == null )
			{
				MessageBroker messageBroker = MessageBroker.GetMessageBroker(serverId);
                DataService dataService = messageBroker.GetServiceByMessageType("flex.data.messages.DataMessage") as DataService;
				return Begin(dataService);
			}
			else
				return DataServiceTransaction.CurrentDataServiceTransaction;
		}
		/// <summary>
		/// This version of the Begin method uses the default MessageBroker.
		/// </summary>
		/// <returns></returns>
		public static DataServiceTransaction Begin()
		{
			return Begin((string)null);
		}

		internal static DataServiceTransaction Begin(DataService dataService)
		{
			DataServiceTransaction dataServiceTransaction = new DataServiceTransaction(dataService);
			SetCurrentDataServiceTransaction(dataServiceTransaction);
			return dataServiceTransaction;
		}
		/// <summary>
		/// Marks the DataServiceTransaction so we rollback the transaction instead of committing it when it completes.
		/// </summary>
		public void SetRollbackOnly()
		{
			_rollbackOnly = true;
		}

        private void ProcessRefreshFills()
        {
            for (int i = 0; _refreshFills != null && i < _refreshFills.Count; i++)
            {
                RefreshFillData refreshFill = _refreshFills[i] as RefreshFillData;
                DataDestination dataDestination = _dataService.GetDestination(refreshFill.Destination) as DataDestination;
                if (dataDestination == null)
                    throw new FluorineException(__Res.GetString(__Res.Destination_NotFound, refreshFill.Destination));
                ICollection sequences = dataDestination.SequenceManager.GetSequences(refreshFill.Parameters);
                if (sequences != null)
                {
                    lock (dataDestination.SequenceManager.SyncRoot)
                    {
                        foreach (Sequence sequence in sequences)
                        {
                            DataMessage dataMessage = new DataMessage();
                            dataMessage.operation = DataMessage.FillOperation;
                            if (sequence.Parameters != null)
                                dataMessage.body = sequence.Parameters;
                            else
                                dataMessage.body = new object[0];
                            if (_clientId != null)
                                dataMessage.clientId = _clientId;
                            else
                                dataMessage.clientId = "srv:" + Guid.NewGuid().ToString("D");
                            IList result = dataDestination.ServiceAdapter.Invoke(dataMessage) as IList;
                            if (result.Count > 0)
                            {
                                Sequence sequenceTmp = dataDestination.SequenceManager.CreateSequence(dataMessage.clientId as string, result, sequence.Parameters, this);
                            }
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Clients can call this method to commit the transaction. You should only use this method if 
		/// you used the begin method to create the DataServiceTransaction. 
		/// Otherwise, the gateway will commit or rollback the transaction as necessary.
		/// </summary>
		public void Commit()
		{
			if( _rollbackOnly )
			{
				Rollback();
				return;
			}

			try
			{
                ProcessRefreshFills();

				_pushMessages = new ArrayList();
				for(int i = 0; i < _processedMessageBatches.Count; i++)
				{
					MessageBatch messageBatch = _processedMessageBatches[i] as MessageBatch;
					if( messageBatch.Messages != null && messageBatch.Messages.Count > 0 )
					{
						DataDestination dataDestination = _dataService.GetDestination(messageBatch.IncomingMessage) as DataDestination;
						try
						{
                            dataDestination.SequenceManager.ManageMessageBatch(messageBatch, this);
						}
						catch(Exception ex)
						{
							MessageException messageException = new MessageException(ex);
							ErrorMessage errorMessage = messageException.GetErrorMessage();
							errorMessage.correlationId = messageBatch.IncomingMessage.messageId;
							errorMessage.destination = messageBatch.IncomingMessage.destination;
							messageBatch.Messages.Clear();
							messageBatch.Messages.Add(errorMessage);
						}
						for(int j = 0; j < messageBatch.Messages.Count; j++)
						{
							IMessage message = messageBatch.Messages[j] as IMessage;

							if( !(message is ErrorMessage) )
								_pushMessages.Add(message);
						}
					}
					_outgoingMessages.AddRange(messageBatch.Messages);
				}
			
				for(int i = 0; i < _pushMessages.Count; i++)
				{
					IMessage message = _pushMessages[i] as IMessage;
					DataMessage dataMessage = message as DataMessage;
					if( dataMessage != null )
						PushMessage(GetSubscribers(message), message);
				}
				foreach(DictionaryEntry entry in _clientUpdateCollectionMessages)
				{
					UpdateCollectionMessage updateCollectionMessage = entry.Value as UpdateCollectionMessage;
					_outgoingMessages.Add(updateCollectionMessage);
					PushMessage(GetSubscribers(updateCollectionMessage), updateCollectionMessage);
				}
				foreach(DictionaryEntry entry in _updateCollectionMessages)
				{
					UpdateCollectionMessage updateCollectionMessage = entry.Value as UpdateCollectionMessage;
					_outgoingMessages.Add(updateCollectionMessage);
					PushMessage(GetSubscribers(updateCollectionMessage), updateCollectionMessage);
				}
			}
			finally
			{
				_transactionState = TransactionState.Committed;
			}
		}

		ICollection GetSubscribers(IMessage message)
		{
			MessageDestination destination = _dataService.GetDestination(message) as MessageDestination;
			SubscriptionManager subscriptionManager = destination.SubscriptionManager;
			IList subscribers = subscriptionManager.GetSubscribers();
			subscribers.Remove(message.clientId);
			return subscribers;
		}

		private void PushMessage(ICollection subscribers, IMessage message)
		{
			//Get subscribers here
			_dataService.PushMessageToClients(subscribers, message);
		}

		internal IList GetOutgoingMessages()
		{
			return _outgoingMessages;
		}

		/// <summary>
		/// Rollsback this transaction. 
		/// You should only use this method if you created the DataServiceTransaction with the Begin method. 
		/// </summary>
		public void Rollback()
		{
			try
			{
			}
			finally
			{
				_transactionState = TransactionState.RolledBack;
			}
		}
		/// <summary>
		/// Send an update event to clients subscribed to this message. Note that this method does not send 
		/// the change to the adapter/assembler - it assumes that the changes have already been applied 
		/// or are being applied. It only updates the clients with the new version of this data. 
		/// 
		/// You must supply a destination parameter and a new version of the object. If you supply a 
		/// non-null previous version, this object is used to detect conflicts on the client in case 
		/// the client's version of the data does not match the previous version. You may also supply 
		/// a list of property names that have changed as a hint to the client to indicate which properties 
		/// should be checked for conflicts and updated. If you supply null for the changes, all 
		/// properties on the client are updated. These property names do not accept any kind of dot 
		/// notation to specify that a property of a property has changed. Only top level property 
		/// names are allowed.
		/// </summary>
		/// <param name="destination">Name of the Data Management Services destination that is managing the item you want to update.</param>
		/// <param name="newVersion">New version of the item to update. The identity of the item is used to determine which item to update.</param>
		/// <param name="previousVersion">If not null, this contains a version of the item you intend to update. The client can detect a conflict if its version does not match the previousVersion. If you specify the value as null, a conflict is only detected if the client has pending changes for the item being updated.</param>
		/// <param name="changes">Array of property names which are to be updated. You can provide a null value to indicate that all property values may have changed.</param>
		public void UpdateItem(string destination, object newVersion, object previousVersion, string[] changes)
		{
            DataMessage dataMessage = new DataMessage();
            DataDestination dataDestination = _dataService.GetDestination(destination) as DataDestination;
            object[] body = new object[3];
            body[0] = changes;
            body[2] = newVersion;
            body[1] = previousVersion;
            dataMessage.operation = DataMessage.UpdateOperation;
            dataMessage.body = body;
            dataMessage.destination = destination;
            if (_clientId != null)
                dataMessage.clientId = _clientId;
            else
                dataMessage.clientId = "srv:" + Guid.NewGuid().ToString("D");
            dataMessage.identity = Identity.GetIdentity(newVersion, dataDestination);
            dataMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":" + _idCounter.ToString();
            System.Threading.Interlocked.Increment(ref _idCounter);
            ArrayList messages = new ArrayList(1);
            messages.Add(dataMessage);
            MessageBatch messageBatch = new MessageBatch(dataMessage, messages);
            _processedMessageBatches.Add(messageBatch);
        }
		/// <summary>
		/// You use this method to indicate to to the Data Management Service that a new item has been created. 
		/// The Data Management Service goes through all sequences currently being managed by clients and 
		/// determine whether this item belongs in each sequence. Usually it re-evaluates each fill method 
		/// to make this determination (though you can control how this is done for each fill method). 
		/// When it finds a sequence that contains this item, it then sends a create message for this 
		/// item to each client subscribed for that sequence.
		/// 
		/// Note that this method does not send a create message to the adapter/assembler for this item. 
		/// It assumes that your backend database has already been updated with the data or is being 
		/// updated in this transaction. If this transaction is rolled back, no changes are applied. 
		/// </summary>
		/// <param name="destination">Name of the destination that is to be managing this newly created item.</param>
		/// <param name="item">New item to create.</param>
		public void CreateItem(string destination, object item)
		{
            DataMessage dataMessage = new DataMessage();
            DataDestination dataDestination = _dataService.GetDestination(destination) as DataDestination;
            dataMessage.operation = DataMessage.CreateOperation;
            dataMessage.body = item;
            dataMessage.destination = destination;
            if (_clientId != null)
                dataMessage.clientId = _clientId;
            else
                dataMessage.clientId = "srv:" + Guid.NewGuid().ToString("D");
            dataMessage.identity = Identity.GetIdentity(item, dataDestination);
            dataMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":" + _idCounter.ToString();
            System.Threading.Interlocked.Increment(ref _idCounter);
            ArrayList messages = new ArrayList(1);
            messages.Add(dataMessage);
            MessageBatch messageBatch = new MessageBatch(dataMessage, messages);
            AddProcessedMessageBatch(messageBatch);
        }
		/// <summary>
		/// Sends a deleteItem method to the clients that are sync'd to sequences that contain this item. 
		/// It does not send a delete message to the adapter/assembler but instead assumes that this 
		/// item is already have been deleted from the database or is being deleted in this transaction. 
		/// If you rollback the transaction, this message is also rolled back.
		/// 
		/// This version of the delete method causes clients to generate a conflict if they have a version 
		/// of the item that does not match the version of the item specified. You can use the 
		/// DeleteItemWithId method to unconditionally delete an item on the client if you do not have 
		/// the original version. 
		/// </summary>
		/// <param name="destination">Name of the destination containing the item to be deleted.</param>
		/// <param name="item">Version of the item to delete. Clients can detect a conflict if this version of the item does not match the version they are currently managing.</param>
		public void DeleteItem(string destination, object item)
		{
            DataMessage dataMessage = new DataMessage();
            DataDestination dataDestination = _dataService.GetDestination(destination) as DataDestination;
            dataMessage.operation = DataMessage.DeleteOperation;
            dataMessage.body = item;
            dataMessage.destination = destination;
            if (_clientId != null)
                dataMessage.clientId = _clientId;
            else
                dataMessage.clientId = "srv:" + Guid.NewGuid().ToString("D");
            dataMessage.identity = Identity.GetIdentity(item, dataDestination);
            dataMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":" + _idCounter.ToString();
            System.Threading.Interlocked.Increment(ref _idCounter);

            ArrayList messages = new ArrayList(1);
            messages.Add(dataMessage);
            MessageBatch messageBatch = new MessageBatch(dataMessage, messages);
            AddProcessedMessageBatch(messageBatch);
        }
		/// <summary>
		/// This version of the deleteItem method does not provide for conflict detection if the item has been modified before the delete occurs; it is deleted.
		/// </summary>
		/// <param name="destination">Name of the destination containing the item to be deleted.</param>
		/// <param name="identity">A Hashtable containing entries for each of the id properties for this item (the key is the id property name, the value is its value).</param>
		public void DeleteItemWithId(string destination, Hashtable identity)
		{
            DataMessage dataMessage = new DataMessage();
            DataDestination dataDestination = _dataService.GetDestination(destination) as DataDestination;
            dataMessage.operation = DataMessage.DeleteOperation;
            dataMessage.body = null;
            dataMessage.destination = destination;
            if (_clientId != null)
                dataMessage.clientId = _clientId;
            else
                dataMessage.clientId = "srv:" + Guid.NewGuid().ToString("D");
            dataMessage.identity = identity;
            dataMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":" + _idCounter.ToString();
            System.Threading.Interlocked.Increment(ref _idCounter);

            ArrayList messages = new ArrayList(1);
            messages.Add(dataMessage);
            MessageBatch messageBatch = new MessageBatch(dataMessage, messages);
            AddProcessedMessageBatch(messageBatch);
        }
		/// <summary>
		/// For a matching list of auto-sync'd fills, re-executes the fill method, compares the identities 
		/// of the items returned to the those returned the last time we executed it with 
		/// AutoSyncEnabled=true. It builds an update collection events for any fills that have changed. 
		/// This contains the items that have been added to or removed from the list but does not look 
		/// for changes made to the properties of those items. This update collection message is sent to 
		/// clients along with the other messages in this transaction when you commit. If the transaction 
		/// is rolled back, the fills are not updated. 
		/// 
		/// If you want to update the property values of items, you'll need to use updateItem on the individual items that have changed. 
		/// 
		/// If you provide null for the fill parameters argument, all auto-sync'd fills are refreshed. 
		/// If you provide a list of fill parameters, we match that list of fill parameters against the 
		/// list provided by the clients when they executed the fills. If the fill parameters match, that 
		/// fill is refreshed. The matching algorithm works as follows. If you provide a value for a given 
		/// fill parameter, the equals method is used on it to compare against the fill parameter value 
		/// that the client used when it executed the fill. If you provide a null parameter value, 
		/// it matches that slot for all fills.
		/// </summary>
		/// <param name="destination">Destination on which the desired fills were created against.</param>
		/// <param name="fillParameters">Fill parameter pattern that defines the fills to be refreshed.</param>
        public void RefreshFill(string destination, IList fillParameters)
		{
            if (_refreshFills == null)
                _refreshFills = new ArrayList(1);
            _refreshFills.Add(new RefreshFillData(destination, fillParameters));
		}
		/// <summary>
		/// When you call the updateItem, createItem, and the deleteItem methods, normally these messages 
		/// are sent to other peers in the cluster so they are distributed by those nodes to clients 
		/// connected to them. If your code is arranging to send these updates from each instance in 
		/// the cluster, set SendMessagesToPeers=false before you call the updateItem, createItem method, 
		/// and so forth. 
		/// </summary>
		public bool SendMessagesToPeers
		{
			get{ return _sendMessagesToPeers; }
			set{ _sendMessagesToPeers = value; }
		}

		internal void AddProcessedMessageBatch(MessageBatch messageBatch)
		{
			_processedMessageBatches.Add(messageBatch);
		}

		internal void AddClientUpdateCollection(UpdateCollectionMessage updateCollectionMessage)
		{
			_clientUpdateCollectionMessages[updateCollectionMessage.collectionId] = updateCollectionMessage;
		}

		internal void GenerateUpdateCollectionMessage(int updateType, DataDestination dataDestination, Sequence sequence, int position, Identity identity)
		{
			UpdateCollectionMessage updateCollectionMessage = CreateUpdateCollectionMessage(dataDestination, sequence);
			updateCollectionMessage.AddItemIdentityChange(updateType, position, identity);
            if (updateCollectionMessage.collectionId != null)
                _updateCollectionMessages[updateCollectionMessage.collectionId] = updateCollectionMessage;
            else
            {
                //without fill parameters
                _updateCollectionMessages[new object[0]] = updateCollectionMessage;
            }
		}

		private UpdateCollectionMessage CreateUpdateCollectionMessage(DataDestination dataDestination, Sequence sequence)
		{
			UpdateCollectionMessage updateCollectionMessage = new UpdateCollectionMessage();
			updateCollectionMessage.clientId = this.ClientId;
			updateCollectionMessage.updateMode = UpdateCollectionMessage.ServerUpdate;
			// The unique identifier for the collection that was updated. For a collection filled with the 
			// DataService.fill() method this contains an Array of the parameters specified.
			updateCollectionMessage.collectionId = sequence.Parameters;
			updateCollectionMessage.destination = dataDestination.Id;
			updateCollectionMessage.correlationId = this.CorrelationId;
			updateCollectionMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":" + _idCounter.ToString();
			System.Threading.Interlocked.Increment(ref _idCounter);

			return updateCollectionMessage;
		}

	}
}
