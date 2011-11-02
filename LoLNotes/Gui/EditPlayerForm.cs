using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

            foreach (var n in Enum.GetNames(typeof (KnownColor)))
            {
                ColorBox.Items.Add(n);
            }

            ColorBox.SelectedIndex = ColorBox.Items.IndexOf(KnownColor.Green.ToString());
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
                var brush = new SolidBrush(Color.FromKnownColor((KnownColor)Enum.Parse(typeof(KnownColor), n)));
                g.DrawString(n, font, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(brush, rect.X + 110, rect.Y, rect.Width, rect.Height);
            }
        }
    }
}
