// $ANTLR 2.7.6 (2005-12-22): "selector.g" -> "ExpressionParser.cs"$

namespace FluorineFx.Expression
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	using AST                      = antlr.collections.AST;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
	internal 	class ExpressionParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int EXPR = 4;
		public const int OPERAND = 5;
		public const int FALSE = 6;
		public const int TRUE = 7;
		public const int AND = 8;
		public const int OR = 9;
		public const int NOT = 10;
		public const int IN = 11;
		public const int IS = 12;
		public const int BETWEEN = 13;
		public const int LIKE = 14;
		public const int NULL_LITERAL = 15;
		public const int PLUS = 16;
		public const int MINUS = 17;
		public const int STAR = 18;
		public const int DIV = 19;
		public const int MOD = 20;
		public const int POWER = 21;
		public const int LPAREN = 22;
		public const int RPAREN = 23;
		public const int POUND = 24;
		public const int ID = 25;
		public const int COMMA = 26;
		public const int INTEGER_LITERAL = 27;
		public const int HEXADECIMAL_INTEGER_LITERAL = 28;
		public const int REAL_LITERAL = 29;
		public const int STRING_LITERAL = 30;
		public const int LITERAL_date = 31;
		public const int EQUAL = 32;
		public const int NOT_EQUAL = 33;
		public const int LESS_THAN = 34;
		public const int LESS_THAN_OR_EQUAL = 35;
		public const int GREATER_THAN = 36;
		public const int GREATER_THAN_OR_EQUAL = 37;
		public const int WS = 38;
		public const int AT = 39;
		public const int QMARK = 40;
		public const int DOLLAR = 41;
		public const int LBRACKET = 42;
		public const int RBRACKET = 43;
		public const int LCURLY = 44;
		public const int RCURLY = 45;
		public const int SEMI = 46;
		public const int COLON = 47;
		public const int DOT_ESCAPED = 48;
		public const int APOS = 49;
		public const int NUMERIC_LITERAL = 50;
		public const int DECIMAL_DIGIT = 51;
		public const int INTEGER_TYPE_SUFFIX = 52;
		public const int HEX_DIGIT = 53;
		public const int EXPONENT_PART = 54;
		public const int SIGN = 55;
		public const int REAL_TYPE_SUFFIX = 56;
		
		
    public override void reportError(RecognitionException ex)
    {
		//base.reportError(ex);
        throw ex;
    }

    public override void reportError(string error)
    {
		//base.reportError(error);
        throw new RecognitionException(error);
    }
    
    private string GetRelationalOperatorNodeType(string op)
    {
        switch (op.ToLower())
        {
            case "=" : return "FluorineFx.Expression.OpEqual";
            case "<>" : return "FluorineFx.Expression.OpNotEqual";
            case "<" : return "FluorineFx.Expression.OpLess";
            case "<=" : return "FluorineFx.Expression.OpLessOrEqual";
            case ">" : return "FluorineFx.Expression.OpGreater";
            case ">=" : return "FluorineFx.Expression.OpGreaterOrEqual";
            case "in" : return "FluorineFx.Expression.OpIn";
            case "is" : return "FluorineFx.Expression.OpIs";
            case "between" : return "FluorineFx.Expression.OpBetween";
            case "like" : return "FluorineFx.Expression.OpLike";
            default : 
                throw new ArgumentException("Node type for operator '" + op + "' is not defined.");
        }
    }
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
			initializeFactory();
		}
		
		
		protected ExpressionParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public ExpressionParser(TokenBuffer tokenBuf) : this(tokenBuf,2)
		{
		}
		
		protected ExpressionParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public ExpressionParser(TokenStream lexer) : this(lexer,2)
		{
		}
		
		public ExpressionParser(ParserSharedInputState state) : base(state,2)
		{
			initialize();
		}
		
	public void expr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST expr_AST = null;
		
		try {      // for error handling
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(Token.EOF_TYPE);
			expr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_0_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = expr_AST;
	}
	
	public void expression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST expression_AST = null;
		
		try {      // for error handling
			logicalOrExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			expression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_1_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = expression_AST;
	}
	
	public void logicalOrExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST logicalOrExpression_AST = null;
		
		try {      // for error handling
			logicalAndExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==OR))
					{
						FluorineFx.Expression.OpOR tmp2_AST = null;
						tmp2_AST = (FluorineFx.Expression.OpOR) astFactory.create(LT(1), "FluorineFx.Expression.OpOR");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp2_AST);
						match(OR);
						logicalAndExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop5_breakloop;
					}
					
				}
