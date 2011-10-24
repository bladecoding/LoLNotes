using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LoLBans.Controls;
using LoLBans.Properties;
using System.Linq;
using LoLBans.Readers;

namespace LoLBans
{
    public partial class MainForm : Form
    {
        static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
        static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");

        readonly LoLConnection Connection;
        readonly GameDTOReader GameReader;

        public MainForm()
        {
            InitializeComponent();

            Connection = new LoLConnection("lolbans");
            GameReader = new GameDTOReader(Connection);

            GameReader.ObjectRead += GameReader_OnGameDTO;

            Connection.Start();
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
