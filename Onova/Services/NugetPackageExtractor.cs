using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Utils;
using Onova.Utils.Extensions;

namespace Onova.Services;

/// <summary>
/// Extracts files from NuGet packages.
/// </summary>
public class NugetPackageExtractor : IPackageExtractor
{
    private readonly string _rootDirPath;

    /// <summary>
    /// Initializes an instance of <see cref="NugetPackageExtractor" />.
    /// </summary>
    public NugetPackageExtractor(string rootDirPath)
    {
        _rootDirPath = rootDirPath;
    }

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
        var entries = archive.Entries
            .Where(e => e.FullName.StartsWith(_rootDirPath, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // For progress reporting
        var totalBytes = entries.Sum(e => e.Length);
        var totalBytesCopied = 0L;

        // Loop through entries
        foreach (var entry in entries)
        {
            // Get relative entry path
            var relativeEntryPath = entry.FullName[_rootDirPath.Length..].TrimStart('/', '\\');

            // Get destination paths
            var entryDestFilePath = Path.Combine(destDirPath, relativeEntryPath);
            var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

            // Create directory
            if (!string.IsNullOrWhiteSpace(entryDestDirPath))
                Directory.CreateDirectory(entryDestDirPath);

            // If the entry is a directory - continue
            if (
                entry.FullName.Last() == Path.DirectorySeparatorChar
                || entry.FullName.Last() == Path.AltDirectorySeparatorChar
            )
                continue;

            // Extract entry
            using var input = entry.Open();
            using var output = File.Create(entryDestFilePath);

            using var buffer = PooledBuffer.ForStream();
            int bytesCopied;
            do
            {
                bytesCopied = await input.CopyBufferedToAsync(
                    output,
                    buffer.Array,
                    cancellationToken
                );
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / totalBytes);
            } while (bytesCopied > 0);
        }
    }
}
