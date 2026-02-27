using System.IO;

namespace Onova.Tests.Utils.Extensions;

internal static class DirectoryExtensions
{
    extension(Directory)
    {
        public static void DeleteIfExists(string dirPath, bool recursive = true)
        {
            try
            {
                Directory.Delete(dirPath, recursive);
            }
            catch (DirectoryNotFoundException) { }
        }

        public static void Reset(string dirPath)
        {
            DeleteIfExists(dirPath);
            Directory.CreateDirectory(dirPath);
        }
    }
}
