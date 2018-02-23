using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Extracts packages as ZIP archives.
    /// </summary>
    public class ZipPackageExtractor : IPackageExtractor
    {
        /// <inheritdoc />
        public async Task ExtractPackageAsync(string packageFilePath, string outputDirPath)
        {
            packageFilePath.GuardNotNull(nameof(packageFilePath));
            outputDirPath.GuardNotNull(nameof(outputDirPath));

            // Read the zip
            using (var stream = File.OpenRead(packageFilePath))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                // Loop through all entries
                foreach (var entry in zip.Entries)
                {
                    var entryOutputFilePath = Path.Combine(outputDirPath, entry.FullName);
                    var entryOutputDirPath = Path.GetDirectoryName(entryOutputFilePath);

                    // Create directory
                    Directory.CreateDirectory(entryOutputDirPath);

                    // Extract entry
                    using (var entryStream = entry.Open())
                    using (var entryOutputFileStream = File.Create(entryOutputFilePath))
                        await entryStream.CopyToAsync(entryOutputFileStream).ConfigureAwait(false);
                }
            }
        }
    }
}