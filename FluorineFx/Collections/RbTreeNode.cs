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
    /// Node of a red-black tree.
    /// </summary>
    public class RbTreeNode
    {
        /// <summary>
        /// Creates a default node with value val.
        /// </summary>
        /// <remarks>
        /// Node has no children and is red.
        /// </remarks>
        public RbTreeNode(object val):this(val, RbTreeNode.Nil, RbTreeNode.Nil, RbTreeNode.Nil, true)
        {
        }

        /// <summary>
        /// Creates a node from field values.
        /// </summary>
        public RbTreeNode(object val, RbTreeNode parent, RbTreeNode left, RbTreeNode right, bool isRed)
        {
            Value = val;
            Parent = parent;
            Left = left;
            Right = right;
            IsRed = isRed;
        }

        /// <summary>
        /// true if node is logically null (leaf) node, false otherwise.
        /// </summary>
        public virtual bool IsNull { get { return false; } }

        /// <summary>
        /// Value held by the node.
        /// </summary>
        public object Value;

        /// <summary>
        /// Parent node.
        /// </summary>
        public RbTreeNode Parent;

        /// <summary>
        /// Left child.
        /// </summary>
        public RbTreeNode Left;

        /// <summary>
        /// Right child.
        /// </summary>
        public RbTreeNode Right;

        /// <summary>
        /// true if node is red, false if node is black.
        /// </summary>
        public bool IsRed;

        /// <summary>
        /// Logically null (leaf) node.
        /// </summary>
        public static readonly RbTreeNode Nil = new NullNode();

        /// <summary>
        /// Constructor used internally for creating a Nil node.
        /// </summary>
        protected RbTreeNode() { }

        /// <summary>
        /// Represents null node
        /// </summary>
        /// <remarks>
        /// Null node is a leaf node with null value and without children.
        /// It is always black.
        /// </remarks>
        private class NullNode : RbTreeNode
        {
            /// <summary>
            /// Creates a logically null node.
            /// </summary>
            public NullNode()
            {
                Parent = this;
                Left = this;
                Right = this;
                IsRed = false;
            }

            /// <summary>
            /// Returns true for logically null node.
            /// </summary>
            public override bool IsNull { get { return true; } }
        }
    }
}
