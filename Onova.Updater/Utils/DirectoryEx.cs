using System.IO;

namespace Onova.Updater.Utils;

internal static class DirectoryEx
{
    public static void Copy(string sourceDirPath, string destDirPath, bool overwrite = true)
    {
        Directory.CreateDirectory(destDirPath);

        // Copy files
        foreach (var sourceFilePath in Directory.GetFiles(sourceDirPath))
        {
            var destFileName = Path.GetFileName(sourceFilePath);
            var destFilePath = Path.Combine(destDirPath, destFileName);
            File.Copy(sourceFilePath, destFilePath, overwrite);
        }

        // Copy subdirectories recursively
        foreach (var sourceSubDirPath in Directory.GetDirectories(sourceDirPath))
        {
            var destSubDirName = Path.GetFileName(sourceSubDirPath);
            var destSubDirPath = Path.Combine(destDirPath, destSubDirName);
            Copy(sourceSubDirPath, destSubDirPath, overwrite);
        }
    }
}