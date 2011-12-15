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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.TA;
using FluorineFx;
using LoLNotes.Flash;
using LoLNotes.Gui.Controls;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.GameStats.PlayerStats;
using LoLNotes.Messages.Readers;
using LoLNotes.Messages.Translators;
using LoLNotes.Properties;
using LoLNotes.Proxy;
using LoLNotes.Storage;
using LoLNotes.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotMissing.Logging;

namespace LoLNotes.Gui
{
	public partial class MainForm : Form
	{
		static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
		static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");
		const string LoaderVersion = "1.2";
		const string SettingsFile = "settings.json";

		readonly Dictionary<string, Icon> IconCache;
		readonly Dictionary<string, CertificateHolder> Certificates;
		RtmpsProxyHost Connection;
		MessageReader Reader;
		IObjectContainer Database;
		GameStorage Recorder;
		MainSettings Settings;



		public MainForm()
		{
			InitializeComponent();

			Logger.Instance.Register(new DefaultListener(Levels.All, OnLog));
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			StaticLogger.Info(string.Format("Version {0}{1}", AssemblyAttributes.FileVersion, AssemblyAttributes.Configuration));

			Settings = new MainSettings();
			Settings.Load(SettingsFile);

			IconCache = new Dictionary<string, Icon>
            {
                {"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
                {"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
                {"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
            };
			UpdateIcon();

			Database = Db4oEmbedded.OpenFile(CreateConfig(), "db.yap");

			Certificates = new Dictionary<string, CertificateHolder>
			{
				{"NA", new CertificateHolder("prod.na1.lol.riotgames.com", Resources.prod_na1_lol_riotgames_com)},
				{"EU", new CertificateHolder("prod.eu.lol.riotgames.com", Resources.prod_eu_lol_riotgames_com)},
				{"EUN", new CertificateHolder("prod.eun1.lol.riotgames.com", Resources.prod_eun1_lol_riotgames_com)},
 			};
			foreach (var kv in Certificates)
				RegionList.Items.Add(kv.Key);
			int idx = RegionList.Items.IndexOf(Settings.Region);
			RegionList.SelectedIndex = idx != -1 ? idx : 0;	 //This ends up calling UpdateRegion so no reason to initialize the connection here.

			//Add this after otherwise it will save immediately due to RegionList.SelectedIndex
			Settings.PropertyChanged += Settings_PropertyChanged;

			StaticLogger.Info("Startup Completed");
		}

		void Settings_Loaded(object sender, EventArgs e)
		{
			TraceCheck.Checked = Settings.TraceLog;
			DebugCheck.Checked = Settings.DebugLog;
		}

		readonly object settingslock = new object();
		void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			lock (settingslock)
			{
				StaticLogger.Trace("Settings saved");
				Settings.Save(SettingsFile);
			}
		}

		void UpdateRegion()
		{
			if (Connection != null)
				Connection.Dispose();

			var cert = Certificates.FirstOrDefault(kv => kv.Key == Settings.Region).Value;
			if (cert == null)
				cert = Certificates.First().Value;

			Connection = new RtmpsProxyHost(2099, cert.Domain, 2099, cert.Certificate);
			Reader = new MessageReader(Connection);

			Connection.Connected += Connection_Connected;
			Reader.ObjectRead += Reader_ObjectRead;

			//Recorder must be initiated after Reader.ObjectRead as
			//the last event handler is called first
			Recorder = new GameStorage(Database, Connection);
			Recorder.PlayerUpdate += Recorder_PlayerUpdate;

			Connection.Start();
		}

		static IEmbeddedConfiguration CreateConfig()
		{
			var config = Db4oEmbedded.NewConfiguration();
			config.Common.ObjectClass(typeof(PlayerEntry)).ObjectField("Id").Indexed(true);
			config.Common.ObjectClass(typeof(PlayerEntry)).ObjectField("TimeStamp").Indexed(true);
			config.Common.ObjectClass(typeof(GameDTO)).ObjectField("Id").Indexed(true);
			config.Common.ObjectClass(typeof(GameDTO)).ObjectField("TimeStamp").Indexed(true);
			config.Common.ObjectClass(typeof(EndOfGameStats)).ObjectField("GameId").Indexed(true);
			config.Common.ObjectClass(typeof(EndOfGameStats)).ObjectField("TimeStamp").Indexed(true);
			config.Common.Add(new TransparentPersistenceSupport());
			config.Common.Add(new TransparentActivationSupport());
			return config;
		}

		readonly object cachelock = new object();
		void UpdatePlayer(PlayerEntry player)
		{
			lock (cachelock)
			{
				var lists = new List<TeamControl> { teamControl1, teamControl2 };
				foreach (var list in lists)
				{
					foreach (var plr in list.Players)
					{
						if (plr != null && plr.Player != null && plr.Player.Id == player.Id && player.GameTimeStamp >= plr.Player.GameTimeStamp)
						{
							plr.SetData(player);
							StaticLogger.Trace("Updating stale player " + player.Name);
							break;
						}
					}
				}
				for (int i = 0; i < PlayerCache.Count; i++)
				{
					var plrentry = PlayerCache[i];
					if (plrentry.Id == player.Id && player.GameTimeStamp >= plrentry.GameTimeStamp)
					{
						PlayerCache[i] = player;
						StaticLogger.Trace("Updating stale player cache " + player.Name);
						break;
					}
				}
			}
		}

		void Recorder_PlayerUpdate(PlayerEntry player)
		{
			Task.Factory.StartNew(() => UpdatePlayer(player));
		}

		void SetTitle(string title)
		{
			Text = string.Format(
					"LoLNotes v{0}{1}{2}",
					AssemblyAttributes.FileVersion,
					AssemblyAttributes.Configuration,
					!string.IsNullOrEmpty(title) ? " - " + title : "");
		}

		void SetDownloadLink(string link)
		{
			DownloadLink.Text = link;
		}

		void CheckVersion()
		{
			try
			{
				using (var wc = new WebClient())
				{
					string raw = wc.DownloadString("https://raw.github.com/high6/LoLNotes/master/Release.txt");
					var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
					BeginInvoke(new Action<string>(SetTitle), string.Format("v{0}{1}", dict["Version"], dict["ReleaseName"]));
					BeginInvoke(new Action<string>(SetDownloadLink), dict["Link"]);
				}
			}
			catch (WebException we)
			{
				StaticLogger.Warning(we);
			}
			catch (Exception e)
			{
				StaticLogger.Error(e);
			}
		}

		void SetChanges(string data)
		{
			try
			{
				var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

				ChangesText.Text = "";


				foreach (var kv in dict)
				{
					ChangesText.SelectionFont = new Font(ChangesText.Font.FontFamily, ChangesText.Font.SizeInPoints, FontStyle.Bold);
					ChangesText.AppendText(kv.Key);
					ChangesText.AppendText(Environment.NewLine);
					ChangesText.SelectionFont = new Font(ChangesText.Font.FontFamily, ChangesText.Font.SizeInPoints, ChangesText.Font.Style);
					if (kv.Value is JArray)
					{
						var list = kv.Value as JArray;
						foreach (var item in list)
						{
							ChangesText.AppendText(item.ToString());
							ChangesText.AppendText(Environment.NewLine);
						}
					}
					else
					{
						ChangesText.AppendText(kv.Value.ToString());
						ChangesText.AppendText(Environment.NewLine);
					}
				}
			}
			catch (Exception e)
			{
				StaticLogger.Error(e);
			}
		}

		void GetChanges()
		{
			try
			{
				using (var wc = new WebClient())
				{
					string raw = wc.DownloadString("https://raw.github.com/high6/LoLNotes/master/Changes.txt");
					ChangesText.BeginInvoke(new Action<string>(SetChanges), raw);
				}
			}
			catch (WebException we)
			{
				StaticLogger.Warning(we);
			}
		}

		void MainForm_Load(object sender, EventArgs e)
		{
			SetTitle("(Checking)");
			Task.Factory.StartNew(CheckVersion);
			Task.Factory.StartNew(GetChanges);
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogToFile(string.Format("[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})", Levels.Fatal.ToString().ToUpper(), e.ExceptionObject, DateTime.UtcNow));
		}

		void Log(Levels level, object obj)
		{
			object log = string.Format(
					"[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})",
					level.ToString().ToUpper(),
					obj,
					DateTime.UtcNow);
			Debug.WriteLine(log);
			Task.Factory.StartNew(LogToFile, log);
			Task.Factory.StartNew(AddLogToList, log);
		}

		void OnLog(Levels level, object obj)
		{
			if (level == Levels.Trace && !Settings.TraceLog)
				return;
			if (level == Levels.Debug && !Settings.DebugLog)
				return;

			if (obj is Exception)
				Log(level, string.Format("{0} [{1}]", ((Exception)obj).Message, Convert.ToBase64String(Encoding.ASCII.GetBytes(obj.ToString()))));
			else
				Log(level, obj);
		}

		void AddLogToList(object obj)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<object>(AddLogToList), obj);
				return;
			}
			if (LogList.Items.Count > 1000)
				LogList.Items.RemoveAt(0);
			LogList.Items.Add(obj.ToString());
			LogList.SelectedIndex = LogList.Items.Count - 1;
			LogList.SelectedIndex = -1;
		}

