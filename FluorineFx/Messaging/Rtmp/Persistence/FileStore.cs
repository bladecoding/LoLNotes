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
using System.IO;
using System.Reflection;
using System.Text;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Persistence;
using FluorineFx.Context;
using FluorineFx.IO;

namespace FluorineFx.Messaging.Rtmp.Persistence
{
	/// <summary>
	/// Simple file-based persistence for objects. Lowers memory usage if used instead of RAM memory storage.
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class FileStore : MemoryStore
	{
		private string _path = PersistenceUtils.PersistencePath;
        private string _extension = ".bin";

		public FileStore(IScope scope) : base(scope)
		{
		}

        public string Extension
        {
            get { return _extension; }
            set { _extension = value; }
        }

		/// <summary>
		/// Get filename for persistable object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		private string GetObjectFilename(IPersistable obj) 
		{
			string name = obj.Name;
			if (name == null) 
				name = MemoryStore.PersistenceNoName;
            return PersistenceUtils.GetFilename(_scope, _path, name, _extension);
		}

		/// <summary>
		/// Return file path for persistable object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		private string GetObjectFilePath(IPersistable obj) 
		{
			return GetObjectFilePath(obj, false);
		}

		/// <summary>
		/// Return file path for persistable object.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="completePath"></param>
		/// <returns></returns>
		private string GetObjectFilePath(IPersistable obj, bool completePath) 
		{
            return PersistenceUtils.GetPath(_scope, _path);
		}

		private IPersistable LoadObject(string name)
		{
            return LoadObject(name, null);
		}

		private IPersistable LoadObject(string name, IPersistable obj) 
		{
            string fileName = PersistenceUtils.GetFilename(_scope, _path, name, _extension);
            FileInfo file = _scope.Context.GetResource(fileName).File;
            if (!file.Exists)
				return null;// No such file

            IPersistable result = obj;
            lock (this.SyncRoot)
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    AMFReader reader = new AMFReader(fs);
                    string typeName = reader.ReadData() as string;
                    result = ObjectFactory.CreateInstance(typeName) as IPersistable;
                    //result.Path = GetObjectPath(name, result.Name);
                    //result = amfDeserializer.ReadData() as IPersistable;
                    result.Deserialize(reader);
                }
            }
			return result;
		}

		private bool SaveObject(IPersistable obj) 
		{
            string filename = GetObjectFilename(obj);
            FileInfo file = _scope.Context.GetResource(filename).File;
            string path = file.DirectoryName;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            lock (this.SyncRoot)
            {
                MemoryStream ms = new MemoryStream();
                AMFWriter writer = new AMFWriter(ms);
                writer.UseLegacyCollection = false;
                writer.WriteString(obj.GetType().FullName);
                //amfSerializer.WriteData(ObjectEncoding.AMF0, obj);
                obj.Serialize(writer);
                writer.Flush();
                byte[] buffer = ms.ToArray();
                ms.Close();
                using (FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Close();
                }
            }
			return true;
		}

		public override IPersistable Load(string name)
		{
			IPersistable result = base.Load(name);
			if (result != null) 
				// Object has already been loaded
				return result;
            return LoadObject(name);
		}

		public override bool Load(IPersistable obj)
		{
			if(obj.IsPersistent)
				// Already loaded
				return true;
			return (LoadObject(GetObjectFilename(obj), obj) != null);
		}

		public override bool Save(IPersistable obj)
		{
			if( !base.Save (obj) )
				return false;
			bool persistent = this.SaveObject(obj);
			obj.IsPersistent = persistent;
			return persistent;
		}

		public override bool Remove(string name)
		{
			base.Remove(name);
            string fileName = PersistenceUtils.GetFilename(_scope, _path, name, _extension);
            FileInfo file = _scope.Context.GetResource(fileName).File;
            if (!file.Exists)
				// File already deleted
				return true;
            lock (this.SyncRoot)
            {
                file.Delete();
            }
			return true;
		}

		public override bool Remove(IPersistable obj)
		{
			return base.Remove(GetObjectId(obj));
		}

	}
}
