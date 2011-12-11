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

using LoLNotes.Gui.Controls;

namespace LoLNotes.Gui
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
            if (disposing && (Connection != null))
            {
                Connection.Dispose();
            }
            if (disposing && (Database != null))
            {
                Database.Dispose();
            }
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
			this.components = new System.ComponentModel.Container();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.GameTab = new System.Windows.Forms.TabPage();
			this.teamControl2 = new LoLNotes.Gui.Controls.TeamControl();
			this.PlayerEditStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.teamControl1 = new LoLNotes.Gui.Controls.TeamControl();
			this.SearchTab = new System.Windows.Forms.TabPage();
			this.SettingsTab = new System.Windows.Forms.TabPage();
			this.RegionList = new System.Windows.Forms.ComboBox();
			this.DownloadLink = new System.Windows.Forms.LinkLabel();
			this.InstallButton = new System.Windows.Forms.Button();
			this.LogTab = new System.Windows.Forms.TabPage();
			this.LogList = new System.Windows.Forms.ListBox();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.ChangesText = new System.Windows.Forms.RichTextBox();
			this.RebuildWorker = new System.ComponentModel.BackgroundWorker();
			this.tabControl1.SuspendLayout();
			this.GameTab.SuspendLayout();
			this.PlayerEditStrip.SuspendLayout();
			this.SettingsTab.SuspendLayout();
			this.LogTab.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.GameTab);
			this.tabControl1.Controls.Add(this.SearchTab);
			this.tabControl1.Controls.Add(this.SettingsTab);
			this.tabControl1.Controls.Add(this.LogTab);
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(484, 722);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
			// 
			// GameTab
			// 
			this.GameTab.Controls.Add(this.teamControl2);
			this.GameTab.Controls.Add(this.teamControl1);
			this.GameTab.Location = new System.Drawing.Point(4, 22);
			this.GameTab.Name = "GameTab";
			this.GameTab.Padding = new System.Windows.Forms.Padding(3);
			this.GameTab.Size = new System.Drawing.Size(476, 696);
			this.GameTab.TabIndex = 0;
			this.GameTab.Text = "Game";
			this.GameTab.UseVisualStyleBackColor = true;
			// 
			// teamControl2
			// 
			this.teamControl2.Location = new System.Drawing.Point(268, 6);
			this.teamControl2.Name = "teamControl2";
			this.teamControl2.PlayerContextMenuStrip = this.PlayerEditStrip;
			this.teamControl2.Size = new System.Drawing.Size(200, 686);
			this.teamControl2.TabIndex = 1;
			this.teamControl2.TeamSize = 5;
			this.teamControl2.Text = "Team 2";
			// 
			// PlayerEditStrip
			// 
			this.PlayerEditStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.clearToolStripMenuItem});
			this.PlayerEditStrip.Name = "PlayerEditStrip";
			this.PlayerEditStrip.Size = new System.Drawing.Size(102, 48);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
			this.editToolStripMenuItem.Text = "Edit";
			this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
			// 
			// clearToolStripMenuItem
			// 
			this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
			this.clearToolStripMenuItem.Text = "Clear";
			this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
			// 
			// teamControl1
			// 
			this.teamControl1.Location = new System.Drawing.Point(8, 6);
			this.teamControl1.Name = "teamControl1";
			this.teamControl1.PlayerContextMenuStrip = this.PlayerEditStrip;
			this.teamControl1.Size = new System.Drawing.Size(200, 686);
			this.teamControl1.TabIndex = 0;
			this.teamControl1.TeamSize = 5;
			this.teamControl1.Text = "Team 1";
			// 
			// SearchTab
			// 
			this.SearchTab.Location = new System.Drawing.Point(4, 22);
			this.SearchTab.Name = "SearchTab";
			this.SearchTab.Size = new System.Drawing.Size(476, 696);
			this.SearchTab.TabIndex = 1;
			this.SearchTab.Text = "Search";
			this.SearchTab.UseVisualStyleBackColor = true;
			// 
			// SettingsTab
			// 
			this.SettingsTab.Controls.Add(this.RegionList);
			this.SettingsTab.Controls.Add(this.DownloadLink);
			this.SettingsTab.Controls.Add(this.InstallButton);
			this.SettingsTab.Location = new System.Drawing.Point(4, 22);
			this.SettingsTab.Name = "SettingsTab";
			this.SettingsTab.Size = new System.Drawing.Size(476, 696);
			this.SettingsTab.TabIndex = 2;
			this.SettingsTab.Text = "Settings";
			this.SettingsTab.UseVisualStyleBackColor = true;
			// 
			// RegionList
			// 
			this.RegionList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RegionList.FormattingEnabled = true;
			this.RegionList.Location = new System.Drawing.Point(11, 39);
			this.RegionList.Name = "RegionList";
			this.RegionList.Size = new System.Drawing.Size(121, 21);
			this.RegionList.TabIndex = 3;
			this.RegionList.SelectedIndexChanged += new System.EventHandler(this.RegionList_SelectedIndexChanged);
			// 
			// DownloadLink
			// 
			this.DownloadLink.AutoSize = true;
			this.DownloadLink.Location = new System.Drawing.Point(8, 65);
			this.DownloadLink.Name = "DownloadLink";
			this.DownloadLink.Size = new System.Drawing.Size(177, 13);
			this.DownloadLink.TabIndex = 2;
			this.DownloadLink.TabStop = true;
			this.DownloadLink.Text = "https://github.com/high6/LoLNotes";
			this.DownloadLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DownloadLink_LinkClicked);
			// 
			// InstallButton
			// 
			this.InstallButton.Location = new System.Drawing.Point(10, 10);
			this.InstallButton.Name = "InstallButton";
			this.InstallButton.Size = new System.Drawing.Size(75, 23);
			this.InstallButton.TabIndex = 0;
			this.InstallButton.Text = "Install";
			this.InstallButton.UseVisualStyleBackColor = true;
			this.InstallButton.Click += new System.EventHandler(this.button1_Click);
			// 
			// LogTab
			// 
			this.LogTab.Controls.Add(this.LogList);
			this.LogTab.Location = new System.Drawing.Point(4, 22);
			this.LogTab.Name = "LogTab";
			this.LogTab.Size = new System.Drawing.Size(476, 696);
			this.LogTab.TabIndex = 3;
			this.LogTab.Text = "Log";
			this.LogTab.UseVisualStyleBackColor = true;
			// 
			// LogList
			// 
			this.LogList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogList.FormattingEnabled = true;
			this.LogList.Location = new System.Drawing.Point(0, 0);
			this.LogList.Name = "LogList";
			this.LogList.Size = new System.Drawing.Size(476, 696);
			this.LogList.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.ChangesText);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(476, 696);
			this.tabPage1.TabIndex = 4;
			this.tabPage1.Text = "Changes";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// ChangesText
			// 
			this.ChangesText.BackColor = System.Drawing.SystemColors.Window;
			this.ChangesText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChangesText.Location = new System.Drawing.Point(3, 3);
			this.ChangesText.Name = "ChangesText";
			this.ChangesText.ReadOnly = true;
			this.ChangesText.Size = new System.Drawing.Size(470, 690);
			this.ChangesText.TabIndex = 0;
			this.ChangesText.Text = "Loading...";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 722);
			this.Controls.Add(this.tabControl1);
			this.Name = "MainForm";
			this.Text = "LoL";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.tabControl1.ResumeLayout(false);
			this.GameTab.ResumeLayout(false);
			this.PlayerEditStrip.ResumeLayout(false);
			this.SettingsTab.ResumeLayout(false);
			this.SettingsTab.PerformLayout();
			this.LogTab.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage GameTab;
        private System.Windows.Forms.TabPage SearchTab;
        private System.Windows.Forms.TabPage SettingsTab;
        private Controls.TeamControl teamControl1;
        private System.Windows.Forms.Button InstallButton;
        private Controls.TeamControl teamControl2;
        private System.Windows.Forms.TabPage LogTab;
		private System.Windows.Forms.ListBox LogList;
        private System.ComponentModel.BackgroundWorker RebuildWorker;
        private System.Windows.Forms.ContextMenuStrip PlayerEditStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.LinkLabel DownloadLink;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox ChangesText;
		private System.Windows.Forms.ComboBox RegionList;

    }
}

