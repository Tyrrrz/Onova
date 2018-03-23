using System.IO;

namespace Onova.Tests.Internal
{
    internal static class DirectoryEx
    {
        public static void Reset(string dirPath)
        {
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
            Directory.CreateDirectory(dirPath);
        }
    }
}