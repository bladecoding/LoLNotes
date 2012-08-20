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

#if (NET_1_1)
    //NA
#else

using System;
using System.Data;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;

namespace FluorineFx.Util
{
    /// <summary>
    /// Schema Importer Extension used for WebServices in order to recognize the schema for DataTable within wsdl
    /// </summary>
    class DataTableSchemaImporterExtension : SchemaImporterExtension
    {
        Hashtable importedTypes = new Hashtable();

        public override string ImportSchemaType(string name, string schemaNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            IList values = schemas.GetSchemas(schemaNamespace);
            if (values.Count != 1)
            {
                return null;
            }
            XmlSchema schema = values[0] as XmlSchema;
            if (schema == null)
                return null;
            XmlSchemaType type = (XmlSchemaType)schema.SchemaTypes[new XmlQualifiedName(name, schemaNamespace)];
            return ImportSchemaType(type, context, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
        }


        public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            if (type == null)
            {
                return null;
            }

            if (importedTypes[type] != null)
            {
                mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                return (string)importedTypes[type];
            }
            if (!(context is XmlSchemaElement))
                return null;

            if (type is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ct = (XmlSchemaComplexType)type;
                if (ct.Particle is XmlSchemaSequence)
                {
                    XmlSchemaObjectCollection items = ((XmlSchemaSequence)ct.Particle).Items;
                    if (items.Count == 2 && items[0] is XmlSchemaAny && items[1] is XmlSchemaAny)
                    {
                        XmlSchemaAny any0 = (XmlSchemaAny)items[0];
                        XmlSchemaAny any1 = (XmlSchemaAny)items[1];
                        if (any0.Namespace == XmlSchema.Namespace && any1.Namespace == "urn:schemas-microsoft-com:xml-diffgram-v1")
                        {
                            string typeName = typeof(DataTable).FullName;
                            importedTypes.Add(type, typeName);
                            mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataTable).Namespace));
                            compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                            return typeName;
                        }
                    }
                }
            }
            return null;
        }
    }
}
#endif
