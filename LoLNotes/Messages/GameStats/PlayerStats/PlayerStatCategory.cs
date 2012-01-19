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
using System.Diagnostics;
using FluorineFx;
using LoLNotes.Flash;

namespace LoLNotes.Messages.GameStats.PlayerStats
{
    [DebuggerDisplay("{DisplayName}")]
    public class PlayerStatCategory : BaseObject, ICloneable
    {
		public PlayerStatCategory()
			: base(null)
        {
        }
		public PlayerStatCategory(ASObject thebase)
			: base(thebase)
        {
            BaseObject.SetFields(this, Base);
        }

        [InternalName("displayName")]
        public string DisplayName { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("priority")]
        public int Priority { get; set; }


        public object Clone()
        {
            return new PlayerStatCategory
            {
                DisplayName = DisplayName,
                Name = Name,
                Priority = Priority,
            };
        }
    }
}
