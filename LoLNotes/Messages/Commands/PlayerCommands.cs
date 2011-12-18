/*
copyright (C) 2011 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Linq;
using FluorineFx;
using FluorineFx.Messaging.Messages;
using LoLNotes.Messages.Statistics;
using LoLNotes.Messages.Summoner;
using LoLNotes.Messages.Translators;
using LoLNotes.Proxy;
using LoLNotes.Util;
using NotMissing.Logging;

namespace LoLNotes.Messages.Commands
{
	public class PlayerCommands
	{
		public RtmpsProxyHost Host { get; protected set; }
		public PlayerCommands(RtmpsProxyHost host)
		{
			Host = host;
		}

		public PublicSummoner GetPlayerByName(string name)
		{
			var msg = new RemotingMessage();
			msg.operation = "getSummonerByName";
			msg.destination = "summonerService";
			msg.headers["DSRequestTimeout"] = 60;
			msg.headers["DSId"] = RtmpUtil.RandomUidString();
			msg.headers["DSEndpoint"] = "my-rtmps";
			msg.body = new object[] { name };
			msg.messageId = RtmpUtil.RandomUidString();

			var result = Host.Call(msg);
			if (result == null)
			{
				StaticLogger.Warning("GetPlayerByName Host.Call returned null");
				return null;
			}

			var body = RtmpUtil.GetBodies(result).FirstOrDefault();
			if (body == null)
			{
				StaticLogger.Debug("GetPlayerByName RtmpUtil.GetBodies returned null");
				return null;
			}

			var ao = body.Item1 as ASObject;
			if (ao == null)
			{
				StaticLogger.Debug("GetPlayerByName expected ASObject, got " + body.Item1.GetType());
				return null;
			}

			var summoner = MessageTranslator.Instance.GetObject<PublicSummoner>(ao);
			if (summoner == null)
			{
				StaticLogger.Debug("GetPlayerByName expected PublicSummoner, got " + ao.TypeName);
				return null;
			}

			summoner.TimeStamp = body.Item2;
			return summoner;
		}

		public PlayerLifetimeStats RetrievePlayerStatsByAccountId(int id)
		{
			var msg = new RemotingMessage();
			msg.operation = "retrievePlayerStatsByAccountId";
			msg.destination = "playerStatsService";
			msg.headers["DSRequestTimeout"] = 60;
			msg.headers["DSId"] = RtmpUtil.RandomUidString();
			msg.headers["DSEndpoint"] = "my-rtmps";
			msg.body = new object[] { id, "CURRENT" };
			msg.messageId = RtmpUtil.RandomUidString();

			var result = Host.Call(msg);
			if (result == null)
			{
				StaticLogger.Warning("RetrievePlayerStatsByAccountId Host.Call returned null");
				return null;
			}

			var body = RtmpUtil.GetBodies(result).FirstOrDefault();
			if (body == null)
			{
				StaticLogger.Debug("RetrievePlayerStatsByAccountId RtmpUtil.GetBodies returned null");
				return null;
			}

			var ao = body.Item1 as ASObject;
			if (ao == null)
			{
				StaticLogger.Debug("RetrievePlayerStatsByAccountId expected ASObject, got " + body.Item1.GetType());
				return null;
			}

			var stats = MessageTranslator.Instance.GetObject<PlayerLifetimeStats>(ao);
			if (stats == null)
			{
				StaticLogger.Debug("RetrievePlayerStatsByAccountId expected PlayerLifetimeStats, got " + ao.TypeName);
				return null;
			}

			stats.TimeStamp = body.Item2;
			return stats;
		}
	}
}
