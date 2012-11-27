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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using LoLNotes.Flash;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using LoLNotes.Messaging;
using NotMissing;
using NotMissing.Logging;

namespace LoLNotes.Storage
{
	public class GameStorage
	{
		/// <summary>
		/// This lock is to prevents 2 objects from being added at the same time.
		/// </summary>
		public readonly object DatabaseLock = new object();
		readonly IObjectContainer Database;
		readonly IMessageProcessor Flash;
		readonly MessageReader Reader;

		public delegate void PlayerUpdateHandler(PlayerEntry player);
		/// <summary>
		/// Called when the recorder gets a new/updated player.
		/// </summary>
		public event PlayerUpdateHandler PlayerUpdate;

		public GameStorage(IObjectContainer db, IMessageProcessor flash)
		{
			Database = db;
			Flash = flash;

			Reader = new MessageReader(Flash);
			Reader.ObjectRead += Reader_ObjectRead;
		}

		void Reader_ObjectRead(object obj)
		{
			if (obj is GameDTO)
				LobbyRead((GameDTO)obj);
			else if (obj is EndOfGameStats)
				StatsRead((EndOfGameStats)obj);
		}

		void LobbyRead(GameDTO lobby)
		{
			Task.Factory.StartNew(() => CommitLobby(lobby), TaskCreationOptions.LongRunning);
		}
		void StatsRead(EndOfGameStats game)
		{
            Task.Factory.StartNew(() => CommitGame(game), TaskCreationOptions.LongRunning);
		}

		/// <summary>
		/// Lock the database and commit changes.
		/// </summary>
		public void Commit()
		{
			Stopwatch sw;
			lock (DatabaseLock)
			{
				sw = Stopwatch.StartNew();

				Database.Commit();
			}
			sw.Stop();
			StaticLogger.Debug(string.Format("Committed in {0}ms", sw.ElapsedMilliseconds));
		}

		/// <summary>
		/// Records/Commits the lobby to the database. Locking the database lock.
		/// </summary>
		/// <param name="lobby"></param>
		public void CommitLobby(GameDTO lobby)
		{
			bool committed = false;
			Stopwatch sw;
			lock (DatabaseLock)
			{
				sw = Stopwatch.StartNew();
				
				committed = RecordLobby(lobby);
				if (committed)
					Database.Commit();
			}
			sw.Stop();

			if (committed)
				StaticLogger.Debug(string.Format("Lobby committed in {0}ms", sw.ElapsedMilliseconds));
		}

		/// <summary>
		/// Checks the database for the lobby and if it does not exist then it adds it.
		/// Does not lock the database or commit
		/// </summary>
		/// <param name="lobby"></param>
		/// <returns>True if players need to be committed</returns>
		public bool RecordLobby(GameDTO lobby)
		{
			if (lobby == null)
				throw new ArgumentNullException("lobby");

			lock (_lobbycache)
			{
				//Lets clear the cache if it somehow hits 1k people.
				//That way people who afk in lobbies don't complain about a memory leak.
				if (_currentlobby == 0 || _currentlobby != lobby.Id || _lobbycache.Count > 1000)
				{
					_currentlobby = lobby.Id;
					_lobbycache.Clear();
				}
			}


			bool commit = false;

			foreach (PlayerParticipant plr in lobby.TeamOne.Union(lobby.TeamTwo).Where(p => p is PlayerParticipant).ToList())
			{
				var entry = new PlayerEntry(plr);
				if (RecordPlayer(entry, false))
				{
					commit = true;
				}
			}

			return commit;
		}
		protected Int64 _currentlobby = 0;
		protected readonly List<PlayerEntry> _lobbycache = new List<PlayerEntry>();


		/// <summary>
		/// Records/Commits the player to the database. Locking the database lock.
		/// </summary>
		/// <param name="entry">Player to record</param>
		/// <returns>The PlayerEntry from the database</returns>
		public void CommitPlayer(PlayerEntry entry)
		{
			Stopwatch sw;
			lock (DatabaseLock)
			{
				sw = Stopwatch.StartNew();

				RecordPlayer(entry, true);
				Database.Commit();
			}
			sw.Stop();
			StaticLogger.Debug(string.Format("PlayerEntry committed in {0}ms", sw.ElapsedMilliseconds));
		}

		/// <summary>
		/// Checks the database for the player and if it does not exist then it adds it.
		/// Does not lock the database or commit
		/// </summary>
		/// <param name="entry">Player to record</param>
		/// <param name="overwrite">Overwrite an existing entry</param>
		/// <returns>Returns true if the player was recorded, otherwise false</returns>
		public bool RecordPlayer(PlayerEntry entry, bool overwrite)
		{
			if (entry == null)
				throw new ArgumentNullException("entry");

			lock (_lobbycache)
			{
				//Return if the player has been cached and we are not overwriting.
				if (_lobbycache.Find(p => p.Id == entry.Id) != null && !overwrite)
					return false;
			}

			var match = Database.Query<PlayerEntry>().FirstOrDefault(m => m.Id == entry.Id);
			if (match == null || overwrite)
			{
				entry = entry.CloneT();

				if (match != null)
					Database.Delete(match);
				Database.Store(entry);

				lock (_lobbycache)
				{
					var idx = _lobbycache.FindIndex(p => p.Id == entry.Id);
					if (idx != -1)
					{
						_lobbycache[idx] = entry;
					}
					else
					{
						_lobbycache.Add(entry);
					}
				}
			}
			else
			{
				return false;
			}
			OnPlayerUpdate(entry);
			return true;
		}

		public PlayerEntry GetPlayer(Int64 id)
		{
			lock (_lobbycache)
			{
				var ply = _lobbycache.Find(p => p.Id == id);
				if (ply != null)
					return ply;
			}
			lock (DatabaseLock)
			{
				var ret = Database.Query<PlayerEntry>().FirstOrDefault(e => e.Id == id);
				if (ret == null)
					return null;
				ret = ret.CloneT();
				Database.Deactivate(ret, int.MaxValue);
				return ret;
			}
		}

		public void CommitGame(EndOfGameStats game)
		{
			Stopwatch sw;
			lock (DatabaseLock)
			{
				sw = Stopwatch.StartNew();

				RecordGame(game);
				Database.Commit();
			}
			sw.Stop();
			StaticLogger.Debug(string.Format("EndOfGameStats committed in {0}ms", sw.ElapsedMilliseconds));
		}

		public EndOfGameStats RecordGame(EndOfGameStats game)
		{
			if (game == null)
				throw new ArgumentNullException("game");

			var match = Database.Query<EndOfGameStats>().FirstOrDefault(m => m.GameId == game.GameId);
			if (match != null)
			{
				//If the object read is older then don't bother adding it.
				//However it may have new player information so don't return.
				if (game.TimeStamp > match.TimeStamp)
				{
					Database.Delete(match);
					Database.Store(game.CloneT());
				}
			}
			else
			{
				Database.Store(game.CloneT());
			}


			//No reason to record players here?
			//We should have gotten them all in the GameDTO

			//var statslist = game.TeamPlayerStats.Union(game.OtherTeamPlayerStats).ToList();
			//for (int i = 0; i < statslist.Count; i++)
			//{
			//    var entry = GetPlayer(statslist[i].UserId);
			//    if (entry == null)
			//    {
			//        entry = new PlayerEntry(statslist[i]);
			//        RecordPlayer(entry, false);
			//    }
			//}

			return match;
		}

		protected virtual void OnPlayerUpdate(PlayerEntry player)
		{
			if (PlayerUpdate != null)
				PlayerUpdate(player);
		}
	}
}
