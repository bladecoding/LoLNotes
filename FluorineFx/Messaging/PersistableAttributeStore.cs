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
using System.IO;
using System.Collections;
#if !(NET_1_1)
using System.Collections.Generic;
#endif
using FluorineFx.IO;
using FluorineFx.Messaging.Api;
using FluorineFx.Messaging.Api.Persistence;

namespace FluorineFx.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
    [CLSCompliant(false)]
    public class PersistableAttributeStore : AttributeStore, IPersistable
    {
        /// <summary>
        /// Persistence flag
        /// </summary>
        protected bool _persistent = true;
        /// <summary>
        /// Attribute store name
        /// </summary>
        protected string _name;
        /// <summary>
        /// Attribute store path (on local hard drive)
        /// </summary>
        protected string _path;
        /// <summary>
        /// Attribute store type
        /// </summary>
        protected string _type;
        /// <summary>
        /// Last modified timestamp
        /// </summary>
        protected long _lastModified = -1;
        /// <summary>
        /// Store object that deals with save/load routines.
        /// </summary>
        protected IPersistenceStore _store = null;

        /// <summary>
        /// Initializes a new instance of the PersistableAttributeStore.
        /// </summary>
        /// <param name="type">Attribute store type.</param>
        /// <param name="name">Attribute store name.</param>
        /// <param name="path">Attribute store path.</param>
        /// <param name="persistent">Whether store is persistent or not.</param>
        public PersistableAttributeStore(string type, string name, string path, bool persistent)
        {
            _name = name;
            _path = path;
            _type = type;
            _persistent = persistent;
        }

        /// <summary>
        /// Gets or sets the attribute store type.
        /// </summary>
        /// <value>The attribute store type.</value>
        public virtual string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #region IPersistable Members

        /// <summary>
        /// Gets or sets a value indicating whether the object is persistent.
        /// </summary>
        /// <value>A value indicating whether the object is persistent.</value>
        public virtual bool IsPersistent
        {
            get { return _persistent; }
            set { _persistent = value; }
        }
        /// <summary>
        /// Gets or sets the name of the persistent object.
        /// </summary>
        /// <value>The name of the persistent object.</value>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the path of the persistent object.
        /// </summary>
        /// <value>The path of the persistent object.</value>
        public virtual string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        /// <summary>
        /// Gets the timestamp when the object was last modified.
        /// </summary>
        /// <value>The timestamp when the object was last modified.</value>
        public virtual long LastModified
        {
            get { return _lastModified; }
        }
        /// <summary>
        /// Gets or sets the persistence store this object is stored in.
        /// </summary>
        /// <value>The persistence store this object is stored in.</value>
        public virtual IPersistenceStore Store
        {
            get { return _store; }
            set
            {
                _store = value;
                if (_store != null)
                    _store.Load(this);
            }
        }

        /// <summary>
        /// Writes the object to the specified output stream.
        /// </summary>
        /// <param name="writer">Writer to write to.</param>
        public void Serialize(AMFWriter writer)
        {
#if !(NET_1_1)
            Dictionary<string, object> persistentAttributes = new Dictionary<string, object>();
#else
            Hashtable persistentAttributes = new Hashtable();
#endif
            foreach (string attribute in this.GetAttributeNames())
            {
                if (attribute.StartsWith(Constants.TransientPrefix))
                    continue;
                persistentAttributes.Add(attribute, this[attribute]);
            }
            writer.WriteData(ObjectEncoding.AMF0, persistentAttributes);
        }

        /// <summary>
        /// Loads the object from the specified input stream.
        /// </summary>
        /// <param name="reader">Reader to load from.</param>
        public void Deserialize(AMFReader reader)
        {
            this.RemoveAttributes();
            IDictionary persistentAttributes = reader.ReadData() as IDictionary;
            this.SetAttributes(persistentAttributes);
        }

        #endregion

        /// <summary>
        /// Set last modified flag to current system time.
        /// </summary>
        protected void OnModified()
        {
            _lastModified = System.Environment.TickCount;
            if (_store != null)
                _store.Save(this);
        }

        /// <summary>
        /// Removes an attribute.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>
        /// true if the attribute was found and removed otherwise false.
        /// </returns>
        public override bool RemoveAttribute(string name)
        {
            bool result = base.RemoveAttribute(name);
            if (result && !name.StartsWith(Constants.TransientPrefix))
                OnModified();
            return result;
        }
        /// <summary>
        /// Removes all attributes.
        /// </summary>
        public override void RemoveAttributes()
        {
            base.RemoveAttributes();
            OnModified();
        }
        /// <summary>
        /// Sets an attribute on this object.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>
        /// true if the attribute value changed otherwise false.
        /// </returns>
        public override bool SetAttribute(string name, object value)
        {
            bool result = base.SetAttribute(name, value);
            if (result && !name.StartsWith(Constants.TransientPrefix))
                OnModified();
            return result;
        }
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Attribute store.</param>
        public override void SetAttributes(IAttributeStore values)
        {
            base.SetAttributes(values);
            OnModified();
        }

#if !(NET_1_1)
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public override void SetAttributes(IDictionary<string, object> values)
        {
            base.SetAttributes(values);
            OnModified();
        }
#else
        /// <summary>
        /// Sets multiple attributes on this object.
        /// </summary>
        /// <param name="values">Dictionary of attributes.</param>
        public override void SetAttributes(IDictionary values)
		{
			base.SetAttributes (values);
			OnModified();
		}
#endif
    }
}