_loop5_breakloop:				;
			}    // ( ... )*
			logicalOrExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_1_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = logicalOrExpression_AST;
	}
	
	public void logicalAndExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST logicalAndExpression_AST = null;
		
		try {      // for error handling
			relationalExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==AND))
					{
						FluorineFx.Expression.OpAND tmp3_AST = null;
						tmp3_AST = (FluorineFx.Expression.OpAND) astFactory.create(LT(1), "FluorineFx.Expression.OpAND");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp3_AST);
						match(AND);
						relationalExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop8_breakloop;
					}
					
				}
_loop8_breakloop:				;
			}    // ( ... )*
			logicalAndExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = logicalAndExpression_AST;
	}
	
	public void relationalExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST relationalExpression_AST = null;
		FluorineFx.Expression.FluorineAST e1_AST = null;
		FluorineFx.Expression.FluorineAST op_AST = null;
		
		try {      // for error handling
			sumExpr();
			if (0 == inputState.guessing)
			{
				e1_AST = (FluorineFx.Expression.FluorineAST)returnAST;
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				switch ( LA(1) )
				{
				case IS:
				case EQUAL:
				case NOT_EQUAL:
				case LESS_THAN:
				case LESS_THAN_OR_EQUAL:
				case GREATER_THAN:
				case GREATER_THAN_OR_EQUAL:
				{
					relationalOperator();
					if (0 == inputState.guessing)
					{
						op_AST = (FluorineFx.Expression.FluorineAST)returnAST;
					}
					sumExpr();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					if (0==inputState.guessing)
					{
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,op_AST.getText(),GetRelationalOperatorNodeType(op_AST.getText())), (AST)relationalExpression_AST);
						currentAST.root = relationalExpression_AST;
						if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
							currentAST.child = relationalExpression_AST.getFirstChild();
						else
							currentAST.child = relationalExpression_AST;
						currentAST.advanceChildToEnd();
					}
					break;
				}
				case IN:
				{
					match(IN);
					listInitializer();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					if (0==inputState.guessing)
					{
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"IN",GetRelationalOperatorNodeType("IN")), (AST)relationalExpression_AST);
						currentAST.root = relationalExpression_AST;
						if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
							currentAST.child = relationalExpression_AST.getFirstChild();
						else
							currentAST.child = relationalExpression_AST;
						currentAST.advanceChildToEnd();
					}
					break;
				}
				case BETWEEN:
				{
					match(BETWEEN);
					betweenExpr();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					if (0==inputState.guessing)
					{
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"BETWEEN",GetRelationalOperatorNodeType("BETWEEN")), (AST)relationalExpression_AST);
						currentAST.root = relationalExpression_AST;
						if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
							currentAST.child = relationalExpression_AST.getFirstChild();
						else
							currentAST.child = relationalExpression_AST;
						currentAST.advanceChildToEnd();
					}
					break;
				}
				case LIKE:
				{
					match(LIKE);
					pattern();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					if (0==inputState.guessing)
					{
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
						relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"LIKE",GetRelationalOperatorNodeType("LIKE")), (AST)relationalExpression_AST);
						currentAST.root = relationalExpression_AST;
						if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
							currentAST.child = relationalExpression_AST.getFirstChild();
						else
							currentAST.child = relationalExpression_AST;
						currentAST.advanceChildToEnd();
					}
					break;
				}
				case EOF:
				case AND:
				case OR:
				case RPAREN:
				case COMMA:
				{
					break;
				}
				default:
					if ((LA(1)==NOT) && (LA(2)==IN))
					{
						match(NOT);
						match(IN);
						listInitializer();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						if (0==inputState.guessing)
						{
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"NOT","FluorineFx.Expression.OpNOT"), (AST)(FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"IN",GetRelationalOperatorNodeType("IN")), (AST)relationalExpression_AST));
							currentAST.root = relationalExpression_AST;
							if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
								currentAST.child = relationalExpression_AST.getFirstChild();
							else
								currentAST.child = relationalExpression_AST;
							currentAST.advanceChildToEnd();
						}
					}
					else if ((LA(1)==NOT) && (LA(2)==BETWEEN)) {
						match(NOT);
						match(BETWEEN);
						betweenExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						if (0==inputState.guessing)
						{
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"NOT","FluorineFx.Expression.OpNOT"), (AST)(FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"BETWEEN",GetRelationalOperatorNodeType("BETWEEN")), (AST)relationalExpression_AST));
							currentAST.root = relationalExpression_AST;
							if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
								currentAST.child = relationalExpression_AST.getFirstChild();
							else
								currentAST.child = relationalExpression_AST;
							currentAST.advanceChildToEnd();
						}
					}
					else if ((LA(1)==NOT) && (LA(2)==LIKE)) {
						match(NOT);
						match(LIKE);
						pattern();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
						if (0==inputState.guessing)
						{
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
							relationalExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"NOT","FluorineFx.Expression.OpNOT"), (AST)(FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"LIKE",GetRelationalOperatorNodeType("LIKE")), (AST)relationalExpression_AST));
							currentAST.root = relationalExpression_AST;
							if ( (null != relationalExpression_AST) && (null != relationalExpression_AST.getFirstChild()) )
								currentAST.child = relationalExpression_AST.getFirstChild();
							else
								currentAST.child = relationalExpression_AST;
							currentAST.advanceChildToEnd();
						}
					}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				break; }
			}
			relationalExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = relationalExpression_AST;
	}
	
	public void sumExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST sumExpr_AST = null;
		
		try {      // for error handling
			prodExpr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PLUS||LA(1)==MINUS))
					{
						{
							if ((LA(1)==PLUS))
							{
								FluorineFx.Expression.OpADD tmp13_AST = null;
								tmp13_AST = (FluorineFx.Expression.OpADD) astFactory.create(LT(1), "FluorineFx.Expression.OpADD");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp13_AST);
								match(PLUS);
							}
							else if ((LA(1)==MINUS)) {
								FluorineFx.Expression.OpSUBTRACT tmp14_AST = null;
								tmp14_AST = (FluorineFx.Expression.OpSUBTRACT) astFactory.create(LT(1), "FluorineFx.Expression.OpSUBTRACT");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp14_AST);
								match(MINUS);
							}
							else
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							
						}
						prodExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop14_breakloop;
					}
					
				}