		readonly object LogLock = new object();
		const string LogFile = "Log.txt";
		void LogToFile(object obj)
		{
			try
			{
				lock (LogLock)
				{
					File.AppendAllText(LogFile, obj + Environment.NewLine);
				}
			}
			catch (Exception ex)
			{
				AddLogToList(string.Format("[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})", Levels.Fatal.ToString().ToUpper(), ex.Message, DateTime.UtcNow));
			}
		}

		void UpdateIcon()
		{
			if (!IsInstalled)
				Icon = IconCache["Red"];
			else if (Connection != null && Connection.IsConnected)
				Icon = IconCache["Green"];
			else
				Icon = IconCache["Yellow"];
		}

		void Connection_Connected(object obj)
		{
			if (Created)
				BeginInvoke(new Action(UpdateIcon));
		}

		void Reader_ObjectRead(object obj)
		{
			var lobby = obj as GameDTO;
			var game = obj as EndOfGameStats;
			if (lobby != null)
				UpdateLists(lobby);
			if (game != null)
				UpdateLists(game);
		}

		public GameDTO CurrentGame;
		public List<PlayerEntry> PlayerCache = new List<PlayerEntry>();

		public void UpdateLists(EndOfGameStats game)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<EndOfGameStats>(UpdateLists), game);
				return;
			}

			var teams = new List<PlayerStatsSummaryList> { game.TeamPlayerStats, game.OtherTeamPlayerStats };
			var lists = new List<TeamControl> { teamControl1, teamControl2 };

			for (int i = 0; i < lists.Count; i++)
			{
				var list = lists[i];
				var team = teams[i];

				if (team == null)
				{
					list.Visible = false;
					continue;
				}

				for (int o = 0; o < list.Players.Count; o++)
				{
					if (o < team.Count)
					{
						var ply = team[o];

						if (ply != null)
						{
							lock (cachelock)
							{
								var entry = PlayerCache.Find(p => p.Id == ply.UserId);
								if (entry == null)
								{
									var plycontrol = list.Players[o];
									plycontrol.Loading = true;
									plycontrol.SetData(new GameParticipant { Name = ply.SummonerName });
									Task.Factory.StartNew(() => LoadPlayer(ply.UserId, plycontrol));
								}
								else
								{
									list.Players[o].SetData(entry);
								}
							}
						}
					}
					else
					{
						list.Players[o].SetData();
					}
				}
			}
		}

		public void UpdateLists(GameDTO lobby)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<GameDTO>(UpdateLists), lobby);
				return;
			}

			if (CurrentGame == null || CurrentGame.Id != lobby.Id)
			{
				lock (cachelock)
				{
					PlayerCache.Clear();
					CurrentGame = lobby;
				}
			}
			else
			{
				//Check if the teams are the same.
				//If they are the same that means nothing has changed and we can return.
				var oldteams = new List<TeamParticipants> { CurrentGame.TeamOne, CurrentGame.TeamTwo };
				var newteams = new List<TeamParticipants> { lobby.TeamOne, lobby.TeamTwo };

				bool same = true;
				for (int i = 0; i < oldteams.Count && i < newteams.Count; i++)
				{
					if (!oldteams[i].SequenceEqual(newteams[i]))
					{
						same = false;
						break;
					}
				}

				if (same)
					return;
			}

			var teams = new List<TeamParticipants> { lobby.TeamOne, lobby.TeamTwo };
			var lists = new List<TeamControl> { teamControl1, teamControl2 };

			for (int i = 0; i < lists.Count; i++)
			{
				var list = lists[i];
				var team = teams[i];

				if (team == null)
				{
					list.Visible = false;
					continue;
				}

				for (int o = 0; o < list.Players.Count; o++)
				{
					if (o < team.Count)
					{
						var ply = team[o] as PlayerParticipant;

						if (ply != null)
						{
							lock (cachelock)
							{
								var entry = PlayerCache.Find(p => p.Id == ply.Id);
								if (entry == null)
								{
									var plycontrol = list.Players[o];
									plycontrol.Loading = true;
									plycontrol.SetData(ply);
									Task.Factory.StartNew(() => LoadPlayer(ply.Id, plycontrol));
								}
								else
								{
									list.Players[o].SetData(entry);
								}
							}
						}
						else
						{
							list.Players[o].SetData(team[o]);
						}
					}
					else
					{
						list.Players[o].SetData();
					}
				}
			}
		}

		void UpdatePlayerControl(PlayerControl control, PlayerEntry entry)
		{
			if (entry != null)
			{
				control.SetData(entry);
			}
			else
			{
				control.Loading = false;
				control.SetNoStats();
			}
		}

		/// <summary>
		/// Query and cache player data
		/// </summary>
		/// <param name="id">Id of the player to load</param>
		/// <param name="control">Control to update</param>
		void LoadPlayer(int id, PlayerControl control)
		{
			Stopwatch sw = Stopwatch.StartNew();

			var entry = Recorder.GetPlayer(id);

			if (entry == null)
			{
				//Create a fake entry so that the UpdatePlayerHandler can update it
				entry = new PlayerEntry { Id = id };
			}

			lock (cachelock)
			{
				if (PlayerCache.FindIndex(p => p.Id == entry.Id) == -1)
					PlayerCache.Add(entry);
			}

			sw.Stop();
			StaticLogger.Trace(string.Format("Player query in {0}ms", sw.ElapsedMilliseconds));

			UpdatePlayerControl(control, entry);
		}


		void Install()
		{
			try
			{
				if (!Directory.Exists(LolBansPath))
					Directory.CreateDirectory(LolBansPath);

				File.WriteAllBytes(LoaderFile, Resources.LolLoader);

				var shortfilename = AppInit.GetShortPath(LoaderFile);

				var dlls = AppInit.AppInitDlls32;
				if (!dlls.Contains(shortfilename))
				{
					dlls.Add(shortfilename);
					AppInit.AppInitDlls32 = dlls;
				}

				var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadWrite);
				foreach (var hold in Certificates)
				{
					if (store.Certificates.Contains(hold.Value.Certificate))
						continue;
					store.Add(hold.Value.Certificate);
				}
				store.Close();
			}
			catch (SecurityException se)
			{
				StaticLogger.Warning(se);
			}
			catch (Exception e)
			{
				StaticLogger.Error("Failed to install " + e);
			}
		}

		bool IsInstalled
		{
			get
			{
				try
				{
					if (!File.Exists(LoaderFile))
						return false;

					var version = FileVersionInfo.GetVersionInfo(LoaderFile);
					if (version.FileVersion == null || version.FileVersion != LoaderVersion)
						return false;

					var shortfilename = AppInit.GetShortPath(LoaderFile);
					var dlls = AppInit.AppInitDlls32;

					return dlls.Contains(shortfilename);
				}
				catch (SecurityException se)
				{
					StaticLogger.Warning(se);
					return false;
				}
			}
		}

		void Uninstall()
		{
			try
			{
				var shortfilename = AppInit.GetShortPath(LoaderFile);

				var dlls = AppInit.AppInitDlls32;
				if (dlls.Contains(shortfilename))
				{
					dlls.Remove(AppInit.GetShortPath(shortfilename));
					AppInit.AppInitDlls32 = dlls;
				}

				if (File.Exists(LoaderFile))
					File.Delete(LoaderFile);

				var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadWrite);
				foreach (var hold in Certificates)
				{
					if (!store.Certificates.Contains(hold.Value.Certificate))
						continue;
					store.Remove(hold.Value.Certificate);
				}
				store.Close();
			}
			catch (SecurityException se)
			{
				StaticLogger.Warning(se);
			}
			catch (Exception e)
			{
				StaticLogger.Error("Failed to uninstall " + e);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!Wow.IsAdministrator)
			{
				MessageBox.Show("You must run LoLNotes as admin to install/uninstall it");
				return;
			}
			try
			{

				if (IsInstalled)
				{
					Uninstall();
				}
				else
				{
					Install();
				}
			}
			catch (UnauthorizedAccessException uaex)
			{
				MessageBox.Show("Unable to fully install/uninstall. Make sure LoL is not running.");
				StaticLogger.Warning(uaex);
			}
			InstallButton.Text = IsInstalled ? "Uninstall" : "Install";
			UpdateIcon();
		}

		private void tabControl1_Selected(object sender, TabControlEventArgs e)
		{
			if (e.Action == TabControlAction.Selected && e.TabPage == SettingsTab)
			{
				InstallButton.Text = IsInstalled ? "Uninstall" : "Install";
			}
		}

		private void editToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripItem;
			if (menuItem == null)
				return;

			var owner = menuItem.Owner as ContextMenuStrip;
			if (owner == null)
				return;

			var plrcontrol = owner.SourceControl as PlayerControl;
			if (plrcontrol == null)
				return;

			if (plrcontrol.Player == null)
				return;

			var form = new EditPlayerForm(plrcontrol.Player);
			if (form.ShowDialog() != DialogResult.OK)
				return;

			plrcontrol.Player.Note = form.NoteText.Text;
			if (form.ColorBox.SelectedIndex != -1)
				plrcontrol.Player.NoteColor = Color.FromName(form.ColorBox.Items[form.ColorBox.SelectedIndex].ToString());
			plrcontrol.UpdateView();

			Task.Factory.StartNew(() => Recorder.CommitPlayer(plrcontrol.Player, true));
		}

		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripItem;
			if (menuItem == null)
				return;

			var owner = menuItem.Owner as ContextMenuStrip;
			if (owner == null)
				return;

			var plrcontrol = owner.SourceControl as PlayerControl;
			if (plrcontrol == null)
				return;

			if (plrcontrol.Player == null)
				return;

			plrcontrol.Player.Note = "";
			plrcontrol.Player.NoteColor = default(Color);
			plrcontrol.UpdateView();

			Task.Factory.StartNew(() => Recorder.CommitPlayer(plrcontrol.Player, true));
		}

		private void DownloadLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(DownloadLink.Text);
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			Settings_Loaded(this, new EventArgs());
			//Start after the form is shown otherwise Invokes will fail
			Connection.Start();
		}

		private void RegionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Region = RegionList.SelectedItem.ToString();
			UpdateRegion();
		}

		private void ImportButton_Click(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Filter = "json files (*.json)|*.json";
				ofd.InitialDirectory = Application.StartupPath;
				ofd.RestoreDirectory = true;

				if (ofd.ShowDialog() != DialogResult.OK)
					return;

				var serializer = new JsonSerializer();
				serializer.TypeNameHandling = TypeNameHandling.Auto;
				using (var json = new JsonTextReader(new StreamReader(ofd.OpenFile())))
				{
					var export = serializer.Deserialize<JsonExportHolder>(json);

					foreach (var ply in export.Players)
						Recorder.RecordPlayer(ply, false); 
					
					foreach (var lobby in export.GameDtos)
						Recorder.RecordLobby(lobby);

					foreach (var end in export.EndStats)
						Recorder.RecordGame(end);

					Recorder.Commit();
				}
			}
		}

		void ActivateList(IList list)
		{
			foreach (var obj in list)
			{
				Database.Activate(obj, int.MaxValue);
			}
		}

		class JsonExportHolder
		{
			public List<GameDTO> GameDtos;
			public List<EndOfGameStats> EndStats;
			public List<PlayerEntry> Players;
		}

		private void ExportButton_Click(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = "json files (*.json)|*.json";
				sfd.InitialDirectory = Application.StartupPath;
				sfd.RestoreDirectory = true;

				if (sfd.ShowDialog() != DialogResult.OK)
					return;

				var export = new JsonExportHolder
				{
					GameDtos = Database.Query<GameDTO>().ToList(),
					EndStats = Database.Query<EndOfGameStats>().ToList(),
					Players = Database.Query<PlayerEntry>().ToList(),
				};
				ActivateList(export.EndStats);
				ActivateList(export.GameDtos);
				ActivateList(export.Players);

				var serializer = new JsonSerializer();
				serializer.TypeNameHandling = TypeNameHandling.Auto;
				using (var json = new JsonTextWriter(new StreamWriter(sfd.OpenFile())))
				{
					json.Formatting = Formatting.Indented;
					serializer.Serialize(json, export);
				}
			}
		}

		private void DebugCheck_Click(object sender, EventArgs e)
		{
			Settings.DebugLog = DebugCheck.Checked;
		}

		private void TraceCheck_Click(object sender, EventArgs e)
		{
			Settings.TraceLog = TraceCheck.Checked;
		}
	}
}
