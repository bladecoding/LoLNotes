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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.TA;
using LoLNotes.Flash;
using LoLNotes.Gui.Controls;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using LoLNotes.Messages.Translators;
using LoLNotes.Properties;
using LoLNotes.Storage;
using LoLNotes.Util;
using NotMissing.Logging;

namespace LoLNotes.Gui
{
    public partial class MainForm : Form
    {
        static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
        static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");

        readonly Dictionary<string, Icon> IconCache;
        readonly PipeProcessor Connection;
        readonly MessageReader Reader;
        readonly IObjectContainer Database;
        readonly GameStorage Recorder;

        public MainForm()
        {
            InitializeComponent();

            Logger.Instance.Register(new DefaultListener(Levels.All, OnLog));

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            IconCache = new Dictionary<string, Icon>
            {
                {"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
                {"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
                {"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
            };

            UpdateIcon();

            var config = Db4oEmbedded.NewConfiguration();

            config.Common.ObjectClass(typeof(PlayerEntry)).ObjectField("Id").Indexed(true);
            config.Common.ObjectClass(typeof(PlayerEntry)).ObjectField("TimeStamp").Indexed(true);
            config.Common.ObjectClass(typeof(GameDTO)).ObjectField("Id").Indexed(true);
            config.Common.ObjectClass(typeof(GameDTO)).ObjectField("TimeStamp").Indexed(true);
            config.Common.ObjectClass(typeof(EndOfGameStats)).ObjectField("GameId").Indexed(true);
            config.Common.ObjectClass(typeof(EndOfGameStats)).ObjectField("TimeStamp").Indexed(true);

            config.Common.Add(new TransparentPersistenceSupport());
            config.Common.Add(new TransparentActivationSupport());

            Database = Db4oEmbedded.OpenFile(config, "db.yap");

            Connection = new PipeProcessor("lolbans");
            Reader = new MessageReader(Connection);
            Recorder = new GameStorage(Database, Connection);

            Connection.Connected += Connection_Connected;
            Reader.ObjectRead += Reader_ObjectRead;

            Connection.Start();

#if TESTING
            var pipe = new NamedPipeServerStream("lolbans", PipeDirection.InOut, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            pipe.BeginWaitForConnection(delegate(IAsyncResult ar)
            {
                pipe.EndWaitForConnection(ar);
                var bytes = File.ReadAllBytes("ExampleData\\ExampleEndOfGameStats.txt");
                pipe.Write(bytes, 0, bytes.Length);
                bytes = File.ReadAllBytes("ExampleData\\ExampleGameDTO.txt");
                pipe.Write(bytes, 0, bytes.Length);
            }, pipe);
#endif

            StaticLogger.Info("Startup Completed");
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogToFile(string.Format("[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})", Levels.Fatal.ToString().ToUpper(), e.ExceptionObject, DateTime.UtcNow));
        }

        void OnLog(Levels level, object obj)
        {
            object log = string.Format("[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})", level.ToString().ToUpper(), obj, DateTime.UtcNow);
            Debug.WriteLine(log);
            Task.Factory.StartNew(LogToFile, log);
            //if ((level & Levels.Trace) == 0)
            Task.Factory.StartNew(AddLogToList, log);
        }

        void AddLogToList(object obj)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object>(AddLogToList), obj);
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
            lock (LogLock)
            {
                File.AppendAllText(LogFile, obj + Environment.NewLine);
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
                Invoke(new Action(UpdateIcon));
        }

        void Reader_ObjectRead(object obj)
        {
            var game = obj as GameDTO;
            if (game == null)
                return;

            UpdateLists(game);
        }

        public GameDTO CurrentGame;
        public List<PlayerEntry> PlayerCache = new List<PlayerEntry>();

        public void UpdateLists(GameDTO game)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<GameDTO>(UpdateLists), game);
                return;
            }

            if (CurrentGame == null || CurrentGame.Id != game.Id)
            {
                PlayerCache.Clear();
                CurrentGame = game;
            }
            else
            {
                //Check if the teams are the same.
                //If they are the same that means nothing has changed and we can return.
                var oldteams = new List<TeamParticipants> { CurrentGame.TeamOne, CurrentGame.TeamTwo };
                var newteams = new List<TeamParticipants> { CurrentGame.TeamOne, CurrentGame.TeamTwo };

                for (int i = 0; i < oldteams.Count && i < newteams.Count; i++)
                {
                    if (!oldteams[i].SequenceEqual(newteams[i]))
                        return;
                }
            }

            var teams = new List<TeamParticipants> { game.TeamOne, game.TeamTwo };
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
                            var entry = PlayerCache.Find(p => p.Id == ply.Id);
                            if (entry == null)
                            {
                                var plycontrol = list.Players[o];
                                Task.Factory.StartNew(() => LoadPlayer(ply.Id, plycontrol));
                                plycontrol.Loading = true;
                                plycontrol.SetData(team[o]);
                            }
                            else
                            {
                                list.Players[o].SetData(entry);
                            }
                        }
                        else
                        {
                            list.Players[o].SetData(team[o]);
                        }
                        list.Players[o].Visible = true;
                    }
                    else
                    {
                        list.Players[o].Visible = false;
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

            var entry = Database.Query<PlayerEntry>().
                Where(e => e.Id == id).
                FirstOrDefault();

            if (entry != null)
                PlayerCache.Add(entry);

            sw.Stop();
            StaticLogger.Trace(string.Format("Player query in {0}ms", sw.ElapsedMilliseconds));

            control.Invoke(new Action<PlayerControl, PlayerEntry>(UpdatePlayerControl), control, entry);
        }


