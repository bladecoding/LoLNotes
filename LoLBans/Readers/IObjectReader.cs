using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans.Readers
{
    public delegate void ObjectReadD<T>(T obj);
    public interface IObjectReader<T>
    {
        event ObjectReadD<T> ObjectRead; 
    }
}
