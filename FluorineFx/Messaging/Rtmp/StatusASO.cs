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
using FluorineFx;

namespace FluorineFx.Messaging.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class StatusASO : ASObject
	{
        /// <summary>
        /// Error constant.
        /// </summary>
		public const string ERROR = "error";
        /// <summary>
        /// Status constant.
        /// </summary>
		public const string STATUS = "status";
        /// <summary>
        /// Warning constant.
        /// </summary>
		public const string WARNING = "warning";

        /// <summary>
        /// The NetConnection.call method was not able to invoke the server-side method or command.
        /// </summary>
		public const string NC_CALL_FAILED = "NetConnection.Call.Failed";
        /// <summary>
        /// The URI specified in the NetConnection.connect method did not specify 'rtmp' as the protocol. Either not supported version of AMF was used (3 when only 0 is supported).
        /// </summary>
		public const string NC_CALL_BADVERSION = "NetConnection.Call.BadVersion";
        /// <summary>
        /// The application has been shut down (for example, if the application is out of
        /// memory resources and must shut down to prevent the server from crashing) or the server has shut down.
        /// </summary>
		public const string NC_CONNECT_APPSHUTDOWN = "NetConnection.Connect.AppShutdown";
        /// <summary>
        /// The connection was closed successfully.
        /// </summary>
		public const string NC_CONNECT_CLOSED = "NetConnection.Connect.Closed";
        /// <summary>
        /// The connection attempt failed.
        /// </summary>
		public const string NC_CONNECT_FAILED = "NetConnection.Connect.Failed";
        /// <summary>
        /// The client does not have permission to connect to the application, the
        /// application expected different parameters from those that were passed,
        /// or the application name specified during the connection attempt was not found on the server.
        /// </summary>
		public const string NC_CONNECT_REJECTED = "NetConnection.Connect.Rejected";
        /// <summary>
        /// The connection attempt succeeded.
        /// </summary>
		public const string NC_CONNECT_SUCCESS = "NetConnection.Connect.Success";
        /// <summary>
        /// The application name specified during connect is invalid.
        /// </summary>
	    public const string NC_CONNECT_INVALID_APPLICATION = "NetConnection.Connect.InvalidApp";
        /// <summary>
        /// Invalid arguments were passed to a NetStream method.
        /// </summary>
	    public const string NS_INVALID_ARGUMENT = "NetStream.InvalidArg";
        /// <summary>
        /// A recorded stream was deleted successfully.
        /// </summary>
		public const string NS_CLEAR_SUCCESS = "NetStream.Clear.Success";
        /// <summary>
        /// A recorded stream failed to delete.
        /// </summary>
		public const string NS_CLEAR_FAILED = "NetStream.Clear.Failed";
        /// <summary>
        /// An attempt to publish was successful.
        /// </summary>
		public const string NS_PUBLISH_START = "NetStream.Publish.Start";
        /// <summary>
        /// An attempt was made to publish a stream that is already being published by someone else.
        /// </summary>
		public const string NS_PUBLISH_BADNAME = "NetStream.Publish.BadName";
        /// <summary>
        /// An attempt to use a Stream method (at client-side) failed.
        /// </summary>
		public const string NS_FAILED = "NetStream.Failed";
        /// <summary>
        /// An attempt to unpublish was successful.
        /// </summary>
		public const string NS_UNPUBLISHED_SUCCESS = "NetStream.Unpublish.Success";
        /// <summary>
        /// Recording was started.
        /// </summary>
		public const string NS_RECORD_START = "NetStream.Record.Start";
        /// <summary>
        /// An attempt was made to record a read-only stream.
        /// </summary>
		public const string NS_RECORD_NOACCESS = "NetStream.Record.NoAccess";
        /// <summary>
        /// Recording was stopped.
        /// </summary>
		public const string NS_RECORD_STOP = "NetStream.Record.Stop";
        /// <summary>
        /// An attempt to record a stream failed.
        /// </summary>
		public const string NS_RECORD_FAILED = "NetStream.Record.Failed";
        /// <summary>
        /// Data is playing behind the normal speed.
        /// </summary>
		public const string NS_PLAY_INSUFFICIENT_BW = "NetStream.Play.InsufficientBW";
        /// <summary>
        /// Play was started.
        /// </summary>
		public const string NS_PLAY_START = "NetStream.Play.Start";
        /// <summary>
        /// An attempt was made to play a stream that does not exist.
        /// </summary>
		public const string NS_PLAY_STREAMNOTFOUND = "NetStream.Play.StreamNotFound";
        /// <summary>
        /// Play was stopped.
        /// </summary>
		public const string NS_PLAY_STOP = "NetStream.Play.Stop";
        /// <summary>
        /// An attempt to play back a stream failed.
        /// </summary>
		public const string NS_PLAY_FAILED = "NetStream.Play.Failed";
        /// <summary>
        /// A playlist was reset.
        /// </summary>
		public const string NS_PLAY_RESET = "NetStream.Play.Reset";
        /// <summary>
        /// The initial publish to a stream was successful. This message is sent to all subscribers.
        /// </summary>
		public const string NS_PLAY_PUBLISHNOTIFY = "NetStream.Play.PublishNotify";
        /// <summary>
        /// An unpublish from a stream was successful. This message is sent to all subscribers.
        /// </summary>
		public const string NS_PLAY_UNPUBLISHNOTIFY = "NetStream.Play.UnpublishNotify";
        /// <summary>
        /// Playlist playback switched from one stream to another.
        /// </summary>
        public const string NS_PLAY_SWITCH = "NetStream.Play.Switch";
        /// <summary>
        /// Playlist playback is complete.
        /// </summary>
    	public const string NS_PLAY_COMPLETE = "NetStream.Play.Complete";
        /// <summary>
        /// The subscriber has used the seek command to move to a particular location in the recorded stream.
        /// </summary>
		public const string NS_SEEK_NOTIFY = "NetStream.Seek.Notify";
        /// <summary>
        /// The stream doesn't support seeking.
        /// </summary>
        public const string NS_SEEK_FAILED = "NetStream.Seek.Failed";
        /// <summary>
        /// The subscriber has used the seek command to move to a particular location in the recorded stream.
        /// </summary>
		public const string NS_PAUSE_NOTIFY = "NetStream.Pause.Notify";
        /// <summary>
        /// Publishing has stopped.
        /// </summary>
		public const string NS_UNPAUSE_NOTIFY = "NetStream.Unpause.Notify";
        /// <summary>
        /// Undocumented code.
        /// </summary>
		public const string NS_DATA_START = "NetStream.Data.Start";
        /// <summary>
        /// The ActionScript engine has encountered a runtime error.
        /// </summary>
		public const string APP_SCRIPT_ERROR = "Application.Script.Error";
        /// <summary>
        /// The ActionScript engine has encountered a runtime warning.
        /// </summary>
		public const string APP_SCRIPT_WARNING = "Application.Script.Warning";
        /// <summary>
        /// The ActionScript engine is low on runtime memory.
        /// </summary>
		public const string APP_RESOURCE_LOWMEMORY = "Application.Resource.LowMemory";
        /// <summary>
        /// This information object is passed to the onAppStop handler when the application is being shut down.
        /// </summary>
		public const string APP_SHUTDOWN = "Application.Shutdown";
        /// <summary>
        /// This information object is passed to the onAppStop event handler when the application instance is about to be destroyed by the server.
        /// </summary>
		public const string APP_GC = "Application.GC";

        /// <summary>
        /// Read access to a shared object was denied.
        /// </summary>
	    public const string SO_NO_READ_ACCESS = "SharedObject.NoReadAccess";
        /// <summary>
        /// Write access to a shared object was denied.
        /// </summary>
	    public const string SO_NO_WRITE_ACCESS = "SharedObject.NoWriteAccess";
        /// <summary>
        /// The creation of a shared object was denied.
        /// </summary>
	    public const string SO_CREATION_FAILED = "SharedObject.ObjectCreationFailed";
        /// <summary>
        /// The persistence parameter passed to SharedObject.getRemote() is different from the one used when the shared object was created.
        /// </summary>
	    public const string SO_PERSISTENCE_MISMATCH = "SharedObject.BadPersistence";

		/// <summary>
		/// Initializes a new instance of the StatusASO class.
		/// </summary>
		private StatusASO()
		{
		}
		/// <summary>
		/// Initializes a new instance of the StatusASO class.
		/// </summary>
        /// <param name="code">Status code.</param>
        /// <param name="level">Level.</param>
        /// <param name="description">Description.</param>
		/// <param name="application"></param>
		/// <param name="objectEncoding"></param>
        internal StatusASO(string code, string level, string description, object application, ObjectEncoding objectEncoding)
		{
			Add("code", code);
			Add("level", level);
			Add("description", description);
			Add("application", application);
			Add("objectEncoding", (double)objectEncoding);
		}
        /// <summary>
        /// Initializes a new instance of the StatusASO class.
        /// </summary>
        /// <param name="code">Status code.</param>
        /// <param name="level">Level.</param>
        /// <param name="description">Description.</param>
        internal StatusASO(string code, string level, string description)
        {
            Add("code", code);
            Add("level", level);
            Add("description", description);
        }
        /// <summary>
        /// Initializes a new instance of the StatusASO class.
        /// </summary>
        /// <param name="code">Status code.</param>
        internal StatusASO(string code) 
        {
            Add("code", code);
            Add("level", StatusASO.STATUS);
	    }
        /// <summary>
        /// Sets the "application" property.
        /// </summary>
		public object Application
		{
			set{ this["application"] = value; }
		}
        /// <summary>
        /// Gets or sets the "objectEncoding" property.
        /// </summary>
		public double objectEncoding
		{
			set{ this["objectEncoding"] = value; }
			get
            { 
                if( this.ContainsKey("objectEncoding") )
                    return (double)this["objectEncoding"];
                return (double)ObjectEncoding.AMF0;
            }
		}
        /// <summary>
        /// Gets or sets the "clientid" property.
        /// </summary>
        public int clientid
        {
            set { this["clientid"] = value; }
            get 
            {
                if (this.ContainsKey("clientid"))
                    return (int)this["clientid"];
                return 0;
            }
        }
        /// <summary>
        /// Gets or sets the "level" property.
        /// </summary>
        public string level
        {
            set { this["level"] = value; }
            get 
            {
                if (this.ContainsKey("level"))
                    return (string)this["level"];
                return null;
            }
        }
        /// <summary>
        /// Gets or sets the "code" property.
        /// </summary>
        public string code
        {
            set { this["code"] = value; }
            get 
            {
                if (this.ContainsKey("code"))
                    return (string)this["code"];
                return null;
            }
        }
        /// <summary>
        /// Gets or sets the "description" property.
        /// </summary>
        public string description
        {
            set { this["description"] = value; }
            get 
            {
                if (this.ContainsKey("description"))
                    return (string)this["description"];
                return null;
            }
        }
        /// <summary>
        /// Gets or sets the "details" property.
        /// </summary>
        public string details
        {
            set { this["details"] = value; }
            get 
            {
                if (this.ContainsKey("details"))
                    return (string)this["details"];
                return null;
            }
        }
        /// <summary>
        /// Gets a Status ActionScript Object with the specified status code.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="objectEncoding">Encoding.</param>
        /// <returns>A Status ActionScript Object.</returns>
		public static StatusASO GetStatusObject(string statusCode, ObjectEncoding objectEncoding)
		{
			switch(statusCode)
			{
				case NC_CALL_FAILED:
					return new StatusASO(NC_CALL_FAILED, ERROR, string.Empty, null, objectEncoding);
				case NC_CALL_BADVERSION:
					return new StatusASO(NC_CALL_BADVERSION, ERROR, string.Empty, null, objectEncoding);
				case NC_CONNECT_APPSHUTDOWN:
					return new StatusASO(NC_CONNECT_APPSHUTDOWN, ERROR, string.Empty, null, objectEncoding);
				case NC_CONNECT_CLOSED:
					return new StatusASO(NC_CONNECT_CLOSED, STATUS, string.Empty, null, objectEncoding);
				case NC_CONNECT_FAILED:
					return new StatusASO(NC_CONNECT_FAILED, ERROR, string.Empty, null, objectEncoding);
				case NC_CONNECT_REJECTED:
					return new StatusASO(NC_CONNECT_REJECTED, ERROR, string.Empty, null, objectEncoding);
				case NC_CONNECT_SUCCESS:
					return new StatusASO(NC_CONNECT_SUCCESS, STATUS, string.Empty, null, objectEncoding);
				default:
					return new StatusASO(NC_CALL_BADVERSION, ERROR, string.Empty, null, objectEncoding);
			}
		}
	}
}
