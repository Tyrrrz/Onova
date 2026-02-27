using System;
using System.IO;

namespace Onova.Utils.Extensions;

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

        public static bool CheckWriteAccess(string dirPath)
        {
            var testFilePath = Path.Combine(dirPath, Guid.NewGuid().ToString());

            try
            {
                File.WriteAllText(testFilePath, "");
                File.Delete(testFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
