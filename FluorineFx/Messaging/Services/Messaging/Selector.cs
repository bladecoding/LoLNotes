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
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using log4net;
using FluorineFx.Expression;

namespace FluorineFx.Messaging.Services.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    class Selector : IComparable
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(Selector));
		private EvaluateInvoker _evaluateMethod;
        readonly string _expression;

		public Selector(string expression, EvaluateInvoker evaluateMethod)
		{
			_evaluateMethod = evaluateMethod;
            _expression = expression;
		}

        public string Expression
        {
            get { return _expression; }
        } 

		public bool Evaluate(object root, IDictionary variables)
		{
			object result = _evaluateMethod(root, variables);
            if (result is bool)
                return (bool)result;
            else
            {
                if (log.IsDebugEnabled)
                    log.Debug(__Res.GetString(__Res.Selector_InvalidResult));
                return false;
            }
		}

		public static Selector CreateSelector(string expression)
		{
			IExpression expressionObj = FluorineFx.Expression.Expression.Parse(expression);

#if !(NET_1_1)
            if ( expressionObj is IExpressionGenerator )
			{
                /*
				IExpressionGenerator expressionGenerator = expressionObj as IExpressionGenerator;
				bool canSkipChecks = SecurityManager.IsGranted(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
				DynamicMethod method = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(IDictionary) }, typeof(Selector).Module, canSkipChecks);
				ILGenerator ilg = method.GetILGenerator();
				expressionGenerator.Emit(ilg);
				EvaluateInvoker evaluateInvoker = (EvaluateInvoker)method.CreateDelegate(typeof(EvaluateInvoker));
                */
				return new Selector(expression, new EvaluateInvoker(expressionObj.Evaluate));
			}
			else
				return new Selector(expression, new EvaluateInvoker(expressionObj.Evaluate));
#else
			return new Selector(expression, new EvaluateInvoker(expressionObj.Evaluate));
#endif
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Selector)
            {
                Selector other = (Selector)obj;
                return string.Equals(other.Expression, _expression) ? 0 : -1;
            }
            return -1;            
        }

        #endregion

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return _expression.GetHashCode();
        }
    }
}
