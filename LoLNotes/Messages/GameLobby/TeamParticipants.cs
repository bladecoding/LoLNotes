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
using FluorineFx;
using FluorineFx.AMF3;
using LoLNotes.Flash;
using LoLNotes.Messages.GameLobby.Participants;

namespace LoLNotes.Messages.GameLobby
{
    [DebuggerDisplay("Count: {Count}")]
    public class TeamParticipants : List<Participant>
    {
        protected readonly ArrayCollection Base;
        public TeamParticipants()
        {
        }
        public TeamParticipants(IEnumerable<Participant> collection)
            : base(collection)
        {
        }

		public TeamParticipants(ArrayCollection thebase)
        {
            Base = thebase;
            if (Base == null)
                return;

            foreach (var item in Base)
            {
				var obj = item as ASObject;
				if (obj == null)
					continue;

                if (obj.TypeName.Contains("PlayerParticipant"))
                {
                    Add(new PlayerParticipant(obj));
                }
				else if (obj.TypeName.Contains("ObfuscatedParticipant"))
                {
                    Add(new ObfuscatedParticipant(obj));
                }
				else if (obj.TypeName.Contains("BotParticipant"))
                {
                    Add(new BotParticipant(obj));
                }
                else
                {
                    throw new NotSupportedException("Unexcepted type in team array " + obj.TypeName);
                }
            }
        }
    }
}
