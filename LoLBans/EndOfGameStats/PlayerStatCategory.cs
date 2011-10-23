using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LoLBans
{
    [DebuggerDisplay("{DisplayName}")]
    public class PlayerStatCategory
    {
        protected readonly FlashObject Base;
        public PlayerStatCategory(FlashObject thebase)
        {
            Base = thebase;

            FlashObject.SetFields(this, Base);
        }

        [InternalName("displayName")]
        public string DisplayName { get; protected set; }

        [InternalName("name")]
        public string Name { get; protected set; }

        [InternalName("priority")]
        public int Priority { get; protected set; }

    }
}