        void Install()
        {
            if (!Directory.Exists(LolBansPath))
                Directory.CreateDirectory(LolBansPath);

            if (!File.Exists(LoaderFile))
                File.WriteAllBytes(LoaderFile, Resources.LolLoader);

            var shortfilename = Wow.GetShortPath(LoaderFile);

            var dlls = Wow.AppInitDlls32;
            if (!dlls.Contains(shortfilename))
            {
                dlls.Add(Wow.GetShortPath(shortfilename));
                Wow.AppInitDlls32 = dlls;
            }
        }

        bool IsInstalled
        {
            get
            {
                if (!File.Exists(LoaderFile))
                    return false;

                var shortfilename = Wow.GetShortPath(LoaderFile);
                var dlls = Wow.AppInitDlls32;

                return dlls.Contains(shortfilename);
            }
        }

        void Uninstall()
        {
            var shortfilename = Wow.GetShortPath(LoaderFile);

            var dlls = Wow.AppInitDlls32;
            if (dlls.Contains(shortfilename))
            {
                dlls.Remove(Wow.GetShortPath(shortfilename));
                Wow.AppInitDlls32 = dlls;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Wow.IsAdministrator)
            {
                MessageBox.Show("You must run LoLBans as admin to install/uninstall it");
                return;
            }
            if (IsInstalled)
            {
                Uninstall();
            }
            else
            {
                Install();
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

        static string GetRadsPath()
        {
            var proc = Process.GetProcessesByName("LoLLauncher").FirstOrDefault();
            if (proc == null)
                return null;

            const string search = "rads";
            string path = proc.MainModule.FileName;
            int idx = path.ToLower().IndexOf(search);
            if (idx == -1)
                return null;

            return path.Substring(0, idx + search.Length);
        }

        private void RebuildButton_Click(object sender, EventArgs e)
        {
            if (RebuildWorker.IsBusy)
            {
                StaticLogger.Warning("Rebuild working already running");
                return;
            }

            string path = GetRadsPath();
            if (path == null)
            {
                var msg = "LoLLauncher must be running to rebuild. Or Rads is missing";
                StaticLogger.Warning(msg);
                MessageBox.Show(msg);
                return;
            }

            RebuildButton.Text = "Rebuilding";
            RebuildWorker.RunWorkerAsync(path);
        }

        private void RebuildWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var watch = Stopwatch.StartNew();
            e.Result = watch;

            var radspath = (string)e.Argument;
            var releasepath = Path.Combine(radspath, "projects\\lol_air_client\\releases");
            if (!Directory.Exists(releasepath))
            {
                StaticLogger.Warning("Unable to locate " + releasepath);
                return;
            }

            var logs = new List<FileInfo>();
            foreach (var dir in new DirectoryInfo(releasepath).GetDirectories())
            {
                var logpath = Path.Combine(dir.FullName, "deploy\\logs");
                if (!dir.Exists)
                    continue;

                foreach (var file in new DirectoryInfo(logpath).GetFiles())
                {
                    if (file.Exists)
                        logs.Add(file);
                }
            }

            long current = 0;
            long filesizes = logs.Sum(file => file.Length);
            long currentfile = 0;

            foreach (var file in logs)
            {
                StaticLogger.Info(string.Format("Rebuilding {0}, {1}/{2} ({3}%)",
                    file.Name,
                    currentfile,
                    logs.Count,
                    (int)((Double)current / filesizes * 100d)
                ));
                try
                {
                    using (var reader = new LogReader(file.OpenRead()))
                    {

                        var templobbies = new List<GameDTO>();

                        try
                        {
                            while (true)
                            {
                                var flashobj = reader.Read() as FlashObject;
                                if (flashobj == null)
                                    continue;

                                var obj = MessageTranslator.Instance.GetObject(flashobj);
                                var stats = obj as EndOfGameStats;
                                var lobby = obj as GameDTO;

                                if (stats != null)
                                    Recorder.CommitGame(stats);
                                else if (lobby != null)
                                    templobbies.Add(lobby);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                        }

                        if (templobbies.Count > 0)
                        {
                            //Recording lobbies can be pre-filtered to improve store times
                            //By reducing the checks before sending to the database.
                            var lobbies = new List<GameDTO>();
                            foreach (var lobby in templobbies)
                            {
                                var idx = lobbies.FindIndex(l => l.Id == lobby.Id);
                                if (idx != -1)
                                {
                                    if (lobby.TimeStamp > lobbies[idx].TimeStamp)
                                        lobbies[idx] = lobby;
                                }
                                else
                                {
                                    lobbies.Add(lobby);
                                }
                            }

                            foreach (var lobby in lobbies)
                                Recorder.CommitLobby(lobby);
                        }
                    }
                }
                catch (IOException ioex)
                {
                    StaticLogger.Warning(ioex);
                }
                catch (Exception ex)
                {
                    StaticLogger.Error(ex);
                }
                current += file.Length;
                currentfile++;
            }

            watch.Stop();

            StaticLogger.Info(string.Format("Finished rebuilding {0} files in {1} seconds", logs.Count, (int)watch.Elapsed.TotalSeconds));
        }

        private void RebuildWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RebuildButton.Text = "Rebuild";
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
    }
}
