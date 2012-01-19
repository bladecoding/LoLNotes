namespace LoLNotes.Gui
{
    partial class EditPlayerForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.NoteText = new System.Windows.Forms.TextBox();
			this.ColorBox = new System.Windows.Forms.ComboBox();
			this.SaveButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// NoteText
			// 
			this.NoteText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NoteText.Location = new System.Drawing.Point(12, 12);
			this.NoteText.Name = "NoteText";
			this.NoteText.Size = new System.Drawing.Size(322, 20);
			this.NoteText.TabIndex = 0;
			this.NoteText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NoteText_KeyDown);
			// 
			// ColorBox
			// 
			this.ColorBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColorBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.ColorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ColorBox.FormattingEnabled = true;
			this.ColorBox.Location = new System.Drawing.Point(12, 38);
			this.ColorBox.Name = "ColorBox";
			this.ColorBox.Size = new System.Drawing.Size(241, 21);
			this.ColorBox.TabIndex = 1;
			this.ColorBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ColorBox_DrawItem);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = new System.Drawing.Point(259, 36);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// EditPlayerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(342, 66);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.ColorBox);
			this.Controls.Add(this.NoteText);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "EditPlayerForm";
			this.Text = "Edit ";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SaveButton;
        public System.Windows.Forms.TextBox NoteText;
        public System.Windows.Forms.ComboBox ColorBox;
    }
}
