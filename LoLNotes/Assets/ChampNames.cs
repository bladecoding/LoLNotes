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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LoLNotes.Properties;
using Newtonsoft.Json;

namespace LoLNotes.Assets
{
	public class ChampNames : Dictionary<int, string>
	{
		protected static readonly ChampNames _instance;
		public static ChampNames Instance { get { return _instance; } }

		static ChampNames()
		{
            _instance = JsonConvert.DeserializeObject<ChampNames>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Content/Data/ChampData.json")));
		}

		public static string Get(int key)
		{
			return GetOrDefault(key) ?? key.ToString(CultureInfo.InvariantCulture);
		}
		public static string GetOrDefault(int key)
		{
			string ret;
			return _instance.TryGetValue(key, out ret) ? ret : default(string);
		}
	}
}
