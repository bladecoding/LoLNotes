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

namespace LoLNotes.Messages.GameLobby.Participants
{
    [DebuggerDisplay("{Name}")]
    public class PlayerParticipant : GameParticipant
    {
        public PlayerParticipant()
            : base(null)
        {
        }
        public PlayerParticipant(ASObject thebase)
            : base(thebase)
        {
            BaseObject.SetFields(this, thebase);
        }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }
		[InternalName("summonerId")]
		public Int64 SummonerId { get; set; }
		[InternalName("accountId")]
		public Int64 AccountId { get; set; }
		[InternalName("teamParticipantId")]
		public Int64 TeamParticipantId { get; set; }



		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is PlayerParticipant)
				return SummonerId == ((PlayerParticipant)obj).SummonerId;
			return base.Equals(obj);
		}

        public override object Clone()
        {
            return new PlayerParticipant
            {
                ProfileIconId = ProfileIconId,
                SummonerId = SummonerId,
                InternalName = InternalName,
                IsMe = IsMe,
                IsGameOwner = IsGameOwner,
                Name = Name,
                PickMode = PickMode,
                PickTurn = PickTurn,
				TeamParticipantId = TeamParticipantId,
            };
        }

    }
}
