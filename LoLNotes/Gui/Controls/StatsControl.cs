using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LoLNotes.Messages.Statistics;

namespace LoLNotes.Gui.Controls
{
	public partial class StatsControl : UserControl
	{
		protected readonly Dictionary<Control, string> Prepends = new Dictionary<Control, string>();
		public StatsControl()
		{
			InitializeComponent();

			foreach (Control control in flowLayoutPanel1.Controls)
				Prepends.Add(control, control.Text);
		}

		public void SetStatSummary(PlayerStatSummary stat)
		{
			var values = new Dictionary<Control, string>()
			{
				{GameType, stat.PlayerStatSummaryType},
				{MaxElo, stat.MaxRating.ToString()},	
				{Elo, stat.Rating.ToString()},
				{Wins, stat.Wins.ToString()},
				{Losses, stat.Losses.ToString()},
				{Leaves, stat.Leaves.ToString()}
			};

			foreach (var label in values)
			{
				label.Key.Text = Prepends[label.Key] + label.Value;
			}
		}
	}
}
