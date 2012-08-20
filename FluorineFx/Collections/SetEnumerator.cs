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

namespace FluorineFx.Collections
{
    /// <summary>
    /// Summary description for SetEnumerator.
    /// </summary>
    internal class SetEnumerator : System.Collections.IEnumerator
    {
        RbTree _tree;
        RbTreeNode _currentNode = null;

        public SetEnumerator(RbTree tree)
        {
            _tree = tree;
        }

        #region IEnumerator Members

        public void Reset()
        {
            _currentNode = null;
        }

        public object Current
        {
            get
            {
                // if _currentNode is null, will throw an exception, which confirms to IEnumerable spec
                return _currentNode.Value;
            }
        }

        public bool MoveNext()
        {
            if (_currentNode == null)
            {
                _currentNode = _tree.First;
            }
            else
            {
                _currentNode = _tree.Next(_currentNode);
            }

            return !_currentNode.IsNull;
        }

        #endregion
    }
}
