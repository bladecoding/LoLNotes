using System;
using System.Windows.Forms;

namespace LoLBans.Controls
{
    public partial class TeamControl : UserControl
    {
        /// <summary>
        /// This control is used for sizing and such.
        /// </summary>
        protected PlayerControl BasePlayer;

        protected int teamsize;
        public int TeamSize 
        {
            get { return teamsize; }
            set { teamsize = value; Height = value * BasePlayer.Height; }
        }

        public TeamControl()
        {
            BasePlayer = new PlayerControl();
            TeamSize = 6;
            InitializeComponent();
        }
    }
}
