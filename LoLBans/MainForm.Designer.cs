namespace LoLBans
{
    partial class MainForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.GameTab = new System.Windows.Forms.TabPage();
            this.SearchTab = new System.Windows.Forms.TabPage();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.GameTab);
            this.tabControl1.Controls.Add(this.SearchTab);
            this.tabControl1.Controls.Add(this.SettingsTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(600, 541);
            this.tabControl1.TabIndex = 0;
            // 
            // GameTab
            // 
            this.GameTab.Location = new System.Drawing.Point(4, 22);
            this.GameTab.Name = "GameTab";
            this.GameTab.Padding = new System.Windows.Forms.Padding(3);
            this.GameTab.Size = new System.Drawing.Size(592, 515);
            this.GameTab.TabIndex = 0;
            this.GameTab.Text = "Game";
            this.GameTab.UseVisualStyleBackColor = true;
            // 
            // SearchTab
            // 
            this.SearchTab.Location = new System.Drawing.Point(4, 22);
            this.SearchTab.Name = "SearchTab";
            this.SearchTab.Size = new System.Drawing.Size(592, 515);
            this.SearchTab.TabIndex = 1;
            this.SearchTab.Text = "Search";
            this.SearchTab.UseVisualStyleBackColor = true;
            // 
            // SettingsTab
            // 
            this.SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Size = new System.Drawing.Size(592, 515);
            this.SettingsTab.TabIndex = 2;
            this.SettingsTab.Text = "Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 541);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "LoL";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage GameTab;
        private System.Windows.Forms.TabPage SearchTab;
        private System.Windows.Forms.TabPage SettingsTab;

    }
}

