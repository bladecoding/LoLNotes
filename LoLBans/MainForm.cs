using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LoLBans.Controls;
using LoLBans.Properties;
using System.Linq;

namespace LoLBans
{
    public partial class MainForm : Form
    {
        static readonly string LolBansPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lolbans");
        static readonly string LoaderFile = Path.Combine(LolBansPath, "LoLLoader.dll");

        readonly LoLConnection Connection;

        public MainForm()
        {
            InitializeComponent();

            Connection = new LoLConnection("lolbans");
            Connection.ProcessObject += ConnectionProcessObject;
            Connection.Start();
        }

        void ConnectionProcessObject(FlashObject obj)
        {
            var body = obj["body"];
            if (body == null)
                return;
            if (!body.Value.Contains("com.riotgames.platform.gameclient.domain::GameDTO"))
                return;

            var teams = GetTeams(body);

            UpdateLists(teams);
        }

        public void UpdateLists(List<Team> teams)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<List<Team>>(UpdateLists), teams);
                return;
            }

            var boxs = new List<TeamControl> { teamControl1, teamControl2 };

            for (int i = 0; i < boxs.Count && i < teams.Count; i++)
            {

                if (teams[i] == null)
                {
                    foreach (var ply in boxs[i].Players)
                        ply.SummonerName = "Unknown";
                    continue;
                }

                for (int o = 0; o < boxs[i].Players.Count; o++)
                {
                    if (o < teams[i].Players.Count)
                    {
                        boxs[i].Players[o].SummonerName = 
                            string.IsNullOrEmpty(teams[i].Players[o].Name) ?
                            teams[i].Players[o].Name :
                            "Unknown";
                        boxs[i].Players[o].Visible = true;
                    }
                    else
                    {
                        boxs[i].Players[o].Visible = false;
                    }
                }
            }
        }

        public static List<Team> GetTeams(FlashObject body)
        {
            return new List<Team>
            {
                GetTeam(body["teamOne"]),
                GetTeam(body["teamTwo"]),
            };
        }

        public static Team GetTeam(FlashObject team)
        {
            if (team == null)
                return null;

            var ret = new Team();
            var array = team["list"]["source"];
            foreach (var field in array.Fields)
            {
                var ply = new Player();
                int id;
                if (field["summonerId"] != null && int.TryParse(field["summonerId"].Value, out id))
                    ply.Id = id;
                ply.InternalName = field["summonerInternalName"] != null ? field["summonerInternalName"].Value : null;
                ply.Name = field["summonerName"] != null ? field["summonerName"].Value : null;
                ret.Players.Add(ply);
            }
            return ret;
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
