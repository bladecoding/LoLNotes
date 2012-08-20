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
using log4net;
using FluorineFx.AMF3;
using FluorineFx.Data;
using FluorineFx.Data.Messages;
using FluorineFx.Data.Assemblers;
using FluorineFx.Messaging;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Services;

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class DotNetAdapter : ServiceAdapter
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(DotNetAdapter));

		public DotNetAdapter()
		{
		}

		public override object Invoke(IMessage message)
		{
			object result = null;
			DataMessage dataMessage = message as DataMessage;
			switch(dataMessage.operation)
			{
				case DataMessage.FillOperation:
					result = Fill(dataMessage);
					break;
				case DataMessage.UpdateOperation:
					result = Update(dataMessage);
					break;
				case DataMessage.CreateOperation:
				case DataMessage.CreateAndSequenceOperation:
					result = Create(dataMessage);
					break;
				case DataMessage.DeleteOperation:
					result = Delete(dataMessage);
					break;
                //case DataMessage.GetSequenceIdOperation://Handled in DataService
                //    result = Fill(dataMessage);
                //    break;
                case DataMessage.GetOperation:
					result = GetItem(dataMessage);
					break;
				case DataMessage.BatchedOperation:
					result = Batch(dataMessage);
					break;
				default:
					if(log != null && log.IsErrorEnabled)
						log.Error(__Res.GetString(__Res.DataService_Unknown, dataMessage.operation));
					break;
			}
			return result;
		}

		private IAssembler GetAssembler()
		{
			//Hashtable properties = this.DestinationSettings.Properties;
			//string assemblerTypeName = properties["source"] as string;
            //IAssembler assembler = ObjectFactory.CreateInstance(assemblerTypeName) as IAssembler;
            //return assembler;
            FactoryInstance factoryInstance = this.Destination.GetFactoryInstance();
            object instance = factoryInstance.Lookup();
            return instance as IAssembler;
		}

		public bool AutoRefreshFill(IList parameters)
		{
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " AutoRefreshFill");
				return assembler.AutoRefreshFill(parameters);
			}
			return false;
		}

		private IList Fill(DataMessage dataMessage)
		{
			IList result = null;
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				IList parameters = dataMessage.body as IList;
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " Fill");
				result = assembler.Fill(parameters);
				return result;
			}
			return null;
		}

		private object GetItem(DataMessage dataMessage)
		{
			object result = null;
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " GetItem");
				result = assembler.GetItem(dataMessage.identity);
				return result;
			}
			return null;
		}

		private IMessage Update(DataMessage dataMessage)
		{
			IList parameters = dataMessage.body as IList;
			IList changeObject = parameters[0] as IList;
			if(changeObject == null || changeObject.Count == 0)
				return dataMessage;

			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " Update");
				assembler.UpdateItem(parameters[2], parameters[1], parameters[0] as IList);
			}
			return dataMessage;
		}

		private IMessage Create(DataMessage dataMessage)
		{
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " CreateItem");
				assembler.CreateItem(dataMessage.body);
				Identity identity = Identity.GetIdentity(dataMessage.body, this.Destination as DataDestination);
				dataMessage.identity = identity;
			}
			return dataMessage;
		}

		private IMessage Delete(DataMessage dataMessage)
		{
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " Delete");
				assembler.DeleteItem(dataMessage.body);
			}
			return dataMessage;
		}

		private IMessage UpdateCollection(UpdateCollectionMessage updateCollectionMessage, IList messages)
		{
			IList updateCollectionRanges = updateCollectionMessage.body as IList;
			for(int i = 0; i < updateCollectionRanges.Count; i++)
			{
				UpdateCollectionRange updateCollectionRange = updateCollectionRanges[i] as UpdateCollectionRange;
				for(int j = 0; j < updateCollectionRange.identities.Length; j++)
				{
					string messageId = updateCollectionRange.identities[j] as string;
					Identity identity = null;
					if( messageId != null )
					{
						//Search for previous Create or CreateAndSequence
						//This was a "pending update collection" sent from the client and it must be replaced by the actual Identity
						IMessage refMessage = GetMessage(messages, messageId);
						DataMessage dataMessage = refMessage as DataMessage;
						if( dataMessage != null )
						{
							DataDestination dataDestination = this.Destination as DataDestination;
							identity = Identity.GetIdentity(dataMessage.body, dataDestination);
						}
						//replace with the actual identity
						updateCollectionRange.identities[j] = identity;
					}
					else
					{
                        IDictionary identityMap = updateCollectionRange.identities[j] as IDictionary;
						if( identityMap != null )
							identity = new Identity(identityMap);
					}

					IList fillParameters = updateCollectionMessage.collectionId as IList;

					IAssembler assembler = GetAssembler();
					if( assembler != null )
					{
						if( updateCollectionRange.updateType == UpdateCollectionRange.InsertIntoCollection )
							assembler.AddItemToFill(fillParameters, updateCollectionRange.position + j ,identity);
						if( updateCollectionRange.updateType == UpdateCollectionRange.DeleteFromCollection )
							assembler.RemoveItemFromFill(fillParameters, updateCollectionRange.position ,identity);
					}
				}
			}
			return updateCollectionMessage;
		}

		private IMessage GetMessage(IList messages, string messageId)
		{
			foreach(IMessage message in messages)
			{
				if(message.messageId == messageId)
					return message;
			}
			return null;
		}

		private IList Batch(DataMessage dataMessage)
		{
			ArrayList result = new ArrayList();
			IList messageBatch = dataMessage.body as IList;
			for(int i = 0; i < messageBatch.Count; i++)
			{
				IMessage message = messageBatch[i] as IMessage;
				try
				{
					if( message is UpdateCollectionMessage )
					{
						result.Add(UpdateCollection(message as UpdateCollectionMessage, messageBatch));
					}
					else
					{
						object obj = Invoke(message);
						result.Add(obj);
					}
				}
				catch(DataSyncException dataSyncException)
				{
					DataErrorMessage dataErrorMessage = dataSyncException.GetErrorMessage() as DataErrorMessage;
					dataErrorMessage.cause = message as DataMessage;
					dataErrorMessage.correlationId = message.messageId;
					dataErrorMessage.destination = message.destination;
					result.Add(dataErrorMessage);
				}
				catch(Exception exception)
				{
					MessageException messageException = new MessageException(exception);
					ErrorMessage errorMessage = messageException.GetErrorMessage();
					errorMessage.correlationId = message.messageId;
					errorMessage.destination = message.destination;
					result.Add(errorMessage);
				}
			}
			return result;
		}

		public int RefreshFill(IList fillParameters, object item, bool isCreate)
		{
			IAssembler assembler = GetAssembler();
			if( assembler != null )
			{
				if(log != null && log.IsDebugEnabled)
					log.Debug(assembler.GetType().FullName + " RefreshFill");
				return assembler.RefreshFill(fillParameters, item, isCreate);
			}
			return Assembler.DoNotExecuteFill;
		}
	}
}
