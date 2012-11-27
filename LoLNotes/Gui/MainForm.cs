/*
copyright (C) 2011-2012 by high828@gmail.com

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
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.TA;
using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.IO;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;
using LoLNotes.Gui.Controls;
using LoLNotes.Messages.Account;
using LoLNotes.Messages.Champion;
using LoLNotes.Messages.Commands;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using LoLNotes.Messages.Statistics;
using LoLNotes.Messages.Summoner;
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
		public static readonly string Version = AssemblyAttributes.FileVersion + AssemblyAttributes.Configuration;
		const string SettingsFile = "settings.json";

		readonly Dictionary<string, Icon> Icons;
        readonly Dictionary<string, CertificateHolder> Certificates;
		readonly Dictionary<ProcessInjector.GetModuleFrom, RadioButton> ModuleResolvers;
		readonly List<PlayerCache> PlayersCache = new List<PlayerCache>();
		readonly ProcessQueue<string> TrackingQueue = new ProcessQueue<string>();
		readonly ProcessMonitor launcher = new ProcessMonitor(new[] { "LoLLauncher" });

		RtmpsProxyHost Connection;
		MessageReader Reader;
		IObjectContainer Database;
		GameStorage Recorder;
		CertificateInstaller Installer;
		ProcessInjector Injector;
		GameDTO CurrentGame;
		List<ChampionDTO> Champions;
		SummonerData SelfSummoner;

		MainSettings Settings { get { return MainSettings.Instance; } }

		public MainForm()
		{
			InitializeComponent();

			Logger.Instance.Register(new DefaultListener(Levels.All, OnLog));
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Application.ThreadException += Application_ThreadException;
			StaticLogger.Info(string.Format("Version {0}", Version));

			Settings.Load(SettingsFile);

			Icons = new Dictionary<string, Icon>
			{
				{"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
				{"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
				{"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
			};

            Certificates = LoadCertificates(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Content/Certificates"));
            if (Certificates.Count < 1)
            {
                MessageBox.Show("Unable to load any certificates");
                Application.Exit();
                return;
            }

			ModuleResolvers = new Dictionary<ProcessInjector.GetModuleFrom, RadioButton>
			{	 
				{ProcessInjector.GetModuleFrom.Toolhelp32Snapshot, ToolHelpRadio},
				{ProcessInjector.GetModuleFrom.ProcessClass, ProcessRadio},
				{ProcessInjector.GetModuleFrom.Mirroring, MirrorRadio}
			};
			foreach (var kv in ModuleResolvers)
			{
				kv.Value.Click += moduleresolvers_Click;
			}

			Database = Db4oEmbedded.OpenFile(CreateConfig(), "db.yap");

			var cert = Certificates.FirstOrDefault(kv => kv.Key == Settings.Region).Value;
			if (cert == null)
				cert = Certificates.First().Value;

			Injector = new ProcessInjector("lolclient");
			Connection = new RtmpsProxyHost(2099, cert.Domain, 2099, cert.Certificate);
			Reader = new MessageReader(Connection);

			Connection.Connected += Connection_Connected;
			Injector.Injected += Injector_Injected;
			Reader.ObjectRead += Reader_ObjectRead;

			//Recorder must be initiated after Reader.ObjectRead as
			//the last event handler is called first
			Recorder = new GameStorage(Database, Connection);

			Connection.CallResult += Connection_Call;
			Connection.Notify += Connection_Notify;


			foreach (var kv in Certificates)
				RegionList.Items.Add(kv.Key);
			int idx = RegionList.Items.IndexOf(Settings.Region);
			RegionList.SelectedIndex = idx != -1 ? idx : 0;	 //This ends up calling UpdateRegion so no reason to initialize the connection here.

            DefaultGameTab.Text = Settings.DefaultGameTab;

			Installer = new CertificateInstaller(Certificates.Select(c => c.Value.Certificate).ToArray());

			TrackingQueue.Process += TrackingQueue_Process;
			launcher.ProcessFound += launcher_ProcessFound;

#if DEBUG
			button1.Visible = true;
#endif

			StaticLogger.Info("Startup Completed");
		}

        Dictionary<string, CertificateHolder> LoadCertificates(string path)
        {
            var ret = new Dictionary<string, CertificateHolder>();
            if (!Directory.Exists(path))
                return ret;

            foreach (var file in new DirectoryInfo(path).GetFiles("*.p12"))
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(file.Name);
                var idx = nameNoExt.IndexOf('_');
                var name = idx != -1 ? nameNoExt.Substring(0, idx) : nameNoExt;
                var host = idx != -1 ? nameNoExt.Substring(idx + 1) : nameNoExt;
                ret[name] = new CertificateHolder(host, File.ReadAllBytes(file.FullName));
            }

            return ret;
        }

		void moduleresolvers_Click(object sender, EventArgs e)
		{
			var check = ModuleResolvers.FirstOrDefault(kv => kv.Value.Checked);
			Settings.ModuleResolver = check.Key.ToString();
			Injector.Clear();
		}

		void launcher_ProcessFound(object sender, ProcessMonitor.ProcessEventArgs e)
		{
			try
			{
				if (!Settings.DeleteLeaveBuster)
					return;

				var dir = Path.GetDirectoryName(e.Process.MainModule.FileName);
				if (dir == null)
				{
					StaticLogger.Warning("Launcher module not found");
					return;
				}

				var needle = "\\RADS\\";
				var i = dir.LastIndexOf(needle, StringComparison.InvariantCulture);
				if (i == -1)
				{
					StaticLogger.Warning("Launcher Rads not found");
					return;
				}

				dir = dir.Remove(i + needle.Length);
				dir = Path.Combine(dir, "projects\\lol_air_client\\releases");

				if (!Directory.Exists(dir))
				{
					StaticLogger.Warning("lol_air_client directory not found");
					return;
				}

				foreach (var ver in new DirectoryInfo(dir).GetDirectories())
				{
					var filename = Path.Combine(ver.FullName, "deploy\\preferences\\global\\global.properties");
					if (!File.Exists(filename))
					{
						StaticLogger.Warning(filename + " not found");
						continue;
					}

					ASObject obj = null;
					using (var amf = new AMFReader(File.OpenRead(filename)))
					{
						try
						{
							obj = amf.ReadAMF3Data() as ASObject;
							if (obj == null)
							{
								StaticLogger.Warning("Failed to read " + filename);
								continue;
							}
						}
						catch (Exception ex)
						{
                            StaticLogger.Warning("LeaverBuster: Unable to read global.properties '" + filename + "'");
							continue;
						}
					}
					object leaver;
					object locale;
					if ((obj.TryGetValue("leaverData", out leaver) && leaver != null) ||
						(obj.TryGetValue("localeData", out locale) && locale != null))
					{
						obj["leaverData"] = null;
						obj["localeData"] = null;
						using (var amf = new AMFWriter(File.Open(filename, FileMode.Create, FileAccess.Write)))
						{
							try
							{
								amf.WriteAMF3Data(obj);
								StaticLogger.Info("Removed leaverData/localeData from global.properties");
							}
							catch (Exception ex)
							{
                                StaticLogger.Warning("LeaverBuster: Unable to write global.properties '" + filename + "'");
								continue;
							}
						}
					}
					else
					{
						StaticLogger.Info("leaverData/localeData already removed from global.properties");
					}
				}
			}
			catch (Exception ex)
			{
				StaticLogger.Error(ex);
			}
		}

		void TrackingQueue_Process(object sender, ProcessQueueEventArgs<string> e)
		{
			try
			{
				var hr = (HttpWebRequest)WebRequest.Create("http://bit.ly/unCoIY");
				hr.ServicePoint.Expect100Continue = false;
				hr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0";
				hr.Referer = string.Format("http://lolnotes-{0}-app.org/{1}", Version, e.Item);
				hr.AllowAutoRedirect = false;
				using (var resp = (HttpWebResponse)hr.GetResponse())
				{
				}
			}
			catch (WebException we)
			{
				StaticLogger.Warning(we);
			}
			catch (Exception ex)
			{
				StaticLogger.Warning(ex);
			}
		}

		void Injector_Injected(object sender, EventArgs e)
		{
			if (Created)
				BeginInvoke(new Action(UpdateIcon));
		}

		void Settings_Loaded(object sender, EventArgs e)
		{
			TraceCheck.Checked = Settings.TraceLog;
			DebugCheck.Checked = Settings.DebugLog;
			DevCheck.Checked = Settings.DevMode;
			LeaveCheck.Checked = Settings.DeleteLeaveBuster;
			var mod = ModuleResolvers.FirstOrDefault(kv => kv.Key.ToString() == Settings.ModuleResolver);
			if (mod.Value == null)
				mod = ModuleResolvers.First();
			mod.Value.Checked = true;
            TopChampsBox.Checked = (Settings.LoadWhatData & LoadDataEnum.TopChamps) != 0;
            StatsBox.Checked = (Settings.LoadWhatData & LoadDataEnum.Stats) != 0;
            RecentGamesBox.Checked = (Settings.LoadWhatData & LoadDataEnum.RecentGames) != 0;
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

		void SetTitle(string title)
		{
			Text = string.Format(
					"LoLNotes v{0}{1}",
					Version,
					!string.IsNullOrEmpty(title) ? " - " + title : "");
		}

		//Allows for FInvoke(delegate {});
		void FInvoke(MethodInvoker inv)
		{
			BeginInvoke(inv);
		}


		void SetRelease(JObject data)
		{
			if (data == null)
				return;
			SetTitle(string.Format("v{0}{1}", data.Value<string>("Version"), data.Value<string>("ReleaseName")));
            DownloadLink.Links.Add(0, DownloadLink.Text.Length + 1, data.Value<string>("Link"));
		}

		void SetChanges(JObject data)
		{
			if (data == null)
				return;
			try
			{
				ChangesText.Text = "";

				foreach (var kv in data)
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

		void SetNews(JObject data)
		{
			if (data == null)
				return;
			NewsBrowser.Navigate("about:blank");
			if (NewsBrowser.Document != null)
				NewsBrowser.Document.Write(string.Empty);
			NewsBrowser.DocumentText = data.Value<string>("html");
		}

		void GetGeneral()
		{
			try
			{
				using (var wc = new WebClient())
				{
					string raw = wc.DownloadString("https://raw.github.com/bladecoding/LoLNotes/master/General.txt");
					var json = JsonConvert.DeserializeObject<JObject>(raw);
					FInvoke(delegate
					{
						SetChanges(json["Changes"] as JObject);
						SetRelease(json["Release"] as JObject);
						SetNews(json["News"] as JObject);
					});
				}
			}
			catch (JsonReaderException jre)
			{
				StaticLogger.Warning(jre);
			}
			catch (WebException we)
			{
				StaticLogger.Warning(we);
			}
		}

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			LogException(e.Exception, true);
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogException((Exception)e.ExceptionObject, !e.IsTerminating);
			//Bypass the queue and log it now if we are terminating.
			if (e.IsTerminating)
				TrackingQueue_Process(this, new ProcessQueueEventArgs<string> { Item = string.Format("error/{0}", Parse.ToBase64(e.ExceptionObject.ToString())) });
		}

		void LogException(Exception ex, bool track)
		{
            var log = string.Format(
               "[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})",
               Levels.Fatal.ToString().ToUpper(),
               string.Format("{0} [{1}]", ex.Message, Parse.ToBase64(ex.ToString())),
               DateTime.UtcNow);
            LogToFile(log);
            AddLogToList(log);

			if (track)
				TrackingQueue.Enqueue(string.Format("error/{0}", Parse.ToBase64(ex.ToString())));
		}

		void Log(Levels level, object obj)
		{
			var log = string.Format(
					"[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})",
					level.ToString().ToUpper(),
					obj,
					DateTime.UtcNow);
            Task.Factory.StartNew(LogToFile, log, TaskCreationOptions.LongRunning);
            AddLogToList(log);
		}

		void OnLog(Levels level, object obj)
		{
			if (level == Levels.Trace && !Settings.TraceLog)
				return;
			if (level == Levels.Debug && !Settings.DebugLog)
				return;

			if (level == Levels.Error && obj is Exception)
			{
				TrackingQueue.Enqueue(string.Format("error/{0}", Parse.ToBase64(obj.ToString())));
			}

			if (obj is Exception)
				Log(level, string.Format("{0} [{1}]", ((Exception)obj).Message, Parse.ToBase64(obj.ToString())));
			else
				Log(level, obj);
		}

		void AddLogToList(string log)
		{
			if (InvokeRequired)
			{
                BeginInvoke(new Action<string>(AddLogToList), log);
				return;
			}
			if (LogList.Items.Count > 1000)
				LogList.Items.RemoveAt(0);
            LogList.Items.Add(log);
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
			if (!Injector.IsInjected)
				Icon = Icons["Red"];
			else if (Connection != null && Connection.IsConnected)
				Icon = Icons["Green"];
			else
				Icon = Icons["Yellow"];
		}

		void Connection_Connected(object sender, EventArgs e)
		{
			if (Created)
				BeginInvoke(new Action(UpdateIcon));
		}

		void Reader_ObjectRead(object obj)
		{
			if (obj is GameDTO)
				UpdateLists((GameDTO)obj);
			else if (obj is EndOfGameStats)
				ClearCache(); //clear the player cache after each match.
			else if (obj is List<ChampionDTO>)
				Champions = (List<ChampionDTO>)obj;
			else if (obj is LoginDataPacket)
				SelfSummoner = ((LoginDataPacket)obj).AllSummonerData.Summoner;
		}

		public void ClearCache()
		{
			lock (PlayersCache)
			{
				PlayersCache.Clear();
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
				CurrentGame = lobby;
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

				CurrentGame = lobby;
			}

			var teamids = new List<Int64>();
			var teams = new List<TeamParticipants> { lobby.TeamOne, lobby.TeamTwo };
			var lists = new List<TeamControl> { teamControl1, teamControl2 };


            //Load the opposite team first. Not currently useful with the way things are loaded.
            if (SelfSummoner != null && lobby.TeamOne.Find(p => p is PlayerParticipant && ((PlayerParticipant)p).SummonerId == SelfSummoner.SummonerId) != null)
            {
                teams.Reverse();
                lists.Reverse();
            }

			for (int i = 0; i < lists.Count; i++)
			{
				var list = lists[i];
				var team = teams[i];

				if (team == null)
				{
					list.Visible = false;
					continue;
				}
				list.Visible = true;

				for (int o = 0; o < list.Players.Count; o++)
				{
					list.Players[o].Tag = null;
					if (o < team.Count)
					{
						var plycontrol = list.Players[o];
						plycontrol.Visible = true;
						var ply = team[o] as PlayerParticipant;

						if (ply != null && ply.SummonerId != 0)
						{
							//Used for synchronization.
							//Ensures that when the loaded data comes back to the UI thread that its what we wanted.
							plycontrol.Tag = ply;

							plycontrol.SetLoading(true);
							plycontrol.SetEmpty();
							plycontrol.SetParticipant(ply);
                            Task.Factory.StartNew(() => LoadPlayer(ply, plycontrol), TaskCreationOptions.LongRunning);
						}
						else
						{
							plycontrol.SetEmpty();
							plycontrol.SetParticipant(team[o]);
						}

						if (ply != null)
						{
							if (ply.TeamParticipantId != 0)
							{
								var idx = teamids.FindIndex(t => t == ply.TeamParticipantId);
								if (idx == -1)
								{
									idx = teamids.Count;
									teamids.Add(ply.TeamParticipantId);
								}
								plycontrol.SetTeam(idx + 1);
							}
						}
					}
					else
					{
						list.Players[o].Visible = false;
						list.Players[o].SetEmpty();
					}
				}
			}
		}

		void LoadPlayerUIFinish(PlayerCache ply, PlayerControl control)
		{
			FInvoke(delegate
			{
				//Now that we are back on the UI thread.
				//Lets check to make sure the control still wants this specific data.
				if (control.Tag == null || ((PlayerParticipant)control.Tag).SummonerId != ply.Summoner.SummonerId)
					return;


                control.DefaultGameTab = Settings.DefaultGameTab;
				control.SetPlayer(ply.Player);
                if (ply.Stats != null)
				    control.SetStats(ply.Summoner, ply.Stats);
                if (ply.RecentChamps != null)
				    control.SetChamps(ply.RecentChamps);
                if (ply.Games != null)
				    control.SetGames(ply.Games);
				control.SetSeen(ply.SeenCount);
				control.SetLoading(false);

				if (ply.Stats != null)
				{
					foreach (var stat in ply.Stats.PlayerStatSummaries.PlayerStatSummarySet)
					{
						if (!comboBox1.Items.Contains(stat.PlayerStatSummaryType))
							comboBox1.Items.Add(stat.PlayerStatSummaryType);
                        if (!DefaultGameTab.Items.Contains(stat.PlayerStatSummaryType))
                            DefaultGameTab.Items.Add(stat.PlayerStatSummaryType);
					}
				}
			});
		}

		/// <summary>
		/// Query and cache player data
		/// </summary>
		/// <param name="player">Player to load</param>
		/// <param name="control">Control to update</param>
		void LoadPlayer(PlayerParticipant player, PlayerControl control)
		{
			PlayerCache existing;
			var ply = new PlayerCache();
			try
			{

				lock (PlayersCache)
				{
					//Clear the cache every 50000 players to prevent crashing afk lobbies.
					if (PlayersCache.Count > 50000)
						PlayersCache.Clear();

					//Does the player already exist in the cache?
					if ((existing = PlayersCache.Find(p => p.Player != null && p.Player.Id == player.SummonerId)) == null)
					{
						PlayersCache.Add(ply);
					}
				}

				//If another thread is loading the player data, lets wait for it to finish and use its data.
				if (existing != null)
				{
					existing.LoadWait.WaitOne();
					LoadPlayerUIFinish(existing, control);
					return;
				}


				using (SimpleLogTimer.Start("Player query"))
				{
					var entry = Recorder.GetPlayer(player.SummonerId);
					ply.Player = entry ?? ply.Player;
				}

				using (SimpleLogTimer.Start("Stats query"))
				{
					var cmd = new PlayerCommands(Connection);
					var summoner = cmd.GetPlayerByName(player.Name);
					if (summoner != null)
					{
						ply.Summoner = summoner;
                        if ((Settings.LoadWhatData & LoadDataEnum.Stats) != 0)
						    ply.Stats = cmd.RetrievePlayerStatsByAccountId(summoner.AccountId);
                        if ((Settings.LoadWhatData & LoadDataEnum.TopChamps) != 0)
                            ply.RecentChamps = cmd.RetrieveTopPlayedChampions(summoner.AccountId, "CLASSIC");
                        if ((Settings.LoadWhatData & LoadDataEnum.RecentGames) != 0) 
                            ply.Games = cmd.GetRecentGames(summoner.AccountId);
					}
					else
					{
						StaticLogger.Debug(string.Format("Player {0} not found", player.Name));
						ply.LoadWait.Set();
						return;
					}
				}

				using (SimpleLogTimer.Start("Seen query"))
				{
					if (SelfSummoner != null && SelfSummoner.SummonerId != ply.Summoner.SummonerId && ply.Games != null)
						ply.SeenCount = ply.Games.GameStatistics.Count(pgs => pgs.FellowPlayers.Any(fp => fp.SummonerId == SelfSummoner.SummonerId));
				}

				ply.LoadWait.Set();
				LoadPlayerUIFinish(ply, control);
			}
			catch (Exception ex)
			{
				ply.LoadWait.Set(); //
				StaticLogger.Warning(ex);
			}
		}

		private void InstallButton_Click(object sender, EventArgs e)
		{
			if (!Wow.IsAdministrator)
			{
				MessageBox.Show("You must run LoLNotes as admin to install/uninstall it");
				return;
			}
			try
			{

				if (Installer.IsInstalled)
				{
					Installer.Uninstall();
				}
				else
                {
                    Installer.Uninstall();
					Installer.Install();
				}
			}
			catch (UnauthorizedAccessException uaex)
			{
				MessageBox.Show("Unable to fully install/uninstall. Make sure LoL is not running.");
				StaticLogger.Warning(uaex);
			}
			InstallButton.Text = Installer.IsInstalled ? "Uninstall" : "Install";
			UpdateIcon();
		}

		private void tabControl1_Selected(object sender, TabControlEventArgs e)
		{
			if (e.Action == TabControlAction.Selected && e.TabPage == SettingsTab)
			{
				InstallButton.Text = Installer.IsInstalled ? "Uninstall" : "Install";
			}
		}

		static T GetParent<T>(Control c) where T : Control
		{
			if (c == null)
				return null;
			if (c.GetType() == typeof(T))
			{
				return (T)c;
			}
			return GetParent<T>(c.Parent);
		}

		private void editToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripItem;
			if (menuItem == null)
				return;

			var owner = menuItem.Owner as ContextMenuStrip;
			if (owner == null)
				return;

			var plrcontrol = GetParent<PlayerControl>(owner.SourceControl);
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
			plrcontrol.SetPlayer(plrcontrol.Player); //Forces the notes/color to update

            Task.Factory.StartNew(() => Recorder.CommitPlayer(plrcontrol.Player), TaskCreationOptions.LongRunning);
		}

		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripItem;
			if (menuItem == null)
				return;

			var owner = menuItem.Owner as ContextMenuStrip;
			if (owner == null)
				return;

			var plrcontrol = GetParent<PlayerControl>(owner.SourceControl);
			if (plrcontrol == null)
				return;

			if (plrcontrol.Player == null)
				return;

			plrcontrol.Player.Note = "";
			plrcontrol.Player.NoteColor = default(Color);
			plrcontrol.SetPlayer(plrcontrol.Player); //Forces the notes/color to update

            Task.Factory.StartNew(() => Recorder.CommitPlayer(plrcontrol.Player), TaskCreationOptions.LongRunning);
		}

		private void DownloadLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start((string)e.Link.LinkData);
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			SetTitle("(Checking)");
            Task.Factory.StartNew(GetGeneral, TaskCreationOptions.LongRunning);
			TrackingQueue.Enqueue("startup");

			Settings_Loaded(this, new EventArgs());
			UpdateIcon();

			//Add this after otherwise it will save immediately due to RegionList.SelectedIndex
			Settings.PropertyChanged += Settings_PropertyChanged;

			//Start after the form is shown otherwise Invokes will fail
			Connection.Start();
			Injector.Start();
			launcher.Start();

			try
			{
				var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans", "LoLLoader.dll");
				if (File.Exists(filename))
				{
					StaticLogger.Info("Uninstalling old loader");

					var shortfilename = AppInit.GetShortPath(filename);

					var dlls = AppInit.AppInitDlls32;
					if (dlls.Contains(shortfilename))
					{
						dlls.Remove(AppInit.GetShortPath(shortfilename));
						AppInit.AppInitDlls32 = dlls;
					}

					if (File.Exists(filename))
						File.Delete(filename);
				}
			}
			catch (SecurityException se)
			{
				StaticLogger.Warning(se);
			}
			catch (Exception ex)
			{
				StaticLogger.Error("Failed to uninstall " + ex);
			}
		}

		private void RegionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Region = RegionList.SelectedItem.ToString();

			var cert = Certificates.FirstOrDefault(kv => kv.Key == Settings.Region).Value;
			if (cert == null)
				cert = Certificates.First().Value;

			Connection.ChangeRemote(cert.Domain, cert.Certificate);
		}

        private void DefaultGameTab_TextChanged(object sender, EventArgs e)
        {
            Settings.DefaultGameTab = DefaultGameTab.Text;
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

				using (var fs = ofd.OpenFile())
				{
					DbExporter.Import(Recorder, fs);
				}
			}
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

				using (var fs = sfd.OpenFile())
				{
					DbExporter.Export(Version, Database, fs);
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

		private void DevCheck_Click(object sender, EventArgs e)
		{
			Settings.DevMode = DevCheck.Checked;
		}

		static string CallArgToString(object arg)
		{
			if (arg is RemotingMessage)
			{
				return ((RemotingMessage)arg).operation;
			}
			if (arg is DSK)
			{
				var dsk = (DSK)arg;
				var ao = dsk.Body as ASObject;
				if (ao != null)
					return ao.TypeName;
			}
			if (arg is CommandMessage)
			{
				return CommandMessage.OperationToString(((CommandMessage)arg).operation);
			}
			return arg.ToString();
		}

		void Connection_Call(object sender, Notify call, Notify result)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<object, Notify, Notify>(Connection_Call), sender, call, result);
				return;
			}

			if (!DevCheck.Checked)
				return;

			var text = string.Format(
				"Call {0} ({1}), Return ({2})",
				call.ServiceCall.ServiceMethodName,
				string.Join(", ", call.ServiceCall.Arguments.Select(CallArgToString)),
				string.Join(", ", result.ServiceCall.Arguments.Select(CallArgToString))
			);
			var item = new ListViewItem(text)
			{
				Tag = new List<Notify> { call, result }
			};

			CallView.Items.Add(item);

		}
		void Connection_Notify(object sender, Notify notify)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<object, Notify>(Connection_Notify), sender, notify);
				return;
			}

			if (!DevCheck.Checked)
				return;

			var text = string.Format(
				"Recv {0}({1})",
				!string.IsNullOrEmpty(notify.ServiceCall.ServiceMethodName) ? notify.ServiceCall.ServiceMethodName + " " : "",
				string.Join(", ", notify.ServiceCall.Arguments.Select(CallArgToString))
			);
			var item = new ListViewItem(text)
			{
				Tag = new List<Notify> { notify }
			};

			CallView.Items.Add(item);
		}

		private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			CallView.Items.Clear();
		}

		private void CallView_Resize(object sender, EventArgs e)
		{
			CallView.Columns[0].Width = CallView.Width;
		}

		static TreeNode GetNode(object arg, string name = "")
		{
			if (arg is ASObject)
			{
				var ao = (ASObject)arg;
				var children = new List<TreeNode>();
				foreach (var kv in ao)
				{
					var node = GetNode(kv.Value, kv.Key);
					if (node == null)
						node = new TreeNode(kv.Key + " = " + (kv.Value ?? "null"));
					children.Add(node);
				}

				string typename = ao.TypeName;
				if (typename == null && children.Count > 1)
					typename = "ASObject";

				string text = name;
				if (!string.IsNullOrEmpty(text))
					text += " ";
				text += (typename != null ? "(" + typename + ")" : "null");

				return new TreeNode(text, children.ToArray());
			}
			if (arg is Dictionary<string, object>)
			{
				if (string.IsNullOrEmpty(name))
					name = "Dictionary";

				var dict = (Dictionary<string, object>)arg;
				var children = new List<TreeNode>();
				foreach (var kv in dict)
				{
					var node = GetNode(kv.Value, kv.Key);
					if (node == null)
						node = new TreeNode(kv.Key + " = " + (kv.Value ?? "null"));
					children.Add(node);
				}
				return new TreeNode(name, children.ToArray());
			}
			if (arg is ArrayCollection)
			{
				var list = (ArrayCollection)arg;
				var children = new List<TreeNode>();
				for (int i = 0; i < list.Count; i++)
				{
					var node = GetNode(list[i], "[" + i + "]");
					if (node == null)
						node = new TreeNode(list[i].ToString());
					children.Add(node);
				}
				if (!string.IsNullOrEmpty(name))
					name += " ";
				name += "(Array)";
				if (children.Count < 1)
				{
					name += " = { }";
				}
				return new TreeNode(name, children.ToArray());
			}
			if (arg is object[])
			{
				var list = (object[])arg;
				var children = new List<TreeNode>();
				for (int i = 0; i < list.Length; i++)
				{
					var node = GetNode(list[i], "[" + i + "]");
					if (node == null)
						node = new TreeNode(list[i].ToString());
					children.Add(node);
				}
				if (!string.IsNullOrEmpty(name))
					name = " ";
				name += "(Array)";
				if (children.Count < 1)
				{
					name += " = { }";
				}
				return new TreeNode(name, children.ToArray());
			}
			return null;
		}

		private void CallView_SelectedIndexChanged(object sender, EventArgs e)
		{
			CallTree.Nodes.Clear();

			if (CallView.SelectedItems.Count < 1)
				return;

			var notifies = CallView.SelectedItems[0].Tag as List<Notify>;
			if (notifies == null)
				return;

			foreach (var notify in notifies)
			{
				var children = new List<TreeNode>();
				var bodies = RtmpUtil.GetBodies(notify);
				foreach (var body in bodies)
				{
					children.Add(GetNode(body.Item1) ?? new TreeNode(body.Item1 != null ? body.Item1.ToString() : ""));
				}

				CallTree.Nodes.Add(new TreeNode(!RtmpUtil.IsResult(notify) ? "Call" : "Return", children.ToArray()));
			}

			foreach (TreeNode node in CallTree.Nodes)
			{
				node.Expand();
				foreach (TreeNode node2 in node.Nodes)
				{
					node2.Expand();
				}
			}
		}

		/// <summary>
		/// Recursively adds a "TypeName" key to the ASObjects as newtonsoft doesn't serialize it.
		/// </summary>
		/// <param name="obj"></param>
		void AddMissingTypeNames(object obj)
		{
			if (obj == null)
				return;

			if (obj is ASObject)
			{
				var ao = (ASObject)obj;
				ao["TypeName"] = ao.TypeName;
				foreach (var kv in ao)
					AddMissingTypeNames(kv.Value);
			}
			else if (obj is IList)
			{
				var list = (IList)obj;
				foreach (var item in list)
					AddMissingTypeNames(item);
			}
		}

		private void dumpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CallView.SelectedItems.Count < 1)
				return;

			var notifies = CallView.SelectedItems[0].Tag as List<Notify>;
			if (notifies == null)
				return;

			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = "text files (*.txt)|*.txt";
				sfd.InitialDirectory = Application.StartupPath;
				sfd.RestoreDirectory = true;

				if (sfd.ShowDialog() != DialogResult.OK)
					return;

				//TypeName in ASObject is not serialized(Most likely due to inheriting Dictionary?).
				//Lets manually add the TypeName field to the dictionary.
				foreach (var notify in notifies)
				{
					var bodies = RtmpUtil.GetBodies(notify);
					foreach (var body in bodies)
						AddMissingTypeNames(body.Item1);
				}

				using (var sw = new StreamWriter(sfd.OpenFile()))
				{
					sw.Write(JsonConvert.SerializeObject(notifies, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));
				}
			}
		}

		private void MainForm_ResizeBegin(object sender, EventArgs e)
		{
			SuspendLayout();
		}

		private void MainForm_ResizeEnd(object sender, EventArgs e)
		{
			ResumeLayout();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			var str = PlayerControl.MinifyStatType(comboBox1.SelectedItem as string);
			var teams = new List<TeamControl> { teamControl1, teamControl2 };
			foreach (var team in teams)
			{
				foreach (var ply in team.Players)
				{
					for (int i = 0; i < ply.InfoTabs.TabPages.Count; i++)
					{
						if (ply.InfoTabs.TabPages[i].Text == str)
						{
							ply.InfoTabs.SelectedIndex = i;
							break;
						}
					}
				}
			}
			comboBox1.SelectedIndex = -1;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var cmd = new PlayerCommands(Connection);
			var summoner = cmd.GetPlayerByName(SelfSummoner.Username);
			if (summoner != null)
			{
				cmd.RetrievePlayerStatsByAccountId(summoner.AccountId);
				cmd.RetrieveTopPlayedChampions(summoner.AccountId, "CLASSIC");
				cmd.GetRecentGames(summoner.AccountId);
			}


			//var cmd = new PlayerCommands(Connection);
			//var obj = cmd.InvokeServiceUnknown(
			//    "gameService",
			//    "quitGame"
			//);

			//if (Champions == null)
			//    return;

			//var sorted = Champions.OrderBy(c => ChampNames.Get(c.ChampionId)).ToList();

			//var cmd = new PlayerCommands(Connection);
			//for (int i = 0; i < sorted.Count; i++)
			//{
			//    if (sorted[i].FreeToPlay || sorted[i].Owned)
			//    {
			//        var id = sorted[i].ChampionId;
			//        //ThreadPool.QueueUserWorkItem(delegate
			//        //{
			//        var obj = cmd.InvokeServiceUnknown(
			//            "gameService",
			//            "selectChampion",
			//            id
			//        );
			//        //});
			//    }
			//}
        }

        private void RecentGamesBox_CheckedChanged(object sender, EventArgs e)
        {
            if (RecentGamesBox.Checked)
                Settings.LoadWhatData |= LoadDataEnum.RecentGames;
            else
                Settings.LoadWhatData &= ~LoadDataEnum.RecentGames;
        }

        private void StatsBox_CheckedChanged(object sender, EventArgs e)
        {
            if (StatsBox.Checked)
                Settings.LoadWhatData |= LoadDataEnum.Stats;
            else
                Settings.LoadWhatData &= ~LoadDataEnum.Stats;
        }

        private void TopChampsBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TopChampsBox.Checked)
                Settings.LoadWhatData |= LoadDataEnum.TopChamps;
            else
                Settings.LoadWhatData &= ~LoadDataEnum.TopChamps;
        }
	}
}
