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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using LoLNotes.Storage;

namespace LoLNotes.Gui
{
	public partial class EditPlayerForm : Form
	{
		public EditPlayerForm()
		{
			InitializeComponent();

			foreach (var n in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
			{
				ColorBox.Items.Add(n.Name);
			}

			ColorBox.SelectedIndex = ColorBox.Items.IndexOf("Green");
		}

		public EditPlayerForm(PlayerEntry entry)
			: this()
		{
			if (entry == null)
				return;
			NoteText.Text = entry.Note ?? "";
			ColorBox.SelectedIndex = ColorBox.Items.IndexOf(entry.NoteColor.ToKnownColor().ToString());
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void ColorBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle rect = e.Bounds;
			if (e.Index >= 0)
			{
				string n = ((ComboBox)sender).Items[e.Index].ToString();
				var font = new Font("Arial", 9, FontStyle.Regular);
				var brush = new SolidBrush((Color)typeof(Color).GetProperty(n).GetValue(null, null));
				g.DrawString(n, font, Brushes.Black, rect.X, rect.Top);
				g.FillRectangle(brush, rect.X + 110, rect.Y, rect.Width, rect.Height);
			}
		}

		private void NoteText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				DialogResult = DialogResult.OK;
				Close();
				e.Handled = true;
			}
		}
	}
}
