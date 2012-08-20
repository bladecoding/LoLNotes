using System;
using antlr;
using antlr.collections;

namespace FluorineFx.Expression
{
	/// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    internal class FluorineAST : antlr.BaseAST
	{
        internal class FluorineASTCreator : antlr.ASTNodeCreator
		{
            public override antlr.collections.AST Create()
			{
				return new FluorineAST();
			}

			public override string ASTNodeTypeName
			{
				get { return typeof(FluorineAST).FullName; }
			}
		}

		/// <summary>
		/// The global FluorineAST node factory
		/// </summary>
		internal static readonly FluorineASTCreator Creator = new FluorineASTCreator();

		#region Members

		private string text;
		private int ttype;

		#endregion

		/// <summary>
		/// Create an instance
		/// </summary>
		public FluorineAST()
		{}

		/// <summary>
		/// Create an instance from a token
		/// </summary>
		public FluorineAST(IToken token)
		{
			initialize(token);
		}

		/// <summary>
		/// initialize this instance from an AST
		/// </summary>
		public override void initialize(AST t)
		{
			this.setText(t.getText());
			this.Type = t.Type;
		}

		/// <summary>
		/// initialize this instance from an IToken
		/// </summary>
		public override void initialize(IToken tok)
		{
			this.setText(tok.getText());
			this.Type = tok.Type;
		}

		/// <summary>
		/// initialize this instance from a token type number and a text
		/// </summary>
		public override void initialize(int t, string txt)
		{
			this.Type = t;
			this.setText(txt);
		}

		/// <summary>
		/// gets or sets the token type of this node
		/// </summary>
		public override int Type
		{
			get { return this.ttype; }
			set { this.ttype = value; }
		}

		/// <summary>
		/// gets or sets the text of this node
		/// </summary>
		public string Text
		{
			get { return this.getText(); }
			set { this.setText(value); }
		}

		/// <summary>
		/// sets the text of this node
		/// </summary>
		public override void setText(string txt)
		{
			this.text = txt;
		}

		/// <summary>
		/// gets the text of this node
		/// </summary>
		public override string getText()
		{
			return this.text;
		}


	}
}
