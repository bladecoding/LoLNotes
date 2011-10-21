using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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

            var boxs = new List<ListBox>();

            for (int i = 0; i < boxs.Count && i < teams.Count; i++)
            {
                boxs[i].Items.Clear();
                if (teams[i] == null)
                    continue;
                foreach (var ply in teams[i].Players)
                {
                    boxs[i].Items.Add(ply.Name ?? "Unknown");
                }
            }
        }

        public static List<Team> GetTeams(FlashObject body)
        {
            var ret = new List<Team>
            {
                GetTeam(body["teamOne"]),
                GetTeam(body["teamTwo"]),
            };
            return ret.Where(t => t != null).ToList();
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
    }
}
