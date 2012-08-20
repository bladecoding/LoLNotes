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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace FluorineFx.Json
{
    public class Identifier
    {
        private string _name;

        public string Name
        {
            get { return _name; }
        }

        public Identifier(string name)
        {
            _name = name;
        }

        private static bool IsAsciiLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public override bool Equals(object obj)
        {
            Identifier function = obj as Identifier;

            return Equals(function);
        }

        public bool Equals(Identifier function)
        {
            return (_name == function.Name);
        }

        public static bool Equals(Identifier a, Identifier b)
        {
            if (a == b)
                return true;

            if (a != null && b != null)
                return a.Equals(b);

            return false;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool operator ==(Identifier a, Identifier b)
        {
            return Identifier.Equals(a, b);
        }

        public static bool operator !=(Identifier a, Identifier b)
        {
            return !Identifier.Equals(a, b);
        }
    }
}