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

using log4net;
using FluorineFx.Configuration;
using FluorineFx.AMF3;
using FluorineFx.IO.Bytecode;

namespace FluorineFx.IO.Readers
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AMF3OptimizedObjectReader : IAMFReader
	{
        private static readonly ILog _log = LogManager.GetLogger(typeof(AMF3OptimizedObjectReader));

        Hashtable _optimizedReaders;

		public AMF3OptimizedObjectReader()
		{
            _optimizedReaders = new Hashtable();
		}

		#region IAMFReader Members

		public object ReadData(AMFReader reader)
		{
			int handle = reader.ReadAMF3IntegerData();
			bool inline = ((handle & 1) != 0 ); handle = handle >> 1;
			if (!inline)
			{
				//An object reference
				return reader.ReadAMF3ObjectReference(handle);
			}
			else
			{
				ClassDefinition classDefinition = reader.ReadClassDefinition(handle);
				object instance = null;
                IReflectionOptimizer reflectionOptimizer = _optimizedReaders[classDefinition.ClassName] as IReflectionOptimizer;
				if (reflectionOptimizer == null)
				{
					lock (_optimizedReaders)
					{
						if (classDefinition.IsTypedObject)
						{
							if (!_optimizedReaders.Contains(classDefinition.ClassName))
							{
								//Temporary reader
                                _optimizedReaders[classDefinition.ClassName] = new AMF3TempObjectReader();
								Type type = ObjectFactory.Locate(classDefinition.ClassName);
								if (type != null)
								{
									instance = ObjectFactory.CreateInstance(type);
                                    if (classDefinition.IsExternalizable)
                                    {
                                        reflectionOptimizer = new AMF3ExternalizableReader();
                                        _optimizedReaders[classDefinition.ClassName] = reflectionOptimizer;
                                        instance = reflectionOptimizer.ReadData(reader, classDefinition);
                                    }
                                    else
                                    {
                                        reader.AddAMF3ObjectReference(instance);

                                        IBytecodeProvider bytecodeProvider = null;
#if NET_1_1
									    //codedom only
									    if( FluorineConfiguration.Instance.OptimizerSettings != null )
										    bytecodeProvider = new FluorineFx.IO.Bytecode.CodeDom.BytecodeProvider();
#else
                                        if (FluorineConfiguration.Instance.OptimizerSettings.Provider == "codedom")
                                            bytecodeProvider = new FluorineFx.IO.Bytecode.CodeDom.BytecodeProvider();
                                        if (FluorineConfiguration.Instance.OptimizerSettings.Provider == "il")
                                            bytecodeProvider = new FluorineFx.IO.Bytecode.Lightweight.BytecodeProvider();
#endif

                                        reflectionOptimizer = bytecodeProvider.GetReflectionOptimizer(type, classDefinition, reader, instance);
                                        //Fixup
                                        if (reflectionOptimizer != null)
                                            _optimizedReaders[classDefinition.ClassName] = reflectionOptimizer;
                                        else
                                            _optimizedReaders[classDefinition.ClassName] = new AMF3TempObjectReader();
                                    }
								}
								else
								{
                                    reflectionOptimizer = new AMF3TypedASObjectReader(classDefinition.ClassName);
                                    _optimizedReaders[classDefinition.ClassName] = reflectionOptimizer;
                                    instance = reflectionOptimizer.ReadData(reader, classDefinition);
								}
							}
							else
							{
                                reflectionOptimizer = _optimizedReaders[classDefinition.ClassName] as IReflectionOptimizer;
								instance = reflectionOptimizer.ReadData(reader, classDefinition);
							}
						}
						else
						{
                            reflectionOptimizer = new AMF3TypedASObjectReader(classDefinition.ClassName);
                            _optimizedReaders[classDefinition.ClassName] = reflectionOptimizer;
                            instance = reflectionOptimizer.ReadData(reader, classDefinition);
						}
					}
				}
				else
				{
					instance = reflectionOptimizer.ReadData(reader, classDefinition);
				}
				return instance;
			}
		}

		#endregion
	}

    class AMF3TempObjectReader : IReflectionOptimizer
    {
        #region IReflectionOptimizer Members

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        public object ReadData(AMFReader reader, ClassDefinition classDefinition)
        {
            object obj = reader.ReadAMF3Object(classDefinition);
            return obj;
        }

        #endregion
    }
}
