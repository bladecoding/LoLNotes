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
using System.Reflection.Emit;
using antlr.collections;

namespace FluorineFx.Expression
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	abstract class NodeWithArguments : BaseNode
    {
        private BaseNode[] args;

        public NodeWithArguments()
        {
        }

        /// <summary>
        /// Initializes the node. 
        /// </summary>
        private void InitializeNode()
        {
            if (args == null)
            {
                lock (this)
                {
                    if (args == null)
                    {
                        ArrayList argList = new ArrayList();

                        AST node = this.getFirstChild();

                        while (node != null)
                        {
                            argList.Add(node);
                            node = node.getNextSibling();
                        }
                        args = (BaseNode[])argList.ToArray(typeof(BaseNode));
                    }
                }
            }
        }

        /// <summary>
        /// Asserts the argument count.
        /// </summary>
        /// <param name="requiredCount">The required count.</param>
        protected void AssertArgumentCount(int requiredCount)
        {
            InitializeNode();
            if (requiredCount != args.Length)
            {
                throw new ArgumentException("This expression node requires exactly " +
                                            requiredCount + " argument(s) and " +
                                            args.Length + " were specified.");
            }
        }

        /// <summary>
        /// Resolves the arguments.
        /// </summary>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>An array of argument values</returns>
        protected object[] ResolveArguments(EvaluationContext evalContext)
        {
            InitializeNode();
            object[] values = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                values[i] = ResolveArgument(i, evalContext);
            }
            return values;
        }


        /// <summary>
        /// Resolves the argument.
        /// </summary>
        /// <param name="position">Argument position.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Resolved argument value.</returns>
        protected object ResolveArgument(int position, EvaluationContext evalContext)
        {
            InitializeNode();
            return ((BaseNode)args[position]).EvaluateInternal(evalContext.ThisContext, evalContext);
        }
    }
}