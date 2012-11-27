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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LoLNotes.Assets;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.Statistics;
using LoLNotes.Messages.Summoner;
using LoLNotes.Storage;
using System.Linq;
using LoLNotes.Extensions;
using NotMissing.Logging;

namespace LoLNotes.Gui.Controls
{
	public partial class PlayerControl : UserControl
	{
		public TeamControl Parent { get; set; }
		public PlayerEntry Player { get; set; }

        public string DefaultGameTab { get; set; }

        static protected Dictionary<string, string> LeagueRegions = new Dictionary<string, string>
		{
			{"NA", "na"},
			{"EUW", "euw"},
			{"EUN", "eune"}
		};

		public PlayerControl()
		{
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			InitializeComponent();

			LoadingPicture.Visible = false;
			LevelLabel.Text = "Level: ";
		}
		public PlayerControl(TeamControl parent)
			: this()
		{
			Parent = parent;
		}

		protected override void OnLoad(EventArgs e)
		{
			InfoTabs.ContextMenuStrip = ContextMenuStrip; //Set here because its virtual
			base.OnLoad(e);
		}

		const int BorderSize = 5;
		protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
			base.OnPaint(e);
			var pen = new Pen(Player != null && Player.NoteColor.A != 0 ? Player.NoteColor : Color.Green, BorderSize);
			e.Graphics.DrawRectangle(pen, BorderSize, BorderSize, Width - BorderSize * 2, Height - BorderSize * 2);
		}

		void SetName(string str)
		{
			NameLabel.Links.Clear();
			NameLabel.Text = str;
		}

