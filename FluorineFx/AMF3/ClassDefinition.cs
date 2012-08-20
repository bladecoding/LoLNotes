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
using System.Diagnostics;
using System.Reflection;
using System.Collections;

using FluorineFx.Exceptions;

namespace FluorineFx.AMF3
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
	public sealed class ClassDefinition
	{
        private string _className;
        private ClassMember[] _members;
        private bool _externalizable;
        private bool _dynamic;

        internal static ClassMember[] EmptyClassMembers = new ClassMember[0];

        internal ClassDefinition(string className, ClassMember[] members, bool externalizable, bool dynamic)
		{
			_className = className;
            _members = members;
			_externalizable = externalizable;
			_dynamic = dynamic;
		}

        /// <summary>
        /// Gets the class name.
        /// </summary>
		public string ClassName{ get{ return _className; } }
        /// <summary>
        /// Gets the class member count.
        /// </summary>
		public int MemberCount
        { 
            get
            {
                if (_members == null)
                    return 0;
                return _members.Length; 
            } 
        }
        /// <summary>
        /// Gets the array of class members.
        /// </summary>
        public ClassMember[] Members { get { return _members; } }
        /// <summary>
        /// Indicates whether the class is externalizable.
        /// </summary>
		public bool IsExternalizable{ get{ return _externalizable; } }
        /// <summary>
        /// Indicates whether the class is dynamic.
        /// </summary>
		public bool IsDynamic{ get{ return _dynamic; } }
        /// <summary>
        /// Indicates whether the class is typed (not anonymous).
        /// </summary>
		public bool IsTypedObject{ get{ return (_className != null && _className != string.Empty); } }
 	}

    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class ClassMember
    {
        string _name;
        BindingFlags _bindingFlags;
        MemberTypes _memberType;
        /// <summary>
        /// Cached member custom attributes.
        /// </summary>
        object[] _customAttributes;

        internal ClassMember(string name, BindingFlags bindingFlags, MemberTypes memberType, object[] customAttributes)
        {
            _name = name;
            _bindingFlags = bindingFlags;
            _memberType = memberType;
            _customAttributes = customAttributes;
        }
        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        /// <summary>
        /// Gets the member binding flags.
        /// </summary>
        public BindingFlags BindingFlags
        {
            get { return _bindingFlags; }
        }
        /// <summary>
        /// Gets the member type.
        /// </summary>
        public MemberTypes MemberType
        {
            get { return _memberType; }
        }
        /// <summary>
        /// Gets member custom attributes.
        /// </summary>
        public object[] CustomAttributes
        {
            get { return _customAttributes; }
        }


		public override string ToString()
		{
			return Name + ", " + base.ToString();
		}
    }
}
