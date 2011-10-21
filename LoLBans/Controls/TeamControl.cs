using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LoLBans.Controls
{
    public partial class TeamControl : UserControl
    {
        /// <summary>
        /// This control is used for sizing and such.
        /// </summary>
        protected PlayerControl BasePlayer;

        const int PlayersStartY = 36;
        const int PlayersYSpacing = 10;

        protected int teamsize;
        public int TeamSize
        {
            get
            {
                return teamsize;
            }
            set
            {
                teamsize = value;
                Width = BasePlayer.Width;
                Height = PlayersStartY + value * (BasePlayer.Height + PlayersYSpacing);

                if (Players != null)
                {
                    foreach (PlayerControl p in Players)
                        p.Dispose();
                }

                Players = new List<PlayerControl>();
                for (int i = 0; i < value; i++)
                {
                    var control = new PlayerControl();
                    control.Location = new Point(0, PlayersStartY + (BasePlayer.Height + PlayersYSpacing) * i);
                    Players.Add(control);
                    Controls.Add(control);
                }

            }
        }

        public List<PlayerControl> Players = new List<PlayerControl>();

        public TeamControl()
        {
            BasePlayer = new PlayerControl();
            TeamSize = 5;
            InitializeComponent();

        }


        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text
        {
            get
            {
                return NameLabel.Text;
            }
            set
            {
                NameLabel.Text = value;
            }
        }
    }
}
