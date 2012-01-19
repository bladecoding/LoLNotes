/*
copyright (C) 2011-2012 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LoLNotes.Extensions
{
	public static class TableLayoutPanelExt
	{
		/// <summary>
		/// Makes sure the panel has the number of columns/rows.
		/// </summary>
		/// <param name="panel"></param>
		/// <param name="columns"></param>
		/// <param name="rows"></param>
		public static void EnsureSize(this TableLayoutPanel panel, int columns, int rows)
		{
			while (panel.RowStyles.Count < rows)
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			while (panel.ColumnStyles.Count < columns)
				panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

			if (panel.RowCount < rows)
				panel.RowCount = rows;
			if (panel.ColumnCount < columns)
				panel.ColumnCount = columns;
		}

		/// <summary>
		/// Adds a control to the x/y of the panel ensuring the row/column is created
		/// </summary>
		/// <param name="panel"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="control"></param>
		public static void AddControl(this TableLayoutPanel panel, Control control, int x, int y)
		{
			panel.EnsureSize(x + 1, y + 1);
			panel.Controls.Add(control, x, y);
		}
	}
}
