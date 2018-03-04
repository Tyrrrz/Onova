using System;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Services
{
    /// <summary>
    /// Provider for extracting packages.
    /// </summary>
    public interface IPackageExtractor
    {
        /// <summary>
        /// Extracts contents of the given package to the given output directory.
        /// </summary>
        Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}