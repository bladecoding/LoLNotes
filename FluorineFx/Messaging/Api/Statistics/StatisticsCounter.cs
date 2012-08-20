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

namespace FluorineFx.Messaging.Api.Statistics
{
    /// <summary>
    /// Counts numbers used by the statistics. Keeps track of current, maximum and total numbers.
    /// </summary>
    public class StatisticsCounter : IStatisticsCounter
    {
        /// <summary>
        /// Current number.
        /// </summary>
        private int _current;
        /// <summary>
        /// Total number.
        /// </summary>
        private int _total;
        /// <summary>
        /// Maximum number.
        /// </summary>
        private int _max;

        /// <summary>
        /// Increment statistics by one.
        /// </summary>
        public void Increment()
        {
            System.Threading.Interlocked.Increment(ref _total);
            System.Threading.Interlocked.Increment(ref _current);
            _max = Math.Max(_max, _current);
        }
        /// <summary>
        /// Decrement statistics by one.
        /// </summary>
        public void Decrement()
        {
            System.Threading.Interlocked.Decrement(ref _current);
        }
        /// <summary>
        /// Gets current number.
        /// </summary>
        public int Current
        {
            get { return _current; }
        }
        /// <summary>
        /// Gets total number.
        /// </summary>
        public int Total
        {
            get { return _total; }
        }
        /// <summary>
        /// Gets maximum number.
        /// </summary>
        public int Max
        {
            get { return _max; }
        }
    }
}
