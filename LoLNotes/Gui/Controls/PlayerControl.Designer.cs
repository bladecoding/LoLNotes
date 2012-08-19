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
			this.LevelLabel = new System.Windows.Forms.Label();
			this.LoadingPicture = new System.Windows.Forms.PictureBox();
			this.InfoTabs = new System.Windows.Forms.TabControl();
			this.TeamLabel = new System.Windows.Forms.Label();
			this.NameLabel = new System.Windows.Forms.LinkLabel();
			this.SeenCountLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// LevelLabel
			// 
			this.LevelLabel.AutoSize = true;
			this.LevelLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LevelLabel.Location = new System.Drawing.Point(10, 89);
			this.LevelLabel.Name = "LevelLabel";
			this.LevelLabel.Size = new System.Drawing.Size(46, 13);
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
			this.InfoTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InfoTabs.ItemSize = new System.Drawing.Size(0, 15);
			this.InfoTabs.Location = new System.Drawing.Point(160, 6);
			this.InfoTabs.Multiline = true;
			this.InfoTabs.Name = "InfoTabs";
			this.InfoTabs.SelectedIndex = 0;
			this.InfoTabs.Size = new System.Drawing.Size(334, 138);
			this.InfoTabs.TabIndex = 3;
			// 
			// TeamLabel
			// 
			this.TeamLabel.AutoSize = true;
			this.TeamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TeamLabel.Location = new System.Drawing.Point(10, 74);
			this.TeamLabel.Name = "TeamLabel";
			this.TeamLabel.Size = new System.Drawing.Size(32, 13);
			this.TeamLabel.TabIndex = 4;
			this.TeamLabel.Text = "Solo";
			// 
			// NameLabel
			// 
			this.NameLabel.AutoSize = true;
			this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NameLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.NameLabel.Location = new System.Drawing.Point(10, 10);
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.Size = new System.Drawing.Size(130, 13);
			this.NameLabel.TabIndex = 5;
			this.NameLabel.TabStop = true;
			this.NameLabel.Text = "NAMEEEEEEEEEEEE";
			this.NameLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NameLabel_LinkClicked);
			// 
			// SeenCountLabel
			// 
			this.SeenCountLabel.AutoSize = true;
			this.SeenCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SeenCountLabel.Location = new System.Drawing.Point(10, 104);
			this.SeenCountLabel.Name = "SeenCountLabel";
			this.SeenCountLabel.Size = new System.Drawing.Size(44, 13);
			this.SeenCountLabel.TabIndex = 6;
			this.SeenCountLabel.Text = "Seen: ";
			// 
			// PlayerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.SeenCountLabel);
			this.Controls.Add(this.NameLabel);
			this.Controls.Add(this.TeamLabel);
			this.Controls.Add(this.InfoTabs);
			this.Controls.Add(this.LoadingPicture);
			this.Controls.Add(this.LevelLabel);
			this.Name = "PlayerControl";
			this.Size = new System.Drawing.Size(500, 150);
			((System.ComponentModel.ISupportInitialize)(this.LoadingPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.Label LevelLabel;
		private System.Windows.Forms.PictureBox LoadingPicture;
		public System.Windows.Forms.TabControl InfoTabs;
		private System.Windows.Forms.Label TeamLabel;
		private System.Windows.Forms.LinkLabel NameLabel;
		private System.Windows.Forms.Label SeenCountLabel;
    }
}
