using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace FluorineFx.Util
{
    /// <summary> 
    /// Helper methods for array containing culturally-invariant strings.
    /// </summary>
    internal sealed class InvariantStringArray
    {
        private InvariantStringArray() { }

        public static void Sort(string[] keys, Array items)
        {
            Debug.Assert(keys != null);
            Debug.Assert(items != null);

            Array.Sort(keys, items, InvariantComparer);
        }

        public static int BinarySearch(string[] values, string sought)
        {
            Debug.Assert(values != null);

            return Array.BinarySearch(values, sought, InvariantComparer);
        }

        private static IComparer InvariantComparer
        {
            get
            {
#if NET_1_0
                return StringComparer.DefaultInvariant;
#else
                return Comparer.DefaultInvariant;
#endif
            }
        }

#if NET_1_0
        
        [ Serializable ]
        private sealed class StringComparer : IComparer
        {
            private CompareInfo _compareInfo;
            
            public static readonly StringComparer DefaultInvariant = new StringComparer(CultureInfo.InvariantCulture);

            private StringComparer(CultureInfo culture)
            {
                Debug.Assert(culture != null);
                
                _compareInfo = culture.CompareInfo;
            }

            public int Compare(object x, object y)
            {
                if (x == y) 
                    return 0;
                else if (x == null) 
                    return -1;
                else if (y == null) 
                    return 1;
                else
                    return _compareInfo.Compare((string) x, (string) y);
            }
        }

#endif
    }        
}
