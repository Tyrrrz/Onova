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

            var entryBaseOffset = totalBytesCopied;
            await input.CopyToAsync(
                output,
                entry.Length,
                progress?.Pipe(p => new Progress<double>(entryP =>
                    p.Report((entryBaseOffset + (long)(entryP * entry.Length)) / (double)totalBytes)
                )),
                cancellationToken
            );
            totalBytesCopied += entry.Length;
        }
    }
}
