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
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using LoLNotes.Messages.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.riotgames.platform.systemstate
{
	public class ClientSystemStatesNotification : IExternalizable
	{
		//Removed implementation until actually needed.
		//That way we don't break things for no reason.

		//public bool practiceGameEnabled { get; set; }
		//public bool advancedTutorialEnabled { get; set; }
		//public Int32 minNumPlayersForPracticeGame { get; set; }
		//public bool archivedStatsEnabled { get; set; }
		//public bool storeCustomerEnabled { get; set; }
		//public bool socialIntegrationEnabled { get; set; }
		//public bool runeUniquePerSpellBook { get; set; }
		//public bool tribunalEnabled { get; set; }
		//public bool mucServiceEnabled { get; set; }
		//public bool observerModeEnabled { get; set; }
		//public Int32 spectatorSlotLimit { get; set; }
		//public Int32 clientHeartBeatRateSeconds { get; set; }
		//public String observableCustomGameModes { get; set; }
		//public bool teamServiceEnabled { get; set; }
		//public bool modularGameModeEnabled { get; set; }
		//public QueueThrottleDTO queueThrottleDTO { get; set; }
		//public List<int> practiceGameTypeConfigIdList { get; set; }
		//public List<int> freeToPlayChampionIdList { get; set; }
		//public List<int> enabledQueueIdsList { get; set; }
		//public List<int> obtainableChampionSkinIDList { get; set; }
		//public List<int> unobtainableChampionSkinIDList { get; set; }
		//public List<int> inactiveChampionIdList { get; set; }
		//public List<int> inactiveSpellIdList { get; set; }
		//public List<int> inactiveTutorialSpellIdList { get; set; }
		//public List<int> inactiveClassicSpellIdList { get; set; }
		//public List<int> inactiveOdinSpellIdList { get; set; }
		//public List<string> observableGameModes { get; set; }
		//public List<GameMapEnabledDTO> gameMapEnabledDTOList { get; set; }

		public string Json { get; set; }


		public void ReadExternal(IDataInput input)
		{
			Json = input.ReadUTFBytes(input.ReadUnsignedInt());
		}

		public void WriteExternal(IDataOutput output)
		{
            var bytes = Encoding.UTF8.GetBytes(Json);

            output.WriteInt(bytes.Length);
            output.WriteBytes(bytes, 0, bytes.Length);
		}
	}
}
