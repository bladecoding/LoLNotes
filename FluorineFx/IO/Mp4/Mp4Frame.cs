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

namespace FluorineFx.IO.Mp4
{
    class Mp4Frame : IComparable<Mp4Frame>
    {
        private byte type;

        public byte Type
        {
            get { return type; }
            set { type = value; }
        }

        private long offset;

        public long Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private int size;

        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        private double time;

        public double Time
        {
            get { return time; }
            set { time = value; }
        }

        private bool isKeyFrame;

        public bool IsKeyFrame
        {
            get { return isKeyFrame; }
            set { isKeyFrame = value; }
        }

        public override int GetHashCode()
        {
		    int prime = 31;
		    int result = 1;
		    result = prime * result + (int) (offset ^ (offset >> 32));
		    result = prime * result + type;
		    return result;            
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (!(obj is Mp4Frame))
                return false;
            Mp4Frame other = (Mp4Frame)obj;
            if (offset != other.Offset)
                return false;
            if (type != other.Type)
                return false;
            return true;
        }

        #region IComparable<Mp4Frame> Members

        /// <summary>
        /// The frames are expected to be sorted by their timestamp
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Mp4Frame other)
        {
            int ret = 0;
            if (this.time > other.Time)
            {
                ret = 1;
            }
            else if (this.time < other.Time)
            {
                ret = -1;
            }
            else if (this.time == other.Time && this.offset > other.Offset)
            {
                ret = 1;
            }
            else if (this.time == other.Time && this.offset < other.Offset)
            {
                ret = -1;
            }
            return ret;
        }

        #endregion
    }
}
