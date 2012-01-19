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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Db4objects.Db4o;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameStats;
using Newtonsoft.Json;

namespace LoLNotes.Storage
{
	public static class DbExporter
	{
		static void ActivateList(IObjectContainer db, IList list)
		{
			foreach (var obj in list)
			{
				db.Activate(obj, int.MaxValue);
			}
		}
		public static void Import(GameStorage storage, Stream stream)
		{
			var serializer = new JsonSerializer();
			serializer.TypeNameHandling = TypeNameHandling.Auto;
			using (var json = new JsonTextReader(new StreamReader(stream)))
			{
				var export = serializer.Deserialize<JsonExportHolder>(json);

				foreach (var ply in export.Players)
					storage.RecordPlayer(ply, true);

				foreach (var end in export.EndStats)
					storage.RecordGame(end);

				storage.Commit();
			}
		}
		public static void Export(string version, IObjectContainer db, Stream stream)
		{
			var export = new JsonExportHolder
			{
				Version = version,
				EndStats = db.Query<EndOfGameStats>().ToList(),
				Players = db.Query<PlayerEntry>().ToList(),
			};
			ActivateList(db, export.EndStats);
			ActivateList(db, export.Players);

			var serializer = new JsonSerializer();
			serializer.TypeNameHandling = TypeNameHandling.Auto;
			using (var json = new JsonTextWriter(new StreamWriter(stream)))
			{
				json.Formatting = Formatting.Indented;
				serializer.Serialize(json, export);
			}
		}
	}

	public class JsonExportHolder
	{
		public string Version { get; set; }
		public List<EndOfGameStats> EndStats { get; set; }
		public List<PlayerEntry> Players { get; set; }
	}
}
