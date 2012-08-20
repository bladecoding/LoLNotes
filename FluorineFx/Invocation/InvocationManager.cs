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
using System.Reflection;
using System.Collections;
#if !(NET_1_1)
using System.Collections.Generic;
#endif

namespace FluorineFx.Invocation
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class InvocationManager : IInvocationManager
	{
#if !(NET_1_1)
        Stack<object> _context;
        Dictionary<object, object> _properties;
#else
        Stack		_context;
        Hashtable	_properties;
#endif
        object		_result;

#if !(NET_1_1)
        public InvocationManager()
        {
            _context = new Stack<object>();
            _properties = new Dictionary<object, object>();
        }

        #region IInvocationManager Members

        public Stack<object> Context
        {
            get
            {
                return _context;
            }
        }

        public Dictionary<object, object> Properties
        {
            get
            {
                return _properties;
            }
        }

        public object Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        #endregion

#else
		public InvocationManager()
		{
			_context = new Stack();
			_properties = new Hashtable();
		}

		#region IInvocationManager Members

		public Stack Context
		{
			get
			{
				return _context;
			}
		}

		public Hashtable Properties
		{
			get
			{
				return _properties;
			}
		}

		public object Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
			}
		}

		#endregion
#endif
	}
}
