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
using FluorineFx;
using LoLNotes.Flash;
using NotMissing;

namespace LoLNotes.Messages.GameLobby
{
	[Message(".GameDTO")]
    public class GameDTO : MessageObject, ICloneable
    {
        public GameDTO()
            : base(null)
        {
        }

        public GameDTO(ASObject obj)
            : base(obj)
        {
            BaseObject.SetFields(this, obj);
        }

        [InternalName("maxNumPlayers")]
        public int MaxPlayers
        {
            get;
            set;
        }

        [InternalName("name")]
        public string Name
        {
            get;
            set;
        }

        [InternalName("mapId")]
        public int MapId
        {
            get;
            set;
        }

        [InternalName("id")]
        public Int64 Id
        {
            get;
            set;
        }
        [InternalName("gameMode")]
        public string GameMode
        {
            get;
            set;
        }
        [InternalName("gameState")]
        public string GameState
        {
            get;
            set;
        }
        [InternalName("gameType")]
        public string GameType
        {
            get;
            set;
        }
        [InternalName("teamOne")]
        public TeamParticipants TeamOne
        {
            get;
            set;
        }
        [InternalName("teamTwo")]
        public TeamParticipants TeamTwo
        {
            get;
            set;
        }

        public object Clone()
        {
            return new GameDTO
            {
                MaxPlayers = MaxPlayers,
                Name = Name,
                MapId = MapId,
                Id = Id,
                GameMode = GameMode,
                GameState = GameState,
                GameType = GameType,
                TeamOne = new TeamParticipants(TeamOne.Clone()),
                TeamTwo = new TeamParticipants(TeamTwo.Clone()),
                TimeStamp = TimeStamp,
            };
        }
    }
}
