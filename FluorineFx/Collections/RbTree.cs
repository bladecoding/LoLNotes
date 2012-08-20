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

namespace FluorineFx.Collections
{
    /// <summary>
    /// Red-black tree, an almost balanced binary tree.
    /// </summary>
    /// <remarks>Red-black tree remains "almost balanced" when nodes
    /// are added or removed. See "Introduction to Algorithms" by T. Cormen at al.
    /// (ISBN 0262032937) or your favorite computer science text book for details.</remarks>
    public class RbTree
    {
        private RbTreeNode _root;	// sentinel, not a real root; real root is _root.Left
        private IComparer _comparer;

        /// <summary>
        /// Logical root node of the tree.
        /// </summary>
        /// <remarks>
        /// RbTreeNode.Nil if the tree is empty.
        /// </remarks>
        public RbTreeNode Root { get { return _root.Left; } }

        /// <summary>
        /// Comparer used in comparisons between nodes.
        /// </summary>
        public IComparer Comparer { get { return _comparer; } }

        /// <summary>
        /// Result of insertion into a red-black tree.
        /// </summary>
        public struct InsertResult
        {
            /// <summary>
            /// true if new node was inserted, false if old node was used.
            /// </summary>
            public bool NewNode;

            /// <summary>
            /// Value of inserted or replaced node.
            /// </summary>
            public RbTreeNode Node;

            /// <summary>
            /// Creates new instance of InsertResult.
            /// </summary>
            public InsertResult(bool newNode, RbTreeNode node)
            {
                NewNode = newNode;
                Node = node;
            }
        }

        /// <summary>
        /// Creates a new instance of a red-black tree.
        /// </summary>
        public RbTree(IComparer comparer)
        {
            _root = new RbTreeNode(null);
            _root.IsRed = false;
            _comparer = comparer;
        }

        /// <summary>
        /// Inserts item based only on its value, probably violating Left/Right property.
        /// </summary>
        /// <param name="z">Node to insert.</param>
        /// 
        /// <param name="allowDuplicates">If true, will insert the node even if equal node 
        /// already exists in the tree. If false, behavior depends on replaceIfDuplicate.
        /// </param>
        /// 
        /// <param name="replaceIfDuplicate">Matters only if node equal to z exists in the 
        /// tree and allowDuplicates is false. If replaceIfDuplicate is true, z replaces
        /// existing node. Otherwise, tree does not change.
        /// </param>
        /// <returns>
        /// result.NewNode is true if new node was inserted to the tree'
        /// result.Node contains newly inserted node if any, or node equal to z
        /// </returns>
        /// 
        private InsertResult BinaryInsert(RbTreeNode z, bool allowDuplicates, bool replaceIfDuplicate)
        {
            z.Left = RbTreeNode.Nil;
            z.Right = RbTreeNode.Nil;

            RbTreeNode y = _root;
            RbTreeNode x = _root.Left;

            while (x != RbTreeNode.Nil)
            {
                y = x;
                int result = _comparer.Compare(x.Value, z.Value);

                if (!allowDuplicates && (result == 0))
                {
                    if (replaceIfDuplicate)
                    {
                        x.Value = z.Value;
                    }

                    return new InsertResult(false, x);
                }

                if (result > 0) // x.Value > z.Value
                {
                    x = x.Left;
                }
                else
                {
                    x = x.Right;
                }
            }

            z.Parent = y;

            if ((y == _root) || (_comparer.Compare(y.Value, z.Value) > 0))
            {
                y.Left = z;
            }
            else
            {
                y.Right = z;
            }

            z.Parent = y;
            return new InsertResult(true, z);
        }

        /// <summary>
        /// Left rotation of the tree around x.
        /// </summary>
        private void LeftRotate(RbTreeNode x)
        {
            RbTreeNode y = x.Right;
            x.Right = y.Left;

            if (y.Left != RbTreeNode.Nil)
                y.Left.Parent = x;

            y.Parent = x.Parent;

            if (x == x.Parent.Left)
            {
                x.Parent.Left = y;
            }
            else
            {
                x.Parent.Right = y;
            }

            y.Left = x;
            x.Parent = y;
        }

        /// <summary>
        /// Right rotation of the tree around y.
        /// </summary>
        static private void RightRotate(RbTreeNode y)
        {
            RbTreeNode x = y.Left;
            y.Left = x.Right;

            if (x.Right != RbTreeNode.Nil)
            {
                x.Right.Parent = y;
            }

            x.Parent = y.Parent;

            if (y == y.Parent.Left)
            {
                y.Parent.Left = x;
            }
            else
            {
                y.Parent.Right = x;
            }

            x.Right = y;
            y.Parent = x;
        }

