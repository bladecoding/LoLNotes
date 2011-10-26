using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LoLNotes.Util
{
    public static class Compression
    {
        public static string DecompressString(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        public static byte[] CompressString(string str)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    byte[] bstr = Encoding.UTF8.GetBytes(str);
                    gzip.Write(bstr, 0, bstr.Length);
                }
                return ms.ToArray();
            }
        }
    }
}
