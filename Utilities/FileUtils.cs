using System;
using System.IO;
using System.Security.Cryptography;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class FileUtils
    {
        public static string GetMD5(string file)
        {
            if (!File.Exists(file))
                return null;

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(file))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
