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
using System.IO;

using antlr;
using antlr.collections;

namespace FluorineFx.Expression
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class Expression : BaseNode
	{
		public Expression()
		{
		}

		private class FluorineASTFactory : ASTFactory
		{
			public FluorineASTFactory(Type t) : base(t.FullName)
			{
				base.defaultASTNodeTypeObject_ = t;
				base.typename2creator_ = new Hashtable(32, 0.3f);
				base.typename2creator_[t.FullName] = FluorineAST.Creator;
			}
		}

		private class FluorineExpressionParser : ExpressionParser
		{
			public FluorineExpressionParser(TokenStream lexer) : base(lexer)
			{
				base.astFactory = new FluorineASTFactory(typeof(FluorineAST));
				base.initialize();
			}
		}

		public static IExpression Parse(string expression)
		{
			if (expression!=null && expression != string.Empty)
			{
				ExpressionLexer lexer = new ExpressionLexer(new StringReader(expression));
				ExpressionParser parser = new FluorineExpressionParser(lexer);

				parser.expr();
				return parser.getAST() as IExpression;
			}
			else
			{
				return new Expression();
			}
		}

		protected override object Evaluate(object context, EvaluationContext evalContext)
		{
			object result = context;
			if (this.getNumberOfChildren() > 0)
			{
				AST node = this.getFirstChild();
				while (node != null)
				{
					result = ((BaseNode)node).EvaluateInternal( result, evalContext );
					node = node.getNextSibling();
				}
			}
			return result;
		}
	}
}
