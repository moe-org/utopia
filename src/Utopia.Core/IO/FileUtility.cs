#region

using System.Security.Cryptography;

#endregion

namespace Utopia.Core.IO;

public static class FileUtility
{
    public static byte[] GetFileSHA256(string filename)
    {
        using (var md5 = SHA256.Create())
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                return md5.ComputeHash(stream);
            }
        }
    }
}

