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
		public PlayerEntry Player { get; set; }

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

		protected override void OnLoad(EventArgs e)
		{
			InfoTabs.ContextMenuStrip = ContextMenuStrip; //Set here because its virtual
			base.OnLoad(e);
		}

		const int BorderSize = 5;
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var pen = new Pen(Player != null && Player.NoteColor.A != 0 ? Player.NoteColor : Color.Green, BorderSize);
			e.Graphics.DrawRectangle(pen, BorderSize, BorderSize, Width - BorderSize * 2, Height - BorderSize * 2);
		}

		void SetName(string str)
		{
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
			LoadingPicture.Visible = loading;
		}

		void SetTitle(PlayerEntry ply)
		{
			SetName(ply.Name);
		}
		void SetTitle(Participant part)
		{
			var opart = part as ObfuscatedParticipant;
			var gpart = part as GameParticipant;
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
		}

		void SetLevel(int level)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetLevel), level);
				return;
			}

			LevelLabel.Text = "Level: " + level;
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

			RemoveAll(t => (t.Tag as string) == "Note");

			if (string.IsNullOrWhiteSpace(plr.Note))
				return;

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
			InfoTabs.TabPages.Add(tab);

			ResumeLayout();

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
			if (InvokeRequired)
			{
				Invoke(new Action<PublicSummoner, PlayerLifetimeStats>(SetStats), summoner, stats);
				return;
			}

			RemoveAll(t => (t.Tag as string) == "Stats");

			if (summoner == null || stats == null)
				return;

			SuspendLayout();

			SetLevel(summoner.SummonerLevel);

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

				InfoTabs.TabPages.Add(tab);
			}

			ResumeLayout();
		}

		static string MinifyStatType(string name)
		{
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
			if (InvokeRequired)
			{
				Invoke(new Action<ChampionStatInfoList>(SetChamps), champs);
				return;
			}

			RemoveAll(t => (t.Tag as string) == "Champs");

			if (champs == null)
				return;

			if (champs.Count < 1)
				return;

			SuspendLayout();

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
			InfoTabs.TabPages.Add(tab);

			ResumeLayout();
		}
		public void SetGames(RecentGames games)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<RecentGames>(SetGames), games);
				return;
			}

			RemoveAll(t => (t.Tag as string) == "Recent");

			if (games.GameStatistics.Count < 1)
				return;

			SuspendLayout();

			var layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;

			int rows = Math.Max(1, games.GameStatistics.Count / 2);
			int cols = 2;

			while (layout.RowStyles.Count < rows)
				layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			while (layout.ColumnStyles.Count < cols)
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

			layout.RowCount = rows;
			layout.ColumnCount = cols;

			var list = games.GameStatistics.OrderByDescending(p => p.GameId).ToList();
			for (int x = 0; x < cols; x++)
			{
				for (int y = 0; y < rows;  y++)
				{
					int idx = y + (x * rows);
					if (idx >= list.Count)
						break;

					var game = list[idx];
					if (game.ChampionId == 0)
						continue;

					var champ = ChampNames.Get(game.ChampionId);
					var won = game.Statistics.GetInt(RawStat.WIN) != 0;
					var kills = game.Statistics.GetInt(RawStat.CHAMPION_KILLS);
					var deaths = game.Statistics.GetInt(RawStat.DEATHS);
					var assists = game.Statistics.GetInt(RawStat.ASSISTS);

					var lbl = new Label
					{
						Font = new Font("Bitstream Vera Sans Mono", 8.25F, FontStyle.Bold),
						AutoSize = true,
						Text = string.Format(
							"[{0}] {1} ({2}/{3}/{4}){5}",
							won ? "W" : "L",
							champ,
							kills,
							deaths,
							assists,
							game.QueueType == "BOT" ? " (B)" : ""
						)
					};
					layout.Controls.Add(lbl, x, y);
				}
			}

			var tab = new TabPage("Recent")
			{
				BackColor = this.BackColor,
				Tag = "Recent"
			};
			tab.Controls.Add(layout);
			InfoTabs.TabPages.Add(tab);

			ResumeLayout();
		}
	}
}
