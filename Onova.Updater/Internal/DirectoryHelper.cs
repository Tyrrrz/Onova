using System.IO;

namespace Onova.Updater.Internal
{
    internal static class DirectoryHelper
    {
        public static void Copy(string sourceDirPath, string destDirPath, bool overwrite = true)
        {
            Directory.CreateDirectory(destDirPath);

            // Get all subdirectories in source directory
            var sourceSubDirPaths = Directory.EnumerateDirectories(sourceDirPath, "*", SearchOption.TopDirectoryOnly);

            // Recursively copy them
            foreach (var sourceSubDirPath in sourceSubDirPaths)
            {
                var destSubDirPath = Path.Combine(destDirPath, Path.GetDirectoryName(sourceSubDirPath));
                Copy(sourceSubDirPath, destSubDirPath, overwrite);
            }

            // Get all files in source directory
            var sourceFilePaths = Directory.EnumerateFiles(sourceDirPath, "*", SearchOption.TopDirectoryOnly);

            // Copy them
            foreach (var sourceFilePath in sourceFilePaths)
            {
                // Get destination file path
                var destFilePath = Path.Combine(destDirPath, Path.GetFileName(sourceFilePath));
                File.Copy(sourceFilePath, destFilePath, overwrite);
            }
        }
    }
}