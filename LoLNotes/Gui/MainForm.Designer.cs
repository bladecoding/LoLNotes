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
            this.NewsTab = new System.Windows.Forms.TabPage();
            this.NewsBrowser = new System.Windows.Forms.WebBrowser();
            this.GameTab = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.PlayerEditStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsTab = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.DefaultGameTab = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.DownloadLink = new System.Windows.Forms.LinkLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RegionList = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.InstallButton = new System.Windows.Forms.Button();
            this.ModuleGroupBox = new System.Windows.Forms.GroupBox();
            this.MirrorRadio = new System.Windows.Forms.RadioButton();
            this.ToolHelpRadio = new System.Windows.Forms.RadioButton();
            this.ProcessRadio = new System.Windows.Forms.RadioButton();
            this.LeaveCheck = new System.Windows.Forms.CheckBox();
            this.DevCheck = new System.Windows.Forms.CheckBox();
            this.LogGroupBox = new System.Windows.Forms.GroupBox();
            this.DebugCheck = new System.Windows.Forms.CheckBox();
            this.TraceCheck = new System.Windows.Forms.CheckBox();
            this.DatabaseGroupBox = new System.Windows.Forms.GroupBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ImportButton = new System.Windows.Forms.Button();
            this.LogTab = new System.Windows.Forms.TabPage();
            this.LogList = new System.Windows.Forms.ListBox();
            this.ChangesTab = new System.Windows.Forms.TabPage();
            this.ChangesText = new System.Windows.Forms.RichTextBox();
            this.DevTab = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CallView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CallEditStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.CallTree = new System.Windows.Forms.TreeView();
            this.teamControl1 = new LoLNotes.Gui.Controls.TeamControl();
            this.teamControl2 = new LoLNotes.Gui.Controls.TeamControl();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.RecentGamesBox = new System.Windows.Forms.CheckBox();
            this.StatsBox = new System.Windows.Forms.CheckBox();
            this.TopChampsBox = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.NewsTab.SuspendLayout();
            this.GameTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.PlayerEditStrip.SuspendLayout();
            this.SettingsTab.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.ModuleGroupBox.SuspendLayout();
            this.LogGroupBox.SuspendLayout();
            this.DatabaseGroupBox.SuspendLayout();
            this.LogTab.SuspendLayout();
            this.ChangesTab.SuspendLayout();
            this.DevTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.CallEditStrip.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.NewsTab);
            this.tabControl1.Controls.Add(this.GameTab);
            this.tabControl1.Controls.Add(this.SettingsTab);
            this.tabControl1.Controls.Add(this.LogTab);
            this.tabControl1.Controls.Add(this.ChangesTab);
            this.tabControl1.Controls.Add(this.DevTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1165, 868);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // NewsTab
            // 
            this.NewsTab.Controls.Add(this.NewsBrowser);
            this.NewsTab.Location = new System.Drawing.Point(4, 22);
            this.NewsTab.Name = "NewsTab";
            this.NewsTab.Padding = new System.Windows.Forms.Padding(3);
            this.NewsTab.Size = new System.Drawing.Size(1157, 842);
            this.NewsTab.TabIndex = 6;
            this.NewsTab.Text = "News";
            this.NewsTab.UseVisualStyleBackColor = true;
            // 
            // NewsBrowser
            // 
            this.NewsBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NewsBrowser.Location = new System.Drawing.Point(3, 3);
            this.NewsBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.NewsBrowser.Name = "NewsBrowser";
            this.NewsBrowser.ScriptErrorsSuppressed = true;
            this.NewsBrowser.Size = new System.Drawing.Size(1151, 836);
            this.NewsBrowser.TabIndex = 0;
            // 
            // GameTab
            // 
            this.GameTab.AutoScroll = true;
            this.GameTab.Controls.Add(this.splitContainer2);
            this.GameTab.Location = new System.Drawing.Point(4, 22);
            this.GameTab.Name = "GameTab";
            this.GameTab.Padding = new System.Windows.Forms.Padding(3);
            this.GameTab.Size = new System.Drawing.Size(1157, 842);
            this.GameTab.TabIndex = 0;
            this.GameTab.Text = "Game";
            this.GameTab.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AutoScroll = true;
            this.splitContainer2.Panel1.AutoScrollMinSize = new System.Drawing.Size(500, 836);
            this.splitContainer2.Panel1.Controls.Add(this.comboBox1);
            this.splitContainer2.Panel1.Controls.Add(this.button1);
            this.splitContainer2.Panel1.Controls.Add(this.teamControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.AutoScrollMinSize = new System.Drawing.Size(500, 836);
            this.splitContainer2.Panel2.Controls.Add(this.teamControl2);
            this.splitContainer2.Size = new System.Drawing.Size(1151, 836);
            this.splitContainer2.SplitterDistance = 575;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Champs",
            "Recent"});
            this.comboBox1.Location = new System.Drawing.Point(94, 4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 2;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(221, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
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
            // SettingsTab
            // 
            this.SettingsTab.Controls.Add(this.groupBox5);
            this.SettingsTab.Controls.Add(this.groupBox4);
            this.SettingsTab.Controls.Add(this.groupBox3);
            this.SettingsTab.Controls.Add(this.groupBox2);
            this.SettingsTab.Controls.Add(this.groupBox1);
            this.SettingsTab.Controls.Add(this.ModuleGroupBox);
            this.SettingsTab.Controls.Add(this.LeaveCheck);
            this.SettingsTab.Controls.Add(this.DevCheck);
            this.SettingsTab.Controls.Add(this.LogGroupBox);
            this.SettingsTab.Controls.Add(this.DatabaseGroupBox);
            this.SettingsTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.Size = new System.Drawing.Size(1157, 842);
            this.SettingsTab.TabIndex = 2;
            this.SettingsTab.Text = "Settings";
            this.SettingsTab.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.DefaultGameTab);
            this.groupBox4.Location = new System.Drawing.Point(103, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(155, 50);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Default Game Tab";
            // 
            // DefaultGameTab
            // 
            this.DefaultGameTab.FormattingEnabled = true;
            this.DefaultGameTab.Items.AddRange(new object[] {
            "Champs",
            "Recent"});
            this.DefaultGameTab.Location = new System.Drawing.Point(6, 19);
            this.DefaultGameTab.Name = "DefaultGameTab";
            this.DefaultGameTab.Size = new System.Drawing.Size(143, 21);
            this.DefaultGameTab.TabIndex = 0;
            this.DefaultGameTab.Text = "Recent";
            this.DefaultGameTab.TextChanged += new System.EventHandler(this.DefaultGameTab_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.DownloadLink);
            this.groupBox3.Location = new System.Drawing.Point(11, 114);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(81, 39);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Update Link";
            // 
            // DownloadLink
            // 
            this.DownloadLink.AutoSize = true;
            this.DownloadLink.Location = new System.Drawing.Point(6, 16);
            this.DownloadLink.Name = "DownloadLink";
            this.DownloadLink.Size = new System.Drawing.Size(55, 13);
            this.DownloadLink.TabIndex = 2;
            this.DownloadLink.TabStop = true;
            this.DownloadLink.Text = "Download";
            this.DownloadLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DownloadLink_LinkClicked);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RegionList);
            this.groupBox2.Location = new System.Drawing.Point(11, 59);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(155, 49);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "League Of Legends Region";
            // 
            // RegionList
            // 
            this.RegionList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RegionList.FormattingEnabled = true;
            this.RegionList.Location = new System.Drawing.Point(6, 19);
            this.RegionList.Name = "RegionList";
            this.RegionList.Size = new System.Drawing.Size(121, 21);
            this.RegionList.TabIndex = 3;
            this.RegionList.SelectedIndexChanged += new System.EventHandler(this.RegionList_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.InstallButton);
            this.groupBox1.Location = new System.Drawing.Point(11, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(86, 50);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Certificates";
            // 
            // InstallButton
            // 
            this.InstallButton.Location = new System.Drawing.Point(6, 19);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(75, 23);
            this.InstallButton.TabIndex = 0;
            this.InstallButton.Text = "Install";
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // ModuleGroupBox
            // 
            this.ModuleGroupBox.Controls.Add(this.MirrorRadio);
            this.ModuleGroupBox.Controls.Add(this.ToolHelpRadio);
            this.ModuleGroupBox.Controls.Add(this.ProcessRadio);
            this.ModuleGroupBox.Location = new System.Drawing.Point(107, 159);
            this.ModuleGroupBox.Name = "ModuleGroupBox";
            this.ModuleGroupBox.Size = new System.Drawing.Size(125, 95);
            this.ModuleGroupBox.TabIndex = 8;
            this.ModuleGroupBox.TabStop = false;
            this.ModuleGroupBox.Text = "Module Resolver";
            // 
            // MirrorRadio
            // 
            this.MirrorRadio.AutoSize = true;
            this.MirrorRadio.Location = new System.Drawing.Point(6, 65);
            this.MirrorRadio.Name = "MirrorRadio";
            this.MirrorRadio.Size = new System.Drawing.Size(51, 17);
            this.MirrorRadio.TabIndex = 2;
            this.MirrorRadio.Text = "Mirror";
            this.MirrorRadio.UseVisualStyleBackColor = true;
            // 
            // ToolHelpRadio
            // 
            this.ToolHelpRadio.AutoSize = true;
            this.ToolHelpRadio.Location = new System.Drawing.Point(6, 42);
            this.ToolHelpRadio.Name = "ToolHelpRadio";
            this.ToolHelpRadio.Size = new System.Drawing.Size(78, 17);
            this.ToolHelpRadio.TabIndex = 1;
            this.ToolHelpRadio.Text = "Toolhelp32";
            this.ToolHelpRadio.UseVisualStyleBackColor = true;
            // 
            // ProcessRadio
            // 
            this.ProcessRadio.AutoSize = true;
            this.ProcessRadio.Checked = true;
            this.ProcessRadio.Location = new System.Drawing.Point(6, 19);
            this.ProcessRadio.Name = "ProcessRadio";
            this.ProcessRadio.Size = new System.Drawing.Size(88, 17);
            this.ProcessRadio.TabIndex = 0;
            this.ProcessRadio.TabStop = true;
            this.ProcessRadio.Text = "ProcessClass";
            this.ProcessRadio.UseVisualStyleBackColor = true;
            // 
            // LeaveCheck
            // 
            this.LeaveCheck.AutoSize = true;
            this.LeaveCheck.Location = new System.Drawing.Point(16, 345);
            this.LeaveCheck.Name = "LeaveCheck";
            this.LeaveCheck.Size = new System.Drawing.Size(120, 17);
            this.LeaveCheck.TabIndex = 7;
            this.LeaveCheck.Text = "Delete LeaveBuster";
            this.LeaveCheck.UseVisualStyleBackColor = true;
            // 
            // DevCheck
            // 
            this.DevCheck.AutoSize = true;
            this.DevCheck.Location = new System.Drawing.Point(16, 322);
            this.DevCheck.Name = "DevCheck";
            this.DevCheck.Size = new System.Drawing.Size(76, 17);
            this.DevCheck.TabIndex = 6;
            this.DevCheck.Text = "Dev Mode";
            this.DevCheck.UseVisualStyleBackColor = true;
            this.DevCheck.Click += new System.EventHandler(this.DevCheck_Click);
            // 
            // LogGroupBox
            // 
            this.LogGroupBox.Controls.Add(this.DebugCheck);
            this.LogGroupBox.Controls.Add(this.TraceCheck);
            this.LogGroupBox.Location = new System.Drawing.Point(10, 247);
            this.LogGroupBox.Name = "LogGroupBox";
            this.LogGroupBox.Size = new System.Drawing.Size(91, 69);
            this.LogGroupBox.TabIndex = 5;
            this.LogGroupBox.TabStop = false;
            this.LogGroupBox.Text = "Log Levels";
            // 
            // DebugCheck
            // 
            this.DebugCheck.AutoSize = true;
            this.DebugCheck.Location = new System.Drawing.Point(6, 42);
            this.DebugCheck.Name = "DebugCheck";
            this.DebugCheck.Size = new System.Drawing.Size(58, 17);
            this.DebugCheck.TabIndex = 1;
            this.DebugCheck.Text = "Debug";
            this.DebugCheck.UseVisualStyleBackColor = true;
            this.DebugCheck.Click += new System.EventHandler(this.DebugCheck_Click);
            // 
            // TraceCheck
            // 
            this.TraceCheck.AutoSize = true;
            this.TraceCheck.Location = new System.Drawing.Point(6, 19);
            this.TraceCheck.Name = "TraceCheck";
            this.TraceCheck.Size = new System.Drawing.Size(54, 17);
            this.TraceCheck.TabIndex = 0;
            this.TraceCheck.Text = "Trace";
            this.TraceCheck.UseVisualStyleBackColor = true;
            this.TraceCheck.Click += new System.EventHandler(this.TraceCheck_Click);
            // 
            // DatabaseGroupBox
            // 
            this.DatabaseGroupBox.Controls.Add(this.ExportButton);
            this.DatabaseGroupBox.Controls.Add(this.ImportButton);
            this.DatabaseGroupBox.Location = new System.Drawing.Point(11, 159);
            this.DatabaseGroupBox.Name = "DatabaseGroupBox";
            this.DatabaseGroupBox.Size = new System.Drawing.Size(90, 82);
            this.DatabaseGroupBox.TabIndex = 4;
            this.DatabaseGroupBox.TabStop = false;
            this.DatabaseGroupBox.Text = "Database";
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(6, 48);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 1;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.Location = new System.Drawing.Point(6, 19);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(75, 23);
            this.ImportButton.TabIndex = 0;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // LogTab
            // 
            this.LogTab.Controls.Add(this.LogList);
            this.LogTab.Location = new System.Drawing.Point(4, 22);
            this.LogTab.Name = "LogTab";
            this.LogTab.Size = new System.Drawing.Size(1157, 842);
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
            this.LogList.Size = new System.Drawing.Size(1157, 842);
            this.LogList.TabIndex = 0;
            // 
            // ChangesTab
            // 
            this.ChangesTab.Controls.Add(this.ChangesText);
            this.ChangesTab.Location = new System.Drawing.Point(4, 22);
            this.ChangesTab.Name = "ChangesTab";
            this.ChangesTab.Padding = new System.Windows.Forms.Padding(3);
            this.ChangesTab.Size = new System.Drawing.Size(1157, 842);
            this.ChangesTab.TabIndex = 4;
            this.ChangesTab.Text = "Changes";
            this.ChangesTab.UseVisualStyleBackColor = true;
            // 
            // ChangesText
            // 
            this.ChangesText.BackColor = System.Drawing.SystemColors.Window;
            this.ChangesText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesText.Location = new System.Drawing.Point(3, 3);
            this.ChangesText.Name = "ChangesText";
            this.ChangesText.ReadOnly = true;
            this.ChangesText.Size = new System.Drawing.Size(1151, 836);
            this.ChangesText.TabIndex = 0;
            this.ChangesText.Text = "Loading...";
            // 
            // DevTab
            // 
            this.DevTab.Controls.Add(this.splitContainer1);
            this.DevTab.Location = new System.Drawing.Point(4, 22);
            this.DevTab.Name = "DevTab";
            this.DevTab.Size = new System.Drawing.Size(1157, 842);
            this.DevTab.TabIndex = 5;
            this.DevTab.Text = "Dev";
            this.DevTab.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CallView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CallTree);
            this.splitContainer1.Size = new System.Drawing.Size(1157, 842);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.TabIndex = 0;
            // 
            // CallView
            // 
            this.CallView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.CallView.ContextMenuStrip = this.CallEditStrip;
            this.CallView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CallView.FullRowSelect = true;
            this.CallView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.CallView.HideSelection = false;
            this.CallView.Location = new System.Drawing.Point(0, 0);
            this.CallView.MultiSelect = false;
            this.CallView.Name = "CallView";
            this.CallView.Size = new System.Drawing.Size(1157, 270);
            this.CallView.TabIndex = 0;
            this.CallView.UseCompatibleStateImageBehavior = false;
            this.CallView.View = System.Windows.Forms.View.Details;
            this.CallView.SelectedIndexChanged += new System.EventHandler(this.CallView_SelectedIndexChanged);
            this.CallView.Resize += new System.EventHandler(this.CallView_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 476;
            // 
            // CallEditStrip
            // 
            this.CallEditStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dumpToolStripMenuItem,
            this.clearToolStripMenuItem1});
            this.CallEditStrip.Name = "CallEditStrip";
            this.CallEditStrip.Size = new System.Drawing.Size(108, 48);
            // 
            // dumpToolStripMenuItem
            // 
            this.dumpToolStripMenuItem.Name = "dumpToolStripMenuItem";
            this.dumpToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.dumpToolStripMenuItem.Text = "Dump";
            this.dumpToolStripMenuItem.Click += new System.EventHandler(this.dumpToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem1
            // 
            this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
            this.clearToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.clearToolStripMenuItem1.Text = "Clear";
            this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clearToolStripMenuItem1_Click);
            // 
            // CallTree
            // 
            this.CallTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CallTree.Location = new System.Drawing.Point(0, 0);
            this.CallTree.Name = "CallTree";
            this.CallTree.Size = new System.Drawing.Size(1157, 568);
            this.CallTree.TabIndex = 0;
            // 
            // teamControl1
            // 
            this.teamControl1.BackColor = System.Drawing.Color.White;
            this.teamControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamControl1.Location = new System.Drawing.Point(0, 0);
            this.teamControl1.MinimumSize = new System.Drawing.Size(500, 836);
            this.teamControl1.Name = "teamControl1";
            this.teamControl1.PlayerContextMenuStrip = this.PlayerEditStrip;
            this.teamControl1.Size = new System.Drawing.Size(575, 836);
            this.teamControl1.TabIndex = 0;
            this.teamControl1.TeamSize = 5;
            this.teamControl1.Text = "Team 1";
            // 
            // teamControl2
            // 
            this.teamControl2.BackColor = System.Drawing.Color.White;
            this.teamControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamControl2.Location = new System.Drawing.Point(0, 0);
            this.teamControl2.MinimumSize = new System.Drawing.Size(500, 836);
            this.teamControl2.Name = "teamControl2";
            this.teamControl2.PlayerContextMenuStrip = this.PlayerEditStrip;
            this.teamControl2.Size = new System.Drawing.Size(575, 836);
            this.teamControl2.TabIndex = 1;
            this.teamControl2.TeamSize = 5;
            this.teamControl2.Text = "Team 2";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.TopChampsBox);
            this.groupBox5.Controls.Add(this.StatsBox);
            this.groupBox5.Controls.Add(this.RecentGamesBox);
            this.groupBox5.Location = new System.Drawing.Point(172, 59);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(125, 94);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Load What Stats";
            // 
            // RecentGamesBox
            // 
            this.RecentGamesBox.AutoSize = true;
            this.RecentGamesBox.Checked = true;
            this.RecentGamesBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RecentGamesBox.Location = new System.Drawing.Point(6, 19);
            this.RecentGamesBox.Name = "RecentGamesBox";
            this.RecentGamesBox.Size = new System.Drawing.Size(97, 17);
            this.RecentGamesBox.TabIndex = 0;
            this.RecentGamesBox.Text = "Recent Games";
            this.RecentGamesBox.UseVisualStyleBackColor = true;
            this.RecentGamesBox.CheckedChanged += new System.EventHandler(this.RecentGamesBox_CheckedChanged);
            // 
            // StatsBox
            // 
            this.StatsBox.AutoSize = true;
            this.StatsBox.Checked = true;
            this.StatsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.StatsBox.Location = new System.Drawing.Point(6, 42);
            this.StatsBox.Name = "StatsBox";
            this.StatsBox.Size = new System.Drawing.Size(50, 17);
            this.StatsBox.TabIndex = 1;
            this.StatsBox.Text = "Stats";
            this.StatsBox.UseVisualStyleBackColor = true;
            this.StatsBox.CheckedChanged += new System.EventHandler(this.StatsBox_CheckedChanged);
            // 
            // TopChampsBox
            // 
            this.TopChampsBox.AutoSize = true;
            this.TopChampsBox.Checked = true;
            this.TopChampsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TopChampsBox.Location = new System.Drawing.Point(6, 65);
            this.TopChampsBox.Name = "TopChampsBox";
            this.TopChampsBox.Size = new System.Drawing.Size(86, 17);
            this.TopChampsBox.TabIndex = 2;
            this.TopChampsBox.Text = "Top Champs";
            this.TopChampsBox.UseVisualStyleBackColor = true;
            this.TopChampsBox.CheckedChanged += new System.EventHandler(this.TopChampsBox_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1165, 868);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(800, 38);
            this.Name = "MainForm";
            this.Text = "LoL";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.tabControl1.ResumeLayout(false);
            this.NewsTab.ResumeLayout(false);
            this.GameTab.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.PlayerEditStrip.ResumeLayout(false);
            this.SettingsTab.ResumeLayout(false);
            this.SettingsTab.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ModuleGroupBox.ResumeLayout(false);
            this.ModuleGroupBox.PerformLayout();
            this.LogGroupBox.ResumeLayout(false);
            this.LogGroupBox.PerformLayout();
            this.DatabaseGroupBox.ResumeLayout(false);
            this.LogTab.ResumeLayout(false);
            this.ChangesTab.ResumeLayout(false);
            this.DevTab.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.CallEditStrip.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage GameTab;
        private System.Windows.Forms.TabPage SettingsTab;
        private Controls.TeamControl teamControl1;
        private System.Windows.Forms.Button InstallButton;
        private Controls.TeamControl teamControl2;
        private System.Windows.Forms.TabPage LogTab;
		private System.Windows.Forms.ListBox LogList;
        private System.Windows.Forms.ContextMenuStrip PlayerEditStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.LinkLabel DownloadLink;
        private System.Windows.Forms.TabPage ChangesTab;
        private System.Windows.Forms.RichTextBox ChangesText;
		private System.Windows.Forms.ComboBox RegionList;
		private System.Windows.Forms.TabPage DevTab;
		private System.Windows.Forms.GroupBox DatabaseGroupBox;
		private System.Windows.Forms.Button ExportButton;
		private System.Windows.Forms.Button ImportButton;
		private System.Windows.Forms.GroupBox LogGroupBox;
		private System.Windows.Forms.CheckBox DebugCheck;
		private System.Windows.Forms.CheckBox TraceCheck;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListView CallView;
		private System.Windows.Forms.ContextMenuStrip CallEditStrip;
		private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem1;
		private System.Windows.Forms.CheckBox DevCheck;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ToolStripMenuItem dumpToolStripMenuItem;
		private System.Windows.Forms.TreeView CallTree;
        private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TabPage NewsTab;
		private System.Windows.Forms.WebBrowser NewsBrowser;
		private System.Windows.Forms.CheckBox LeaveCheck;
		private System.Windows.Forms.GroupBox ModuleGroupBox;
		private System.Windows.Forms.RadioButton MirrorRadio;
		private System.Windows.Forms.RadioButton ToolHelpRadio;
		private System.Windows.Forms.RadioButton ProcessRadio;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox DefaultGameTab;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox TopChampsBox;
        private System.Windows.Forms.CheckBox StatsBox;
        private System.Windows.Forms.CheckBox RecentGamesBox;

    }
}

