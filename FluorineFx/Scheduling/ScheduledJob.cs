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

namespace FluorineFx.Scheduling
{
    public abstract class ScheduledJobBase : IScheduledJob
    {
        private string _name;
        private Hashtable _jobDataMap;

        public ScheduledJobBase()
        {
            _name = "job" + Guid.NewGuid().ToString("N");
        }

        public ScheduledJobBase(string name)
        {
            _name = name;
        }
        
        #region IScheduledJob Members

        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (value == null || value.Trim().Length == 0)
                    throw new ArgumentException("Job name cannot be empty.");
                _name = value;
            }
        }

        public Hashtable JobDataMap
        {
            get
            {
                if (_jobDataMap == null)
                {
                    _jobDataMap = new Hashtable();
                }
                return _jobDataMap;
            }
            set { _jobDataMap = value; }
        }

        public abstract void Execute(ScheduledJobContext context);

        #endregion

        public override string ToString()
        {
            return _name;
        }
    }
}
