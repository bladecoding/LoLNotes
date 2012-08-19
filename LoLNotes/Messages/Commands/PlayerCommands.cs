/*
copyright (C) 2011-2012 by high828@gmail.com

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
using System;
using System.Linq;
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;
using LoLNotes.Flash;
using LoLNotes.Messages.GameLobby;
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
		/// <summary>
		/// Used to invoke a service which you don't know what it returns.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="operation"></param>
		/// <param name="args"></param>
		/// <returns>ASObject body</returns>
		public object InvokeServiceUnknown(string service, string operation, params object[] args)
		{
			var msg = new RemotingMessage();
			msg.operation = operation;
			msg.destination = service;
			msg.headers["DSRequestTimeout"] = 60;
			msg.headers["DSId"] = RtmpUtil.RandomUidString();
			msg.headers["DSEndpoint"] = "my-rtmps";
			msg.body = args;
			msg.messageId = RtmpUtil.RandomUidString();

			string endpoint = service + "." + operation;

			var result = Host.Call(msg);
			if (result == null)
			{
				StaticLogger.Warning(string.Format("Invoking {0} returned null", endpoint));
				return null;
			}

			if (RtmpUtil.IsError(result))
			{
				var error = RtmpUtil.GetError(result);
				var errordetail = error != null && error.faultDetail != null ? string.Format(" [{0}]", error.faultDetail) : "";
				var errorstr = error != null && error.faultString != null ? string.Format(", {0}", error.faultString) : "";
				StaticLogger.Warning(string.Format(
					"{0} returned an error{1}{2}", 
					endpoint,
					errorstr,
					errordetail
				));
				return null;
			}

			var body = RtmpUtil.GetBodies(result).FirstOrDefault();
			if (body == null)
			{
				StaticLogger.Debug(endpoint + " RtmpUtil.GetBodies returned null");
				return null;
			}

			return body.Item1;
		}
		public T InvokeService<T>(string service, string operation, params object[] args) where T : class
		{
			var msg = new RemotingMessage();
			msg.operation = operation;
			msg.destination = service;
			msg.headers["DSRequestTimeout"] = 60;
			msg.headers["DSId"] = RtmpUtil.RandomUidString();
			msg.headers["DSEndpoint"] = "my-rtmps";
			msg.body = args;
			msg.messageId = RtmpUtil.RandomUidString();

			string endpoint = service + "." + operation;

			var result = Host.Call(msg);
			if (result == null)
			{
				StaticLogger.Warning(string.Format("Invoking {0} returned null", endpoint));
				return null;
			}

			if (RtmpUtil.IsError(result))
			{
				var error = RtmpUtil.GetError(result);
				var errordetail = error != null && error.faultDetail != null ? string.Format(" [{0}]", error.faultDetail) : "";
				var errorstr = error != null && error.faultString != null ? string.Format(", {0}", error.faultString) : "";
				StaticLogger.Warning(string.Format(
					"{0} returned an error{1}{2}",
					endpoint,
					errorstr,
					errordetail
				));
				return null;
			}

			var body = RtmpUtil.GetBodies(result).FirstOrDefault();
			if (body == null)
			{
				StaticLogger.Debug(endpoint + " RtmpUtil.GetBodies returned null");
				return null;
			}

			if (body.Item1 == null)
			{
				StaticLogger.Debug(endpoint + " Body.Item1 returned null");
				return null;
			}

			object obj = null;
			if (body.Item1 is ASObject)
			{
				var ao = (ASObject)body.Item1;
				obj = MessageTranslator.Instance.GetObject<T>(ao);
				if (obj == null)
				{
					StaticLogger.Debug(endpoint + " expected " + typeof(T) + ", got " + ao.TypeName);
					return null;
				}
			}
			else if (body.Item1 is ArrayCollection)
			{
				try
				{
					obj = Activator.CreateInstance(typeof(T), (ArrayCollection)body.Item1);
				}
				catch (Exception ex)
				{
					StaticLogger.Warning(endpoint + " failed to construct " + typeof(T));
					StaticLogger.Debug(ex);
					return null;
				}
			}
			else
			{
				StaticLogger.Debug(endpoint + " unknown object " + body.Item1.GetType());
				return null;
			}

			if (obj is MessageObject)
				((MessageObject)obj).TimeStamp = body.Item2;

			return (T)obj;
		}

		public PublicSummoner GetPlayerByName(string name)
		{
			return InvokeService<PublicSummoner>(
				"summonerService",
				"getSummonerByName",
				name
			);
		}

		public PlayerLifetimeStats RetrievePlayerStatsByAccountId(Int64 acctid)
		{
			return InvokeService<PlayerLifetimeStats>(
				"playerStatsService",
				"retrievePlayerStatsByAccountId",
				acctid,
				"CURRENT"
			);
		}

		public RecentGames GetRecentGames(Int64 acctid)
		{
			return InvokeService<RecentGames>(
				"playerStatsService",
				"getRecentGames",
				acctid
			);
		}

		public ChampionStatInfoList RetrieveTopPlayedChampions(Int64 acctid, string gamemode)
		{
			return InvokeService<ChampionStatInfoList>(
				"playerStatsService",
				"retrieveTopPlayedChampions",
				acctid,
				gamemode
			);
		}

		public SpellBookPage SelectDefaultSpellBookPage(SpellBookPage page)
		{
			return InvokeService<SpellBookPage>(
				"spellBookService",
				"selectDefaultSpellBookPage",
				page
			);
		}
		public GameDTO GetGame(int num)
		{
			return InvokeService<GameDTO>(
				"gameService",
				"getGame",
				num
			);
		}
	}
}
