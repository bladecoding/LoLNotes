using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LoLBans
{
    [DebuggerDisplay("Count: {Count}")]
    public class Team : List<Participant>
    {
        protected readonly FlashObject Base;
        public Team(FlashObject thebase)
        {
            Base = thebase;

            SetupTeam();
        }

        protected void SetupTeam()
        {
            if (Base == null)
                return;

            var array = Base["list"]["source"];

            foreach (var field in array.Fields)
            {
                if (field.Value.Contains("PlayerParticipant"))
                {
                    Add(new PlayerParticipant(field));
                }
                else if (field.Value.Contains("ObfuscatedParticipant"))
                {
                    Add(new ObfuscatedParticipant(field));
                }
                else if (field.Value.Contains("BotParticipant"))
                {
                    Add(new BotParticipant(field));
                }
                else
                {
                    throw new NotSupportedException("Unexcepted type in team array " + field.Value);
                }
            }
        }
    }
}