#region

using System.IO.Abstractions;
using System.Security.Cryptography;

#endregion

namespace Utopia.Core.IO;

public static class FileUtilities
{
    public static byte[] GetFileMd5(IFileSystem fileSystem,string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = fileSystem.File.OpenRead(filename))
            {
                return md5.ComputeHash(stream);
            }
        }
    }
}