_loop14_breakloop:				;
			}    // ( ... )*
			sumExpr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_4_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = sumExpr_AST;
	}
	
	public void relationalOperator() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST relationalOperator_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case EQUAL:
			{
				FluorineFx.Expression.FluorineAST tmp15_AST = null;
				tmp15_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp15_AST);
				match(EQUAL);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case NOT_EQUAL:
			{
				FluorineFx.Expression.FluorineAST tmp16_AST = null;
				tmp16_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp16_AST);
				match(NOT_EQUAL);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case LESS_THAN:
			{
				FluorineFx.Expression.FluorineAST tmp17_AST = null;
				tmp17_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp17_AST);
				match(LESS_THAN);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case LESS_THAN_OR_EQUAL:
			{
				FluorineFx.Expression.FluorineAST tmp18_AST = null;
				tmp18_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp18_AST);
				match(LESS_THAN_OR_EQUAL);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case GREATER_THAN:
			{
				FluorineFx.Expression.FluorineAST tmp19_AST = null;
				tmp19_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp19_AST);
				match(GREATER_THAN);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case GREATER_THAN_OR_EQUAL:
			{
				FluorineFx.Expression.FluorineAST tmp20_AST = null;
				tmp20_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp20_AST);
				match(GREATER_THAN_OR_EQUAL);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case IS:
			{
				FluorineFx.Expression.FluorineAST tmp21_AST = null;
				tmp21_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
				astFactory.addASTChild(ref currentAST, (AST)tmp21_AST);
				match(IS);
				relationalOperator_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_5_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = relationalOperator_AST;
	}
	
	public void listInitializer() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST listInitializer_AST = null;
		
		try {      // for error handling
			FluorineFx.Expression.ListInitializerNode tmp22_AST = null;
			tmp22_AST = (FluorineFx.Expression.ListInitializerNode) astFactory.create(LT(1), "FluorineFx.Expression.ListInitializerNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp22_AST);
			match(LPAREN);
			primaryExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						primaryExpression();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop38_breakloop;
					}
					
				}
