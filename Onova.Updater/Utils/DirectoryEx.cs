using System;
using System.ComponentModel;
using System.IO;
using Onova.Updater.Native;

namespace Onova.Updater.Utils;

internal static class DirectoryEx
{
    public static IDisposable Lock(
        string dirPath,
        FileAccess access = FileAccess.ReadWrite,
        FileShare share = FileShare.None)
    {
        var handle = NativeMethods.CreateFile(
            dirPath,
            access,
            share,
            IntPtr.Zero,
            FileMode.Open,
            (FileAttributes)0x02000000,
            IntPtr.Zero
        );

        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new Win32Exception();
        }

        return handle;
    }

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