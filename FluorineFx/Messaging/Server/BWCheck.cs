/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Service;

namespace FluorineFx.Messaging.Server
{
    class BWCheck : IPendingServiceCallback
    {
        IConnection _connection;
        double _latency = 0;
        double _cumLatency = 1;
        int _count = 0;
        int _sent = 0;
        double _kbitDown = 0;
        double _deltaDown = 0;
        double _deltaTime = 0;
        ArrayList _pakSent = new ArrayList();
        ArrayList _pakRecv = new ArrayList();
        ArrayList _beginningValues = new ArrayList();

        public BWCheck(IConnection connection)
        {
            _connection = connection;
        }

        public void CalculateClientBw()
        {
            Random random = new Random();
            double[] payload = new double[1200];
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = random.NextDouble();
            }
            _connection.SetAttribute("payload", payload);
            double[] payload1 = new double[12000];
            for (int i = 0; i < payload1.Length; i++)
            {
                payload1[i] = random.NextDouble();
            }
            _connection.SetAttribute("payload1", payload1);
            double[] payload2 = new double[1200];
            for (int i = 0; i < payload2.Length; i++)
            {
                payload2[i] = random.NextDouble();
            }
            _connection.SetAttribute("payload2", payload2);
            
            long start = System.Environment.TickCount;
            _beginningValues.Add(_connection.WrittenBytes);
            _beginningValues.Add(_connection.ReadBytes);
            _beginningValues.Add(start);
            _pakSent.Add(start);
            _sent++;
            // 1st call sends empty packet to get latency
            ServiceUtils.InvokeOnConnection(_connection, "onBWCheck", new object[0], this);
        }

        #region IPendingServiceCallback Members

        public void ResultReceived(IPendingServiceCall call)
        {
            long now1 = System.Environment.TickCount;
            _pakRecv.Add(now1);
            long timePassed = (now1 - (long)_beginningValues[2]);
            _count++;
            if (_count == 1)
            {
                _latency = Math.Min(timePassed, 800);
                _latency = Math.Max(_latency, 10);
                // We now have a latency figure so can start sending test data.
                // Second call.  1st packet sent
                _pakSent.Add(now1);
                _sent++;
                ServiceUtils.InvokeOnConnection(_connection, "onBWCheck", new object[] { _connection.GetAttribute("payload") }, this);
            }
            //The following will progressivly increase the size of the packets been sent until 1 second has elapsed.
            else if ((_count > 1 && _count < 3) && (timePassed < 1000))
            {
                _pakSent.Add(now1);
                _sent++;
                _cumLatency++;
                ServiceUtils.InvokeOnConnection(_connection, "onBWCheck", new object[] { _connection.GetAttribute("payload") }, this);
            }
            else if ((_count >= 3 && _count < 6) && (timePassed < 1000))
            {
                _pakSent.Add(now1);
                _sent++;
                _cumLatency++;
                ServiceUtils.InvokeOnConnection(_connection, "onBWCheck", new object[] { _connection.GetAttribute("payload1") }, this);
            }
            else if (_count >= 6 && (timePassed < 1000))
            {
                _pakSent.Add(now1);
                _sent++;
                _cumLatency++;
                ServiceUtils.InvokeOnConnection(_connection, "onBWCheck", new object[] { _connection.GetAttribute("payload2") }, this);
            }
            //Time elapsed now do the calcs
            else if (_sent == _count)
            {
                // see if we need to normalize latency
                if (_latency >= 100)
                {
                    //make sure satelite and modem is detected properly
                    if ((long)_pakRecv[1] - (long)_pakRecv[0] > 1000)
                    {
                        _latency = 100;
                    }
                }

                _connection.RemoveAttribute("payload");
                _connection.RemoveAttribute("payload1");
                _connection.RemoveAttribute("payload2");
                // tidy up and compute bandwidth
                _deltaDown = (_connection.WrittenBytes - (long)_beginningValues[0]) * 8 / 1000; // bytes to kbits
                _deltaTime = ((now1 - (long)_beginningValues[2]) - (_latency * _cumLatency)) / 1000; // total dl time - latency for each packet sent in secs
                if (_deltaTime <= 0)
                {
                    _deltaTime = (now1 - (long)_beginningValues[2]) / 1000;
                }
                _kbitDown = Math.Round(_deltaDown / _deltaTime); // kbits / sec

                ServiceUtils.InvokeOnConnection(_connection, "onBWDone", new object[] { _kbitDown, _deltaDown, _deltaTime, _latency}, this);
            }
        }

        #endregion
    }
}
