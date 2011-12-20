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
			this.LoadingPicture = new System.Windows.Forms.PictureBox();
			this.InfoTabs = new System.Windows.Forms.TabControl();
			((System.ComponentModel.ISupportInitialize)(this.IconPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// NameLabel
			// 
			this.NameLabel.AutoSize = true;
			this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NameLabel.Location = new System.Drawing.Point(10, 10);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = new System.Drawing.Size(116, 13);
			this.NameLabel.TabIndex = 0;
			this.NameLabel.Text = "Nameeeeeeeeeeee";
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
			this.LevelLabel.Size = new System.Drawing.Size(56, 14);
			this.LevelLabel.TabIndex = 2;
			this.LevelLabel.Text = "Level: ";
			// 
			// LoadingPicture
			// 
			this.LoadingPicture.Image = ((System.Drawing.Image)(resources.GetObject("LoadingPicture.Image")));
			this.LoadingPicture.Location = new System.Drawing.Point(10, 126);
			this.LoadingPicture.Name = "LoadingPicture";
			this.LoadingPicture.Size = new System.Drawing.Size(16, 16);
			this.LoadingPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.LoadingPicture.TabIndex = 0;
			this.LoadingPicture.TabStop = false;
			// 
			// InfoTabs
			// 
			this.InfoTabs.Location = new System.Drawing.Point(80, 26);
			this.InfoTabs.Multiline = true;
			this.InfoTabs.Name = "InfoTabs";
			this.InfoTabs.SelectedIndex = 0;
			this.InfoTabs.Size = new System.Drawing.Size(310, 114);
			this.InfoTabs.TabIndex = 3;
			// 
			// PlayerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.NameLabel);
			this.Controls.Add(this.InfoTabs);
			this.Controls.Add(this.LoadingPicture);
			this.Controls.Add(this.LevelLabel);
			this.Controls.Add(this.IconPicture);
			this.Name = "PlayerControl";
			this.Size = new System.Drawing.Size(400, 150);
			((System.ComponentModel.ISupportInitialize)(this.IconPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.Label NameLabel;
		private System.Windows.Forms.PictureBox IconPicture;
		private System.Windows.Forms.Label LevelLabel;
		private System.Windows.Forms.PictureBox LoadingPicture;
		private System.Windows.Forms.TabControl InfoTabs;
    }
}
