using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("{Name}")]
    public class Player
    {
        public int Id { get; set; }
        public string InternalName { get; set; }
        public string Name { get; set; }
    }
}