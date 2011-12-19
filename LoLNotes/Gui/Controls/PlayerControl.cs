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
using System.Drawing;
using System.Windows.Forms;
using LoLNotes.Assets;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.Statistics;
using LoLNotes.Messages.Summoner;
using LoLNotes.Storage;
using System.Linq;
using NotMissing.Logging;

namespace LoLNotes.Gui.Controls
{
    public partial class PlayerControl : UserControl
    {
        PlayerEntry player;
        public PlayerEntry Player
        {
            get { return player; }
            protected set { player = value; participant = null; }
        }

        Participant participant;
        /// <summary>
        /// Overrides Player.Name, used for "Summoner x"
        /// </summary>
        public Participant Participant
        {
            get { return participant; }
            protected set { participant = value; player = null; }
        }

        int Current;

        public bool Loading { get; set; }


        public PlayerControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();

            foreach (Control c in Controls)
                c.Click += c_Click;
        }

        void c_Click(object sender, EventArgs e)
        {
            Current++;
            UpdateView();
        }

        const int BorderSize = 5;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var pen = new Pen(Player != null && Player.NoteColor.A != 0 ? Player.NoteColor : Color.Green, BorderSize);
            e.Graphics.DrawRectangle(pen, BorderSize, BorderSize, Width - BorderSize * 2, Height - BorderSize * 2);
        }

        void SetTitle(string str)
        {
            if (NameLabel.InvokeRequired)
            {
                NameLabel.BeginInvoke(new Action<string>(SetTitle), str);
                return;
            }
            NameLabel.Text = str;
        }

        void SetDescription(string str)
        {
            if (DescLabel.InvokeRequired)
            {
                DescLabel.BeginInvoke(new Action<string>(SetDescription), str);
                return;
            }
            DescLabel.Text = str;
        }

        void SetVisible(bool visible)
        {
            if (DescLabel.InvokeRequired)
            {
                DescLabel.BeginInvoke(new Action<bool>(SetVisible), visible);
                return;
            }
            Visible = visible;
        }

        void SetTitle(Participant part)
        {
            var opart = part as ObfuscatedParticipant;
            var gpart = part as GameParticipant;
            if (gpart != null)
            {
                SetTitle(gpart.Name);
            }
            else if (opart != null)
            {
                SetTitle("Summoner " + opart.GameUniqueId);
                Loading = false;
            }
            else
            {
                SetTitle("Unknown");
                Loading = false;
            }
        }

        public void SetData()
        {
            Player = null;
            Participant = null;
            SetVisible(false);
        }

        public void SetData(PlayerEntry plr)
        {
            Player = plr;
            Loading = false;
            UpdateView();
            SetVisible(true);
        }
        public void SetData(Participant part)
        {
            Participant = part;
            UpdateView();
            SetVisible(true);
        }

		public void SetData(PublicSummoner ply, PlayerLifetimeStats stats, PlayerStatSummary odin)
		{
			Loading = false;

			SetTitle(ply.Name);

			SetDescription(string.Format(
				"{0}\nLevel: {1}\nWins: {2}\nLosses: {3}\nLeaves: {4}",
				odin.PlayerStatSummaryType,
				ply.SummonerLevel,
				odin.Wins,
				odin.Losses,
				odin.Leaves
			));
			SetVisible(true);
		}

        bool SetStats()
        {
            if (Participant != null)
                SetTitle(Participant);

            if (Player == null)
                return false;

            SetTitle(Player.Name);

			if (Player.StatsList.Count < 1)
				return false;

            var stat = Player.StatsList[Current % Player.StatsList.Count];
            if (stat == null || stat.Summary == null)
                return false;

            GameModes mode;
            if (!Enum.TryParse(stat.GameMode, out mode))
                mode = GameModes.UNKNOWN;

            GameTypes type;
            if (!Enum.TryParse(stat.GameType, out type))
                type = GameTypes.UNKNOWN;

            if (mode == GameModes.UNKNOWN)
                StaticLogger.Error("Unknown GameMode " + stat.GameMode);
            if (type == GameTypes.UNKNOWN)
                StaticLogger.Error("Unknown GameType " + stat.GameType);

            SetDescription(string.Format(
                "{0} {1}\nLevel: {2}\nWins: {3}\nLosses: {4}\nLeaves: {5}\n{6}",
                new DescriptionEnumTypeConverter<GameModes>().ConvertToString(mode),
                new DescriptionEnumTypeConverter<GameTypes>().ConvertToString(type),
                stat.Summary.Level,
                stat.Summary.Wins,
                stat.Summary.Losses,
                stat.Summary.Leaves,
                !string.IsNullOrEmpty(Player.Note) ? string.Format("Note: {0}\n", Player.Note) : ""
            ));

            return true;
        }

        public void SetNoStats()
        {
            if (Loading)
            {
                SetDescription("Loading...");
                return;
            }

            SetDescription(string.Format(
                "No Stats\n{0}",
                (Player != null && !string.IsNullOrEmpty(Player.Note)) ? string.Format("Note: {0}\n", Player.Note) : ""));
        }

        public void UpdateView()
        {
            if (!SetStats())
                SetNoStats();

            Invalidate();
        }
    }
}
