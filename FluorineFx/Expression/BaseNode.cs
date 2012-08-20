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

namespace FluorineFx.Expression
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal abstract class BaseNode : FluorineAST, IExpression, IExpressionGenerator
	{
		#region EvaluationContext class

		/// <summary>
		/// Holds the state during evaluating an expression.
		/// </summary>
		public class EvaluationContext
		{
			#region Holder classes

			private class ThisContextHolder : IDisposable
			{
				private readonly EvaluationContext owner;
				private readonly object savedThisContext;

				public ThisContextHolder(EvaluationContext owner)
				{
					this.owner = owner;
					this.savedThisContext = owner.ThisContext;
				}

				public void Dispose()
				{
					owner.ThisContext = savedThisContext;
				}
			}

			#endregion

			/// <summary>
			/// Gets/Sets the root context of the current evaluation
			/// </summary>
			public object RootContext;
			/// <summary>
			/// Gets the type of the RootContext
			/// </summary>
			public Type RootContextType { get { return (RootContext == null) ? null : RootContext.GetType(); } }
			/// <summary>
			/// Gets/Sets the current context of the current evaluation
			/// </summary>
			public object ThisContext;
			/// <summary>
			/// Gets/Sets global variables of the current evaluation
			/// </summary>
			public IDictionary Variables;

			/// <summary>
			/// Initializes a new EvaluationContext instance.
			/// </summary>
			/// <param name="rootContext">The root context for this evaluation</param>
			/// <param name="globalVariables">dictionary of global variables used during this evaluation</param>
			public EvaluationContext(object rootContext, IDictionary globalVariables)
			{
				this.RootContext = rootContext;
				this.ThisContext = rootContext;
				this.Variables = globalVariables;
			}

			/// <summary>
			/// Switches current ThisContext.
			/// </summary>
			public IDisposable SwitchThisContext()
			{
				return new ThisContextHolder(this);
			}
		}

		#endregion

		public BaseNode()
		{
		}

		#region IExpression Members

		public object Evaluate(object context, IDictionary variables)
		{
			EvaluationContext evalContext = new EvaluationContext(context, variables );
			return Evaluate( context, evalContext );
		}

		#endregion

		/// <summary>
		/// Returns node's value for the given context.
		/// </summary>
		/// <returns>Node's value.</returns>
		protected abstract object Evaluate(object context, EvaluationContext evalContext);

		/// <summary>
		/// Called internally during expression evaluation
		/// </summary>
		/// <param name="context">Object to evaluate node against.</param>
		/// <param name="evalContext">Current expression evaluation context.</param>
		/// <returns></returns>
		protected internal object EvaluateInternal(object context, EvaluationContext evalContext)
		{
			return Evaluate(context, evalContext);
		}

		#region IExpressionGenerator Members

		public virtual void Emit(ILGenerator ilg)
		{
		}

		#endregion
	}
}
