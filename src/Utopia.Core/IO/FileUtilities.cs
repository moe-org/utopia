#region

using System.IO.Abstractions;
using System.Security.Cryptography;

#endregion

namespace Utopia.Core.IO;

public static class FileUtilities
{
    public static byte[] GetFileSHA256(string filename, IFileSystem fileSystem)
    {
        using (var md5 = SHA256.Create())
        {
            using (var stream = fileSystem.File.OpenRead(filename))
            {
                return md5.ComputeHash(stream);
            }
        }
    }
}

