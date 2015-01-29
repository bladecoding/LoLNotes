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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using NotMissing.Logging;

namespace LoLNotes.Assets
{
	public class ChampIcons
	{
		const string ImagePath = "Content\\Images\\";
		const string ChampPath = ImagePath + "ChampIcons\\";
		static readonly object _sync;
		static readonly Dictionary<int, Bitmap> _cache;
		static Bitmap _unknown;


		static ChampIcons()
		{
			_sync = new object();
			_cache = new Dictionary<int, Bitmap>();
			_unknown = SafeBitmap(ImagePath + "unknown.png");
		}


		static void AddCached(int key, Bitmap bmp)
		{
			lock (_sync)
			{
				_cache[key] = bmp;
			}
		}
		static Bitmap FindCached(int key)
		{
			lock (_sync)
			{
				Bitmap ret;
				return _cache.TryGetValue(key, out ret) ? ret : null;
			}
		}

		static Bitmap SafeBitmap(string file)
		{
			try
			{
				return File.Exists(file) ? new Bitmap(file) : null;
			}
			catch (Exception e)
			{
				StaticLogger.Debug(e);
				return null;
			}
		}

		public static Bitmap Get(int key)
		{
			var name = ChampNames.GetOrDefault(key);
			if (name == null)
			{
				StaticLogger.Debug("Unknown champid " + key);
				return _unknown;
			}

			var bmp = FindCached(key);
			if (bmp != null)
				return bmp;

			bmp = SafeBitmap(string.Format("{0}{1}_Square_0.png", ChampPath, name));
			if (bmp == null)
			{
				StaticLogger.Debug("Unknown champ icon " + name);
				return _unknown;
			}

			AddCached(key, bmp);
			return bmp;
		}
	}
}