_loop38_breakloop:				;
			}    // ( ... )*
			match(RPAREN);
			listInitializer_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = listInitializer_AST;
	}
	
	public void betweenExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST betweenExpr_AST = null;
		FluorineFx.Expression.FluorineAST e1_AST = null;
		FluorineFx.Expression.FluorineAST e2_AST = null;
		
		try {      // for error handling
			sumExpr();
			if (0 == inputState.guessing)
			{
				e1_AST = (FluorineFx.Expression.FluorineAST)returnAST;
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			FluorineFx.Expression.ListInitializerNode tmp25_AST = null;
			tmp25_AST = (FluorineFx.Expression.ListInitializerNode) astFactory.create(LT(1), "FluorineFx.Expression.ListInitializerNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp25_AST);
			match(AND);
			sumExpr();
			if (0 == inputState.guessing)
			{
				e2_AST = (FluorineFx.Expression.FluorineAST)returnAST;
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			betweenExpr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = betweenExpr_AST;
	}
	
	public void pattern() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST pattern_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case EOF:
			case AND:
			case OR:
			case RPAREN:
			case COMMA:
			{
				pattern_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case FALSE:
			case TRUE:
			case NULL_LITERAL:
			case INTEGER_LITERAL:
			case HEXADECIMAL_INTEGER_LITERAL:
			case REAL_LITERAL:
			case STRING_LITERAL:
			case LITERAL_date:
			{
				literal();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				pattern_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case POUND:
			case ID:
			{
				functionOrVar();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				pattern_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = pattern_AST;
	}
	
	public void prodExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST prodExpr_AST = null;
		
		try {      // for error handling
			powExpr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if (((LA(1) >= STAR && LA(1) <= MOD)))
					{
						{
							switch ( LA(1) )
							{
							case STAR:
							{
								FluorineFx.Expression.OpMULTIPLY tmp26_AST = null;
								tmp26_AST = (FluorineFx.Expression.OpMULTIPLY) astFactory.create(LT(1), "FluorineFx.Expression.OpMULTIPLY");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp26_AST);
								match(STAR);
								break;
							}
							case DIV:
							{
								FluorineFx.Expression.OpDIVIDE tmp27_AST = null;
								tmp27_AST = (FluorineFx.Expression.OpDIVIDE) astFactory.create(LT(1), "FluorineFx.Expression.OpDIVIDE");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp27_AST);
								match(DIV);
								break;
							}
							case MOD:
							{
								FluorineFx.Expression.OpMODULUS tmp28_AST = null;
								tmp28_AST = (FluorineFx.Expression.OpMODULUS) astFactory.create(LT(1), "FluorineFx.Expression.OpMODULUS");
								astFactory.makeASTRoot(ref currentAST, (AST)tmp28_AST);
								match(MOD);
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
						powExpr();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(ref currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop18_breakloop;
					}
					
				}
_loop18_breakloop:				;
			}    // ( ... )*
			prodExpr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_6_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = prodExpr_AST;
	}
	
	public void powExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST powExpr_AST = null;
		
		try {      // for error handling
			unaryExpression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			{
				if ((LA(1)==POWER))
				{
					FluorineFx.Expression.OpPOWER tmp29_AST = null;
					tmp29_AST = (FluorineFx.Expression.OpPOWER) astFactory.create(LT(1), "FluorineFx.Expression.OpPOWER");
					astFactory.makeASTRoot(ref currentAST, (AST)tmp29_AST);
					match(POWER);
					unaryExpression();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
				}
				else if ((tokenSet_7_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			powExpr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = powExpr_AST;
	}
	
	public void unaryExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST unaryExpression_AST = null;
		
		try {      // for error handling
			if ((LA(1)==NOT||LA(1)==PLUS||LA(1)==MINUS))
			{
				{
					switch ( LA(1) )
					{
					case PLUS:
					{
						FluorineFx.Expression.OpUnaryPlus tmp30_AST = null;
						tmp30_AST = (FluorineFx.Expression.OpUnaryPlus) astFactory.create(LT(1), "FluorineFx.Expression.OpUnaryPlus");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp30_AST);
						match(PLUS);
						break;
					}
					case MINUS:
					{
						FluorineFx.Expression.OpUnaryMinus tmp31_AST = null;
						tmp31_AST = (FluorineFx.Expression.OpUnaryMinus) astFactory.create(LT(1), "FluorineFx.Expression.OpUnaryMinus");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp31_AST);
						match(MINUS);
						break;
					}
					case NOT:
					{
						FluorineFx.Expression.OpNOT tmp32_AST = null;
						tmp32_AST = (FluorineFx.Expression.OpNOT) astFactory.create(LT(1), "FluorineFx.Expression.OpNOT");
						astFactory.makeASTRoot(ref currentAST, (AST)tmp32_AST);
						match(NOT);
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				unaryExpression();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				unaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else if ((tokenSet_8_.member(LA(1)))) {
				primaryExpression();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				unaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = unaryExpression_AST;
	}
	
	public void primaryExpression() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST primaryExpression_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LPAREN:
			{
				parenExpr();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				primaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case FALSE:
			case TRUE:
			case NULL_LITERAL:
			case INTEGER_LITERAL:
			case HEXADECIMAL_INTEGER_LITERAL:
			case REAL_LITERAL:
			case STRING_LITERAL:
			case LITERAL_date:
			{
				literal();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				primaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case POUND:
			case ID:
			{
				functionOrVar();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				if (0==inputState.guessing)
				{
					primaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
					primaryExpression_AST = (FluorineFx.Expression.FluorineAST) astFactory.make((AST)(FluorineFx.Expression.FluorineAST) astFactory.create(EXPR,"expression","FluorineFx.Expression.Expression"), (AST)primaryExpression_AST);
					currentAST.root = primaryExpression_AST;
					if ( (null != primaryExpression_AST) && (null != primaryExpression_AST.getFirstChild()) )
						currentAST.child = primaryExpression_AST.getFirstChild();
					else
						currentAST.child = primaryExpression_AST;
					currentAST.advanceChildToEnd();
				}
				primaryExpression_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = primaryExpression_AST;
	}
	
	public void parenExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST parenExpr_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			match(RPAREN);
			parenExpr_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = parenExpr_AST;
	}
	
	public void literal() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST literal_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case NULL_LITERAL:
			{
				FluorineFx.Expression.NullLiteralNode tmp35_AST = null;
				tmp35_AST = (FluorineFx.Expression.NullLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.NullLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp35_AST);
				match(NULL_LITERAL);
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case INTEGER_LITERAL:
			{
				FluorineFx.Expression.IntLiteralNode tmp36_AST = null;
				tmp36_AST = (FluorineFx.Expression.IntLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.IntLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp36_AST);
				match(INTEGER_LITERAL);
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case HEXADECIMAL_INTEGER_LITERAL:
			{
				FluorineFx.Expression.HexLiteralNode tmp37_AST = null;
				tmp37_AST = (FluorineFx.Expression.HexLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.HexLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp37_AST);
				match(HEXADECIMAL_INTEGER_LITERAL);
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case REAL_LITERAL:
			{
				FluorineFx.Expression.RealLiteralNode tmp38_AST = null;
				tmp38_AST = (FluorineFx.Expression.RealLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.RealLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp38_AST);
				match(REAL_LITERAL);
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case STRING_LITERAL:
			{
				FluorineFx.Expression.StringLiteralNode tmp39_AST = null;
				tmp39_AST = (FluorineFx.Expression.StringLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.StringLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp39_AST);
				match(STRING_LITERAL);
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case FALSE:
			case TRUE:
			{
				boolLiteral();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			case LITERAL_date:
			{
				dateLiteral();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				literal_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = literal_AST;
	}
	
	public void functionOrVar() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST functionOrVar_AST = null;
		
		try {      // for error handling
			bool synPredMatched28 = false;
			if (((LA(1)==POUND)))
			{
				int _m28 = mark();
				synPredMatched28 = true;
				inputState.guessing++;
				try {
					{
						match(POUND);
						match(ID);
						match(LPAREN);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched28 = false;
				}
				rewind(_m28);
				inputState.guessing--;
			}
			if ( synPredMatched28 )
			{
				function();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				functionOrVar_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else if ((LA(1)==ID)) {
				var();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(ref currentAST, (AST)returnAST);
				}
				functionOrVar_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = functionOrVar_AST;
	}
	
	public void function() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST function_AST = null;
		
		try {      // for error handling
			match(POUND);
			FluorineFx.Expression.FunctionNode tmp41_AST = null;
			tmp41_AST = (FluorineFx.Expression.FunctionNode) astFactory.create(LT(1), "FluorineFx.Expression.FunctionNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp41_AST);
			match(ID);
			methodArgs();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			function_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = function_AST;
	}
	
	public void var() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST var_AST = null;
		
		try {      // for error handling
			FluorineFx.Expression.VariableNode tmp42_AST = null;
			tmp42_AST = (FluorineFx.Expression.VariableNode) astFactory.create(LT(1), "FluorineFx.Expression.VariableNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp42_AST);
			match(ID);
			var_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = var_AST;
	}
	
	public void methodArgs() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST methodArgs_AST = null;
		
		try {      // for error handling
			match(LPAREN);
			{
				if ((tokenSet_5_.member(LA(1))))
				{
					argument();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(ref currentAST, (AST)returnAST);
					}
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==COMMA))
							{
								match(COMMA);
								argument();
								if (0 == inputState.guessing)
								{
									astFactory.addASTChild(ref currentAST, (AST)returnAST);
								}
							}
							else
							{
								goto _loop33_breakloop;
							}
							
						}
_loop33_breakloop:						;
					}    // ( ... )*
				}
				else if ((LA(1)==RPAREN)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RPAREN);
			methodArgs_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = methodArgs_AST;
	}
	
	public void argument() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST argument_AST = null;
		
		try {      // for error handling
			expression();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(ref currentAST, (AST)returnAST);
			}
			argument_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_10_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = argument_AST;
	}
	
	public void boolLiteral() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST boolLiteral_AST = null;
		
		try {      // for error handling
			if ((LA(1)==TRUE))
			{
				FluorineFx.Expression.BooleanLiteralNode tmp46_AST = null;
				tmp46_AST = (FluorineFx.Expression.BooleanLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.BooleanLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp46_AST);
				match(TRUE);
				boolLiteral_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else if ((LA(1)==FALSE)) {
				FluorineFx.Expression.BooleanLiteralNode tmp47_AST = null;
				tmp47_AST = (FluorineFx.Expression.BooleanLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.BooleanLiteralNode");
				astFactory.addASTChild(ref currentAST, (AST)tmp47_AST);
				match(FALSE);
				boolLiteral_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = boolLiteral_AST;
	}
	
	public void dateLiteral() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = new ASTPair();
		FluorineFx.Expression.FluorineAST dateLiteral_AST = null;
		
		try {      // for error handling
			FluorineFx.Expression.DateLiteralNode tmp48_AST = null;
			tmp48_AST = (FluorineFx.Expression.DateLiteralNode) astFactory.create(LT(1), "FluorineFx.Expression.DateLiteralNode");
			astFactory.makeASTRoot(ref currentAST, (AST)tmp48_AST);
			match(LITERAL_date);
			match(LPAREN);
			FluorineFx.Expression.FluorineAST tmp50_AST = null;
			tmp50_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
			astFactory.addASTChild(ref currentAST, (AST)tmp50_AST);
			match(STRING_LITERAL);
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					FluorineFx.Expression.FluorineAST tmp52_AST = null;
					tmp52_AST = (FluorineFx.Expression.FluorineAST) astFactory.create(LT(1));
					astFactory.addASTChild(ref currentAST, (AST)tmp52_AST);
					match(STRING_LITERAL);
				}
				else if ((LA(1)==RPAREN)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(RPAREN);
			dateLiteral_AST = (FluorineFx.Expression.FluorineAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_9_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = dateLiteral_AST;
	}
	
	public new FluorineFx.Expression.FluorineAST getAST()
	{
		return (FluorineFx.Expression.FluorineAST) returnAST;
	}
	
	private void initializeFactory()
	{
		if (astFactory == null)
		{
			astFactory = new ASTFactory("FluorineFx.Expression.FluorineAST");
		}
		initializeASTFactory( astFactory );
	}
	static public void initializeASTFactory( ASTFactory factory )
	{
		factory.setMaxNodeType(56);
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""EXPR""",
		@"""OPERAND""",
		@"""FALSE""",
		@"""TRUE""",
		@"""AND""",
		@"""OR""",
		@"""NOT""",
		@"""IN""",
		@"""IS""",
		@"""BETWEEN""",
		@"""LIKE""",
		@"""NULL""",
		@"""PLUS""",
		@"""MINUS""",
		@"""STAR""",
		@"""DIV""",
		@"""MOD""",
		@"""POWER""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""POUND""",
		@"""ID""",
		@"""COMMA""",
		@"""INTEGER_LITERAL""",
		@"""HEXADECIMAL_INTEGER_LITERAL""",
		@"""REAL_LITERAL""",
		@"""STRING_LITERAL""",
		@"""date""",
		@"""EQUAL""",
		@"""NOT_EQUAL""",
		@"""LESS_THAN""",
		@"""LESS_THAN_OR_EQUAL""",
		@"""GREATER_THAN""",
		@"""GREATER_THAN_OR_EQUAL""",
		@"""WS""",
		@"""AT""",
		@"""QMARK""",
		@"""DOLLAR""",
		@"""LBRACKET""",
		@"""RBRACKET""",
		@"""LCURLY""",
		@"""RCURLY""",
		@"""SEMI""",
		@"""COLON""",
		@"""DOT_ESCAPED""",
		@"""APOS""",
		@"""NUMERIC_LITERAL""",
		@"""DECIMAL_DIGIT""",
		@"""INTEGER_TYPE_SUFFIX""",
		@"""HEX_DIGIT""",
		@"""EXPONENT_PART""",
		@"""SIGN""",
		@"""REAL_TYPE_SUFFIX"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 75497474L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 75497986L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 75498242L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 270658469634L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 4215506112L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 270658666242L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 270660501250L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 4215308480L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = { 270662598402L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = { 75497472L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	
}
}
