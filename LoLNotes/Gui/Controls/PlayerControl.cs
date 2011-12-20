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
		public string Name { get; set; }
		public PlayerEntry Player { get; set; }
		public PlayerLifetimeStats LifetimeStats { get; set; }

		int CurrentPage;

		public PlayerControl()
		{
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			InitializeComponent();

			Stats.Visible = false;
			LoadingPicture.Visible = false;
			NotesLabel.Visible = false;
			LevelLabel.Text = "Level: ";
		}

		protected override void OnLoad(EventArgs e)
		{
			Stats.ContextMenuStrip = ContextMenuStrip;
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
				foreach (Control c in InfoPanel.Controls)
					c.Visible = false;
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

		public void SetEmpty()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(SetEmpty));
				return;
			}
			Player = null;
			LifetimeStats = null;
			UpdateView();
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
			UpdateView();
		}
		public void SetParticipant(Participant part)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<Participant>(SetParticipant), part);
				return;
			}
			LifetimeStats = null;
			SetTitle(part);
		}

		public void SetStats(PublicSummoner summoner, PlayerLifetimeStats stats)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<PublicSummoner, PlayerLifetimeStats>(SetStats), summoner, stats);
				return;
			}

			LifetimeStats = stats;

			if (Player == null || string.IsNullOrEmpty(Player.Note))
				CurrentPage = 1; //Skip the first page if no note is set.

			SetLevel(summoner.SummonerLevel);

			UpdateView();
		}

		void UpdateView()
		{

			int count = LifetimeStats != null ? LifetimeStats.PlayerStatSummaries.PlayerStatSummarySet.Count : 0;
			int page = count == 0 ? 0 : Math.Abs(CurrentPage) % (count + 1);

			if (page == 0 || LifetimeStats == null)
			{
				NotesLabel.Text = Player != null && !string.IsNullOrEmpty(Player.Note) ? Player.Note : "No note";

				Stats.Visible = false;
				NotesLabel.Visible = true;
			}
			else
			{
				Stats.SetStatSummary(LifetimeStats.PlayerStatSummaries.PlayerStatSummarySet[page - 1]);

				Stats.Visible = true;
				NotesLabel.Visible = false;
			}

			PageLabel.Text = string.Format("{0} / {1}", page + 1, count + 1);

			Invalidate();
		}

		private void LeftArrow_MouseDown(object sender, MouseEventArgs e)
		{
			CurrentPage--;
			UpdateView();
		}

		private void RightArrow_MouseDown(object sender, MouseEventArgs e)
		{
			CurrentPage++;
			UpdateView();
		}
	}
}
