using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Flash;

namespace LoLNotes.Messages.Summoner
{
    [Message(".SummonerLeaguesDTO")]
    public class SummonerLeaguesDTO : MessageObject
    {
        public SummonerLeaguesDTO(ASObject obj)
            : base(obj)
        {
            BaseObject.SetFields(this, obj);
        }

        [InternalName("summonerLeagues")]
        public ArrayCollection SummonerLeagues { get; set; }
    }
}
