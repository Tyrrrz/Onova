using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Extracts files from zip-archived packages.
    /// </summary>
    public class ZipPackageExtractor : IPackageExtractor
    {
        /// <inheritdoc />
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            sourceFilePath.GuardNotNull(nameof(sourceFilePath));
            destDirPath.GuardNotNull(nameof(destDirPath));

            // Read the zip
            using (var archive = ZipFile.OpenRead(sourceFilePath))
            {
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
                    if (!entryDestDirPath.IsNullOrWhiteSpace())
                        Directory.CreateDirectory(entryDestDirPath);

                    // If the entry is a directory - continue
                    if (entry.FullName.Last() == Path.DirectorySeparatorChar || entry.FullName.Last() == Path.AltDirectorySeparatorChar)
                        continue;

                    // Extract entry
                    using (var input = entry.Open())
                    using (var output = File.Create(entryDestFilePath))
                    {
                        var buffer = new byte[81920];
                        int bytesCopied;
                        do
                        {
                            // Copy
                            bytesCopied = await input.CopyChunkToAsync(output, buffer, cancellationToken);

                            // Report progress
                            totalBytesCopied += bytesCopied;
                            progress?.Report(1.0 * totalBytesCopied / totalBytes);
                        } while (bytesCopied > 0);
                    }
                }
            }
        }
    }
}