        /// <summary>
        /// Inserts object into the tree.
        /// </summary>
        /// <param name="val">Value to insert.</param>
        /// 
        /// <param name="allowDuplicates">If true, will create a new node even if equal node 
        /// already exists in the tree. If false, behavior depends on replaceIfDuplicate.
        /// </param>
        /// 
        /// <param name="replaceIfDuplicate">Matters only if node equal to val exists in the 
        /// tree and allowDuplicates is false. If replaceIfDuplicate is true, val replaces
        /// value of existing node. Otherwise, tree does not change.
        /// </param>
        /// <returns>
        /// <list type="table">
        /// <item><term>result.NewNode</term><description>true if new node was inserted to the tree</description></item>
        /// <item><term>result.Node</term><description>contains newly inserted node if any, or node equal to val</description> </item>
        /// </list>
        /// </returns>
        public InsertResult Insert(object val, bool allowDuplicates, bool replaceIfDuplicate)
        {
            RbTreeNode newNode = new RbTreeNode(val);
            InsertResult result = BinaryInsert(newNode, allowDuplicates, replaceIfDuplicate);

            if (!result.NewNode) return result;

            RbTreeNode x = newNode;
            x.IsRed = true;

            while (x.Parent.IsRed)
            {
                if (x.Parent == x.Parent.Parent.Left)
                {
                    RbTreeNode y = x.Parent.Parent.Right;

                    if (y.IsRed)
                    {
                        x.Parent.IsRed = false;
                        y.IsRed = false;
                        x.Parent.Parent.IsRed = true;
                        x = x.Parent.Parent;
                    }
                    else
                    {
                        if (x == x.Parent.Right)
                        {
                            x = x.Parent;
                            LeftRotate(x);
                        }

                        x.Parent.IsRed = false;
                        x.Parent.Parent.IsRed = true;
                        RightRotate(x.Parent.Parent);
                    }
                }
                else
                { /* case for x.Parent == x.Parent.Parent.Right */
                    RbTreeNode y = x.Parent.Parent.Left;

                    if (y.IsRed)
                    {
                        x.Parent.IsRed = false;
                        y.IsRed = false;
                        x.Parent.Parent.IsRed = true;
                        x = x.Parent.Parent;
                    }
                    else
                    {
                        if (x == x.Parent.Left)
                        {
                            x = x.Parent;
                            RightRotate(x);
                        }
                        x.Parent.IsRed = false;
                        x.Parent.Parent.IsRed = true;
                        LeftRotate(x.Parent.Parent);
                    }
                }
            }

            _root.Left.IsRed = false;

            return new InsertResult(true, newNode);
        }

        private void DeleteFixUp(RbTreeNode x)
        {
            RbTreeNode root = _root.Left;

            while ((!x.IsRed) && (root != x))
            {
                if (x == x.Parent.Left)
                {
                    RbTreeNode w = x.Parent.Right;

                    if (w.IsRed)
                    {
                        w.IsRed = false;
                        x.Parent.IsRed = true;
                        LeftRotate(x.Parent);
                        w = x.Parent.Right;
                    }

                    if ((!w.Right.IsRed) && (!w.Left.IsRed))
                    {
                        w.IsRed = true;
                        x = x.Parent;
                    }
                    else
                    {
                        if (!w.Right.IsRed)
                        {
                            w.Left.IsRed = false;
                            w.IsRed = true;
                            RightRotate(w);
                            w = x.Parent.Right;
                        }

                        w.IsRed = x.Parent.IsRed;
                        x.Parent.IsRed = false;
                        w.Right.IsRed = false;
                        LeftRotate(x.Parent);

                        x = root; /* this is to exit while loop */
                    }
                }
                else
                { /* the code below is has Left and Right switched from above */
                    RbTreeNode w = x.Parent.Left;
                    if (w.IsRed)
                    {
                        w.IsRed = false;
                        x.Parent.IsRed = true;
                        RightRotate(x.Parent);
                        w = x.Parent.Left;
                    }
                    if ((!w.Right.IsRed) && (!w.Left.IsRed))
                    {
                        w.IsRed = true;
                        x = x.Parent;
                    }
                    else
                    {
                        if (!w.Left.IsRed)
                        {
                            w.Right.IsRed = false;
                            w.IsRed = true;
                            LeftRotate(w);
                            w = x.Parent.Left;
                        }
                        w.IsRed = x.Parent.IsRed;
                        x.Parent.IsRed = false;
                        w.Left.IsRed = false;
                        RightRotate(x.Parent);
                        x = root; /* this is to exit while loop */
                    }
                }
            }

            x.IsRed = false;
        }

        /// <summary>
        /// Logically first node in the tree.
        /// </summary>
        /// <remarks>
        /// RbTreeNode.Nil if the tree is empty.
        /// </remarks>
        public RbTreeNode First
        {
            get
            {
                RbTreeNode x = Root;
                while (x.Left != RbTreeNode.Nil)
                {
                    x = x.Left;
                }

                return x;
            }
        }

        /// <summary>
        /// Logically last node in the tree.
        /// </summary>
        /// <remarks>
        /// RbTreeNode.Nil if the tree is empty.
        /// </remarks>
        public RbTreeNode Last
        {
            get
            {
                RbTreeNode x = Root;
                while (x.Right != RbTreeNode.Nil)
                {
                    x = x.Right;
                }

                return x;
            }
        }

