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
// Import log4net classes.
using log4net;
using log4net.Config;

using FluorineFx.Util;
using FluorineFx.Collections;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Config;
using FluorineFx.Messaging.Services;
using FluorineFx.Messaging.Messages;
using FluorineFx.Data.Messages;
using FluorineFx.Data.Assemblers;

namespace FluorineFx.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SequenceManager
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(SequenceManager));

		object _objLock = new object();
		DataDestination _dataDestination;

        CopyOnWriteDictionary _sequenceIdToSequenceHash;
        Hashtable        _parametersToSequenceIdHash;
		Hashtable		_itemIdToSequenceIdMapHash;
		Hashtable		_itemIdToItemHash;
		Hashtable		_clientIdToSequenceHash;

		public SequenceManager(DataDestination dataDestination)
		{
			_dataDestination = dataDestination;
            _sequenceIdToSequenceHash = new CopyOnWriteDictionary();
#if (NET_1_1)
			_parametersToSequenceIdHash = new Hashtable(new ListHashCodeProvider(), new ListComparer());
#else
            _parametersToSequenceIdHash = new Hashtable(new ListHashCodeProvider());
#endif
            _itemIdToSequenceIdMapHash = new Hashtable();
			_clientIdToSequenceHash = new Hashtable();
			_itemIdToItemHash = new Hashtable();
		}


        public object SyncRoot { get { return _objLock; } }

		private Sequence[] GetSequences()
		{
			lock(_objLock)
			{
				ArrayList result = new ArrayList(_sequenceIdToSequenceHash.Values);
				return result.ToArray(typeof(Sequence)) as Sequence[];
			}
		}

		private Sequence GetSequence(int sequenceId)
		{
			lock(_objLock)
			{
				return _sequenceIdToSequenceHash[sequenceId] as Sequence;
			}
		}

		public Sequence GetSequence(IList fillParameters)
		{
			lock(_objLock)
			{
                if (fillParameters != null)
                {
                    if (_parametersToSequenceIdHash.Contains(fillParameters))
                        return _parametersToSequenceIdHash[fillParameters] as Sequence;
                }
                else
                {
                    IDictionary sequenceIdMap = _itemIdToSequenceIdMapHash[new Identity(fillParameters)] as IDictionary;
                    if (sequenceIdMap != null)
                    {
                        foreach (Sequence sequence in sequenceIdMap.Values)
                        {
                            if (sequence.Parameters == null)
                                return sequence;
                        }
                    }
                }
				return null;
			}
		}

        /// <summary>
        /// Fill parameters are used to create a matching list of fills that are currently being cached by active clients.
        /// This list can be null, which means that all fills on that destination match.
        /// If the list is non-null, matches fills made by clients with the same number of parameters if all of the slots match 
        /// </summary>
        /// <param name="fillParameters"></param>
        /// <returns></returns>
        public ICollection GetSequences(IList fillParameters)
        {
            lock (_objLock)
            {
                if (fillParameters != null)
                {                    
                    Sequence sequence = _parametersToSequenceIdHash[fillParameters] as Sequence;
                    if (sequence != null)
                    {
                        ArrayList result = new ArrayList(1);
                        result.Add(sequence);
                        return result;
                    }
                }
                else
                {
                    return _sequenceIdToSequenceHash.Values;
                }
            }
            return null;
        }

		private void RemoveSequence(int sequenceId)
		{
			lock(_objLock)
			{
				Sequence sequence = GetSequence(sequenceId);
				if( sequence != null )
				{
					if( log.IsDebugEnabled )
                        log.Debug(__Res.GetString(__Res.SequenceManager_Remove, sequence.Id));

					for(int i = sequence.Count-1; i >= 0; i--)
					{
						Identity identity = sequence[i];
						RemoveIdentityFromSequence(sequence, identity, i, null);

					}
					if( sequence.Parameters != null )
						_parametersToSequenceIdHash.Remove(sequence.Parameters);

					_sequenceIdToSequenceHash.Remove(sequenceId);//clear entry

					if( log.IsDebugEnabled )
						log.Debug(__Res.GetString(__Res.SequenceManager_RemoveStatus, _dataDestination.Id, _sequenceIdToSequenceHash.Count));
				}
			}
		}

		public int AddIdentityToSequence(Sequence sequence, int position, Identity identity, DataServiceTransaction dataServiceTransaction)
		{
			lock(_objLock)
			{
				if(position == -1 || position > sequence.Size)
					position = sequence.Add(identity);
				else
					sequence.Insert(position, identity);

                IDictionary sequenceIdMap = _itemIdToSequenceIdMapHash[identity] as IDictionary;
				if( sequenceIdMap == null )
				{
					sequenceIdMap = new Hashtable();
					_itemIdToSequenceIdMapHash[identity] = sequenceIdMap;
				}
				sequenceIdMap[sequence.Id] = sequence;

				if(dataServiceTransaction != null)
					dataServiceTransaction.GenerateUpdateCollectionMessage(UpdateCollectionRange.InsertIntoCollection, _dataDestination, sequence, position, identity);

				return position;
			}
		}

		public int AddIdentityToSequence(Sequence sequence, Identity identity, DataServiceTransaction dataServiceTransaction)
		{
			return AddIdentityToSequence(sequence, -1, identity, dataServiceTransaction);
		}

		public void RemoveIdentityFromSequence(Sequence sequence, Identity identity, DataServiceTransaction dataServiceTransaction)
		{
			RemoveIdentityFromSequence(sequence, identity, sequence.IndexOf(identity), dataServiceTransaction);
		}

		public void RemoveIdentityFromSequence(Sequence sequence, Identity identity, int position, DataServiceTransaction dataServiceTransaction)
		{
			if( position == -1 )
				return;
			lock(_objLock)
			{
                IDictionary sequenceIdMap = _itemIdToSequenceIdMapHash[identity] as IDictionary;
                if (sequenceIdMap != null)
                {
                    sequenceIdMap.Remove(sequence.Id);
                    //Release the item if it does'n occur in any sequence
                    if (sequenceIdMap.Count == 0)
                    {
                        _itemIdToItemHash.Remove(identity);
                        _itemIdToSequenceIdMapHash.Remove(identity);
                    }
                    if (sequence[position].Equals(identity))
                        sequence.RemoveAt(position);
                    else
                        sequence.Remove(identity);

                    if (dataServiceTransaction != null)
                        dataServiceTransaction.GenerateUpdateCollectionMessage(UpdateCollectionRange.DeleteFromCollection, _dataDestination, sequence, position, identity);
                }
                else
                {
                    _itemIdToItemHash.Remove(identity);
                    sequence.Remove(identity);
                }
			}
		}

		private ItemWrapper GetItem(Identity identity)
		{
			lock(_objLock)
			{
				return _itemIdToItemHash[identity] as ItemWrapper;
			}
		}

		public Sequence CreateSequence(string clientId, IList result, IList parameters, DataServiceTransaction dataServiceTransaction)
		{
			Sequence sequence = null;
			Identity[] identities = new Identity[result.Count];

			lock(_objLock)
			{
				for(int i = 0; i < identities.Length; i++)
				{
					if( result[i] != null )
					{
						Identity identity = Identity.GetIdentity(result[i], _dataDestination);
						identities[i] = identity;
						if( ! _itemIdToItemHash.ContainsKey(identity) )
							_itemIdToItemHash.Add(identity, new ItemWrapper(result[i]));
						else
						{
							ItemWrapper itemWrapper = _itemIdToItemHash[identity] as ItemWrapper;
							itemWrapper.Instance = result[i];
						}
					}
				}
				//Lookup existing sequence
                if (parameters != null)
                {
                    if (_parametersToSequenceIdHash.Contains(parameters))
                        sequence = _parametersToSequenceIdHash[parameters] as Sequence;
                }
                else
                {
                    IDictionary sequenceIdMap = _itemIdToSequenceIdMapHash[identities[0]] as IDictionary;
                    if (sequenceIdMap != null)
                    {
                        foreach (Sequence sequenceTmp in sequenceIdMap.Values)
                        {
                            if (sequenceTmp.Parameters == null)
                            {
                                sequence = sequenceTmp;
                                break;
                            }
                        }
                    }
                }
                //if (parameters == null)
                //    parameters = new ArrayList();
				
				if( sequence == null )
				{
					sequence = new Sequence();
					sequence.Id = sequence.GetHashCode();

                    object[] parametersArray = null;
                    if (parameters != null)
                    {
                        parametersArray = new object[parameters.Count];
                        parameters.CopyTo(parametersArray, 0);
                        sequence.Parameters = parametersArray;
                        _parametersToSequenceIdHash[parameters] = sequence;
                    }

					for(int i = 0; i < identities.Length; i++)
					{
						Identity identity = identities[i];
						AddIdentityToSequence(sequence, identity, dataServiceTransaction);
					}

					_sequenceIdToSequenceHash[sequence.Id] = sequence;

					if( log.IsDebugEnabled )
						log.Debug(__Res.GetString(__Res.SequenceManager_CreateSeq, sequence.Id, clientId));

				}
				else
				{
					for(int i = 0; i < identities.Length; i++)
					{
						Identity identity = identities[i];
						Identity existingIdentity = null;
						if( i < sequence.Count )
							existingIdentity = sequence[i];
						if( !identity.Equals(existingIdentity) )
						{
							//Identity not found in sequence
							if( !sequence.Contains(identity) )
							{
								int position = AddIdentityToSequence(sequence, identity, dataServiceTransaction);
							}
						}
					}
				}
				sequence.AddSubscriber(clientId);
				ArrayList sequences;
				if( _clientIdToSequenceHash.Contains(clientId) )
					sequences = _clientIdToSequenceHash[clientId] as ArrayList;
				else
				{
					sequences = new ArrayList();
					_clientIdToSequenceHash[clientId] = sequences;
				}
				if( !sequences.Contains(sequence) )
					sequences.Add(sequence);
			}
			return sequence;
		}

		public AcknowledgeMessage ManageSequence(DataMessage dataMessage, IList items)
		{
			return ManageSequence(dataMessage, items, null);
		}

		public AcknowledgeMessage ManageSequence(DataMessage dataMessage, IList items, DataServiceTransaction dataServiceTransaction)
		{
			AcknowledgeMessage acknowledgeMessage = null;
			switch(dataMessage.operation)
			{
				case DataMessage.FillOperation:
				{
					Sequence sequence = CreateSequence(dataMessage.clientId as string, items, dataMessage.body as IList, dataServiceTransaction);
					acknowledgeMessage = GetSequencedMessage(dataMessage, sequence);
				}
					break;
				case DataMessage.GetOperation:
				case DataMessage.GetSequenceIdOperation:
				{
					Sequence sequence = CreateSequence(dataMessage.clientId as string, items, null, dataServiceTransaction);
					acknowledgeMessage = GetSequencedMessage(dataMessage, sequence);
				}
					break;
				default:
				{
					if( log != null && log.IsErrorEnabled )
						log.Error(__Res.GetString(__Res.SequenceManager_Unknown, dataMessage.operation));
				}
					break;
			}
			return acknowledgeMessage;
		}

        public void ManageMessageBatch(MessageBatch messageBatch, DataServiceTransaction dataServiceTransaction)
		{
			DataMessage dataMessage = messageBatch.IncomingMessage;
			//Manage existing sequences
			for(int j = 0; j < messageBatch.Messages.Count; j++)
			{
				IMessage message = messageBatch.Messages[j] as IMessage;
				if( message is UpdateCollectionMessage )
				{
					UpdateCollectionMessage updateCollectionMessage = message as UpdateCollectionMessage;
					//update collections, fix sequences
					IList fillParameters = updateCollectionMessage.collectionId as IList;
					Sequence sequence = _dataDestination.SequenceManager.GetSequence(fillParameters);
					if( sequence != null )
					{
						ApplyUpdateCollectionMessage(sequence, updateCollectionMessage);
					}
				}
			}
            for (int j = 0; j < messageBatch.Messages.Count; j++)
            {
                DataMessage dataMessageTmp = messageBatch.Messages[j] as DataMessage;
                if (dataMessageTmp != null)
                {
                    switch (dataMessageTmp.operation)
                    {
                        case DataMessage.CreateAndSequenceOperation:
                            {
                                //dataMessage.identity contains identity
                                //dataMessage.body contains the object
                                IList result = new ArrayList();
                                result.Add(dataMessageTmp.body);
                                //Will generate an UpdateCollectionMessage too (server adding item to collection)
                                Sequence sequence = this.CreateSequence(dataMessageTmp.clientId as string, result, null, dataServiceTransaction);
                                SequencedMessage sequencedMessage = this.GetSequencedMessage(dataMessageTmp, sequence);
                                messageBatch.Messages[j] = sequencedMessage;
                            }
                            break;
                    }
                }
            }
            for (int j = 0; j < messageBatch.Messages.Count; j++)
			{
                if (messageBatch.Messages[j] is DataMessage)
                {
                    DataMessage dataMessageTmp = messageBatch.Messages[j] as DataMessage;
                    SyncSequenceChanges(dataMessageTmp, dataServiceTransaction);
                }
                if (messageBatch.Messages[j] is SequencedMessage)
                {
                    SequencedMessage sequencedMessage = messageBatch.Messages[j] as SequencedMessage;
                    DataMessage dataMessageTmp = sequencedMessage.dataMessage;
                    SyncSequenceChanges(dataMessageTmp, dataServiceTransaction);
                }
            }
		}

		void ApplyUpdateCollectionMessage(Sequence sequence, UpdateCollectionMessage updateCollectionMessage)
		{
			IList updateCollectionRanges = updateCollectionMessage.body as IList;
			for(int k = 0; k < updateCollectionRanges.Count; k++)
			{
				UpdateCollectionRange updateCollectionRange = updateCollectionRanges[k] as UpdateCollectionRange;
				int insertCount = 0;
				for(int l = 0; l < updateCollectionRange.identities.Length; l++)
				{
					Identity identity = updateCollectionRange.identities[l] as Identity;
					if( identity == null )
					{
                        identity = new Identity(updateCollectionRange.identities[l] as IDictionary);
					}
					if( updateCollectionRange.updateType == UpdateCollectionRange.InsertIntoCollection )
					{
						this.AddIdentityToSequence(sequence, updateCollectionRange.position + insertCount, identity, null);
						insertCount++;
					}
					if( updateCollectionRange.updateType == UpdateCollectionRange.DeleteFromCollection )
						this.RemoveIdentityFromSequence(sequence, identity, updateCollectionRange.position, null);
				}
			}
		}

		void SyncSequenceChanges(DataMessage dataMessage, DataServiceTransaction dataServiceTransaction)
		{
			lock(_objLock)
			{
                ArrayList sequenceList = new ArrayList(_sequenceIdToSequenceHash.Values.Count);
                sequenceList.AddRange(_sequenceIdToSequenceHash.Values);//Hashtable may be changed here
                foreach (Sequence sequence in sequenceList)
				{
					switch(dataMessage.operation)
					{
						case DataMessage.CreateOperation:
						case DataMessage.CreateAndSequenceOperation:
							RefreshSequence(sequence, dataMessage, dataMessage.body, dataServiceTransaction);
							break;
						case DataMessage.DeleteOperation:
						{
							//RefreshSequence(sequence, dataMessage, dataMessage.body, dataServiceTransaction);
							Identity identity = Identity.GetIdentity(dataMessage.body, _dataDestination);
							int index = sequence.IndexOf(identity);
							if( index != -1 )
								RemoveIdentityFromSequence(sequence, identity, dataServiceTransaction);
						}
							break;
						case DataMessage.UpdateOperation:
							RefreshSequence(sequence, dataMessage, (dataMessage.body as IList)[2], dataServiceTransaction);
							break;
					}
				}
			}
		}

		public Sequence RefreshSequence(Sequence sequence, DataMessage dataMessage, object item, DataServiceTransaction dataServiceTransaction)
		{
            if (sequence.Parameters == null)
                return sequence;
			DotNetAdapter dotNetAdapter = _dataDestination.ServiceAdapter as DotNetAdapter;
			if( dotNetAdapter != null )
			{
				bool isCreate = (dataMessage.operation == DataMessage.CreateOperation || dataMessage.operation == DataMessage.CreateAndSequenceOperation);
				int fill = dotNetAdapter.RefreshFill( sequence.Parameters, item, isCreate );
				switch(fill)
				{
					case Assembler.ExecuteFill:
					{
						IList parameters = sequence.Parameters;
                        //if (parameters == null)
                        //    parameters = new object[0];
						DataMessage fillDataMessage = new DataMessage();
						fillDataMessage.clientId = dataMessage.clientId;
						fillDataMessage.operation = DataMessage.FillOperation;
                        fillDataMessage.body = parameters != null ? parameters : new object[0];
						IList result = _dataDestination.ServiceAdapter.Invoke(fillDataMessage) as IList;
                        return CreateSequence(dataMessage.clientId as string, result, parameters, dataServiceTransaction);
					} 
					case Assembler.AppendToFill:
					{
						Identity identity = Identity.GetIdentity(item, _dataDestination);
						if( !sequence.Contains(identity) )
							AddIdentityToSequence(sequence, identity, dataServiceTransaction);
                        _itemIdToItemHash[identity] = new ItemWrapper(item);
					}
						break;
					case Assembler.RemoveFromFill:
					{
						Identity identity = Identity.GetIdentity(item, _dataDestination);
						if( sequence.Contains(identity) )
							RemoveIdentityFromSequence(sequence, identity, dataServiceTransaction);
					}
						break;
					case Assembler.DoNotExecuteFill:
						break;
				}
			}
			return sequence;
		}

		public SequencedMessage GetSequencedMessage(DataMessage dataMessage, Sequence sequence)
		{
			if( dataMessage.headers != null && dataMessage.headers.ContainsKey(DataMessage.PageSizeHeader) )
			{
				return GetPagedMessage(dataMessage, sequence);
			}
			else
			{
				SequencedMessage sequencedMessage = new SequencedMessage();
				sequencedMessage.destination = dataMessage.destination;
				sequencedMessage.sequenceId = sequence.Id;
				sequencedMessage.sequenceSize = sequence.Size;
				//object[] body = new object[result.Count];
				//result.CopyTo(body, 0);
				object[] body = new object[sequence.Count];
				lock(_objLock)
				{
					for(int i = 0; i < sequence.Count; i++)
					{
						ItemWrapper itemWrapper = GetItem(sequence[i]) as ItemWrapper;
						if( itemWrapper != null )
							body[i] = itemWrapper.Instance;
					}
				}
				sequencedMessage.body = body;
				sequencedMessage.sequenceProxies = null;
				sequencedMessage.dataMessage = dataMessage;

				sequencedMessage.messageId = dataMessage.messageId;
				sequencedMessage.clientId = dataMessage.clientId;
				sequencedMessage.correlationId = dataMessage.messageId;
				//dataMessage.identity = new Hashtable(0);

				return sequencedMessage;
			}
		}

		public SequencedMessage GetPageItems(DataMessage dataMessage)
		{
			int sequenceId = (int)dataMessage.headers[DataMessage.SequenceIdHeader];
			Sequence sequence = GetSequence(sequenceId);
            if (sequence != null)
            {
                IList DSids = dataMessage.headers["DSids"] as IList;
                //ArrayList items = new ArrayList(DSids.Count);
                SequencedMessage sequencedMessage = new SequencedMessage();
                object[] items = new object[DSids.Count];
                lock (_objLock)
                {
                    for (int i = 0; i < DSids.Count; i++)
                    {
                        Identity identity = new Identity(DSids[i] as IDictionary);
                        ItemWrapper itemWrapper = GetItem(identity);
                        //items.Add(item);
                        items[i] = itemWrapper.Instance;
                    }

                    sequencedMessage.destination = dataMessage.destination;
                    sequencedMessage.sequenceId = sequence.Id;
                    sequencedMessage.sequenceSize = sequence.Size;
                    sequencedMessage.sequenceProxies = null;

                    sequencedMessage.body = items;
                }
                return sequencedMessage;
            }
            else
            {
                DataServiceException dse = new DataServiceException(string.Format("Sequence {0} in destination {1} was not found", sequenceId, dataMessage.destination));
                throw dse;
            }
		}

		public void ReleaseCollectionOperation(DataMessage dataMessage)
		{
			lock(_objLock)
			{
				int sequenceId = (int)dataMessage.headers[DataMessage.SequenceIdHeader];
				if( log != null && log.IsDebugEnabled )
					log.Debug(__Res.GetString(__Res.SequenceManager_ReleaseCollection, sequenceId, dataMessage.clientId));

				Sequence sequence = GetSequence(sequenceId);
				IList parameters = dataMessage.body as IList;
				RemoveSubscriberFromSequence(dataMessage.clientId as string, sequence);
			}
		}

		public void ReleaseItemOperation(DataMessage dataMessage)
		{
            //TODO
			int sequenceId = (int)dataMessage.headers[DataMessage.SequenceIdHeader];
			Sequence sequence = GetSequence(sequenceId);
		}

		public void RemoveSubscriber(string clientId)
		{
			if( log.IsDebugEnabled )
				log.Debug(__Res.GetString(__Res.SequenceManager_RemoveSubscriber, clientId));
			lock(_objLock)
			{
				if( _clientIdToSequenceHash.Contains(clientId) )
				{
					ArrayList sequences = _clientIdToSequenceHash[clientId] as ArrayList;
					for(int i = 0; i < sequences.Count; i++)
					{
						Sequence sequence = sequences[i] as Sequence;
						sequence.RemoveSubscriber(clientId);
						//Delete the sequence if there are no subscribers left
						if( sequence.SubscriberCount == 0 )
						{
							if( log.IsDebugEnabled )
								log.Debug(__Res.GetString(__Res.SequenceManager_RemoveEmptySeq, sequence.Id));
							RemoveSequence(sequence.Id);
						}
					}
					_clientIdToSequenceHash.Remove(clientId);
				}
			}
		}

		public void RemoveSubscriberFromSequence(string clientId, Sequence sequence)
		{
			if( sequence != null )
			{
				if( log.IsDebugEnabled )
					log.Debug(__Res.GetString(__Res.SequenceManager_RemoveSubscriberSeq, clientId, sequence.Id));
				lock(_objLock)
				{
					if( _clientIdToSequenceHash.Contains(clientId) )
					{
						ArrayList sequences = _clientIdToSequenceHash[clientId] as ArrayList;
						for(int i = 0; i < sequences.Count; i++)
						{
							Sequence sequenceTmp = sequences[i] as Sequence;
							if( sequence == sequenceTmp )
							{
								sequence.RemoveSubscriber(clientId);
								//Delete the sequence if there are no subscribers left
								if( sequence.SubscriberCount == 0 )
								{
									if( log.IsDebugEnabled )
                                        log.Debug(__Res.GetString(__Res.SequenceManager_RemoveEmptySeq, sequence.Id));
									RemoveSequence(sequence.Id);
								}
								sequences.RemoveAt(i);//remove this sequence from client's list
								break;
							}
						}
					}
				}
			}
		}

		private UpdateCollectionMessage CreateUpdateCollectionMessage(DataMessage dataMessage, Sequence sequence, Identity identity, int position, int updateMode)
		{
			UpdateCollectionMessage updateCollectionMessage = new UpdateCollectionMessage();
			// The unique identifier for the collection that was updated. For a collection filled with the 
			// DataService.fill() method this contains an Array of the parameters specified.
			updateCollectionMessage.collectionId = sequence.Parameters;
			updateCollectionMessage.destination = dataMessage.destination;
			updateCollectionMessage.replace = false;
			updateCollectionMessage.updateMode = updateMode;
			updateCollectionMessage.messageId = "srv:" + Guid.NewGuid().ToString("D") + ":0";
			updateCollectionMessage.correlationId = dataMessage.correlationId;

			UpdateCollectionRange updateCollectionRange = new UpdateCollectionRange();
			// An Array of identity objects that represent which items were either deleted or inserted in the 
			// associated collection starting at the position indicated by the position property
			updateCollectionRange.identities = new object[1];
			//(updateCollectionRange.identities as IList).Add( identity );
			(updateCollectionRange.identities as object[])[0] = identity;
			updateCollectionRange.updateType = UpdateCollectionRange.InsertIntoCollection;
			updateCollectionRange.position = position;
					
			//ArrayList body = new ArrayList();
			//body.Add(updateCollectionRange);
			object[] body = new object[1]; body[0] = updateCollectionRange;
			updateCollectionMessage.body = body;
			return updateCollectionMessage;
		}

		public PagedMessage GetPagedMessage(DataMessage dataMessage, Sequence sequence)
		{
            int pageSize = (int)dataMessage.headers[DataMessage.PageSizeHeader];
            int pageIndex = 0;
            if (dataMessage.headers.ContainsKey(DataMessage.PageIndexHeader))
                pageIndex = (int)dataMessage.headers[DataMessage.PageIndexHeader];
            pageIndex = Math.Max(0, pageIndex);//negative pageIndex???
            int pageCount = (int)Math.Ceiling((double)sequence.Size / pageSize);
            int pageStart = pageIndex * pageSize;
            int pageEnd = Math.Min(pageStart + pageSize, sequence.Size);

            PagedMessage pagedMessage = new PagedMessage();
            pagedMessage.pageIndex = pageIndex;
            pagedMessage.pageCount = pageCount;
            pagedMessage.sequenceSize = sequence.Size;
            pagedMessage.sequenceId = sequence.Id;
            object[] pagedResult = new object[pageEnd - pageStart];
            lock (_objLock)
            {
                for (int i = pageStart; i < pageEnd; i++)
                {
                    Identity identity = sequence[i];
                    //pagedResult.Add( _itemIdToItemHash[identity] );
                    if (_itemIdToItemHash.Contains(identity))
                        pagedResult[i - pageStart] = (_itemIdToItemHash[identity] as ItemWrapper).Instance;
                }
            }
            pagedMessage.body = pagedResult;
            pagedMessage.destination = dataMessage.destination;
            pagedMessage.dataMessage = dataMessage;
            return pagedMessage;
		}

		public PagedMessage GetPage(DataMessage dataMessage)
		{
			int sequenceId = (int)dataMessage.headers[DataMessage.SequenceIdHeader];
			Sequence sequence = GetSequence(sequenceId);
            if (sequence != null)
                return GetPagedMessage(dataMessage, sequence);
            else
            {
                DataServiceException dse = new DataServiceException(string.Format("Sequence {0} in destination {1} was not found", sequenceId, dataMessage.destination));
                throw dse;
            }
		}

		internal void Dump(DumpContext dumpContext)
		{
			dumpContext.AppendLine("SequenceManager, Items count = " + _itemIdToItemHash.Count.ToString() + ", Subscribers = " + _clientIdToSequenceHash.Count.ToString());
			Sequence[] sequences = this.GetSequences();
			if( sequences.Length > 0 )
			{
				dumpContext.AppendLine("Sequences");
				foreach(Sequence sequence in sequences)
				{
					dumpContext.Indent();
					sequence.Dump(dumpContext);
					dumpContext.Unindent();
				}
			}
			else
			{
				//dumpContext.AppendLine("No sequences");
			}
		}
	}
}
