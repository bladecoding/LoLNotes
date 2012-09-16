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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.TA;
using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.IO;
using FluorineFx.Messaging.Messages;
using FluorineFx.Messaging.Rtmp.Event;
using LoLNotes.Gui.Controls;
using LoLNotes.Messages.Account;
using LoLNotes.Messages.Champion;
using LoLNotes.Messages.Commands;
using LoLNotes.Messages.GameLobby;
using LoLNotes.Messages.GameLobby.Participants;
using LoLNotes.Messages.GameStats;
using LoLNotes.Messages.Readers;
using LoLNotes.Messages.Statistics;
using LoLNotes.Messages.Summoner;
using LoLNotes.Properties;
using LoLNotes.Proxy;
using LoLNotes.Storage;
using LoLNotes.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotMissing.Logging;

namespace LoLNotes.Gui
{
	public partial class MainForm : Form
    {
        public static readonly string Version = AssemblyAttributes.FileVersion + AssemblyAttributes.Configuration;
		readonly Dictionary<string, Icon> Icons;
		readonly Dictionary<LeagueRegion, CertificateHolder> Certificates;

        //RtmpsProxyHost is an rtmps proxy that allows us to intercept/send packets.
		RtmpsProxyHost Connection;
        //MessageReader converts ASObjects to Message Objects making them easier to work with.
		MessageReader Reader;
        //CertificateInstaller installs the ssl certificates which are required for RtmpsProxyHost to work.
		CertificateInstaller Installer;
        //ProcessInjector forces lol to connect to RtmpsProxyHost instead of their servers.
		ProcessInjector Injector;
		GameDTO CurrentGame;
		List<ChampionDTO> Champions;
		SummonerData SelfSummoner;

		public MainForm()
		{
			InitializeComponent();

			Icons = new Dictionary<string, Icon>
            {
                {"Red",  Icon.FromHandle(Resources.circle_red.GetHicon())},
                {"Yellow",  Icon.FromHandle(Resources.circle_yellow.GetHicon())},
                {"Green",  Icon.FromHandle(Resources.circle_green.GetHicon())},
            };
			Certificates = new Dictionary<LeagueRegion, CertificateHolder>
			{
				{LeagueRegion.NA, new CertificateHolder("prod.na1.lol.riotgames.com", Resources.prod_na1_lol_riotgames_com)},
				{LeagueRegion.EUW, new CertificateHolder("prod.eu.lol.riotgames.com", Resources.prod_eu_lol_riotgames_com)},
				{LeagueRegion.EUN, new CertificateHolder("prod.eun1.lol.riotgames.com", Resources.prod_eun1_lol_riotgames_com)},
				{LeagueRegion.GARENA, new CertificateHolder("prod.lol.garenanow.com", Resources.prod_lol_garenanow_com)},
 			};

            var cert = Certificates[LeagueRegion.NA];

			Injector = new ProcessInjector("lolclient");
			Connection = new RtmpsProxyHost(2099, cert.Domain, 2099, cert.Certificate);
			Reader = new MessageReader(Connection);

			Connection.Connected += Connection_Connected;

			Injector.Injected += Injector_Injected;

			Reader.ObjectRead += Reader_ObjectRead;

			Installer = new CertificateInstaller(Certificates.Select(c => c.Value.Certificate).ToArray());
            //Install the ssl certificates
            if (!Installer.IsInstalled)
                Installer.Install();
		}

		void Injector_Injected(object sender, EventArgs e)
		{
			if (Created)
				BeginInvoke(new Action(UpdateIcon));
		}

		void UpdateIcon()
		{
			if (!Injector.IsInjected)
				Icon = Icons["Red"];
			else if (Connection != null && Connection.IsConnected)
				Icon = Icons["Green"];
			else
				Icon = Icons["Yellow"];
		}

		void Connection_Connected(object sender, EventArgs e)
		{
			if (Created)
				BeginInvoke(new Action(UpdateIcon));
		}

		void Reader_ObjectRead(object obj)
		{
            if (obj is GameDTO)
                CurrentGame = (GameDTO)obj;
            else if (obj is EndOfGameStats)
                return;
            else if (obj is List<ChampionDTO>)
                Champions = (List<ChampionDTO>)obj;
            else if (obj is LoginDataPacket)
                SelfSummoner = ((LoginDataPacket)obj).AllSummonerData.Summoner;
		}

		private void MainForm_Shown(object sender, EventArgs e)
        {
			UpdateIcon();

			//Start after the form is shown otherwise Invokes will fail
			Connection.Start();
			Injector.Start();
		}
	}
}
