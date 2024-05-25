#region

using System.Security.Cryptography;
using Zio;

#endregion

namespace Utopia.Core.IO;

public static class FileUtilities
{
    public static byte[] GetFileSHA256(string filename, IFileSystem fileSystem)
    {
        using (var md5 = SHA256.Create())
        {
            using (var stream = fileSystem.OpenFile(filename, FileMode.Open, FileAccess.Read))
            {
                return md5.ComputeHash(stream);
            }
        }
    }
}

