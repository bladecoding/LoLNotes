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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LoLNotes.GameLobby.Participants;
using LoLNotes.GameStats.PlayerStats;

namespace LoLNotes
{
    public partial class PlayerControl : UserControl
    {

        public PlayerControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            InitializeComponent();

            SetDescription("");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawRectangle(new Pen(Color.Green, 5), 0, 0, Width, Height);
        }

        void SetSummonerName(string str)
        {
            if (NameLabel.InvokeRequired)
            {
                NameLabel.Invoke(new Action<string>(SetSummonerName), str);
                return;
            }
            NameLabel.Text = str;
        }


        void SetDescription(string str)
        {
            if (DescLabel.InvokeRequired)
            {
                DescLabel.Invoke(new Action<string>(SetDescription), str);
                return;
            }
            DescLabel.Text = str;
        }

        const string DescriptionFormat = "Level: {0}\nWins: {1}\nLosses: {2}\nLeaves: {3}";
        void SetDescription(int level, int wins, int losses, int leaves)
        {
            SetDescription(string.Format(DescriptionFormat, level, wins, losses, leaves));
        }

        public void SetData(PlayerStatsSummary stats)
        {
            SetSummonerName(stats.SummonerName);
            SetDescription(stats.Level, stats.Wins, stats.Losses, stats.Leaves);
        }
        public void SetData(Participant part)
        {
            var opart = part as ObfuscatedParticipant;
            var gpart = part as GameParticipant;
            if (gpart != null)
            {
                SetSummonerName(gpart.Name);
                SetDescription(gpart is PlayerParticipant ? "No stats found." : "");
            }
            else if (opart != null)
            {
                SetSummonerName("Summoner " + opart.GameUniqueId);
                SetDescription("");
            }
            else
            {
                SetSummonerName("Unknown");
                SetDescription("");
            }
        }
    }
}
