using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans
{
    public class PlayerParticipantStatsSummaryList : List<PlayerParticipantStatsSummary>
    {   
        protected readonly FlashObject Base;
        public PlayerParticipantStatsSummaryList(FlashObject thebase)
        {
            Base = thebase;

            if (Base == null)
                return;

            var array = Base["list"]["source"];
            foreach (var field in array.Fields)
            {
                Add(new PlayerParticipantStatsSummary(field));
            }
        }
    }
}
