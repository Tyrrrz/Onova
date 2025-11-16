using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Onova.Updater.Utils.Extensions;

namespace Onova.Updater.Utils;

internal static class DirectoryEx
{
    // Performs a recursive copy of the source directory to the destination directory as an atomic operation
    public static void Copy(string sourceDirPath, string destDirPath, bool overwrite = true)
    {
        // Acquire locks in the destination directory for each file from the source directory
        var sourceStreams = new List<FileStream>();
        var destStreams = new List<FileStream>();

        try
        {
            foreach (
                var sourceFilePath in Directory.GetFiles(
                    sourceDirPath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                sourceStreams.Add(
                    File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                );

                var destFilePath = Path.Combine(
                    destDirPath,
                    Path.GetRelativePath(sourceDirPath, sourceFilePath)
                );

                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath) ?? destDirPath);

                destStreams.Add(
                    File.Open(
                        destFilePath,
                        overwrite ? FileMode.OpenOrCreate : FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.None
                    )
                );
            }

            // Copy data from the source files to the destination files
            foreach (var (sourceStream, destStream) in sourceStreams.Zip(destStreams))
            {
                sourceStream.CopyTo(destStream);

                // Truncate the destination file if the source file is shorter
                destStream.SetLength(sourceStream.Length);
            }
        }
        finally
        {
            sourceStreams.Concat(destStreams).Cast<IDisposable>().DisposeAll();
        }
    }
}
