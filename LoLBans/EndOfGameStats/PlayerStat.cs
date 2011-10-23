using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LoLBans
{
    [DebuggerDisplay("{DisplayName}")]
    public class PlayerStat
    {
        protected readonly FlashObject Base;
        public PlayerStat(FlashObject thebase)
        {
            Base = thebase;

            FlashObject.SetFields(this, Base);
        }

        [InternalName("displayName")]
        public string DisplayName { get; protected set; }

        [InternalName("priority")]
        public int Priority { get; protected set; }

        [InternalName("statCategory")]
        public PlayerStatCategory Category { get; protected set; }

        [InternalName("statTypeName")]
        public string StatTypeName { get; protected set; }

        [InternalName("value")]
        public int Value { get; protected set; }
    }
}
