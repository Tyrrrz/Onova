using System;
using System.IO;

namespace Onova.Internal
{
    internal static class DirectoryHelper
    {
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

        public static void ResetDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
            Directory.CreateDirectory(dirPath);
        }
    }
}