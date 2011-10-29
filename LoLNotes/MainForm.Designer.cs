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

using LoLNotes.Controls;

namespace LoLNotes
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.GameTab = new System.Windows.Forms.TabPage();
            this.SearchTab = new System.Windows.Forms.TabPage();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.RebuildButton = new System.Windows.Forms.Button();
            this.InstallButton = new System.Windows.Forms.Button();
            this.LogTab = new System.Windows.Forms.TabPage();
            this.LogList = new System.Windows.Forms.ListBox();
            this.teamControl2 = new LoLNotes.Controls.TeamControl();
            this.teamControl1 = new LoLNotes.Controls.TeamControl();
            this.RebuildWorker = new System.ComponentModel.BackgroundWorker();
            this.tabControl1.SuspendLayout();
            this.GameTab.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.LogTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.GameTab);
            this.tabControl1.Controls.Add(this.SearchTab);
            this.tabControl1.Controls.Add(this.SettingsTab);
            this.tabControl1.Controls.Add(this.LogTab);
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
            this.SettingsTab.Controls.Add(this.RebuildButton);
            this.SettingsTab.Controls.Add(this.InstallButton);
            this.SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Size = new System.Drawing.Size(476, 696);
            this.SettingsTab.TabIndex = 2;
            this.SettingsTab.Text = "Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            // 
            // RebuildButton
            // 
            this.RebuildButton.Location = new System.Drawing.Point(10, 39);
            this.RebuildButton.Name = "RebuildButton";
            this.RebuildButton.Size = new System.Drawing.Size(75, 23);
            this.RebuildButton.TabIndex = 1;
            this.RebuildButton.Text = "Rebuild";
            this.RebuildButton.UseVisualStyleBackColor = true;
            this.RebuildButton.Click += new System.EventHandler(this.RebuildButton_Click);
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
            // teamControl2
            // 
            this.teamControl2.Location = new System.Drawing.Point(268, 6);
            this.teamControl2.Name = "teamControl2";
            this.teamControl2.Size = new System.Drawing.Size(200, 686);
            this.teamControl2.TabIndex = 1;
            this.teamControl2.TeamSize = 5;
            this.teamControl2.Text = "Team 2";
            // 
            // teamControl1
            // 
            this.teamControl1.Location = new System.Drawing.Point(8, 6);
            this.teamControl1.Name = "teamControl1";
            this.teamControl1.Size = new System.Drawing.Size(200, 686);
            this.teamControl1.TabIndex = 0;
            this.teamControl1.TeamSize = 5;
            this.teamControl1.Text = "Team 1";
            // 
            // RebuildWorker
            // 
            this.RebuildWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.RebuildWorker_DoWork);
            this.RebuildWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RebuildWorker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 722);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "LoL";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.GameTab.ResumeLayout(false);
            this.SettingsTab.ResumeLayout(false);
            this.LogTab.ResumeLayout(false);
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
        private System.Windows.Forms.Button RebuildButton;
        private System.ComponentModel.BackgroundWorker RebuildWorker;

    }
}

