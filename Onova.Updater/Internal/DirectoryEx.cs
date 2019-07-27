using System.IO;
using System.Threading;

namespace Onova.Updater.Internal
{
    internal static class DirectoryEx
    {
        public static void Copy(string sourceDirPath, string destDirPath, bool overwrite = true)
        {
            Directory.CreateDirectory(destDirPath);

            // Get all files in source directory
            var sourceFilePaths = Directory.EnumerateFiles(sourceDirPath);

            // Copy them
            foreach (var sourceFilePath in sourceFilePaths)
            {
                // Get destination file path
                var destFileName = Path.GetFileName(sourceFilePath);
                var destFilePath = Path.Combine(destDirPath, destFileName);

                Program.WriteLog($"Waiting for file to be writable... {destFilePath}");
                while (File.Exists(destFilePath) && !FileEx.CheckWriteAccess(destFilePath))
                    Thread.Sleep(100);

                File.Copy(sourceFilePath, destFilePath, overwrite);
            }

            // Get all subdirectories in source directory
            var sourceSubDirPaths = Directory.EnumerateDirectories(sourceDirPath);

            // Recursively copy them
            foreach (var sourceSubDirPath in sourceSubDirPaths)
            {
                var destSubDirName = Path.GetFileName(sourceSubDirPath);
                var destSubDirPath = Path.Combine(destDirPath, destSubDirName);
                Copy(sourceSubDirPath, destSubDirPath, overwrite);
            }
        }
    }
}