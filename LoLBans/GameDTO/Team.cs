using System.Collections.Generic;
using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("Count: {Count}")]
    public class Team : List<Participant>
    {
    }
}