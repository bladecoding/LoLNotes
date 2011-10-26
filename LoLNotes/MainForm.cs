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
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using LoLNotes.Controls;
using LoLNotes.GameLobby;
using LoLNotes.GameLobby.Participants;
using LoLNotes.GameStats;
using LoLNotes.Properties;
using LoLNotes.Readers;
using LoLNotes.Util;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace LoLNotes
{
    public partial class MainForm : Form
    {
        static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
        static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");

        readonly Dictionary<string, Icon> IconCache;
        readonly LoLConnection Connection;
        readonly GameLobbyReader LobbyReader;
        readonly GameStatsReader StatsReader;
        readonly DocumentStore Store;
        readonly GameRecorder Recorder;



        public class Person
        {
            public string Name;
            public int Age;
        }

        public MainForm()
        {
            InitializeComponent();

            IconCache = new Dictionary<string, Icon>
            {
                {"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
                {"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
                {"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
            };

            Icon = IsInstalled ? IconCache["Yellow"] : IconCache["Red"];


            Store = new EmbeddableDocumentStore 
            {
                DataDirectory = "data"
            };
            Store.Initialize();

            Connection = new LoLConnection("lolbans");
            LobbyReader = new GameLobbyReader(Connection);
            StatsReader = new GameStatsReader(Connection);

            Connection.Connected += Connection_Connected;
            LobbyReader.ObjectRead += GameReader_OnGameDTO;

            Recorder = new GameRecorder(Store, Connection);

            //Pipe server for testing EndOfGameStats.

            var pipe = new NamedPipeServerStream("lolbans", PipeDirection.InOut, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            pipe.BeginWaitForConnection(delegate(IAsyncResult ar)
            {
                pipe.EndWaitForConnection(ar);
                var bytes = File.ReadAllBytes("ExampleData\\ExampleEndOfGameStats.txt");
                pipe.Write(bytes, 0, bytes.Length);
            }, pipe);


            Connection.Start();
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
            UpdateLists(new List<TeamParticipants> { game.TeamOne, game.TeamTwo });
        }

        public void UpdateLists(List<TeamParticipants> teams)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<List<TeamParticipants>>(UpdateLists), teams);
                return;
            }

            var lists = new List<TeamControl> { teamControl1, teamControl2 };

            for (int i = 0; i < lists.Count; i++)
            {
                if (teams[i] == null)
                {
                    lists[i].Visible = false;
                    continue;
                }

                for (int o = 0; o < lists[i].Players.Count; o++)
                {
                    if (o < teams[i].Count)
                    {
                        if (teams[i][o] is ObfuscatedParticipant)
                        {
                            lists[i].Players[o].SummonerName = string.Format(
                                "Summoner {0}",
                                ((ObfuscatedParticipant)teams[i][o]).GameUniqueId
                            );
                        }
                        else if (teams[i][o] is GameParticipant)
                        {
                            lists[i].Players[o].SummonerName = ((GameParticipant)teams[i][o]).Name;
                        }
                        else
                        {
                            lists[i].Players[o].SummonerName = "Unknown";
                        }
                        lists[i].Players[o].Visible = true;
                    }
                    else
                    {
                        lists[i].Players[o].Visible = false;
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

        private void GameTab_Click(object sender, EventArgs e)
        {

        }
    }
}
