using System;

namespace LoLBans
{
    //Using these methods for the exceptions
    public static class Parse
    {
        public static int Int(string str)
        {
            if (str == "NaN")
                return 0;

            int ret;
            if (int.TryParse(str, out ret))
                return ret;

            throw new FormatException(string.Format("Expected {0} got {1}", ret.GetType(), str));
        }
        public static bool Bool(string str)
        {
            bool ret;
            if (bool.TryParse(str, out ret))
                return ret;

            throw new FormatException(string.Format("Expected {0} got {1}", ret.GetType(), str));
        }
    }
}
