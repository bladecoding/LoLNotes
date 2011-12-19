/*
copyright (C) 2011 by high828@gmail.com

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

namespace LoLNotes.Gui.Controls
{
    partial class PlayerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerControl));
			this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.NameLabel = new System.Windows.Forms.Label();
			this.IconPicture = new System.Windows.Forms.PictureBox();
			this.LevelLabel = new System.Windows.Forms.Label();
			this.InfoPanel = new System.Windows.Forms.Panel();
			this.NotesLabel = new System.Windows.Forms.Label();
			this.LoadingPicture = new System.Windows.Forms.PictureBox();
			this.RightArrow = new System.Windows.Forms.PictureBox();
			this.LeftArrow = new System.Windows.Forms.PictureBox();
			this.PageLabel = new System.Windows.Forms.Label();
			this.Stats = new LoLNotes.Gui.Controls.StatsControl();
			((System.ComponentModel.ISupportInitialize)(this.IconPicture)).BeginInit();
			this.InfoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RightArrow)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LeftArrow)).BeginInit();
			this.SuspendLayout();
			// 
			// NameLabel
			// 
			this.NameLabel.AutoSize = true;
			this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NameLabel.Location = new System.Drawing.Point(10, 10);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = new System.Drawing.Size(39, 13);
			this.NameLabel.TabIndex = 0;
			this.NameLabel.Text = "Name";
			// 
			// IconPicture
			// 
			this.IconPicture.BackColor = System.Drawing.Color.Transparent;
			this.IconPicture.Location = new System.Drawing.Point(10, 40);
			this.IconPicture.Name = "IconPicture";
			this.IconPicture.Size = new System.Drawing.Size(64, 64);
			this.IconPicture.TabIndex = 1;
			this.IconPicture.TabStop = false;
			// 
			// LevelLabel
			// 
			this.LevelLabel.AutoSize = true;
			this.LevelLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LevelLabel.Location = new System.Drawing.Point(10, 110);
			this.LevelLabel.Name = "LevelLabel";
			this.LevelLabel.Size = new System.Drawing.Size(46, 13);
			this.LevelLabel.TabIndex = 2;
			this.LevelLabel.Text = "Level: ";
			// 
			// InfoPanel
			// 
			this.InfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InfoPanel.BackColor = System.Drawing.Color.Transparent;
			this.InfoPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.InfoPanel.Controls.Add(this.NotesLabel);
			this.InfoPanel.Controls.Add(this.LoadingPicture);
			this.InfoPanel.Controls.Add(this.Stats);
			this.InfoPanel.Location = new System.Drawing.Point(85, 40);
			this.InfoPanel.Name = "InfoPanel";
			this.InfoPanel.Size = new System.Drawing.Size(300, 100);
			this.InfoPanel.TabIndex = 4;
			// 
			// NotesLabel
			// 
			this.NotesLabel.AutoSize = true;
			this.NotesLabel.Location = new System.Drawing.Point(5, 5);
			this.NotesLabel.Name = "NotesLabel";
			this.NotesLabel.Size = new System.Drawing.Size(35, 13);
			this.NotesLabel.TabIndex = 1;
			this.NotesLabel.Text = "Notes";
			// 
			// LoadingPicture
			// 
			this.LoadingPicture.Image = ((System.Drawing.Image)(resources.GetObject("LoadingPicture.Image")));
			this.LoadingPicture.Location = new System.Drawing.Point(118, 18);
			this.LoadingPicture.Name = "LoadingPicture";
			this.LoadingPicture.Size = new System.Drawing.Size(64, 64);
			this.LoadingPicture.TabIndex = 0;
			this.LoadingPicture.TabStop = false;
			// 
			// RightArrow
			// 
			this.RightArrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RightArrow.BackColor = System.Drawing.Color.Transparent;
			this.RightArrow.ErrorImage = null;
			this.RightArrow.Image = ((System.Drawing.Image)(resources.GetObject("RightArrow.Image")));
			this.RightArrow.InitialImage = null;
			this.RightArrow.Location = new System.Drawing.Point(345, 10);
			this.RightArrow.Name = "RightArrow";
			this.RightArrow.Size = new System.Drawing.Size(40, 25);
			this.RightArrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.RightArrow.TabIndex = 5;
			this.RightArrow.TabStop = false;
			this.RightArrow.Click += new System.EventHandler(this.RightArrow_Click);
			// 
			// LeftArrow
			// 
			this.LeftArrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LeftArrow.BackColor = System.Drawing.Color.Transparent;
			this.LeftArrow.ErrorImage = null;
			this.LeftArrow.Image = ((System.Drawing.Image)(resources.GetObject("LeftArrow.Image")));
			this.LeftArrow.InitialImage = null;
			this.LeftArrow.Location = new System.Drawing.Point(299, 10);
			this.LeftArrow.Name = "LeftArrow";
			this.LeftArrow.Size = new System.Drawing.Size(40, 25);
			this.LeftArrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.LeftArrow.TabIndex = 6;
			this.LeftArrow.TabStop = false;
			this.LeftArrow.Click += new System.EventHandler(this.LeftArrow_Click);
			// 
			// PageLabel
			// 
			this.PageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PageLabel.Location = new System.Drawing.Point(249, 22);
			this.PageLabel.Name = "PageLabel";
			this.PageLabel.Size = new System.Drawing.Size(44, 13);
			this.PageLabel.TabIndex = 7;
			this.PageLabel.Text = "0 / 0";
			this.PageLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Stats
			// 
			this.Stats.BackColor = System.Drawing.Color.Transparent;
			this.Stats.Location = new System.Drawing.Point(0, 0);
			this.Stats.Name = "Stats";
			this.Stats.Size = new System.Drawing.Size(300, 100);
			this.Stats.TabIndex = 2;
			// 
			// PlayerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.PageLabel);
			this.Controls.Add(this.LeftArrow);
			this.Controls.Add(this.RightArrow);
			this.Controls.Add(this.InfoPanel);
			this.Controls.Add(this.LevelLabel);
			this.Controls.Add(this.IconPicture);
			this.Controls.Add(this.NameLabel);
			this.Name = "PlayerControl";
			this.Size = new System.Drawing.Size(400, 150);
			((System.ComponentModel.ISupportInitialize)(this.IconPicture)).EndInit();
			this.InfoPanel.ResumeLayout(false);
			this.InfoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RightArrow)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LeftArrow)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.PictureBox IconPicture;
		private System.Windows.Forms.Label LevelLabel;
		private System.Windows.Forms.Panel InfoPanel;
		private System.Windows.Forms.PictureBox RightArrow;
		private System.Windows.Forms.PictureBox LeftArrow;
		private System.Windows.Forms.PictureBox LoadingPicture;
		private System.Windows.Forms.Label PageLabel;
		private System.Windows.Forms.Label NotesLabel;
		private StatsControl Stats;
    }
}