		public void SetLoading(bool loading)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<bool>(SetLoading), loading);
				return;
			}
            if (loading)
            {
                InfoTabs.TabPages.Clear();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(DefaultGameTab) && InfoTabs.TabPages[DefaultGameTab] != null)
                    InfoTabs.SelectTab(DefaultGameTab);
            }
			LoadingPicture.Visible = loading;
		}

		void SetTitle(PlayerEntry ply)
		{
			SetName(ply.Name);
			NameLabel.Links.Add(0, ply.Name.Length, Tuple.Create(ply.Id, ply.Name));
		}
		void SetTitle(Participant part)
		{
			var opart = part as ObfuscatedParticipant;
			var gpart = part as GameParticipant;
			var ppart = part as PlayerParticipant;
			if (gpart != null)
			{
				SetName(gpart.Name);
			}
			else if (opart != null)
			{
				SetName("Summoner " + opart.GameUniqueId);
			}
			else
			{
				SetName("Unknown");
			}

			if (ppart != null)
			{
				NameLabel.Links.Add(0, ppart.Name.Length, Tuple.Create(ppart.SummonerId, ppart.Name));
			}
		}

		void SetLevel(int level)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetLevel), level);
				return;
			}

			LevelLabel.Text = "Level: " + (level != 0 ? Convert.ToString(level) : "?");
		}

		void RemoveAll(Predicate<TabPage> find)
		{
			for (int i = 0; i < InfoTabs.TabPages.Count; i++)
			{
				if (find(InfoTabs.TabPages[i]))
					InfoTabs.TabPages.RemoveAt(i--);
			}
		}

		public void SetEmpty()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(SetEmpty));
				return;
			}
			Player = null;
			InfoTabs.TabPages.Clear();
			SetLevel(0);
			SetTeam(0);
			SetSeen(0);
			Invalidate(); //Force the border to redraw.
		}

        public void AddTab(TabPage page)
        {
            InfoTabs.TabPages.Add(page);
        }

		public void SetPlayer(PlayerEntry plr)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<PlayerEntry>(SetPlayer), plr);
				return;
			}
			Player = plr;
			SetTitle(plr);

			RemoveAll(p => (p.Tag as string) == "Note");

			if (!string.IsNullOrWhiteSpace(plr.Note))
			{
				SuspendLayout();

				var tab = new TabPage("Note")
				{
					Tag = "Note",
					BackColor = this.BackColor
				};
				var lbl = new Label
				{
					Font = new Font(Font.FontFamily, Font.SizeInPoints, FontStyle.Bold),
					Text = plr.Note
				};
				tab.Controls.Add(lbl);
                AddTab(tab);

				ResumeLayout();
			}

			Invalidate(); //Forces the color change
		}
		public void SetParticipant(Participant part)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<Participant>(SetParticipant), part);
				return;
			}
			Player = null;
			SetTitle(part);
		}

		public void SetStats(PublicSummoner summoner, PlayerLifetimeStats stats)
		{
			if (summoner == null || stats == null)
				return;

			if (InvokeRequired)
			{
				Invoke(new Action<PublicSummoner, PlayerLifetimeStats>(SetStats), summoner, stats);
				return;
			}

			SetLevel(summoner.SummonerLevel);
			RemoveAll(p => (p.Tag as string) == "Stats");

			foreach (var stat in stats.PlayerStatSummaries.PlayerStatSummarySet)
			{
				var sc = new StatsControl { Dock = DockStyle.Fill, Tag = "Stats" };
				sc.SetStatSummary(stat);

				var tab = new TabPage(MinifyStatType(stat.PlayerStatSummaryType))
				{
					BackColor = this.BackColor,
					Tag = "Stats"
				};
				tab.Controls.Add(sc);
                AddTab(tab);
			}
		}

		public static string MinifyStatType(string name)
		{
			if (name == null)
				return null;

			var replacements = new Dictionary<string, string>
			{
				{"Ranked", "R"},
				{"Premade", "P"},
				{"Solo", "S"},
				{"Unranked", "UR"},
			};
			foreach (var kv in replacements)
				name = name.Replace(kv.Key, kv.Value);
			return name;
		}

		public void SetChamps(ChampionStatInfoList champs)
		{
			if (champs == null || champs.Count < 1)
				return;

			if (InvokeRequired)
			{
				Invoke(new Action<ChampionStatInfoList>(SetChamps), champs);
				return;
			}

			RemoveAll(p => (p.Tag as string) == "Champs");

			var layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			foreach (var champ in champs)
			{
				if (champ.ChampionId == 0)
					continue;

				var lbl = new Label
				{
					Font = new Font("Bitstream Vera Sans Mono", 8.25F, FontStyle.Bold),
					AutoSize = true,
					Text = string.Format("{0} ({1})", ChampNames.Get(champ.ChampionId), champ.TotalGamesPlayed)
				};
				layout.Controls.Add(lbl);
			}

			var tab = new TabPage("Champs")
			{
				BackColor = this.BackColor,
				Tag = "Champs"
			};
			tab.Controls.Add(layout);
            AddTab(tab);
		}

		public void SetSeen(int times)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetSeen), times);
				return;
			}

			SeenCountLabel.Visible = times > 0;
			SeenCountLabel.Text = "Seen: " + times;
		}

		public void SetGames(RecentGames games)
		{
			if (games == null || games.GameStatistics.Count < 1)
				return;

			if (InvokeRequired)
			{
				Invoke(new Action<RecentGames>(SetGames), games);
				return;
			}

			RemoveAll(p => (p.Tag as string) == "Recent");

			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Margin = Padding.Empty,
			};

			const int rows = 5;
			const int cols = 2;

			var list = games.GameStatistics.OrderByDescending(p => p.GameId).ToList();
			for (int x = 0; x < cols; x++)
			{
				for (int y = 0; y < rows; y++)
				{
					int idx = y + (x * rows);
					if (idx >= list.Count)
						break;

					var game = list[idx];
					if (game.ChampionId == 0)
						continue;

					var won = game.Statistics.GetInt(RawStat.WIN) != 0;
					var kills = game.Statistics.GetInt(RawStat.CHAMPION_KILLS);
					var deaths = game.Statistics.GetInt(RawStat.DEATHS);
					var assists = game.Statistics.GetInt(RawStat.ASSISTS);
					var left = game.Leaver;
					var botgame = game.QueueType == "BOT";

					var wonlabel = CreateLabel(string.Format("{0}{1}", left ? "[L] " : "", won ? "Won" : "Lost"));
					wonlabel.ForeColor = won ? Color.Green : Color.Red;

					var kdrlbl = CreateLabel(string.Format("({0}/{1}/{2})", kills, deaths, assists));
					kdrlbl.ForeColor = GetKdrColor(kills, deaths);

					var champicon = new PictureBox
					{
						Image = ChampIcons.Get(game.ChampionId),
						Margin = Padding.Empty,
						SizeMode = PictureBoxSizeMode.StretchImage,
						Size = new Size(20, 20)
					};

					if (botgame)
						wonlabel.ForeColor = kdrlbl.ForeColor = Color.Black;


					var controls = new List<Control>
					{
						champicon,
						wonlabel,
						kdrlbl,
						CreateSpellPicture(game.Spell1),
						CreateSpellPicture(game.Spell2)
					};
					//Add a space between the last column in each set.
					controls[controls.Count - 1].Margin = new Padding(0, 0, 5, 0);

					for (int i = 0; i < controls.Count; i++)
					{
						layout.AddControl(controls[i], i + x * controls.Count, y);
					}
				}
			}

			var tab = new TabPage("Recent")
			{
				BackColor = this.BackColor,
				Tag = "Recent"
			};
			tab.Controls.Add(layout);
            AddTab(tab);
		}

		static Label CreateLabel(string text)
		{
			var label = new Label
			{
				Font = new Font("Bitstream Vera Sans Mono", 8.25F, FontStyle.Bold),
				Text = text,
				TextAlign = ContentAlignment.MiddleLeft,
				Margin = Padding.Empty,
				AutoSize = false
			};
			label.Width = label.PreferredWidth;
			label.Height = 20;
			return label;
		}

		static PictureBox CreateSpellPicture(int id)
		{
			return new PictureBox
			{
				Image = SpellIcons.Get(id),
				Margin = Padding.Empty,
				SizeMode = PictureBoxSizeMode.StretchImage,
				Size = new Size(20, 20)
			};
		}

		static Color GetKdrColor(int kills, int deaths)
		{
			if (deaths == 0)
				deaths = 1;

			double ratio = (double)kills / deaths;
			ratio = Math.Min(ratio, 1);

			var orange = Color.Orange;
			var red = Color.Red;
			var green = Color.Green;

			Color top;
			Color bot;
			if (ratio < 0.5d)
			{
				top = red;
				bot = orange;
				ratio *= 2;
			}
			else
			{
				top = orange;
				bot = green;
				ratio -= 0.5d;
				ratio *= 2;
			}
			return Interpolate(top, bot, ratio);
		}

		static byte Interpolate(byte from, byte to, double step)
		{
			return (byte)(from + (to - from) * step);

		}
		static Color Interpolate(Color from, Color to, double step)
		{
			return Color.FromArgb(
				Interpolate(from.A, to.A, step),
				Interpolate(from.R, to.R, step),
				Interpolate(from.G, to.G, step),
				Interpolate(from.B, to.B, step)
			);
		}

		static void kdrtest()
		{
			for (var i = 0; i < 10; i++)
			{
				var color = GetKdrColor(i, 10);
				Debug.WriteLine(string.Format("Color.FromArgb({0}, {1}, {2}, {3})", color.A, color.R, color.G, color.B));
			}
		}

		public void SetTeam(int num)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetTeam), num);
				return;
			}

			if (num == 0)
			{
				TeamLabel.Text = "Solo";
			}
			else
			{

				TeamLabel.Text = string.Format("Team {0}", num);
			}
		}

		private void NameLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData == null)
				return;
			var plr = (Tuple<Int64, string>)e.Link.LinkData;
			string region;
			if (!LeagueRegions.TryGetValue(MainSettings.Instance.Region, out region))
			{
				StaticLogger.Info("Region " + MainSettings.Instance.Region + " is not supported");
				return;
			}

			string url = null;
			if (e.Button == MouseButtons.Left)
			{
				url = string.Format("http://www.lolking.net/summoner/{0}/{1}", region, plr.Item1);
			}
			else if (e.Button == MouseButtons.Middle)
			{
				url = string.Format("http://bladecoding.com/lolnotes/leagueofstats.php?name={0}", plr.Item2);
			}

			if (url != null)
			{
				Process.Start(url);
				e.Link.Visited = true;
			}
		}
	}
}
