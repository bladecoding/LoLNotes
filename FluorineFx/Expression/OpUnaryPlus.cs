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
using FluorineFx.Util;

namespace FluorineFx.Expression
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal class OpUnaryPlus : UnaryOperator
	{
		public OpUnaryPlus()
		{
		}

		protected override object Evaluate(object context, EvaluationContext evalContext)
		{
			object n = Operand.EvaluateInternal( context, evalContext );
			if (!NumberUtils.IsNumber(n))
				throw new ArgumentException("Specified operand is not a number. Only numbers support unary minus operator.");
			return n;
		}
	}
}