        /// <summary>
        /// Node in the tree that follows given node.
        /// </summary>
        /// <remarks>
        /// If x is logically last node in the tree, returns RbTreeNode.Nil.
        /// </remarks>
        public RbTreeNode Next(RbTreeNode x)
        {
            RbTreeNode root = _root;
            RbTreeNode y = x.Right;

            if (y != RbTreeNode.Nil)
            {
                while (y.Left != RbTreeNode.Nil)
                { /* returns the minimum of the right subtree of x */
                    y = y.Left;
                }

                return (y);
            }
            else
            {
                y = x.Parent;

                while (x == y.Right)
                { /* sentinel used instead of checking for nil */
                    x = y;
                    y = y.Parent;
                }

                if (y == root) return (RbTreeNode.Nil);
                return (y);
            }
        }

        /// <summary>
        /// Node in the tree that precedes given node.
        /// </summary>
        /// <remarks>
        /// If x is logically fisrt node, returns RbTreeNode.Nil.
        /// </remarks>
        public RbTreeNode Prev(RbTreeNode x)
        {
            RbTreeNode root = _root;
            RbTreeNode y = x.Left;


            if (y != RbTreeNode.Nil)
            {
                while (y.Right != RbTreeNode.Nil)
                { /* returns the maximum of the left subtree of x */
                    y = y.Right;
                }
                return (y);
            }
            else
            {
                y = x.Parent;
                while (x == y.Left)
                {
                    if (y == root) return RbTreeNode.Nil;
                    x = y;
                    y = y.Parent;
                }
                return (y);
            }
        }

        /// <summary>
        /// Returns fisrt node whose value is not less than parameter.
        /// </summary>
        /// <param name="val">The value to look for.</param>
        /// <returns>The node, if found, or RbTreeNode.Nil.</returns>
        public RbTreeNode LowerBound(object val)
        {
            RbTreeNode x = Root;
            RbTreeNode y = RbTreeNode.Nil; // previous value of x

            while (x != RbTreeNode.Nil)
            {
                if (_comparer.Compare(val, x.Value) <= 0) // val <= x.Value
                {
                    y = x;
                    x = x.Left;
                }
                else
                {
                    x = x.Right;
                }
            }

            return y;
        }

        /// <summary>
        /// Returns first node whose value is strictly greater than parameter.
        /// </summary>
        /// <param name="val">The value to look for.</param>
        /// <returns>The node, if found, or RbTreeNode.Nil.</returns>
        public RbTreeNode UpperBound(object val)
        {
            RbTreeNode x = Root;
            RbTreeNode y = RbTreeNode.Nil;

            while (x != RbTreeNode.Nil)
            {
                if (_comparer.Compare(val, x.Value) < 0) // val < x.Value
                {
                    y = x;
                    x = x.Left;
                }
                else
                {
                    x = x.Right;
                }
            }

            return y;
        }

        /// <summary>
        /// Removes node from the tree, re-arranging red-black structure as needed.
        /// </summary>
        public void Erase(RbTreeNode z)
        {
            RbTreeNode y;
            RbTreeNode x;
            RbTreeNode root = _root;

            if ((z.Left == RbTreeNode.Nil) || (z.Right == RbTreeNode.Nil))
            {
                y = z;
            }
            else
            {
                y = Next(z);
            }

            x = (y.Left == RbTreeNode.Nil) ? y.Right : y.Left;
            if (_root == (x.Parent = y.Parent))  /* assignment of y.p to x.p is intentional */
            {
                _root.Left = x;
            }
            else
            {
                if (y == y.Parent.Left)
                {
                    y.Parent.Left = x;
                }
                else
                {
                    y.Parent.Right = x;
                }
            }

            if (!(y.IsRed))
                DeleteFixUp(x);

            if (y != z)
            {
                y.Left = z.Left;
                y.Right = z.Right;
                y.Parent = z.Parent;
                y.IsRed = z.IsRed;
                z.Left.Parent = z.Right.Parent = y;

                if (z == z.Parent.Left)
                {
                    z.Parent.Left = y;
                }
                else
                {
                    z.Parent.Right = y;
                }
            }

            IDisposable zVal = z.Value as IDisposable;
            if (zVal != null)
                zVal.Dispose();
        }

        /// <summary>
        /// Removes object(s) from the tree.
        /// </summary>
        /// <param name="val">Object to remove.</param>
        /// <returns>Number of nodes removed.</returns>
        /// <remarks>
        /// Finds and removes all nodes in the tree whose values compare equal to val.
        /// Returns number of removed nodes (possibly zero).
        /// </remarks>
        public int Erase(object val)
        {
            RbTreeNode first = LowerBound(val);
            RbTreeNode last = UpperBound(val);
            int nDeleted = 0;

            while (first != last)
            {
                RbTreeNode temp = first;
                first = Next(first);
                Erase(temp);
                ++nDeleted;
            }

            return nDeleted;
        }
    }
}
