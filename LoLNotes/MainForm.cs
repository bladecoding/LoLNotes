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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using LoLNotes.Controls;
using LoLNotes.Flash;
using LoLNotes.GameLobby;
using LoLNotes.GameLobby.Participants;
using LoLNotes.GameStats;
using LoLNotes.Properties;
using LoLNotes.Util;
using Db4objects.Db4o;
using NotMissing.Logging;

namespace LoLNotes
{
    public partial class MainForm : Form
    {
        static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
        static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");

        readonly Dictionary<string, Icon> IconCache;
        readonly PipeProcessor Connection;
        readonly GameLobbyReader LobbyReader;
        readonly GameStatsReader StatsReader;
        readonly IObjectContainer Database;
        readonly GameRecorder Recorder;

        public MainForm()
        {
            InitializeComponent();

            Logger.Instance.Register(new DefaultListener(Levels.All, OnLog));

            IconCache = new Dictionary<string, Icon>
            {
                {"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
                {"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
                {"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
            };

            Icon = IsInstalled ? IconCache["Yellow"] : IconCache["Red"];

            //TODO: Find a better way than this.
            var config = Db4oEmbedded.NewConfiguration();
            config.Common.ObjectClass(typeof(PlayerEntry)).CascadeOnUpdate(true);
            config.Common.ObjectClass(typeof(PlayerEntry)).CascadeOnActivate(true);
            config.Common.ObjectClass(typeof(PlayerEntry)).CascadeOnDelete(true);
            config.Common.ObjectClass(typeof(StatsEntry)).CascadeOnUpdate(true);
            config.Common.ObjectClass(typeof(StatsEntry)).CascadeOnActivate(true);
            config.Common.ObjectClass(typeof(StatsEntry)).CascadeOnDelete(true);

            Database = Db4oEmbedded.OpenFile(config, "db.yap");

            Connection = new PipeProcessor("lolbans");
            LobbyReader = new GameLobbyReader(Connection);
            StatsReader = new GameStatsReader(Connection);
            Recorder = new GameRecorder(Database, Connection);

            Connection.Connected += Connection_Connected;
            LobbyReader.ObjectRead += GameReader_OnGameDTO;

            //Pipe server for testing EndOfGameStats/GameDTO.

            //var pipe = new NamedPipeServerStream("lolbans", PipeDirection.InOut, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            //pipe.BeginWaitForConnection(delegate(IAsyncResult ar)
            //{
            //    pipe.EndWaitForConnection(ar);
            //    var bytes = File.ReadAllBytes("ExampleData\\ExampleEndOfGameStats.txt");
            //    pipe.Write(bytes, 0, bytes.Length);
            //    bytes = File.ReadAllBytes("ExampleData\\ExampleGameDTO.txt");
            //    pipe.Write(bytes, 0, bytes.Length);
            //}, pipe);

            Connection.Start();

            StaticLogger.Info("Startup Completed");
        }

        void OnLog(Levels level, object obj)
        {
            object log = string.Format("[{0}] {1} ({2:MM/dd/yyyy HH:mm:ss.fff})", level.ToString().ToUpper(), obj, DateTime.UtcNow);
            Debug.WriteLine(log);
            Task.Factory.StartNew(LogToFile, log);
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

        void Connection_Connected(object obj)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object>(Connection_Connected), obj);
                return;
            }
            Icon = Connection.IsConnected ? IconCache["Green"] : IconCache["Yellow"];
        }

        void GameReader_OnGameDTO(GameDTO game)
        {
            UpdateLists(game, new List<TeamParticipants> { game.TeamOne, game.TeamTwo });
        }

        public GameDTO CurrentGame = null;
        public List<PlayerEntry> PlayerCache = new List<PlayerEntry>();

        public void UpdateLists(GameDTO game, List<TeamParticipants> teams)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<GameDTO, List<TeamParticipants>>(UpdateLists), game, teams);
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
                var curteam = CurrentGame.TeamOne.Union(CurrentGame.TeamTwo).ToList();
                var newteam = game.TeamOne.Union(game.TeamTwo).ToList();
                if (curteam.Count == newteam.Count && newteam.All(curteam.Contains))
                    return;
            }

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
                                Stopwatch sw = Stopwatch.StartNew();

                                entry = Database.Query<PlayerEntry>().
                                    Where(e => e.Id == ply.Id).
                                    OrderByDescending(e => e.TimeStamp).
                                    FirstOrDefault();

                                if (entry != null)
                                    PlayerCache.Add(entry);

                                sw.Stop();
                                StaticLogger.Trace(string.Format("Player query in {0}ms", sw.ElapsedMilliseconds));
                            }

                            if (entry != null)
                            {
                                list.Players[o].SetData(game, entry);
                                list.Players[o].Visible = true;
                                continue;
                            }
                        }
                        list.Players[o].SetData(game, team[o]);
                        list.Players[o].Visible = true;
                    }
                    else
                    {
                        list.Players[o].Visible = false;
                    }
                }
            }
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
            if (!IsInstalled && !Wow.IsAdministrator)
                MessageBox.Show("You must run LoLBans as admin to install it");

            //Install();
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
            Icon = IsInstalled ? IconCache["Yellow"] : IconCache["Red"];
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

            string search = "rads";
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
                StaticLogger.Info(string.Format("Rebuild {0}/{1} ({2}%)",
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

                                var stats = new GameStatsReader().GetObject(flashobj);
                                var lobby = new GameLobbyReader().GetObject(flashobj);

                                if (stats != null)
                                    Recorder.RecordGame(stats);
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
                                Recorder.RecordLobby(lobby);
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
    }
}
