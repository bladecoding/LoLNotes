using System.Collections.Generic;
using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("Count: {Players.Count}")]
    public class Team
    {
        public List<Player> Players { get; set; }
        public Team()
        {
            Players = new List<Player>();
        }
    }
}