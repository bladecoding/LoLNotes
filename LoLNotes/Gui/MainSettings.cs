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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NotMissing.Logging;

namespace LoLNotes.Gui
{
	public class MainSettings
	{
		public string Region { get; set; }

		public MainSettings()
		{
			Region = "NA";
		}

		public bool Save(string file)
		{
			try
			{
				using (var sw = new StreamWriter(File.Open(file, FileMode.Create, FileAccess.Write)))
				{
					sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
					return true;
				}
			}
			catch (IOException io)
			{
				StaticLogger.Debug(io);
				return false;
			}
		}

		public static MainSettings Load(string file)
		{
			try
			{
				if (!File.Exists(file))
					return new MainSettings();

				using (var sr = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read)))
				{
					return JsonConvert.DeserializeObject<MainSettings>(sr.ReadToEnd());
				}
			}
			catch (IOException io)
			{
				StaticLogger.Debug(io);
				return new MainSettings();
			}
		}
	}
}
