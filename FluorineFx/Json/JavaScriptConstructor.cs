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
using System.Text;

namespace FluorineFx.Json
{
    /// <summary>
    /// Represents a JavaScript constructor.
    /// </summary>
    public class JavaScriptConstructor
    {
        private string _name;
        private JavaScriptParameters _parameters;

        public JavaScriptParameters Parameters
        {
            get { return _parameters; }
        }

        public string Name
        {
            get { return _name; }
        }

        public JavaScriptConstructor(string name, JavaScriptParameters parameters)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.Length == 0)
                throw new ArgumentException("Constructor name cannot be empty.", "name");

            _name = name;
            _parameters = parameters != null ? parameters : JavaScriptParameters.Empty;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("new ");
            sb.Append(_name);
            sb.Append("(");
            if (_parameters != null)
            {
                for (int i = 0; i < _parameters.Count; i++)
                {
                    sb.Append(_parameters[i]);
                }
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}