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

namespace FluorineFx.Net
{
    /// <summary>
    /// The function invoked if the call to the server succeeds and returns a result.
    /// </summary>
    public delegate void ResultFunction<T>(T @object);
    /// <summary>
    /// The function invoked if the server returns an error.
    /// </summary>
    public delegate void StatusFunction(Fault fault);

    /// <summary>
    /// The Responder class provides an object that is used in NetConnection.call() to handle return values from the server related to the success or failure of specific operations. 
    /// When working with NetConnection.call(), you may encounter a network operation fault specific to the current operation or a fault related to the current connection status. 
    /// Operation errors target the Responder object instead of the NetConnection object for easier error handling.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Responder<T>
    {
        private readonly ResultFunction<T> _result;
        private readonly StatusFunction _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="Responder&lt;T&gt;"/> class.
        /// You pass a Responder object to NetConnection.call() to handle return values from the server. You may pass null for either or both parameters.
        /// </summary>
        /// <param name="result">The function invoked if the call to the server succeeds and returns a result.</param>
        public Responder(ResultFunction<T> result)
            : this(result, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Responder&lt;T&gt;"/> class.
        /// You pass a Responder object to NetConnection.call() to handle return values from the server. You may pass null for either or both parameters.
        /// </summary>
        /// <param name="result">The function invoked if the call to the server succeeds and returns a result.</param>
        /// <param name="status">The function invoked if the server returns an error.</param>
        public Responder(ResultFunction<T> result, StatusFunction status)
        {
            _result = result;
            _status = status;
        }

        /// <summary>
        /// Gets the function invoked if the call to the server succeeds and returns a result.
        /// </summary>
        /// <value>The result function.</value>
        public ResultFunction<T> Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets the function invoked if the server returns an error.
        /// </summary>
        /// <value>The status function.</value>
        public StatusFunction Status
        {
            get { return _status; }
        }
    }
}
