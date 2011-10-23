using System;

namespace LoLBans
{
    public static class Parse
    {
        public static int Int(string str)
        {
            int ret;
            if (int.TryParse(str, out ret))
                return ret;
            throw new ArgumentException(string.Format("Expected {0}", ret.GetType()));
        }
        public static bool Bool(string str)
        {
            bool ret;
            if (bool.TryParse(str, out ret))
                return ret;
            throw new ArgumentException(string.Format("Expected {0}", ret.GetType()));
        }
    }
}
