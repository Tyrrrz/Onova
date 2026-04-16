using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;

namespace Onova.Services;

/// <summary>
/// Extracts files from zip-archived packages.
/// </summary>
public class ZipPackageExtractor : IPackageExtractor
{
    /// <inheritdoc />
    public async Task ExtractPackageAsync(
        string sourceFilePath,
        string destDirPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Read the zip
        using var archive = ZipFile.OpenRead(sourceFilePath);

        // For progress reporting
        var totalBytes = archive.Entries.Sum(e => e.Length);
        var totalBytesCopied = 0L;

        // Loop through all entries
        foreach (var entry in archive.Entries)
        {
            // Get destination paths
            var entryDestFilePath = Path.Combine(destDirPath, entry.FullName);
            var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

            // Create directory
            if (!string.IsNullOrWhiteSpace(entryDestDirPath))
                Directory.CreateDirectory(entryDestDirPath);

            // If the entry is a directory - continue
            if (Path.EndsInDirectorySeparator(entry.FullName))
                continue;

            // Extract entry
            using var input = entry.Open();
            using var output = File.Create(entryDestFilePath);

            var entryLocalProgress =
                progress is null || entry.Length <= 0
                    ? null
                    : new Progress<double>(p =>
                        progress.Report(
                            (totalBytesCopied + (long)(p * entry.Length)) / (double)totalBytes
                        )
                    );

            await input.CopyToAsync(output, entry.Length, entryLocalProgress, cancellationToken);
            totalBytesCopied += entry.Length;
        }
    }
}
