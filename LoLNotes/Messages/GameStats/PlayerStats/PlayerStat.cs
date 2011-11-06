﻿using System;
using System.Diagnostics;
using LoLNotes.Flash;
using NotMissing;

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



namespace LoLNotes.Messages.GameStats.PlayerStats
{
    [DebuggerDisplay("{DisplayName}")]
    public class PlayerStat : ICloneable
    {
        protected readonly FlashObject Base;
        public PlayerStat()
        {
        }

        public PlayerStat(FlashObject thebase)
        {
            Base = thebase;

            FlashObject.SetFields(this, Base);
        }

        [InternalName("displayName")]
        public string DisplayName { get; set; }

        [InternalName("priority")]
        public int Priority { get; set; }

        [InternalName("statCategory")]
        public PlayerStatCategory Category { get; set; }

        [InternalName("statTypeName")]
        public string StatTypeName { get; set; }

        [InternalName("value")]
        public int Value { get; set; }

        public object Clone()
        {
            return new PlayerStat
            {
                DisplayName = DisplayName,
                Priority = Priority,
                Category = Category.CloneT(),
                StatTypeName = StatTypeName,
                Value = Value,
            };
        }
    }
}
