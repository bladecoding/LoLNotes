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

        public Dictionary<string, object> GetQueueByName(string queueName)
        {
			foreach (Dictionary<string, object> queueInfo in this.SummonerLeagues.List)
			{
				if (queueInfo["queue"].ToString() == queueName)
				{
					return queueInfo;
				}
			}

			return null;
        }

		public static string GetRanking(Dictionary<string, object> queueInfo)
		{
			if (queueInfo == null)
			{
				return "Unranked";
			}
			return String.Format("{0}: {1}", queueInfo["tier"].ToString(), queueInfo["requestorsRank"].ToString());
		}
    }
}
