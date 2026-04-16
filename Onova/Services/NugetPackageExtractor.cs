using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;

namespace Onova.Services;

/// <summary>
/// Extracts files from NuGet packages.
/// </summary>
public class NugetPackageExtractor(string rootDirPath) : IPackageExtractor
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

        // Get entries in the content directory
        var entries = archive
            .Entries.Where(e =>
                e.FullName.StartsWith(rootDirPath, StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        // For progress reporting
        var totalBytes = entries.Sum(e => e.Length);
        var totalBytesCopied = 0L;

        // Loop through entries
        foreach (var entry in entries)
        {
            // Get relative entry path
            var relativeEntryPath = entry.FullName[rootDirPath.Length..].TrimStart('/', '\\');

            // Get destination paths
            var entryDestFilePath = Path.Combine(destDirPath, relativeEntryPath);
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
