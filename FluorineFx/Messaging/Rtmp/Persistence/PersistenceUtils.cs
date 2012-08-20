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
using System.IO;
using FluorineFx.Messaging.Api;

namespace FluorineFx.Messaging.Rtmp.Persistence
{
    class PersistenceUtils
    {
        public const string PersistencePath = "persistence";

        public static string GetFilename(IScope scope, string folder, string name, string extension)
        {
            string path = GetPath(scope, folder);
            path = Path.Combine(path, name + extension);
            return path;
        }

        public static string GetPath(IScope scope, string folder)
        {
            StringBuilder result = new StringBuilder();
            IScope app = ScopeUtils.FindApplication(scope);
            if (app != null)
            {
                do
                {
                    result.Insert(0, scope.Name + Path.DirectorySeparatorChar);
                    scope = scope.Parent;
                }
                while (scope.Depth >= app.Depth);
                result.Insert(0, "apps" + Path.DirectorySeparatorChar);
            }
            result.Insert(0, "~");
            result.Append(folder);
            result.Append(Path.DirectorySeparatorChar);
            return result.ToString();
        }
    }
}
