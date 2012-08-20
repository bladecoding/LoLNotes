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

namespace FluorineFx.Net
{
    /// <summary>
    /// Event dispatched when a remote SharedObject instance has been updated by the server.
    /// </summary>
    public class SyncEventArgs : EventArgs
    {
        ASObject[] _changeList;

        internal SyncEventArgs(ASObject[] changeList)
        {
            _changeList = changeList;
        }
        /// <summary>
        /// <para>
        /// An array of objects; each object contains properties that describe the changed members of a remote shared object.
        /// The properties of each object are <b>code</b>, <b>name</b>, and <b>oldValue</b>.
        /// </para>
        /// <para>
        /// When you initially connect to a remote shared object that is persistent locally and/or on the server, all the properties of this object are set to empty strings.
        /// </para>
        /// </summary>
        public ASObject[] ChangeList
        {
            get { return _changeList; }
        }
    }
}
