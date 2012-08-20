// $ANTLR 2.7.6 (2005-12-22): "selector.g" -> "ExpressionLexer.cs"$

namespace FluorineFx.Expression
{
	// Generate header specific to lexer CSharp file
	using System;
	using Stream                          = System.IO.Stream;
	using TextReader                      = System.IO.TextReader;
	using Hashtable                       = System.Collections.Hashtable;
	using Comparer                        = System.Collections.Comparer;
	
	using TokenStreamException            = antlr.TokenStreamException;
	using TokenStreamIOException          = antlr.TokenStreamIOException;
	using TokenStreamRecognitionException = antlr.TokenStreamRecognitionException;
	using CharStreamException             = antlr.CharStreamException;
	using CharStreamIOException           = antlr.CharStreamIOException;
	using ANTLRException                  = antlr.ANTLRException;
	using CharScanner                     = antlr.CharScanner;
	using InputBuffer                     = antlr.InputBuffer;
	using ByteBuffer                      = antlr.ByteBuffer;
	using CharBuffer                      = antlr.CharBuffer;
	using Token                           = antlr.Token;
	using IToken                          = antlr.IToken;
	using CommonToken                     = antlr.CommonToken;
	using SemanticException               = antlr.SemanticException;
	using RecognitionException            = antlr.RecognitionException;
	using NoViableAltForCharException     = antlr.NoViableAltForCharException;
	using MismatchedCharException         = antlr.MismatchedCharException;
	using TokenStream                     = antlr.TokenStream;
	using LexerSharedInputState           = antlr.LexerSharedInputState;
	using BitSet                          = antlr.collections.impl.BitSet;
	
	internal 	class ExpressionLexer : antlr.CharScanner	, TokenStream
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
		
		public ExpressionLexer(Stream ins) : this(new ByteBuffer(ins))
		{
		}
		
		public ExpressionLexer(TextReader r) : this(new CharBuffer(r))
		{
		}
		
		public ExpressionLexer(InputBuffer ib)		 : this(new LexerSharedInputState(ib))
		{
		}
		
		public ExpressionLexer(LexerSharedInputState state) : base(state)
		{
			initialize();
		}
		private void initialize()
		{
			caseSensitiveLiterals = true;
			setCaseSensitive(true);
			literals = new Hashtable(100, (float) 0.4, null, Comparer.Default);
			literals.Add("IS", 12);
			literals.Add("NULL", 15);
			literals.Add("LIKE", 14);
			literals.Add("date", 31);
			literals.Add("TRUE", 7);
			literals.Add("IN", 11);
			literals.Add("OR", 9);
			literals.Add("NOT", 10);
			literals.Add("AND", 8);
			literals.Add("FALSE", 6);
			literals.Add("BETWEEN", 13);
		}
		
