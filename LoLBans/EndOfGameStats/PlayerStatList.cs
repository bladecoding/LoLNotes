using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans
{
    public class PlayerStatList : List<PlayerStat>
    {
        protected readonly FlashObject Base;
        public PlayerStatList(FlashObject thebase)
        {
            Base = thebase;

            if (Base == null)
                return;

            var array = Base["list"]["source"];
            foreach (var field in array.Fields)
            {
                Add(new PlayerStat(field));
            }
        }
    }
}
