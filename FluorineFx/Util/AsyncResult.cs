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
using System.Threading;

namespace FluorineFx.Util
{
#if !(NET_1_1)
    internal class AsyncResult<TResult> : AsyncResultNoResult
    {
        // Field set when operation completes
        private TResult m_result = default(TResult);

        public AsyncResult(AsyncCallback asyncCallback, Object state)
            : base(asyncCallback, state)
        {
        }

        public void SetAsCompleted(TResult result, Boolean completedSynchronously)
        {
            // Save the asynchronous operation's result
            m_result = result;

            // Tell the base class that the operation completed 
            // sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        new public TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return m_result;  // Return the result (if above didn't throw)
        }
    }
#endif
}