		override public IToken nextToken()			//throws TokenStreamException
		{
			IToken theRetToken = null;
tryAgain:
			for (;;)
			{
				IToken _token = null;
				int _ttype = Token.INVALID_TYPE;
				resetText();
				try     // for char stream error handling
				{
					try     // for lexical error handling
					{
						switch ( cached_LA1 )
						{
						case '\t':  case '\n':  case '\r':  case ' ':
						{
							mWS(true);
							theRetToken = returnToken_;
							break;
						}
						case '@':
						{
							mAT(true);
							theRetToken = returnToken_;
							break;
						}
						case '?':
						{
							mQMARK(true);
							theRetToken = returnToken_;
							break;
						}
						case '$':
						{
							mDOLLAR(true);
							theRetToken = returnToken_;
							break;
						}
						case '#':
						{
							mPOUND(true);
							theRetToken = returnToken_;
							break;
						}
						case '(':
						{
							mLPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case ')':
						{
							mRPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case '[':
						{
							mLBRACKET(true);
							theRetToken = returnToken_;
							break;
						}
						case ']':
						{
							mRBRACKET(true);
							theRetToken = returnToken_;
							break;
						}
						case '{':
						{
							mLCURLY(true);
							theRetToken = returnToken_;
							break;
						}
						case '}':
						{
							mRCURLY(true);
							theRetToken = returnToken_;
							break;
						}
						case ',':
						{
							mCOMMA(true);
							theRetToken = returnToken_;
							break;
						}
						case ';':
						{
							mSEMI(true);
							theRetToken = returnToken_;
							break;
						}
						case ':':
						{
							mCOLON(true);
							theRetToken = returnToken_;
							break;
						}
						case '+':
						{
							mPLUS(true);
							theRetToken = returnToken_;
							break;
						}
						case '-':
						{
							mMINUS(true);
							theRetToken = returnToken_;
							break;
						}
						case '/':
						{
							mDIV(true);
							theRetToken = returnToken_;
							break;
						}
						case '*':
						{
							mSTAR(true);
							theRetToken = returnToken_;
							break;
						}
						case '%':
						{
							mMOD(true);
							theRetToken = returnToken_;
							break;
						}
						case '^':
						{
							mPOWER(true);
							theRetToken = returnToken_;
							break;
						}
						case '=':
						{
							mEQUAL(true);
							theRetToken = returnToken_;
							break;
						}
						case '\\':
						{
							mDOT_ESCAPED(true);
							theRetToken = returnToken_;
							break;
						}
						case '\'':
						{
							mSTRING_LITERAL(true);
							theRetToken = returnToken_;
							break;
						}
						case 'A':  case 'B':  case 'C':  case 'D':
						case 'E':  case 'F':  case 'G':  case 'H':
						case 'I':  case 'J':  case 'K':  case 'L':
						case 'M':  case 'N':  case 'O':  case 'P':
						case 'Q':  case 'R':  case 'S':  case 'T':
						case 'U':  case 'V':  case 'W':  case 'X':
						case 'Y':  case 'Z':  case '_':  case 'a':
						case 'b':  case 'c':  case 'd':  case 'e':
						case 'f':  case 'g':  case 'h':  case 'i':
						case 'j':  case 'k':  case 'l':  case 'm':
						case 'n':  case 'o':  case 'p':  case 'q':
						case 'r':  case 's':  case 't':  case 'u':
						case 'v':  case 'w':  case 'x':  case 'y':
						case 'z':
						{
							mID(true);
							theRetToken = returnToken_;
							break;
						}
						default:
							if ((cached_LA1=='<') && (cached_LA2=='>'))
							{
								mNOT_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='<') && (cached_LA2=='=')) {
								mLESS_THAN_OR_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='>') && (cached_LA2=='=')) {
								mGREATER_THAN_OR_EQUAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='0') && (cached_LA2=='x')) {
								mHEXADECIMAL_INTEGER_LITERAL(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='<') && (true)) {
								mLESS_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='>') && (true)) {
								mGREATER_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((tokenSet_0_.member(cached_LA1)) && (true)) {
								mNUMERIC_LITERAL(true);
								theRetToken = returnToken_;
							}
						else
						{
							if (cached_LA1==EOF_CHAR) { uponEOF(); returnToken_ = makeToken(Token.EOF_TYPE); }
				else {throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());}
						}
						break; }
						if ( null==returnToken_ ) goto tryAgain; // found SKIP token
						_ttype = returnToken_.Type;
						_ttype = testLiteralsTable(_ttype);
						returnToken_.Type = _ttype;
						return returnToken_;
					}
					catch (RecognitionException e) {
							throw new TokenStreamRecognitionException(e);
					}
				}
				catch (CharStreamException cse) {
					if ( cse is CharStreamIOException ) {
						throw new TokenStreamIOException(((CharStreamIOException)cse).io);
					}
					else {
						throw new TokenStreamException(cse.Message);
					}
				}
			}
		}
		
	public void mWS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = WS;
		
		{
			switch ( cached_LA1 )
			{
			case ' ':
			{
				match(' ');
				break;
			}
			case '\t':
			{
				match('\t');
				break;
			}
			case '\n':
			{
				match('\n');
				break;
			}
			case '\r':
			{
				match('\r');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		if (0==inputState.guessing)
		{
			_ttype = Token.SKIP;
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mAT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = AT;
		
		match('@');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mQMARK(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = QMARK;
		
		match('?');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOLLAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DOLLAR;
		
		match('$');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPOUND(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = POUND;
		
		match('#');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LPAREN;
		
		match('(');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RPAREN;
		
		match(')');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLBRACKET(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LBRACKET;
		
		match('[');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRBRACKET(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RBRACKET;
		
		match(']');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLCURLY(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LCURLY;
		
		match('{');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRCURLY(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RCURLY;
		
		match('}');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOMMA(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COMMA;
		
		match(',');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSEMI(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SEMI;
		
		match(';');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOLON(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COLON;
		
		match(':');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPLUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = PLUS;
		
		match('+');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMINUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = MINUS;
		
		match('-');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDIV(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DIV;
		
		match('/');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = STAR;
		
		match('*');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMOD(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = MOD;
		
		match('%');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPOWER(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = POWER;
		
		match('^');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mEQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = EQUAL;
		
		match("=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNOT_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NOT_EQUAL;
		
		match("<>");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLESS_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LESS_THAN;
		
		match('<');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLESS_THAN_OR_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LESS_THAN_OR_EQUAL;
		
		match("<=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mGREATER_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = GREATER_THAN;
		
		match('>');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mGREATER_THAN_OR_EQUAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = GREATER_THAN_OR_EQUAL;
		
		match(">=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOT_ESCAPED(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DOT_ESCAPED;
		
		match("\\.");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTRING_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = STRING_LITERAL;
		
		int _saveIndex = 0;
		_saveIndex = text.Length;
		match('\'');
		text.Length = _saveIndex;
		{    // ( ... )*
			for (;;)
			{
				if ((cached_LA1=='\'') && (cached_LA2=='\''))
				{
					mAPOS(false);
				}
				else if ((tokenSet_1_.member(cached_LA1))) {
					matchNot('\'');
				}
				else
				{
					goto _loop75_breakloop;
				}
				
			}
_loop75_breakloop:			;
		}    // ( ... )*
		_saveIndex = text.Length;
		match('\'');
		text.Length = _saveIndex;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mAPOS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = APOS;
		
		int _saveIndex = 0;
		_saveIndex = text.Length;
		match('\'');
		text.Length = _saveIndex;
		match('\'');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mID(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ID;
		
		{
			switch ( cached_LA1 )
			{
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':  case 'g':  case 'h':
			case 'i':  case 'j':  case 'k':  case 'l':
			case 'm':  case 'n':  case 'o':  case 'p':
			case 'q':  case 'r':  case 's':  case 't':
			case 'u':  case 'v':  case 'w':  case 'x':
			case 'y':  case 'z':
			{
				matchRange('a','z');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':  case 'G':  case 'H':
			case 'I':  case 'J':  case 'K':  case 'L':
			case 'M':  case 'N':  case 'O':  case 'P':
			case 'Q':  case 'R':  case 'S':  case 'T':
			case 'U':  case 'V':  case 'W':  case 'X':
			case 'Y':  case 'Z':
			{
				matchRange('A','Z');
				break;
			}
			case '_':
			{
				match('_');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		{    // ( ... )*
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case 'a':  case 'b':  case 'c':  case 'd':
				case 'e':  case 'f':  case 'g':  case 'h':
				case 'i':  case 'j':  case 'k':  case 'l':
				case 'm':  case 'n':  case 'o':  case 'p':
				case 'q':  case 'r':  case 's':  case 't':
				case 'u':  case 'v':  case 'w':  case 'x':
				case 'y':  case 'z':
				{
					matchRange('a','z');
					break;
				}
				case 'A':  case 'B':  case 'C':  case 'D':
				case 'E':  case 'F':  case 'G':  case 'H':
				case 'I':  case 'J':  case 'K':  case 'L':
				case 'M':  case 'N':  case 'O':  case 'P':
				case 'Q':  case 'R':  case 'S':  case 'T':
				case 'U':  case 'V':  case 'W':  case 'X':
				case 'Y':  case 'Z':
				{
					matchRange('A','Z');
					break;
				}
				case '_':
				{
					match('_');
					break;
				}
				case '0':  case '1':  case '2':  case '3':
				case '4':  case '5':  case '6':  case '7':
				case '8':  case '9':
				{
					matchRange('0','9');
					break;
				}
				case '\\':
				{
					mDOT_ESCAPED(false);
					break;
				}
				default:
				{
					goto _loop80_breakloop;
				}
				 }
			}
_loop80_breakloop:			;
		}    // ( ... )*
		_ttype = testLiteralsTable(_ttype);
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNUMERIC_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NUMERIC_LITERAL;
		
		bool synPredMatched91 = false;
		if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_0_.member(cached_LA2))))
		{
			int _m91 = mark();
			synPredMatched91 = true;
			inputState.guessing++;
			try {
				{
					{ // ( ... )+
						int _cnt90=0;
						for (;;)
						{
							if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
							{
								mDECIMAL_DIGIT(false);
							}
							else
							{
								if (_cnt90 >= 1) { goto _loop90_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
							}
							
							_cnt90++;
						}
_loop90_breakloop:						;
					}    // ( ... )+
					match('.');
					mDECIMAL_DIGIT(false);
				}
			}
			catch (RecognitionException)
			{
				synPredMatched91 = false;
			}
			rewind(_m91);
			inputState.guessing--;
		}
		if ( synPredMatched91 )
		{
			{ // ( ... )+
				int _cnt93=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt93 >= 1) { goto _loop93_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt93++;
				}
_loop93_breakloop:				;
			}    // ( ... )+
			match('.');
			{ // ( ... )+
				int _cnt95=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt95 >= 1) { goto _loop95_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt95++;
				}
_loop95_breakloop:				;
			}    // ( ... )+
			{
				if ((cached_LA1=='E'||cached_LA1=='e'))
				{
					mEXPONENT_PART(false);
				}
				else {
				}
				
			}
			{
				if ((tokenSet_2_.member(cached_LA1)))
				{
					mREAL_TYPE_SUFFIX(false);
				}
				else {
				}
				
			}
			if (0==inputState.guessing)
			{
				_ttype = REAL_LITERAL;
			}
		}
		else {
			bool synPredMatched102 = false;
			if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_3_.member(cached_LA2))))
			{
				int _m102 = mark();
				synPredMatched102 = true;
				inputState.guessing++;
				try {
					{
						{ // ( ... )+
							int _cnt100=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt100 >= 1) { goto _loop100_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt100++;
							}
_loop100_breakloop:							;
						}    // ( ... )+
						{
							mEXPONENT_PART(false);
						}
					}
				}
				catch (RecognitionException)
				{
					synPredMatched102 = false;
				}
				rewind(_m102);
				inputState.guessing--;
			}
			if ( synPredMatched102 )
			{
				{ // ( ... )+
					int _cnt104=0;
					for (;;)
					{
						if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
						{
							mDECIMAL_DIGIT(false);
						}
						else
						{
							if (_cnt104 >= 1) { goto _loop104_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
						}
						
						_cnt104++;
					}
_loop104_breakloop:					;
				}    // ( ... )+
				{
					mEXPONENT_PART(false);
				}
				{
					if ((tokenSet_2_.member(cached_LA1)))
					{
						mREAL_TYPE_SUFFIX(false);
					}
					else {
					}
					
				}
				if (0==inputState.guessing)
				{
					_ttype = REAL_LITERAL;
				}
			}
			else {
				bool synPredMatched111 = false;
				if ((((cached_LA1 >= '0' && cached_LA1 <= '9')) && (tokenSet_4_.member(cached_LA2))))
				{
					int _m111 = mark();
					synPredMatched111 = true;
					inputState.guessing++;
					try {
						{
							{ // ( ... )+
								int _cnt109=0;
								for (;;)
								{
									if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
									{
										mDECIMAL_DIGIT(false);
									}
									else
									{
										if (_cnt109 >= 1) { goto _loop109_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
									}
									
									_cnt109++;
								}
_loop109_breakloop:								;
							}    // ( ... )+
							{
								mREAL_TYPE_SUFFIX(false);
							}
						}
					}
					catch (RecognitionException)
					{
						synPredMatched111 = false;
					}
					rewind(_m111);
					inputState.guessing--;
				}
				if ( synPredMatched111 )
				{
					{ // ( ... )+
						int _cnt113=0;
						for (;;)
						{
							if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
							{
								mDECIMAL_DIGIT(false);
							}
							else
							{
								if (_cnt113 >= 1) { goto _loop113_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
							}
							
							_cnt113++;
						}
_loop113_breakloop:						;
					}    // ( ... )+
					{
						mREAL_TYPE_SUFFIX(false);
					}
					if (0==inputState.guessing)
					{
						_ttype = REAL_LITERAL;
					}
				}
				else {
					bool synPredMatched83 = false;
					if (((cached_LA1=='.')))
					{
						int _m83 = mark();
						synPredMatched83 = true;
						inputState.guessing++;
						try {
							{
								match('.');
								mDECIMAL_DIGIT(false);
							}
						}
						catch (RecognitionException)
						{
							synPredMatched83 = false;
						}
						rewind(_m83);
						inputState.guessing--;
					}
					if ( synPredMatched83 )
					{
						match('.');
						{ // ( ... )+
							int _cnt85=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt85 >= 1) { goto _loop85_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt85++;
							}
_loop85_breakloop:							;
						}    // ( ... )+
						{
							if ((cached_LA1=='E'||cached_LA1=='e'))
							{
								mEXPONENT_PART(false);
							}
							else {
							}
							
						}
						{
							if ((tokenSet_2_.member(cached_LA1)))
							{
								mREAL_TYPE_SUFFIX(false);
							}
							else {
							}
							
						}
						if (0==inputState.guessing)
						{
							_ttype = REAL_LITERAL;
						}
					}
					else if (((cached_LA1 >= '0' && cached_LA1 <= '9')) && (true)) {
						{ // ( ... )+
							int _cnt116=0;
							for (;;)
							{
								if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
								{
									mDECIMAL_DIGIT(false);
								}
								else
								{
									if (_cnt116 >= 1) { goto _loop116_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
								}
								
								_cnt116++;
							}
_loop116_breakloop:							;
						}    // ( ... )+
						{
							if ((tokenSet_5_.member(cached_LA1)))
							{
								mINTEGER_TYPE_SUFFIX(false);
							}
							else {
							}
							
						}
						if (0==inputState.guessing)
						{
							_ttype = INTEGER_LITERAL;
						}
					}
					else
					{
						throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
					}
					}}}
					if (_createToken && (null == _token) && (_ttype != Token.SKIP))
					{
						_token = makeToken(_ttype);
						_token.setText(text.ToString(_begin, text.Length-_begin));
					}
					returnToken_ = _token;
				}
				
	protected void mDECIMAL_DIGIT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DECIMAL_DIGIT;
		
		matchRange('0','9');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mEXPONENT_PART(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = EXPONENT_PART;
		
		switch ( cached_LA1 )
		{
		case 'e':
		{
			match("e");
			{    // ( ... )*
				for (;;)
				{
					if ((cached_LA1=='+'||cached_LA1=='-'))
					{
						mSIGN(false);
					}
					else
					{
						goto _loop128_breakloop;
					}
					
				}
_loop128_breakloop:				;
			}    // ( ... )*
			{ // ( ... )+
				int _cnt130=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt130 >= 1) { goto _loop130_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt130++;
				}
_loop130_breakloop:				;
			}    // ( ... )+
			break;
		}
		case 'E':
		{
			match("E");
			{    // ( ... )*
				for (;;)
				{
					if ((cached_LA1=='+'||cached_LA1=='-'))
					{
						mSIGN(false);
					}
					else
					{
						goto _loop132_breakloop;
					}
					
				}
_loop132_breakloop:				;
			}    // ( ... )*
			{ // ( ... )+
				int _cnt134=0;
				for (;;)
				{
					if (((cached_LA1 >= '0' && cached_LA1 <= '9')))
					{
						mDECIMAL_DIGIT(false);
					}
					else
					{
						if (_cnt134 >= 1) { goto _loop134_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt134++;
				}
_loop134_breakloop:				;
			}    // ( ... )+
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mREAL_TYPE_SUFFIX(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = REAL_TYPE_SUFFIX;
		
		switch ( cached_LA1 )
		{
		case 'F':
		{
			match('F');
			break;
		}
		case 'f':
		{
			match('f');
			break;
		}
		case 'D':
		{
			match('D');
			break;
		}
		case 'd':
		{
			match('d');
			break;
		}
		case 'M':
		{
			match('M');
			break;
		}
		case 'm':
		{
			match('m');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mINTEGER_TYPE_SUFFIX(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = INTEGER_TYPE_SUFFIX;
		
		{
			if ((cached_LA1=='U') && (cached_LA2=='L'))
			{
				match("UL");
			}
			else if ((cached_LA1=='L') && (cached_LA2=='U')) {
				match("LU");
			}
			else if ((cached_LA1=='u') && (cached_LA2=='l')) {
				match("ul");
			}
			else if ((cached_LA1=='l') && (cached_LA2=='u')) {
				match("lu");
			}
			else if ((cached_LA1=='U') && (cached_LA2=='L')) {
				match("UL");
			}
			else if ((cached_LA1=='L') && (cached_LA2=='U')) {
				match("LU");
			}
			else if ((cached_LA1=='u') && (cached_LA2=='L')) {
				match("uL");
			}
			else if ((cached_LA1=='l') && (cached_LA2=='U')) {
				match("lU");
			}
			else if ((cached_LA1=='U') && (true)) {
				match("U");
			}
			else if ((cached_LA1=='L') && (true)) {
				match("L");
			}
			else if ((cached_LA1=='u') && (true)) {
				match("u");
			}
			else if ((cached_LA1=='l') && (true)) {
				match("l");
			}
			else
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mHEXADECIMAL_INTEGER_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = HEXADECIMAL_INTEGER_LITERAL;
		
		match("0x");
		{ // ( ... )+
			int _cnt120=0;
			for (;;)
			{
				if ((tokenSet_6_.member(cached_LA1)))
				{
					mHEX_DIGIT(false);
				}
				else
				{
					if (_cnt120 >= 1) { goto _loop120_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				
				_cnt120++;
			}
_loop120_breakloop:			;
		}    // ( ... )+
		{
			if ((tokenSet_5_.member(cached_LA1)))
			{
				mINTEGER_TYPE_SUFFIX(false);
			}
			else {
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mHEX_DIGIT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = HEX_DIGIT;
		
		switch ( cached_LA1 )
		{
		case '0':
		{
			match('0');
			break;
		}
		case '1':
		{
			match('1');
			break;
		}
		case '2':
		{
			match('2');
			break;
		}
		case '3':
		{
			match('3');
			break;
		}
		case '4':
		{
			match('4');
			break;
		}
		case '5':
		{
			match('5');
			break;
		}
		case '6':
		{
			match('6');
			break;
		}
		case '7':
		{
			match('7');
			break;
		}
		case '8':
		{
			match('8');
			break;
		}
		case '9':
		{
			match('9');
			break;
		}
		case 'A':
		{
			match('A');
			break;
		}
		case 'B':
		{
			match('B');
			break;
		}
		case 'C':
		{
			match('C');
			break;
		}
		case 'D':
		{
			match('D');
			break;
		}
		case 'E':
		{
			match('E');
			break;
		}
		case 'F':
		{
			match('F');
			break;
		}
		case 'a':
		{
			match('a');
			break;
		}
		case 'b':
		{
			match('b');
			break;
		}
		case 'c':
		{
			match('c');
			break;
		}
		case 'd':
		{
			match('d');
			break;
		}
		case 'e':
		{
			match('e');
			break;
		}
		case 'f':
		{
			match('f');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SIGN;
		
		switch ( cached_LA1 )
		{
		case '+':
		{
			match('+');
			break;
		}
		case '-':
		{
			match('-');
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = new long[1025];
		data[0]=288019269919178752L;
		for (int i = 1; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = new long[2048];
		data[0]=-549755813889L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = new long[1025];
		data[0]=0L;
		data[1]=35527969480784L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=137438953504L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=35527969480784L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = new long[1025];
		data[0]=0L;
		data[1]=9024791442886656L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = new long[1025];
		data[0]=287948901175001088L;
		data[1]=541165879422L;
		for (int i = 2; i<=1024; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	
}
}
