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
using System.Resources;

namespace FluorineFx
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal class __Res
	{
		private static ResourceManager _resMgr;

        internal const string Amf_Begin = "Amf_Begin";
        internal const string Amf_End = "Amf_End";
        internal const string Amf_Fatal = "Amf_Fatal";
        internal const string Amf_Fatal404 = "Amf_Fatal404";
        internal const string Amf_ReadBodyFail = "Amf_ReadBodyFail";
        internal const string Amf_SerializationFail = "Amf_SerializationFail";
        internal const string Amf_ResponseFail = "Amf_ResponseFail";

        internal const string Context_MissingError = "Context_MissingError";
        internal const string Context_Initialized = "Context_Initialized";

        internal const string Rtmpt_Begin = "Rtmpt_Begin";
        internal const string Rtmpt_End = "Rtmpt_End";
        internal const string Rtmpt_Fatal = "Rtmpt_Fatal";
        internal const string Rtmpt_Fatal404 = "Rtmpt_Fatal404";
        internal const string Rtmpt_CommandBadRequest = "Rtmpt_CommandBadRequest";
        internal const string Rtmpt_CommandNotSupported = "Rtmpt_CommandNotSupported";
        internal const string Rtmpt_CommandOpen = "Rtmpt_CommandOpen";
        internal const string Rtmpt_CommandSend = "Rtmpt_CommandSend";
        internal const string Rtmpt_CommandIdle = "Rtmpt_CommandIdle";
        internal const string Rtmpt_CommandClose = "Rtmpt_CommandClose";
        internal const string Rtmpt_ReturningMessages = "Rtmpt_ReturningMessages";
        internal const string Rtmpt_NotifyError = "Rtmpt_NotifyError";
        internal const string Rtmpt_UnknownClient = "Rtmpt_UnknownClient";

        internal const string Swx_Begin = "Swx_Begin";
        internal const string Swx_End = "Swx_End";
        internal const string Swx_Fatal = "Swx_Fatal";
        internal const string Swx_Fatal404 = "Swx_Fatal404";
        internal const string Swx_InvalidCrossDomainUrl = "Swx_InvalidCrossDomainUrl";

        internal const string Json_Begin = "Json_Begin";
        internal const string Json_End = "Json_End";
        internal const string Json_Fatal = "Json_Fatal";
        internal const string Json_Fatal404 = "Json_Fatal404";

        internal const string Rtmp_HSInitBuffering = "Rtmp_HSInitBuffering";
        internal const string Rtmp_HSReplyBuffering = "Rtmp_HSReplyBuffering";
        internal const string Rtmp_HeaderBuffering = "Rtmp_HeaderBuffering";
        internal const string Rtmp_DataBuffering = "Rtmp_DataBuffering";
        internal const string Rtmp_ChunkSmall = "Rtmp_ChunkSmall";
        internal const string Rtmp_DecodeHeader = "Rtmp_DecodeHeader";
        internal const string Rtmp_ServerAddMapping = "Rtmp_ServerAddMapping";
        internal const string Rtmp_ServerRemoveMapping = "Rtmp_ServerRemoveMapping";
        internal const string Rtmp_SocketListenerAccept = "Rtmp_SocketListenerAccept";
        internal const string Rtmp_SocketBeginReceive = "Rtmp_SocketBeginReceive";
        internal const string Rtmp_SocketReceiveProcessing = "Rtmp_SocketReceiveProcessing";
        internal const string Rtmp_SocketBeginRead = "Rtmp_SocketBeginRead";
        internal const string Rtmp_SocketReadProcessing = "Rtmp_SocketReadProcessing";
        internal const string Rtmp_SocketBeginSend = "Rtmp_SocketBeginSend";
        internal const string Rtmp_SocketSendProcessing = "Rtmp_SocketSendProcessing";
        internal const string Rtmp_SocketConnectionReset = "Rtmp_SocketConnectionReset";
        internal const string Rtmp_SocketConnectionTimeout = "Rtmp_SocketConnectionTimeout";
        internal const string Rtmp_SocketConnectionAborted = "Rtmp_SocketConnectionAborted";
        internal const string Rtmp_SocketDisconnectProcessing = "Rtmp_SocketDisconnectProcessing";
        internal const string Rtmp_ConnectionClose = "Rtmp_ConnectionClose";
        internal const string Rtmp_CouldNotProcessMessage = "Rtmp_CouldNotProcessMessage";
        internal const string Rtmp_BeginHandlePacket = "Rtmp_BeginHandlePacket";
        internal const string Rtmp_EndHandlePacket = "Rtmp_EndHandlePacket";
        internal const string Rtmp_BeginDisconnect = "Rtmp_BeginDisconnect";
        internal const string Rtmp_WritePacket = "Rtmp_WritePacket";
        internal const string Rtmp_SocketSend = "Rtmp_SocketSend";
        internal const string Rtmp_HandlerError = "Rtmp_HandlerError";

        internal const string PushNotSupported = "PushNotSupported";

        internal const string Arg_Mismatch = "Arg_Mismatch";

        internal const string Cache_Hit = "Cache_Hit";
        internal const string Cache_HitKey = "Cache_HitKey";

        internal const string Compiler_Error = "Compiler_Error";

        internal const string ClassDefinition_Loaded = "ClassDefinition_Loaded";
        internal const string ClassDefinition_LoadedUntyped = "ClassDefinition_LoadedUntyped";
        internal const string Externalizable_CastFail = "Externalizable_CastFail";
        internal const string TypeIdentifier_Loaded = "TypeIdentifier_Loaded";
        internal const string TypeLoad_ASO = "TypeLoad_ASO";
        internal const string TypeMapping_Write = "TypeMapping_Write";
        internal const string TypeSerializer_NotFound = "TypeSerializer_NotFound";

        internal const string Endpoint_BindFail = "Endpoint_BindFail";
        internal const string Endpoint_Bind = "Endpoint_Bind";
        internal const string Endpoint_HandleMessage = "Endpoint_HandleMessage";
        internal const string Endpoint_Response = "Endpoint_Response";

        internal const string Type_InitError = "Type_InitError";
        internal const string Type_Mismatch = "Type_Mismatch";
        internal const string Type_MismatchMissingSource = "Type_MismatchMissingSource";

        internal const string Wsdl_ProxyGen = "Wsdl_ProxyGen";
        internal const string Wsdl_ProxyGenFail = "Wsdl_ProxyGenFail";

        internal const string Destination_NotFound = "Destination_NotFound";
        internal const string Destination_Reinit = "Destination_Reinit";

        internal const string MessageBroker_NotAvailable = "MessageBroker_NotAvailable";
        internal const string MessageBroker_RegisterError = "MessageBroker_RegisterError";
        internal const string MessageBroker_RoutingError = "MessageBroker_RoutingError";
        internal const string MessageBroker_RoutingMessage = "MessageBroker_RoutingMessage";
        internal const string MessageBroker_Response = "MessageBroker_Response";

        internal const string MessageServer_TryingServiceConfig = "MessageServer_TryingServiceConfig";
        internal const string MessageServer_LoadingConfigDefault = "MessageServer_LoadingConfigDefault";
        internal const string MessageServer_LoadingServiceConfig = "MessageServer_LoadingServiceConfig";
        internal const string MessageServer_Start = "MessageServer_Start";
        internal const string MessageServer_Started = "MessageServer_Started";
        internal const string MessageServer_StartError = "MessageServer_StartError";
        internal const string MessageServer_Stop = "MessageServer_Stop";
        internal const string MessageServer_AccessFail = "MessageServer_AccessFail";
        internal const string MessageServer_Create = "MessageServer_Create";
        internal const string MessageServer_MissingAdapter = "MessageServer_MissingAdapter";

        internal const string MessageClient_Disconnect = "MessageClient_Disconnect";
        internal const string MessageClient_Unsubscribe = "MessageClient_Unsubscribe";
        internal const string MessageClient_Timeout = "MessageClient_Timeout";

        internal const string MessageDestination_RemoveSubscriber = "MessageDestination_RemoveSubscriber";

        internal const string MessageServiceSubscribe = "MessageServiceSubscribe";
        internal const string MessageServiceUnsubscribe = "MessageServiceUnsubscribe";
        internal const string MessageServiceUnknown = "MessageServiceUnknown";
        internal const string MessageServiceRoute = "MessageServiceRoute";
        internal const string MessageServicePush = "MessageServicePush";
        internal const string MessageServicePushBinary = "MessageServicePushBinary";

        internal const string Subtopic_Invalid = "Subtopic_Invalid";
        internal const string Selector_InvalidResult = "Selector_InvalidResult";


        internal const string Client_Create = "Client_Create";
        internal const string Client_Invalidated = "Client_Invalidated";
        internal const string Client_Lease = "Client_Lease";
        internal const string ClientManager_CacheExpired = "ClientManager_CacheExpired";
        internal const string ClientManager_Remove = "ClientManager_Remove";

        internal const string Session_Create = "Session_Create";
        internal const string Session_Invalidated = "Session_Invalidated";
        internal const string Session_Lease = "Session_Lease";
        internal const string SessionManager_CacheExpired = "SessionManager_CacheExpired";
        internal const string SessionManager_Remove = "SessionManager_Remove";

        internal const string SubscriptionManager_CacheExpired = "SubscriptionManager_CacheExpired";
        internal const string SubscriptionManager_Remove = "SubscriptionManager_Remove";
        
        internal const string Invalid_Destination = "Invalid_Destination";

        internal const string Security_AccessNotAllowed = "Security_AccessNotAllowed";
        internal const string Security_LoginMissing = "Security_LoginMissing";
        internal const string Security_ConstraintRefNotFound = "Security_ConstraintRefNotFound";
        internal const string Security_ConstraintSectionNotFound = "Security_ConstraintSectionNotFound";
        internal const string Security_AuthenticationFailed = "Security_AuthenticationFailed";

        internal const string SocketServer_Start = "SocketServer_Start";
        internal const string SocketServer_Started = "SocketServer_Started";
        internal const string SocketServer_Stopping = "SocketServer_Stopping";
        internal const string SocketServer_Stopped = "SocketServer_Stopped";
        internal const string SocketServer_Failed = "SocketServer_Failed";
        internal const string SocketServer_ListenerFail = "SocketServer_ListenerFail";
        internal const string SocketServer_SocketOptionFail = "SocketServer_SocketOptionFail";

        internal const string RtmpEndpoint_Start = "RtmpEndpoint_Start";
        internal const string RtmpEndpoint_Starting = "RtmpEndpoint_Starting";
        internal const string RtmpEndpoint_Started = "RtmpEndpoint_Started";
        internal const string RtmpEndpoint_Stopping = "RtmpEndpoint_Stopping";
        internal const string RtmpEndpoint_Stopped = "RtmpEndpoint_Stopped";
        internal const string RtmpEndpoint_Failed = "RtmpEndpoint_Failed";
        internal const string RtmpEndpoint_Error = "RtmpEndpoint_Error";

        internal const string Scope_Connect = "Scope_Connect";
        internal const string Scope_NotFound = "Scope_NotFound";
        internal const string Scope_ChildNotFound = "Scope_ChildNotFound";
        internal const string Scope_Check = "Scope_Check";
        internal const string Scope_CheckHostPath = "Scope_CheckHostPath";
        internal const string Scope_CheckWildcardHostPath = "Scope_CheckWildcardHostPath";
        internal const string Scope_CheckHostNoPath = "Scope_CheckHostNoPath";
        internal const string Scope_CheckDefaultHostPath = "Scope_CheckDefaultHostPath";
        internal const string Scope_UnregisterError = "Scope_UnregisterError";
        internal const string Scope_DisconnectError = "Scope_DisconnectError";

        internal const string SharedObject_Delete = "SharedObject_Delete";
        internal const string SharedObject_DeleteError = "SharedObject_DeleteError";
        internal const string SharedObject_StoreError = "SharedObject_StoreError";
        internal const string SharedObject_Sync = "SharedObject_Sync";
        internal const string SharedObject_SyncConnError = "SharedObject_SyncConnError";

        internal const string SharedObjectService_CreateStore = "SharedObjectService_CreateStore";
        internal const string SharedObjectService_CreateStoreError = "SharedObjectService_CreateStoreError";

        internal const string DataDestination_RemoveSubscriber = "DataDestination_RemoveSubscriber";

        internal const string DataService_Unknown = "DataService_Unknown";

        internal const string Sequence_AddSubscriber = "Sequence_AddSubscriber";
        internal const string Sequence_RemoveSubscriber = "Sequence_RemoveSubscriber";

        internal const string SequenceManager_CreateSeq = "SequenceManager_CreateSeq";
        internal const string SequenceManager_Remove = "SequenceManager_Remove";
        internal const string SequenceManager_RemoveStatus = "SequenceManager_RemoveStatus";
        internal const string SequenceManager_Unknown = "SequenceManager_Unknown";
        internal const string SequenceManager_ReleaseCollection = "SequenceManager_ReleaseCollection";
        internal const string SequenceManager_RemoveSubscriber = "SequenceManager_RemoveSubscriber";
        internal const string SequenceManager_RemoveEmptySeq = "SequenceManager_RemoveEmptySeq";
        internal const string SequenceManager_RemoveSubscriberSeq = "SequenceManager_RemoveSubscriberSeq";

        internal const string Service_NotFound = "Service_NotFound";
        internal const string Service_Mapping = "Service_Mapping";
        internal const string ServiceHandler_InvocationFailed = "ServiceHandler_InvocationFailed";

        internal const string Identity_Failed = "Identity_Failed";

        internal const string Invoke_Method = "Invoke_Method";

        internal const string Channel_NotFound = "Channel_NotFound";


        internal const string TypeHelper_Probing = "TypeHelper_Probing";
        internal const string TypeHelper_LoadDllFail = "TypeHelper_LoadDllFail";
        internal const string TypeHelper_ConversionFail = "TypeHelper_ConversionFail";

        internal const string Invocation_NoSuitableMethod = "Invocation_NoSuitableMethod";
        internal const string Invocation_Ambiguity = "Invocation_Ambiguity";
        internal const string Invocation_ParameterType = "Invocation_ParameterType";
        internal const string Invocation_Failed = "Invocation_Failed";

        internal const string ServiceInvoker_Resolve = "ServiceInvoker_Resolve";
        internal const string ServiceInvoker_ResolveFail = "ServiceInvoker_ResolveFail";

        internal const string Reflection_MemberNotFound = "Reflection_MemberNotFound";
        internal const string Reflection_PropertyReadOnly = "Reflection_PropertyReadOnly";
        internal const string Reflection_PropertySetFail = "Reflection_PropertySetFail";
        internal const string Reflection_PropertyIndexFail = "Reflection_PropertyIndexFail";
        internal const string Reflection_FieldSetFail = "Reflection_FieldSetFail";

        internal const string AppAdapter_AppConnect = "AppAdapter_AppConnect";
        internal const string AppAdapter_AppDisconnect = "AppAdapter_AppDisconnect";
        internal const string AppAdapter_RoomConnect = "AppAdapter_RoomConnect";
        internal const string AppAdapter_RoomDisconnect = "AppAdapter_RoomDisconnect";
        internal const string AppAdapter_AppStart = "AppAdapter_AppStart";
        internal const string AppAdapter_RoomStart = "AppAdapter_RoomStart";
        internal const string AppAdapter_AppStop = "AppAdapter_AppStop";
        internal const string AppAdapter_RoomStop = "AppAdapter_RoomStop";
        internal const string AppAdapter_AppJoin = "AppAdapter_AppJoin";
        internal const string AppAdapter_AppLeave = "AppAdapter_AppLeave";
        internal const string AppAdapter_RoomJoin = "AppAdapter_RoomJoin";
        internal const string AppAdapter_RoomLeave = "AppAdapter_RoomLeave";

        internal const string Compress_Info = "Compress_Info";

        internal const string Fluorine_InitModule = "Fluorine_InitModule";
        internal const string Fluorine_Start = "Fluorine_Start";
        internal const string Fluorine_Version = "Fluorine_Version";
        internal const string Fluorine_Fatal = "Fluorine_Fatal";

        internal const string ServiceBrowser_Aquire = "ServiceBrowser_Aquire";
        internal const string ServiceBrowser_Aquired = "ServiceBrowser_Aquired";
        internal const string ServiceBrowser_AquireFail = "ServiceBrowser_AquireFail";

        internal const string ServiceAdapter_MissingSettings = "ServiceAdapter_MissingSettings";
        internal const string ServiceAdapter_Stop = "ServiceAdapter_Stop";
        internal const string ServiceAdapter_ManageFail = "ServiceAdapter_ManageFail";

        internal const string Msmq_StartQueue = "Msmq_StartQueue";
        internal const string Msmq_InitFormatter = "Msmq_InitFormatter";
        internal const string Msmq_Receive = "Msmq_Receive";
        internal const string Msmq_Send = "Msmq_Send";
        internal const string Msmq_Fail = "Msmq_Fail";
        internal const string Msmq_Enable = "Msmq_Enable";
        internal const string Msmq_Poison = "Msmq_Poison";

        internal const string Silverlight_StartPS = "Silverlight_StartPS";
        internal const string Silverlight_PSError = "Silverlight_PSError";

        internal const string Optimizer_Fatal = "Optimizer_Fatal";
        internal const string Optimizer_Warning = "Optimizer_Warning";
        internal const string Optimizer_FileLocation = "Optimizer_FileLocation";

        internal const string Error_ContextDump  = "Error_ContextDump";

		internal static string GetString(string key)
		{
			if (_resMgr == null)
			{
                _resMgr = new ResourceManager("FluorineFx.Resources.Resource", typeof(__Res).Assembly);
			}
			string text = _resMgr.GetString(key);
			if (text == null)
			{
				throw new ApplicationException("Missing resource from FluorineFx library!  Key: " + key);
			}
			return text;
		}

		internal static string GetString(string key, params object[] inserts)
		{
			return string.Format(GetString(key), inserts);
		}
	}
}
